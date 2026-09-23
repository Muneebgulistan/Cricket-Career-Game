namespace CricketGame.Cricket
{
    public enum PlayingRole
    {
        Batsman,
        Bowler,
        AllRounder,
        WicketKeeper
    }

    public enum BattingStyle
    {
        RightHand,
        LeftHand
    }

    public enum BowlingStyle
    {
        RightArmFast,
        LeftArmFast,
        RightArmMedium,
        LeftArmMedium,
        RightArmOffSpin,
        RightArmLegSpin,
        LeftArmOrthodox,
        LeftArmUnorthodox,
        None
    }

    public enum MatchFormat
    {
        T10,
        T20,
        ODI,
        Test
    }

    public enum CareerLevel
    {
        Under16Cup,
        Under19Cup,
        DomesticCricket,
        CountryRegionalLeague,
        HomeSeries,
        AwaySeries,
        TestSeries,
        T20WorldCup,
        ODIWorldCup
    }

    public enum CareerStatus
    {
        Active,
        Injured,
        Dropped,
        Rested,
        Retired
    }

    public enum PlayerHand
    {
        Right,
        Left
    }

    public enum BowlingType
    {
        Pace,
        Seam,
        Swing,
        Spin,
        None
    }

    public enum PitchType
    {
        Standard,
        Green,
        Dusty,
        Dead
    }
}
