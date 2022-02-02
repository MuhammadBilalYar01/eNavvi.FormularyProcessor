using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNavvi.FormularyProcessor.Models
{
    public class DrugDTO
    {
        public int DrugId { get; set; }
        public bool IsQuantityLimited { get; set; }
        public bool IsStepTherapy { get; set; }
        public bool IsPriorAuthorization { get; set; }
        public string DrugName { get; set; }
        public int? Rxcui { get; set; }
        public string PlanName { get; set; }
        public int PlanId { get; set; }
    }
}
