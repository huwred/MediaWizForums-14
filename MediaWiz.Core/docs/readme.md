# MediaWizForums 14 #
Simple Forum add on for Umbraco 14+. 

## 17.0.0

- **Umbraco 17 Compatibility**  
  Verified and updated to run smoothly on Umbraco 17 and later versions.

- **Bundled tinyMCE Editor 5.10.7**  
  A local version of **tinyMCE** is included to maintain compatability with the existing code now that Umbraco no longer uses it. Ensures consistent post‑editing without external reliance.

- **package.xml Adjustments**  
  New package created in Umbraco 17, improved Templates, CSS rendering and maintain consistent styling across forum components.

- **Scripts TagHelper**
  Added a TagHelper for rendering CSS and JS scripts into the forum Layout from partials and viewcomponents. You will need to add a reference for it in your `_ViewImports.cshtml` file.

  ```csharp
  @using MediaWiz.Forums.TagHelpers
  ...
  @addTagHelper *, MediaWiz.Forums
  ```
  
- **Indexing/Search**
  New and improved indexing for members and posts. Advanced search options added for finding posts.

- **General**  
  Codebase reviewed for compatibility, with small refinements to keep the package lightweight and developer‑friendly, improved error handling.


## Previous versions

### 16.0.2
Fixes and refactoring for Umbraco 16 migration scripts.

### 16.0.1
Included local version of tinyMCE for the forum posts editor, this is to avoid issues with the Umbraco 16.0.1 update that removed the tinyMCE package from Umbraco.
This version of MediaWizForums is compatible with Umbraco 16.0.0 and later.
Minor changes to the package.xml to improve css look and feel of the forum.

### 16.0.0
Minor changes to enable installation on Umbraco 16

### 14.1.0

Added support for Umbraco15 + .net 9

### 14.0.2

Fixed issues with entrypoints in the backoffice

### 14.0.0

If you want to use a member type other than the created "forumMember", add the following setting in appsettings.json
```
  "MediaWizOptions": {
    "MemberTypeAlias": "myMemberType",
    ...
  }
```

If your website already contains any document types using the same aliases as the forum package. 

doctypes created that may cause conflicts "login", "members", "profile", "register", "reset", "verify"

If you encounter this issue it is possible to add a setting to the "MediaWizOptions"

```
  "MediaWizOptions": {
    "ForumDoctypes": "prefix",
    ...
  }
```
Adding this value will force the install to load a different package.xml in the migration and create the document types using the prefix "forum" instead, this should avoid any conflicts

Tested in v14 of Umbraco


Config section in appsettings.json
```
  "MediaWizOptions": {
    "MaxFileSize": 8,  - Maximum file size in MB
    "AllowedFiles": [ ".gif", ".jpg", ".png", ".svg", ".webp" ], - Allowed image file extensions
    "UniqueFilenames": true - if true uses random guid for filename, if false uses name of uploaded file
  }
```

