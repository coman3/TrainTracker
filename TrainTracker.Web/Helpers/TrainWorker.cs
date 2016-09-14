using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Microsoft.ApplicationInsights;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using TrainTracker.Models;
using TrainTracker.Web.Hubs;
using TrainTracker.Web.Models;
using TrainTracker.Web.Repository;

namespace TrainTracker.Web.Helpers
{
    public class TrainWorker : IRegisteredObject
    {
        private TelemetryClient _telemetry = new TelemetryClient();
        private DateTime CurrentTime => DateTime.Now.AddHours(-9);
        private TimeSpan UpdateInterval => TimeSpan.FromMilliseconds(500);
        private readonly TrackTrackerRepository _repository = new TrackTrackerRepository();
        private IHubConnectionContext<dynamic> Clients { get; set; }
        private Stopwatch _stateChangeStopwatch;

        #region Thread Safe Values

        public bool HasLoaded => _generateThreadFinished && Trains != null && Trains.Count > 0;

        public static TrainWorker Instance => _instance.Value;
        private bool _generateThreadFinished;

        private static readonly Lazy<TrainWorker> _instance =
            new Lazy<TrainWorker>(() => new TrainWorker(GlobalHost.ConnectionManager.GetHubContext<TrainHub>().Clients));

        private readonly Task _trainGeneratorTask;
        private readonly System.Threading.Timer _updateTrainPosistionTimer;
        private readonly object _updateTainsLock = new object();
        private volatile bool _updatingTrains;
        
        #endregion

        public event OnUpdatedTrains OnUpdatedTrains;
        public event OnGenerateStateChange OnGenerateStateChange;
        public List<Train> Trains { get; set; }

        public TrainWorker(IHubConnectionContext<dynamic> clients)
        {
            _stateChangeStopwatch = Stopwatch.StartNew();
            _telemetry.TrackEvent("TrainWorkerConstruct", new Dictionary<string, string>
            {
                ["Time"] = DateTime.Now.ToString("F"),
            });
            GenerateState = GenerateState.Nothing;
            Clients = clients;
            _updateTrainPosistionTimer = new Timer(UpdateTrainPosistions, null, UpdateInterval, UpdateInterval);
            _trainGeneratorTask = GenerateTrains();
            OnGenerateStateChange += TrainWorker_OnGenerateStateChange;
        }

        private void TrainWorker_OnGenerateStateChange(TrainWorker sender, GenerateStateArgs args)
        {
            _stateChangeStopwatch.Stop();
            _telemetry.TrackEvent("TrainWorkerStateChange", new Dictionary<string, string>()
            {
                ["NewState"] = args.NewState.ToString(),
                ["OldState"] = args.OldState.ToString()
            }, new Dictionary<string, double>
            {
                ["Time"] = _stateChangeStopwatch.Elapsed.TotalMilliseconds,
            });
            Console.WriteLine("Train Worker State Changed");
            if (args.NewState == GenerateState.Finished)
            {
                _repository.UnloadCache();
                Console.WriteLine("Clearing Unessasary Cache (May not do anything RAM usage wise)");
            }
            if (args.NewState < GenerateState.Finished)
            {
                _stateChangeStopwatch = Stopwatch.StartNew();
            }

        }

        #region Train Update

        private void UpdateTrainPosistions(object state)
        {
            lock (_updateTainsLock)
            {
                if (!_updatingTrains)
                {
                    _updatingTrains = true;
                    var trainsUpdated = new List<Train>();
                    try
                    {
                        if (!HasLoaded) return;
                        var trains = Trains.Where(x => x.IsTrainRunning(CurrentTime, _repository)).ToList();
                        var countLoaded = 0;
                        foreach (var train in trains)
                        {
                            if (train.Loaded)
                            {
                                train.Update(CurrentTime);
                                trainsUpdated.Add(train);
                                countLoaded++;
                            }
                            else
                            {
                                train.LoadAsync(_repository);
                            }
                                
                        }
                        if (countLoaded >= trains.Count && GenerateState == GenerateState.Loading)
                        {
                            GenerateState = GenerateState.Finished;
                        }
                    }
                    finally
                    {
                        OnUpdatedTrains?.Invoke(this, trainsUpdated);
                        _updatingTrains = false;
                    }
                }
            }

        }

        #endregion

        #region Train Generation

        private async Task GenerateTrains()
        {
            var stopWatch = Stopwatch.StartNew();
            await Task.Factory.StartNew(() =>
            {
                GenerateState = GenerateState.Generating;
                Trains = new List<Train>();
                var tripTimes =
                    _repository.StopTimes.GroupBy(x => x.TripId)
                        .OrderBy(x => x.Key)
                        .Select(
                            x => new
                            {
                                x.Key,
                                Items = x.Select(i => new Train.StopTimeMinified
                                {
                                    ArrivalTimeRaw = i.ArrivalTimeRaw,
                                    DepartureTimeRaw = i.DepartureTimeRaw,
                                    StopId = i.StopId
                                })
                            })
                        .ToList();
                Parallel.ForEach(tripTimes, tripGroup =>
                {
                    var train = GenerateTrain(tripGroup.Key,
                        tripGroup.Items.OrderBy(x => x.ArrivalTime).ToList());
                    lock (Trains)
                    {
                        Trains.Add(train);
                    }
                });
                _generateThreadFinished = true;
                GenerateState = GenerateState.Loading;
            });
            stopWatch.Stop();
            _telemetry.TrackEvent("TrainWorkerGenerate", null, new Dictionary<string, double>
            {
                ["Time"] = stopWatch.Elapsed.TotalMilliseconds,
                ["TrainsGenerated"] = Trains.Count,

            });
        }

        private Train GenerateTrain(string tripId, List<Train.StopTimeMinified> stopTimes)
        {
            dynamic trip;
            lock (_repository)
            {
                trip =
                    _repository.TripsCache.First(x=> x.TripId == tripId);
            }
            return new Train
            {
                Loaded = false,
                TripId = tripId,
                Trip = trip,
                StopTimes = stopTimes,
            };
        }

        #endregion

        #region Event Handlers

        private GenerateState _currentGenerateStateValue = GenerateState.Nothing;
        public GenerateState GenerateState
        {
            get { return _currentGenerateStateValue; }
            private set
            {
                OnGenerateStateChange?.Invoke(this, new GenerateStateArgs(value, _currentGenerateStateValue));
                _currentGenerateStateValue = value;
            }
        }
        #endregion

        public void Stop(bool immediate)
        {
            
        }

       
    }

    public delegate void OnGenerateStateChange(TrainWorker sender, GenerateStateArgs args);
    public delegate void OnUpdatedTrains(TrainWorker sender, List<Train> updatedTrains);
}