# Unified deployment finding verification

Disposition: **ship for U1/U2 only** — U1 resolved; U2 resolved.

Bounded follow-up to `docs/UI_REVIEW_UNIFIED.md`, 12 November 2026. Inspected the latest native Unity 1280 × 720 `C:/Users/braya/Documents/GitHub/UNIVERSIDAD/Zombtoy/My project/Library/ZombtoyValidationClean/TestResults/arsenal-menu.png` and updated `Assets/_Project/Runtime/Gameplay/ArsenalExpansion.cs`. This verifies the original two findings only; it is not a new audit. No implementation edits were made by this review.

| Finding | Status | Evidence |
| --- | --- | --- |
| U1 — Explain the selected starting kit and character supplies | **resolved** | The capture now shows Viktor's +1 battery and +1 plate in the backpack, subject to limits, together with his passive. The selected kit explicitly adds +24 shared rounds to the backpack, subject to limits, once per sortie. The source also defines the other kits as +1 spare plate and +2 bandages, and character supplies as base loadout, +1 medkit/+1 serum, or +2 bandages. Those descriptions match the current grant branches. The five-line summary preserves passive and arsenal information and fits above the footer without visible clipping. The once-per-sortie language identifies the kit as a starting grant; checkpoint execution itself was not verified visually. |
| U2 — Identify the firearm choice as the initial equipped weapon | **resolved** | The visible heading now says `ARMA INICIAL / LLEVAS LAS SEIS [1-6 / T EN PARTIDA]`. The summary explicitly says `reserva compartida`. Specialist and melee headings also mark their keys as in-game controls. The choice's initial-equipment purpose, six-weapon availability and shared reserve can now be understood before opening help. |

The capture retains four marked choices and the separate focus outline on unselected Arma corta. It contains actual rendered text and is valid evidence. No approved comp or HTML detector applies to this native uGUI follow-up.

Limits: this capture shows Viktor, Arma pesada, Cuchillo KM-9 and Reserva de municion. Other character/kit descriptions were checked in source, not independently captured in every permutation. It does not demonstrate complete manual keyboard traversal, alternate resolutions, hover/pressed behavior, gameplay effects or continued-checkpoint behavior. The parent reports a passing filtered integration test and 18 EditMode tests after the copy changes, and 27 PlayMode tests before them; those executions are separate functional evidence and were not independently rerun by this reviewer.

This bounded ship disposition closes U1/U2 only. It grants no whole-game, model, combat, audio, animation, accessibility, hardware/FPS or human aesthetic approval.
