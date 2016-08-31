using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using TrainTracker.Web.Models;

namespace TrainTracker.Web.Repository
{
    public class TrackTrackerRepository
    {
        private TrackTracker _dbContext;

        public DbSet<Route> Routes => _dbContext.Routes;
        public DbSet<Shape> Shapes => _dbContext.Shapes;
        public DbSet<Stop> Stops => _dbContext.Stops;
        public DbSet<Trip> Trips => _dbContext.Trips;
        public DbSet<Stop_times> StopTimes => _dbContext.Stop_times;
        public DbSet<Calendar> Calendars => _dbContext.Calendars;
        public DbSet<Calendar_dates> CalendarDates => _dbContext.Calendar_dates;

        public TrackTrackerRepository()
        {
            _dbContext = new TrackTracker();
            
        }

        public IEnumerable<Trip> GetRouteTrips(Route route)
        {
            return Trips.Where(x => x.C_route_id == route.C_route_id);
        }
        public IEnumerable<Shape> GetTripShapes(Trip trip)
        {
            return Shapes.Where(x => x.C_shape_id == trip.shape_id);
        }
        public Route GetTripRoute(Trip trip)
        {
            return Routes.First(x => x.C_route_id == trip.C_route_id);
        }
        public IEnumerable<Stop> GetTripStops(Trip trip)
        {
            return
                StopTimes.Where(x => x.C_trip_id == trip.trip_id)
                    .SelectMany(x => Stops.Where(t => t.C_stop_id == x.stop_id.Value));
            //Select All Stop Times with tripid = trip.tripid and group to "TripStopTimes"
            //Select all Stops from "TripStopTimes" where stopid is "TripStopTimes".stopid.
        }
        public bool IsTripRunning(Trip trip, DateTime? atTime = null)
        {
            if(atTime == null)
                atTime = DateTime.Now;
            var calDate = CalendarDates.Any(x => x.C_service_id == trip.service_id && x.date == atTime.Value.ToString("yyyyMMdd"));
            if (calDate) return false;

            var cal = Calendars.First(x => x.C_service_id == trip.service_id);
            var day = atTime.Value.DayOfWeek;
            switch (day)
            {
                case DayOfWeek.Sunday:
                    return cal.sunday.HasValue && cal.sunday.Value;
                case DayOfWeek.Monday:
                    return cal.monday.HasValue && cal.monday.Value;
                case DayOfWeek.Tuesday:
                    return cal.tuesday.HasValue && cal.tuesday.Value;
                case DayOfWeek.Wednesday:
                    return cal.wednesday.HasValue && cal.wednesday.Value;
                case DayOfWeek.Thursday:
                    return cal.thursday.HasValue && cal.thursday.Value;
                case DayOfWeek.Friday:
                    return cal.friday.HasValue && cal.friday.Value;
                case DayOfWeek.Saturday:
                    return cal.saturday.HasValue && cal.saturday.Value;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IEnumerable<Trip> GetStopTrips(Stop stop)
        {
            return
                StopTimes.Where(x => x.stop_id.HasValue && x.stop_id.Value == stop.C_stop_id)
                    .SelectMany(x => Trips.Where(t => t.trip_id == x.C_trip_id));
            //Select All Stop Times with stopid = stop.stopid and group to "StopStopTimes"
            //Select all trips from "StopStopTimes" where tripid is "StopStopTimes".tripid.
        }


    }
}