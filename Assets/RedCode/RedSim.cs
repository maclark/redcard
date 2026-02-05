using UnityEngine;

namespace RedCard {

    public class RedSim {

        public static void Corner() {

            //if (Match.mm) {
            //    Transform[] cornerSpots = Match.mm.GetCornerSpots();
            //    int cornerIndex = 0;
            //    float sqrDist = float.MaxValue;
            //    for (int i = 0; i < cornerSpots.Length; ++i) {
            //        float thisSqrDist = Vector3.SqrMagnitude(cornerSpots[i].position - target.transform.position);
            //        if (thisSqrDist < sqrDist) {
            //            sqrDist = thisSqrDist;
            //            cornerIndex = i;
            //        }
            //    }
            //    Match.mm.Corner(cornerIndex);
            //}

        }

        public static void GoalScored(RedTeam team) {

            //if (team.fsTeam) {
            //    // BallActionDetector used this var for the homeOrAway arg:
            //    // var goal = position.x > sizeOfField.x / 2;
            //    // in Shooting.cs home team team id 1 and away team to 0
            //    bool homeOrAway = team.id == 1;
            //    print("team.id " + team.id);
            //    if (homeOrAway) print("home team scored");
            //    else print("away team scored");
            //        EventManager.Trigger(new GoalEvent(homeOrAway));
            //    Match.mm.MatchBall.Release();
            //}


        }

        public static void Penalty(Vector3 penaltySpot, RedTeam shootingTeam) {
            //if (Match.mm) {
            //    Match.mm.Foul(FStudio.MatchEngine.Enums.FoulType.Foul, penaltySpot, shootingTeam.fsTeam, FStudio.Data.Positions.AMF);
            //}

        }

        public static string matchMinutes {
            get {
#if FOOTBALL_SIMULATOR
            if (FStudio.MatchEngine.MatchManager.Current) {
                return FStudio.MatchEngine.MatchManager.Current.minutes.ToString();
            }
#else
                return "0.00s";
#endif
            }
        }

        public static void MakeRedPlayers(RedTeam team) {
#if FOOTBALL_SIMULATOR
            foreach (var p in current.GameTeam1.GamePlayers) {
                RedPlayer rp = new RedPlayer();
                rp.surname = p.MatchPlayer.Player.Name;
                rp.team = team;
                GameObject outline = null;
                if (RedMatch.Match.playerOutlinePrefab) outline = GameObject.Instantiate(RedMatch.Match.playerOutlinePrefab);
                else outline = new GameObject();
                outline.transform.SetParent(p.PlayerController.UnityObject.transform);
                outline.transform.localPosition = Vector3.zero;
                outline.transform.localRotation = Quaternion.identity;
                outline.SetActive(false);

                RefTarget target = p.PlayerController.UnityObject.AddComponent<RefTarget>();
                target.targetType = TargetType.Player;
                target.outline = outline;
                target.attackingEnd = team.attackingEnd;
                RedMatch.Match.allPlayers.Add(target, rp);

                p.target = target;
                rp.angerBar = p.PlayerController.UnityObject.GetComponentInChildren<AngerBar>();
                rp.angerBar.SetFill(rp.anger / RedMatch.Match.settings.maxAnger);
            }
#else
            Debug.LogWarning("no simulator to make players for");
#endif
        }

        public static Vector3 CurrentBallPos {
            // Match.current.ball or whatever
            get {
                return Vector3.zero;
            }
        }

        public static Vector3 Goal1Pos {
            //current.goalNet1.gameObject.transform.position.x;
            get {
                return Vector3.zero;
            }
        }

        public static Vector3 Goal2Pos {
            //current.goalNet2.gameObject.transform.position.x;
            get {
                return Vector3.zero;
            }
        }
    }
}
