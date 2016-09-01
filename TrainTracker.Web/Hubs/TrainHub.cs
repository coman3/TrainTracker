using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;
using Microsoft.AspNet.SignalR;
using TrainTracker.Web.Helpers;
using TrainTracker.Web.Models;

namespace TrainTracker.Web.Hubs
{
    public class TrainHub : Hub
    {
        private Repository.TrackTrackerRepository _repository = new Repository.TrackTrackerRepository();

        public void RequestTrainLocations(string tripId, string time)
        {
            var timeNow = DateTime.Parse(time);
            var trip = _repository.Trips.First(x => x.trip_id == tripId);
            var tripStops = _repository.GetTripStopTimes(trip).OrderBy(x => x.arrival_time).ToList();
            var firstItem = tripStops.First();
            var lastItem = tripStops.Last();
            if (!firstItem.arrival_time.HasValue || !lastItem.arrival_time.HasValue) return;
            if (firstItem.arrival_time.Value.TimeOfDay > timeNow.TimeOfDay) return;
            if (lastItem.arrival_time.Value.TimeOfDay < timeNow.TimeOfDay) return;

            var lastStopIndex = GetPreviousStopIndex(timeNow, tripStops);
            var lastStop = tripStops[lastStopIndex];
            var nextStop = tripStops[lastStopIndex + 1];
            var lastStopStop = _repository.Stops.First(x => x.stop_id == lastStop.stop_id);
            var nextStopStop = _repository.Stops.First(x => x.stop_id == nextStop.stop_id);

            var shapes = _repository.GetTripShapes(trip).ToList();
            var lastStationClosestShapePoint =
                shapes.OrderBy(
                    x =>
                        Maths.Distance(x.shape_pt_lat, x.shape_pt_lon, lastStopStop.stop_lat,
                            lastStopStop.stop_lon)).First();

            var nextStationClosestShapePoint =
                shapes.OrderBy(
                    x =>
                        Maths.Distance(x.shape_pt_lat, x.shape_pt_lon, nextStopStop.stop_lat,
                            nextStopStop.stop_lon)).First();

            var shapesUntilNextStation =
                shapes.Skip(lastStationClosestShapePoint.shape_pt_sequence - 1)
                    .Take(nextStationClosestShapePoint.shape_pt_sequence + 1 -
                          lastStationClosestShapePoint.shape_pt_sequence).ToList();

            var trainPos = CalculateTrainPosistion(timeNow, lastStop, nextStop, shapesUntilNextStation);

            Clients.Caller.updateTrainLocations(lastStopStop, nextStopStop, trainPos);
        }

        private static LatLng CalculateTrainPosistion(DateTime timeNow, Stop_times lastStop, Stop_times nextStop, List<Shape> shapesUntilNextStation)
        {
            if (!nextStop.arrival_time.HasValue || !lastStop.arrival_time.HasValue) return null;

            var dif = nextStop.arrival_time.Value.TimeOfDay - lastStop.arrival_time.Value.TimeOfDay;
            var currentDif = nextStop.arrival_time.Value.TimeOfDay - timeNow.TimeOfDay;
            var percent = 1 - currentDif.TotalSeconds / dif.TotalSeconds;

            var distanceToStop = shapesUntilNextStation.Last().shape_dist_traveled;
            var distanceFromStop = shapesUntilNextStation.First().shape_dist_traveled;
            var distanceToNext = distanceToStop - distanceFromStop;
            var distanceTraveled = distanceToNext * percent;
            var trainPos = new LatLng();
            for (int s = 1; s < shapesUntilNextStation.Count; s++)
            {
                var point = shapesUntilNextStation[s];
                if (point.shape_dist_traveled > distanceFromStop + distanceTraveled)
                {
                    var prevPoint = shapesUntilNextStation[s - 1];
                    if (!prevPoint.shape_dist_traveled.HasValue) return null;
                    var distanceSkiped = prevPoint.shape_dist_traveled.Value - distanceFromStop.Value;
                    var shapeDistanceTraveled = distanceTraveled - distanceSkiped;
                    var shapeDistance = point.shape_dist_traveled - prevPoint.shape_dist_traveled;
                    var percentDistance = shapeDistance.Value * shapeDistanceTraveled.Value / shapeDistance.Value;

                    var lat1 = prevPoint.shape_pt_lat;
                    var lon1 = prevPoint.shape_pt_lon;

                    var lat2 = point.shape_pt_lat;
                    var lon2 = point.shape_pt_lon;

                    var result = Maths.ToBearingDistance(percentDistance, lat1, lon1, Maths.CalculateBearing(lat1, lon1, lat2, lon2).ToRadians());

                    trainPos = new LatLng { Latitude = result.Item1, Longitude = result.Item2 };
                    break;
                }
            }

            return trainPos;
        }



        private static int GetPreviousStopIndex(DateTime timeNow, List<Stop_times> tripStops)
        {
            for (int i = 1; i < tripStops.Count - 2; i++)
            {
                var currentStop = tripStops[i];
                if (!currentStop.arrival_time.HasValue) continue;

                if (currentStop.arrival_time.Value.TimeOfDay > timeNow.TimeOfDay)
                {
                    return i - 1;
                }
            }
            return 0;
        }
    }

    public class UpdateTrain
    {
        public Bounds Bounds { get; set; }
        public Train[] Train { get; set; }
    }

    public class Bounds
    {
        public LatLng NorthWest { get; set; }
        public LatLng SouthEast { get; set; }
    }

    public class Train
    {
        public LatLng Posistion { get; set; }
        public string TripId { get; set; }
        public string TripHeadsign { get; set; }
    }

    public class LatLng
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}