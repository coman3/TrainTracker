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

        public IEnumerable<Trip> GetRouteTrips(Route route, DateTime? filterForDay)
        {
            return
                Trips.Where(x => x.route_id == route.route_id)
                    .ToList()
                    .Where(x => !filterForDay.HasValue || IsTripRunning(x, filterForDay));
        }
        public IEnumerable<Shape> GetTripShapes(Trip trip)
        {
            return Shapes.Where(x => x.shape_id == trip.shape_id).OrderBy(x=> x.shape_pt_sequence);
        }
        public Route GetTripRoute(Trip trip)
        {
            return Routes.First(x => x.route_id == trip.route_id);
        }
        public IEnumerable<Stop> GetTripStops(Trip trip)
        {
            return
                StopTimes.Where(x => x.trip_id == trip.trip_id)
                    .SelectMany(x => Stops.Where(t => t.stop_id == x.stop_id));
            //Select All Stop Times with tripid = trip.tripid and group to "TripStopTimes"
            //Select all Stops from "TripStopTimes" where stopid is "TripStopTimes".stopid.
        }
        public IEnumerable<Stop_times> GetTripStopTimes(Trip trip)
        {
            return
                StopTimes.Where(x => x.trip_id == trip.trip_id);
            //Select All Stop Times with tripid = trip.tripid and group to "TripStopTimes"
            //Select all Stops from "TripStopTimes" where stopid is "TripStopTimes".stopid.
        }

        public bool IsTripRunning(Trip trip, DateTime? atTime = null)
        {
            if (atTime == null)
                atTime = DateTime.Now;
            var calDate = CalendarDates.ToList().Any(
                    x => x.service_id == trip.service_id &&
                     x.date == atTime.Value.ToString("yyyyMMdd") &&
                      x.exception_type == 2);
            if (calDate) return false;

            var cal = Calendars.First(x => x.service_id == trip.service_id);
            var day = atTime.Value.DayOfWeek;
            switch (day)
            {
                case DayOfWeek.Sunday:
                    return cal.sunday;
                case DayOfWeek.Monday:
                    return  cal.monday;
                case DayOfWeek.Tuesday:
                    return cal.tuesday;
                case DayOfWeek.Wednesday:
                    return cal.wednesday;
                case DayOfWeek.Thursday:
                    return cal.thursday;
                case DayOfWeek.Friday:
                    return cal.friday;
                case DayOfWeek.Saturday:
                    return cal.saturday;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IEnumerable<Trip> GetStopTrips(Stop stop)
        {
            return
                StopTimes.Where(x => x.stop_id == stop.stop_id)
                    .SelectMany(x => Trips.Where(t => t.trip_id == x.trip_id));
            //Select All Stop Times with stopid = stop.stopid and group to "StopStopTimes"
            //Select all trips from "StopStopTimes" where tripid is "StopStopTimes".tripid.
        }


    }
}