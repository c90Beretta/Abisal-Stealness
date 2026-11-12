# Deployment armory UI review

Disposition: **fix**

Reviewed the native Unity composed capture `Library/ZombtoyValidationClean/TestResults/arsenal-menu.png` at its actual 1280 × 720 size, together with `Assets/_Project/Runtime/Gameplay/ArsenalExpansion.cs` and the relevant `GameHud.cs` controls. Direction remains the incumbent industrial horror / isometric PS1 world in PRODUCT.md, DESIGN.md and `.impeccable/industrial-horror.md`. Applied the mechanical review guidance from `impeccable/reference/craft-floor.md` within this native Unity scope. No approved raster comp exists; no comp comparison or HTML detector applies.

## Material findings

### F1 — Persistently identify the three selected options

The capture shows every role, weapon and kit button with the same surface and text treatment. The current values can be recovered from the summary, but a player must match that text back to three separate groups. `Button()` only attaches the assignment callback; it does not retain controls or update their presentation from `role`, `selected` or `upgrade`. Unity's focused/selected button state represents navigation focus and cannot represent three independent equipped choices.

Add a persistent selected treatment for one option in each group, with a visible text marker as well as a surface/border change. Keep navigation focus distinguishable from equipped state. Verification: change all three groups, move focus away, and capture the screen with the three actual choices still clearly marked; include keyboard focus evidence.

### F2 — Explain role and kit benefits before deployment

The headings and option names are legible, but they do not explain the decision. “Explorador” and “Sanitario” do not reveal their starting benefits; “Placa reforzada” implies armor being worn while the code grants an inventory plate. “Suministros medicos” grants two bandages, and “Reserva de municion” grants 24 reserve rounds. Those quantities and whether equipment is granted or already applied are absent. The summary repeats the names instead of explaining their effects. A player cannot make an informed starting selection from this screen.

Show concise accurate benefits for the selected role and kit, including quantities and whether supplies go into inventory. Describe Asalto honestly if it has no extra benefit. Preserve the shared ammunition explanation. Verification: review the real text for each role and kit and capture the longest combination without wrapping into the footer or truncation.

## What the evidence supports

At 720p the two panels fit within the frame without visible overlap or clipped copy. The title, grouped role/weapon/kit controls and left-panel start action are easy to locate. All six weapon classes are visible at once. The short footer points toward the existing start action, and the selection summary clearly names the chosen weapon. Warm light text on near-black surfaces fits the existing HUD direction; no material low-contrast text issue is apparent in this capture. This is visual observation, not a measured contrast certification.

## Limits and closure

This is a review of deployment UI legibility and function discoverability only. The supplied still and source do not establish successful mouse/keyboard operation, focus traversal, hover/pressed states, checkpoint behavior, 1080p or alternate-aspect layout, or the effects of each selection in play. Recheck those interactions and recapture the modified menu after F1/F2. No implementation was changed in this review.

This disposition neither approves the whole game nor evaluates enemy art, combat/audio quality, animation, hardware performance, FPS, accessibility compliance, or the user's final aesthetic acceptance.
