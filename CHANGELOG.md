# Changelog

All notable changes to this project will be documented in this file.

## [1.2.1] - Fixed Preset Override Persistence

Preset-applied hierarchy rows now behave like linked presets until manually edited.

Manual overrides to a row's icon, built-in icon, colour, or colour display options now break the preset link.
Editing a preset still updates all hierarchy rows that remain linked to that preset.

Local row customisations now persist correctly after project reloads instead of being overwritten by the previously applied preset.

## [1.2.0] - Ability to split colours between icon and bar

Sometimes separators look odd buired in hierarchies so this lets you choose where you want them

## [1.1.0] - Changes to UI and the ability to set your own presets

Ability to set your own presets.
Made the UI more ergonomic

## [1.0.2] - Intelligent Saving

This will remember colours across scenes.
i.e., make a camera called MyCamera. Save its scene as scene2 and the colours persist.

Removes open script from inside menu. Double click icon.

## [1.0.1] - Better Unity compatibility

### Fixed
- Added support for Unity 6000.4+ using the EntityId hierarchy callback.
- Retained compatibility with older Unity versions using the legacy hierarchy callback.

## [1.0.0]

### Changed
- Now uses `OnValidate` for improved automatic updates.
- Sets the tag to `EditorOnly` to prevent separators from being included in builds.

## [0.0.0] - Initial Release

### Added
- Implemented `HierarchySeparator` to create aesthetic separators in the Unity Editor hierarchy.
- Supports Unity **2021.3** and later.
- Simple and lightweight utility for organizing scene hierarchy.

---

This project follows [Semantic Versioning](https://semver.org/).