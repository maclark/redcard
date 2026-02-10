using UnityEngine;
using System.Collections.Generic;

namespace RedCard {
    public class Jugador {

        public int id = -1;
        public string givenName;
        public string surname;
        public RedTeam team;
        public RefTarget target;
        public AngerBar angerBar;
        public bool isGoalie = false;
        public float anger = 0f;
        public JugadorController controller;



        public List<Behavior> behaviors = new List<Behavior>() {
            // to do

        }; 

        public void Behave(
                in float time,
                in float dt,
                in float xFieldEnd,
                in float yFieldEnd,
                in MatchStatus matchStatus,
                in TeamPosture teamPosture,
                in float xOpponentOffsideLine,
                in float xOurOffsideLine,
                RedBall matchBall,
                GoalNet opponentGoalNet,
                in Jugador[] teammates,
                in Jugador[] opponents
            ) {

            // ...

        }


        public void ProcessBehaviors(in float time) {

            // if (controller.IsAnimationABlocker( 

            // skip rate is an AI difficulty level thing
            // skipRate =
            // if skip rate > ?, return


            //IEnumerable<Behavior> forcedBehaviors = behaviors.Where(x => x.forceBehavior);
            // foreach (Behavior b in forcedBehaviors) ... return

            // bool reset = 

            // ....
        }

    }
}
