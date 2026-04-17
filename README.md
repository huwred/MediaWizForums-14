## For versions of Umbraco prior to 14 please use https://github.com/huwred/MediaWizForums ##

## MediaWiz Forums
[![Platform](https://img.shields.io/badge/Umbraco-17-%233544B1?style=flat&logo=umbraco)](https://umbraco.com/products/umbraco-cms/)
[![NuGet](https://img.shields.io/nuget/v/MediaWiz.Forums.14.svg)](https://www.nuget.org/packages/MediaWiz.Forums.14/)
[![GitHub](https://img.shields.io/github/license/huwred/MediaWizForums-14?label=license&style=flat)](https://github.com/huwred/MediaWizForums-14/blob/Umbraco17/LICENSE)

**MediaWiz Forums** provides a basic Forum add on for Umbraco.

### Installation
> [!IMPORTANT]
> **v17.x** supports Umbraco v17
>
> **v16.x** supports Umbraco v16
> 
> **v14.x** supports Umbraco v14+
> 
> **For previous versions** Please use https://github.com/huwred/MediaWizForums
> 
> To understand more about which Umbraco CMS versions are actively supported by Umbraco HQ, please see [Umbraco's Long-term Support (LTS) and End-of-Life (EOL) policy](https://umbraco.com/products/knowledge-center/long-term-support-and-end-of-life/).

The Umbraco 17 version of this package is [available via NuGet](https://www.nuget.org/packages/MediaWiz.Forums.14).

To install the package, you can use either .NET CLI:

```powershell
dotnet add package MediaWiz.Forums.14 --version 17.0.0
```

or the NuGet Package Manager:

```bash
Install-Package MediaWiz.Forums.14 -Version 17.0.0
```

### Setup
The package can then be configured in `appsettings.json`:

```json
  "MediaWizOptions": {
    "MaxFileSize": 8, 
    "AllowedFiles": [ ".gif", ".jpg", ".png", ".svg", ".webp", ".mp4" ],
  },
```
### Options
The following options are available, in `appsettings.json`:

```json
  "MediaWizOptions": {
    "MemberTypeAlias": "",
    "ForumDoctypes": "", 
    "MaxFileSize": 8,
    "AllowedFiles": [], 
    "UniqueFilenames": false
  },
```

#### Settings
##### MediaWiz Forums
| Property           | Type                                      | Description                                    |
|--------------------|-------------------------------------------|------------------------------------------------|
| MediaWizOptions    | [`MediaWizOptions`](#mediawizoptions)     | Configure settings for the MediaWiz Forums     |

##### Options
| Property           | Type                       | Description                                                                                                                            |
|--------------------|----------------------------|----------------------------------------------------------------------------------------------------------------------------------------|
| MemberTypeAlias    | `string`                   | Defaults to "forumMember" if not set                                                                                                   |
| ForumDoctypes      | `string`                   | If your site already contains any [`Document Types`](#doctypes) using the same aliases as the forum, set a prefix to avoid conflicts.  |
| MaxFileSize        | `integer`                  | Maximum file size in MB.                                                                                                               |
| AllowedFiles       | `string[]` \| `List<string>` | A list of file extensions that can be uploaded in a post. If left blank, no files will be allowed.                                   |
| UniqueFilenames    | `boolean`                  | When uploading, if true a random guid is used for all filenames, default uses the name of uploaded file.                               |

#### Document Types
| Property           | Type              | Description                                   |
|--------------------|-------------------|-----------------------------------------------|
| searchPage         | `searchPage`      | Search page node                              |
| members            | `members`         | Members root node  (dummy holding node)       |
| profile            | `profile`         | Member profile node                           |
| login              | `login`           | Login node                                    |
| register           | `register`        | Registration node                             |
| reset              | `signatures`      | Password reset node                           |
| verify             | `verify`          | Email verification node                       |



### Getting Started

After installation, you'll need to:

1. **Register TagHelpers**: In your Umbraco project, open the _ViewImports file and add the following ... `@addTagHelper *, MediaWiz.Forums`
2. **Login to Umbraco**: Navigate to `/umbraco` and login with the credentials you specified (default: admin@example.com / 1234567890)
2. **Publish the Forums Page**: Go to the Content section and publish the forums holder page including it's children
3. **Set Master template**: Got to the Settings section and navigate to Templates, select the `forumMaster` template and assign your main site Layout file.
3. **Save Dictionary Items**: Navigate to the Translation section and save at least one dictionary item to initialize translations
4. **View Your Site**: The forum should now be accessible at the `/forums` URL



### License
Copyright &copy; 2022-2025 [Huw Reddick](https://umbraco.themediawizards.co.uk), and other contributors.

Licensed under the [MIT License](https://github.com/huwred/MediaWizForums-14/blob/Umbraco17/LICENSE.md).


