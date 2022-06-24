
# TeleportEverything
 > Produced by: Kpro and Elg

![](https://staticdelivery.nexusmods.com/mods/3667/images/1806/1806-1647282849-1161973402.png)

## About

This mod is intended to add additional excitement and functionality to the portal system. Use it to brings wolves to the fight or use it to lure trolls into a trap!  These modes also work with dungeons! So its possible to bring a wolf into a swamp crypt and for a wraith to follow you into the crypt!
## Features

### Modes:

1. **Standard**. 

    Vanilla behavior for portals except that you can still transport allies.

2. **Vikings don't run**. 

    If this mode is selected from the **F1** configuration manager menu, portals will not work with enemies nearby. It is intended to make a cowardly retreat through a portal in order to avoid combat a bit more challenging but not impossible.

3. **Take them with you**. 

    If this mode is selected from the **F1** configuration manager menu, enemies within the search range will have small chance of following you through the portal. Set the search range to a high number and watch out!

4. **Transport Allies**. 

    If this mode is toggled on, allies within the search range will teleport with you. You can teleport wolves, boar, lox, or anything creature at all. Hey, if you want to bring some greydwarves through the portal with you, no one will judge ;). The ally does not need to be tamed to transport so a hostile wolf or boar can come through as easily as a tamed wolf or boar.

## Usage:

### **Message Mode**
You can change the display message mode between: top left, center, or none.

### **Transport Allies**

1. Define the ally type that you want to transport using the **F1** configuration menu. There quick check boxes to select wolves, boar, or lox. If you want to filter which animals can be teleportable, enable the filter mask and use the transport mask by typing in the spawn name of the creatures. An example would be "**Greydwarf**" without the quote marks. (In this case, only Greydwarfs would be allowed) You can obtain the list spawn names easily on the web.
2. Define the Ally Mode from the dropdown list. At present, you can select either: 
   * "No Allies",
   * "All Tamed", 
   * "Only Follow", 
   * "All tamed except Named", 
   * "Only Named".

Teleport Everything will search for allies within a search cylinder. It should not be necessary but you can configure the radius and height of the search cylinder to meet your needs. For example, you might set the height to 1.5 in order to avoid transporting allies on different levels of your base.

* Enable Server/Player Filter Mask (advanced): 
    * If enabled, only the allies in the Transport Mask field will be allowed
    * If disabled, all tameable allies can be teleportable
* Server and Player Transport Masks (advanced): accepts comma delimited list of prefab names. E.g.: wolf,lox

### **Teleport Self**

1. Select the teleport mode. Currently you can select "**Standard**" which will give you vanilla behavior.  "**Vikings don't run**" mode will prevent you from teleporting if mobs are within a search sphere. The search radius is configurable in the **F1** configuration menu. "**Take them with you**" will give mobs a chance of following you through the portal. If this mode is selected, a dash through the portal with a troll in pursuit may end up in troll fight within your base! Its a super fun mode.
2. At present, enemies will spawn at a random location forward within 6.2 meters (default) of the portal.

### **Transport Items**
If this mode is toggled on, players may transport **ores, ingots, and eggs**. In order to offset the advantage of transporting ores, players may set a **"transport fee"** that deducts a percentage of the contraband ores, ingots.

### **Portal Behavior**
* Portal Activation Range: You can increase or reduce the activation range, 5 meters is the default game value.
* Portal Sound Volume: You can change it between 0 and 100%. 80% is the default game value.

## Install Notes

It is recommended that you install the BepInEx Configuration Manager mod too. This mod will enable you to access the configuration settings simply by hitting F1 and edit your mods configs in game. 

You can also define the configuration settings directly in the cfg file which is generated after the first run. BepInEx/config/com.kpro.TeleportEverything.cfg.
## Manual Installation

>Install *BepinEx* per the author's instructions here:https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/
>Install BepInEx Configuration Manager

>Place ***TeleportEverything.dll*** into your **Bepinex\plugins** folder in valheim.

## Dependencies

To make it work properly you will need some other mods or tools called dependencies so make sure that everything is installed as it should. To make it work properly you need these dependencies:

* BepInEx Valheim (Mandatory)
* BepInEx Configuration Manager (Recommended)

## Mod Compatibility

- OdinsUndercroft: ![YES]
- Basement or BasementJVLedition: ![YES]
- AllTameableHandH: ![YES]
  - (If your vertical tolerance is low, you may not be able to transport a flying drake)
- MapTeleport: ![YES]
- FastTeleport: ![YES]

<p>
  <p align="center"><h2>For Questions or Comments find Elg or KPro in the Odin Plus Team on Discord:</h2></p>

  <p align="center"><a href="https://discord.gg/mbkPcvu9ax"><img src="https://i.imgur.com/Ji3u63C.png"></a>
</p>

## Changelog
- Version 1.6.2
  - fix tax to deduct from total number of ores instead of each stack
  - fix tax to apply also to quantities less than the tax
- Version 1.6.1
  - Add height check to prevent creatures from getting stuck in crypts
- Version 1.6
  - Added Server Sync (Now compatible with servers and clients)
  - Separated Server and Player Filter Mask (if mask is not enabled, all allies can be transported)
  - Removed delayed spawn
  - Removed fee on disconnected portals
  - Spawn offset fixed
  - Teleport with enemies fixed
- Version 1.5
  - Added delayed spawn to allies and enemies (single player)
  - Added settings to change portal sound volume and activation range
  - Added advanced settings (if you need more options, enable config manager advanced mode)
- Version 1.4
  - Added transport of ores, ingots, and eggs.
  - Added optional "transport fee" that reduces ore, ingots, and eggs as a fee for transport these "contraband" items.
  - Added configurations settings for this feature.
- Version 1.3
  - Added display messages top left, center, or none
  - Fixed teleport bug out of sunken crypts
  - Added configurable spawn location for allys
  - Transport mask now accepts comma delimited list. Ignores case and white space.
- Version 1.2
  - Fixed error in calculation of vertical distance. Fixed error determine whether boars or wolves are named.
- Version 1.1
  - Update provides additional control over transport mechanic including control over the search radius and ability to limit transport to tamed, named, follow, etc.
- Version 1.0
  - This is the alpha-version of the mod. Feedback and bug reports are appreciated.

[YES]: https://img.shields.io/badge/YES-success?style=flat-square