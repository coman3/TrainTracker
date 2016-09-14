using System;

namespace TrainTracker.Models
{
    public class StopTime
    {
        public int Id { get; set; }
        public string TripId { get; set; }
        public DateTime ArrivalTime => GetTime(ArrivalTimeRaw);
        public string ArrivalTimeRaw { get; set; }
        public DateTime DepartureTime => GetTime(DepartureTimeRaw);

        public static DateTime GetTime(string timeRaw)
        {
            if(string.IsNullOrEmpty(timeRaw)) return DateTime.MinValue;
            var values = timeRaw.Split(':');
            var time = new DateTime(1, 1, 1).AddHours(int.Parse(values[0])).AddMinutes(int.Parse(values[1])).AddSeconds(int.Parse(values[2]));
            return time;
        }

        public string DepartureTimeRaw { get; set; }
        public int StopId { get; set; }
        public short StopSequence { get; set; }
        public string StopHeadsign { get; set; }
        public byte? PickupType { get; set; }
        public byte? DropOffType { get; set; }
        public double? ShapeDistTraveled { get; set; }
    }
}
