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

    public enum TacticPresetTypes {
        Balanced = 3,
        Offensive = 5,
        CounterAttack = 2,
        ParkTheBus = 0,
        HighPressure = 4,
        Defensive = 1,
        ParameterCount = 6
    }

    [System.Flags]
    public enum FormationPosition {
        GK = 1 << 1,
        RB = 1 << 2,
        LB = 1 << 3,
        CB = 1 << 4,
        CB_R = 1 << 5,
        CB_L = 1 << 6,
        DMF = 1 << 7,
        DMF_R = 1 << 8,
        DMF_L = 1 << 9,
        CM = 1 << 10,
        CM_L = 1 << 11,
        CM_R = 1 << 12,
        RMF = 1 << 13,
        LMF = 1 << 14,
        AMF = 1 << 15,
        AMF_R = 1 << 16,
        AMF_L = 1 << 17,
        LW = 1 << 18,
        RW = 1 << 19,
        ST = 1 << 20,
        ST_L = 1 << 21,
        ST_R = 1 << 22,
        ParametersCount = 1 << 23
    }
    public enum BootColor {
        Black,
        Red,
        Orange,
        Purple,
        Cyan,
        Gray,
        White,
        ParametersCount
    }




}

