# page-title-change-detector
A tiny Windows utility program written in C# that downloads a web page's source code at a regular interval and alerts you when the title changes

This program offers one simple functionality:
* Select an interval (in seconds)
* Enter the URL of a web page
* Press 'Start'

When you press 'Start', the program will retrieve the page title once and set it as 'Initial page title'.
Then it will redownload the same page regularly at the specified interval.
If a change is detected, the program stops checking, emits a sound, and displays an alert message box.

Requirements:
* Microsoft Windows
* .NET Framework 4.5.1+
* Internet connection
