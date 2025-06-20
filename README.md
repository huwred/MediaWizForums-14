## For versions of Umbraco prior to 14 please use https://github.com/huwred/MediaWizForums ##

# MediaWizForums 14 #
Simple Forum add on for Umbraco 14. 

## 16.0.0
Minor changes to enable installation on Umbraco 16

## 14.0.0 ##

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

