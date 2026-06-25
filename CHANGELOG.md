# Changelog

All notable changes to this project will be documented in this file.

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