using eNavvi.MedicareProcessor.Models;
using eNavvi.MedicareProcessor.Parser;
using Newtonsoft.Json;

namespace eNavvi.MedicareProcessor.Service
{
    internal class DrugService
    {
        static readonly object _object = new object();
        public void Execute(List<PlanDTO> plans)
        {
            var drugs = DrugParser.Parse();
            int drugWithoutName = drugs.Where(x => string.IsNullOrEmpty(x.DrugName)).Count();

            Console.WriteLine("Drugs without name: " + drugWithoutName);

            var prices = PriceParser.Parse();
            Console.WriteLine("Price Loadded");
            foreach (var drug in drugs)
            {
                if (prices.ContainsKey(drug.Ndc))
                    drug.Price = prices[drug.Ndc];
            }

            int missingprice = drugs.Where(x => string.IsNullOrEmpty(x.Price)).Count();
            Console.WriteLine("Drugs without price: " + missingprice);

            Directory.CreateDirectory("Formularies");
            Console.WriteLine("Total Plan: " + plans.Count);
            Parallel.ForEach(plans, item =>
            {
                Console.WriteLine(item.PlanName);
                var data = drugs.Where(x => x.FormularyId == item.FormId).Select(x => new StandardizePlan
                {
                    DrugName = x.DrugName,
                    Rxcui = x.Rxcui,
                    Ndc = x.Ndc,
                    QuantityLimit = x.ql,
                    StepTherapy = x.st,
                    PriorAuthorization = x.pa,
                    TierLow = x.Tier,
                    Extra = x.extraInfo,
                    Price = x.Price,
                    UpdatedMethod = 2
                }).ToList();
                item.Processed = data.Count;
                Directory.CreateDirectory("Formularies/" + item.PlanName.Replace(":", "").Replace("*", ""));
                lock (_object)
                    File.WriteAllText($"Formularies/{item.PlanName.Replace(":", "").Replace("*", "")}/Formulary.json", JsonConvert.SerializeObject(data));
            });
            foreach (var item in plans.GroupBy(x => x.Processed))
            {
                Console.WriteLine($"{item.Key}=>{item.Count()}");
            }
            Console.WriteLine("Drug Processing completed");
        }
    }
}
