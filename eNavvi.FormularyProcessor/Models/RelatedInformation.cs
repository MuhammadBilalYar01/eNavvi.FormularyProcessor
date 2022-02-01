namespace eNavvi.FormularyProcessor.Models
{
    public class RelatedInformation
    {
        public string Rxcui { get; set; }
        public List<string> Ingredients { get; set; }
        public List<string> BrandNames { get; set; }
        public List<string> DosageGroup { get; set; }
        public List<string> DrugClasses { get; set; }
    }
}
