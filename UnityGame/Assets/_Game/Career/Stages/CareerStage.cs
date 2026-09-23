using System;

namespace CricketGame.Career.Stages
{
    public enum CareerStage
    {
        UNDER_16,
        UNDER_19,
        DOMESTIC,
        COUNTRY_LEAGUE,
        HOME_SERIES,
        AWAY_SERIES,
        TEST_SERIES,
        T20_WORLD_CUP,
        ODI_WORLD_CUP
    }

    public enum CareerStageStatus
    {
        Locked,
        Available,
        InProgress,
        Completed
    }
}
