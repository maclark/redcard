using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using System.Collections.Generic;

namespace RedCard {

    public enum WorldStatus {
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

    public partial class RedMatch : MonoBehaviour {

        // assigned in prefab
        [Header("ASSIGNATIONS")]
        public RedSettings settings;
        public CustomizationOptions customizationOptions;
        public Menu menu;
        public GameObject jugadorPrefab;
        public GameObject uiSprayLinePrefab;
        public DevConsole console;
        public PhysicsMaterial jugadorMaterial;

        // assigned in scene, overriding prefab
        [Header("SCENE ASSIGNATIONS")]
        public Transform lineHead;
        public RedBall matchBall;
        public Transform field;
        public Transform endlineLeft;
        public Transform endlineRight;
        public Transform sidelineTop; 
        public Transform sidelineBot; 
        public GoalNet goalNet0;
        public GoalNet goalNet1;
        public Transform offsideLineA;
        public Transform offsideLineB;
        public Transform densityPointA; 
        public Transform densityPointB; 
        public string[] nombres = new string[22];

        [Header("SETTINGS")]
        public bool frozenWaitingForCall = false;
        public float throwInDotThreshold = .33f;

        [Header("VARS")]
        public WorldStatus matchState = WorldStatus.Unset;
        public MatchStatus matchStatus = MatchStatus.NotPlaying;
        public bool paused = false;
        public Transform botLeftBox0;
        public Transform topRightBox0;
        public Transform botLeftBox1;
        public Transform topRightBox1;
        public RedTeam losAl;
        public RedTeam somerville;
        public RefControls arbitro;
        public HUD hud;
        public float clock;
        public float elapsedInState;
        public List<RefTarget> targets = new List<RefTarget>();
        public List<CallData> correctCalls = new List<CallData>();

        [Header("CHECKPOINTS")]
        public bool takenBallOffPodium = false;

        internal List<RefTarget> cornerFlags = new List<RefTarget>();
        internal List<RefTarget> sixYardBoxes = new List<RefTarget>();
        internal Dictionary<RefTarget, Jugador> allJugadores = new Dictionary<RefTarget, Jugador>();
        internal Bounds eastBox;
        internal Bounds westBox;

        private bool initialized = false;
        private Vector3 centerSpot;
        private Vector2 fieldSize;
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

            float fieldLength = endlineRight.position.x - endlineLeft.position.x;
            float fieldWidth = sidelineTop.position.y - sidelineBot.position.y;
            fieldSize = new Vector2(fieldLength, fieldWidth);

            matchState = WorldStatus.PreMatchTunnelsAndLocker;
            losAl = new RedTeam();
            losAl.id = 1;
            losAl.squadName = "Los Alamitos";
            losAl.attackingEnd = FieldEnd.East;
            losAl.goalNet = goalNet0;
            losAl.goalNet.name = $"goalNet({losAl.squadName})";
            losAl.offsideLine = offsideLineA;
            losAl.offsideLine.name = $"offsideLine({losAl.squadName})";
            losAl.densityPoint = densityPointA;
            losAl.densityPoint.name = $"densityPoint({losAl.squadName})";
            somerville = new RedTeam();
            somerville.id = 2;
            somerville.attackingEnd = FieldEnd.West;
            somerville.squadName = "Somerville";
            somerville.goalNet = goalNet1;
            somerville.goalNet.name = $"goalNet({somerville.squadName})";
            somerville.offsideLine = offsideLineB;
            somerville.offsideLine.name = $"offsideLine({somerville.squadName})";
            somerville.densityPoint = densityPointB;
            somerville.densityPoint.name = $"densityPoint({somerville.squadName})";

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
            losAl.jugadores.Clear();
            somerville.jugadores.Clear();

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
                    jugador.team = losAl;
                    lateralShift = -1f;
                }
                else {
                    linePos = i - 11;
                    jugador.isGoalie = i == 11;
                    jugador.team = somerville;
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

            float x1 = goalNet0.transform.position.x;
            float x2 = goalNet1.transform.position.x;
            centerSpot = new Vector3((x2 - x1) / 2f, 0f, 0f); 
            // team1 is defined as attacking East
            // if team1's goal is farther in +x-axis,
            // then it is attacking -x-axis
            East = x1 > x2 ? -Vector3.right : Vector3.right;
            West = -East;

            // make sure the goals are spread along the correct
            Debug.Assert(Mathf.Abs(x1 - x2) > 10f);

            print(losAl.squadName + " is TeamA_" + losAl.id + " is GameTeam1, attacking " + losAl.attackingEnd + ", " + EndDir(losAl.attackingEnd));
            print(somerville.squadName + " is TeamB_" + somerville.id + " is GameTeam2, attacking " + somerville.attackingEnd + ", " + EndDir(somerville.attackingEnd));

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
                        if (target.attackingEnd == losAl.attackingEnd) losAl.sixYardBox = target;
                        else if (target.attackingEnd == somerville.attackingEnd) somerville.sixYardBox = target;

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
        }

        private float CalculateOffside(float xNet, List<Jugador> jugadores) {
            float signGoal = Mathf.Sign(xNet - xCenter);
            float deepest = 0f;
            float secondDeepest = 0f;
            for (int i = 0; i < jugadores.Count; i++) {
                float xJugador = jugadores[i].controller.transform.position.x;
                float toJugador = xJugador - xCenter;
                if (Mathf.Sign(toJugador) == signGoal) {
                    float jugadorDepth = Mathf.Abs(toJugador);
                    if (jugadorDepth > deepest) {
                        secondDeepest = deepest;
                        deepest = jugadorDepth;
                    }
                    else if (jugadorDepth > secondDeepest) {
                        secondDeepest = jugadorDepth;
                    }
                }
            }
            float toBall = xBall - xCenter;
            if (Mathf.Sign(toBall) == signGoal) {
                float ballDepth = Mathf.Abs(toBall);
                if (ballDepth > secondDeepest) secondDeepest = ballDepth;
            }
            return xCenter + secondDeepest * signGoal;
        }

        // i'm not surprised, but i don't know why FS uses LATEUpdate
        public void LateUpdate() {

            // calculate some basic stuff to help everyone behave :)
            xBall = matchBall.transform.position.x;
            xCenter = field.transform.position.x;
            losAl.offsideLine.SetX(CalculateOffside(losAl.goalNet.transform.position.x, losAl.jugadores));
            somerville.offsideLine.SetX(CalculateOffside(somerville.goalNet.transform.position.x, somerville.jugadores));

            float dt = Time.deltaTime;
            float time = Time.time;

            elapsedInState += dt;

            float caculateDensity(RedTeam team) {
                if (team.jugadores.Count == 0) return 0f; ///////////////
                float xTotal = 0f;
                for (int i = 0; i < team.jugadores.Count; i++) {
                    Jugador jug = team.jugadores[i];
                    if (!jug.isGoalie) {
                        xTotal += jug.controller.transform.position.x;
                    }
                }
                return xTotal / team.jugadores.Count;
            }

            float losAlDensity = Mathf.Lerp(caculateDensity(losAl), xBall, .5f);
            float somervilleDensity = Mathf.Lerp(caculateDensity(somerville), xBall, .5f);
            losAl.densityPoint.position = new Vector3(caculateDensity(losAl), 0f, fieldSize.y / 2f);
            somerville.densityPoint.position = new Vector3(caculateDensity(somerville), 0f, fieldSize.y / 2f);


            switch (matchState) {
                case WorldStatus.PreMatchTunnelsAndLocker:
                    break;
                case WorldStatus.PreGameWarmup:
                    break;
                case WorldStatus.FirstHalf:
                    clock += dt;
                    break;
                case WorldStatus.HalfTimeLeavingField:
                    break;
                case WorldStatus.HalfTimeTunnelsAndLocker:
                    break;
                case WorldStatus.HalfTimeReturningToField:
                    break;
                case WorldStatus.SecondHalf:
                    clock += dt;
                    break;
                case WorldStatus.PostMatchLeavingField:
                    break;
                case WorldStatus.PostMatchTunnelsAndLocker:
                    break;

                default:
                    Debug.LogError("unhandled match state: " + matchState);
                    break;
            }

            TeamPosture losAlPosture;
            TeamPosture somervillePosture;
            if (matchBall.holder == null) {
                losAlPosture = TeamPosture.BallChasing;
                somervillePosture = TeamPosture.BallChasing;
            }
            else {
                if (matchBall.holder.team == losAl) {
                    if (matchBall.holder.isGoalie) {
                        losAlPosture = TeamPosture.WaitingForGK;
                        somervillePosture = TeamPosture.WaitingForOpponentGK;
                    }
                    else {
                        losAlPosture = TeamPosture.Attacking;
                        somervillePosture = TeamPosture.Defending;
                    }
                }
                // somerville is possessing
                else {
                    if (matchBall.holder.isGoalie) {
                        losAlPosture = TeamPosture.WaitingForOpponentGK;
                        somervillePosture = TeamPosture.WaitingForGK;
                    }
                    else {
                        losAlPosture = TeamPosture.Defending;
                        somervillePosture = TeamPosture.Attacking;
                    }
                }
            }

            losAl.DoTeamwork(
                in time,
                in dt,
                in fieldSize.x,
                in fieldSize.y,
                in matchStatus,
                in losAlPosture,
                in losAlDensity,
                losAl.offsideLine.position.x,
                somerville.offsideLine.position.x,
                matchBall,
                losAl.goalNet,
                somerville.goalNet,
                losAl.jugadores.ToArray(),
                somerville.jugadores.ToArray());

            somerville.DoTeamwork(
                in time,
                in dt,
                in fieldSize.x,
                in fieldSize.y,
                in matchStatus,
                in somervillePosture,
                in somervilleDensity,
                somerville.offsideLine.position.x,
                losAl.offsideLine.position.x,
                matchBall,
                somerville.goalNet,
                losAl.goalNet,
                somerville.jugadores.ToArray(),
                losAl.jugadores.ToArray());

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