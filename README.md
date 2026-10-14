# First-Person Horror Kit

Unity scripts for a first-person horror game: the player controller with footsteps that change with the ground, camera feel, a flashlight, and interaction with doors, light switches and pickups.

## Contents

| Script | What it does |
|---|---|
| `PlayerController` | Walks and sprints on a `CharacterController`, with gravity. Slides along walls it runs into. |
| `Footsteps` | Plays a step sound at a set pace while the player walks or sprints. The clip depends on the terrain layer or Physics Material underfoot. |
| `FootstepProfile` | The asset that holds the step clips. Each set has walk and sprint clips and belongs to a Physics Material or to terrain layers, with default clips for any other ground. |
| `MouseLook` | Smoothed mouse look with a slight tilt when strafing and a slow hand shake. |
| `HeadBob` | Bobs the camera while walking and adds a slow breathing motion when standing still. The bob is weaker right in front of a wall. |
| `CameraZoom` | Zooms while the right mouse button is held and lowers look sensitivity while zoomed. A short focus pulse plays when the zoom settles. |
| `Flashlight` | Turns the light on and off with a short fade. |
| `FlashlightRealism` | Lets the brightness and beam angle drift slightly while the light is on. |
| `FlashlightSwaySmooth` | Makes the flashlight model lag a little behind the camera. |
| `InteractionController` | Uses the nearest interactable under the crosshair. |
| `Crosshair` | Changes colour over an interactable and shows its prompt. |
| `InteractionDialog` | Asks for confirmation before an interaction, when the target wants that. |
| `SoftwareCursor` | Draws the mouse cursor as an image on a Canvas. |
| `Door` | Opens and closes through an `Animator`. Can be locked. |
| `LightSwitch` | Turns lights and emissive materials on and off. |
| `Pickup` | An item the player can take, with an optional confirmation. |
| `GameplayState` | Blocks player input while a menu or dialog is open. |

## Requirements

Unity 6000.4 or newer. TextMeshPro comes with `com.unity.ugui`, which the package lists as a dependency.

Set Active Input Handling to `Input Manager (Old)` or `Both` in Project Settings > Player > Other Settings. The scripts read input through the old `Input` class.

## Install

In the Package Manager, choose + > Install package from git URL and enter:

```
https://github.com/fmihalcioiu/unity-first-person-horror-kit.git
```

Or add this line to `Packages/manifest.json`:

```json
"com.first-person-horror-kit": "https://github.com/fmihalcioiu/unity-first-person-horror-kit.git"
```

## Player setup

Laid out like this, the scripts find most of their references on their own:

```
Player            CharacterController, PlayerController, Footsteps, AudioSource
└── Camera        Camera, MouseLook, HeadBob, CameraZoom, InteractionController
    └── Flashlight      Flashlight, FlashlightRealism, FlashlightSwaySmooth
        └── Spot Light  Light
```

- `MouseLook`: `Player Body` is filled in when you add the component to a direct child of the player.
- `InteractionController`: `Ray Origin` defaults to the camera it sits on. Set `Interact Layer` to the layers your interactables use, and assign an `Interaction Dialog` if some interactions should ask first.
- `FlashlightSwaySmooth`: assign `Flashlight Model` and `Player Camera` by hand.
- `CameraZoom`: assign `Flashlight Zoom Root` to move the flashlight model out of view while zoomed.
- `Footsteps`: assign a `Profile`. Steps play through the `AudioSource` on the player unless you assign another one in `Footstep Source`.

The package has no sound files. Create a footstep profile from Create > com.first_person_horror_kit > Audio > Footstep Profile and fill it with your own clips.

`Door` and `LightSwitch` play their sounds through an `AudioSource` on their own object. The other scripts with sounds have a field for their `AudioSource`, which defaults to one on their own object.

## UI setup

`Crosshair` goes on a Canvas. Assign its `Crosshair Image` and a TextMeshPro `Prompt Text`; `Player Camera` defaults to the main camera. It runs its own raycast, so give it the same `Interact Distance` and `Interact Mask` as the `InteractionController`.

`InteractionDialog` also goes on a Canvas. It needs a `Root` panel holding the message text and two buttons, each with a label.

`SoftwareCursor` is optional and goes on the same Canvas with a cursor image. While it is active, the dialog hides the system cursor and this one follows the mouse.

## Interactables

Each interactable needs a collider that is not a trigger, on a layer included in `Interact Layer`.

`Door` needs an `Animator` with `Open` and `Close` states on its first layer, and a `BoxCollider` on the same object. The collider turns off while the door is moving. A door can be locked. `Animate Additional` moves a second object along with it: another `Door`, such as the other half of a double door, or any `Animator` with `Open` and `Close` states.

`LightSwitch` needs its `Target Lights` and the `Emissive Renderers` whose emission follows them. `Switch Child` is the part that rotates when the switch flips. Call `SetIsOn(bool, bool)` to switch it from code. `Switch Id` lets your own scripts tell switches apart.

`Pickup` takes a `Pickup Interaction Profile` asset for its prompt and the wording of its confirmation dialog. Create one from Create > com.first_person_horror_kit > Interaction > Pickup Interaction Profile. `On Picked Up` runs when the item is taken, and then the pickup destroys its GameObject. Put the pickup sound's `AudioSource` on another object, because one on the pickup is destroyed with it and the sound cuts off.

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

Implement `IConfirmableInteractable` as well to have the dialog ask first. Returning `null` from `GetConfirmationPrompt()` skips the dialog for that interaction.

## Blocking input

`GameplayState.SetInputBlocked(true)` stops the kit's scripts from reading input, for example while a menu is open. Pass `false` to give control back. The dialog does this itself.

## Default keys

Every key can be changed in the Inspector.

| Action | Key |
|---|---|
| Move | Input Manager axes `Horizontal` and `Vertical` |
| Look | Input Manager axes `Mouse X` and `Mouse Y` |
| Sprint | Left Shift |
| Interact | E |
| Flashlight | F |
| Zoom | Right mouse button |
| Dialog: confirm / cancel | Enter / Escape |
| Dialog: next / previous option | Right, Down or Tab / Left or Up |

## Licence

MIT. See [LICENSE.md](LICENSE.md).
