CommentsPlus
============

Formats code comments in italics, and enables extra formatting for special comments

By Mads Houmann

Based in part on code by Noah Richards and Tomas Restrepo.

Special Comments:
-----------------

```
//! Important - formatted as bold.
//? Question - colored red.
//x Removed - formatted as strikeout.
//TODO: Task - colored light green.
//!? WT*!? - colored purple.
```

Supported File Types
--------------------

Should work with all VS supported source code file types.

Tested with: .cs, .vb, .h/.cpp, .py, .js, .ps1, .html, .xml, .xaml,

Settings
--------

The formatting can be changed in the Options dialog under Environment, Fonts and Colors, Text Editor.

Registry Settings
-----------------

HKCU\Software\CommentsPlus

* DWORD value EnableTags
* DWORD value EnableItalics
