# Changelog
![Discord](https://img.shields.io/discord/994213697490800670?label=Ultraleap%20Developer%20Community&logo=discord)
[![documentation](https://img.shields.io/badge/Documentation-docs.ultraleap.com-00cf75)](https://docs.ultraleap.com/touchfree-user-manual/)
[![mail](https://img.shields.io/badge/Contact-support%40ultraleap.com-00cf75)](mailto:support@ultraleap.com)

All notable changes to TouchFree software components are documented in this file.
Change fragments are commonly categorised by the software component they relate to.

Components include:
- Service: The .NET TouchFree Service which processes hand data received from the Ultraleap Tracking Service into TouchFree input events
- Tracking Service: The Ultraleap Tracking Service which manages connecting to camera hardware and running the hand tracking model
- Installer: The TouchFree combined installer which installs all software components in a bundle
- Web Settings: The settings UI currently used on Brightsign
- System Tray: The Windows tray icon
- Overlay Application: The TouchFree Overlay Application launched from the tray icon that enables using TouchFree as a pointing device

TouchFree web tooling changes are documented separately.
See [here if looking at the source](./TF_Tooling_Web/CHANGELOG.md) otherwise [here on GitHub](https://github.com/ultraleap/TouchFree/blob/develop/TF_Tooling_Web/CHANGELOG.md).

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Web Settings: Added a button that links to docs
- Web Settings: Auto calibration can be progressed by pressing spacebar

### Fixed
- Service: Fixed an issue where the TouchFree Service would frequently throw exceptions when a client disconnects

## [2.5.0-beta] - 2022-11-30

### Added
- Web Settings/Service: TouchFree version is now shown in the UI and logged by the service
- Web Settings: UI will now scale dynamically to viewport resolution

### Changed
- Web Settings: Configuration can be updated when no camera is connected
- Web Settings: Service status icons will now update immediately upon device connection/disconnection
- Web Settings: Improved the performance of the camera masking screen

### Fixed
- Web Settings/Service: Fixed an issue where using Touch Plane with Scroll and Drag caused cursor jumps
- Web Settings/Service: Fixed an issue where clicking was very difficult while Touch Plane and Scroll and Drag were both active
- Web Settings: Interaction specific settings now correctly save when modified in the UI

## [2.4.0-beta] - 2022-09-14

### Added
- Web Settings: New settings UI including visualized hands, tracked hands and masking ability
- Web Settings: Visual feedback in the UI is provided when tracked hands are lost
- Web Settings/Service: New quick setup calibration process, eliminating the need for keyboard input

### Changed
- Tracking Service: Improved tracking performance and stability when using `Above Facing User` (screentop) camera mode
- Service: Tracking configuration is now stored in TouchFree and forced to load on device boot & camera connection
- All TouchFree related settings are now camera independent, allowing configuration settings to be copied between similar kiosk hardware

### Fixed
- Service: TouchFree no longer sends multiple input events, which can cause performance issues in some cases
- Service: Air Push clicks are no longer oversensitive when scroll & drag is disabled
- Service: Fixed an issue preventing changing masking options or accessing Tracking service logs
- Web Settings: SVG cursor no longer blocks chrome dev tools