# Agent instructions

A [MeitouClient](https://github.com/Meitou-Modding-Ecosystem/MeitouClient) code mod repo in the Meitou-Modding-Ecosystem org. GPL-3.0-or-later.
`CLAUDE.md` is canonical; keep `AGENTS.md` identical. Everything above `## This repo

- Animal Behaviors (`AnimalBehaviors/`, mod id `animal.behaviors`, slug `animal-behaviors`; docs `docs/animal-behaviors.md`). No frameworks:
  `engine` pins the same commit as Husbandry's, its first dependent.
- It is a framework: `IAnimalBehaviorsApi` (animal kinds, sexes, hitching posts). A change to it updates
  `docs/depending-on-animal-behaviors.md` and the tests in `AnimalBehaviors.Tests/`, and bumps the minor version. Each merge into `beta`
  publishes a prerelease dependents lock (`animal-behaviors-beta.yml`).
