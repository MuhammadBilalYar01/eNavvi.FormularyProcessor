using eNavvi.MedicareProcessor.Models;
using eNavvi.MedicareProcessor.Parser;
using eNavvi.MedicareProcessor.Service;
using Newtonsoft.Json;
using System.Diagnostics;

namespace eNavvi.MedicareProcessor
{
    public class Program
    {
        public static readonly Configurations config;
        static readonly object _object = new object();
        static Program()
        {
            string data = File.ReadAllText("appsettings.json");
            config = JsonConvert.DeserializeObject<Configurations>(data);
        }
        static void Main(string[] args)
        {
            Stopwatch t = new Stopwatch();
            t.Start();
            Console.WriteLine("=> Loading Plans");
            List<PlanDTO> plans = PlanParser.Parse();

            Console.WriteLine("=> Loading Drugs");
            DrugService drugService = new DrugService();
            drugService.Execute(plans);

            Console.WriteLine("Writing plans");
            PlanService service = new PlanService();
            service.Execute(plans);

            Console.WriteLine("=> Loading Benefit");
            BenefitService benefit = new BenefitService();
            benefit.Execute(plans);

            t.Stop();
            Console.WriteLine("Total time comsumed: " + t.Elapsed.TotalSeconds);
        }
    }
}