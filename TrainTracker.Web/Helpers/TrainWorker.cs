using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
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
        private TimeSpan UpdateInterval => TimeSpan.FromMilliseconds(2000);
        private readonly TrackTrackerRepository _repository = new TrackTrackerRepository();
        private IHubConnectionContext<dynamic> Clients { get; set; }

        #region Thread Safe Values
        public bool HasLoaded => _generateThreadFinished && Trains != null && Trains.Count > 0;

        public static TrainWorker Instance => _instance.Value;
        private bool _generateThreadFinished;
        private static readonly Lazy<TrainWorker> _instance = new Lazy<TrainWorker>(() => new TrainWorker(GlobalHost.ConnectionManager.GetHubContext<TrainHub>().Clients));
        private readonly Task _trainGeneratorTask;
        private readonly System.Threading.Timer _updateTrainPosistionTimer;
        private readonly object _updateTainsLock = new object();
        private volatile bool _updatingTrains;
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
                    try
                    {
                        if (!HasLoaded) return;
                        foreach (var train in Trains)
                        {
                            if (train.Loaded)
                            {
                                train.Update(CurrentTime);
                                if(train.TrainRunning)
                                    Clients.All.trainUpdated(new { Position = train.Posistion, Id = train.TripId, Headsign = train.TripHeadsign});
                            }
                            else
                                train.LoadAsync(_repository);
                        }
                        UpdatedTrains?.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                    finally
                    {
                        _updatingTrains = false;
                    }
                    
                }
            }
            
        }

        public async Task GenerateTrains()
        {
            await Task.Factory.StartNew(() =>
            {
                Trains = new List<Train>();
                var tripTimes =
                    _repository.StopTimes.OrderBy(x => x.arrival_time)
                        .GroupBy(x => x.trip_id).ToDictionary(x=> x.Key, c=> c.ToList());
                Console.WriteLine(tripTimes.Count());
                foreach (var tripGroup in tripTimes)
                {

                    Trains.Add(GenerateTrain(tripGroup.Key, tripGroup.Value));
                    //var runningTrips = _repository.GetRouteTrips(route, CurrentTime);
                    //foreach (var runningTrip in runningTrips)
                    //{
                    //    
                    //}
                }
                _generateThreadFinished = true;
            });
        }

        public Train GenerateTrain(string tripId, List<Stop_times> stopTimes)
        {
            return new Train
            {
                Loaded = false,
                LastUpdated = DateTime.MinValue,
                TripId = tripId,
                StopTimes = stopTimes    
            };
        }
    }
    public delegate void UpdatedTrainHandler(object sender, EventArgs args);
}