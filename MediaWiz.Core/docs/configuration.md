# MediaWiz Forums
[![Platform](https://img.shields.io/badge/Umbraco-17-%233544B1?style=flat&logo=umbraco)](https://umbraco.com/products/umbraco-cms/)
[![NuGet](https://img.shields.io/nuget/v/MediaWiz.Forums.14.svg)](https://www.nuget.org/packages/MediaWiz.Forums.14/)
[![GitHub](https://img.shields.io/github/license/huwred/MediaWizForums-14?label=license&style=flat)](https://github.com/huwred/MediaWizForums-14/blob/Umbraco17/LICENSE)

**MediaWiz Forums** provides a basic Forum add on for Umbraco.

## Installation
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

The Umbraco v17 version of this package is [available via NuGet](https://www.nuget.org/packages/MediaWiz.Forums.14).

To install the package, you can use either .NET CLI:

```
dotnet add package MediaWiz.Forums.14 --version 17.0.0
```

or the NuGet Package Manager:

```
Install-Package MediaWiz.Forums.14 -Version 17.0.0
```
## Setup
The package can then be configured in `appsettings.json`:

```json
  "MediaWizOptions": {
    "MaxFileSize": 8, 
    "AllowedFiles": [ ".gif", ".jpg", ".png", ".svg", ".webp", ".mp4" ],
  },
```
## Options
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

### Settings
#### MediaWiz Forums
| Property        | Type                                      | Description                                    |
|-----------------|-------------------------------------------|------------------------------------------------|
| MediaWizOptions | [`MediaWizOptions`](#mediawizoptions)     | Configure settings for the MediaWiz Forums     |

#### Options
| Property           | Type                     | Description                                                                                                                      |
|--------------------|--------------------------|----------------------------------------------------------------------------------------------------------------------------------|
| MemberTypeAlias    | string                   | Defaults to "forumMember" if not set                                                                                             |
| ForumDoctypes      | string                   | If your site already contains any [`Document Types`](#doctypes) using the same aliases as the forum, set a prefix to avoid conflicts.  |
| MaxFileSize        | integer                  | Maximum file size in MB.                                                                                                         |
| AllowedFiles       | string[] \| List<string> | A list of file extensions that can be uploaded in a post. If left blank, no files will be allowed.                               |
| UniqueFilenames    | boolean                  | When uploading, if true a random guid is used for filenames, default uses the name of uploaded file.                             |

### Document Types
| Property        | Type            | Description                                   |
|-----------------|-----------------|-----------------------------------------------|
| searchPage      | searchPage      | Search page node                              |
| members         | members         | Members root node  (dummy holding node)       |
| profile         | profile         | Member profile node                           |
| login           | login           | Login node                                    |
| register        | register        | Registration node                             |
| reset           | signatures      | Password reset node                           |
| verify          | verify          | Email verification node                       |



---


## Getting Started

After installation, you'll need to:

1. **Login to Umbraco**: Navigate to `/umbraco` and login with the credentials you specified (default: admin@example.com / 1234567890)
2. **Publish the Home Page**: Go to the Content section and publish the home page
3. **Save Dictionary Items**: Navigate to the Translation section and save at least one dictionary item to initialize translations
4. **View Your Site**: The frontend should now be accessible at the root URL

---

## License
Copyright &copy; 2022-2025 [Huw Reddick](https://umbraco.themediawizards.co.uk), and other contributors.

Licensed under the [MIT License](https://github.com/huwred/MediaWizForums-14/blob/Umbraco17/LICENSE.md).
