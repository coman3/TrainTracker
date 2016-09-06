using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using TrainTracker.Web.Hubs;
using TrainTracker.Web.Models;
using TrainTracker.Web.Repository;

namespace TrainTracker.Web.Helpers
{
    public class TrainWorker
    {
        private DateTime CurrentTime => DateTime.Now;
        private TimeSpan UpdateInterval => TimeSpan.FromMilliseconds(1000);
        private readonly TrackTrackerRepository _repository = new TrackTrackerRepository();
        private IHubConnectionContext<dynamic> Clients { get; set; }
        private ConcurrentDictionary<string, Bounds> 

        #region Thread Safe Values
        public bool HasLoaded => _trainGeneratorTask.IsCompleted && Trains != null && Trains.Count > 0;
        public static TrainWorker Instance => _instance.Value;

        private static readonly Lazy<TrainWorker> _instance = new Lazy<TrainWorker>(() => new TrainWorker(GlobalHost.ConnectionManager.GetHubContext<TrainHub>().Clients));
        private readonly Task _trainGeneratorTask;
        private readonly System.Threading.Timer _updateTrainPosistionTimer;
        private readonly object _updateTainsLock = new object();
        private volatile bool _updatingTrains = false;
        #endregion

        public List<Train> Trains { get; set; }
        public event UpdatedTrainHandler UpdatedTrains;
        
        public TrainWorker(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
            _updateTrainPosistionTimer = new System.Threading.Timer(UpdateTrainPosistions, null, UpdateInterval, UpdateInterval);
            _trainGeneratorTask = GenerateTrains();
        }

        public void UpdateTrainPosistions(object state)
        {
            lock (_updateTainsLock)
            {
                if (!_updatingTrains)
                {
                    _updatingTrains = true;
                    if (Trains == null || !_trainGeneratorTask.IsCompleted) return;
                    foreach (var train in Trains)
                    {
                        if (train.Loaded)
                            train.Update(CurrentTime);

                    }
                    UpdatedTrains?.Invoke(this, EventArgs.Empty);
                    _updatingTrains = false;
                }
            }
            
        }

        public async Task GenerateTrains()
        {
            await Task.Factory.StartNew(() =>
            {
                Trains = new List<Train>();
                foreach (var route in _repository.Routes)
                {
                    var runningTrips = _repository.GetRouteTrips(route, CurrentTime);
                    foreach (var runningTrip in runningTrips)
                    {
                        Trains.Add(GenerateTrain(runningTrip));
                    }
                }
            });
        }

        public Train GenerateTrain(Trip trip)
        {
            
            return new Train
            {
                Loaded = false,
                LastUpdated = DateTime.MinValue,
                TripHeadsign = trip.trip_headsign,
                Trip = trip,
                
            };
        }
    }
    public delegate void UpdatedTrainHandler(object sender, EventArgs args);
}