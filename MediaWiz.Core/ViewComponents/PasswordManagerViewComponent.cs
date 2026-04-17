using System.Threading.Tasks;
using MediaWiz.Forums.Extensions;
using MediaWiz.Forums.Models;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;

namespace MediaWiz.Forums.ViewComponents
{
    public class PasswordManagerViewComponent : ViewComponent
    {
        private readonly IDictionaryItemService _dictionaryService;
    
        public PasswordManagerViewComponent(IDictionaryItemService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }
    
        public async Task<IViewComponentResult> InvokeAsync(string Template, ForumForgotPasswordModel Model, bool captcha = true)
        {
            TempData["HideForm"] = captcha;
        
            switch (Template)
            {
                case "ForgotPassword":
                    var viewModel = new ForgotPasswordViewModel
                    {
                        Model = Model ?? new ForumForgotPasswordModel(),
                        RequestSentTitle = await _dictionaryService.GetOrCreateDictionaryValue(
                            "Forums.Members.Forgotpassword.RequestSent", "Reset Request Sent"),
                        RequestSentMessage = await _dictionaryService.GetOrCreateDictionaryValue(
                            "Forums.Members.Forgotpassword.RequestSent.Message", 
                            "We have sent a password reset message to the email on your account, please check your email and follow the link to reset your password."),
                        SendButtonText = await _dictionaryService.GetOrCreateDictionaryValue(
                            "Forums.Members.Forgotpassword.Send", "Send reset request")
                    };
                    return await Task.FromResult((IViewComponentResult)View(Template, viewModel));
                
                case "ChangePassword" :
                    return await Task.FromResult((IViewComponentResult)View(Template,new Umbraco.Cms.Core.Models.ChangingPasswordModel
                    {
                        NewPassword = null
                    }));

                case "ResetPassword" :
                    return await Task.FromResult((IViewComponentResult)View(Template));

            }
            return await Task.FromResult((IViewComponentResult)View(Template));
        }
    }


}
