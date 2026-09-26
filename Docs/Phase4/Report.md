# Phase 4 — Gameplay feel and readability candidate

**Implementation and automated verification are complete for this candidate. Human review identified momentum and goal-marker clarity issues, recorded for the next tuning pass. Phase 4 is not accepted or frozen.** Phase 5 has not started.

## A. Baseline versus candidate

The clean baseline was `2b57cc5` on main. The local origin/main tracking ref already matched it at task start; no push was performed by this task. Unity stayed at **6000.3.11f1 (3000ef702840)**. The original repository stayed read-only. [Baseline.md](Baseline.md) was recorded before tuning.

One shared `Assets/Resources/Gameplay Feel.asset` now owns movement, input multipliers and camera response. It loads at startup, not per level. Obsolete scene/prefab movement overrides were removed. The physics model remains target velocity with bounded acceleration; only horizontal velocity is controlled.

| Parameter | Phase 3 Sandbox | Candidate | Reason |
|---|---:|---:|---|
| Target speed (world units/s) | 8 | 9.5 | Modest faster traversal, paired with braking rather than speed alone. |
| Moving response (1/s) | 12 | 12 | Keep prompt acceleration/correction instead of adding sluggish easing. |
| Released-input response (1/s) | 12 | 18 | Shorter drift for precise corrections despite higher travel speed. |
| Acceleration limit (units/s²) | 120 (speed × 15) | 120, explicit | Do not increase peak correction force with top speed. |
| Horizontal safety cap | 15 | 9.5 | Keep collision/external motion bounded by the same shared speed. |
| Velocity-error deadband | 0.05 | 0.02 | Smaller residual drift; explicit stop below deadband on zero input. |
| Keyboard multiplier | 100 | 100 | Retain full keyboard response and axis conventions. |
| Touch multiplier | 0.2 | 1 | Full-axis touch can reach the shared target range; proportional response remains. Requires device judgment. |
| Gyro multiplier | 2 | 2 | Leave device sensitivity/axes pending real-device verification. |
| Linear damping | 0.5 | 0.5 | Preserve existing physical damping. |
| Mass / angular damping / sphere radius | 1 / 0.05 / 0.5 | Unchanged | No collision-size or mass retuning. |
| Rigidbody interpolation | None | Interpolate | Smooth rendering between physics steps. |
| Collision detection | Discrete | ContinuousDynamic | Protect wall contact at the higher target speed. |
| Visual-only tilt maximum | 20° | 6° | Reduce motion and dense-board readability disturbance. Human comfort still pending. |
| Tilt response | 5/s | 5/s | Keep existing smoothing. Profile supports zero tilt. |

Fixed timestep remains 0.02 seconds. Collider materials, gravity, hole animations and all level geometry remain unchanged. Analog input is radially clamped only above magnitude one; nonzero analog input is never normalized to full strength. The integrator bounds each velocity correction at its target to prevent overshoot on unusually large steps.

### Measured comparison, not a human feel rating

The PlayMode test used the actual Sandbox marble/floor and deterministic test input. It replayed the recorded Phase 3 controller (speed 8, response 12, force cap 120, cap 15), then quarter/full candidate input on the same physical rig. It held input for 35 fixed steps, measured release travel over 30 steps, then tested reversal. The comparison uses current interpolation/collision settings for both controllers; it is not a launch of the historical build.

| Sample | Speed after hold | Release distance | Time to reverse velocity sign |
|---|---:|---:|---:|
| Recorded Phase 3 controller, full input | 7.2059 | 0.5309 | 0.08 s |
| Candidate, quarter input | 2.2729 | 0.1265 | 0.06 s |
| Candidate, full input | 8.6452 | 0.4004 | 0.08 s |

Approximately 20% more travel speed and 25% less release drift in this setup. Measured speeds include collision/friction/damping; they are not the configured target. Results do not establish subjective cornering, phone control quality, or campaign difficulty.

## B. Readability changes

- **Unlock:** dark backing plus thicker amber boundary; an inner cyan arc tracks actual two-second attempt progress. Early exit visibly empties it. Completion keeps the full arc, adds a check mark, and changes the boundary. No extra animation delay or mechanic change.
- **Goal:** a small raised world-space ring and stem identify the physical destination above walls. A barred amber ring indicates locked; an open cyan ring indicates valid. Shape reinforces color. Physical trigger and goal position are unchanged. Captures of 48/50 show the marker visible even where the physical hole remains partially obscured.
- **Dark:** the one shared profile changed height 10.8→8, range 38.61→16, outer angle 46.6°→62°, inner angle 4°→32°, intensity 3→1.6. The approximate floor footprint remains about five world units in radius; the bright center is less washed out. Ambient and reflections remain suppressed; most of the maze stays dark. Goals/rings use unlit landmarks. Marble metallic/gloss changed 1/1→0.25/0.5, with modest emission for local visibility.
- **Reveal:** collision enables the wall immediately; a warm outline/tint fades over 0.35 seconds and then stops. No repeated flashing, collision delay, mesh scaling or collider change. Reconfiguration/disable clears transient feedback; attempt state remains authoritative. Initial tint-only feedback proved too subtle in captures and was strengthened with the outline.
- **Camera:** only the existing camera pivot tilts; physical boards stay fixed. Reduced maximum tilt is provisional pending comfort feedback.
- **Audio evaluation:** the existing shared drop cue remains. The assigned clang clip is unused, and dedicated reveal/unlock-completion cues are absent. No new audio routing or placeholder sound reuse was added: collision rate limiting, success/failure distinction, reveal and unlock cues should be auditioned together in a later audio pass. No sound library was sourced.

[Selected exact engine captures](Captures) include ring progress/reset/completion, reveal discovery/settled, Dark combinations and dense goals. Full unedited capture runs remain in the local Phase4 workspace. Images demonstrate visibility, not human comprehension or comfort.

## C. Representative playtest

[Playtest.md](Playtest.md) provides one row for each requested level, observed visual evidence, pending human ratings and launch instructions. The agent inspected representative camera captures and scripted interactions; **human feedback identified abrupt release stopping and a confusing offset marker above Unlock goals**. The user asked to note these for later; no additional tuning was applied after that feedback. Specific level numbers and full movement/readability/comfort ratings were not supplied, so the per-level sheet remains open.

Use **Mirlini → Phase 4 playtest** to select/restart 1, 2, 46, 7, 11, 16, 26, 33, 48 or 50. The selection survives editor domain reload. This is editor-only tooling; no final menu was added.

## D. Tests actually run

Complete unfiltered suites in Unity 6000.3.11f1:

| Suite | Total | Passed | Failed | Skipped | Inconclusive |
|---|---:|---:|---:|---:|---:|
| EditMode | 109 | 109 | 0 | 0 | 0 |
| PlayMode | 24 | 24 | 0 | 0 | 0 |

Evidence: [EditMode.xml](Verification/EditMode.xml), [PlayMode.xml](Verification/PlayMode.xml), [Summary.json](Verification/Summary.json).

Five new EditMode tests protect profile/material validity, proportional analog input and diagonal limits, bounded stopping/vertical preservation, target/reversal limits, and malformed profile rejection. Factory tests now verify the shared profile rather than obsolete hard-coded multiplier expectations.

Three new PlayMode tests cover ring progress/pause/reset/completion/replay and goal presentation, reveal feedback cleanup/collider/attempt persistence, and controlled physical movement including baseline comparison. Existing Phase 1–3 tests still pass, including all-50 setup, traversal, calibration state, help, hole-entry outcome and attempt persistence. No tests were removed or skipped. Tests were rerun after the reveal/material refinements; final XML is retained.

Production level assets/metas and both campaign asset bodies match their pre-phase hashes. No migration or layout change occurred. [UnchangedContent.json](Verification/UnchangedContent.json) records the comparison. The shared Dark profile is intentionally changed and excluded from that unchanged-content claim.

## E. Follow-ups and acceptance boundary

Required before acceptance: address the user-reported abrupt stopping (more believable coast/momentum) and confusing goal-marker offset. The latter is the raised marker's camera projection, not a moved physical hole. Reconsider its placement/presentation as one reusable effect, without per-level geometry changes. These findings are recorded for the next tuning pass at the user's request. Then obtain representative movement/readability/comfort approval, including dense precision, Dark navigation and touch range. Do not freeze timing baselines yet.

Later: Android gyro/touch/device verification; final session flow and timer; reference runner/star calibration; save/progression; full menus/results/selection; Call for Help UI; full audio/effects; accessibility/settings completion; player builds; Steam/Google Play release setup. Higher-speed collision behavior still needs hardware and hands-on maze testing even though integration tests pass.

## F. Changes and commits

- Movement: GameplayFeel profile/resource, MarbleController/Behaviour, input factory, camera tilt, removal of obsolete serialized knobs, marble material.
- World feedback: UnlockRingPresentation, GoalPresentation, WorldFeedback material/helper, RevealWall transition; narrow hookups in existing owners; shared Dark profile inner-angle control.
- Review/verification: GameplayFeelReview editor window, focused EditMode/PlayMode tests, baseline, candidate report, human playtest sheet and evidence.

Local commits: `233dc5b` — Add shared feel tuning and world readability review candidate; followed by **Document Phase 4 evidence and unresolved human feedback** (hash in the final handoff/Git history). Nothing pushed. Original repository untouched.

## G. Google Drive knowledge base

The four existing documents are updated in place with candidate status and human approval still pending: Project Overview and Current State; Design and Technical Reference; Level Mechanics and Migration Reference; Roadmap to Release. Updates preserve historical Phase 3 results and approved mechanic/geometry rules. Both user-reported findings are recorded as unresolved. No document is marked Phase 4 complete. All replacements were verified by connector readback; each original document remains one tab with its existing section structure.

- [Project Overview and Current State](https://docs.google.com/document/d/1EoaJB9SgxPJIWIV4YBwbxvXZVKa26w_4tcHbFNRPdrY)
- [Design and Technical Reference](https://docs.google.com/document/d/14UjYFd_bd2SO5nW4Z6Fq43LYtHZpVtyKTyTK9k9zuw4)
- [Level Mechanics and Migration Reference](https://docs.google.com/document/d/1SmtVjkWFrlqjTQxHQN5Y7wCdMzd9BxzyHYu0agtJe_g)
- [Roadmap to Release](https://docs.google.com/document/d/1iPM4vwNduKHGttxah-GsCwGeLh3ddZqSkG3mvlzHw5k)

## H. Phase 5 recommendation

**Session Flow + Timer** is the next implementation phase only after the representative human review accepts the candidate or identifies and resolves remaining feel/readability problems. Timer/session implementation can then use a documented stable movement reference. Star calibration remains separate and later. Phase 5 has not begun.

Answer to the phase's final question: automated stability and visual evidence are encouraging, but **the human review identified momentum and goal-marker clarity issues, so the candidate is not ready to freeze**.
