using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using TrainTracker.Models;
using TrainTracker.Web.Models;
using Calendar = TrainTracker.Models.Calendar;

namespace TrainTracker.Web.Repository
{
    public class TrackTrackerRepository
    {
        private TrainTrackerContext _dbContext;

        public DbSet<Route> Routes => _dbContext.Routes;
        public List<Route> RoutesCache => _routes ?? (_routes = _dbContext.Routes.ToList());
        private List<Route> _routes;

        public DbSet<Shape> Shapes => _dbContext.Shapes;
        public List<Shape> ShapesCache => _shapes ?? (_shapes = _dbContext.Shapes.ToList());
        private List<Shape> _shapes;

        public DbSet<Stop> Stops => _dbContext.Stops;
        public List<Stop> StopsCache => _stops ?? (_stops = _dbContext.Stops.ToList());
        private List<Stop> _stops;

        public DbSet<Trip> Trips => _dbContext.Trips;
        public List<Trip> TripsCache => _trips ?? (_trips = _dbContext.Trips.ToList());
        private List<Trip> _trips;

        public DbSet<StopTime> StopTimes => _dbContext.StopTimes;
        public List<StopTime> StopTimesCache => _stoptimes ?? (_stoptimes =_dbContext.StopTimes.ToList());
        private List<StopTime> _stoptimes;

        public DbSet<Calendar> Calendars => _dbContext.Calendars;
        public List<Calendar> CalendarsCache => _calendars ?? (_calendars = _dbContext.Calendars.ToList());
        private List<Calendar> _calendars;

        //public DbSet<CalendarDate> CalendarDates => _dbContext.CalendarDates;
        public List<CalendarDate> CalendarDatesCache => 
            _calendarDates ?? (_calendarDates = _dbContext.CalendarDates.ToList());
        private List<CalendarDate> _calendarDates;

        public TrackTrackerRepository()
        {
            _dbContext = new TrainTrackerContext();
        }

        public IEnumerable<Trip> GetRouteTrips(Route route, DateTime? filterForDay)
        {
            return
                TripsCache.Where(x => x.RouteId == route.RouteId)
                    .ToList()
                    .Where(x => !filterForDay.HasValue || IsTripRunning(x, filterForDay));
        }
        public IEnumerable<Shape> GetTripShapes(Trip trip)
        {
            return ShapesCache.Where(x => x.ShapeId == trip.ShapeId).OrderBy(x=> x.ShapePtSequence);
        }
        public Route GetTripRoute(Trip trip)
        {
            return RoutesCache.First(x => x.RouteId == trip.RouteId);
        }
        public IEnumerable<Stop> GetTripStops(Trip trip)
        {
            return
                StopTimesCache.Where(x => x.TripId == trip.TripId)
                    .SelectMany(x => StopsCache.Where(t => t.StopId == x.StopId));
            //Select All Stop Times with tripid = trip.tripid and group to "TripStopTimes"
            //Select all Stops from "TripStopTimes" where stopid is "TripStopTimes".stopid.
        }
        public IEnumerable<StopTime> GetTripStopTimes(Trip trip)
        {
            return
                StopTimesCache.Where(x => x.TripId == trip.TripId);
            //Select All Stop Times with tripid = trip.tripid and group to "TripStopTimes"
            //Select all Stops from "TripStopTimes" where stopid is "TripStopTimes".stopid.
        }

        public bool IsTripRunning(Trip trip, DateTime? atTime = null)
        {
            if (atTime == null)
                atTime = DateTime.Now;

            var result = CalendarDatesCache.Any(
                x => x.ServiceId == trip.ServiceId &&
                     x.Date == atTime.Value.ToString("yyyyMMdd") &&
                     x.ExceptionType == 2);
            if (result) return false;

            var cal = CalendarsCache.FirstOrDefault(x => x.ServiceId == trip.ServiceId &&
            DateTime.ParseExact(x.StartDate, "yyyyMMdd", DateTimeFormatInfo.InvariantInfo) < atTime.Value &&
            DateTime.ParseExact(x.EndDate, "yyyyMMdd", DateTimeFormatInfo.InvariantInfo) > atTime.Value);
            if (cal == null) return false;

            var day = atTime.Value.DayOfWeek;
            switch (day)
            {
                case DayOfWeek.Sunday:
                    result = cal.Sunday;
                    break;
                case DayOfWeek.Monday:
                    result = cal.Monday;
                    break;
                case DayOfWeek.Tuesday:
                    result = cal.Tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    result = cal.Wednesday;
                    break;
                case DayOfWeek.Thursday:
                    result = cal.Thursday;
                    break;
                case DayOfWeek.Friday:
                    result = cal.Friday;
                    break;
                case DayOfWeek.Saturday:
                    result = cal.Saturday;
                    break;
            }
            return result;

            //TODO
            //var stopTimes = StopTimesCache.Where(x => x.trip_id == trip.trip_id).OrderBy(x=> x.arrival_time).ToList();
            //if (stopTimes.Count(x => x.arrival_time.HasValue || x.departure_time.HasValue) < 1) return false;

            //var first = stopTimes.First(x => x.departure_time.HasValue);
            //var last = stopTimes.Last(x => x.departure_time.HasValue);
            

            //if (first.arrival_time.Value.TimeOfDay < atTime.Value.TimeOfDay && atTime.Value.TimeOfDay < last.arrival_time.Value.TimeOfDay)
            //{
            //    result = true;
            //}

            //return result;

        }

        public void UnloadCache(bool removeCalendarData = false, bool removeStopData = false)
        {
            _trips?.Clear();
            _trips?.TrimExcess();
            _trips = null;

            _shapes?.Clear();
            _shapes?.TrimExcess();
            _shapes = null;

            _routes?.Clear();
            _routes?.TrimExcess();
            _routes = null;

            _stoptimes?.Clear();
            _stoptimes?.TrimExcess();
            _stoptimes = null;
            if (removeStopData)
            {
                _stops?.Clear();
                _stops?.TrimExcess();
                _stops = null;
            }
            if (removeCalendarData)
            {
                _calendarDates?.Clear();
                _calendarDates?.TrimExcess();
                _calendarDates = null;
                _calendars?.Clear();
                _calendars?.TrimExcess();
                _calendars = null;
            }
        }

        public IEnumerable<Trip> GetStopTrips(Stop stop)
        {
            return
                StopTimesCache.Where(x => x.StopId == stop.StopId)
                    .SelectMany(x => TripsCache.Where(t => t.TripId == x.TripId));
            //Select All Stop Times with stopid = stop.stopid and group to "StopStopTimes"
            //Select all trips from "StopStopTimes" where tripid is "StopStopTimes".tripid.
        }


    }
}