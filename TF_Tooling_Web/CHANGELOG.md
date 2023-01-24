# TouchFree Web Tooling Changelog
![Discord](https://img.shields.io/discord/994213697490800670?label=Ultraleap%20Developer%20Community&logo=discord)
[![documentation](https://img.shields.io/badge/Documentation-docs.ultraleap.com-00cf75)](https://docs.ultraleap.com/touchfree-user-manual/)
[![mail](https://img.shields.io/badge/Contact-support%40ultraleap.com-00cf75)](mailto:support@ultraleap.com)

All notable changes to the TouchFree Web Tooling project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `ServiceStatus` now includes the service version, tracking version, camera serial number and camera firmware version.
- `OnServiceStatusChange` event - provides data about the status of the TouchFree Service whenever it changes.
- `WhenConnected` event - functions identically to `OnConnected` but will dispatch an event immediately if already connected to the TouchFree Service.
- `IsConnected` function exported as part of the top level `TouchFree` object for querying whether or not the client is connected to the TouchFree Service.

### Deprecated
- `ConnectionManager.AddConnectionListener()` - functions identically to the `WhenConnected` event added this release and has been deprecated in favor of it.
- `ConnectionManager.AddServiceStatusListener()` - functions identically to the `OnTrackingServiceStateChange` event and has been deprecated in favor of it.

## [1.3.0] - 2022-11-23

### Added
- Top level `TouchFree` object as a place for functionality to accomplish common TouchFree tasks.
- `Init` function in the `TouchFree` object which simplifies initialization of TouchFree to a single call.
- Event handling functions in `TouchFree` object:
    - `RegisterEvent` for registering callbacks to global TouchFree events safely. Returns a convenient object that can be used to `UnregisterEventCallback`.
    - `DispatchEvent` for dispatching events safely - not typically needed by library consumers.

### Changed
- TouchFree will now send Click events.

## [1.2.0] - 2022-09-30

### Added
- New cursor that uses SVGs rather than images - allowing for integration into a page using a single line of code.
- The Ultraleap Tracking Service can now be configured through the Tooling for Web.

### Changed
- The TouchFree cursor will now scroll scrollable areas without the need for extra code in the InputController.

## [1.1.1] - 2022-06-14

### Added
- `ServiceConnection.Disconnect` allowing a user to force close the web socket.

### Fixed
- Several internal fixes to ensure stability.

## [1.1.0] - 2022-06-8

### Added
- Input Action Events can now be sent to HTML elements at the position of the cursor, making it easier to build reactive content.
- TouchFree tooling clients can request updates to the config files via the service to make global changes to the TouchFree configuration for all clients.
- TouchFree service state can be queried.

### Fixed
- NONE events are now properly culled from overfilled InputAction Queues, improving performance.
- Web Tooling cursor position now aligns correctly with scaled displays.
- Cursor position no longer needs to be inverted in the Y axis every time it's used.
    - NOTE: If you have used TouchFree cursor data directly, you will need to update your code to remove the inversion of the Y Axis data

## [1.0.0] - 2021-09-7

Initial release