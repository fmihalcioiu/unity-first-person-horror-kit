# First-Person Horror Kit

First-person player, camera, flashlight and interaction scripts for horror games in Unity.

## What's inside

- **Player:** `PlayerController` moves a `CharacterController` with walking, sprinting and gravity, and slides along walls instead of sticking to them.
- **Camera:** `MouseLook` gives smoothed look with a slight tilt when strafing and a slow hand shake. `HeadBob` adds head bob and idle breathing, damped near walls. `CameraZoom` zooms while the right mouse button is held, lowers look sensitivity while zoomed and ends with a short focus pulse.
- **Flashlight:**
  - `Flashlight` toggles with a short fade.
  - `FlashlightRealism` adds a small flicker in brightness and beam angle.
  - `FlashlightSwaySmooth` makes the flashlight model trail the camera slightly.
- **Interaction:** `InteractionController` uses the nearest interactable under the crosshair. `Crosshair` changes colour and shows the prompt. `InteractionDialog` asks for confirmation before an interaction when the target wants one. `SoftwareCursor` draws a cursor on a Canvas.
- **Interactables:** `Door`, `LightSwitch` and `Pickup`, plus the `IInteractable` and `IConfirmableInteractable` interfaces for your own objects.

## Requirements

- Unity 6000.4 or newer.
- TextMeshPro, which comes with `com.unity.ugui` and is installed as a dependency.
- **Active Input Handling** set to **Input Manager (Old)** or **Both** (Project Settings > Player > Other Settings). The scripts read input through the `Input` class.

## Install

In the Package Manager, choose **+ > Install package from git URL** and enter:

```
https://github.com/fmihalcioiu/unity-first-person-horror-kit.git
```

Or add it to `Packages/manifest.json`:

```json
"com.first-person-horror-kit": "https://github.com/fmihalcioiu/unity-first-person-horror-kit.git"
```

## Player setup

This layout lets the scripts find most of their references on their own:

```
Player            CharacterController, PlayerController
└── Camera        Camera, MouseLook, HeadBob, CameraZoom, InteractionController
    └── Flashlight      Flashlight, FlashlightRealism, FlashlightSwaySmooth
        └── Spot Light  Light
```

- **MouseLook:** **Player Body** is the Player object. It is filled in automatically when you add MouseLook to a direct child of the Player.
- **InteractionController:** **Ray Origin** defaults to its own object, the camera. Set **Interact Layer** to the layers your interactables are on. To ask before an interaction, assign an **Interaction Dialog**.
- **FlashlightSwaySmooth:** assign **Flashlight Model** and **Player Camera**.
- **CameraZoom:** to move the flashlight model out of view while zoomed, assign **Flashlight Zoom Root**.
- **Audio:** `Door` and `LightSwitch` play their sounds through an `AudioSource` on their own object. The other scripts with sounds have an **Audio Source** field, which defaults to a source on their own object.

## UI setup

- **Crosshair:** on a Canvas. Assign the crosshair **Image**, a TextMeshPro **Prompt Text** and the **Player Camera**. It runs its own raycast, so match its **Interact Distance** and **Interact Mask** to the InteractionController's.
- **InteractionDialog:** on a Canvas. Assign the **Root** panel, the message text, the two buttons and their texts.
- **SoftwareCursor (optional):** on the same Canvas, with a cursor image. When one is active, the dialog hides the system cursor and this one follows the mouse.

## Interactables

Every interactable needs a non-trigger collider on a layer included in **Interact Layer**.

- **Door:** needs an `Animator` with **Open** and **Close** states on its first layer, and a `BoxCollider` on the same object. The collider is turned off while the door animates. A door can be locked. **Animate Additional** opens and closes a second object with it: another `Door`, such as the other half of a double door, or any `Animator` with **Open** and **Close** states.
- **LightSwitch:** assign the **Target Lights**, the **Emissive Renderers** whose emission turns on and off with them, and the **Switch Child** to rotate. `SetIsOn(bool, bool)` switches it from code, and **Switch Id** names it for your own scripts.
- **Pickup:** give it a **Pickup Interaction Profile**, which holds the prompt, the confirmation text and the button labels. Create one from **Create > com.first_person_horror_kit > Interaction > Pickup Interaction Profile**.
  - **On Picked Up** runs when the item is taken, and the pickup then destroys its own GameObject.
  - For the pickup sound, assign an **Audio Source** on another object. A source on the pickup itself is destroyed with it, which cuts the sound off.

## Your own interactables

```csharp
using com.first_person_horror_kit.Core;
using UnityEngine;

public class Radio : MonoBehaviour, IInteractable
{
    public void Interact() { /* turn it on */ }
    public string GetPrompt() => "[E] USE";
    public string GetDisplayName() => "Radio";
}
```

Add `IConfirmableInteractable` to make the dialog ask first. Return `null` from `GetConfirmationPrompt()` to skip the dialog for that interaction.

## Blocking input

`GameplayState.SetInputBlocked(true)` pauses movement, look, head bob, zoom, the flashlight and interaction, for example while a menu is open. Call it again with `false` to give control back. The dialog does this on its own.

## Default keys

All keys can be changed in the Inspector.

| Action | Key |
|---|---|
| Move | Input Manager axes **Horizontal** and **Vertical** |
| Look | Input Manager axes **Mouse X** and **Mouse Y** |
| Sprint | Left Shift |
| Interact | E |
| Flashlight | F |
| Zoom | Right mouse button |
| Dialog: confirm / cancel | Enter / Escape |
| Dialog: next / previous option | Right, Down or Tab / Left or Up |

## Licence

MIT, see [LICENSE.md](LICENSE.md).
