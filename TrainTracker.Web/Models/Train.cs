using System;
using System.Collections.Generic;
using System.Linq;
using TrainTracker.Web.Helpers;
using TrainTracker.Web.Hubs;
using TrainTracker.Web.Repository;

namespace TrainTracker.Web.Models
{
    public class Train
    {
        public LatLng Posistion { get; set; }
        public Trip Trip { get; set; }
        public string TripHeadsign { get; set; }
        public DateTime LastUpdated { get; set; }
        public Stop_times LastStopTime { get; set; }
        public Stop_times NextStopTime { get; set; }
        public List<Shape> TripShapes { get; set; }
        public List<Shape> ShapesToNextStation { get; set; }
        public List<Stop_times> StopTimes { get; set; }
        public List<Stop> Stops { get; set; }
        public bool Loaded { get; set; }

        public void Load(TrackTrackerRepository repository)
        {
            var tripStops = repository.GetTripStopTimes(Trip).OrderBy(x => x.arrival_time).ToList();
            StopTimes = tripStops;
            TripShapes = repository.GetTripShapes(Trip).ToList();
            Stops = tripStops.Select(x => repository.Stops.First(s => s.stop_id == x.stop_id)).ToList();
            Loaded = true;
        }

        public void Update(DateTime currentTime)
        {
            if(!Loaded) throw new InvalidOperationException("Train Not Loaded!");
            var lastStopTimeIndex = GetPreviousStopIndex(currentTime, StopTimes);
            LastStopTime = StopTimes[lastStopTimeIndex];
            NextStopTime = StopTimes[lastStopTimeIndex + 1];
            var lastStation = Stops.First(x => x.stop_id == LastStopTime.stop_id);
            var nextStation = Stops.First(x => x.stop_id == NextStopTime.stop_id);

            var lastStationClosestShapePoint =
                TripShapes.OrderBy(
                    x =>
                        Maths.Distance(x.shape_pt_lat, x.shape_pt_lon, lastStation.stop_lat,
                            lastStation.stop_lon)).First();

            var nextStationClosestShapePoint =
                TripShapes.OrderBy(
                    x =>
                        Maths.Distance(x.shape_pt_lat, x.shape_pt_lon, nextStation.stop_lat,
                            nextStation.stop_lon)).First();

            ShapesToNextStation = 
                TripShapes.Skip(lastStationClosestShapePoint.shape_pt_sequence - 1)
                    .Take(nextStationClosestShapePoint.shape_pt_sequence + 1 -
                          lastStationClosestShapePoint.shape_pt_sequence).ToList();
            Posistion = CalculateTrainPosistion(currentTime, this);
        }
        private static LatLng CalculateTrainPosistion(DateTime currentTime, Train train)
        {
            if (!train.NextStopTime.arrival_time.HasValue || !train.LastStopTime.departure_time.HasValue)
                return LatLng.Invalid;

            var dif = train.NextStopTime.arrival_time.Value.TimeOfDay - train.LastStopTime.departure_time.Value.TimeOfDay;
            var currentDif = train.NextStopTime.arrival_time.Value.TimeOfDay - currentTime.TimeOfDay;
            var percent = 1 - currentDif.TotalSeconds / dif.TotalSeconds;

            var distanceToStop = train.ShapesToNextStation.Last().shape_dist_traveled;
            var distanceFromStop = train.ShapesToNextStation.First().shape_dist_traveled;
            var distanceToNext = distanceToStop - distanceFromStop;
            var distanceTraveled = distanceToNext * percent;
            var trainPos = LatLng.Invalid;
            for (int s = 1; s < train.ShapesToNextStation.Count; s++)
            {
                var point = train.ShapesToNextStation[s];
                if (point.shape_dist_traveled > distanceFromStop + distanceTraveled)
                {
                    var prevPoint = train.ShapesToNextStation[s - 1];
                    if (!prevPoint.shape_dist_traveled.HasValue)
                        return LatLng.Invalid;
                    var distanceSkiped = prevPoint.shape_dist_traveled.Value - distanceFromStop.Value;
                    var shapeDistanceTraveled = distanceTraveled - distanceSkiped;
                    var shapeDistance = point.shape_dist_traveled - prevPoint.shape_dist_traveled;
                    var percentDistance = shapeDistance.Value * shapeDistanceTraveled.Value / shapeDistance.Value;

                    var lat1 = prevPoint.shape_pt_lat;
                    var lon1 = prevPoint.shape_pt_lon;

                    var lat2 = point.shape_pt_lat;
                    var lon2 = point.shape_pt_lon;

                    var result = Maths.ToBearingDistance(percentDistance, lat1, lon1, Maths.CalculateBearing(lat1, lon1, lat2, lon2).ToRadians());

                    trainPos = new LatLng(result.Item1, result.Item2);
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
}