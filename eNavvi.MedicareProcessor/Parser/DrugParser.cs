namespace eNavvi.MedicareProcessor.Parser
{
    class Drug
    {
        public string FormularyId { get; set; }
        public string Rxcui { get; set; }
        public string Ndc { get; set; }
        public string Tier { get; set; }
        public bool ql { get; set; }
        public string ql_amount { get; set; }
        public string ql_days { get; set; }
        public bool pa { get; set; }
        public bool st { get; set; }
        public string extraInfo
        {
            get
            {
                return $"limit {ql_amount} in {ql_days} days; ";
            }
        }
        public string DrugName { get; set; }
        public string Price { get; set; }
    }
    internal class DrugParser
    {
        static readonly string Path;
        private static Dictionary<string, string> drugName;

        static DrugParser()
        {
            Path = $"Sources/{Program.config.Sourcess.Drug}";
            drugName = DrugNameParser.Parse();
            Console.WriteLine("Loaded Drug Names");
        }
        public static List<Drug> Parse()
        {
            var lines = File.ReadAllLines(Path);
            var drugs = (from fields in lines.Skip(1).Select(x => x.Split("|"))
                         select new Drug
                         {
                             FormularyId = fields[0],
                             Rxcui = fields[3],
                             Ndc = fields[4],
                             Tier = fields[5],
                             ql = fields[6] == "Y" ? true : false,
                             ql_amount = fields[7],
                             ql_days = fields[8],
                             pa = fields[9] == "Y" ? true : false,
                             st = fields[10] == "Y" ? true : false,
                         }).ToList();

            foreach (var item in drugs)
            {
                if (drugName.ContainsKey(item.Rxcui))
                {
                    item.DrugName = drugName[item.Rxcui];
                }
            }

            Console.WriteLine("Loaded drugs");
            return drugs;
        }

    }
}
