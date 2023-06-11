# Le Bonk - Spice up that Gameplay!
Hi, my name is Paul and alongside two colleagues from university I created the game **Le Bonk** as a project during my master's degree. **Le Bonk** is a couch-versus multiplayer game in which each player controls a mole that can perform a series of attacks in order to knock other moles out of different arenas. I focused on designing and implementing gameplay such as the controller, mechanics and level elements. In this blog I want to explain why we decided to implement such level elements alongside the core mechanics as well as argue, why they are a fun and interesting way to spice up couch-versus multiplayer games.

![](https://github.com/paulp1412/lebonk-blog/blob/main/img/splash.png)

## The core mechanics
The core mechanics are split up among two scenarios (and can only be used in those scenarios!):
### The mole *has* a hammer in its hand
* Hammer Throw: the mole throws the hammer in the direction it currently looks at, if another mole is in a certain range and field of view, the hammer will be targetting that mole and inflict knockback.
* <details>
    <summary>Gif</summary>
      ![](https://github.com/paulp1412/lebonk-blog/blob/main/gif/hammer_throw.gif)
  </details>
* Leap Attack: the mole leaps forward and creates a shockwave underneath itself, knocking up (stunning) all moles (overground and underground) in a circular shape
### The mole *does not* have a hammer in its hand
* Burrow: the mole goes underground, gains a slight increase in movement speed and cannot be targetted by the *Hammer Throw*; this mechanic is either cancelled by a time-limit, by pressing the same button again or by using the *Dash*
* Dash: this mechanic is only available during *Burrow*; when any other mole that is overground is in a certain range and fov (similar to *Hammer Throw*) the mole's dash will be directly targetting that mole and inflicts knockback; given that the target currently has a hammer equipped, it will be stolen as a bonus 

Those individual mechanics create the possiblity to chain them together in order to do some neat combos and bonk the other moles out of the arena!
Popular combos include:
* Combo 1: hammer equipped > Leap Attack > Hammer Throw > Burrow > Dash
* Combo 2: no hammer equipped > Burrow > Dash (on target with hammer!) > Hammer Throw > Burrow > Dash
* Combo 3: hammer equipped > Hammer Throw > Combo 2

Although those mechanics are really fun to use and also feel quiet satisfieing if used successfully on your opponents, something was still missing. Some maps had the issue that rounds took way too long to be played out due to camper spots or obstacles that prevent the players from falling down the edge of the arena too quickly. Some arenas also made use of so-called *Death Objects* that - how the name already reveals - kill the player upon contact. However, playing numerous *Death Objects* would take away the fun of the already decent gameplay so we introduced *Third Party Events* that are triggered at random moments during a round in order to make the arena more messi and add a factor of randomness to the current gameplay.

## Adding Level Elements (Third-Party Events)
It was important for us - especially me - that level elements blend in with the theme of each arena, so we created individual ones for each theme:
* Wild West: Minecarts that inflict a horrendous amount of knockback
* Winter: Ice Spikes that also inflict knockback, but not as strong as *Minecarts* (credits to Sandro Figo, who polished the Ice Spike in terms of model, sfx, vfx etc. during his transfer-project for **Le Bonk**)
* Summer: A Hawk that flies across the arena picking up a mole that is underneath him and dropping above the death zone of the arena (which is a river the mole is not supposed to fall in to)
* Graveyard: Zombie Hands that knock the mole towards the next edge where it can fall down (credits to Iris Trummer, who implemented this Level Element during her transfer-project for **Le Bonk**)
* Japan: Sky Lanterns that fall from the sky and create a fire on the ground (*Death Object*) for a short amount of time

After we introduced those Third-Party Events, rounds seemed to end quicker on average and players also seemed to enjoy the increasing level of randomness as an additional danger-factor while maneuvering their moles through each arena.

## Code
In case you are interested in the implementation, check out the 'scripts' folder of this repository and take a look at the base ThirdPartyEvent.cs as well as the IceSpikeSpawner.cs as an example. Used assets: **UnityAtoms**, **OdinInspector**
