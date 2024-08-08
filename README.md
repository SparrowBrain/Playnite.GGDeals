# Playnite.GGDeals
![DownloadCountTotal](https://img.shields.io/github/downloads/sparrowbrain/playnite.ggdeals/total?label=total%20downloads&style=for-the-badge)
![LatestVersion](https://img.shields.io/github/v/release/SparrowBrain/Playnite.GGDeals?label=Latest%20version&style=for-the-badge)
![DownloadCountLatest](https://img.shields.io/github/downloads/SparrowBrain/Playnite.GGDeals/latest/total?style=for-the-badge)

## What Is It?
Playnite extension to sync library with GG.deals website.

You can add games via:
* Game menu;
* Extensions menu;
* Games are added to GG.deals automatically when you add them to your library, after the metadata is fetched.

![Main NextPlay view screenshot](/ci/screenshots/01.jpg)
![Main NextPlay view screenshot](/ci/screenshots/02.jpg)
![Main NextPlay view screenshot](/ci/screenshots/03.jpg)

Currently there is no way to remove games from GG.deals via this extension, and it's not planned.

## How to use it?
In order to use this extension, you will need to generate an authentication token on [GG.deals website](https://gg.deals/):
1. Login
2. Go to [Settings](https://gg.deals/settings/) (link here, or click on your avatar top right)
3. Connect to Playnite
4. Copy the generated token

And then in Playnite:
1. Playnite menu
2. Add-ons...
3. Extensions settings
4. Generic
5. GG.deals
6. Paste the token into "Authentication Token" field

![Main NextPlay view screenshot](/ci/screenshots/05.jpg)

You can also allow the extension to add GG.deals link to processed game's page.

## How does it work?
It uses GG.deals API to import the games. The extension sends these game fields:
* Id
* GameId
* Links
* Source
* ReleaseDate
* ReleaseYear
* Name
* ggLauncher (custom fields that depends on game's library plugin ID)

GG.deals then uses this data to match the game with one on their database. API can return these outcomes:
* Added - game was added to your collection
* Skipped - game was skipped, because it was already in your collection
* Ignored - the item was ignored, because it's not applicable for GG.deals (demo, beta, or something similar)
* Miss - game was not found in GG.deals database
* Error - there was an error while adding the game

In case of errors and misses, you can check the failures list for more information.

![Main NextPlay view screenshot](/ci/screenshots/04.jpg)

## Disclaimers
* While this extension uses GG.deals API and received support from the devs, it is not an official extension.
* The extension sends the game data listed above to the API for matching. Don't store any personal data there.
* API matching accuracy will depend on the quality of your games' metadata. As of writing, links are very important for accurate matching.
* The extension adds tags with GG.deals status to your games (for example `[GGDeals] Synced`). This allows the extension to track which games were synced, etc.

## Installation
You can install it either from Playnite's addon browser, or from [the web addon browser](https://playnite.link/addons.html#SparrowBrain_GGDeals).

## Translation
You can help with translation by visiting [the project on Crowdin](https://crowdin.com/project/sparrowbrain-playnite-ggdeals).
