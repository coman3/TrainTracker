namespace TrainTracker.Models
{
    public class Trip
    {
        public string RouteId { get; set; }
        public string ServiceId { get; set; }
        public string TripId { get; set; }
        public string ShapeId { get; set; }
        public string TripHeadsign { get; set; }
        public byte DirectionId { get; set; }
    }
}
