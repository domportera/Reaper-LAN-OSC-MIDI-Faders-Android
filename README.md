## [Reaper MIDI OSC Faders over Wifi - Android](https://forum.cockos.com/showthread.php?t=235365)

<img src="https://i.imgur.com/EMef9Ou.png" width="640" height="auto">

This is my repo for my customizable Reaper OSC MIDI control solution for Android. Built in Unity. All feedback and feature requests are welcome! Currently this only works with Reaper, but I am open to extending it to other DAWs if people request it.

Also in-development are custom OSC messages that can be pre-programmed and mapped to specific controllers.

<img src="https://i.imgur.com/eso7wAj.png" width="640" height="auto"/>
<img src="https://i.imgur.com/5B7qOs0.png" width="640" height="auto"/>
<img src="https://i.imgur.com/5AEvgB3.png" width="640" height="auto"/>
<img src="https://i.imgur.com/owOULni.png" width="640" height="auto"/>
<img src="https://user-images.githubusercontent.com/6530600/211082844-412bb4b3-9a97-44ed-8132-4bce85e3057c.png" width="100%" height="auto"/>



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
 
<img src="https://i.imgur.com/Y7WnuAu.png" alt="Fader options" width="640" height="auto">

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
 - Displaying fader value curves on the fader
 - A way to import and export user data

