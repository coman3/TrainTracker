namespace TrainTracker.Models
{
    public class TrainUpdate
    {
        public LatLng Posistion { get; set; }
        public string TripId { get; set; }
        public string Tripheadsign { get; set; }

        public TrainUpdate(LatLng posistion, string tripId, string tripheadsign)
        {
            Posistion = posistion;
            TripId = tripId;
            Tripheadsign = tripheadsign;
        }
    }
}