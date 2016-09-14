using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrainTracker.Models;
using TrainTracker.Web.Helpers;
using TrainTracker.Web.Models;
using TrainTracker.Web.Repository;

namespace TrainTracker.Web.Controllers
{
    public class StopTripController : PagedApiControler<StopTripController.StopTripRequest, Trip>
    {
        private readonly TrackTrackerRepository _repository = new TrackTrackerRepository();

        protected override int MaxResults => 10;
        protected override string TokenKey => "ifFpHTuYOMJarUjYpH50";

        protected override PagedList<Trip> GetData(StopTripRequest data)
        {
            return PageData(data,
                _repository.GetStopTrips(new Stop {StopId = data.Id})
                .OrderBy(x => x.TripId));
        }

        protected override IEnumerable<Trip> ProcessPagedData(IEnumerable<Trip> enumerable)
        {
            return enumerable.Select(x => new TripDetails(_repository, x));
        }

        public class StopTripRequest : RequestData 
        {
            public int Id { get; set; }
        }
    }
}