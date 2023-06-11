# Le Bonk - Spice up that Gameplay!
Hi, my name is Paul and alongside two colleagues from university I created the game **Le Bonk** as a project during my master's degree. **Le Bonk** is a couch-versus multiplayer game in which each player controls a mole that can perform a series of attacks in order to knock other moles out of different arenas. I focused on designing and implementing gameplay such as the controller, mechanics and level elements. In this blog I want to explain why we decided to implement such level elements alongside the core mechanics as well as argue, why they are a fun and interesting way to spice up couch-versus multiplayer games.

## The core mechanics
The core mechanics are split up among two scenarios (and can only be used in those scenarios!):
### The mole *has* a hammer in its hand
* Hammer Throw: the mole throws the hammer in the direction it currently looks at, if another mole is in a certain range and field of view, the hammer will be targetting that mole and inflict knockback.
* Leap Attack: the mole leaps forward and creates a shockwave underneath itself, knocking up (stunning) all moles (overground and underground) in a circular shape
### The mole *does not* have a hammer in its hand
* Burrow: the mole goes underground, gains a slight increase in movement speed and cannot be targetted by the *Hammer Throw*; this mechanic is either cancelled by a time-limit, by pressing the same button again or by using the *Dash*
* Dash: this mechanic is only available during *Burrow*; when any other mole that is overground is in a certain range and fov (similar to *Hammer Throw*) the mole's dash will be directly targetting that mole and inflicts knockback; given that the target currently has a hammer equipped, it will be stolen as a bonus 
