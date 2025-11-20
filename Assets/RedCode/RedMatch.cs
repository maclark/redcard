using System;
using UnityEngine;
using System.Collections.Generic;

namespace RedCard {

    public enum State {
        Unset,
        PreMatchTunnelsAndLocker,
        PreMatchField,
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

    // we need match jobs
    // calling fouls/missing fouls
    // then we classify fouls with anger levels and shit
    public enum Call {
        Unspecified,
        StartMatch,
        HalfTime,
        EndMatch,
        Foul,
        Yellow, // this would include flopping
        Red,
        ThrowIn,
        GoalKick,
        CornerKick,
        PenaltyKick,
        Advantage, // how do we remember the need to go back and give yellow?
        GoalScored,
    }

    [System.Serializable]
    public class CallData {
        public Call call; // call that should be made
        public bool timedOut = false;
        public bool made = false;
        public bool missed = false;
        public int objectIndex;

        public RefTarget target;
        public RedTeam team;

        // coach, linesman, bench, invader, could all be player
        public RedPlayer victim; 
        public RedPlayer perpetrator; 
        public float occurredAt;
        public Vector3 location;
    }

    public enum FieldEnd {
        Unassigned,
        East,
        West,
    }

    public class RedTeam {
        public int id = 0;
        public int goals = 0;
        public string squadName = "unnamed";
        public RefTarget goal;
        public FieldEnd attackingEnd;
        public float respect = 1f;
    }

    public class RedPlayer {
        public int id = 0;
        public string firstName = "firstName";
        public string surname = "lastName";
        public RedTeam team;
        public float anger = 0f;
        public AngerBar angerBar;
    }

    public partial class RedMatch : MonoBehaviour {

        internal RedSettings settings;
        public CustomizationOptions customizationOptions;
        public State state = State.Unset;
        public bool frozenWaitingForCall = false;
        public MainMenu menu;

        public float throwInDotThreshold = .33f;
        public Transform botLeftBox0;
        public Transform topRightBox0;
        public Transform botLeftBox1;
        public Transform topRightBox1;
        public GameObject playerOutlinePrefab;
        public GameObject uiSprayLinePrefab;
        public RedTeam teamA;
        public RedTeam teamB;
        public RefControls arbitro;
        public HUD hud;
        public List<RefTarget> targets = new List<RefTarget>();
        public List<CallData> correctCalls = new List<CallData>();

        internal List<RefTarget> cornerFlags = new List<RefTarget>();
        internal List<RefTarget> sixYardBoxes = new List<RefTarget>();
        internal Dictionary<RefTarget, RedPlayer> allPlayers = new Dictionary<RefTarget, RedPlayer>();
        internal Bounds eastBox;
        internal Bounds westBox;

        private bool initialized = false;
        private Vector3 centerSpot;
        private static RedMatch _match;
        private static Vector3 East = Vector3.right;
        private static Vector3 West = -Vector3.right;


        public static Action<Semantics, RefTarget> OnRefSpoke;

        public const string REFEREEING_ACTION_MAP = "Refereeing";

        public static RedMatch match {
            get {
                if (!_match)  _match = FindAnyObjectByType<RedMatch>();
                if (!_match) Debug.LogError("can't find redmatch anywhere!");
                return _match;
            }
        }



        private void Awake() {
            if (_match) {
                Debug.LogError("a meatch already exists!");
                Destroy(gameObject);
            }
            else {
                _match = this;
                Init();
            }
        }

        public void Init() {
            if (!initialized) {

                Language.current = Language.english;

                initialized = true;
                state = State.PreMatchTunnelsAndLocker;
                teamA = new RedTeam();
                teamA.id = 1;
                teamA.squadName = "Arrows";
                teamA.attackingEnd = FieldEnd.East;
                teamB = new RedTeam();
                teamB.id = 2;
                teamB.attackingEnd = FieldEnd.West;
                teamB.squadName = "Bulls";

                settings = Resources.Load<RedSettings>("RedSettings");
                Debug.Assert(settings);

                hud = FindFirstObjectByType<HUD>();
                Debug.Assert(hud);

                Debug.Assert(customizationOptions);
            }
        }

        public static void InitFS() {

            //Debug.Assert(current.GameTeam1);
            //Debug.Assert(current.GameTeam2);
            //Debug.Assert(current.GameTeam1 != current.GameTeam2);

            if (match) {
                float x1 = RedSim.Goal1Pos.x;
                float x2 = RedSim.Goal2Pos.x;
                match.centerSpot = new Vector3((x2 - x1) / 2f, 0f, 0f); 
                // team1 is defined as attacking East
                // if team1's goal is farther in +x-axis,
                // then it is attacking -x-axis
                East = x1 > x2 ? -Vector3.right : Vector3.right;
                West = -East;

                // make sure the goals are spread along the correct
                Debug.Assert(Mathf.Abs(x1 - x2) > 10f);

                //Match.mm = current;
                //Match.teamA.fsTeam = current.GameTeam1;
                match.teamA.id = RedSim.Team1Id;
                match.teamA.squadName = RedSim.SquadName(match.teamA.id) + "_A";
                //Match.teamB.fsTeam = current.GameTeam2;
                match.teamB.id = RedSim.Team2Id;
                match.teamB.squadName = RedSim.SquadName(match.teamB.id) + "_B";

                if (match.teamA.squadName == match.teamB.squadName) {
                    Debug.LogWarning("c'mon, same team is playing each other...");
                }

                print(match.teamA.squadName + " is TeamA_" + match.teamA.id + " is GameTeam1, attacking " + match.teamA.attackingEnd + ", " + EndDir(match.teamA.attackingEnd));
                print(match.teamB.squadName + " is TeamB_" + match.teamB.id + " is GameTeam2, attacking " + match.teamB.attackingEnd + ", " + EndDir(match.teamB.attackingEnd));

                //Match.currentMatchBall = current.MatchBall.gameObject;

                RedSim.MakeRedPlayers(match.teamA);
                RedSim.MakeRedPlayers(match.teamB);
                
                // #LINESMEN
                // #COACH
                // #PHYSIO
                // #BENCH
                // #INVADER
                // need to register these other RedPlayers

                foreach (var target in match.targets) {
                    switch (target.targetType) {
                        case TargetType.CornerFlag:
                            target.attackingEnd = NearestEnd(target.transform.position);
                            match.cornerFlags.Add(target);
                            break;
                        case TargetType.SixYardBox:
                            if (target.attackingEnd == FieldEnd.Unassigned) {
                                Debug.LogWarning("unassigned attacking end on six yard box");
                            }

                            match.sixYardBoxes.Add(target);
                            if (target.attackingEnd == match.teamA.attackingEnd) match.teamA.goal = target;
                            else if (target.attackingEnd == match.teamB.attackingEnd) match.teamB.goal = target;

                            EighteenYardBox box = target.GetComponentInChildren<EighteenYardBox>();
                            Debug.Assert(box && box.botLeft && box.topRight);
                            box.botLeft.gameObject.SetActive(false);
                            box.topRight.gameObject.SetActive(false);

                            Bounds b = NearestEnd(target.transform.position) == FieldEnd.East ? match.eastBox : match.westBox;
                            Vector3 boxCenter = (box.topRight.position - box.botLeft.position) / 2f;
                            Vector3 boxSize = new Vector3(box.topRight.position.x - box.botLeft.position.x,
                                5f,
                                box.topRight.position.z - box.botLeft.position.z);
                            b = new Bounds(boxCenter, boxSize);
                            break;

                        case TargetType.PenaltySpot:
                            float x = target.transform.position.x;
                            if (x > match.centerSpot.x) {
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

                Debug.Assert(match.teamA.goal);
                Debug.Assert(match.teamB.goal);



            }
            else Debug.LogError("no RedMatch.Match?");
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

        internal static void ProcessEmotions(RedPlayer player) {
            //#TODO
        }

        internal static void Angered(RedPlayer player, float amount, float teamAmount) {
            Angered(player.team, teamAmount);
            float total = amount / player.team.respect;
            player.anger += total;
            player.angerBar.SetFill(player.anger / match.settings.maxAnger);
            if (amount < 0f) print($"{player.surname} soothed to {player.anger}({total}={amount}/{player.team.respect})");
            else print($"{player.surname} angered to {player.anger}({total}={amount}/{player.team.respect})");
        }

        internal static void Angered(RedTeam team, float amount) {
            float total = amount / team.respect;
            if (amount < 0) print($"{team.squadName} soothed {amount}");
            else print($"{team.squadName} angered {amount}");
            foreach(var item in match.allPlayers) {
                if (item.Value.team == team) {
                    item.Value.anger += amount;
                    item.Value.angerBar.SetFill(item.Value.anger / match.settings.maxAnger);
                    ProcessEmotions(item.Value);
                }
            }
        }

        internal static void LoseRespect(RedTeam team, float amount) {
            team.respect -= amount;
            // if we're dividing by respect, must worry about division by zero
            if (team.respect <= 0.1f) team.respect = .1f;
        }

        internal static void RestoreRespect(RedTeam team, float amount) {
            team.respect += amount;
            if (team.respect > 1f) team.respect = 1f;
        }


        internal static void CallHalftime() {
            var cachedGoal = match.teamA.goal;
            var cachedEnd = match.teamA.attackingEnd;
            match.teamA.goal = match.teamB.goal;
            match.teamA.attackingEnd = match.teamB.attackingEnd;
            match.teamB.goal = cachedGoal;
            match.teamB.attackingEnd = cachedEnd;

            print("made call: halftime");
        }

        public static void TrackCorrectCall(CallData data) {
            data.occurredAt = Time.time;
            match.correctCalls.Add(data);
        }

        public static bool InTheBox(Vector3 pos, out FieldEnd end) {
            bool inABox = false;
            end = FieldEnd.Unassigned;
            if (match.eastBox.Contains(pos)) {
                inABox = true;
                end = NearestEnd(pos);
            }
            else if (match.westBox.Contains(pos)) {
                inABox = true;
                end = NearestEnd(pos);
            }
            else inABox = false;
            return inABox;
        }

        internal static FieldEnd NearestEnd(Vector3 pos) {
            FieldEnd end;
            if (East.x > 0) {
                if (pos.x > match.centerSpot.x) end = FieldEnd.East;
                else end = FieldEnd.West;
            }
            else {
                if (pos.x > match.centerSpot.x) end = FieldEnd.West;
                else end = FieldEnd.East;
            }

            print(pos + " nearest end: " + end);
            return end;
        }

        internal static RedTeam WhoseAttackingEnd(Vector3 pos) {
            if (match.teamA.attackingEnd == NearestEnd(pos)) return match.teamA;
            else return match.teamB;
        }

        internal static RedTeam WhoseDefensiveEnd(Vector3 pos) {
            return OtherTeam(WhoseAttackingEnd(pos));
        }

        internal static RedTeam OtherTeam(RedTeam team0) {
            if (match.teamA == team0) return match.teamB;
            else return match.teamA;
        }

        internal static RedTeam WhoseAttackingEnd(FieldEnd end) {
            if (match.teamA.attackingEnd == end) return match.teamA;
            else return match.teamB;
        }

        public static void WhistleBlown(float duration) {

            // track whistle duration to have an effect emotionally on players?
            // or maybe baby whistles for half time are respect lossy

            if (match.frozenWaitingForCall) { 
                // #TODO confusion, loss of respect, unless breaking up a fight
            }
            else {
                FSInterpreter.BlowWhistle();
            }

            print($"whistle blown for {duration}!");
        }

        public static void ShowYellowCard(RefTarget target) {
            if (target) {
                string targetName = target.name;
                Debug.LogWarning("implement yellow card!");
                print("made call: showed yellow to " + targetName);
            }
        }

        public static void ShowRedCard(RefTarget target) {

            if (target) {
                string targetName = target.name;
                switch (target.targetType) {
                    case TargetType.Player:
                    case TargetType.Coach:
                    case TargetType.Fan:
                        Debug.LogWarning("implementing showing card to " + target.targetType);
                        break;
                    default:
                        Debug.LogError("trying to give card to what?");
                        break;
                }
                print("made call: showed red to " + targetName);
            }
        }

        public static void IndicateCall(RefTarget target, Vector3 indicateDir) {
            // #TODO maybe rename this as MakeCall that returns a CallData
            // and then call AssessCall on that

            // #TODO targetting something after targetting and indicating!
            //if (Match.mm && Match.mm.MatchFlags != FStudio.MatchEngine.Enums.MatchStatus.Freeze) {
            if (FSInterpreter.MatchIsFrozen()) { 
                // #RESPECT
                print("what is ref doing indicating during play? (might be ok, if he's saying 'no foul' or 'play on'");
            }
            else if (target == null) {

                if (match.frozenWaitingForCall) {
                    print("footballers no longer frozenWaitingForCall");
                }
                match.frozenWaitingForCall = false;

                // do we check if expecting throw in?
                float eastward = Vector3.Dot(East, indicateDir);
                float westward = Vector3.Dot(West, indicateDir);
                if (eastward > match.throwInDotThreshold) IndicateThrowIn(FieldEnd.East);
                else if (westward > match.throwInDotThreshold) IndicateThrowIn(FieldEnd.West);
                else {
                    // confusion?
                    print("bad aim for calling throw in? eastward: " + eastward + ", westward: " + westward);
                }
            }
            else {
                switch (target.targetType) {
                    case TargetType.Player:
                        // #TODO position of foul could called..hmmm
                        Vector3 pos = RedSim.CurrentBallPos;
                        IndicateNormalFoul(target, pos);
                        break;
                    case TargetType.SixYardBox:
                        IndicateGoalKick(target);
                        break;
                    case TargetType.CornerFlag:
                        IndicateCornerKick(target);
                        break;
                    case TargetType.PenaltySpot:
                        IndicatePenalty(target);
                        break;
                    case TargetType.CenterCircle:
                        FieldEnd scoredAt = (RedSim.CurrentBallPos.x > target.transform.position.x) ? FieldEnd.East : FieldEnd.West;
                        print("scoredAt: " + scoredAt);
                        if (match.teamA.attackingEnd == scoredAt) IndicateGoalScoredBy(match.teamA);
                        else IndicateGoalScoredBy(match.teamB);
                        break;
                    default:
                        Debug.LogWarning("unhandled targettype: " + target.targetType);
                        break;
                }
            }
        }

        public static Vector3 EndDir(FieldEnd end) {
            return end == FieldEnd.East ? East : West;
        }

        public static Vector3 OppositeEndDir(FieldEnd end) {
            return -EndDir(end);
        }

        internal static void IndicateThrowIn(FieldEnd end) {

            RedTeam throwingTeam = WhoseAttackingEnd(end);
            FSInterpreter.ThrowIn(throwingTeam);

            print("made call: throw in awarded to " + throwingTeam.squadName);
            CallData madeCall = new CallData();
            madeCall.call = Call.ThrowIn;
            madeCall.team = throwingTeam;
            AssessCall(madeCall);
        }

        internal static void IndicateNormalFoul(RefTarget target, Vector3 fieldPos) {
            RedPlayer p = match.allPlayers[target];
            RedTeam awardedTeam = OtherTeam(p.team);
            FSInterpreter.Foul(awardedTeam, fieldPos);

            print($"made call: free kick awarded to {awardedTeam.squadName}, foul commited by {p.surname}");
            CallData madeCall = new CallData();
            madeCall.call = Call.Foul;
            madeCall.target = target;
            madeCall.perpetrator = FSInterpreter.GetRedPlayer(target);
            AssessCall(madeCall);
        }


        public static void IndicateGoalKick(RefTarget target) {

            RedTeam kickingTeam = WhoseAttackingEnd(target.attackingEnd);

            FSInterpreter.GoalKick(kickingTeam);

            CallData madeCall = new CallData();
            madeCall.call = Call.GoalKick;

            print("made call: goal kick awarded to " + kickingTeam.squadName); 
            AssessCall(madeCall);
        }

        private static void AssessCall(CallData madeCall) {

            // if pre-game, assessment must be done differently
            switch (match.state) {
                case State.FirstHalf:
                case State.SecondHalf:
                    AssessCallDuringMatch(madeCall);
                    break;

                default:
                    AssessCallOutsideOfMatch(madeCall);
                    break;

            } 
        }

        private static void AssessCallOutsideOfMatch(CallData madeCall) {
            // hmm tbd

        }

        private static void AssessCallDuringMatch(CallData madeCall) { 

            // assess call
            // ...can go through tracked calls in last ... 10 seconds?
            // if one matches well, voila
            // if not, then it gets dubious
            // if match isplaying, this is absurd, unless we do borderline stuff

            bool madeCorrectCall = false;
            CallData correctCall = new CallData();
            for (int i = match.correctCalls.Count - 1; i >= 0; --i) {
                CallData call = match.correctCalls[i];

                if (call.made) {
                    print("skipping made call " + correctCall.call); 
                    continue;
                }


                float lateness = madeCall.occurredAt - correctCall.occurredAt;
                bool callWasLate = lateness > match.settings.maxCallLateness;
                /*
                if (correctCall.timedOut) break;

                float lateness = madeCall.occurredAt - correctCall.occurredAt;
                if (lateness > Match.settings.maxCallLateness) {
                    correctCall.timedOut = true;
                    break;
                }
                */

                if (madeCall.call == call.call) {
                    print("made call matched a tracked call: " + madeCall.call);
                    switch (call.call) {
                        case Call.GoalKick:
                        case Call.CornerKick:
                        case Call.Red:
                        case Call.Yellow:
                        case Call.Foul:
                            madeCorrectCall = call.target == madeCall.target;
                            break;
                        case Call.ThrowIn:
                        case Call.PenaltyKick:
                        case Call.GoalScored:
                            madeCorrectCall = call.team == madeCall.team;
                            break;
                        default:
                            Debug.LogWarning("unhandled call assessment for " + madeCall.call);
                            break;
                    }
                }


                if (madeCorrectCall) {
                    call.made = true;
                    correctCall = call;
                    break;
                }
            }

            // ok, now we know if correct call was made or not
            var sets = match.settings;
            CallData closestCorrectCall = new CallData();
            switch (madeCall.call) {
                case Call.Foul:
                    if (madeCorrectCall) {
                        // player fouled should calm anger and (restore respect that was pending loss?)
                        // team awarded foul should be slightly calmed, respect pending loss is restored
                        // team called should be slightly angered
                        // player called for foul more angered
                        correctCall.victim.anger += sets.angerFouled * correctCall.victim.team.respect;
                        correctCall.perpetrator.anger += sets.angerCalledForFoul / correctCall.perpetrator.team.respect;


                        // restore injured team's respect fully (we're ignoring speed of call atm)
                        // soothe injured player and team somewhat
                        RestoreRespect(correctCall.victim.team, sets.respectLostWrongFoul);
                        Angered(correctCall.victim, sets.soothedGotCall, sets.teamSoothedGotCall);

                        // make perpetrate a little angry and his team a little angry
                        Angered(correctCall.perpetrator, sets.angerCalledForFoul, sets.teamAngerCalledForFoul);
                    }
                    break;
                case Call.GoalKick:
                case Call.CornerKick:
                case Call.Red:
                case Call.Yellow:
                    madeCorrectCall = correctCall.target == madeCall.target;
                    break;
                case Call.ThrowIn:
                case Call.PenaltyKick:
                case Call.GoalScored:
                    madeCorrectCall = correctCall.team == madeCall.team;
                    break;
                default:
                    Debug.LogWarning("unhandled call assessment for " + correctCall.call);
                    break;
            }

            if (madeCorrectCall) {
                Debug.Log("yay! correct call!");
                match.hud.ShowCorrectCall();
            }
            else {
                // uh oh, bad call
                Debug.Log("uh oh! bad call!");
                match.hud.ShowBadCall();
                switch (madeCall.call) {

                    case Call.Foul:
                        // falsely called gets v angry
                        // his team gets angry
                        // both teams lose respect
                        Angered(madeCall.perpetrator, sets.angerWronglyCalledForFoul, sets.teamAngerWronglyCalledForFoul);
                        LoseRespect(madeCall.perpetrator.team, sets.respectLostWrongFoul);
                        LoseRespect(OtherTeam(madeCall.perpetrator.team), sets.respectLostWrongFoul);
                        break;

                    default:
                        Debug.LogWarning("unhandled reaction to bad call: " + madeCall.call);
                        if (madeCall.perpetrator != null) {
                            Angered(madeCall.perpetrator, sets.angerWronglyCalledForFoul, sets.teamAngerWronglyCalledForFoul);
                        }
                        else {
                            Angered(match.teamA, sets.teamAngerWronglyCalledForFoul);
                            Angered(match.teamB, sets.teamAngerWronglyCalledForFoul);
                        }
                        LoseRespect(match.teamA, sets.respectLostWrongFoul);
                        LoseRespect(match.teamB, sets.respectLostWrongFoul);
                        break;

                }
            }
        }

        public static void IndicateCornerKick(RefTarget target) {

            RedSim.Corner();
            RedTeam kickingTeam = WhoseAttackingEnd(target.attackingEnd);
            print("made call: corner called for " + kickingTeam.squadName);
            CallData madeCall = new CallData();
            madeCall.call = Call.CornerKick;
            madeCall.target = target;
            AssessCall(madeCall);
        }

        public static void IndicateGoalScoredBy(RedTeam team) {
            team.goals++;

            RedSim.GoalScored(team);

            print($"made call: {team.squadName}({team.id}) scored");
            CallData madeCall = new CallData();
            madeCall.call = Call.GoalScored;
            madeCall.team = team;
            AssessCall(madeCall);
        }

        public static void IndicatePenalty(RefTarget target) {
            RedTeam shootingTeam = WhoseAttackingEnd(target.attackingEnd);
            Vector3 penaltySpot = target.transform.position;
            penaltySpot.y = 0f;

            RedSim.Penalty(penaltySpot, shootingTeam);

            Debug.LogWarning("penalty kicks not implemented");
            print("made call: penalty awarded to " + shootingTeam.squadName);
            CallData madeCall = new CallData();
            madeCall.call = Call.PenaltyKick;
            madeCall.team = shootingTeam;
            AssessCall(madeCall);
        }

        public static void LineSprayed(List<Vector3> sprayLine, int segments=100) {
            if (sprayLine.Count < 2) return;

            List<Vector3> smoothedLine = FoamBlob.SmoothLine(sprayLine, segments);
            LineRenderer lr = Instantiate(match.uiSprayLinePrefab, smoothedLine[0], Quaternion.identity).GetComponent<LineRenderer>();

            lr.positionCount = smoothedLine.Count;
            lr.SetPositions(smoothedLine.ToArray());
        }


    }
}