using System;
using TrainTracker.Models;

namespace TrainTracker.Web.Models
{
    public class GenerateStateArgs : EventArgs
    {
        public GenerateState NewState { get; set; }
        public GenerateState OldState { get; set; }

        public GenerateStateArgs(GenerateState newState, GenerateState oldState)
        {
            NewState = newState;
            OldState = oldState;
        }
    }
}