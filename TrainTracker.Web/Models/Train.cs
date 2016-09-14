using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TrainTracker.Helpers;
using TrainTracker.Models;
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
        public StopTimeMinified LastStopTime { get; set; }
        public StopTimeMinified NextStopTime { get; set; }
        public List<Shape> TripShapes { get; set; }
        public List<Shape> ShapesToNextStation { get; set; }
        public List<StopTimeMinified> StopTimes { get; set; }
        public List<Stop> Stops { get; set; }
        public bool Loaded { get; set; }
        public bool TrainRunning { get; private set; } = true;

        private bool LoadingTask { get; set; }

        public void Load(TrackTrackerRepository repository)
        {
            if(Loaded)
                return;
            if (Trip == null)
            {
                Trip = repository.TripsCache.First(x => x.TripId == TripId);
                TripHeadsign = Trip.TripHeadsign;
            }
            if (StopTimes == null)
                StopTimes = repository.GetTripStopTimes(Trip).OrderBy(x => x.ArrivalTime).Select(x => new StopTimeMinified
                {
                    ArrivalTimeRaw = x.ArrivalTimeRaw,
                    DepartureTimeRaw = x.DepartureTimeRaw,
                    StopId = x.StopId,
                    ShapeDistanceTraveled = x.ShapeDistTraveled.Value
                }).ToList();
            
            TripShapes = repository.GetTripShapes(Trip).ToList();
            Stops = StopTimes.Select(x => repository.StopsCache.First(s => s.StopId == x.StopId)).ToList();
            Loaded = true;
        }

        public void Update(DateTime currentTime)
        {
            if(!Loaded)
                throw new InvalidOperationException("Train Not Loaded!");

            if(StopTimes.Count < 1) return;
            if (TripShapes.Count < 1) return;

            var lastStopTimeIndex = GetPreviousStopIndex(currentTime, StopTimes);
            if(lastStopTimeIndex < 0) return;
            
            LastStopTime = StopTimes[lastStopTimeIndex];
            NextStopTime = StopTimes[lastStopTimeIndex + 1];

            var lastStationClosestShapePoint =
                TripShapes.OrderBy(x => x.ShapeDistTraveled - LastStopTime.ShapeDistanceTraveled).First();

            var nextStationClosestShapePoint =
                TripShapes.OrderBy(
                    x => x.ShapeDistTraveled - NextStopTime.ShapeDistanceTraveled).First();

            ShapesToNextStation = 
                TripShapes.Skip(lastStationClosestShapePoint.ShapePtSequence - 1)
                    .Take(nextStationClosestShapePoint.ShapePtSequence + 1 -
                          lastStationClosestShapePoint.ShapePtSequence).ToList();
            Posistion = CalculateTrainPosistion(currentTime, this);
        }
        private static LatLng CalculateTrainPosistion(DateTime currentTime, Train train)
        {
            if (train.NextStopTime.ArrivalTime == DateTime.MinValue || train.LastStopTime.DepartureTime == DateTime.MinValue)
                return LatLng.Invalid;

            var dif = train.NextStopTime.ArrivalTime.TimeOfDay - train.LastStopTime.DepartureTime.TimeOfDay;
            var currentDif = train.NextStopTime.ArrivalTime.TimeOfDay - currentTime.TimeOfDay;
            var percent = 1 - currentDif.TotalSeconds / dif.TotalSeconds;

            var distanceToStop = train.ShapesToNextStation.Last().ShapeDistTraveled;
            var distanceFromStop = train.ShapesToNextStation.First().ShapeDistTraveled;
            var distanceToNext = distanceToStop - distanceFromStop;
            var distanceTraveled = distanceToNext * percent;
            var trainPos = LatLng.Invalid;
            for (int s = 1; s < train.ShapesToNextStation.Count; s++)
            {
                var point = train.ShapesToNextStation[s];
                if (point.ShapeDistTraveled > distanceFromStop + distanceTraveled)
                {
                    var prevPoint = train.ShapesToNextStation[s - 1];
                    if (!prevPoint.ShapeDistTraveled.HasValue)
                        return LatLng.Invalid;
                    var distanceSkiped = prevPoint.ShapeDistTraveled.Value - distanceFromStop.Value;
                    var shapeDistanceTraveled = distanceTraveled - distanceSkiped;
                    var shapeDistance = point.ShapeDistTraveled - prevPoint.ShapeDistTraveled;
                    var percentDistance = shapeDistance.Value * shapeDistanceTraveled.Value / shapeDistance.Value;

                    var p1 = new LatLng(prevPoint.ShapePtLat, prevPoint.ShapePtLon);
                    var p2 = new LatLng(point.ShapePtLat, point.ShapePtLon);

                    trainPos = Maths.ToBearingDistance(percentDistance, p1, Maths.CalculateBearing(p1, p2).ToRadians());
                    break;
                }
            }

            return trainPos;
        }

        public bool IsTrainRunning(DateTime currentTime, TrackTrackerRepository repository )
        {
            var firstStopTime = StopTimes.FirstOrDefault();
            var lastStopTime = StopTimes.LastOrDefault();
            if (firstStopTime == null || lastStopTime == null)
            {
                TrainRunning = false;
                return false;
            }

            // ReSharper disable PossibleInvalidOperationException
            if (firstStopTime.DepartureTime.TimeOfDay < currentTime.TimeOfDay 
                && lastStopTime.ArrivalTime.TimeOfDay > currentTime.TimeOfDay)
            {
                TrainRunning = true;
                return repository.IsTripRunning(Trip, currentTime);
            }

            TrainRunning = false;
            return false;
        }
        private static int GetPreviousStopIndex(DateTime timeNow, List<StopTimeMinified> tripStops)
        {
            for (int i = 1; i < tripStops.Count; i++)
            {
                var currentStop = tripStops[i];
                if (currentStop.ArrivalTime == DateTime.MinValue) continue;

                if (currentStop.ArrivalTime.TimeOfDay > timeNow.TimeOfDay)
                {
                    return i - 1;
                }
            }
            return -1;
        }

        public void LoadAsync(TrackTrackerRepository repository)
        {
            if (!LoadingTask)
            {
                LoadingTask = true;
                Task.Factory.StartNew(() =>
                {
                    lock (repository)
                    {
                        Load(repository);
                    }
                });
            }
        }
        public class StopTimeMinified
        {
            internal string ArrivalTimeRaw { get; set; }
            public DateTime ArrivalTime => StopTime.GetTime(ArrivalTimeRaw);

            internal string DepartureTimeRaw { get; set; }
            public DateTime DepartureTime => StopTime.GetTime(DepartureTimeRaw);

            public double ShapeDistanceTraveled { get; set; }
            public int StopId { get; set; }

            public StopTimeMinified()
            {
                
            }
        }

    }

}