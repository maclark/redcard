using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

namespace RedCard { 
    

    ///////// RedMatch.Refereeing
    public partial class RedMatch : MonoBehaviour 
    {

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

        [Serializable]
        public class CallData {
            public Call call; // call that should be made
            public bool timedOut = false;
            public bool made = false;
            public bool missed = false;
            public int objectIndex;

            public RefTarget target;
            public RedTeam team;

            // coach, linesman, bench, invader, could all be player
            public Jugador victim; 
            public Jugador perpetrator; 
            public float occurredAt;
            public Vector3 location;
        }

        public static Action<Semantics, RefTarget> OnRefSpoke;
        
        internal static void ProcessEmotions(Jugador player) {
            //#TODO
        }

        internal static void Angered(Jugador player, float amount, float teamAmount) {
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
            foreach(var item in match.allJugadores) {
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
            var cachedGoal = match.losAl.ourGoal;
            var cachedEnd = match.losAl.attackingEnd;
            match.losAl.ourGoal = match.somerville.ourGoal;
            match.losAl.attackingEnd = match.somerville.attackingEnd;
            match.somerville.ourGoal = cachedGoal;
            match.somerville.attackingEnd = cachedEnd;

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
            if (match.losAl.attackingEnd == NearestEnd(pos)) return match.losAl;
            else return match.somerville;
        }

        internal static RedTeam WhoseDefensiveEnd(Vector3 pos) {
            return OtherTeam(WhoseAttackingEnd(pos));
        }

        internal static RedTeam OtherTeam(RedTeam team0) {
            if (match.losAl == team0) return match.somerville;
            else return match.losAl;
        }

        internal static RedTeam WhoseAttackingEnd(FieldEnd end) {
            if (match.losAl.attackingEnd == end) return match.losAl;
            else return match.somerville;
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

        public void IndicateCall(RefTarget target, Vector3 indicateDir) {
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
                        IndicateNormalFoul(target, matchBall.transform.position);
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
                        FieldEnd scoredAt = (matchBall.transform.position.x > target.transform.position.x) ? FieldEnd.East : FieldEnd.West;
                        print("scoredAt: " + scoredAt);
                        if (match.losAl.attackingEnd == scoredAt) IndicateGoalScoredBy(match.losAl);
                        else IndicateGoalScoredBy(match.somerville);
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
            Jugador p = match.allJugadores[target];
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
            switch (match.matchState) {
                case WorldStatus.FirstHalf:
                case WorldStatus.SecondHalf:
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
                            Angered(match.losAl, sets.teamAngerWronglyCalledForFoul);
                            Angered(match.somerville, sets.teamAngerWronglyCalledForFoul);
                        }
                        LoseRespect(match.losAl, sets.respectLostWrongFoul);
                        LoseRespect(match.somerville, sets.respectLostWrongFoul);
                        break;

                }
            }
        }

        public static void IndicateCornerKick(RefTarget target) {

            //RedSim.Corner();
            RedTeam kickingTeam = WhoseAttackingEnd(target.attackingEnd);
            print("made call: corner called for " + kickingTeam.squadName);
            CallData madeCall = new CallData();
            madeCall.call = Call.CornerKick;
            madeCall.target = target;
            AssessCall(madeCall);
        }

        public static void IndicateGoalScoredBy(RedTeam team) {
            team.goals++;

            //RedSim.GoalScored(team);

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

            //RedSim.Penalty(penaltySpot, shootingTeam);

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

        public void PauseGame(InputAction.CallbackContext ctx) {
            if (menu.title) {
                // no pausing at title screen duh
                return;
            }

            if (menu.gameObject.activeSelf) {
                print("unpausing");
                paused = false;
                Time.timeScale = 1f;
                menu.gameObject.SetActive(false);
                AudioManager.PlaySFXOneShot(menu.unpausedSound);
                // hmm need to do stuff
            }
            else {
                print("pausing");
                paused = true;
                Time.timeScale = 0f;
                menu.gameObject.SetActive(true);
                menu.OpenTopLevelMenu();
                AudioManager.PlaySFXOneShot(menu.pausedSound);
            }
        }

    }
}
