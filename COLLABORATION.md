# Collaboration

Do both of these **before** adding a collaborator to the repo. They are cheap now
and expensive to retrofit after the first conflicted scene.

## 1. Set up the UnityYAMLMerge driver (once per machine)

`.gitattributes` marks scenes, prefabs, and other Unity YAML as
`merge=unityyamlmerge`, but that name means nothing until each machine defines the
driver. Without it git silently falls back to a plain text merge, which mangles
YAML that has moved around.

Run once, per machine, inside the repo (drop `--local` to apply it to all repos):

```sh
git config --local merge.unityyamlmerge.name "Unity SmartMerge"
git config --local merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/6000.5.7f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p --force --fallback-none %O %B %A %A'
git config --local merge.unityyamlmerge.recursive binary
```

Adjust the path for a different Unity version or install location; on macOS it is
under `/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/Tools/`.

Verify with `git config --get merge.unityyamlmerge.driver` — empty output means it
is not active.

## 2. Agree on scene and prefab ownership

Unity scenes and prefabs do not merge reliably even with SmartMerge. Treat them as
single-writer files:

- One person owns a given scene or prefab at a time. Say so in chat before editing.
- Prefer building work as prefabs and dropping one instance into the scene, so two
  people can work in parallel without touching the same `.unity` file.
- Keep scene edits short-lived. Commit and push the same day rather than sitting on
  a modified scene for a week.
- If two people did edit the same scene, do not hand-resolve the conflict. Pick one
  version, take the other person's changes manually in the editor, and move on.

Git has no locking here, so this is a convention the team has to hold, not
something the repo enforces.
