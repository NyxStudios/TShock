<p align="center">
  <img src="https://tshock.co/newlogo.png" alt="TShock for Terraria"><br />
  <a href="https://ci.appveyor.com/project/hakusaro/tshock">
    <img src="https://ci.appveyor.com/api/projects/status/chhe61q227lqdlg1?svg=true" alt="AppVeyor Build Status">
  </a>
  <a href="https://github.com/Pryaxis/TShock/actions">
    <img src="https://github.com/Pryaxis/TShock/workflows/Build%20Server/badge.svg" alt="GitHub Actions Build Status">
  </a>
  <a href="#contributors">
    <img src="https://img.shields.io/badge/all_contributors-1-orange.svg?style=flat-square" alt="All contributors">
  </a>
  <br/><br/>
  <a href="https://github.com/Pryaxis/TShock/blob/general-devel/README_cn.md">查看中文版</a>
</p>

TShock is a toolbox for Terraria servers and communities. That toolbox is jam packed with anti-cheat tools, server-side characters, groups, permissions, item bans, tons of commands, and limitless potential. It's one of a kind.

**We are currently polishing on Terraria 1.4.0.2 (curRelease = 226). For updates, check our [twitter, @Pryaxis](https://twitter.com/Pryaxis).**

* Download: [Stable](https://github.com/TShock/TShock/releases) or [Experimental](#experimental-downloads).
* Read [the documentation](https://tshock.readme.io/) to quickly get up to speed.
* Join [Discord](https://discord.gg/Cav9nYX).
* Use the ancient [old forums](https://tshock.co/xf/index.php?resources/) to find old stuff.
* Talk on [GitHub discussions](https://github.com/Pryaxis/TShock/discussions) to ask for help, chat, and other things. This is the best way to get help if Discord isn't your thing.

----

## Table of Contents

  * [New to TShock?](#new-to-tshock)
  * [Experimental Downloads](#experimental-downloads)
  * [Developer's Guide](#developers-guide)
    * [Background](#background)
    * [Building](#building)
      * [On Windows](#on-windows)
        * [The Terraria Server API](#the-terraria-server-api)
        * [TShock](#tshock)
      * [On macOS](#on-macos)
      * [On Linux](#on-linux)
      * [On Unix](#on-unix)
        * [The Terraria Server API](#the-terraria-server-api-1)
        * [TShock](#tshock-1)
    * [Working with Terraria](#working-with-terraria)
  * [Code of Conduct](#code-of-conduct)

## New to TShock?

_These instructions assume Windows. If you're setting up on Linux or macOS, please refer to [the in-depth guide](https://tshock.readme.io/docs/getting-started) (and don't forget to install the *latest version* of `mono-complete` on Linux)._

1. Download [the latest stable version](https://github.com/TShock/TShock/releases) and `unzip` the folder using your favorite unzip tool. Make sure that all of the files in the zip get into one folder. This is where your server will be stored. The file structure looks like this:

      
          GeoIP.dat
          Newtonsoft.Json.dll
          OTAPI.dll
          ServerPlugins\
          |------BCrypt.Net.dll
          |------HttpServer.dll
          |------Mono.Data.Sqlite.dll
          |------MySql.Data.dll
          |------TShockAPI.dll
          TerrariaServer.exe
          sqlite3.dll
      

1. Start `TerrariaServer.exe` and TShock will boot. Answer the startup questions, and you should be ready to roll. In the background, TShock made some folders for you. We'll come back to those later.

1. Startup Terraria. Connect to a `multiplayer` server via IP and enter `localhost` if you're doing this on your local computer. If you're doing it on another computer, you need its IP address.

1. Look at the server console for the _setup code_. Type `/setup [code]` (example: `/setup 12345`), then a space, then the code you see in the console in your game chat. Instead of chatting, you'll run a command on the server. This one makes you temporary admin. All commands are prefixed with `/` or `!` (to make them silent).

1. Use the in-game command `/user add [account name] [password] owner` (example: `/user add shank lovely-ashes owner`) to create an account. This gives you owner rights on your server, which you can configure more to your liking later.

1. Login to your newly created account with `/login [account name] [password]` (example: `/login shank lovely-ashes`). You should see a login success message.

1. Turn off the setup system with `/setup` and your server is setup for initial use. TShock also created several files inside a new `tshock` folder. These files include `config.json` (our big configuration file), `sscconfig.json` (the server side characters configuration file), and `tshock.sqlite`. Don't lose your `tshock.sqlite` or you'll have to re-setup TShock.

1. You can now [customize your configuration](https://tshock.readme.io/docs/config-settings), build groups, ban items, and install more plugins.

## Experimental Downloads

To download experimental versions of TShock, you have two real options: AppVeyor builds or GitHub builds. You can also get archived Travis CI builds. Fair warning though: experimental versions of TShock are point-in-time releases that are not technically supported by us. If you have to report an issue, please make it clear which commit or branch you downloaded your build from, which service, and the build number if applicable.

On [AppVeyor](https://ci.appveyor.com/project/hakusaro/tshock/), click on history, find the build you want, click on the commit message, and then click on the artifacts tab. You can download either the debug or the release build. AppVeyor only keeps builds back 6 months though.

On [GitHub](https://github.com/Pryaxis/TShock/), click on the actions tab, then click on "build server" on the commit or branch you want. If it was successful, you can download either the experimental release or debug artifacts.

For old builds from Travis CI, you can still get them (for now) from us directly, on [our Travis CI artifact mirror](https://travis.tshock.co/). Please note that these builds should be considered legacy and for archival purposes only. If you need them in the long term, please raise an issue explaining why before they are removed.

## Developer's Guide

Whether you want to contribute to TShock by sending a pull request, customize it to suit your own elvish desires, or want to build your own plugin, this is the best starting point. By the end of this, you'll be able to build TShock from source, start to finish. More than that, though, you'll know how to start on the path of becoming an expert TShock developer.

But first, you need some background.

### Background

Terraria is a C# application written on the .NET framework using the XNA game framework. TShock is a mod for Terraria's server, which is also written in C# on the .NET framework. Some might compare TShock to hMod in the Minecraft world (the precursor to Bukkit and its server, CraftBukkit). This is a good comparison to make in how the underlying build process works. When the project started, TShock was injected directly into the decompiled source code for Terraria. Unlike Minecraft, Terraria is not obfuscated, which means that many variable names and inner workings are sanely-named out of the box. Now, TShock uses advanced techniques to operate.

TShock is, first and foremost, a plugin written for the server variant of the Terraria API, an unofficial construct originally built by `bladecoding`. `TShock` has been colloquially used to refer to both the plugin as well as the server and plugin together. Similarly, the Terraria API's client version was abandoned long ago, and development of the `Server` API led to the abbreviation `TSAPI`, for `Terraria Server API`. The plugin `TShock` is executed by the [Terraria Server API](https://github.com/Pryaxis/TerrariaAPI-Server), which is in turn bound to the `Open Terraria API`, more commonly `OTAPI`. The [Open Terraria API](https://github.com/DeathCradle/Open-Terraria-API) is maintained by [DeathCradle](https://github.com/DeathCradle).

Now, the way that `TShock` runs on `TSAPI` through `OTAPI` can be summarized as the following:

1. The Open Terraria API deeply integrates with Terraria by modifying the official server's binary directly. This is done through rewriting the Terraria bytecode, the [CIL code](https://en.wikipedia.org/wiki/Common_Intermediate_Language), using a patching tool designed by DeathCradle and tools from the Mono project. For `TSAPI`, additional modifications are done to support TSAPI specific features. This done through the `TShock Mintaka Patcher`.
2. The `Terraria Server API` uses hooks provided by `OTAPI` to provide higher level hooks as well as legacy hooks for existing TSAPI applications.
3. `TShock` is executed by `TSAPI`, uses hooks provided by both `TSAPI` and `OTAPI`, and provides even higher level hooks and support tools to other `TSAPI` plugins.

With all of this in mind, the primary goal when compiling TShock is to remember that only the second and third layers are required to be interacted with. The first layer, `OTAPI`, is provided pre-compiled through NuGet. The second layer, `TSAPI`, is provided in the `TShock` repository through a git submodule. Its primary home is the [Terraria Server API repository](https://github.com/Pryaxis/TerrariaAPI-Server).

Let's get started.

### Building

#### Prerequisite

1. git installed, you may gain help from [this](https://dotnet.microsoft.com/download/dotnet-core/3.1)
1. dotnet core sdk 3.1 installed, you may gain help from [this](https://dotnet.microsoft.com/download/dotnet-core/3.1)

#### Get the source code

Open your command line tool or terminal, input following commands
```
          $ git clone https://github.com/Pryaxis/TShock
          $ git submodule update --init
```
These commands will pull the source code down to your disk

#### Compile

If you are using IDE(like [Visual Studio](https://visualstudio.microsoft.com/) or [JetBrains Rider](https://www.jetbrains.com/rider/) ):
1. Open TShock.sln in the source code folder
1. Click the build button of your IDE

If you are using command line, just input `dotnet build TShock.sln`

### Working with Terraria

Working with Terraria in TShock and in other Terraria Server API plugins is different from most other APIs. Due to the nature of how OTAPI works, you have direct access to all public fields in the `Terraria` namespace. This means that you can access Terraria member methods directly. TShock and other plugins do this quite often, mostly to modify the game world, send data, and receive data. Calls to `Main` are one such example of direct access to Terraria. This is the equivalent to `net.minecraft.server` (NMS) calls in CraftBukkit.

You might find yourself wondering where these fields are. Pryaxis provides the decompiled [Sources](https://github.com/pryaxis/Sources) to Terraria's server, updated with each release. Note that these decompiled servers do not re-compile. The process of fixing the decompiles has proven to be nearly impossible in a reasonable timeframe with the modern Terraria Server.

Finally, you may be interested in developing other Terraria Server API plugins. The [TShockResources](https://github.com/TShockResources) organization has several plugins you can look at and build on. TShock is itself a plugin, and most plugins are open source. This gives you ample room to figure out where to go next.

Need help? Join us on [Telegram](https://t.me/pryaxis) or [Discord](https://discord.gg/Cav9nYX).

## Code of Conduct

> By participating in the TShock for Terraria community, all members will adhere to maintaining decorum with respect to all humans, in and out of the community. Members will not engage in discussion that inappropriately disparages or marginalizes any group of people or any individual. Members will not attempt to further or advance an agenda to the point of being overbearing or close minded (such as through spreading FUD). Members will not abuse services provided to them and will follow the guidance of community leaders on a situational basis about what abuse consists of. Members will adhere to United States and international law. If members notice a violation of this code of conduct, they will not engage but will instead contact the leadership team on either the forums or Discord.

> Do not attempt to circumvent or bypass the code of conduct by using clever logic or reasoning (e.g., insulting Facepunch members, because they weren't directly mentioned here).

## Contributors

Thanks goes to these wonderful people ([emoji key](https://allcontributors.org/docs/en/emoji-key)):

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tr>
    <td align="center"><a href="https://avikav.net"><img src="https://avatars2.githubusercontent.com/u/18518861?v=4" width="100px;" alt=""/><br /><sub><b>AviKav</b></sub></a><br /><a href="https://github.com/Pryaxis/TShock/issues?q=author%3AAviKav" title="Bug reports">🐛</a> <a href="https://github.com/Pryaxis/TShock/commits?author=AviKav" title="Tests">⚠️</a></td>
    <td align="center"><a href="https://tshock.co"><img src="https://avatars0.githubusercontent.com/u/3332657?v=4" width="100px;" alt=""/><br /><sub><b>Rodrigo Rente</b></sub></a><br /><a href="https://github.com/Pryaxis/TShock/commits?author=AxisKriel" title="Code">💻</a> <a href="#projectManagement-AxisKriel" title="Project Management">📆</a> <a href="https://github.com/Pryaxis/TShock/commits?author=AxisKriel" title="Tests">⚠️</a></td>
    <td align="center"><a href="https://sgkoi.dev"><img src="https://avatars2.githubusercontent.com/u/9637711?v=4" width="100px;" alt=""/><br /><sub><b>Stargazing Koishi</b></sub></a><br /><a href="https://github.com/Pryaxis/TShock/commits?author=sgkoishi" title="Code">💻</a> <a href="#infra-sgkoishi" title="Infrastructure (Hosting, Build-Tools, etc)">🚇</a></td>
    <td align="center"><a href="https://github.com/AxeelAnder"><img src="https://avatars2.githubusercontent.com/u/25691207?v=4" width="100px;" alt=""/><br /><sub><b>Axeel</b></sub></a><br /><a href="https://github.com/Pryaxis/TShock/commits?author=AxeelAnder" title="Documentation">📖</a> <a href="#projectManagement-AxeelAnder" title="Project Management">📆</a></td>
    <td align="center"><a href="https://aurora-gaming.org/"><img src="https://avatars0.githubusercontent.com/u/58985873?v=4" width="100px;" alt=""/><br /><sub><b>Patrikkk</b></sub></a><br /><a href="https://github.com/Pryaxis/TShock/commits?author=Patrikkk" title="Code">💻</a> <a href="https://github.com/Pryaxis/TShock/commits?author=Patrikkk" title="Documentation">📖</a> <a href="https://github.com/Pryaxis/TShock/commits?author=Patrikkk" title="Tests">⚠️</a></td>
  </tr>
</table>

<!-- markdownlint-enable -->
<!-- prettier-ignore-end -->
<!-- ALL-CONTRIBUTORS-LIST:END -->

This project follows the [all-contributors](https://github.com/all-contributors/all-contributors) specification. Contributions of any kind welcome!
