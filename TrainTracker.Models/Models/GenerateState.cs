namespace TrainTracker.Models
{
    public enum GenerateState : byte
    {
        Nothing = 0,
        Starting = 1,
        Generating = 2,
        Loading = 3,
        Finished = 4
    }
}