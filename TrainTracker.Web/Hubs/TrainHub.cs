using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Web.Hosting;
using Microsoft.AspNet.SignalR;
using TrainTracker.Helpers;
using TrainTracker.Models;
using TrainTracker.Web.Helpers;
using TrainTracker.Web.Models;

namespace TrainTracker.Web.Hubs
{
    public class TrainHub : Hub<ITrainHub>
    {
        private readonly TrainWorker _trainWorker;
        public const double MaxViewDistance = 10;
        public static ConcurrentDictionary<string, LatLng> UserCenters { get; set; }
        private static bool _eventsHooked;
        public TrainHub() : this(TrainWorker.Instance)
        {

        }

        public TrainHub(TrainWorker worker)
        {
            _trainWorker = worker;
            if (!_eventsHooked)
            {
                _eventsHooked = true;
                _trainWorker.OnUpdatedTrains += _trainWorker_OnUpdatedTrains;
                _trainWorker.OnGenerateStateChange += _trainWorker_OnGenerateStateChange;
            }
        }

        private static void _trainWorker_OnGenerateStateChange(TrainWorker sender, Models.GenerateStateArgs args)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<TrainHub, ITrainHub>();
            hubContext.Clients.All.GenerateStateChanged(args.NewState);
        }

        private void _trainWorker_OnUpdatedTrains(TrainWorker sender, List<Train> updateTrains)
        {
            if(UserCenters == null) return;
            if(_trainWorker.GenerateState < GenerateState.Loading) return;
            
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<TrainHub, ITrainHub>();
            foreach (var userBound in UserCenters)
            {
                var trains = updateTrains.Where(x => Maths.Distance(userBound.Value, x.Posistion) < MaxViewDistance)
                    .Select(x => new TrainUpdate(x.Posistion, x.TripId, x.TripHeadsign))
                    .ToList();
                hubContext.Clients.Client(userBound.Key).UpdateTrains(trains);
            }
        }

        public override Task OnConnected()
        {
            if (UserCenters == null) UserCenters = new ConcurrentDictionary<string, LatLng>();
            UserCenters[Context.ConnectionId] = LatLng.Invalid;
            return base.OnConnected();
        }

        public void SetCenter(LatLng point)
        {
            UserCenters[Context.ConnectionId] = point;
        }
    }

    public interface ITrainHub
    {
        void UpdateTrains(List<TrainUpdate> updates);
        void GenerateStateChanged(GenerateState state);
    }
}