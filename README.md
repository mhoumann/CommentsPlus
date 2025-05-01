CommentsPlus
============

Formats code comments with extra formatting for special comments, and optionally comments in *italics*.

By Mads Houmann

Based in part on code by Noah Richards and Tomas Restrepo.

![Sample](Shared/cp-screenshot-white.png)

Special Comments:
-----------------

```csharp
//! Important - formatted as bold.
//? Question - colored red.
//x Removed - formatted as strikeout.
//TODO: Task - colored dark orange.
//TODO@NN: Task for NN - colored dark orange - case insensitive
//HACK: Task - colored dark orange.
//!? WT*!? - colored purple.
```

Supported File Types
--------------------

Should work with all or most VS supported source code file types.

Tested with: .cs, .vb, .h/.cpp, .py, .js, .ts, .ps1, .html, .xml, .xaml, .yml,

Settings
--------

The formatting can be changed in the Options dialog under Environment > Fonts and Colors > Text Editor.

Italics
-------

**NOTE:** *Italics* support is now disabled in this extension by default, since it is supported natively in VS 2022 from v17.10 🥳 🎉

You may need to manually change formatting of various comment types in the Options dialog: Environment > Fonts and Colors > Text Editor.

- Comment
- HTML Comment
- XML Doc Comment

Alternatively enable CommentsPlus *italics* support via the registry setting - see below.

Registry Settings
-----------------

HKCU\Software\CommentsPlus

* DWORD value EnableTags
* DWORD value EnableItalics
