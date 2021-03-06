﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OCM.API.Common.Model
{
    public class ChargePoint
    {
        [DisplayName("OCM Ref")]
        public int ID { get; set; }

        [DisplayName("Unique ID")]
        [StringLength(100)]
        public string UUID { get; set; }

        [DisplayName("Parent POI")]
        public int? ParentChargePointID { get; set; }

        public int? DataProviderID { get; set; }
        [DisplayName("Data Provider")]
        public DataProvider DataProvider { get; set; }
        
        [DisplayName("Data Providers Reference")]
        [StringLength(100)]
        public string DataProvidersReference { get; set; }

        public int? OperatorID { get; set; } 
        [DisplayName("Network/Operator")]
        public OperatorInfo OperatorInfo { get; set; }

        [DisplayName("Operators Own Ref")]
        [StringLength(100)]
        public string OperatorsReference { get; set; }

        public int? UsageTypeID { get; set; }
        [DisplayName("Usage Type")]
        public UsageType UsageType { get; set; }

        [DisplayName("Usage Cost")]
        [StringLength(200)]
        public string UsageCost { get; set; }

        [DisplayName("Nearest Address")]
        public AddressInfo AddressInfo { get; set; }

        [DisplayName("Number Of Stations/Bays")]
        [Range(0, 100)]
        public int? NumberOfPoints { get; set; }

        [DisplayName("General Comments"), DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string GeneralComments { get; set; }

        [DisplayName("Date Planned")]
        public DateTime? DatePlanned { get; set; }

        [DisplayName("Date Last Confirmed")]
        public DateTime? DateLastConfirmed { get; set; }

        public int? StatusTypeID { get; set; }
        [DisplayName("Operational Status")]
        public StatusType StatusType { get; set; }

        [DisplayName("Date Status Last Updated")]
        public DateTime? DateLastStatusUpdate { get; set; }

        [DisplayName("Data Quality Level")]
        [Range(1, 5)]
        public int? DataQualityLevel { get; set; }

        [DisplayName("Data Added")]
        public DateTime? DateCreated { get; set; }

        [DisplayName("Contributor")]
        public User Contributor { get; set; }

        public int? SubmissionStatusTypeID { get; set; }
        [DisplayName("Submission Status")]
        public SubmissionStatusType SubmissionStatus { get; set; }

        [DisplayName("Comments/Checkins")]
        public List<UserComment> UserComments { get; set; }

        [DisplayName("% Similarity")]
        public int? PercentageSimilarity { get; set; }

        [DisplayName("Equipment Info")]
        public List<ConnectionInfo> Connections { get; set; }

        [DisplayName("Media Items")]
        public List<MediaItem> MediaItems { get; set; }

        [DisplayName("Metadata")]
        public List<MetadataValue> MetadataValues { get; set; }


        #region deprecated properties
        [Obsolete, JsonIgnore]
        public List<ChargerInfo> Chargers { get; set; }
        [Obsolete, JsonIgnore]
        public string MetadataTags { get; set; }
        #endregion


#if !PORTABLE
        /// <summary>
        /// Get a simple summary description of the Charge Point including address/access info
        /// </summary>
        /// <param name="UseHTML"></param>
        /// <returns></returns>
        public string GetSummaryDescription(bool UseHTML)
        {
            string description = "";
            string address = "";
            string newline = "\r\n";
            if (UseHTML) newline = "<br/>";

            address = GetAddressSummary(UseHTML);

            if (!String.IsNullOrEmpty(address)) description = (!String.IsNullOrEmpty(description) ? description + newline : "") + address;

            if (!String.IsNullOrEmpty(GeneralComments)) description += newline + "<em>" + this.GeneralComments + "</em>";

            if (this.AddressInfo != null)
            {
                if (this.AddressInfo.AccessComments != null) description += newline + this.AddressInfo.AccessComments;
            }

            if (this.Connections != null)
            {
                if (this.Connections.Count > 0)
                {
                    description += newline + "Equipment:";
                    foreach (var c in this.Connections)
                    {
                        description += newline;
                        if (c.Level != null) description += c.Level.Title;
                        if (c.ConnectionType != null) description += " Connection Type:" + c.ConnectionType.Title;
                    }
                }
            }

            if (this.StatusType != null)
            {
                description += newline + "Status: " + this.StatusType.Title + " Last Updated " + (this.DateLastStatusUpdate.HasValue ? this.DateLastStatusUpdate.Value.ToShortDateString() : "");
            }

            description += newline + "<a href=\"http://openchargemap.org/site/poi/details/" + this.ID +
                           "\">View Details (OCM-" + this.ID + ")</a>";
            return description;
        }

#endif

        public string GetAddressSummary(bool UseHTML)
        {
            string address = "";

            if (this.AddressInfo != null)
            {
                if (this.AddressInfo.AddressLine1 != null) address += "\r\n" + this.AddressInfo.AddressLine1;
                if (this.AddressInfo.AddressLine2 != null) address += "\r\n" + this.AddressInfo.AddressLine2;
                if (this.AddressInfo.Town != null) address += "\r\n" + this.AddressInfo.Town;
                if (this.AddressInfo.StateOrProvince != null) address += "\r\n" + this.AddressInfo.StateOrProvince;
                if (this.AddressInfo.Postcode != null) address += "\r\n" + this.AddressInfo.Postcode;
            }

            return address;
        }
    }
}