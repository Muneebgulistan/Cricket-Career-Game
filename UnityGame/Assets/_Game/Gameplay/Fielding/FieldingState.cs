namespace CricketGame.Gameplay.Fielding
{
    public enum FieldingState
    {
        Idle,
        Ready,
        Anticipating,
        MovingToBall,
        ApproachingBall,
        Pickup,
        ThrowPreparation,
        Throwing,
        FollowThrough,
        Returning,
        Completed
    }
}
