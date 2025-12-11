using MediaWiz.Forums.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Packaging;
using Umbraco.Extensions;

namespace MediaWiz.Forums.Migrations
{
    public class ImportPackageXmlMigration : AsyncPackageMigrationBase
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

        protected override Task MigrateAsync()
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
            return Task.CompletedTask;
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
                    
                    await _dictionaryService.CreateAsync(parentnode,Constants.Security.SuperUserKey);

                    var newitem = _dictionaryService.GetAsync("Forums.ForgotPasswordView").Result ?? new DictionaryItem(parentnode.Key,"Forums.ForgotPasswordView");
                    newitem.AddOrUpdateDictionaryValue(lang,"/reset");
                    await _dictionaryService.CreateAsync(newitem,Constants.Security.SuperUserKey);

                    newitem = _dictionaryService.GetAsync("Forums.ForumUrl").Result ?? new DictionaryItem(parentnode.Key,"Forums.ForumUrl");
                    newitem.AddOrUpdateDictionaryValue(lang,"/");
                    await _dictionaryService.CreateAsync(newitem,Constants.Security.SuperUserKey);

                    newitem = _dictionaryService.GetAsync("Forums.LoginUrl").Result ?? new DictionaryItem(parentnode.Key,"Forums.LoginUrl");
                    newitem.AddOrUpdateDictionaryValue(lang,"/login");
                    await _dictionaryService.CreateAsync(newitem,Constants.Security.SuperUserKey);

                    newitem = _dictionaryService.GetAsync("Forums.CaptchaErrMsg").Result ?? new DictionaryItem(parentnode.Key,"Forums.CaptchaErrMsg");
                    newitem.AddOrUpdateDictionaryValue(lang,"Incorrect answer");
                    await _dictionaryService.CreateAsync(newitem,Constants.Security.SuperUserKey);

                    newitem = _dictionaryService.GetAsync("Forums.RegisterUrl").Result ?? new DictionaryItem(parentnode.Key,"Forums.RegisterUrl");
                    newitem.AddOrUpdateDictionaryValue(lang,"/register");
                    await _dictionaryService.CreateAsync(newitem,Constants.Security.SuperUserKey);

                    newitem = _dictionaryService.GetAsync("Forums.VerifyUrl").Result ?? new DictionaryItem(parentnode.Key,"Forums.VerifyUrl");
                    newitem.AddOrUpdateDictionaryValue(lang,"/verify");
                    await _dictionaryService.CreateAsync(newitem,Constants.Security.SuperUserKey);

                }


            }
            catch (Exception e)
            {
                _logger.LogError( e, "Executing AddDictionaryItems");

            }

        }
    }
}
