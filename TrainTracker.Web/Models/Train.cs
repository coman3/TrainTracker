using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TrainTracker.Web.Helpers;
using TrainTracker.Web.Hubs;
using TrainTracker.Web.Repository;

namespace TrainTracker.Web.Models
{
    public class Train
    {
        public LatLng Posistion { get; set; }
        public Trip Trip { get; set; }
        public string TripId { get; set; }
        public string TripHeadsign { get; set; }
        public DateTime LastUpdated { get; set; }
        public Stop_times LastStopTime { get; set; }
        public Stop_times NextStopTime { get; set; }
        public List<Shape> TripShapes { get; set; }
        public List<Shape> ShapesToNextStation { get; set; }
        public List<Stop_times> StopTimes { get; set; }
        public List<Stop> Stops { get; set; }
        public bool Loaded { get; set; }
        public bool TrainRunning { get; private set; } = true;

        private Task _loadingTask { get; set; }

        public void Load(TrackTrackerRepository repository)
        {
            if (Trip == null)
            {
                Trip = repository.Trips.First(x => x.trip_id == TripId);
                TripHeadsign = Trip.trip_headsign;
            }
            if (StopTimes == null)
                StopTimes = repository.GetTripStopTimes(Trip).OrderBy(x => x.arrival_time).ToList();

            TripShapes = repository.GetTripShapes(Trip).ToList();
            Stops = StopTimes.Select(x => repository.Stops.First(s => s.stop_id == x.stop_id)).ToList();
            Loaded = true;
        }

        public void Update(DateTime currentTime)
        {
            if(!Loaded)
                throw new InvalidOperationException("Train Not Loaded!");
            if(!IsTrainRunning(currentTime))
                return;

            var lastStopTimeIndex = GetPreviousStopIndex(currentTime, StopTimes);
            if(lastStopTimeIndex < 0) return;
            
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
            LastUpdated = currentTime;
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

        private bool IsTrainRunning(DateTime currentTime)
        {
            var firstStopTime = StopTimes.FirstOrDefault(x => x.departure_time.HasValue);
            var lastStopTime = StopTimes.LastOrDefault(x => x.arrival_time.HasValue);
            if (firstStopTime == null || lastStopTime == null)
            {
                TrainRunning = false;
                return false;
            }

            // ReSharper disable PossibleInvalidOperationException
            if (firstStopTime.departure_time.Value.TimeOfDay < currentTime.TimeOfDay 
                && lastStopTime.arrival_time.Value.TimeOfDay > currentTime.TimeOfDay)
            {
                TrainRunning = true;
                return true;
            }
            TrainRunning = false;
            return false;
        }
        private static int GetPreviousStopIndex(DateTime timeNow, List<Stop_times> tripStops)
        {
            for (int i = 1; i < tripStops.Count; i++)
            {
                var currentStop = tripStops[i];
                if (!currentStop.arrival_time.HasValue) continue;

                if (currentStop.arrival_time.Value.TimeOfDay > timeNow.TimeOfDay)
                {
                    return i - 1;
                }
            }
            return -1;
        }

        public void LoadAsync(TrackTrackerRepository repository)
        {
            if (_loadingTask == null)
            {
                
                _loadingTask = Task.Factory.StartNew(() =>
                {
                    lock (repository)
                    {
                        Load(repository);
                    }
                });
            }
        }
    }
}