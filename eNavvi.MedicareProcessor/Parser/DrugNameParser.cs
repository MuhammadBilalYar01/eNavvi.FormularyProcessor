namespace eNavvi.MedicareProcessor.Parser
{
    class DrugNameDTO
    {
        public string Rxcui { get; set; }
        public string Name { get; set; }
    }
    internal class DrugNameParser
    {
        static readonly string Path;
        static DrugNameParser()
        {
            Path = $"Sources/{Program.config.Sourcess.DrugName}";
        }
        public static Dictionary<string, string> Parse()
        {
            var lines = File.ReadAllLines(Path);
            
            var drugName = (from x in lines.Select(x => x.Split("|"))
                            select new DrugNameDTO
                            {
                                Rxcui = x[0],
                                Name = x[2]

                            }).ToList();
            
            return drugName.ToDictionary(x => x.Rxcui, x => x.Name);
        }
    }
}
