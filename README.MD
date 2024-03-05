![Copyright ev0lve Digital](https://img.shields.io/badge/Copyright-ev0lve%20Digital-blue) ![GitHub License](https://img.shields.io/github/license/HvH-gg/CS2-Essentials) ![Issues](https://img.shields.io/github/issues/HvH-gg/CS2-Essentials) ![Downloads](https://img.shields.io/github/downloads/HvH-gg/CS2-Essentials/total) ![Stars](https://img.shields.io/github/stars/HvH-gg/CS2-Essentials)

# HvH.gg CS2 Essentials (1.0.0)
### If you use this plugin, you do NOT need [RapidFireFix](https://github.com/HvH-gg/RapidFireFix) anymore.

Our CS2 Essentials plugin is the only plugin you need to run a successful HvH server. It includes basic features like **reset score** and **rage quit** as well as optional restrictions for **weapons**, **friendly fire** and **rapid fire**.

# Features
- Reset score `!rs`
- Rage quit `!rq`
- Restrict weapons (awp, scout, autosniper)
- Restrict friendly fire (default, only utility damage)
- Restrict rapid fire (0 allows rapid fire, 1 blocks rapid fire, 2 reflects damage scaled, 3 reflects damage but keeps the player alive)
- Restrict teleport

# Requirements
- [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=stable)
- [CounterStrikeSharp(v167)](https://github.com/roflmuffin/CounterStrikeSharp/releases)

# Install
1. Install Metamod:Source and CounterStrikeSharp
2. Place the `addons` folder in your servers `game/csgo/` directory  
   ![extract](https://du.hurenso.hn/r/0NyFPY.png)
3. Edit the config file to your liking  
   3.1. Located at `addons/counterstrikesharp/configs/plugins/CS2Essentials/CS2Essentials.json`
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
   "RestrictTeleport": true, // if true, the teleport exploit will be restricted and players will be teleported back to their original position
   "AllowAdPrint": true, // if true, players will see a "powered by HvH.gg" ad in the chat with the settings print
   "AllowResetScore": true, // if true, players will be able to reset their score with `!rs`
   "AllowRageQuit": true, // if true, players will be able to rage quit with `!rq`
   "ChatPrefix": "[{Red}Hv{DarkRed}H{Default}.gg]" // chat prefix for plugin messages
   "ConfigVersion": 2 // do not change
}
```

# ChatPrefix Colors
You can use all available colors from CounterStrikeSharp in the chat prefix.

# Credits
- [Metamod:Source](https://www.sourcemm.net/)
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
- [MagicBulletFix](https://github.com/CS2Plugins/MagicBulletFix)

# Buy us a coffee
If you like our work, consider [buying us a coffee](https://hvh.gg/plugins).