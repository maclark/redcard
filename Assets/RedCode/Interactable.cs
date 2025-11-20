using UnityEngine;

namespace RedCard {

    public class Interactable : MonoBehaviour {
        public enum Name {
            Unnamed,
            DoorKnob,
            LockerRoomLightSwitch,
            BathroomMirror,

            Count,
        }

        public Name intName;
        public bool isInteractable = true;

        public static void InteractWith(Interactable interactable) {
            switch (interactable.intName) {
                case Name.DoorKnob:

                    if (interactable.transform.parent.TryGetComponent(out DoorHinge hinge)) {
                        if (hinge.state == DoorHinge.State.Opened || hinge.state == DoorHinge.State.Opening) {
                            print("closing door");
                            hinge.enabled = true;
                            hinge.state = DoorHinge.State.Closing;
                            hinge.doorCollider.enabled = false;
                        }
                        else {
                            print("opening door");
                            hinge.enabled = true;
                            hinge.state = DoorHinge.State.Opening;
                            hinge.doorCollider.enabled = false;
                        }
                    }
                    else Debug.LogWarning(interactable.name + " had no door on its parent!");
                    break;

                case Name.LockerRoomLightSwitch:

                    print("flipping switch");

                    bool turningOn = true;
                    if (interactable.TryGetComponent(out LightSwitch lightSwitch)) {
                        turningOn = !lightSwitch.on;
                        lightSwitch.on = !lightSwitch.on;
                    } 
                    
                    LockerRoom lockerRoom = FindAnyObjectByType<LockerRoom>();
                    if (lockerRoom) {
                        float intensity = turningOn ? 4f : 0f;
                        Color bulbColor = turningOn ? Color.white : Color.gray; 
                        for (int i = 0; i < lockerRoom.ceilingLights.Length; i++) {
                            lockerRoom.ceilingLights[i].intensity = intensity;
                            lockerRoom.ceilingBulbs[i].materials[0].SetColor("_EmissionColor", bulbColor * intensity);
                        }
                    }
                    break;

                case Name.BathroomMirror:
                    if (interactable.TryGetComponent(out BathroomMirror mirror)) {
                        mirror.ApproachMirror(RedMatch.match.arbitro);
                    }
                    else Debug.LogError("mirror missingm mirror");
                    break;


                default:
                    Debug.LogWarning("unhandled interactable " + interactable.name);
                    break;
            }
        }
    }
}
