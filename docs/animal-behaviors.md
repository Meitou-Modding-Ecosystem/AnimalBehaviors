# Animal Behaviors

The framework for animal mods: the list of supported animals, animal sexes, and the **Hitched** job that every mod's hitching posts share.
It adds no buildings or records: Husbandry's Hitching Post (and any other mod's post) offers the job. How a mod uses it:
[depending-on-animal-behaviors.md](depending-on-animal-behaviors.md).

## Hitching

- **Job**: task type 4901 "Hitched" (README "Numbers"), **Hitch here** on every BUILDING a mod marked as a hitching post, for animals. The
  engine lets only the mod that claims a job type offer it, so the framework owns the one job and adds every marked post to its offer. The
  posts are separate structures; an animal is hitched to the one it was sent to. A post that is gone or unfinished lets its animals go.
- **Other factions' posts**: the job is offered on any faction's finished post (the engine's `ModJobOffer.Owners = Any`). With no dependent's
  rule such a post is free. A dependent's hitch rule is asked when an animal is hitched to another faction's post and at each new game day it
  stays; it may charge, change relations or report a crime itself, or refuse, which unhitches the animal with the rule's text floating over it.
- **Pen**: a hitched animal standing idle more than **Pen radius** (setting, default 100 game units, about 10 m) from its post walks back to
  within half the radius; one inside strolls to another spot now and then (setting **Grazing**). Anything else it is doing comes first, so it
  leaves the pen only for a food run to a feeder or food on the ground, a fight, a job of its jobs list (Follow or Bodyguard: remove it to keep
  the animal home), or a player's order. Checked once a game second per animal.
- **Room**: a post's record may cap its animals (int field `animal behaviors hitch capacity`; Husbandry's Hitching Post holds 6). Each check
  a post over its cap keeps the animals it held at the last check, then the lowest slots, and lets the rest go with "This post is full (6
  animals)" floating over each. A dependent hitching an animal itself (a newborn) asks first (`TryHitch`), which refuses a full post with
  the same warning.
- **Saving**: the engine saves no mod jobs, so the hitches (portrait serial and post) are kept in the sidecar and put back at the first tick
  after a load. Sidecar v2 adds the day the rules were last asked and the animals let stay at other factions' posts, so a load asks no rule
  again.

## Sexes

The game gives animals no sex (no ANIMAL_CHARACTER has `female chance`; Meitou draws every animal male, engine docs/game/animals.md "Breeding
and eggs"). The framework decides it half and half from the character's portrait serial, saved with the character, with the random purpose
Husbandry used before the framework existed, so every animal keeps its sex. A mod that makes an animal with a set sex writes it to
`Sexes.Assigned` (kept by serial in the sidecar). The character panel shows it on every animal's Age line ("Adult (female)", and a "Sex:" row
in its tooltip; a Harmony postfix on the engine's `HudTips.Lines`), and the animal buying window's "Gender:" line shows it too.

## Supported animals

Any of the player's animals can be hitched, other mods' included (the engine's `SquadRegistry.IsAnimal`). What else an animal may do depends
on its RACE; each flag is read by the mod that gives the behaviour (Husbandry: breeding, training, capsules):

| Support | What it allows |
| --- | --- |
| Breeding | hitched adult pairs of the race breed |
| Training | it learns skills |
| Taming | a wild one, knocked out, can be caught |
| Summoning | one of the player's own can be stored and summoned again |

A race's support is the last registration naming its string id, else the last naming its name (without case), else:

| Race | Support | Why |
| --- | --- | --- |
| Leviathan (built in) | breeding, training | too big to catch or store |
| Any robot race (`is robot`) | training | it neither breeds nor fits a capsule |
| Any other animal race | everything | |

The full list for the loaded mods is debug row **Animal Behaviors: supported animals**, also logged when the first world is built.

## Settings

**Pen radius** (30 to 600, default 100) and **Grazing** (default on), under the mod's row on the Settings screen.

## Debug rows (F10)

- **Animal Behaviors: hitched animals**: each hitched animal with its race, sex, age, hunger and post.
- **Animal Behaviors: supported animals**: every race of the loaded data and what it supports.

## Save data

Sidecar `animal-behaviors`, version 1: the hitches (portrait serial, post site id), then the set sexes (serial, female). Husbandry 0.7 and
earlier kept the hitches and sexes in its own sidecar; Husbandry hands them over when it loads such a save.
