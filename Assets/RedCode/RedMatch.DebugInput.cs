using UnityEngine;
using UnityEngine.InputSystem;

namespace RedCard { 

    // debug input class
    public partial class RedMatch : MonoBehaviour {


        public bool DebugInput() {

            //print("f:" + Time.frameCount);


            //// #DEBUG
            //if (Input.GetKeyDown(KeyCode.F10)) {
            //    Debug.LogWarning("resetting!");
            //    UnityEngine.SceneManagement.SceneManager.LoadScene("_StartingScene"); 
            //    return;
            //}

            DialogWheel w = arbitro.hud.wheel;

            if (Keyboard.current.tabKey.wasPressedThisFrame) {
                if (!Cursor.visible) {
                    print("free looking on");
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    arbitro.canLookAround = false;
                }
                else {
                    print("free looking off");
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    arbitro.canLookAround = true;
                }
            }
            else if (Keyboard.current.yKey.wasPressedThisFrame) {
                w.PopulateBoxes(w.coinFlipWinnerQuestion);
            }
            else if (Keyboard.current.uKey.wasPressedThisFrame) {
                w.PopulateBoxes(w.coinFlipLoserQuestion);
            }
            else if (Keyboard.current.iKey.wasPressedThisFrame) {
                w.PopulateBoxes(w.duringPlay);
            }
            else if (Keyboard.current.oKey.wasPressedThisFrame) {
                w.PopulateBoxes(w.coinFlipExplanation);
            }


            return false;
        }
    }

}
