namespace eNavvi.FormularyProcessor.Models
{
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
    }
}
