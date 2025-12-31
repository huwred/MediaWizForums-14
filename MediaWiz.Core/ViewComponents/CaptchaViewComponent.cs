using MediaWiz.Forums.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MediaWiz.Forums.ViewComponents
{
    public class CaptchaViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CaptchaViewComponent> _logger;

        public CaptchaViewComponent(IHttpContextAccessor httpContextAccessor, ILogger<CaptchaViewComponent> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                Captcha captcha = new Captcha(200, 80, 6);
                TempData["b64"] = captcha.GenerateAsB64(Captcha.CaptchaType.Circle);
                _httpContextAccessor.HttpContext.Session.SetString("Captcha", captcha.GetAnswer());
                return await Task.FromResult((IViewComponentResult)View("Captcha"));

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error generating captcha");
                throw;
            }
        }

    }
}