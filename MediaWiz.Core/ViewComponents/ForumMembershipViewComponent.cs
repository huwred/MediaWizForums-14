﻿using MediaWiz.Forums.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MediaWiz.Forums.ViewModels;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Models;
using Umbraco.Cms.Web.Website.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common;

namespace MediaWiz.Forums.ViewComponents
{
    public class ForumMembershipViewComponent : ViewComponent
    {
        private readonly UmbracoHelper _umbracoHelper;
        private readonly MemberModelBuilderFactory _memberModelBuilderFactory;
        private readonly IMemberService _memberService;
        private readonly IMemberManager _memberManager;

        public ForumMembershipViewComponent(UmbracoHelper umbracoHelper,MemberModelBuilderFactory memberModelBuilderFactory,IMemberService memberService, IMemberManager memberManager)
        {
            _umbracoHelper = umbracoHelper;
            _memberModelBuilderFactory = memberModelBuilderFactory;

            _memberManager = memberManager;
            _memberService = memberService;

        }

        public async Task<IViewComponentResult> InvokeAsync(string qryUser, string Template, int pageSize)
        {

            switch (Template)
            {

                case "Register" :
                    return await Task.FromResult((IViewComponentResult)View(Template));

                case "Login" :
                    var loginModel = new LoginModel();

                    string forumUrl = _umbracoHelper.GetDictionaryValueOrDefault("Forums.ForumUrl","/");
                    loginModel.RedirectUrl = forumUrl;
                    return await Task.FromResult((IViewComponentResult)View(Template,loginModel));

                case "LoginStatus" :
                    return await Task.FromResult((IViewComponentResult)View(Template));
                case "EditProfile" :
                    var model = new ProfileViewModel
                    {
                        // Build a profile model to edit
                        ProfileModel = await _memberModelBuilderFactory
                            .CreateProfileModel()
                            // If null or not set, this will redirect to the current page
                            .WithRedirectUrl(null)
                            // Include editable custom properties on the form
                            .WithCustomProperties(true)
                            .BuildForCurrentMemberAsync(),
                        Username = qryUser,
                        CurrentUser = _memberManager.GetCurrentMemberAsync().Result
                    };

                    if (model.CurrentUser != null)
                    {
                        model.MemberIdentity = _memberManager.FindByNameAsync(model.CurrentUser.UserName).Result;
                        model.ViewMember = _memberService.GetByKey(model.MemberIdentity.Key);
                    }

                    if (model.Username == model.CurrentUser?.UserName)
                    {
                        return await Task.FromResult((IViewComponentResult)View(Template,model));
                    }
                    return await Task.FromResult((IViewComponentResult)View("ViewProfile",model));
                case "ListMembers" :
                    TempData["PageSize"] = pageSize;
                    return await Task.FromResult((IViewComponentResult)View("Members"));

            }
            return await Task.FromResult((IViewComponentResult)View(Template));
        }
    }
}
