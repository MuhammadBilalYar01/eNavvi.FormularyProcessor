namespace eNavvi.MedicareProcessor.Parser
{
    class DrugNameDTO
    {
        public string Rxcui { get; set; }
        public string Name { get; set; }
    }
    internal class DrugNameParser
    {
        static string Path = "D:\\enavvi\\rxnorm-raw-data\\RXNCONSO-New.RRF";
        public static Dictionary<string, string> Parse(string path = "")

        {
            string filePath;
            if (string.IsNullOrEmpty(path))
                filePath = Path;
            else
                filePath = path;

            var lines = File.ReadAllLines(filePath);
            
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
