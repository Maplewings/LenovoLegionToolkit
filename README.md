# Lenovo Legion Toolkit

This is a small utility created for Lenovo Legion laptops, that allows to change a couple of features that are only available in Lenovo Vantage.

It is a single executable app that needs no installation and runs no background services. It uses less memory, virtually no CPU and contains no telemetry.

![screenshot](assets/screenshot.png)


## Disclaimer

This is a hobby project. I want to make it available on more devices, but it will take some time, so please be patient and read through this readme carefully.

The tool comes with no warranty. Use at you own risk.


## Compatibility

* Lenovo Legion 7 16ACHg6
* Lenovo Legion 7 16ITHg6
* Lenovo Legion 7 15IMHg05
* Lenovo Legion 5 Pro 16ACH6H
* Lenovo Legion 5 15ACH6H
* Lenovo Legion 5 15ARH05
* Lenobo Legion 5 15IMH05H
* Lenovo Legion 5 R7000P2021H
* Lenobo Legion S7 15ACH6
* Lenovo Legion Y740 17IRHg
* Lenovo Legion Y540 15IRH

This app can probably work on a lot more 2020/2021 Lenovo laptops. If you want to help expand the list check the *Contribution* section.


## Features

The app allows to:

* Switching between Quiet, Balance and Performance modes, including changing Windows power plans,

* Enabling and disabling Hybrid Mode,

* Changing Battery charging options: conservation, rapid charging and normal charging,

* Always on USB charge settings: on, off and on when sleeping,

* Flip to start, Over Drive, Fn and Touchpad locks.

This app also allows you to disable Lenovo Vantage without uninstalling it. It it especially useful, you want to keep Vantage around i.e. for checking updates.

You can do it from the Tools menu on top. If you disable Vantage, it will stop and disable it's services, and disable all funky Lenovo Scheduled tasks. Once Vantage is disabled, it will no longer start any background processes on startup. If you want to open Lenovo Vantage, you need to re-enable it from the tool, otherwise it will prompt to reinstall itself.


## Requirements

This tool is written with .NET 5 which means you need to have **.NET Desktop Runtime 5**. You can get it from here:

https://dotnet.microsoft.com/download/dotnet/5.0/runtime

You don't need to install it if you already have the .NET SDK 5 installed.


## Credits

Credits go to ViRb3, for creating Lenovo Controller, which was used as a base for this tool.

Check out his repo: https://github.com/ViRb3/LenovoController


## Contribution

I appreciate any feedback that you have, so please do not hesitate to report issues. PRs are also welcome!

It would be great to expand the list of compatible devices, but to do it your help is needed.

If you are willing to check if this app works correctly on your device that is currently unsupported, you can do it by starting the app with ``--skip-compat-check`` argument.

Remember that some functions may not function properly, so keep this in mind.

If you do it, I would really appreciate if you create an issue here on GitHub with the results of your testing.

Make sure to include following information in your issue:

1. Full model name (i.e. Legion 5 Pro 16ACH6H)
2. List of features that are working as expected.
3. List of features that seem to not work (they do nothing).
4. List of features that crash the app.

The more info you add, the better the app will get over time.

Thanks in advance!
