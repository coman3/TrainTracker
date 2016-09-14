using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TrainTracker.Models;
using TrainTracker.Web.Repository;

namespace TrainTracker.Web.Models
{
    [NotMapped]
    public class TripDetails : Trip
    {
        public TripDetails()
        {
            
        }
        public TripDetails(TrackTrackerRepository repository, Trip trip)
        {
            TripId = trip.TripId;
            RouteId = trip.RouteId;
            ShapeId = trip.ShapeId;
            ServiceId = trip.ServiceId;
            Route = repository.GetTripRoute(trip);
            Service = repository.CalendarsCache.First(x => x.ServiceId == trip.ServiceId);
            TripHeadsign = trip.TripHeadsign;
            DirectionId = trip.DirectionId;
        }
        public Route Route { get; set; }
        public Calendar Service { get; set; }
    }
}