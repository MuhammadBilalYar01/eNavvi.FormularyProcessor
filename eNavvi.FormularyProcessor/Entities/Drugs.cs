﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace eNavvi.FormularyProcessor.Entities
{
    public partial class Drugs
    {
        public Drugs()
        {
            DrugDetails = new HashSet<DrugDetails>();
        }

        public int Id { get; set; }
        public string DrugName { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public int? Rxcui { get; set; }

        public virtual ICollection<DrugDetails> DrugDetails { get; set; }
    }
}