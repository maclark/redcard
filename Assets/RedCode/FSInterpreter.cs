using UnityEngine;
//using FStudio.MatchEngine;
//using FStudio.MatchEngine.Players;

namespace RedCard {
    public static class FSInterpreter {

        private static RedMatch Match => RedMatch.match;

        //public static RedTeam GetTeam(PlayerBase player) {
        //    RedTeam team = null;

        //    if (Match.teamA.fsTeam == player.GameTeam) team = Match.teamA;
        //    else if (Match.teamB.fsTeam == player.GameTeam) team = Match.teamB;
        //    else {
        //        Debug.LogError("playerbase doesn't have team");
        //        team = new RedTeam();
        //    }

        //    return team;
        //}

        //public static RedPlayer GetRedPlayer(PlayerBase playerBase) {
        //    return GetRedPlayer(playerBase.PlayerController.UnityObject.GetComponentInChildren<RefTarget>());
        //}
        public static Jugador GetRedPlayer(RefTarget target) {
            return RedMatch.match.allJugadores[target];
        }

        //public static void HandleKickOff(MatchManager mm) {

        //    if (RedMatch.Match.state < State.FirstHalf) {
        //        Debug.Log("first half kickoff");
        //        RedMatch.Match.state = State.FirstHalf;
        //    }
        //    else if (RedMatch.Match.state == State.HalfTimeLeavingField ||
        //        RedMatch.Match.state == State.HalfTimeReturningToField ||
        //        RedMatch.Match.state == State.HalfTimeReturningToField) {
        //        RedMatch.Match.state = State.SecondHalf;
        //        Debug.Log("second half kickoff");
        //    }
        //    else Debug.LogWarning("kickoff from what state? " + RedMatch.Match.state);
        //}

        //public static void HandleTackleAsFoul(MatchManager mm, PlayerBase tackler, PlayerBase victim) {

        //    CallData call = new CallData();
        //    call.call = Call.Foul;
        //    call.perpetrator = GetRedPlayer(tackler); 
        //    call.target = tackler.target;
        //    call.team = GetTeam(victim);
        //    call.victim = GetRedPlayer(victim);

        //    // penalty check!
        //    Vector3 pos = tackler.PlayerController.UnityObject.transform.position;
        //    if (RedMatch.InTheBox(pos, out var end)) {
        //        if (call.team.attackingEnd == end) call.call = Call.PenaltyKick;
        //    }

        //    RedMatch.TrackCorrectCall(call);
        //    Debug.Log($"correct call: {call.call} by {tackler.MatchPlayer.Player.Name} on {tackler.GameTeam.Team.Team.TeamName}({tackler.GameTeam.TeamId})");
        //}

        //public static void HandleGoalEvent(MatchManager mm, Vector3 ballPos) {
        //    CallData call = new CallData();
        //    call.call = Call.GoalScored;
        //    call.team = RedMatch.WhoseAttackingEnd(ballPos);
        //    RedMatch.TrackCorrectCall(call);
        //    Debug.Log($"correct call: goal for {call.team.squadName}({call.team.fsTeam.Team.Team.TeamName}{call.team.fsTeam.TeamId})");

        //    mm.MatchFlags = FStudio.MatchEngine.Enums.MatchStatus.Freeze;
        //    Match.frozenWaitingForCall = true;
        //    Debug.Log("MatchFlags now: " +  MatchManager.Current.MatchFlags);
        //}

        //public static void HandleThrowInEvent(MatchManager mm, Vector3 pos) {

        //    CallData call = new CallData();
        //    call.call = Call.ThrowIn;

        //    PlayerBase fsPlayer = mm.MatchBall.LastTouchedPlayer;
        //    Debug.Log($"out for throw in off of {fsPlayer.MatchPlayer.Player.Name}({fsPlayer.GameTeam.Team.Team.TeamName})");

        //    RedTeam offLast = mm.MatchBall.LastTouchedPlayer.GameTeam == Match.teamA.fsTeam ? Match.teamA : Match.teamB;
        //    call.team = RedMatch.OtherTeam(offLast);
        //    call.location = pos;
        //    Debug.Log("correct call: throw in for " + call.team.squadName);
        //    RedMatch.TrackCorrectCall(call);

        //    mm.MatchFlags = FStudio.MatchEngine.Enums.MatchStatus.Freeze;
        //    Match.frozenWaitingForCall = true;
        //    Debug.Log("MatchFlags now: " +  MatchManager.Current.MatchFlags);
        //}

        //public static void HandleEndlineOutEvent(MatchManager mm, Vector3 ballPos) {

        //    CallData data = new CallData();

        //    RedTeam defendingTeam = RedMatch.WhoseDefensiveEnd(ballPos);
        //    Debug.Log("ball crossed endling near goal of " + defendingTeam.squadName);
        //    Debug.Log("last touched: " + mm.MatchBall.LastTouchedPlayer.MatchPlayer.Player.Name + "(" + mm.MatchBall.LastTouchedPlayer.GameTeam.Team.Team.TeamName + ")");
        //    if (mm.MatchBall.LastTouchedPlayer.GameTeam == defendingTeam.fsTeam) {
        //        // corner kick!
        //        RefTarget corner = Match.cornerFlags.ClosestTo(ballPos);
        //        data.call = Call.CornerKick;
        //        data.team = RedMatch.OtherTeam(defendingTeam);
        //        data.target = corner;
        //        Debug.Log("correct call: corner at " + corner.name + " for " + data.team.squadName);
        //    }
        //    else {
        //        // goal kick!
        //        data.call = Call.GoalKick;
        //        data.team = defendingTeam;
        //        data.target = defendingTeam.goal;
        //        Debug.Log("correct call: goal kick for " + data.team.squadName);
        //    }

        //    RedMatch.TrackCorrectCall(data);

        //    mm.MatchFlags = FStudio.MatchEngine.Enums.MatchStatus.Freeze;
        //    Match.frozenWaitingForCall = true;
        //    Debug.Log("MatchFlags now: " +  MatchManager.Current.MatchFlags);
        //}

        public static void Foul(RedTeam team, Vector3 pos) {

            //if (Match.mm) {
            //    Match.mm.Foul(FStudio.MatchEngine.Enums.FoulType.Foul, fieldPos, awardedTeam.fsTeam, FStudio.Data.Positions.AMF);
            //}
        }

        public static void GoalKick(RedTeam team) {
            //if (Match.mm) {
            //    Match.mm.GoalKick(kickingTeam.id);
            //}

        }

        public static void ThrowIn(RedTeam team) {
            //if (mm) {
            //    Match.mm.lastOff = OtherTeam(throwingTeam).fsTeam;
            //    EventManager.Trigger(new ThrowInEvent(Match.currentMatchBall.transform.position));
            //}
        }

        public static bool MatchIsFrozen() {
            return false;
        }

        public static void BlowWhistle() {

            // current match manager is mm
                //if (Match.mm) {
                //    Match.mm.BlowWhistle();
                //}
        }
    }
}
