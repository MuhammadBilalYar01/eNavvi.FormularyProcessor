using eNavvi.MedicareProcessor.Parser;
using eNavvi.MedicareProcessor.Service;
using System.Diagnostics;

namespace eNavvi.MedicareProcessor
{
    public class Program
    {
        public static readonly string PublishDate = "16 Nov 2022";
        static readonly object _object = new object();
        static void Main(string[] args)
        {

            Stopwatch t = new Stopwatch();
            t.Start();
            Console.WriteLine("=> Loading Plans");
            PlanService service = new PlanService();
            var plans = service.Execute();

            Console.WriteLine("=> Loading Drugs");
            DrugService drugService = new DrugService();
            drugService.Execute(plans);

            Console.WriteLine("=> Loading Benefit");
            BenefitService benefit= new BenefitService();
            benefit.Execute(plans);

            t.Stop();
            Console.WriteLine("Total time comsumed: " + t.Elapsed.TotalSeconds);
            Console.ReadKey();
        }
    }
}