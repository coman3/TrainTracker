using System;

namespace TrainTracker.Web.Helpers
{
    public static class NumericExtensions
    {
        public static double ToRadians(this double val)
        {
            return val * Math.PI / 180;
        }
        public static double ToDegrees(this double val)
        {
            return val * 180 / Math.PI;
        }
    }
}