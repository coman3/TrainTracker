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

        public List<Calendar> Calendars =>
            _calendars ?? (_calendars = _dbContext.Calendars.ToList());
        private List<Calendar> _calendars;
        public List<Calendar_dates> CalendarDates => 
            _calendarDates ?? (_calendarDates = _dbContext.Calendar_dates.ToList());
        private List<Calendar_dates> _calendarDates;

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
            var result = false;
            if (atTime == null)
                atTime = DateTime.Now;

            result = CalendarDates.Any(
                    x => x.service_id == trip.service_id &&
                     x.date == atTime.Value.ToString("yyyyMMdd") &&
                      x.exception_type == 2);
            if (result) return false;

            var cal = Calendars.First(x => x.service_id == trip.service_id);
            var day = atTime.Value.DayOfWeek;
            switch (day)
            {
                case DayOfWeek.Sunday:
                    result = cal.sunday;
                    break;
                case DayOfWeek.Monday:
                    result = cal.monday;
                    break;
                case DayOfWeek.Tuesday:
                    result = cal.tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    result = cal.wednesday;
                    break;
                case DayOfWeek.Thursday:
                    result = cal.thursday;
                    break;
                case DayOfWeek.Friday:
                    result = cal.friday;
                    break;
                case DayOfWeek.Saturday:
                    result = cal.saturday;
                    break;
            }
            return result;

            //TODO
            //var stopTimes = StopTimes.Where(x => x.trip_id == trip.trip_id).OrderBy(x=> x.arrival_time).ToList();
            //if (stopTimes.Count(x => x.arrival_time.HasValue || x.departure_time.HasValue) < 1) return false;

            //var first = stopTimes.First(x => x.departure_time.HasValue);
            //var last = stopTimes.Last(x => x.departure_time.HasValue);
            

            //if (first.arrival_time.Value.TimeOfDay < atTime.Value.TimeOfDay && atTime.Value.TimeOfDay < last.arrival_time.Value.TimeOfDay)
            //{
            //    result = true;
            //}

            //return result;

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