using System.Reflection;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using MediaWiz.Forums.Helpers;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Packaging;
using Microsoft.Extensions.Logging;
using Umbraco.Extensions;
using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;

namespace MediaWiz.Forums.Migrations
{
    public class ImportPackageXmlMigration : PackageMigrationBase
    {
        private readonly IFileService _fileService;
        private readonly IPackagingService _packagingService;
        private readonly IOptions<ForumConfigOptions> _forumOptions;
        private string ForumDoctypes => _forumOptions.Value.ForumDoctypes;
        private readonly ILogger<PublishApprovalChangesMigration> _logger;
                private readonly IDictionaryItemService _dictionaryService;
        private readonly ILanguageService _languageService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        public ImportPackageXmlMigration(IPackagingService packagingService, IMediaService mediaService, MediaFileManager mediaFileManager, MediaUrlGeneratorCollection mediaUrlGenerators, IShortStringHelper shortStringHelper, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, IMigrationContext context, IOptions<PackageMigrationSettings> packageMigrationsSettings
            ,IFileService fileService,IOptions<ForumConfigOptions> forumOptions,ILogger<PublishApprovalChangesMigration> logger,
            IDictionaryItemService dictionaryService,ILanguageService languageService,IBackOfficeSecurityAccessor backOfficeSecurityAccessor) : base(packagingService, mediaService, mediaFileManager, mediaUrlGenerators, shortStringHelper, contentTypeBaseServiceProvider, context, packageMigrationsSettings)
        {
            _fileService = fileService;
            _packagingService = packagingService;
            _forumOptions = forumOptions;
            _logger = logger;
            _dictionaryService = dictionaryService;
            _languageService = languageService;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;            
        }

        protected override void Migrate()
        {
            _logger.LogInformation("ImportPackageXmlMigration");
            //set the default values for the xml files to import
            var xmlpackage = "package.xml";
            var templatepackage = "packagetemplates.xml";
            if (ForumDoctypes != null) //If the override value is set load the alternate xml files
            {
                xmlpackage = "forumpackage.xml";
                templatepackage = "forumtemplates.xml";
            }
            var asm = Assembly.GetExecutingAssembly();
            //Import the templates
            using(var stream = asm.GetManifestResourceStream("MediaWiz.Forums.Migrations." + templatepackage))
            {
                var templateXml = XDocument.Load(stream);
                _packagingService.InstallCompiledPackageData(templateXml);
            }
            //Import doctypes and content nodes
            using(var stream = asm.GetManifestResourceStream("MediaWiz.Forums.Migrations." + xmlpackage))
            {
                var packageXml = XDocument.Load(stream);
                _packagingService.InstallCompiledPackageData(packageXml);
            }

            AddDictionaryItems();
        }
        private async void AddDictionaryItems()
        {
            try
            {
                var defLang = await _languageService.GetDefaultLanguageAsync();
                ILanguage lang = await _languageService.GetAsync(defLang.IsoCode);
                if(!_dictionaryService.ExistsAsync("MediaWizForums").Result)
                {
                    var parentnode = new DictionaryItem("MediaWizForums");
                    var user = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

                    var newitem = _dictionaryService.GetAsync("Forums.ForgotPasswordView").Result ?? new DictionaryItem(parentnode.Key,"Forums.ForgotPasswordView");
                    newitem.AddOrUpdateDictionaryValue(lang,"/reset");
                    await _dictionaryService.CreateAsync(newitem,user.Key);

                    newitem = _dictionaryService.GetAsync("Forums.ForumUrl").Result ?? new DictionaryItem(parentnode.Key,"Forums.ForumUrl");
                    newitem.AddOrUpdateDictionaryValue(lang,"/");
                    await _dictionaryService.CreateAsync(newitem,user.Key);

                    newitem = _dictionaryService.GetAsync("Forums.LoginUrl").Result ?? new DictionaryItem(parentnode.Key,"Forums.LoginUrl");
                    newitem.AddOrUpdateDictionaryValue(lang,"/login");
                    await _dictionaryService.CreateAsync(newitem,user.Key);

                    newitem = _dictionaryService.GetAsync("Forums.CaptchaErrMsg").Result ?? new DictionaryItem(parentnode.Key,"Forums.CaptchaErrMsg");
                    newitem.AddOrUpdateDictionaryValue(lang,"Incorrect answer");
                    await _dictionaryService.CreateAsync(newitem,user.Key);

                    newitem = _dictionaryService.GetAsync("Forums.RegisterUrl").Result ?? new DictionaryItem(parentnode.Key,"Forums.RegisterUrl");
                    newitem.AddOrUpdateDictionaryValue(lang,"/register");
                    await _dictionaryService.CreateAsync(newitem,user.Key);

                    newitem = _dictionaryService.GetAsync("Forums.VerifyUrl").Result ?? new DictionaryItem(parentnode.Key,"Forums.VerifyUrl");
                    newitem.AddOrUpdateDictionaryValue(lang,"/verify");
                    await _dictionaryService.CreateAsync(newitem,user.Key);

                }


            }
            catch (Exception e)
            {
                _logger.LogError( e, "Executing AddDictionaryItems");

            }

        }
    }
}
