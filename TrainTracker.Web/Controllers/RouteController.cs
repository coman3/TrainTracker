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
    public class RouteController : PagedApiControler<RouteController.RouteRequestData, Route>
    {
        private readonly TrackTrackerRepository _repository = new TrackTrackerRepository();
        protected override string TokenKey => "zQFANtUT9hK8lK5b31nC";

        protected override PagedList<Route> GetData(RouteRequestData data)
        {
            return PageData(data,
                    _repository.Routes
                    .Where(x => x.route_type.HasValue && x.route_type.Value == data.RouteType)
                    .OrderBy(x => x.C_route_id));
        }

        public class RouteRequestData : RequestData
        {
            public int RouteType { get; set; }
        }
    }
}
