# Changelog

## [1.1.0] - 2024-04-24

### Added
- Added Netcode For Entities layout option to each mppm clone if package is installed.
- Close button now displays warning dialog prompt and if confirmed, the virtual player is now deactivated
- Added the ability to mute players to the settings
- Added the ability to change the asset database refresh timeout to the settings
- Can now properly focus on other individual player windows from inside an individual player (by using the keyboard shortcuts)
- Added clearer message to certain types of Symlink failures (on FAT32) which are not supported

### Fixed
- Upgrades of the MPPM package now clear out the local clone cache to ensure stable updates
- Fixed a crash with layout files when computers were set to certain regions
- Changed default multiplayer role of player clones to Client and Server
- Added a minimum width for the main view of the MPPM window
- Escape key no longer closes virtual player windows on Windows
- Fixed issues with heartbeat timeout

## [1.0.0] - 2024-03-12

### Added

- Multiplayer development workflow aiming to offer a more efficient and quick development cycle of multiplayer games. The tool enables opening multiple Unity Editor instances simultaneously on the same development device, using the same source assets on disk.
