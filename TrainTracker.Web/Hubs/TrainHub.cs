using System;
using System.Threading;
using System.Timers;
using System.Web;
using System.Web.Hosting;
using Microsoft.AspNet.SignalR;
using TrainTracker.Web.Helpers;
using Timer = System.Timers.Timer;

namespace TrainTracker.Web.Hubs
{
    public class TrainHub : Hub
    {
        
        private readonly TrainWorker _trainWorker;

        public TrainHub() : this(TrainWorker.Instance)
        {

        }

        public TrainHub(TrainWorker worker)
        {
            _trainWorker = worker;
        }

        private void _trainWorker_UpdatedTrains(object sender, EventArgs args)
        {
            Clients.All.
        }

        public void RequestTrainLocations(string tripId, string time)
        {
            

            //Clients.Caller.updateTrainLocations(lastStopStop, nextStopStop, trainPos);
        }

    }

    
}