using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Web.Website.Models;
using Umbraco.Extensions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static Umbraco.Cms.Core.Constants.Conventions;

namespace MediaWiz.Forums.Controllers;

[UmbracoMemberAuthorize]
public class MediaWizProfileController : SurfaceController
{
    private readonly IMemberManager _memberManager;
    private readonly IMemberService _memberService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IEmailSender _emailSender;
    private readonly ILogger _logger;

    private readonly string _fromEmail;

    public MediaWizProfileController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IMemberManager memberManager,
        IMemberService memberService,
        IMemberTypeService memberTypeService,
        ICoreScopeProvider scopeProvider,
        IHostingEnvironment hostingEnvironment,
        IEmailSender emailSender,
        ILogger<MediaWizProfileController> logger,
        IOptions<GlobalSettings> globalSettings,
        IOptions<ContentSettings> contentSettings)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _memberManager = memberManager;
        _memberService = memberService;
        _memberTypeService = memberTypeService;
        _scopeProvider = scopeProvider;
        _hostingEnvironment = hostingEnvironment;
        _emailSender = emailSender;
        _logger = logger;
        _fromEmail = globalSettings.Value.Smtp?.From != null ? globalSettings.Value.Smtp.From : contentSettings.Value.Notifications.Email;


    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    //[ValidateUmbracoFormRouteString]
    public async Task<IActionResult> HandleUpdateProfile([Bind(Prefix = "profileModel")] ProfileModel model)
    {
        if (ModelState.IsValid == false)
        {
            return CurrentUmbracoPage();
        }

        MergeRouteValuesToModel(model);

        MemberIdentityUser currentMember = await _memberManager.GetUserAsync(HttpContext.User);
        if (currentMember == null!)
        {
            // this shouldn't happen, we also don't want to return an error so just redirect to where we came from
            return RedirectToCurrentUmbracoPage();
        }

        IdentityResult result = await UpdateMemberAsync(model, currentMember);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return CurrentUmbracoPage();
        }


        TempData["FormSuccess"] = true;

        // If there is a specified path to redirect to then use it.
        if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
        {
            return Redirect(model.RedirectUrl!);
        }
        if (currentMember.Email != model.Email)
        {
            // Check if the new email is already in use
            var existingUser = await _memberManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("profileModel", "email is already in use");
                return CurrentUmbracoPage();
            }
            // Generate email change token
            var token = await _memberManager.GenerateEmailConfirmationTokenAsync(currentMember);

            // Create confirmation link
            var encodedToken = System.Web.HttpUtility.UrlEncode(token);
            var callbackUrl = Url.Action(
                "ConfirmEmailChange",
                "ForumsApi",
                new { userId = currentMember.Id, email = model.Email, token = encodedToken },
                protocol: Request.Scheme);

            var messageBody = $"Please confirm your email change by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";

            EmailMessage message = new EmailMessage(_fromEmail, model.Email,
                "Confirm your email change", messageBody, true);

            await _emailSender.SendAsync(message, "Contact");

            ModelState.AddModelError("", "Confirmation link sent to the new email.");
            return CurrentUmbracoPage();

        }

        ModelState.AddModelError("", "Profile Updated successfully");
        return CurrentUmbracoPage();
    }

    /// <summary>
    ///     We pass in values via encrypted route values so they cannot be tampered with and merge them into the model for use
    /// </summary>
    /// <param name="model"></param>
    private void MergeRouteValuesToModel(ProfileModel model)
    {
        if (RouteData.Values.TryGetValue(nameof(ProfileModel.RedirectUrl), out var redirectUrl) && redirectUrl != null)
        {
            model.RedirectUrl = redirectUrl.ToString();
        }
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError("profileModel", error.Description);
        }
    }

    private async Task<IdentityResult> UpdateMemberAsync(ProfileModel model, MemberIdentityUser currentMember)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        //currentMember.Email = model.Email; //requires confirmation!
        currentMember.Name = model.Name;
        //currentMember.UserName = model.UserName; //not allowed to change
        currentMember.Comments = model.Comments;

        IdentityResult saveResult = await _memberManager.UpdateAsync(currentMember);
        if (!saveResult.Succeeded)
        {
            scope.Complete();
            return saveResult;
        }

        // now we can update the custom properties
        // TODO: Ideally we could do this all through our MemberIdentityUser
        IMember member = _memberService.GetById(currentMember.Key);
        if (member == null)
        {
            // should never happen
            throw new InvalidOperationException($"Could not find a member with key: {member?.Key}.");
        }

        IMemberType memberType = _memberTypeService.Get(member.ContentTypeId);

        foreach (MemberPropertyModel property in model.MemberProperties
                     .Where(p => memberType?.PropertyTypeExists(p.Alias) ?? false)
                     .Where(property => member.Properties.Contains(property.Alias))
                     .Where(p => memberType?.MemberCanEditProperty(p.Alias) ?? false))
        {
            member.Properties[property.Alias]?.SetValue(property.Value);
        }

        _memberService.Save(member);

        scope.Complete();

        return saveResult;
    }

}
