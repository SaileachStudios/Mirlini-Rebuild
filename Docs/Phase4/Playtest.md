# Human playtest — findings and remaining review

Phase 4 is **not accepted or frozen yet**. Automated measurements and agent inspection of engine images are separate from human gameplay feel, comfort and comprehension. The user has reported two general findings: release stops too abruptly and lacks believable momentum; the raised marker above Unlock goals appears offset and confusing. These are recorded for a later tuning pass as requested. No specific level numbers or full per-level ratings were supplied, so those ratings remain pending below.

## Run

Open Mirlini-Rebuild in **Unity 6000.3.11f1**, then **Mirlini → Phase 4 playtest**. Select a representative and press **Play selected level in Sandbox**. In Play Mode, the same window can start/restart any representative; wait for hole recovery to finish before changing attempts. Controls are the existing keyboard mapping. This window is editor review tooling, not final menus.

The current full-campaign auto-next behavior remains. Use the review window to return to the requested level. Stop Play Mode to restore the authored Sandbox selection. The shared profile is `Assets/Resources/Gameplay Feel.asset`; restart Play Mode after changing tuning. `TiltDegrees = 0` provides a technical no-motion option for comparison; a player-facing setting is still future work.

## Rating guide

Use **Good / Needs adjustment / Problem** and one short observation for each category:

- Movement: response, speed, precision, overshoot, stopping, wall contact. Try short taps, release, reversal and diagonal correction.
- Readability: marble, destination, ring, local Dark routes, discovery of hidden walls. On Unlock levels, enter briefly, exit to reset, then stay for two seconds and watch the goal change.
- Comfort: tilt, clutter, brightness, and discovery highlight. A still screenshot cannot answer this.

| Level | Focus | Human movement | Human readability | Human comfort | Agent visual finding / remaining check |
|---|---|---|---|---|---|
| 1 | Open baseline | Pending | Pending | Pending | Marble and goal distinguishable; real-physics speed/release checks passed. Assess weight and taps. |
| 2 | Ordinary corridors | Pending | Pending | Pending | Board and raised destination readable; assess cornering/overshoot. |
| 46 | Dense precision | Pending | Pending | Pending | Raised destination visible; assess narrow-maze control and marble occlusion near walls. |
| 7 | Unlock | Pending | Pending | Pending | Contrasting boundary, advancing arc, cleared reset, completed ring and changed goal verified. Assess intuitiveness. |
| 11 | Dark | Pending | Pending | Pending | Local texture retained instead of washed-out light pool; marble/goal identifiable. Assess whether visible route distance is sufficient. |
| 16 | Reveal | Pending | Pending | Pending | Immediate wall appearance plus warm outline, then settled wall. Assess discovery cue during motion. |
| 26 | Unlock + Dark | Pending | Pending | Pending | Ring and locked goal visible while maze remains dark. Assess navigation and landmark prominence. |
| 33 | Unlock + Reveal | Pending | Pending | Pending | Ring and locked beacon readable over hidden maze. Assess combined feedback while moving. |
| 48 | Dense goal | Pending | Pending | Pending | Raised marker is visible above the partly obscured physical hole. Assess whether its stem locates the destination clearly. |
| 50 | Dense final maze | Pending | Pending | Pending | Raised marker visible over dense walls; physical hole unchanged. Assess precision and overall clutter. |

Send results in this compact form: `Level 7 — movement Good; readability Needs adjustment: ...; comfort Good.` Include input method/display conditions where relevant. Android touch/gyro has not been verified on hardware.
