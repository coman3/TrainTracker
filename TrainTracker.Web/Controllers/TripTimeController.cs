using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TrainTracker.Models;
using TrainTracker.Web.Helpers;
using TrainTracker.Web.Models;
using TrainTracker.Web.Repository;

namespace TrainTracker.Web.Controllers
{
    public class TripTimeController : PagedApiControler<TripTimeController.TripTimeRequest, StopTime>
    {
        private readonly TrackTrackerRepository _repository = new TrackTrackerRepository();
        public class TripTimeRequest : RequestData
        {
            public string Id { get; set; }
            public DateTime? AfterTime { get; set; }
        }

        protected override string TokenKey => "IS7DAqTg2aAuFXwe1HQK";

        protected override PagedList<StopTime> GetData(TripTimeRequest data)
        {
            return PageData(data,
                _repository.StopTimesCache.Where(x =>
                    x.TripId == data.Id)
                    .ToList()
                    .Where(x => !data.AfterTime.HasValue ||
                                data.AfterTime.Value.TimeOfDay < x.ArrivalTime.TimeOfDay
                    ).OrderBy(x => x.ArrivalTime));
        }
    }

    
}
