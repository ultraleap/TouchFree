# TouchFree
TouchFree includes an Application, a Windows Service and a series of Tooling package in order to
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

### TouchFree Application

TouchFree Application requires the TouchFree Service to run and allows for a retrofit solution. This application provides
a transparent overlay application that enables you to use TouchFree interactions with any existing touch-supported application

### TouchFree Service:

TouchFree Service must be running in order to make use of TouchFree Application or any of the TouchFree Tooling Clients.
The TouchFree Service is built from within the Unity project found at `./Service_Unity`.

### TouchFree Tooling Client

The TouchFree Client provides access to positional and interaction data, and extensible Cursors
and Input Systems to allow TouchFree Service's data to control those environments. It requires
both the Ultraleap Tracking service installed to provide data from a camera, and a TouchFree Service
(as above).

Two integration environments are available for TouchFree Tooling at the moment:

* Unity
  * The Unity version of TouchFree Tooling can be found in the TouchFree Service project found at
`./Service_Unity`. Builds can be found on the releases page linked above.

* Web (JavaScript)
  * The JavaScript version of TouchFree Tooling can be found at `./Tooling_Web`. Built in TypeScript,
it available in JavaScript form.