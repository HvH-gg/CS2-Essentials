![Copyright ev0lve Digital](https://img.shields.io/badge/Copyright-ev0lve%20Digital-blue) ![GitHub License](https://img.shields.io/github/license/HvH-gg/CS2-Essentials) ![Issues](https://img.shields.io/github/issues/HvH-gg/CS2-Essentials) ![Downloads](https://img.shields.io/github/downloads/HvH-gg/CS2-Essentials/total) ![Stars](https://img.shields.io/github/stars/HvH-gg/CS2-Essentials)

# [HvH.gg](https://hvh.gg) CS2 Essentials (1.2.3)
### If you use this plugin, you do NOT need [RapidFireFix](https://github.com/HvH-gg/RapidFireFix) anymore.

Our CS2 Essentials plugin is the only plugin you need to run a successful HvH server. It includes basic features like **reset score** and **rage quit** as well as optional restrictions for **weapons**, **friendly fire**, **rapid fire** and other exploit/crash fixes.

# Features
- Custom Vote support (requires [CS2-CustomVotes](https://github.com/imi-tat0r/CS2-CustomVotes))
- Reset score `!rs`
- Rage quit `!rq`
- Restrict weapons (awp, scout, autosniper)
- Restrict friendly fire (default, only utility damage)
- Restrict rapid fire (0 allows rapid fire, 1 blocks rapid fire, 2 reflects damage scaled, 3 reflects damage but keeps the player alive)
- **Linux only:** Restrict teleport/airstuck exploit (enabling this will also prevent the server crash exploit)

# Requirements
- [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)
- [CounterStrikeSharp(v191)](https://github.com/roflmuffin/CounterStrikeSharp/releases)

# Install
1. Install Metamod:Source and CounterStrikeSharp
2. Place the `addons` folder in your servers `game/csgo/` directory  
   ![extract](https://du.hurenso.hn/r/0NyFPY.png)
3. Edit the config file to your liking  
   3.1. Located at `addons/counterstrikesharp/configs/plugins/CS2-Essentials/CS2-Essentials.json`
4. Restart your server

# Config
```json
{
   "RapidFireFixMethod": 1, // 0 allows rapid fire, 1 blocks rapid fire, 2 reflects damage scaled, 3 reflects damage but keeps the player alive
   "RapidFireReflectScale": 1, // damage reflect percentage (0.0 - 1.0)
   "AllowedAwpCount": -1, // how many awps are allowed per team (0 for none, -1 for unlimited)
   "AllowedScoutCount": -1, // how many scouts are allowed per team (0 for none, -1 for unlimited)
   "AllowedAutoSniperCount": -1, // how many auto snipers are allowed per team (0 for none, -1 for unlimited)
   "UnmatchedFriendlyFire": true, // if true, only utility damage will be dealt to teammates (like on unmatched.gg)
   "RestrictTeleport": true, // if true, the teleport and airstuck exploit will be restricted. This will also prevent the server crash exploit
   "AllowAdPrint": true, // if true, players will see a "powered by HvH.gg" ad in the chat with the settings print
   "AllowSettingsPrint": true, // if true, players will see an overview of the server settings with `!settings` and on spawn
   "AllowResetScore": true, // if true, players will be able to reset their score with `!rs`
   "AllowRageQuit": true, // if true, players will be able to rage quit with `!rq`
   "ChatPrefix": "[{Red}Hv{DarkRed}H{Default}.gg]", // chat prefix for plugin messages
   "CustomVoteSettings": { // settings for custom votes (requires CS2-CustomVotes)
      "FriendlyFireVote": false, // if true, players will be able to vote for friendly fire settings
      "TeleportFixVote": false, // if true, players will be able to vote for the teleport fix settings
      "RapidFireVote": "full", // You can set the style (`off`, `simple`, `full`) for the rapid fire vote
      "Style":"center" // You can set the style (`center` or `chat`) for the vote menu (might be overridden by CS2-CustomVotes settings)
   },
   "ConfigVersion": 4 // do not change
}
```

# ConVars
Instead of editing the config file, you can also use the following ConVars to change certain settings **on the fly**. These changes will **NOT** be saved to the config file and will be reset after a server restart.
- `hvh_restrict_rapidfire` 0 allows rapid fire, 1 blocks rapid fire, 2 reflects damage scaled, 3 reflects damage but keeps the player alive
- `hvh_rapidfire_reflect_scale` damage reflect percentage, 0.0 - 1.0
- `hvh_restrict_teleport` if true, the teleport and airstuck exploit will be restricted. This will also prevent the server crash exploit
- `hvh_restrict_awp` how many awps are allowed per team, 0 for none, -1 for unlimited
- `hvh_restrict_scout` how many scouts are allowed per team, 0 for none, -1 for unlimited
- `hvh_restrict_auto` how many auto snipers are allowed per team, 0 for none, -1 for unlimited
- `hvh_unmatched_friendlyfire` if true, only utility damage will be dealt to teammates
- `hvh_resetscore 0/1` if true, players will be able to reset their score with `!rs`
- `hvh_ragequit 0/1` if true, players will be able to rage quit with `!rq`

# ChatPrefix Colors
You can use all available colors from CounterStrikeSharp in the chat prefix.

# Custom Votes
If you have [CS2-CustomVotes](https://github.com/imi-tat0r/CS2-CustomVotes) installed, you can use the following custom votes:
- `!rapidfire` You can set the style (`off`, `simple`, `full`) in the config via `CustomVoteSettings.RapidFireVote`
- `!friendlyfire` You can enable this in the config via `CustomVoteSettings.FriendlyFireVote`
- `!teleport` You can enable this in the config via `CustomVoteSettings.TeleportVote`
> **Warning:** Teleport restriction is needed to prevent the server crash exploit. If you enable this vote, the server will be vulnerable to the crash exploit.

# Credits
- [HvH.gg](https://hvh.gg)
- [Metamod:Source](https://www.sourcemm.net/)
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
- [MagicBulletFix](https://github.com/CS2Plugins/MagicBulletFix)

# Buy us a coffee
If you like our work, consider [buying us a coffee](https://hvh.gg/plugins).
