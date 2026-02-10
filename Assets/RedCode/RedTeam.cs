using UnityEngine;
using System.Collections.Generic;


namespace RedCard { 

    public enum TeamPosture {
        Attacking,
        Defending,
        WaitingForGK,
        WaitingForOpponentGK,
        BallChasing,
        ThrowingIn,
        DefendingThrowIn,
        // TakingCorner
        // DefendingCorner
        // kicking off/penalty/free kick?
        // WalkingInTunnel
        // WarmingUp
        // Huddling
        // WalkingOff
        // Skirmishing

        Count,
    }

    [System.Flags]
    public enum MatchStatus {
        NotPlaying = 1<<0,
        WaitingForKickoff = 1<<1,
        Playing = 1<<2,
        Freeze = 1<<3,
        Special = 1<<4,
        // could be a lot more!
    }

    public class RedTeam {
        public int id = -1;
        public int goals = -1;
        public string squadName = "unnamed";
        public GoalNet goalNet;
        public RefTarget sixYardBox;
        public FieldEnd attackingEnd;
        public Transform offsideLine;
        public Transform densityPoint;
        public float respect = 1f;
        public List<Jugador> jugadores = new List<Jugador>();

    public void DoTeamwork(
                in float time,
                in float dt,
                in float xFieldEnd,
                in float yFieldEnd,
                in MatchStatus matchStatus,
                in TeamPosture teamPosture,
                in float xDensity,
                in float xOpponentOffsideLine,
                in float xOurOffsideLine,
                RedBall matchBall,
                GoalNet ourGoalNet,
                GoalNet opponentGoalNet,
                in Jugador[] teammates,
                in Jugador[] opponents) { 

            // tacticManager.Run(); // what's it do? why not in line it?

            float ballProgess = Mathf.Abs(goalNet.transform.position.x - matchBall.transform.position.x) / xFieldEnd;

            {
                // here, we could deal with showing/hiding UI
                // names, which is what FS does
            }

            for (int i = 0; i < jugadores.Count; i++) {

                Jugador jug = jugadores[i];

                if (jug == null) {
                    Debug.LogError($"null jugador_{i} for {squadName}");
                    continue;
                }

                if (!jug.controller.isPhysicsEnabled) {
                    jug.controller.Stop(dt / 4f); // stop slowly;
                }

                foreach (Behavior b in jug.behaviors) {
                    b.SetBehavior(
                        jug,
                        time,
                        dt,
                        xFieldEnd,
                        yFieldEnd,
                        matchStatus,
                        teamPosture,
                        xOpponentOffsideLine,
                        xOurOffsideLine,
                        matchBall,
                        goalNet,
                        opponentGoalNet,
                        teammates,
                        opponents
                        );
                }

                jug.Behave(
                    time,
                    dt,
                    xFieldEnd,
                    yFieldEnd,
                    matchStatus,
                    teamPosture,
                    xOpponentOffsideLine,
                    xOurOffsideLine,
                    matchBall,
                    opponentGoalNet,
                    teammates,
                    opponents);

                //if (!jug.controller.isPhysicsEnabled || jug.ballHitAnimationEvent != BallHitAnimationEvent.None) {
                //    continue;
                //}

                if (matchStatus == MatchStatus.Freeze) {
                    jug.controller.Stop(dt);
                    continue;
                }

                // not sure if we'll ever just not allow jugs to move
                //if (jug != matchBall.holder && !RedMatch.match.CanIMove(teamPosture)) {
                //    jug.controller.Stop(dt);
                //    continue;
                //}

                jug.ProcessBehaviors(time);
            }
        }

        // private void lookToBall(in Vector3 ballPosition, Jugador jug)

        // private void PutPlayerToFieldPosition(

        // public void CornerPrePosition(

        // public void PrePositionDensiveKickOff(

        // public void PrePositionOffensiveKickOff(

        // private void KickOffPlayer(

        // public void KeepPlayerBehaviorsForAShortTime(

        // private void UpdateMarkings(
    }
}
