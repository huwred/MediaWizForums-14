﻿using System;
using System.Linq;
using MediaWiz.Forums.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace MediaWiz.Forums.Controllers
{
    public class VerifyController : RenderController
    {
        private readonly IMemberService _memberService;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ServiceContext _serviceContext;
        private readonly IDictionaryItemService _dictionaryService;

        public VerifyController(ILogger<VerifyController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor,IMemberService memberService, IVariationContextAccessor variationContextAccessor,ServiceContext context,IDictionaryItemService dictionaryService) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _memberService = memberService;
            _variationContextAccessor = variationContextAccessor;
            _serviceContext = context;
            _dictionaryService = dictionaryService;
        }
        public override IActionResult Index()
        {
            VerifyViewModel pageViewModel = new VerifyViewModel(CurrentPage,
                new PublishedValueFallback(_serviceContext, _variationContextAccessor))
            {
                ValidatedMember = null
            };
            return CurrentTemplate(pageViewModel);
        }
        [HttpGet]
        public IActionResult Index([FromQuery(Name = "verifyGuid")] string guid)
        {
            if (guid != null)
            {
                var member = _memberService.GetMembersByPropertyValue("resetGuid", guid, StringPropertyMatchType.Exact);
                VerifyViewModel pageViewModel = new VerifyViewModel(CurrentPage,
                    new PublishedValueFallback(_serviceContext, _variationContextAccessor));
                if (member.Count() == 1)
                {
                    var memberToValidate = member.First();
                    pageViewModel.ValidatedMember = memberToValidate;
                    memberToValidate.SetValue("resetGuid",null);
                    memberToValidate.SetValue("joinedDate",DateTime.UtcNow);
                    memberToValidate.SetValue("hasVerifiedAccount",true);
                    memberToValidate.IsApproved = true;
                    try
                    {
                        var members = _memberService.GetMembersInRole("ForumMember");
                        if (!members.Contains(memberToValidate))
                        {
                            _memberService.AssignRole(memberToValidate.Username, "ForumMember");
                        }
                        var result = _memberService.Save(memberToValidate);
                        if(result.Success)
                        {
                            pageViewModel.Success = true;
                            return CurrentTemplate(pageViewModel);
                        }

                    }
                    catch (Exception e)
                    {
                        var test = e.Message;
                        //throw;
                    }
                    pageViewModel.Success = true;

                }
                else
                {
                    pageViewModel.Success = false;
                    pageViewModel.Error = _dictionaryService.GetOrCreateDictionaryValue("Forums.Error.Verification","Verification code was not found or has expired");
                }
                return CurrentTemplate(pageViewModel);
            }

            VerifyViewModel viewModel = new VerifyViewModel(CurrentPage,
                new PublishedValueFallback(_serviceContext, _variationContextAccessor))
            {
                ValidatedMember = null
            };
            return CurrentTemplate(viewModel);
        }

    }
    public class ForumVerifyController : RenderController
    {
        private readonly IMemberService _memberService;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ServiceContext _serviceContext;
        private readonly IDictionaryItemService _dictionaryService;

        public ForumVerifyController(ILogger<ForumVerifyController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor,IMemberService memberService, IVariationContextAccessor variationContextAccessor,ServiceContext context,IDictionaryItemService dictionaryService) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _memberService = memberService;
            _variationContextAccessor = variationContextAccessor;
            _serviceContext = context;
            _dictionaryService = dictionaryService;
        }
        public override IActionResult Index()
        {
            VerifyViewModel pageViewModel = new VerifyViewModel(CurrentPage,
                new PublishedValueFallback(_serviceContext, _variationContextAccessor))
            {
                ValidatedMember = null
            };
            return CurrentTemplate(pageViewModel);
        }
        [HttpGet]
        public IActionResult Index([FromQuery(Name = "verifyGUID")] string guid)
        {
            if (guid != null)
            {
                var member = _memberService.GetMembersByPropertyValue("resetGuid", guid, StringPropertyMatchType.Exact);
                var enumerable = member as IMember[] ?? member.ToArray();
                VerifyViewModel pageViewModel = new VerifyViewModel(CurrentPage,
                    new PublishedValueFallback(_serviceContext, _variationContextAccessor));
                if (enumerable.Count()==1)
                {
                    var memberToValidate = enumerable.First();
                    memberToValidate.SetValue("resetGuid",null);
                    memberToValidate.SetValue("joinedDate",DateTime.UtcNow);
                    memberToValidate.SetValue("hasVerifiedAccount",true);
                    memberToValidate.IsApproved = true;
                    _memberService.Save(memberToValidate);
                    _memberService.AssignRole(memberToValidate.Email, "ForumMember");
                    TempData["ValidationSuccess"] = true;

                    pageViewModel.ValidatedMember = memberToValidate;
                }
                else
                {
                    TempData["ValidationSuccess"] = null;
                    TempData["ValidationError"] = _dictionaryService.GetOrCreateDictionaryValue("Forums.Error.Verification","Verification code was not found or has expired");
                }
                return CurrentTemplate(pageViewModel);
            }

            VerifyViewModel viewModel = new VerifyViewModel(CurrentPage,
                new PublishedValueFallback(_serviceContext, _variationContextAccessor))
            {
                ValidatedMember = null
            };
            return CurrentTemplate(viewModel);
        }

    }

    public class VerifyViewModel : PublishedContentWrapped
    {
        public IMember ValidatedMember;
        public string NewPassword;
        public string ConfirmPassword;
        public string ResetToken;
        public string MemberId;
        public bool Success;
        public string Error;
        public VerifyViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback)
        {
        }
    }

}