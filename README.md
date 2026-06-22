# Pretty Hierarchy

Create a cleaner, more readable Unity Hierarchy with custom icons, coloured section separators, and automatic component-based icon assignment.

Pretty Hierarchy lets you:

- Add custom icons to specific GameObjects.
- Use Unity built-in icons (Prefab, Folder, Camera, Light, Android, PC, etc.).
- Create coloured hierarchy separators to organise large scenes.
- Automatically show icons for objects containing specific components.
- Open scripts directly from the Hierarchy with a double click.
- Configure everything through a simple context menu.

<img width="829" height="824" alt="image" src="https://github.com/user-attachments/assets/90bcd4e7-fa8d-4e8b-b36a-41359f6eb398" />

---

## Features

### Custom Object Icons

Assign an icon to any GameObject in the Hierarchy.

- Custom texture icons
- Unity built-in icons
- Per-object configuration
- Stored automatically in a settings asset

### Hierarchy Separators

Turn any GameObject into a visual section divider.

Examples:

- UI
- Networking
- Scene
- Config
- Debug
- Permissions
- Game Flow

Each separator can have:

- Custom colour
- Optional icon
- Custom label (the GameObject name)

### Automatic Component Icons

Configure icons for component types.

For example:

- NetworkManager → Networking icon
- Camera → Camera icon
- AudioSource → Audio icon

Any GameObject containing that component will automatically display the configured icon.

---

## Installation

### Via Git URL

Open `Packages/manifest.json` and add:

```json
{
  "dependencies": {
    "com.baawolf.prettyhierarchy": "https://github.com/gavwood/PrettyHierarchy.git"
  }
}
```

Or use:

**Window → Package Manager → Add Package From Git URL**

and enter:

```text
https://github.com/gavwood/PrettyHierarchy.git
```

---

## First Use

After installation, Pretty Hierarchy automatically creates:

```text
Assets/Editor/PrettyHierarchySettings.asset
```

This asset stores all icon and separator configuration.

---

## How To Use

### Open The Pretty Hierarchy Menu

In the Hierarchy:

**Right-click the icon area on any row**

This opens the Pretty Hierarchy editor window for the selected object.

---

### Add A Custom Icon

1. Right-click the icon area.
2. In the Pretty Hierarchy window:
   - Assign a texture to **Custom Texture**
   - Or choose a **Built-in Icon**
3. The icon immediately appears in the Hierarchy.

---

### Create A Separator

1. Right-click the icon area.
2. Choose a preset colour or set your own.
3. Enable or disable **Show Icon**.
4. Rename the GameObject to the desired section title.

Example:

```text
──────────── UI ────────────
───────── Networking ───────
────────── Gameplay ────────
```

The separator colour fills the hierarchy row and the object name is displayed centrally.

---

### Open Scripts Quickly

Double-click the icon area of a GameObject.

Pretty Hierarchy will open the first non-Unity script attached to that object.

---

### Clear An Icon

1. Open the Pretty Hierarchy window.
2. Click:

```text
Clear Icon
```

Only the icon is removed.

---

### Clear Everything

1. Open the Pretty Hierarchy window.
2. Click:

```text
Clear Everything
```

This removes all Pretty Hierarchy data for that GameObject.

---

## Built-in Icons

Pretty Hierarchy includes quick access to Unity icons such as:

- Android
- PC
- Editor
- Prefab
- Folder
- Script
- Camera
- Light
- Audio
- Anchor

You can also use your own textures.

---

## Tips

- Use separators to organise large scenes.
- Use component icons to identify systems instantly.
- Double-click icons to jump directly to scripts.
- Use built-in icons to keep a consistent visual style.

---

## License

MIT License

Copyright © 2026 BaaWolf
