using System;
using System.Collections.Generic;
using System.Linq;
using TrainTracker.Web.Repository;

namespace TrainTracker.Web.Models
{
    public class TripDetails : Trip
    {
        public TripDetails(TrackTrackerRepository repository, Trip trip)
        {
            Id = trip.ID;
            trip_id = trip.trip_id;
            route_id = trip.route_id;
            shape_id = trip.shape_id;
            service_id = trip.service_id;
            Route = repository.GetTripRoute(trip);
            Service = repository.Calendars.First(x => x.service_id == trip.service_id);
            trip_headsign = trip.trip_headsign;
            direction_id = trip.direction_id;
        }
        public int Id { get; set; }
        public Route Route { get; set; }
        public Calendar Service { get; set; }
    }
}