# Emergency Dice

This mod adds new items (equipment and scrap)

## Emergency Die

Updated version of: https://thunderstore.io/c/lethal-company/p/Theronguard/Emergency_Dice/
Their Github Repo: https://github.com/Theronguard/Emergency-Dice

Check out that mod if you don't want my changes! 

## Documentation

To find all Dice and Dice effects to see what they do: https://dice.slayer.games/

## API

You can now add your own dice effects in your own mods
there is an example mod on how to do that here: https://github.com/slayer6409/Dice-Addon


### Config
Be sure to check out the plugin's config file.
It is called EmergencyDice.cfg and it should
be generated when you run the game with the mod for the first time.

You can modify which effects you want or not, or change some other settings.

The [Allowed Effects] tab in the config syncs with all the clients,
meaning that only server needs to disable/enable effects.

Spawn rates are also serverside only, so no need to worry about having everyone
syncing up their values with the host.

### Emergency die shop alias
Some mods (like Lethal Things mod) might use conflicting item names in the shop.
To solve this I added few aliases for the emergency die in the shop.

Try these aliases in the shop:
* emergencydie
* dice
* die
* edie

### Chat commands
I included some chat commands in the mod, mainly for myself to ease debugging.
If you want to use them you have to set

"Allow chat commands = true"

in the config.

To spawn dice use:
* ~edice dice num

where num is an id of the die.
ID's start from 1.

If you want to force an effect, use
* ~edice effect effectName
or
* ~edice menu

### Custom Events
I have made Custom Events as a config option 
so if you wanted enemies inside or outside to spawn 
or if you want a bunch of a specific items to spawn you can now do that.

Just set the amount of custom events in the config, launch the game and close it to generate the configs.
Then just input the name of the enemy or item depending on what one you do. 

To get the item name or enemy name, turn on DebugLogging and for items pick it up, and for enemies launch to a planet, and it will list out every enemy name. 


### Special thanks

Thanks to the Original creator, [Theronguard](https://github.com/Theronguard/Emergency-Dice), who gave me permission to work on and upload this mod!

Thanks to:

[QWERTYRodrigo](https://www.youtube.com/watch?v=I0VF90vxZT8), who made some models for the mod (surfaced die and another other future thing)!

[Xu Xiaolan](https://github.com/XuuXiaolan) for so much help with the code! 

[Glitch](https://www.twitch.tv/a_glitched_npc), [Lizzie](https://www.twitch.tv/lizziegirl0099), [Lunxara](https://www.twitch.tv/lunxara) and so many more amazing people that the list is too long to put, for testing and bug finding! 

Music by Kevin MacLeod Spazzmatica Polka - https://incompetech.com/

Thanks to everyone who participates on github -
for the suggestions, and bug reporting!

Thank you all for rolling the dice!

### All players need this mod
