# Playnite.GGDeals
![DownloadCountTotal](https://img.shields.io/github/downloads/sparrowbrain/playnite.ggdeals/total?label=total%20downloads&style=for-the-badge)
![LatestVersion](https://img.shields.io/github/v/release/SparrowBrain/Playnite.GGDeals?label=Latest%20version&style=for-the-badge)
![DownloadCountLatest](https://img.shields.io/github/downloads/SparrowBrain/Playnite.GGDeals/latest/total?style=for-the-badge)

## What Is It?
Playnite extension to sync library with GG.deals website.

You can add games via:
* Game menu;
* Extensions menu;
* Games are added to GG.deals automatically when you add them to your library.

Currently there is no way to remove games from GG.deals via this extension, and it's not planned.

## How does it work?
Currently, there is no GG.deals API, so the extension simulates clicks on the GG.deals website to add games to your library. This requires you to login into your account.

The extension then when asked to add a game will use the game name to guess the url on the website. If the game exists by that url, the game will be added to respective launcher (depending on the library plugin). 

## Recommended usage
1. Authenticate in the addon settings; 
    ![Main NextPlay view screenshot](/ci/screenshots/05.jpg)
2. Select libraries you want to sync. By default all libraries except for Steam and GOG will be synced;
3. Either sync individual games via the game menu, or sync all games via Main menu -> Extensions -> GG.deals -> Add games to GG.deals collection...;
    ![Main NextPlay view screenshot](/ci/screenshots/01.jpg)
    ![Main NextPlay view screenshot](/ci/screenshots/02.jpg)
    ![Main NextPlay view screenshot](/ci/screenshots/03.jpg)
4. In case there are any syncing failures, check the Main menu -> Extensions -> GG.deals -> Show failures...;
5. Manually add the games to you collection on the website. Remove these games from failures list;
    ![Main NextPlay view screenshot](/ci/screenshots/04.jpg)

In my experience this extension can successfully add ~80% of games. Sadly, the url pattern is not as consistent as I expected, hence some manual labor will be required.

## Disclaimers
* This extension is not affiliated with GG.deals in any way;
* The extension requires you to login into your GG.deals account;
* The extension simulates clicks on the website on your behalf;
* The extension does not use GG.deals API, instead it uses the website. Expect it to break when the website changes. Expect that I won't have the time to fix it.
* The extension uses game name to add it to you library. If the game is Deluxe edition, Complete edition or any other subvariant it might fail to add it as such. Likewise, if there are multiple games with the same name, if might add the wrong one.

## Installation
You can install it either from Playnite's addon browser, or from [the web addon browser](https://playnite.link/addons.html#SparrowBrain_GGDeals).
