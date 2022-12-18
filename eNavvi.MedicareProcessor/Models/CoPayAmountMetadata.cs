using Newtonsoft.Json;

namespace eNavvi.MedicareProcessor.Models
{
    public class CoPayAmountMetadata
    {
        public string Tier { get; set; }
        public string Inw { get; set; }
        public string Onw { get; set; }
        public string Description { get; set; }
        public PrefPhases Prefphases { get; set; }
    }

    public class PrefPhases
    {
        public string phase1 { get; set; }
        public string phase2 { get; set; }
        public string phase3 { get; set; }
        public string phase4 { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class StandardizePlan
    {
        public string DrugName { get; set; }
        public string Rxcui { get; set; }
        public string Ndc { get; set; }
        public bool QuantityLimit { get; set; }
        public bool StepTherapy { get; set; }
        public bool PriorAuthorization { get; set; }
        //public string TierHigh { get; set; }
        public string TierLow { get; set; }
        public string Extra { get; set; }
        public string Price { get; set; }
        public bool IsValid { get; set; } = true;
        public int UpdatedMethod { get; set; }
    }

}
