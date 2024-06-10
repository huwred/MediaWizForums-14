﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using MediaWiz.Forums.Extensions;
using MediaWiz.Forums.Interfaces;
using MediaWiz.Forums.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;

namespace MediaWiz.Forums.Controllers
{
    /// <summary>
    /// Summary description for ForumsSurfaceController
    /// </summary>
    //[PluginController("MediaWiz.Core.Controllers")]
    public class ForumsSurfaceController : SurfaceController
    {
        private readonly IMemberService _memberService;
        private readonly IPublishedContentQuery _publishedContentQuery;
        private readonly IMemberSignInManager _signInManager;
        private readonly IMemberManager _memberManager;

        private readonly IContentService _contentService;
        private readonly IForumMailService _mailService;

        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDictionaryItemService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IdKeyMap _keyMap;
        public ForumsSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider,
            IMemberService memberService,
            IMemberSignInManager signInManager,
            IPublishedContentQuery publishedContentQuery,
            IMemberManager memberManager,
            IContentService contentService,
            IForumMailService mailService,IHttpContextAccessor httpContextAccessor,IDictionaryItemService localizationService,
            ILanguageService languageService,IdKeyMap keyMap) 
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberService = memberService;
            _memberManager = memberManager;
            _signInManager = signInManager;
            _publishedContentQuery = publishedContentQuery;
            _contentService = contentService;
            _mailService = mailService;

            _contextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _languageService = languageService;
            _keyMap = keyMap;
        }
        [HttpGet]
        public IActionResult EditPost(int id)
        {

            return ViewComponent("Posts", new { Template = "EditPostForm", Id = id });

        }

        [HttpPost]
        public async Task<IActionResult> PostReply([Bind(Prefix="Post")]ForumsPostModel model)
        {
            if (await CanPost(model) == false)
            {
                ModelState.AddModelError("Reply",_localizationService.GetOrCreateDictionaryValue("Forums.Error.PostPermission","You do not have permissions to post here") );
                return CurrentUmbracoPage();
            }            
            IEnumerable<ILanguage> languages = _languageService.GetAllAsync().Result;

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Reply",_localizationService.GetOrCreateDictionaryValue("Forums.Error.InvalidReply","Error posting (invalid model)") );
                return  CurrentUmbracoPage();
            }

            var postName =
                $"reply_{DateTime.UtcNow:yyyyMMddhhmmss}";

            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                postName = model.Title;
            }


            var parent = _contentService.GetById(model.ParentId);
            bool newPost = false;
            if (parent != null)
            {
                IContent post = null;
                if (model.Id > 0)
                    post = _contentService.GetById(model.Id);

                if (post == null)
                {
                    post = _contentService.Create(postName, parent, "forumPost");
                    if (post.AvailableCultures.Any())
                    {
                        foreach (var language in languages)
                        {
                            post.SetCultureName(postName,language.IsoCode);
                        }
                    }
                    newPost = true;
                }

                // unlikely but possible we still don't have a node.
                if (post != null )
                {
                    post.SetValue("postTitle", model.Title);
                    post.SetValue("postBody", model.Body);

                    var author = _memberService.GetById(model.AuthorId);
                    if (author != null)
                    {
                        post.SetValue("postCreator", author.Name);
                        post.SetValue("postAuthor", author.Id);
                    }

                    if (parent.ContentType.Alias != "Forum")
                    {
                        // posts that are in a forum, are allowed replies 
                        // thats how the threads work.
                        post.SetValue("allowReplies", true);

                    }
                    

                    post.SetValue("postType", model.IsTopic);
                    if (model.IsTopic)
                    {
                        post.SetValue("intPageSize",parent.GetValue<int>("intPageSize"));
                        if (parent.GetValue<bool>("requireApproval"))
                        {
                            post.SetValue("approved", false);
                        }
                        else
                        {
                            post.SetValue("approved", true);
                        }
                    }
                    else //post is a reply so need to get the Forum
                    {
                        var forum = _contentService.GetById(parent.ParentId);
                        if (forum.GetValue<bool>("requireApproval"))
                        {
                            post.SetValue("approved", false);
                        }
                        else
                        {
                            post.SetValue("approved", true);
                        }
                    }

                    if (!newPost)
                    {
                        post.SetValue("editDate",DateTime.UtcNow);
                    }


                    var saveresult = _contentService.Save(post);
                    var result = _contentService.Publish(post, new string[] { "*" });

                    return RedirectToCurrentUmbracoPage();
                }
            }
            ModelState.AddModelError("Post",_localizationService.GetOrCreateDictionaryValue("Forums.Error.PostError","Error creating the post") );
            return RedirectToCurrentUmbracoPage();
        }
        [HttpPost]
        public async Task<IActionResult> EditPost([Bind(Prefix="Post")]ForumsPostModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("PostEdit", _localizationService.GetOrCreateDictionaryValue("Forums.Error.EditInvalid","Error editing post (invalid model)"));
                return  CurrentUmbracoPage();
            }

            if (await CanPost(model) == false)
            {
                ModelState.AddModelError("PostEdit",_localizationService.GetOrCreateDictionaryValue("Forums.Error.EditPermission","You do not have permissions to edit posts") );
                return CurrentUmbracoPage();
            }
            var parent = _contentService.GetById(model.ParentId);

            if (parent != null)
            {
                IContent post = null;
                if (model.Id > 0)
                    post = _contentService.GetById(model.Id);
                // unlikely but possible we still don't have a node.
                if (post != null )
                {
                    //let's sanitize any text to prevent script injections

                    if (!string.IsNullOrWhiteSpace(model.Title))
                    {
                        post.SetValue("postTitle", model.Title);
                    }
                    
                    post.SetValue("postBody", model.Body);
                    post.SetValue("editDate",DateTime.UtcNow);
                    var saveresult = _contentService.Save(post);
                    var result = _contentService.Publish(post, new string[] { "*" });

                    return Redirect(model.returnPath);
                }
            }
            ModelState.AddModelError("PostEdit",_localizationService.GetOrCreateDictionaryValue("Forums.Error.EditError","Error editing the post") );
            return RedirectToCurrentUmbracoPage();
        }
        [HttpPost]
        //[Authorize(Roles = "ForumAdministrator")]
        public IActionResult CreateForum(/*[Bind(Prefix="Forum")]*/ForumsForumModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Forum",_localizationService.GetOrCreateDictionaryValue("Forums.Error.CreateForum","Error creating Forum (invalid model)"));
                return  CurrentUmbracoPage();
            }

            var parent = _contentService.GetById(model.ParentId);
            var forum = _contentService.CreateContent(model.Title, parent.GetUdi(), "forum");
            forum.SetValue("hideChildrenFromNav",true);
            forum.SetValue("umbracoNaviHide",false);
            forum.SetValue("forumTitle",model.Title);
            forum.SetValue("forumDescription",model.Introduction);
            forum.SetValue("postAtRoot", model.AllowPosts);
            forum.SetValue("isActive",true);
            forum.SetValue("allowImages",model.AllowImages);
            forum.SetValue("requireApproval",model.RequireApproval);
            var saveresult = _contentService.Save(forum);
            var result = _contentService.Publish(forum, new string[] { "*" });
            TempData["ForumSaveResult"] = result;
            return CurrentUmbracoPage();
        }
 
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            TempData.Clear();
            await _signInManager.SignOutAsync();
            var forumRoot = _publishedContentQuery.ContentAtRoot().DescendantsOrSelfOfType("forum").FirstOrDefault(f=>f.Parent == null || f.Parent.ContentType.Alias != "forum");

            return Redirect(forumRoot?.Url());
        }
        [HttpGet]
        public IActionResult Reset(string resetGuid)
        {

            TempData["ResetForm"] = true;
            return View("Reset");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForumForgotPasswordModel model)
        {
            TempData["ResetSent"] = false;
            if (!ModelState.IsValid)
            {
                return ViewComponent("PasswordManager", new { Model = model , Template = "ForgotPassword"});
            }

            var member = _memberService.GetByEmail(model.EmailAddress);
            if (member != null)
            {
                var memberIdentity = await _memberManager.FindByIdAsync(member.Id.ToString());
                // we found a user with that email ....
                var token = await _memberManager.GeneratePasswordResetTokenAsync(memberIdentity);
                var encodedToken = !string.IsNullOrEmpty(token) ? HttpUtility.UrlEncode(token) : string.Empty;
                member.SetValue("resetGuid", token);
                _memberService.Save(member);

                // send email, do not wait as we want it to run in background....
                await _mailService.SendResetPassword(member.Email,token);

                TempData["ResetSent"] = true;
            }
            else
            {
                ModelState.AddModelError("ForgotPasswordForm", 
                    _localizationService.GetOrCreateDictionaryValue("Forums.Error.NoUser","No user found"));
                TempData["ValidationError"] =
                    _localizationService.GetOrCreateDictionaryValue("Forums.Error.NoUser", "No user found");
                //return ViewComponent("PasswordManager", new { Model = model , Template = "ForgotPassword"});
            }

            return CurrentUmbracoPage();
        }
        // double check the current user can post to this forum...
        private async Task<bool> CanPost(ForumsPostModel model)
        {
            if (!_memberManager.IsLoggedIn())
                return false;
            
            if ( model.ParentId > 0 ) 
            {
                var parent = _publishedContentQuery.Content(model.ParentId);
                if ( parent.ContentType.Alias != "forum" )
                {
                    parent = parent.Parent;
                }
                if ( parent != null )
                {
                    var canPostGroups = parent.Value<string>("canPostGroup");

                    // default is any one logged on...
                    if (string.IsNullOrWhiteSpace(canPostGroups))
                        return true;

                    // is the user in any of those groups ?
                    var allowedGroupList = new List<string>();
                    foreach (string memberGroupStr in canPostGroups.Split(','))
                    {
                var key = _keyMap.GetKeyForId(Convert.ToInt32(memberGroupStr),UmbracoObjectTypes.MemberGroup).Result;

                        var memberGroup = Services.MemberGroupService.GetAsync(key).Result;
                        if (memberGroup != null)
                        {
                            allowedGroupList.Add(memberGroup.Name);
                        }
                    }
                    return await _memberManager.IsMemberAuthorizedAsync(allowGroups: allowedGroupList);
                }
            }

            return false;
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public IActionResult Sort([FromBody] Object sort)
        {
            var data = JsonConvert.DeserializeObject<dynamic>(sort.ToString());
            var returnpath = _contextAccessor.HttpContext.Request.Headers["Referer"].ToString();

            returnpath = Regex.Replace(returnpath, @"\?sortdir=[A-Z]{3,4}", "");
            if (returnpath.Contains("?"))
            {
                returnpath += "&sortdir=" + data.sort.Value;
            }
            else
            {
                returnpath += "?sortdir=" + data.sort.Value;
            }

            return Json(new { success = true, message = returnpath });
        }
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public IActionResult Test([FromBody] string sort)
        {
            var data = JsonConvert.DeserializeObject<dynamic>(sort.ToString());
            var returnpath = _contextAccessor.HttpContext.Request.Headers["Referer"].ToString();

            //returnpath = Regex.Replace(returnpath, @"\?sortdir=[A-Z]{3,4}", "");
            //if (returnpath.Contains("?"))
            //{
            //    returnpath += "&sortdir=" + data.sort.Value;
            //}
            //else
            //{
            //    returnpath += "?sortdir=" + data.sort.Value;
            //}

            return Json(new { success = true, message = returnpath });
        }
        #region Captcha Image
 
        /// <summary>
        /// Refreshes the Captch with a new sum
        /// </summary>
        /// <returns>new Captcha ViewComponent instance</returns>
        public IActionResult RefreshCaptcha()
        {
            return ViewComponent("Captcha");
        }
        #endregion
    }
}