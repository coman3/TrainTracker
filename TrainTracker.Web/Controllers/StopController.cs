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
    public class StopController : PagedApiControler<StopController.StopRequestData, Stop>
    {
        private readonly TrackTrackerRepository _repository = new TrackTrackerRepository();
        protected override string TokenKey => "Y4vqnNoff1fj68d7xOxb";

        protected override PagedList<Stop> GetData(StopRequestData data)
        {
            return PageData(data, 
                _repository.Stops
                .OrderBy(x => Maths.Distance(data.NearLatitude, data.NearLongitude, x.stop_lat, x.stop_lon)));
        }

        public class StopRequestData : RequestData
        {
            public double NearLatitude { get; set; }
            public double NearLongitude { get; set; }
        }

    }
}
