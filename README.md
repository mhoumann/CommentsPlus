CommentsPlus
============

Formats code comments in *italics*, and enables extra formatting for special comments

By Mads Houmann

Based in part on code by Noah Richards and Tomas Restrepo.

![Sample](Shared/cp-screenshot-white.png)

Special Comments:
-----------------

```C#
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

Tested with: .cs, .vb, .h/.cpp, .py, .js, .ts, .ps1, .html, .xml, .xaml,

Settings
--------

The formatting can be changed in the Options dialog under Environment, Fonts and Colors, Text Editor.

Registry Settings
-----------------

HKCU\Software\CommentsPlus

* DWORD value EnableTags
* DWORD value EnableItalics
