using UnityEngine;

namespace RedCard {

    public enum MovementType {
        Relax,
        Normal,
        BestHeCanDo
    }

    public enum Acts {
        Nothing,
        GoingToGoal,
        PassingToBetterOpportunity,
        Crossing,
        PassingToClosestTeammate,
        PassingToAvailableClosestTeammate,
        Shoot,
        GoingToTackle,
        LastThingToDoWhenWeHaveTheBall,
        Tackling,
        TacklingFailed,
        Stunned,
        MarkingTheOpponent,
        ReachedToTheGoalButCouldNotShoot_PassingToClosestTeammate,
        NothingToDo_GoingToShoot,
        InMarking_LookingForTeammateToPass,
        NothingToDo_PassingToClosestTeammate,
        GoingToGetTheBall_BallChasing,
        GoingToGetTheBall_WithCaution,
        RunningForward,
        RunningBehindTheDefenseLine,
        PassingToBehindTheDefenseLine,
        ThrowIn,
        StrikerTacticalBehaviour,
        TacticalPositioning,
        DefensiveTacticalPositioningBehaviour,
        BecomeAPassOption,
        TargetMan,
        JoinTheAttack,
        PassAndRun,
        MarkTheLastGuy,
        Dribbling,
        InputPass,
        InputShoot
    }
    public enum BallHitAnimationEvent {
        None,
        Pass,
        LongBall,
        Shoot
    }



    public enum PlayerAnimatorVariable {
        Horizontal,
        Vertical,
        MoveSpeed,
        /// <summary>
        /// Speed of all actions like passing, shooting.
        /// </summary>
        Agility,
        IsHoldingBall,
        Pass_R,
        LongBall_R,
        Shoot_R,
        Shoot_L,
        Tackling,
        Tackled,
        ThrowInIdle,
        IsHappy,
        Struggle,
        Header_R,
        Volley_R,
        Spin, // Dribblesuccess.
        GroundHeader_R,
        Throw_R,
        GKJumpLeft,
        GKJumpRight,
        GKMiss,
        GKBallSave_Low,
        GKDegage_R,

        // L footed
        Pass_L,
        LongBall_L,
        Header_L,
        GroundHeader_L,
        Volley_L,
        Throw_L,
        GKDegage_L,
        // 

        ParameterCount // Parameter count of the animator.
    }
    public enum PassType {
        ShortPass,
        LongPass,
        ThroughtPass
    }
    public struct PassTarget {
        public readonly PassType _PassType;
        public readonly string _OptionName;
        public readonly Jugador _ActualTarget;
        public readonly Vector3 _Position;
        public readonly float _PassPower;

        public PassTarget(
            PassType passType,
            string optionName,
            Vector3 position,
            Jugador actualTarget,
            float passPower) {

            _PassType = passType;
            _OptionName = optionName;
            _ActualTarget = actualTarget;
            _Position = position;
            _PassPower = passPower;
        }

        public bool IsValid => !string.IsNullOrEmpty(_OptionName);
    }
}

