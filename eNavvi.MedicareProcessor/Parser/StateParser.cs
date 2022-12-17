using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNavvi.MedicareProcessor.Parser
{
    internal class StateParser
    {
        private static readonly Dictionary<string, string> states = new Dictionary<string, string>();
        static StateParser()
        {
            states.Add("AL", "1");
            states.Add("AK", "2");
            states.Add("AZ", "3");
            states.Add("AR", "4");
            states.Add("CA", "5");
            states.Add("CO", "6");
            states.Add("CT", "7");
            states.Add("DE", "8");
            states.Add("FL", "9");
            states.Add("GA", "10");
            states.Add("HI", "11");
            states.Add("ID", "12");
            states.Add("IL", "13");
            states.Add("IN", "14");
            states.Add("IA", "15");
            states.Add("KS", "16");
            states.Add("KY", "17");
            states.Add("LA", "18");
            states.Add("ME", "19");
            states.Add("MD", "20");
            states.Add("MA", "21");
            states.Add("MI", "22");
            states.Add("MN", "23");
            states.Add("MS", "24");
            states.Add("MO", "25");
            states.Add("MT", "26");
            states.Add("NE", "27");
            states.Add("NV", "28");
            states.Add("NH", "29");
            states.Add("NJ", "30");
            states.Add("NM", "31");
            states.Add("NY", "32");
            states.Add("NC", "33");
            states.Add("ND", "34");
            states.Add("OH", "35");
            states.Add("OK", "36");
            states.Add("OR", "37");
            states.Add("PA", "38");
            states.Add("RI", "39");
            states.Add("SC", "40");
            states.Add("SD", "41");
            states.Add("TN", "42");
            states.Add("TX", "43");
            states.Add("UT", "44");
            states.Add("VT", "45");
            states.Add("VA", "46");
            states.Add("WA", "47");
            states.Add("DC", "47");
            states.Add("WV", "48");
            states.Add("WI", "49");
            states.Add("WY", "50");
            states.Add("PR", "51");
            states.Add("*", "52");
        }
        public static string Parse(string shortName)
        {
            if (string.IsNullOrWhiteSpace(shortName))
                shortName = "*";

            return states[shortName];
        }
    }
}
