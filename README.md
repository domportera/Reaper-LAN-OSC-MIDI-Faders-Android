## Reaper MIDI OSC Faders over Wifi - Android

<img src="https://i.imgur.com/2jADy1I.png" alt="Main user interface" width="640" height="360">

This is my repo for my customizable Reaper OSC MIDI control solution for Android. Built in Unity. All feedback and feature requests are welcome! Currently this only works with Reaper, but I am open to extending it to other DAWs if people request it.

<img src="https://i.imgur.com/4F4QBAK.png" alt="Options menu" width="640" height="360">

**With this you can:**
 - Control Reaper MIDI wirelessly with your Android device
 - Create (a virtually unlimited quantity of) faders 
 - Create faders that return to their default value when released (like
   a pitch wheel, but with extra options)
 - Map faders to any MIDI control - Pitch, aftertouch, any of the 127 CC
   channels
 - Map each fader to a specific MIDI channel
 - Change fader movement smoothness
 - Change fader width
 - Save profiles of groups of faders with all their parameters
 - Use value curves - Linear, Logarithmic, and Exponential (2) control options for each fader
 - Change the value range (7-bit or 14-bit) of each control
 - All faders and profiles can be named
 - Faders can be sorted into an order that suits your needs best
 - Use built-in defaults to test controls that are verified to work
 
<img src="https://i.imgur.com/Y7WnuAu.png" alt="Fader options" width="640" height="360">

**All you need to do is:**
 - Configure Reaper to receive OSC messages
 - Make sure both devices (phone and PC) are on the same local wifi
   network
 - Set your IP in-app to your PC's IPv4 address
 - Set your port in-app to Reaper's OSC port

**Planned features:**
 - Option for relative fader control, rather than your touch's absolute
   position
 - Custom fader/UI colors
 - A proper how-to guide in-app
 - An in-app MIDI CC reference list
 - Support for multiple aspect ratios (currently only supports 9:16, but
   should work on other aspect ratios. It might look weird.)
 - Displaying fader value curves on the fader
 - A way to import and export user data

