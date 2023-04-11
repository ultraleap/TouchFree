# TouchFree
TouchFree includes an Application, a Windows Service and a series of Tooling packages in order to
convert Ultraleap tracking data into a data structure suitable for input systems for touchscreen
user interfaces.

## WARNINGS
- This repository has been made available for you to work with, edit and make your own versions.
This repository and any derivatives of it is presented on an ‘as-is’ basis in line with the Apache
license. Unless it is an officially released application available from our website or an explicitly
agreed license, it is not supported by Ultraleap and we are not responsible for anything that you
create.

If you require assistance whilst working with these repositories we recommend utilising our [Help Center](https://forums.leapmotion.com/), [Developer Forums](https://support.leapmotion.com/hc/en-us) or our [Documentation Site](https://docs.ultraleap.com/) to help you find the answers or get support from the wider developer community.

## Contents:

### TouchFree Overlay

The TouchFree Overlay application allows for a retrofit solution. This application provides
a transparent overlay that enables you to use TouchFree interactions with any existing touch-supported application.

TouchFree Overlay requires the TouchFree Service to run properly.

As TouchFree Overlay depends on TouchFree Tooling, if you intend to work on TouchFree Overlay as a
developer, you will need to run the `setup_repository.bat` script in the `Scripts` directory. Ensure
that if you have previously worked on the Overlay, that you delete the `TF_Application/Assets/TouchFree`
folder before running this script.

### TouchFree Service:

The TouchFree Service must be running in order to make use of the TouchFree Application or any Client built with the TouchFree Tooling.
The TouchFree Service is built from within the Unity project found at `./TF_Service_and_Tooling_Unity`.

### TouchFree Tooling

The TouchFree Tooling provides a Client connection to the TouchFree Service where it receives positional and interaction data relevant to touchless interaction.
Additionally, TouchFree Tooling provides extendable Cursors and Input Systems to allow the TouchFree Service's data to be used in different environments. It requires
both the Ultraleap Tracking service installed to provide data from a camera, and the TouchFree Service
(as above).

The TouchFree Tooling is available for two integration environments at the moment:

* Unity
  * The Unity version of TouchFree Tooling can be found in the project found at
  `./TF_Service_and_Tooling_Unity`. Builds can be found on the releases page linked above.

* Web (JavaScript)
  * The JavaScript version of TouchFree Tooling can be found at
  `./TF_Tooling_Web`. Built in TypeScript, it available in JavaScript form
