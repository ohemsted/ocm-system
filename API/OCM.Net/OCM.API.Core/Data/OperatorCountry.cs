//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OCM.Core.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class OperatorCountry
    {
        public int ID { get; set; }
        public int OperatorID { get; set; }
        public int CountryID { get; set; }
    
        public virtual Country Country { get; set; }
        public virtual Operator Operator { get; set; }
    }
}
