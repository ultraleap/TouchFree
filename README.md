# ScreenControl
ScreenControl converts Ultraleap tracking data into a data structure suitable for input systems for touchscreen user interfaces via a client package.

## WARNINGS
- This repository has been made available for you to work with, edit and make your own versions.
This respoitory and any derivatives of it is presented on an ‘as-is’ basis in line with the Apache
license. Unless it is an officially released application available from our website or an explicitly
agreed license, it is not supported by Ultraleap and we are not responsible for anything that you
create.

If you require assistance whilst working with these repositories we recommend utilising our [Help Center](https://forums.leapmotion.com/), [Developer Forums](https://support.leapmotion.com/hc/en-us) or our [Documentation Site](https://docs.ultraleap.com/) to help you find the answers or get support from the wider developer community.

## Contents:

### ScreenControl Service:

ScreenControl Service must be running in order to make use of any of the ScreenControl Clients.
The ScreenControl Service is built from within the Unity project found at `./ScreenControl_Unity`.

### ScreenControl Client

The ScreenControl Client provides access to positional and interaction data, and extensible Cursors
and Input Systems to allow ScreenControl Service's data to control those environments. It requires
both the Leap Motion service installed to provide data from a camera, and a ScreenControl Service
(as above).

Two integration environments are available for ScreenControl at the moment:

* Unity
  * The Unity version of ScreenControl can be found in the ScreenControl Unity project found at
`./ScreenControl_Unity`. Builds can be found on the releases page linked above.

* Web (JavaScript)
  * The JavaScript version of ScreenControl can be found at `./ScreenControl_Web`. Built in TypeScript,
it available in JavaScript form.