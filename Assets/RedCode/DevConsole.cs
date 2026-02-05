using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


namespace RedCard {

    public class DevConsole : MonoBehaviour {

        public bool open = false;
        public GUISkin skin;
        public Rect textField;

        private string input = "";
        private string oldInputMap;
        private Dictionary<string, System.Action<string[]>> commands;


        // this should also free cursor/tab, but maybe remember tab steting? idk

        void Awake() {

            commands = new Dictionary<string, System.Action<string[]>>();

            commands["warp"] = args =>
            {
                float x = float.Parse(args[0]);
                float y = float.Parse(args[1]);
                float z = float.Parse(args[2]);
            };

            commands["reset"] = args =>
            {
                Debug.Log("Resetting game state");
            };
        }


        public void Open() {
            enabled = true;
            open = true;

            oldInputMap = RedMatch.GetActiveInputMap();
            RedMatch.AssignInputMap("Console");
        }

        private void Update() {
            if (Keyboard.current.backquoteKey.wasPressedThisFrame) {
                enabled = false;
                open = false;
                input = "";
                RedMatch.AssignInputMap(oldInputMap);
            }
        }


        private void OnGUI() {
            if (!open) return;

            GUI.SetNextControlName("ConsoleInput");
            input = GUI.TextField(textField, input, skin.textField);

            GUI.FocusControl("ConsoleInput");

            // i guess Return doesn't have a KeyDown event type? only Up?
            // it has a type of Layout and then it also complains that Use shouldn't be *used* with Layout type
            if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp) {
                Execute(input);
                input = "";
                Event.current.Use();
            }
        }

        private void Execute(string line) {

            string[] parts = line.Split(' ');
            string cmd = parts[0].ToLower();
            string[] args = parts.Length > 1 ? parts[1..] : new string[0];

            if (commands.TryGetValue(cmd, out System.Action<string[]> action)) {
                action(args);
            }
            else Debug.Log($"unknown commands: {cmd}");
        }

    }
}
