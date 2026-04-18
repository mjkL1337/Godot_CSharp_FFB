# Godot C# Force Feedback Test Project
Test and demo project for Force Feedback in Godot using C# and DirectInput (SharpDX).

## Why
The other alternative is [x-channel's custom engine build](https://github.com/x-channel/godot/), which is stuck on 4.4.2.rc because Godot replaced SDL2 with SDL3 in 4.5 and broke the whole thing. This approach does it entirely in C# so it doesn't care what engine version you're on.

## Features
- Detects the first FFB device it finds automatically
- Hot-plug support
- No custom engine build needed
- Windows only (DirectInput)

Built and tested on 4.6.1.stable.mono.official
