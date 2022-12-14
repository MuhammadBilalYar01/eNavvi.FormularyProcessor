namespace eNavvi.MedicareProcessor.Parser
{
    class PlanDTO
    {
        public PlanDTO() { }
        public PlanDTO(string contractid, string planid, string segmentid)
        {
            FullId = $"{contractid}{planid}{segmentid}";
        }
        public string FullId { get; set; }
        public string State { get; set; }
        public string FormId { get; set; }
        public string PlanName { get; set; }
        public string CountyCode { get; set; }

        public PlanDTO(string countyCode)
        {
            CountyCode = countyCode;
        }
    }
    internal class PlanParser
    {
        static string Path = "D:\\enavvi\\Medicare\\DataProcessing\\planinformation.txt";
        static string PLANINFO_DELIMITER = "|";
        private static List<CountyDTO> _counties;

        static PlanParser()
        {
            _counties = CountyParser.Parse();
        }
        public static List<PlanDTO> Parse()
        {
            var lines = File.ReadAllLines(Path);
            var tempPlans = (from fields in lines.Skip(1).Select(x => x.Split(PLANINFO_DELIMITER))
                             select new PlanDTO(fields[0], fields[1], fields[2])
                             {
                                 State = string.IsNullOrWhiteSpace(fields[11]) ? "*" : fields[11],
                                 PlanName = fields[4],
                                 FormId = fields[5],
                                 CountyCode = string.IsNullOrWhiteSpace(fields[12]) ? "*" : fields[12],
                             }).ToList();

            // Load county name from county codes
            tempPlans = (from x in tempPlans
                         join y in _counties on x.CountyCode equals y.CountyCode
                         select new PlanDTO
                         {
                             FullId = x.FullId,
                             State = x.State,
                             PlanName = x.PlanName,
                             CountyCode = y.CountyName,
                             FormId = x.FormId
                         }).ToList();

            // Combine all counties under same state, plan and id
            tempPlans = tempPlans.GroupBy(x => new { x.FullId, x.State, x.PlanName, x.FormId })
                         .Select(x => new PlanDTO
                         {
                             State = x.Key.State,
                             PlanName = x.Key.PlanName,
                             FullId = x.Key.FullId,
                             FormId = x.Key.FormId,
                             CountyCode = string.Join(",", x.Select(a => a.CountyCode).ToList())
                         }).ToList();

            // rename plan name under same id for differnt state
            var plan = new List<PlanDTO>();
            foreach (var item in tempPlans.GroupBy(x => new { x.PlanName }))
            {
                if (item.Count() == 1)
                    plan.Add(item.First());
                else
                {
                    var renamedPlan = (from x in item
                                       select new PlanDTO
                                       {
                                           State = x.State,
                                           PlanName = $"{x.PlanName}-({x.State})",
                                           FullId = x.FullId,
                                           CountyCode = x.CountyCode,
                                           FormId = x.FormId
                                       }).ToList();
                    plan.AddRange(renamedPlan);
                }
            }

            return plan;
        }
    }
}
