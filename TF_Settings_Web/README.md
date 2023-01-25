# TouchFree Web Settings

The settings page for TouchFree served by the service.

This provides a front end for the following features:

-   Calibration of the service - manual and automatic
-   Camera masking, to remove any problematic areas of the video feed
-   Interaction settings control, including:
    -   Switching between interaction types
    -   Controlling sensitivity
    -   Interaction zone settings
-   View version numbers of the service and camera firmware/serial numbers

## Developer Guide

### Requirements

-   [NodeJS](https://nodejs.dev/)

### Usage

-   Install: `npm i`
    -   Install the required dependencies
-   Run: `npm start`
    -   Start the app in development mode. Includes hot reloading when changes are made.
-   Build: `npm run build`
    -   Build the application into a production state
-   Fix linting: `npm run prettyLint`
    -   Runs prettier and eslint and writes any possible fixes to problems.
