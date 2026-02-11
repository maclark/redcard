using UnityEngine;


namespace RedCard {
    public abstract class Behavior {

        public Jugador jugador { protected set; get; }
        protected float time;
        protected float dt;
        protected float xFieldEnd;
        protected float yFieldEnd;
        protected MatchStatus matchStatus;
        protected TeamPosture teamPosture;
        protected float xOffsideLine;
        protected float xOurOffsideLine;
        protected RedBall ball;
        protected GoalNet goalNet;
        protected GoalNet targetGoalNet;
        protected Jugador[] teammates;
        protected Jugador[] opponents;

        public abstract bool Behave(bool isAlreadyActive);

        // IsRoughValidated(...

        public void SetBehavior(
                        Jugador jugador,
                        in float time,
                        in float dt,
                        in float xFieldEnd,
                        in float yFieldEnd,
                        in MatchStatus matchStatus,
                        in TeamPosture teamPosture,
                        in float xOffsideLine,
                        in float xOurOffsideLine,
                        RedBall matchBall,
                        GoalNet ourGoalNet,
                        GoalNet opponentGoalNet,
                        in Jugador[] teammates,
                        in Jugador[] opponents
                ) {
            this.jugador = jugador;
            this.time = time;
            this.dt = dt;
            this.xFieldEnd = xFieldEnd;
            this.yFieldEnd = yFieldEnd;
            this.matchStatus = matchStatus;
            this.teamPosture = teamPosture;
            this.xOffsideLine = xOffsideLine;
            this.xOurOffsideLine = xOurOffsideLine;
            this.ball = matchBall;
            this.goalNet = ourGoalNet;
            this.targetGoalNet = opponentGoalNet;
            this.teammates = teammates;
            this.opponents = opponents;
        }
    }
}
