**currently abandoned**

The idea of xbridge **was** to maximize code sharing betweed different platforms, including the browser. If anybody wishes to revive the code feel free (MIT License).

So like cordova, xbridge uses javascript for ui/frontend code. In contrast to cordova it uses .NET / Xamarin for the backend, because in contrast to Java, .NET runs on ALL platforms including iOS. 
Only system interaction is meant to be written in .NET. All application logic is meant to be written in javascript.

A few plugins to interact with the system are already implemented. 

- Basic sound is working. 
- Some slightly more reliable timers.
- a wrapper around sqlite to enable websql
- some interaction with a parent window (very partial)
- file access


This is a project I started a few years ago. The code in here should work, but dependencies and build files need to be recreated and updated.
