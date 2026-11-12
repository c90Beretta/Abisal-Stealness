# Deployment armory finding verification

Disposition: **ship for the original F1/F2 finding list only** — F1 resolved; F2 resolved.

This is a bounded follow-up to `docs/UI_REVIEW_ARSENAL.md`, covering only its two material findings. Reviewed the fresh native Unity `Library/ZombtoyValidationClean/TestResults/arsenal-menu.png` at 1280 × 720 and the updated `Assets/_Project/Runtime/Gameplay/ArsenalExpansion.cs`. No implementation edits or new full audit were performed.

| Finding | Status | Evidence and remaining work |
| --- | --- | --- |
| F1 — Persistently identify the three selected options | **resolved** | The second recapture visibly marks Explorador, Arma pesada and Reserva de municion with both `[X]` labels and persistent lighter backgrounds. Unselected choices retain `[ ]`. Arma corta has a separate light outline while remaining unselected, clearly separating navigation focus from the chosen weapon. `Mark()` derives equipped markers/backgrounds from each group's own selection and derives the outline from EventSystem focus. This closes the outstanding marker and focus distinction requirements. |
| F2 — Explain role and kit benefits before deployment | **resolved** | The visible summary states Explorador +1 flashlight battery, shared ammunition and the selected kit's +24 reserve rounds before starting. Source also provides Asalto's base loadout, Sanitario +1 medkit, +1 spare plate and +2 bandages. These match the implemented grants, clarify the spare plate, and fit the concise decision summary. The previous recapture also showed the longer Asalto description without truncating its benefit. |

The original two findings are closed. The supplied capture demonstrates the requested focus/selection distinction; it does not establish an entire manual keyboard traversal session. The parent task reports 27 PlayMode tests passed; their execution was not independently reviewed here and remains separate gameplay evidence. No whole-game approval, performance/FPS claim, alternate-resolution certification, accessibility certification or human aesthetic acceptance is implied.
