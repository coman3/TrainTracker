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
    
    public partial class Shape
    {
        public int ID { get; set; }
        public string C_shape_id { get; set; }
        public Nullable<double> shape_pt_lat { get; set; }
        public Nullable<double> shape_pt_lon { get; set; }
        public Nullable<short> shape_pt_sequence { get; set; }
        public Nullable<double> shape_dist_traveled { get; set; }
    }
}