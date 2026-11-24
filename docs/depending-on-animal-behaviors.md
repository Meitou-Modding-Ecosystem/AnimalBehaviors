# Depending on Animal Behaviors

A mod that adds animals, gives them work or has its own hitching post builds on Animal Behaviors (mod id `animal.behaviors`). Husbandry does.

## Wiring

- Lock a release in `meitou.lock.json` (slug `animal-behaviors`; a beta prerelease `animal-behaviors-<version>-beta.<commit>` to test, never
  the rolling `animal-behaviors-beta`), reference `AnimalBehaviors.dll` with `Private="false"`, and declare it in `meitou.json`:
  `"dependencies": { "animal.behaviors": ">=0.1.0 <0.2.0" }`.
- Get the API in your `Initialize`: `var animals = c.Dependency<IAnimalBehaviorsApi>("animal.behaviors");`.

## Animals

`animals.For(c).AddAnimal(new AnimalKind("<RACE string id or name>", AnimalSupport.Breeding | AnimalSupport.Training, "note"))` during
`Initialize`. The last registration for a race wins, so a mod loaded later can override. `animals.SupportOf(race)` and
`animals.Supports(cold, AnimalSupport.X)` answer what an animal may do; read the flag your behaviour needs.

## Hitching posts

Mark your BUILDING record in your data phase (`IModDataPhase.EditData`): `Sites.MarkHitch(record, capacity)` (capacity 0: no limit). The
framework reads the marks in its `Initialize`, after every data phase, and offers its one Hitched job (4901) on every marked post, so your post
and Husbandry's stay separate structures with the same job. `Hitches.PostOf(cold)` gives an animal's post and `Hitches.All(world)` every
hitched animal with its post.

## State

`animals.In(world)` (in a serial step of that world) or, in your system factory, `animals.Current` (the framework's factory runs first for each
world) gives `IAnimalState`:

- `Sexes`: `Female(cold)`; write `Sexes.Assigned[serial]` for an animal you made with a set sex (saved by the framework).
- `TryHitch(world, slot, post)`: hitches an animal (a newborn to its mother's post) unless the post is full, which floats a warning and returns
  false. `Capacity(post)` is the post's cap.
- `ImportHitches(...)`: hitches to put back at the next tick, for a mod whose older saves kept them.
