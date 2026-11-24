# Animal Behaviors

The framework for animal mods in [MeitouClient](https://github.com/Meitou-Modding-Ecosystem/MeitouClient): the list of supported animals
that any mod can add to, animal sexes, and the Hitched job that every mod's hitching posts share. Mod id `animal.behaviors`, no dependencies;
what it does: [docs/animal-behaviors.md](docs/animal-behaviors.md). Husbandry depends on it. GPL-3.0-or-later.

- Adding animals or your own hitching post from your mod: [docs/depending-on-animal-behaviors.md](docs/depending-on-animal-behaviors.md).

- Build and run locally with Visual Studio, VS Code or the terminal (this repo's engine or the installed launcher, deployed to the game): [docs/play.md](docs/play.md).
- Setting up the repos on your machine: [docs/local-development.md](docs/local-development.md).
- Instructing agents on a change that spans repos: [docs/agents-across-repos.md](docs/agents-across-repos.md).
- Agent rules for the engine pin, versions, CI and releases: [CLAUDE.md](CLAUDE.md) (same as `AGENTS.md`); step-by-step procedures:
  `.claude/skills/meitou-engine-release`.
- Checking a built mod against the newest Meitou release (CI does it on every PR): [docs/refcheck.md](docs/refcheck.md).
- Publishing the mod to the Steam Workshop for the first time, then on every release: [docs/steam-workshop.md](docs/steam-workshop.md).
- Turning `.glb` models (Meshy, Hi3D, Blender) into buildable structures, no FCS needed: [docs/structures.md](docs/structures.md);
  getting a generated model's polycount, size and textures right first: [docs/model-prep.md](docs/model-prep.md).

## Numbers

Task types and dialogue codes are shared by every loaded mod; Animal Behaviors claims only these, inside its block
([mod number blocks](https://github.com/TheIvoryDisaster/GutenStructure/blob/beta/docs/numbers.md)). `NumbersTests` checks the list.

Block 4900-4999

| Number | Pool | Use |
| --- | --- | --- |
| 4901 | Task type | Job: Hitched (offered on every building marked as a hitching post, animals) |

## Layout

- `engine/`: MeitouClient as a git submodule, pinned to the engine commit the mod builds against. `git pull` does not move it: after
  every pull run `powershell -ExecutionPolicy Bypass -File tools\Update-Local.ps1 -Clean -Build`
  ([keep clones current](docs/local-development.md#keep-clones-current)).
- `AnimalBehaviors/`: the mod's project and `meitou.json`. It adds no records (`AnimalBehaviors.mod` is empty). `AnimalBehaviors.Tests/`: its
  tests (no game install needed).
- `meitou.lock.json`: the framework releases the mods build against
  ([framework lock](https://github.com/TheIvoryDisaster/GutenStructure/blob/beta/docs/framework-lock.md)); the Framework update workflow moves it
  to new releases and merges when CI passes ([framework update](https://github.com/TheIvoryDisaster/GutenStructure/blob/beta/docs/framework-update.md)).
- `Play/`: one-click build, deploy and run.
- `tools/`: `Update-Local.ps1`, the local pull and engine-pin check ([keep clones current](docs/local-development.md#keep-clones-current));
  `refcheck.sh` and `RefCheck/`, the engine-reference check ([docs/refcheck.md](docs/refcheck.md)); `StructureImport/`, the model import ([docs/structures.md](docs/structures.md)); `model-info.py` and `prepare-model.py`,
  model checks and texture shrinking ([docs/model-prep.md](docs/model-prep.md)).
- `Directory.Build.props` imports the engine's, so mods build with the game's settings (warnings are errors).
- `Directory.Build.rsp` stamps the engine references as the oldest Meitou release the mods support, equal to each `gameVersion` floor: the
  game refuses a mod referencing a newer engine stamp, and the submodule builds `1.0.0.0`
  ([versioning](https://github.com/TheIvoryDisaster/GutenStructure/blob/beta/docs/versioning.md)). The Meitou compatibility workflow checks the
  mods daily against new Meitou releases ([meitou compat](https://github.com/TheIvoryDisaster/GutenStructure/blob/beta/docs/meitou-compat.md)).

## Branches

PRs into `beta`; CI (build and tests) must pass. `beta` is promoted to `main` once tested in play, and production goes through a PR to
`release`. Each merge into `release` tags it `animal-behaviors-v<version>` and publishes the mod's zip (`.github/workflows/release.yml`)
unless that tag exists; `tools\Tag-Release.ps1` is the manual fallback. Each framework change merged into `beta` also publishes a prerelease
`animal-behaviors-<version>-beta.<commit>` for dependents to lock (`.github/workflows/animal-behaviors-beta.yml`).

- New-repository branch protections and permanent beta/main/release reviews: [docs/branch-policy.md](docs/branch-policy.md). Templates copy source; GutenStructure applies settings through reconciliation.
