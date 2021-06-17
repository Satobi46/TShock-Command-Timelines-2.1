# TShock-Command-Timelines-2.1
==============================
This plugin lets you run custom-made macros to help with things such as countdowns. Just make them in the easy-to-understand programming language in Notepad and save them in the TShock/tshock directory.

Commands (you can use /tl instead of /timeline):
/timeline start <file> [arguments] - Starts a macro with the given filename. If any arguments are defined, type those too.
/timeline stop <file> - Stops the macro with the given filename.
/timeline show - Shows all running macros.

Permissions:
timeline.admin.useall - Lets the group start or stop any timeline.
timeline.use-[file] - Allows a group to use a specific command. For example, you can give a group the permission timeline.use-example.txt, and that group will only be able to use that timeline. You can also specify a folder such as with the permission timeline.use-registeredusers. If you create a folder within the TShock folder called that, any timeline in it will be usable for those with the permission.
  
  Updated API 2.1
  Source: https://github.com/ZakFahey/Command-Timelines/
  Source: https://tshock.co/xf/index.php?threads/command-timelines.3028/
