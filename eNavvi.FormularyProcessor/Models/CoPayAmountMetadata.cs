using Newtonsoft.Json;

namespace eNavvi.FormularyProcessor.Models
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
}
