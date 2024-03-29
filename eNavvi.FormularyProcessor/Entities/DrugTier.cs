﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace eNavvi.FormularyProcessor.Entities
{
    public partial class DrugTier
    {
        public DrugTier()
        {
            DrugDetails = new HashSet<DrugDetails>();
        }

        public int Id { get; set; }
        public string TierName { get; set; }
        public DateTime PublishDate { get; set; }
        public int PlanId { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public bool? Status { get; set; }

        public virtual Plan Plan { get; set; }
        public virtual ICollection<DrugDetails> DrugDetails { get; set; }
    }
}