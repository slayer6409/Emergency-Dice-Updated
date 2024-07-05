# Emergency Dice

This mod adds new items (equipment and scrap)

## Emergency Die

![](https://i.imgur.com/gGdBSz0.png)

This is an item you can buy from the store.
It is a one time use only die, which teleports you to the ship with all your items.
But, not always.

* Rolling 6 will return you to the ship with all the crewmates standing near you (they need to be very close).
* Rolling 4 or 5 will return you to the ship.
* Rolling 3 will cause a mixed effect.
* Rolling 2 will cause something bad to happen.
* You don't want to roll a 1.

## The Gambler

![](https://i.imgur.com/J6biRWU.png)

A new scrap item with rolling outcomes similar to the Emergency die,
but with a larger pool of postivie effects and a bigger chance for a bad outcome.

* Rolling 6 will cause a Great effect.
* Rolling 5 will cause a Good effect.
* Rolling 4 will cause either a Good or a Mixed effect.
* Rolling 3 will cause a Bad effect.
* Rolling 2 will cause either a Bad or an Awful effect.
* Rolling 1 will cause an Awful effect.

You can either sell it, or use it.

## Chronos

![](https://i.imgur.com/wMPUsZB.png)

Similar to The Gambler. Has a higher chance of getting a great effect,
but the outcomes change when the time passes. As the night
comes, the chances to roll a bad/awful effect increase.

It's probably better to use it earlier in the day, but this can
make your whole day troublesome.

* Rolling 6 will cause a Great effect.
* Rolling 5 will cause a Good or a Great effect.
* Rolling 4 will cause either a Bad, Good or a Great effect.
* Rolling 3 will cause a Bad effect.
* Rolling 2 will cause either a Bad or an Awful effect.
* Rolling 1 will cause an Awful effect.

You can either sell it, or use it.

## The Sacrificer

![](https://i.imgur.com/7qePubu.png)

Guarantees a return to the ship, and a bad/awful effect.

* Rolling 6 will cause a Bad effect.
* Rolling 5 will cause a Bad effect.
* Rolling 4 will cause a Bad effect.
* Rolling 3 will cause a Bad or an Awful effect.
* Rolling 2 will cause an Awful effect.
* Rolling 1 will cause two Awful effects.

## The Saint

![](https://i.imgur.com/g5gaoUH.png)

This one will never roll a bad or an awful effect.
It's the rarest die in this mod, so don't expect to see it a lot.

* Rolling 6 will show you a menu from which you can select any Effect.
* Rolling 5 will cause a Great effect.
* Rolling 4 will cause a Good effect.
* Rolling 3 will cause a Good effect.
* Rolling 2 will cause a Good effect.
* Rolling 1 will waste the die.

## Rusty

![](https://i.imgur.com/SjLWGEx.png)

Spawns scrap. Only scrap. Higher rolls mean more scrap.
Has negative outcomes.

* Rolling 6 will spawn 7-8 scrap.
* Rolling 5 will spawn 5-6 scrap
* Rolling 4 will spawn 3-4 scrap.
* Rolling 3 will spawn 1-2 scrap.
* Rolling 2 will cause a Bad effect.
* Rolling 1 will cause an Awful effect

## Effects

### Great

* Gives you Pathfinding Blobs - a new item which shows everyone the way to the main entrance when used
* Increases company's item selling rate for the current quota
* Gives everyone infinite stamina
* Makes all the living enemies explode and die
* Teleports you and anyone near you to the ship with items
* Revives every dead player
* Spawns about 3-9 (random) scrap items beneath you with a lower weight.
* Gives you a shotgun with ammo
* Gives you the ability to fly with your sprint key
* Allows you to pick any effect (both good and bad ones)

### Good

* Makes all flashlights brighter
* Heals all alive players and restores their batteries in every item
* Gives you infinite stamina
* Teleports you to the ship with your items

### Mixed

* Swaps places with another player

### Bad

* Makes you emit an annoying sound periodically
* Spawns spiders and makes them faster
* Spawns an armageddon outside
* Spawns a lot of bugs inside
* Makes the ship door malfunction
* Creates a lot of fake fire exits
* Blocks all fire exits
* Closes all open doors and locks them
* Jumpscares you (can be changed in the settings to be mild)
* Spawns a lot of mines inside
* Changes your speech, and makes it harder to communicate
* Teleports you inside
* Turns off all lights permanently
* Makes you periodcally shake a lot
* Makes the ship lever moving

### Awful

* Spawns a lot of bee hives outside
* Detonates a random player
* Spawns a jester in a popped state, and pops all already existing jesters.
* Spawns landmines and makes them move
* Spawns a coilhead outside
* Causes coilheads to sometimes ignore your stare, which allows them to move while being watched.
* Creates turrets near the ship
* Spawns invisible mines inside
* Creates a lot of turrets inside which shoot instantly
* Spawns a lot of worms outside
* Spawns a lot of zombies inside
* Teleports you to the ship, but leaves a zombie in your place
* Spawns a lot of mines outside
* Breaks a neck of a random player

## More to come!

### Config
Be sure to check out the plugin's config file.
It is called Emergency Dice.cfg and it should
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


### Special thanks
Thanks to everyone who participates on github -
for the suggestions, and bug reporting!

### All players need this mod
