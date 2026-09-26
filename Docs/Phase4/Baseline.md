# Phase 4 baseline — 26 September 2026

Recorded before changing values. Clean main at 2b57cc5; local tracking ref origin/main also at this commit. Unity 6000.3.11f1 (3000ef702840). Original repository clean/read-only.

## Movement and input

Sandbox overrides the marble prefab: target speed 8 world units/s, response coefficient 12/s, horizontal safety cap 15. Controller uses a velocity-error deadband of 0.05 and acceleration limit target speed × 15 = 120 units/s². Same coefficient for acceleration, release and reversal. Euler velocity adjustment at fixed step 0.02 s. No explicit input deadzone after provider; radial clamp at magnitude 1.

Factory multipliers: keyboard 100, touch 0.2, gyro 2. Keyboard uses legacy smoothed axes (Vertical, -Horizontal); touch uses screen-relative position (y, -x); gyro uses gravity (y, -x). Axis conventions stay unchanged. Touch's full-axis target tops out at 1.6 units/s before friction/damping, versus keyboard's 8.

Marble prefab: unit scale, sphere radius 0.5, mass 1, gravity enabled, angular damping 0.05, no constraints, interpolation None, collision detection Discrete. Sandbox linear damping 0.5. Collider physics material remains unchanged. Shrink/grow duration 1 second each. Camera-only pivot tilt: 20 degrees, interpolation coefficient 5/s. Board/collision geometry does not tilt.

User reports somewhat slow movement. No new human baseline playtest was performed by the agent. These are authored settings and implementation facts, not a subjective feel rating.

## Presentation

Unlock line: width 0.06, radius 2.43, floor +0.03, yellow, no dwell-progress/reset/completion presentation. Goal: flat correct/incorrect indicators, partially occluded on 48/50. Reveal: instant renderer enable without discovery effect. Dark: height 10.8, range 38.61, outer angle 46.6°, inner angle 4°, intensity 3, white, black ambient and zero reflection. Marble Standard material: metallic 1, gloss 1, no emission.

Audio: drop clip subscribed to marble-drop event; assigned clang clip unused. No current dedicated reveal/unlock-completion sounds.
