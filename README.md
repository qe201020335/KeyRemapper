# KeyRemapper

[中文说明](README_zh.md)

A **Beat Saber** mod that lets you remap almost any controller button.

---

## Current Capabilities

| Action      | Description                                                 |
|-------------|-------------------------------------------------------------|
| **Pause**   | You can bind **any supported button** as the pause key.     |
| **Restart** | You can bind **any supported button** to restart the level. |

---

## Supported Button Tokens

| Token                        | Physical Button      | Unity `CommonUsages` | Hand  |
|------------------------------|----------------------|----------------------|-------|
| `L_X` / `R_A`                | X (left) / A (right) | `primaryButton`      | L / R |
| `L_Y` / `R_B`                | Y (left) / B (right) | `secondaryButton`    | L / R |
| `L_Grip` / `R_Grip`          | Grip                 | `gripButton`         | L / R |
| `L_Trigger` / `R_Trigger`    | Trigger (click)      | `triggerButton`      | L / R |
| `L_Stick` / `R_Stick`        | Thumb‑stick click    | `primary2DAxisClick` | L / R |
| `L_Menu` / `R_Menu` / `Menu` | Menu button          | `menuButton`         | L / R |

> **Heads‑up – Triggers**  
> Triggers also act as **click** inside the pause menu, so binding `L_Trigger`/`R_Trigger` to *Pause* can make the
> cursor instantly resume the game.  
> If you must use a trigger, bind **only one** side and be careful.

---

## Requirements

- BSIPA
- BSML
- SiraUtil

---

## Installation

1. Install the required mods
2. Download the latest from the [Releases](https://github.com/lyyQwQ/KeyRemapper/releases) page.
3. Unzip the archive into your Beat Saber folder.

---

## Configuration

### In-Game Settings (New in v0.2.0)

You can now configure button mappings directly in-game:
1. Go to **Mods** menu in the main menu
2. Select **KeyRemapper**
3. Use the dropdown menus to add/modify button bindings
4. Changes are saved automatically and take effect immediately

### Manual Configuration (Advanced)

On first launch the mod creates  
`UserData/KeyRemapper.json`. A minimal file looks like:

```jsonc
{
  "Version": 2,
  "Actions": {
    "Pause": {
      "Bindings": [],          // Your custom buttons
      "BlockBuiltIn": false
    },
    "Restart": {
      "Bindings": []
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
      "BlockBuiltIn": false
    },
    "Restart": {
      "Bindings": []
    }
  }
}
```

* Edit the file, add any **Token** from the table above into `Bindings`, save
* Empty array `[]` means “not set”.

---

## Known Limitations

1. **Trigger as Pause** – interferes with menu clicks; consider another button.

---

## Contributing

Pull requests and issues are welcome!  
If you add new buttons or features, please update both the English and Chinese docs.