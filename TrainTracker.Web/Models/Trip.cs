//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TrainTracker.Web.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Trip
    {
        public int ID { get; set; }
        public string route_id { get; set; }
        public string service_id { get; set; }
        public string trip_id { get; set; }
        public string shape_id { get; set; }
        public string trip_headsign { get; set; }
        public Nullable<bool> direction_id { get; set; }
    }
}
