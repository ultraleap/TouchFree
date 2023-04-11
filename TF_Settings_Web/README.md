# TouchFree Web Settings

TouchFree Web Settings provides a user interface to interact with the TouchFree service

It contains the following features:

-   Calibrate the service either automatically or manually
-   Mask off problematic areas of the camera feed, e.g. bright, reflective areas
-   Flip the camera feed vertically
-   Toggle allowing analytics
-   Toggle allowing access to the camera feed
-   Switch between interaction types (Hover and Hold, AirPush etc)
-   Edit interaction specific settings
-   Control sensitivity of the TouchFree cursor
-   Edit Interaction Zone settings
-   View Service and Tracking version numbers
-   View current camera Firmware and Serial number

    Windows Only Features:

-   Open Tracking and Touchfree log files
-   Configure TouchFree Overlay App cursor and CTI

## Developer Guide

### Requirements

-   [NodeJS](https://nodejs.dev/)
-   [Rust + Build tools](https://tauri.app/v1/guides/getting-started/prerequisites)

### Usage

-   Install: `npm i`
    -   Install the required dependencies
-   Run: `npm start`
    -   Start the app in development mode. Includes hot reloading when changes are made
-   Run (desktop): `npm run tauri dev`
    -   Start the app in development mode using Tauri. Includes hot reloading when changes are made
-   Build: `npm run tauri build`
    -   Build the application (and Tauri wrapper) into a production state
-   Fix linting: `npm run prettyLint`
    -   Runs prettier and eslint and writes any possible fixes to problems
