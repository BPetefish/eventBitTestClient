//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eventBitTestClient.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class EntMap
    {
        public long MapID { get; set; }
        public string BoothTransform { get; set; }
        public string Description { get; set; }
        public Nullable<double> EstimatedMetersPerPixel { get; set; }
        public string ImageChunkKeyBase64 { get; set; }
        public Nullable<int> ImageHeight { get; set; }
        public Nullable<int> ImageWidth { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> MaxRasterZoom { get; set; }
        public string Name { get; set; }
        public string OverrideName { get; set; }
        public string sysChangeHashB64 { get; set; }
        public string sysColumnSigB64 { get; set; }
        public int sysEventID { get; set; }
        public Nullable<double> sysInsertDateEpoch { get; set; }
        public string sysInsertedBy { get; set; }
        public Nullable<double> sysRowStampNum { get; set; }
        public string sysRowState { get; set; }
        public Nullable<int> sysSyncEnterpriseID { get; set; }
        public Nullable<double> sysUpdateDateEpoch { get; set; }
        public string sysUpdatedBy { get; set; }
    }
}
