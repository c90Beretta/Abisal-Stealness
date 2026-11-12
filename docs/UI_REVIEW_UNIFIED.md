# Unified deployment menu review

Disposition: **fix**

Fresh review of deployment-menu legibility and discoverability only, 12 November 2026. Inspected the actual native Unity 1280 × 720 capture at `C:/Users/braya/Documents/GitHub/UNIVERSIDAD/Zombtoy/My project/Library/ZombtoyValidationClean/TestResults/arsenal-menu.png`, with `ArsenalExpansion.cs`, `SurvivorRoster.cs` and the model-builder integration. This capture contains the rendered text and is valid evidence; the earlier LFS-pointer image is excluded. Applied the incumbent PRODUCT.md, DESIGN.md, `.impeccable/industrial-horror.md` and Impeccable craft-floor guidance. This is a fresh finish review, not an invocation of the web critique workflow. No approved raster comp exists and the HTML/CSS detector does not apply to native uGUI.

## Material findings

### U1 — Explain the selected starting kit and character supplies (P1)

The screenshot names `Reserva de municion` twice, but never states its quantity. The summary now describes the character's passive, arsenal and combat keys; it omits the kit benefit previously made visible. `ArsenalExpansion.Update()` grants +24 reserve rounds, +2 bandages or +1 inventory plate. In particular, `Placa reforzada` can still be read as equipped armor rather than a spare inventory item. These are meaningful starting choices whose effects the player cannot compare from this menu.

Restore a concise selected-kit benefit with quantity and destination: +1 spare plate in inventory, +2 bandages, or +24 shared reserve rounds, subject to inventory caps. Make clear that kits apply to a new sortie and do not replenish a continued checkpoint. Keep the selected character's actual starting supplies discoverable too: the current source also grants Viktor a battery and plate, Vance a medkit and dose, and Elena two bandages. Preserve the useful passive description rather than replacing it with supply-only copy. The prior arsenal F2 closure applies to its earlier implementation and does not close this regression.

Verification: inspect all four character and three kit descriptions against their current grants; capture the longest resulting combination at 720p with the benefit text and footer intact.

### U2 — Identify the firearm choice as the initial equipped weapon (P2)

`ARMA PRINCIPAL` suggests a restricted loadout. Its adjacent `TECLAS 1-6` hint does not explicitly tell a first-time player that the six weapons remain available or that ammunition is shared. The current summary only names the chosen firearm. The source equips the selection at the beginning while retaining a six-entry loadout; the previous menu's explicit shared-ammunition explanation is gone.

Use an accurate initial-weapon heading or brief helper copy explaining the six available firearms, their 1–6 switching and shared ammunition. Also label combat shortcuts as in-game controls consistently: the specialist heading already says `EN PARTIDA`, while the numbered firearm options and shortcuts can otherwise be read as menu-selection accelerators.

Verification: the rendered menu should let a first-time player distinguish choosing the initial weapon from using in-game weapon shortcuts without opening help.

## What works in the unified capture

- The four characters, six firearm classes, four melee choices and three kits are visible in distinct groups. The two-column choices fit, and no text is visibly missing, clipped or overlapping the start panel in this capture.
- Persistent `[X]` markers and lighter surfaces identify one choice in every group. The light outline on unselected Arma corta is distinguishable from the selected Arma pesada. The added melee group retains this distinction.
- The character's numeric passive is discoverable immediately below the choices. The current long Viktor description fits above the arsenal and control lines.
- `Comenzar nivel / Enter`, visible help and the right-panel footer make the next action discoverable. Bone text, flat charcoal surfaces and muted ochre selection remain coherent with the industrial direction. No material contrast failure is apparent visually; no measured contrast certification is claimed.

## Scope and closure

Fix U1 and U2, then recapture the modified native menu for bounded verification. No implementation changes were made by this review. The supplied still supports the captured selection/focus state, not a complete keyboard traversal, every character state, hover/pressed behavior, checkpoint behavior, alternative resolution or aspect ratio. The dense 16–17 px operational text merits preserving its current space when restoring benefits; do not shrink it to accommodate more lines.

This disposition grants no whole-game approval. It does not evaluate model quality, gameplay balance, combat, audio, animations, accessibility compliance, hardware performance/FPS, or final human aesthetic acceptance. Model-builder source establishes integration context only; its 3D results are outside this menu review.
