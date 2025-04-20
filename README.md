# KeyRemapper

[中文说明](README_zh.md)

A **Beat Saber** mod that lets you remap almost any controller button.

---

## Current Capabilities

| Action | Description |
|--------|-------------|
| **Pause** | You can bind **any supported button** as the pause key. |

> **Restart actions and “block built‑in keys” are **not implemented yet**.  
> They are harmless in the config but currently ignored.

---

## Supported Button Tokens

| Token | Physical Button | Unity `CommonUsages` | Hand |
|-------|----------------|----------------------|------|
| `L_X` / `R_A` | X (left) / A (right) | `primaryButton` | L / R |
| `L_Y` / `R_B` | Y (left) / B (right) | `secondaryButton` | L / R |
| `L_Grip` / `R_Grip` | Grip | `gripButton` | L / R |
| `L_Trigger` / `R_Trigger` | Trigger (click) | `triggerButton` | L / R |
| `L_Stick` / `R_Stick` | Thumb‑stick click | `primary2DAxisClick` | L / R |
| `L_Menu` / `R_Menu` / `Menu` | Menu button | `menuButton` | L / R / Both |

> **Heads‑up – Triggers**  
> Triggers also act as **click** inside the pause menu, so binding `L_Trigger`/`R_Trigger` to *Pause* can make the cursor instantly resume the game.  
> If you must use a trigger, bind **only one** side and be careful.

---

## Installation

1. Install Beat Saber modding tools (e.g. **Mod Assistant**).
2. Download the latest **`KeyRemapper.dll`** from the [Releases](https://github.com/lyyQwQ/KeyRemapper/releases) page.
3. Drop the DLL into your **Beat Saber/Plugins** folder.

---

## Configuration File

On first launch the mod creates  
`UserData/KeyRemapper.json`. A minimal file looks like:

```jsonc
{
  "Version": 2,
  "Actions": {
    "Pause": {
      "Bindings": [],          // Your custom buttons
      "BuiltInKeys": [],       // Native Beat Saber buttons (not blocked yet)
      "BlockBuiltIn": false    // Has no effect for now
    },
    "Restart": {               // Reserved – currently ignored
      "Bindings": [],
      "BuiltInKeys": [],
      "BlockBuiltIn": false
    }
  }
}
```

### Example (author’s own setup)

```jsonc
{
  "Version": 2,
  "Actions": {
    "Pause": {
      "Bindings": ["L_X", "L_Y", "R_A", "R_B"],
      "BuiltInKeys": [],
      "BlockBuiltIn": false
    },
    "Restart": {
      "Bindings": [],
      "BuiltInKeys": [],
      "BlockBuiltIn": false
    }
  }
}
```

* Edit the file, add any **Token** from the table above into `Bindings`, save, and restart the game.
* Empty array `[]` means “not set”.

---

## Known Limitations

1. **Trigger as Pause** – interferes with menu clicks; consider another button.
2. `BlockBuiltIn` does nothing yet – the Menu key still pauses.
3. `Restart` action is not wired up yet.

---

## Contributing

Pull requests and issues are welcome!  
If you add new buttons or features, please update both the English and Chinese docs.