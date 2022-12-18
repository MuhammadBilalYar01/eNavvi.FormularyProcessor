namespace eNavvi.MedicareProcessor.Parser
{
    class CountyDTO
    {
        public string CountyCode { get; set; }
        public string CountyName { get; set; }
    }
    internal class CountyParser
    {
        static readonly string Path ;
        static CountyParser()
        {
            Path = $"Sources/{Program.config.Sourcess.County}";
        }
        public static List<CountyDTO> Parse()
        {
            var lines = File.ReadAllLines(Path);
            var counties = (from x in lines.Select(x => x.Split("|"))
                            select new CountyDTO
                            {
                                CountyCode = x[0],
                                CountyName = x[2]

                            }).ToList();
            counties.Add(new CountyDTO { CountyCode = "*", CountyName = "*" });
            return counties;
        }
    }
}
