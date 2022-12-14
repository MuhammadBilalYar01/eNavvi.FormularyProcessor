using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNavvi.MedicareProcessor.Parser
{
    internal class PriceParser
    {
        static string Path = "D:\\enavvi\\Medicare\\DataProcessing\\";
        
        public static Dictionary<string, string> Parse(string name = "New-pricing")
        {
            Dictionary<string, string> prices = new Dictionary<string, string>();
            foreach (var item in name.Split(","))
            {
                if (!File.Exists(Path + name + ".txt"))
                    continue;
                var lines = File.ReadAllLines(Path + name + ".txt");
                foreach (var line in lines)
                {
                    var data = line.Split("|");
                    if (data.Count() > 1)
                    {
                        if (!prices.ContainsKey(data[0]))
                        {
                            prices.Add(data[0], data[1]);
                        }
                    }
                }

            }
            return prices;
        }
    }
}
