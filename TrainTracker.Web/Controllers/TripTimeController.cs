using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TrainTracker.Web.Helpers;
using TrainTracker.Web.Models;
using TrainTracker.Web.Repository;

namespace TrainTracker.Web.Controllers
{
    public class TripTimeController : PagedApiControler<TripTimeController.TripTimeRequest, Stop_times>
    {
        private readonly TrackTrackerRepository _repository = new TrackTrackerRepository();
        public class TripTimeRequest : RequestData
        {
            public string Id { get; set; }
            public DateTime? AfterTime { get; set; }
        }

        protected override string TokenKey => "IS7DAqTg2aAuFXwe1HQK";

        protected override PagedList<Stop_times> GetData(TripTimeRequest data)
        {
            return PageData(data,
                _repository.StopTimes.Where(x =>
                    x.C_trip_id == data.Id &&
                    x.arrival_time.HasValue).ToList()
                    .Where(x => !data.AfterTime.HasValue ||
                                data.AfterTime.Value.TimeOfDay < x.arrival_time.Value.TimeOfDay
                    ).OrderBy(x => x.arrival_time));
        }
    }

    
}
