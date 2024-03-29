﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace eNavvi.FormularyProcessor.Entities
{
    public partial class DrugDetails
    {
        public int Id { get; set; }
        public long? Ndc { get; set; }
        public bool IsQuantityLimited { get; set; }
        public bool IsStepTherapy { get; set; }
        public bool IsPriorAuthorization { get; set; }
        public bool IsNotCover { get; set; }
        public DateTime PublishDate { get; set; }
        public int TierId { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string ExtraInfo { get; set; }
        public bool? Status { get; set; }
        public int? DrugId { get; set; }
        public string Price { get; set; }

        public virtual Drugs Drug { get; set; }
        public virtual DrugTier Tier { get; set; }
    }
}