using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using System.Collections.Generic;

namespace RedCard {

    public enum MatchState {
        Unset,
        PreMatchTunnelsAndLocker,
        PreGameWarmup,
        FirstHalf,
        HalfTimeLeavingField,
        HalfTimeTunnelsAndLocker,
        HalfTimeReturningToField,
        SecondHalf,
        PostMatchLeavingField,
        PostMatchTunnelsAndLocker,

        Count,
    }

    public enum Vulgarity {
        MomIsWatching,
        MincedOaths,
        Explicit,
    }


    public enum FieldEnd {
        Unassigned,
        East,
        West,
    }

    public class RedTeam {
        public int id = -1;
        public int goals = -1;
        public string squadName = "unnamed";
        public Transform goalNet;
        public RefTarget sixYardBox;
        public FieldEnd attackingEnd;
        public Transform offsideLine;
        public float respect = 1f;
        public List<Jugador> jugadores = new List<Jugador>();
    }

    public partial class RedMatch : MonoBehaviour {

        [Header("ASSIGNATIONS")]
        public RedSettings settings;
        public CustomizationOptions customizationOptions;
        public Menu menu;
        public GameObject jugadorPrefab;
        public GameObject uiSprayLinePrefab;
        public DevConsole console;
        public PhysicsMaterial jugadorMaterial;
        [Header("SCENE ASSIGNATIONS")]
        public Transform lineHead;
        public RedBall matchBall;
        public Transform field;
        public Transform goalNet0;
        public Transform goalNet1;
        public Transform offsideLineA;
        public Transform offsideLineB;
        public string[] nombres = new string[22];

        [Header("SETTINGS")]
        public bool frozenWaitingForCall = false;
        public float throwInDotThreshold = .33f;

        [Header("VARS")]
        public MatchState matchState = MatchState.Unset;
        public bool paused = false;
        public Transform botLeftBox0;
        public Transform topRightBox0;
        public Transform botLeftBox1;
        public Transform topRightBox1;
        public RedTeam teamA;
        public RedTeam teamB;
        public RefControls arbitro;
        public HUD hud;
        public float clock;
        public float elapsedInState;
        public List<RefTarget> targets = new List<RefTarget>();
        public List<CallData> correctCalls = new List<CallData>();

        internal List<RefTarget> cornerFlags = new List<RefTarget>();
        internal List<RefTarget> sixYardBoxes = new List<RefTarget>();
        internal Dictionary<RefTarget, Jugador> allJugadores = new Dictionary<RefTarget, Jugador>();
        internal Bounds eastBox;
        internal Bounds westBox;

        private bool initialized = false;
        private Vector3 centerSpot;
        private static RedMatch _match;
        private static Vector3 East = Vector3.right;
        private static Vector3 West = -Vector3.right;
        private float xBall;
        private float xCenter;


        public const string REFEREEING_ACTION_MAP = "Refereeing";
        public const string UI_MAP = "UI";
        public const string FLIPPING_BOOK_MAP = "FlippingBook";

        public static RedMatch match {
            get {
                if (!_match)  _match = FindAnyObjectByType<RedMatch>();
                if (!_match) Debug.LogError("can't find redmatch anywhere!");
                return _match;
            }
        }

        private void Awake() {
            if (_match) {
                Debug.LogError("a match already exists!");
                Destroy(gameObject);
            }
            else {
                _match = this;
                Init();
            }
        }

        public void Init() {
            if (initialized) return;
            initialized = true;

            Common.Init();

            Language.current = Language.english;

            console.open = false;
            console.enabled = false;

            initialized = true;
            matchState = MatchState.PreMatchTunnelsAndLocker;
            teamA = new RedTeam();
            teamA.id = 1;
            teamA.squadName = "Los Alamitos";
            teamA.attackingEnd = FieldEnd.East;
            teamA.goalNet = goalNet0;
            teamA.offsideLine = offsideLineA;
            teamB = new RedTeam();
            teamB.id = 2;
            teamB.attackingEnd = FieldEnd.West;
            teamB.squadName = "Somerville";
            teamB.goalNet = goalNet1;
            teamB.offsideLine = offsideLineB;

            Debug.Assert(goalNet0);
            Debug.Assert(goalNet1);
            Debug.Assert(offsideLineA);
            Debug.Assert(offsideLineB);
            Debug.Assert(settings);
            Debug.Assert(matchBall);

            hud = FindFirstObjectByType<HUD>();
            Debug.Assert(hud);

            Debug.Assert(customizationOptions);

            var action = PlayerInput.all[0].actions.FindActionMap(UI_MAP).FindAction("Pause");
            if (action != null) {
                action.started += PauseGame;
            }

            // make them in the hallway

            // i looked at MatchManager.CreateMatch(match details) and then that calls CreateMatch(team1, team2)
            // i've cut out a lot of stuff, but that's where it creates the MatchPlayers
            // which i keep
            //
            // FS has statistics, camera, and scene manager setup i've cut out
            // it also jumps straight to kick off, which i'm cutting out
            // also cut out an event callback to make a UI panel appear maybe, who knows
            // there's a function for sending players to kick off positions
            // another Event callback...
            // and sets GameTeam1 and GameTeam2 to the created teams

            // what does new MatchPlayer do! hmmm.....
            /* ok it assigns PlayerEntry
             * it assigns position
             * it assigns number
             * 
             * it calls setup which assigns skills as integers for strength, dribbling, running, passing, etc
             * using ModifySkill which takes into account their assigned position
             * which is how the formation setting stuff comes into play 
             */


            //var homeTeamMatchPlayers = new MatchPlayer[11];
            //for (int i = 0; i < 11; i++) {
            //    homeTeamMatchPlayers[i] = new MatchPlayer(
            //        i + 1,
            //        matchDetails.homeTeam.Players[i],
            //        11 > i ? homeFormation.Positions[i] : matchDetails.homeTeam.Players[i].Position);
            //}

            // what does MatchTeam do!?!?
            /* it just holds TeamEntry, Formation, MatchPlayer[], TeamTactics, TacticPresetType, using home or away kit, AI Level
             * 
             * 
             * 
             */
            //var homeMatchTeam = new MatchTeam() {
            //    Players = homeTeamMatchPlayers,
            //    Team = matchDetails.homeTeam,
            //    Formation = matchDetails.homeTeam.Formation,
            //    TeamTactics = homeTactics,
            //    Kit = matchDetails.homeKitSelection,
            //    AILevel = details.userTeam == MatchCreateRequest.UserTeam.Home ? AILevel.Legendary : details.aiLevel
            //};

            //var awayTeamMatchPlayers = new MatchPlayer[11];

            //for (int i = 0; i < 11; i++) {
            //    awayTeamMatchPlayers[i] = new MatchPlayer(
            //        i + 1,
            //        matchDetails.awayTeam.Players[i],
            //        11 > i ? awayFormation.Positions[i] : matchDetails.awayTeam.Players[i].Position);
            //}

            //var awayMatchTeam = new MatchTeam() {
            //    Players = awayTeamMatchPlayers,
            //    Team = matchDetails.awayTeam,
            //    Formation = matchDetails.awayTeam.Formation,
            //    TeamTactics = awayTactics,
            //    Kit = matchDetails.awayKitSelection,
            //    AILevel = details.userTeam == MatchCreateRequest.UserTeam.Away ? AILevel.Legendary : details.aiLevel
            //};
            //

            // we are the center ref! 
            // need to make the linesmen
            // need to place all players

            // "SetTeam"
            // took in MatchTeam which has info about kit and formation
            // created PlayerBases and fed kit and positional info

            // starting players, not worrying about bench 

            allJugadores.Clear();
            teamA.jugadores.Clear();
            teamB.jugadores.Clear();

            if (nombres.Length != 22) {
                Debug.LogWarning("not 22 nombres!");
                nombres = new string[22];
            }

            string[] givenNames = new string[22];
            string[] surnames = new string[22];
            for (int i = 0; i < nombres.Length; i++) {
                string nombre = nombres[i];
                int firstSpace = nombre.IndexOf(' ');
                if (firstSpace < 0 || firstSpace >= nombre.Length - 1) {
                    Debug.LogError("missing name part: " + nombre);
                    givenNames[i] = nombres[i];
                    surnames[i] = nombres[i];
                }
                else {
                    givenNames[i] = nombre.Substring(0, firstSpace);
                    surnames[i] = nombre.Substring(firstSpace + 1, nombre.Length -  (firstSpace + 1));
                }
                print($"{givenNames[i]}, {givenNames[i].Length}");
                print($"{surnames[i]}, {surnames[i].Length}");
            }

            if (!lineHead) {
                Debug.LogError("missing lineHead!");
                lineHead = new GameObject().transform;
            }

            // starters
            float lineGap = 1.5f;
            int linePos = 0;
            float lateralShift = 0f;
            for (int i = 0; i < 22; i++) {
                Jugador jugador = new Jugador();
                jugador.id = i + 1;
                jugador.givenName = givenNames[i];
                jugador.surname = surnames[i];
                if (i < 11) {
                    linePos = i;
                    jugador.isGoalie = i == 0;
                    jugador.team = teamA;
                    lateralShift = -1f;
                }
                else {
                    linePos = i - 11;
                    jugador.isGoalie = i == 11;
                    jugador.team = teamB;
                    lateralShift = 1f;
                }
                jugador.team.jugadores.Add(jugador);

                jugador.controller = Instantiate(jugadorPrefab, 
                    lineHead.position - lineHead.forward * linePos * lineGap + lineHead.right * lateralShift, 
                    lineHead.rotation)
                    .GetComponent<JugadorController>();

                // see CodeBasedController.SetPlayer
                // #TODO player variety:
                // skipping the height/weight calculations
                // vary collider sizes
                // collider height reflects jumping
                // collider width reflects weight (goalies get boost)
                // vary body types, skipping visual scaling
                // shadow?, skipping take a shadow

                jugador.controller.name = $"{jugador.surname}({jugador.team.squadName})";
                jugador.controller.gameObject.layer = LayerMask.NameToLayer(Tags.JUGADORES_LAYER);
                jugador.controller.rb.mass = 20f; // maps directly to jugador strength, ranges from 20 to 25 maybe
                jugador.controller.rb.isKinematic = true;
                //jugador.graphics....kit, number, name, and goalies special?
                //jugador.anim.SetFloat(JugadorAnimatorVariable.Agility, jugador.agility / 100f);
                //jugador.controller.CollisionEnterEvent = jugador.OnCollisionEnter;
                jugador.controller.isPhysicsEnabled = false;
                jugador.controller.capsule.material = jugadorMaterial;

                // #TODO goalkeepers are special

                allJugadores.Add(jugador.controller.target, jugador);
            }

            // need to place ball (are we going to have many balls?)
            // could just be placed in scene

            float x1 = goalNet0.position.x;// RedSim.Goal1Pos.x;
            float x2 = goalNet1.position.x;// RedSim.Goal2Pos.x;
            centerSpot = new Vector3((x2 - x1) / 2f, 0f, 0f); 
            // team1 is defined as attacking East
            // if team1's goal is farther in +x-axis,
            // then it is attacking -x-axis
            East = x1 > x2 ? -Vector3.right : Vector3.right;
            West = -East;

            // make sure the goals are spread along the correct
            Debug.Assert(Mathf.Abs(x1 - x2) > 10f);

            print(teamA.squadName + " is TeamA_" + teamA.id + " is GameTeam1, attacking " + teamA.attackingEnd + ", " + EndDir(teamA.attackingEnd));
            print(teamB.squadName + " is TeamB_" + teamB.id + " is GameTeam2, attacking " + teamB.attackingEnd + ", " + EndDir(teamB.attackingEnd));

            //Match.currentMatchBall = current.MatchBall.gameObject;

            //RedSim.MakeRedPlayers(teamA);
            //RedSim.MakeRedPlayers(teamB);

            // line up players in tunnel

            // #LINESMEN
            // #COACH
            // #PHYSIO
            // #BENCH
            // #INVADER
            // need to register these other RedPlayers

            foreach (var target in targets) {
                switch (target.targetType) {
                    case TargetType.CornerFlag:
                        target.attackingEnd = NearestEnd(target.transform.position);
                        cornerFlags.Add(target);
                        break;
                    case TargetType.SixYardBox:
                        if (target.attackingEnd == FieldEnd.Unassigned) {
                            Debug.LogWarning("unassigned attacking end on six yard box");
                        }

                        sixYardBoxes.Add(target);
                        if (target.attackingEnd == teamA.attackingEnd) teamA.sixYardBox = target;
                        else if (target.attackingEnd == teamB.attackingEnd) teamB.sixYardBox = target;

                        EighteenYardBox box = target.GetComponentInChildren<EighteenYardBox>();
                        Debug.Assert(box && box.botLeft && box.topRight);
                        box.botLeft.gameObject.SetActive(false);
                        box.topRight.gameObject.SetActive(false);

                        Bounds b = NearestEnd(target.transform.position) == FieldEnd.East ? eastBox : westBox;
                        Vector3 boxCenter = (box.topRight.position - box.botLeft.position) / 2f;
                        Vector3 boxSize = new Vector3(box.topRight.position.x - box.botLeft.position.x,
                            5f,
                            box.topRight.position.z - box.botLeft.position.z);
                        b = new Bounds(boxCenter, boxSize);
                        break;

                    case TargetType.PenaltySpot:
                        float x = target.transform.position.x;
                        if (x > centerSpot.x) {
                            if (East.x > 0f) target.attackingEnd = FieldEnd.East;
                            else target.attackingEnd = FieldEnd.West;
                        }
                        else {
                            if (East.x > 0f) target.attackingEnd = FieldEnd.West;
                            else target.attackingEnd = FieldEnd.East;
                        }
                        break;
                }
            }

            Debug.Assert(teamA.goalNet);
            Debug.Assert(teamB.goalNet);
        }

        private float CalculateOffside(float xNet, List<Jugador> jugadores) {
            float signGoal = Mathf.Sign(xNet - xCenter);
            float last = xCenter;
            float secondToLast = xCenter;
            for (int i = 0; i < jugadores.Count; i++) {
                float xJugador = jugadores[i].controller.transform.position.x;
                if (Mathf.Sign(xJugador) == signGoal) {
                    float toJugador = Mathf.Abs(xJugador - xCenter);
                    if (toJugador > last) {
                        secondToLast = last;
                        last = toJugador;
                    }
                    else if (toJugador > secondToLast) {
                        secondToLast = toJugador;
                    }
                }
            }
            if (Mathf.Sign(xBall) == signGoal) {
                float toBall = Mathf.Abs(xBall - xCenter);
                if (toBall > secondToLast) secondToLast = toBall;
            }
            return xCenter + secondToLast * signGoal;
        }

        public void LateUpdate() {

            // calculate some basic stuff to help everyone behave :)
            xBall = matchBall.transform.position.x;
            xCenter = field.transform.position.x;
            teamA.offsideLine.SetX(CalculateOffside(teamA.goalNet.position.x, teamA.jugadores));
            Debug.Assert(teamB.offsideLine);
            Debug.Assert(teamB.goalNet);
            Debug.Assert(teamB.jugadores != null);
            teamB.offsideLine.SetX(CalculateOffside(teamB.goalNet.position.x, teamB.jugadores));

            float dt = Time.deltaTime;
            float time = Time.time;

            elapsedInState += dt;

            switch (matchState) {
                case MatchState.PreMatchTunnelsAndLocker:
                    break;
                case MatchState.PreGameWarmup:
                    break;
                case MatchState.FirstHalf:
                    clock += dt;
                    break;
                case MatchState.HalfTimeLeavingField:
                    break;
                case MatchState.HalfTimeTunnelsAndLocker:
                    break;
                case MatchState.HalfTimeReturningToField:
                    break;
                case MatchState.SecondHalf:
                    clock += dt;
                    break;
                case MatchState.PostMatchLeavingField:
                    break;
                case MatchState.PostMatchTunnelsAndLocker:
                    break;

                default:
                    Debug.LogError("unhandled match state: " + matchState);
                    break;
            }

            //teamA.Behave();
            //teamB.Behave();

            // foreach (Linesman l in linesmen) l.Behave();
        }


        public static void SetQuality(string levelName) {
            int index = QualitySettings.GetQualityLevel();
            int targetIndex = System.Array.IndexOf(QualitySettings.names, levelName);
            if (targetIndex >= 0) {
                QualitySettings.SetQualityLevel(targetIndex, true);
                Debug.Log($"Switched to quality: {levelName}");
            }
            else Debug.LogError("couldn't find quality level " + levelName);
        }

        private static string active_input_map;
        public static string GetActiveInputMap() => active_input_map;
        public static void AssignInputMap(string mapName) {
            ReadOnlyArray<PlayerInput> allInput = PlayerInput.all;
            foreach (PlayerInput input in allInput) {
                InputActionMap map = input.actions.FindActionMap(mapName);
                if (map != null) {
                    input.SwitchCurrentActionMap(mapName);
                    active_input_map = mapName;
                }
                else Debug.LogError("can't find map: " + mapName);
            }
        }

        public void Save() {
            Debug.LogWarning("implement saving game!");
        }

    }
}