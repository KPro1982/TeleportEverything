# TeleportEverything
**Created by: Kpro and Elg**

This mod is intended to add additional excitement and functionality to the portal system. Use it to brings wolves to the fight or use it to lure trolls into a trap!  These modes also work with dungeons! So its possible to bring a wolf into a swamp crypt and for a wraith to follow you into the crypt!

## Features

### Modes:

1. **Standard**. Vanilla behavior for portals except that you can still transport allies.
2. **Vikings don't run**. If this mode is selected from the F1 configuration menu, portals will not work with enemies nearby. It is intended to make a cowardly retreat through a portal in order to avoid combat a bit more challenging but not impossible.
3. **Take them with you**. If this mode is selected from the F1 configuration men, enemies within the search range will have small chance of following you through the portal. Set the search range to a high number and watch out!
4. **Transport Allies**. If this mode is toggled on, allies within the search range will teleport with you. You can teleport wolves, boar, lox, or anything creature at all. Hey, if you want to bring some greydwarves through the portal with you, no one will judge ;). The ally does not need to be tamed to transport so a hostile wolf or boar can come through as easily as a tamed wolf or boar.

### Usage:

Even if it's not a hard dependency, it is recommended that you install a Configuration Manager mod. 
**With this tool you can simply hit F1 and edit your mods configs in game.**
> You can also define the values directly in the file after the first run: BepInEx/config/com.kpro.TeleportEverything.cfg.

#### **Transport Allies**

1. Define the ally type that you want to transport using the **F1** configuration menu. There are check boxes to select wolves, boar, or lox. If you want to transport something else use the transport mask by typing in the spawn name of the creature that you want to take with you. An example would be "**Greydwarf**" without the quote marks. You can obtain the list spawn names easily on the web. 
2. Define the Ally Mode from the dropdown list. At present, you can select either: **"No Allies", "All Tamed", "Only Follow", "All tamed except Named", "Only Named"**. 
Teleport Everything will search for allies within a search cylinder. It should not be necessary but you can configure the radius and height of the search cylinder to meet your needs. For example, you might set the height to 1.5 in order to avoid transporting allies on different levels of your base.

#### **Teleport Self**

1. Select the teleport mode. Currently you can select "**Standard**" which will give you vanilla behavior.  "**Vikings don't run**" mode will prevent you from teleporting if mobs are within a search sphere. The search radius is configurable in the **F1** configuration menu. "**Take them with you**" will give mobs a small chance of following you through the portal. If this mode is selected, a dash through the portal with a troll in pursuit may end up in troll fight within your base! Its a super fun mode.
2. At present, enemies will spawn at a random location within 5 meters (default) of the portal. However, this "**Max Enemy Displacement**" can be set in the F1 configuration menu. A smaller number means that the mobs will be on you as soon as you materialize. A larger number may mean that the mobs spawn in other rooms, on the roof, etc.

## Installation (manual)

Place the files to your /BepInEx/plugins/ folder.

### Dependencies

To make it work properly you will need some other mods or tools called dependencies so make sure that everything is installed as it should. To make it work properly you need 1 hard dependency:

* BepInEx Valheim

## Mod Compatibility

- OdinsUndercroft: **YES**
- Basement or BasementJVLedition: **YES**
- AllTameableHandH: **YES** 
  - Make sure to add the prefab names to the mask field, like: "Hatchling,Deer" 
  - (If your vertical tolerance is low, you may not be able to transport a flying dragon)

## Changelog
- Version 1.2
  - Fixed error in calculation of vertical distance. Fixed error determine whether boars or wolves are named.
- Version 1.1
  - Update provides additional control over transport mechanic including control over the search radius and ability to limit transport to tamed, named, follow, etc.
- Version 1.0
  - This is the alpha-version of the mod. Feedback and bug reports are appreciated.
This is the alpha-version of the mod. Feedback and bug reports are appreciated.

## Known issues
