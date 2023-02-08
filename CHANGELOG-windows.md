# TouchFree Windows Changelog

![Discord](https://img.shields.io/discord/994213697490800670?label=Ultraleap%20Developer%20Community&logo=discord)
[![documentation](https://img.shields.io/badge/Documentation-docs.ultraleap.com-00cf75)](https://docs.ultraleap.com/touchfree-user-manual/)
[![mail](https://img.shields.io/badge/Contact-support%40ultraleap.com-00cf75)](mailto:support@ultraleap.com)

All notable changes to TouchFree software components are documented in this file.
Change fragments are commonly categorised by the software component they relate to.

Components include:

- Service: The .NET TouchFree Service which processes hand data received from the Ultraleap Tracking Service into TouchFree input events
- Tracking Service: The Ultraleap Tracking Service which manages connecting to camera hardware and running the hand tracking model
- Installer: The TouchFree combined installer which installs all software components in a bundle
- Unity Settings: The settings UI currently opened from the windows tray icon
- System Tray: The Windows tray icon
- Overlay Application: The TouchFree Overlay Application launched from the tray icon that enables using TouchFree as a pointing device

TouchFree web tooling changes are documented separately.
See [here if looking at the source](./TF_Tooling_Web/CHANGELOG.md) otherwise [here on GitHub](https://github.com/ultraleap/TouchFree/blob/develop/TF_Tooling_Web/CHANGELOG.md).

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Service: `HandEntered` & `HandExited` events, sent when the active hand enters and exits the interaction zone (if enabled) respectively

### Fixed

- Unity Settings/Service: Fixed an issue where using Touch Plane with Scroll and Drag caused cursor jumps
- Unity Settings/Service: Fixed an issue where clicking was very difficult while Touch Plane and Scroll and Drag were both active
- Unity Settings: Fixed an issue with moving the bottom masking slider
- Service: Fixed an issue where the TouchFree Service would frequently throw exceptions when a client disconnects

## [2.4.0] - 2022-10-14

### Changed

- Tracking Service: Updated Ultraleap Tracking software which includes stability and security enhancements
- Service: TouchFree detects and reacts to the most appropriate hand in the scene
- All TouchFree related settings are now camera independent, allowing configuration settings to be copied between similar kiosk hardware

### Fixed

- Overlay Application: Fixed an issue where the overlay would not properly enter fullscreen when showing a CTI
- Service: TouchFree no longer sends multiple input events, which can cause performance issues in some cases
- Service: Air Push clicks are no longer oversensitive when scroll & drag is disabled
- Unity Settings: The lower masking slider on the Masking screen no longer becomes unresponsive

## [2.3.2] - 2022-09-23

### Fixed

- Service: TouchFree no longer sends multiple input events when it shouldn’t

## [2.3.1] - 2022-07-13

### Fixed

- Tracking Service: Ultraleap Tracking service is no longer left unresponsive by Windows shutdown/restart

## [2.3.0] - 2022-05-4

### Added

- Installer: Now installs Ultraleap Tracking service with TouchFree

### Changed

- Service: Scroll and Drag is enabled by default, enabling interaction with scrollable content out of the box
- Service: Air Push and hold makes it easier to click when interacting with scrollable content
- Service: Reduced system load when TouchFree is idle
- Unity Settings: Features previously found in the Ultraleap Tracking Control Panel are now presented in TouchFree Settings UI

### Fixed

- Overlay Application: The TouchFree overlay application reconnects if the TouchFree service stops/starts
- Unity Settings: Camera masking screen now displays correctly at 16:10 aspect
- Unity Settings: TouchFree Settings can now be used with a mouse at the same time as a hand is tracked
- Unity Settings: After PC restart the camera masking will now load in the settings UI
- System Tray: Tray icon can now be reopened from start menu
- Service: Tracking now always launches in the correct orientation
- Service: TouchFree handles invalid config files

### Known Issues

- Installer: Ultraleap Tracking service can be installed twice when installing using both the TouchFree installer and the Tracking installer

## [2.2.1] - 2022-02-28

### Fixed

- Service: Incorrect Tracking mode on windows boot

## [2.2.0] - 2022-02-8

### Added

- Unity Settings: Added `Appearance` page to incorporate TouchFree Application specific settings
- Unity Settings: Added `Camera Setup / Camera Feeds and Masking` page
- Unity Settings: Decimal number fields support comma as decimal symbol
- Unity Settings: Users are warned if they have insufficient permissions to edit files
- System Tray: Added option to `Start/Stop TouchFree`

### Changed

- Overlay Application: Moved application specific settings to `Settings/Appearance` page
- Unity Settings: Cursor shown in UI adopts cursor settings from Appearance page

### Fixed

- Unity Settings: Serialization issue when entering commas in text fields
- Unity Settings: Input active when cursor is not
- Unity Settings: Exiting Quick Setup early keeps the selected tracking mode
- Unity Settings: `Appearance/Reset to Defaults` leaves cursor colour settings open
- Service: Tracking mode is not correctly set on system boot when Tracking Service loads after TouchFree Service

### Known Issues

- Starting a second instance of TouchFree Overlay causes a “Fatal Error” popup (first instance remains running)

## [2.1.0] - 2021-09-29

### Added

- Compatibility with Ultraleap Hand Tracking 5.2.0

### Changed

- The `Above Facing User` option is now always available in the camera mounting options

### Fixed

- Fixed an issue where the CTI would open at the wrong resolution
- Fixed an issue where the cursor on the Service Settings UI was not hidden when resetting the camera setup to default values

### Known Issues

- This version is not compatible with previous versions of Leap Motion hand tracking

## [2.0.0] - 2021-09-8

### Added

- TouchFree features a background service that provides input data to connected clients
- The TouchFree application provides the cursor overlay and Windows touch input for retrofitting users
- TouchFree comes with an installer to make it easier to get setup
- A new interaction, `Touch Plane`, provides the option to trigger input at a fixed, user configurable distance from the screen
- Users can now use TouchFree Tooling for web and Unity to integrate TouchFree into their application

## [1.1.0] - 2021-01-22

### Added

- AirPush gesture

## [1.0.0] - 2020-10-8

Initial release
