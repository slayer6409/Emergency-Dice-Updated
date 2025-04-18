-	**1.9.25**
    - Another small bug fix

- 	**1.9.24**
    - Potential fix for a few poltergeist issues
    - And for an error that shouldn't be showing up :D
    - >:D 

- 	**1.9.23**
    - Removed April fools stuff
    - Moved some logging to debug log so not to spam now that **most** of the problems are resolved (I believe)
    - Fixed Poltergeist Compat rolling an effect for everyone in the lobby with only 1 use

-	**1.9.22**
    - Fix for dice not rolling for clients
      - Apparently the dice was despawning on the host before it could roll on clients
      - Could still happen if the client is particually laggy

-	**1.9.21**
    - Fix for a NRE with a null spawnable map object prefab

-	**1.9.20 - A totally _normal, stable, balanced_ "update".**
    - "Fixed" the floating dice issue by leveling the playing field
    - Haha things go brrrrr
    - Do these look a bit off to you?
    - What does this thing do again???
    - Added 10% more bugs
    - The dice have been feeling _lucky_ lately
    - Wait, I added a real thing? …Do ghosts count?
    - Have fun >:D

-   **1.9.20 - Actual patch notes**
    - Disabled a few things to see if those were the causes of issues
    - Possibly fixed dice effect not showing up when rolled
    
-	**1.9.19**
    - Very Big Refactor for handling clients correctly especially for late joining and leaving
    - If something breaks please lmk
    - I am 100% sure at least 1 event will be overlooked and possibly error cuz I had to change a TON of events
    - If I somehow did this entire thing at 5 am without errors I will be amazed lol

-	**1.9.18**
    - Reverted some code
    - Added some changes to check for certain errors
    - Hopefully all of this was to fix the recent errors
      - It still will say that the networker is Despawned/Destroyed on leaving 
      - I left that in for now just in case it isn't fixed
      - I will remove it when the problem is fixed
    - Also fixed Paparazzi not moving (woops my bad)

-	**1.9.17**
    - Potential Fixes for networker going poof
    - and for dice not rolling sometimes

-	**1.9.16**
    - Added logging to when it is destroyed/despawned to print the stacktrace so I know what is causing it
      - It will log it when you return to the menu too though
      - If you run into errors where dice don't roll or any other errors, please send the logs either in the Emergency Dice Updated thread in the Lethal modding Discord or create a github issue
      - Thanks
      - Networker pls no go poof

-	**1.9.15**
    - Added a copyright free mode for freebird based events since it is over 10 seconds (Spazzmatica Polka) (in Misc)
    - Added a way to change the volume as well
    - Added a toggle for brutal events to show up in chat seperate from normal rolls

-	**1.9.14**
    - Fixed Amazon Shipping and Airsupply not spawning mimics randomly
    - Added Beartraps, Crates, and Boomtraps to the spawnmenu of DebugMenu
    - Haha Laser go brrrr

-	**1.9.13**
    - Even more logging (debug) right before a effect is rolled to determine what roll is causing dice to error
    - Added new events >:D
    - Grab object patch fix for debug
    - Fixed Moving traps being silly and staying at the doors
      - They can now do scarier things though >:D 
      - FREEBIRDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD
    - Other small changes here and there

-	**1.9.12**
    - Added additional logging just in case it fails to roll
    - Fixed ship turret erroring when wide ship or mel's 2 story ship is installed
    - Fixed burger flippers spawning in weird positions (sometimes)

- 	**1.9.11**
    - Actually made Weather Registery a soft dependency
    - Had to make a api for adding things to the Debug menu because of that lol
    - Hopefully everything works well
    - Converted Dice Twitch Integration Mod to be implemented via a soft dependency on TwitchChatAPI (and enabled via config) 

- 	**1.9.10**
    - Added a dependency I forgor about 
    - Only temporary until I make a actual soft compat for it (working on it rn)

- 	**1.9.9**
    - Fixed Hoarding bugs and other Enemies Spam rolling dice
    - Fixed a NRE with the gambler
    - Adjusted the offsets of the gambler (hopefully)
    - Some effects now properly trigger for everyone now
    - Fixed the ammo of the shotgun effect not spawning properly
    - Fixed invisible items when cull factory is installed
      - If any item is invisible when they spawn in, let me know on [This github thread](https://github.com/slayer6409/Emergency-Dice-Updated/issues/15) 
	- Added a few new effects
	- There was a lot changed so if something broke, lmk on my github or on the Discord Thread

- 	**1.9.8**
    - Fix for NRE with Lobby Control Experimental
    - Fix for NRE with Debug menu
    - Fix for Zortin2 spawning only on the host
    - Fix for some New Debug menu stuff
    - New Select Effect Menu (If you have NewDebugMenu Checked)
    - Select an Effect from Saint and Rolling now are sorted correctly

- 	**1.9.7**
    - Fix for Bad Romance Causing Errors for Clients
	- Fix for Teleports in Debug Menu not working in LAN play
    - I forgot to mention in a previous changelog Enum Config values are now strings so they won't reset

-	**1.9.6**
    - Yippee!!!
      - Surely nothing will go wrong :D 
    - Search Bar for the New Debug Menu!

- 	**1.9.5**
    - Fixed Delay and Brutal being able to do dice effects on company moons
    - Bad Romance Sound Effect :D

- 	**1.9.4**
    - 4 new things :D
    - Tested these a bit more so hopefully they are fine

- 	**1.9.3**
    - Apparently it broke things
    - Fixed
     
-	**1.9.2**
    - Secrets
      - THIS SHOULDN'T BREAK ANYTHING
      - If it does #BlameGlitch

- 	**1.9.1**
    - Fixed a small issue with the new Debug menu

- 	**1.9.0**
    - New Debug Menu (default off just in case) :D
    - Fixed frame issues (hopefully)
    - **Hopefully no issues this time**

- 	**1.8.5**
    - Fixed some issues with the debug menu

-   **1.8.4**
    - Adjusted values for Zombie Apocalypse, Bug Plague, and Outside Bugs to be at a less laggy value
    - Added a non-deductible breakfast and more events
    - Fixed some stuff

-	**1.8.3**
    - Added a few new cool events (at least I think so lol)
    - Optimized some weird code
      - Not that much though
    - Fixed some issues (still don't know why gambler is stupid)
    - Removed size options due to them breaking a ton of things for some reason
      - Might add them in a separate addon mod if people want them
	- Fixed and brought back Where did my friends go!

-	**1.8.2**
    - Fixed Bald not being Bald for clients

-   **1.8.1**
    - Gambler is gonna remain broken idk
      - Rebuilding it in unity didn't work so idk what to do lol 
    - Added a few new events again :D
    - Fixed some more issues especially with moving things

-	**1.8.0**
    - Most Likely fixed Gambler and if it works I will do the same for Emergency Die
    - Added a few new events
    - Changes to Moving Objects to be better (Thanks Xu)
    - Changes to Custom Events to actually work now
    - Possibly fixed a few errors with other events (purge, teleports, and others)
    - Changes to the Debug menu (Favoriting Changes and Keeping Open on Select (Config option))
    - No _**Secret**_ thing yet **_Soon tm_**
    - **_Fixed Sacrificer_** 
    - Made Glitch Balder Somehow :D

-   **1.7.4**
    - Changed a few things internally
    - Changed the debug menu spawning for when you are dead
    - Few new events
    - Fixed some bugs
    - Lovers now properly reset on orbit
    - Added a temporary option to have 2 random players become lovers on land
    - - It is temporary while I make the next Major update for this mod

-   **1.7.3**
    - Changed revive to just be the new better method
    - Added Dependency for LethalCompany InputUtils to set the Debug Menu Keybind easier
    - Fixed some events
    - Fixed some issues with things
    - New Config option for if Dice Explode
    - Made the crab on the Surfaced Die actually on top now
    - You can now add multiple objects to events, such as "Spring,Flowerman" for an enemy event that spawns both of them
    - - It works for enemies, traps, and items! 
    - Changed README
    - New _Friendly_ event
    - Fixed Emergency Die and Sacrificer sometimes erroring on certain maps****

-	**1.7.2**
	- Reverted back to 1.7.0

-	**1.7.1**
    - I Apparently broke things
    - So this is irrelevant

-	**1.7.0**
    - Added a better way to move traps
    - Added more events

-	**1.6.6**
    - Fixed Meteors
    - Added more to blame glitch (from glitch)
    - Fixed The Emergency Dice not generating its config
    - Testing out a new Revive Method (can be enabled in the config settings)

-	**1.6.5**
    - Fixed Same scrap different dice

-	**1.6.4**
    - Possible fix for unchained
    - Other Fixes with spawning in objects
    - New Events 

-   **1.6.3**
    - Fixed some issues
    - Added a Alarm curse new sfx option

-	**1.6.2**
    - Added a bunch of new effects (mostly surfaced for the new die)
    - Fixed Select Effect menu favorites to actually be sorted now

-   **1.6.1**
    - Networker is now public to allow for easier dice effect making
    - IEBerthas >:D

-	**1.6.0**
    - Added a surfaced die! Thank you so much Rodrigo for the model!
      - The surfaced die, only has surfaced events unless you don't have surfaced installed which it just goes to random 
    - Added a new Surfaced Event 

-	**1.5.24**
    - Changed some debug menu stuff
    - Blame Glitch is worse thanks to glitch

-	**1.5.23**
    - Disabled Where did my friends go in an attempt to fix stuff
    - Fixed Ghost codes and Hoarding bugs doing wierd things with the dice
    - Added an API so mod developers can add their own dice effects to their own mods!
    - Possibly Fixed some other issues

-	**1.5.22**
    - Screaming in the void hoping it will fix things

-   **1.5.21**
    - Fixed some things
    - IDK what else I did tbh 

-	**1.5.20**
    - Possibly Fixed a ton of issues
      - Glitch I swear if you break things...
        - I will have to fix them I guess...
 
-	**1.5.19**
	- Maybe hopefully possibly fixed gambler?!?
	- Bomb Collars Event(s)???
	- Code Rebirth Event(s)???
	- \>:D
	
-	**1.5.18**
	- Added 2 Diversity Effects for the new Diversity Update
	- - Chaos if you want you can choose the names/tooltips for the effects (just message me on discord)
	- Debug Menu Changes
		
-	**1.5.17**
	- Accidentally left the DebugMode on when Patching

-	**1.5.16**
	- Possible fix for Surprise Egg and Egg Boots
	- Actually fixed Jaws Sound this time
	- Scary
	- \>:D
	
-	**1.5.15**
	- Possible fix for burger flippers
	- Flinger ***Should*** now work correctly for all players
	- Tulip Trapeze spawns on a random player now 
	- Debug menu now has Spawn Shop Items
	- Added Beyblade mode for Flinger
	- Chronos(roll 3) and Sacrificer (roll 6) can now spawned mixed effects
	- Apparently Little Company checks don't work for some reason, so there is a config to turn off all sized based things with one click
	 
-	**1.5.14**
	- Added Little Company checks to disable size based things for compatibility
	- Added Where did my friends go
	- Fixed scrolling with the selection menu
	- Fixed Mine and a Hard Place to be not on the host only
	- I might remove gambler and re-make it from scratch (with a potentially new name) if this doesn't work
	- Fixed audio of Can I pet that Dog and Jaws

-	**1.5.13**
	- Added Spicy Nuggies Event (suggested by Lizzie)
	- Added the ability to favorite things in the effect/debug menu by Right-Clicking them
	- Debug menu can now spawn items, monsters, and traps! Items and Traps spawn on you, and monsters spawn inside if you are inside, outside if you are outside. 
	- - It is off by default since it might act weird at first until I know it works fine (It is 5am when I made this lol) (This was mainly so I can test things easier lol)
	- - Also Apparently spawning enemies outside (that aren't meant there) might not have AI correctly without Starlancer AIFix or something similar (same with inside dog)
	- Fixed Burger Flippers and Flinger (Probably)
	- Fixed some things (I don't remember lol)
	- Dev Stuff pt 2
	
-	**1.5.12**
	- Fix for spawning items

-	**1.5.11**
	- Added a favorite system to the select/debug menu
	- Possible fix for things

-	**1.5.10**
	- Possible fix for gambler again again again again
	- Added Event Stuffing for Days
	- Added Event Size Switcher
	- Added Event Burger Flippers
	- Added Event Flinger
	- Added Event Between a Mine and a Hard Place
	- Made Drunk only effect the roller like it was supposed to
	- Fixed Sound effects
	- Fixed Size Difference not fixing for other people
	- Dev stuff
	
-	**1.5.9**
	- Possible fix for egg boots not working on clients
	- Possible fix for gambler again
	- - Still no idea why this is happening

-	**1.5.8**
	- Possible Fix for Revive Removing UI and maybe a fix for flying dice
	- Added Event Blame Glitch (Off by Default xD)
	- Added a new Debug menu option in the config
	- Added Experimental Custom Trap Event Support
	- - It's Experimental since they sometimes spawn in weird places (Working on fixing it)
 
-   **1.5.7**
	- Tulip Trapeze (Thanks al3m33da for the Idea)
	
-   **1.5.6**
	- Added Egg Boots Effect (Thanks to MelanieMelicious for the Idea)
	- Added in Custom Items Events and Custom Enemies Events Support (View the Readme for how to do them)
	
-   **1.5.5**
	- Removed Speedy Boomba since it was causing issues (It will be back later)
	- Fixed Surprise Egg/Surprise Flash for Clients

-	**1.5.4**
	- New Events: Speedy Boomba(Lethal Things), Crates (CodeRebirth), Tornado (CodeRebirth), Egg Fountain, Flashbang Fountain, Can I pet that Dawg
	
-	**1.5.3**
	- Possibly fixed floating dice
	- New Events 21 Gun Salute, The Shining, Merry Christmas (Thanks A Glitched NPC for the Ideas)
	- New Events Eggs, SmolTakey (Requires Takey Plush mod), and Same Scrap Different Dice
	- Added a config option to change the Emergency Die Price
	- Added a config option to add the Emergency Die as a scrap item
	- Added a config option for hypershake to have a timer
	
-	**1.5.2**
	- Removed the FixRenderer since it ***shouldn't*** be needed anymore
	- Disabled Terminal Lockout until I can fix it
	
-	**1.5.1**
	- Fixed the Changelog and Readme
	- Fixed item weight not being updated properly with Heavy and Light Burden

-	**1.5.0**
	- Added 6 New Dice Rolls with Surfaced (Whoops name error)
	- Added a New Dice Roll with LCTarotCards
	- Fixed some other bugs (probably)
	
-	**1.4.10**
	- I hope pt 3
	
-	**1.4.9**
	- I hope pt 2
	- Fixed healing removing health if you were over 100

-	**1.4.8**
	- Fixed Null Reference error with EntranceTeleportPatch (I hope)
	 
-	**1.4.7**
	- Possible fix for dice not rendering when going through the facility doors
	- Added a new config option for displaying the results of dice rolls in chat
	- Possible fix for Item Duplicator errors
	
-	**1.4.6**
	- Possible Fix for not spawning scrap from rusty again (hopefully it works this time lol)

-	**1.4.5**
	- Possible Fix for not spawning scrap
	- Fixed Display Results not showing up if ALL was selected
	- Added new random tooltips for dice effects
	
-	**1.4.4**
	- Possible Fix for Shotgun not spawning (in a kind of weird way)

-	**1.4.3**
	- Made the SFX of Emergency Meeting not so loud
	- Fixed the sprite for Emergency Meeting

-	**1.4.2**
	- Added Emergency Meeting SFX and Sprite
	
-	**1.4.1**
	- Changed how the visor is toggled to account for other mods
	
-	**1.4.0**
	- Changed how the config variables were handled
	- Added a compatability for LethalConfig
	- Possible Fix for shotgun not appearing for everyone
	- Possible better config syncing (It worked fine in lan mode on 2 pcs, so hopefully there isn't any trouble)

-	**1.3.7**
	- Added new events: Invisible Enemies, Emergency Meeting, Terminal Lockout
	- Possible fix for purge not killing enemies
	- Added a config option for Become Small that allows different ways to revert 
	 
-	**1.3.6**
	- Fixed size not reverting on leave
	 
-	**1.3.5**
	- Added new events: Meteors and Become Small
	
-	**1.3.4**
	- Added a new event Barbers
	- Fixed a line of code that accidentally got deleted (whoops)

-   **1.3.3**
	- Fixed a blank config issue with lethal performance 

-   **1.3.2**
	- Fixed Heavy and Light burden to not bug your weight/movement
	- Added config options to make NeckSpin and NeckBreak less Awful

-   **1.3.1**
	- Fixed NeckSpin to actually work not based on framerate (I forgot to multiply by Time.DeltaTime xD)

-   **1.3.0**
	- Added a bunch of new Rolls and Rolls with other mods, LCOffice and LethalMon
	- Possible fix for hud disappearing when revived pt.2 
	- Fixed Item Dupe event not working right
	- Fixed/Replaced a lot of code that disappeared for some reason
	- Optimized a bit of code (especially with spawning in Teleporter traps cuz I am still learning lol)
	
-   **1.2.11**
	- Possibly fixed Shotgun not spawning for people
	- Added a new effect Headspin
	- Possible fix for hud disappearing when revived
	
-   **1.2.10**
	- Changed Readme to add the original creator's plugin GitHub
	
-   **1.2.9**
	- Added an Invisible Teleporter Trap Event
	
-   **1.2.8**
	- Fixes a few things
	- Added a config option to change how rolls are shown to you
	
-   **1.2.7**
	- Added 8 new Dice Rolls
	- Added integration with Lethal Things for Teleporter Traps
	
-   **1.2.5**
	- Turns out I am a bit dumb and didn't know how netcode worked in regard to modding.
	- Added back the random dice spin time
	
-   **1.2.4**
	- Removed the config option for random dice spin time in hopes that it was that that broke the dice networking
	
-   **1.2.3**
	- Possible fix for dice not working for clients
	- Switched the GUID back so Brutal Company Minus Events can work again
	
-   **1.2.2**
	- Added a new event "Random Items from Shop" as a great variant that gives 2-4 items
	- Moved Random Item from shop to Good instead of great
	- Alarm Text :D
	
-   **1.2.1**
    - Some bugfixes
	- Added new Event "Random Item from Shop"
		- Might make it multiple events where great variant gives 2-4 items and good variant gives 1 
	
-   **1.2.0**
    - I have temporarily ported this to v55 with a few changes 
	- Neck Break can go further
	- Config options for changing the force of Hypershake
	- Config option for random dice spin time
	- Config option for use of Gambler and Chronos outside
	- Maybe something else, I honestly don't remember, it is 8am and I need sleep
	
-   **1.1.8**
    - As of this update, check out my GitHub for any version changes: https://github.com/Theronguard/Emergency-Dice/
	
-   **1.1.7**

    -   God help me. Made new effect in 1.1.5 and forgot to add it to the die pool, so it was essentially useless.

-   **1.1.6**

    -   Forgot to turn off the debug mode again

-   **1.1.5**

    -   New negative effects
    -	New positive effect (spawns a shotgun)
    -	bugfixes?
	
-   **1.1.4**

    -   New effect
    -	Restored an 
	
-   **1.1.3**

    -   New negative effects
    -	Bug fixes

-   **1.1.2**

    -   New effects
    -	Bug fixes

-   **1.1.0**

    -   Added new effects
    -	Added new functionality to the Emergency Die (rolling 6 now teleports you and your crewmates)
    -	Bug Fixes
	
-   **1.0.0**

    -   Released