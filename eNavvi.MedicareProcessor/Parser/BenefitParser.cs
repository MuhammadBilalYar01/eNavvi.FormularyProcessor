using eNavvi.FormularyProcessor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNavvi.MedicareProcessor.Parser
{
    class BenefitDTO
    {
        public string Tier { get; set; }
        public string costtype_pref { get; set; }
        public string costamount_pref { get; set; }
        public string costtype_nonpref { get; set; }
        public string costamount_nonpref { get; set; }
        //public string contractid { get; set; }
        //public string planid { get; set; }
        //public string segmentid { get; set; }
        public string coveragelevel { get; set; }
        public string dayssupply { get; set; }
        public string FullId { get; set; }
    }
    internal class BenefitParser
    {
        static string Path = "D:\\enavvi\\Medicare/DataProcessing\\beneficiarycost.txt";
        public static List<CoPayAmountMetadata> Parse()
        {
            var lines = File.ReadAllLines(Path);
            var data = (from fields in lines.Select(x => x.Split("|"))
                        where fields[5] == "1"
                        select new BenefitDTO
                        {
                            Tier = fields[4],
                            costtype_pref = fields[6],
                            costamount_pref = fields[7],
                            costtype_nonpref = fields[10],
                            costamount_nonpref = fields[11],
                            //contractid = fields[0],
                            //planid = fields[1],
                            //segmentid = fields[2],
                            coveragelevel = fields[3],
                            dayssupply = fields[5],
                            FullId = fields[0] + fields[1] + fields[2]
                        }).ToList();

            List<CoPayAmountMetadata> coPayAmountMetadata = new List<CoPayAmountMetadata>();
            foreach (var item in data)
            {
                string copay30inw = "ERR";
                string phasedPay30inw = "";
                string phase = "phase" + (int.Parse(item.coveragelevel) + 1);

                // now need to decide if we're taking the preferred or the nonpreferred number
                if (item.costtype_pref == "0")
                {
                    if (item.costtype_nonpref == "1")
                        phasedPay30inw = "$" + item.costamount_nonpref;
                    else if (item.costtype_nonpref == "2")
                        if (item.costamount_nonpref.Contains("."))
                            phasedPay30inw = item.costamount_nonpref.Split(".")[1] + "%";
                        else
                            phasedPay30inw = item.costamount_nonpref + "%";
                    else if (item.costtype_nonpref == "0")
                        phasedPay30inw = "Not covered";
                }
                else //use the preferred
                {
                    if (item.costtype_pref == "1")
                        phasedPay30inw = "$" + item.costamount_pref;
                    else if (item.costtype_pref == "2")
                        if (item.costamount_pref.Contains("."))
                            phasedPay30inw = item.costamount_pref.Split(".")[1] + "%";
                        else
                            phasedPay30inw = item.costamount_pref + "%";
                    else if (item.costtype_pref == "0")
                        phasedPay30inw = "Not covered";

                }

                var copays = coPayAmountMetadata.Where(x => x.Description == item.FullId);
                bool found = false;
                foreach (var copay in copays)
                {
                    if (copay.Tier == item.Tier)
                    {
                        if (phase == "phase1")
                            copay.Prefphases.phase1 = phasedPay30inw;
                        else if (phase == "phase2")
                            copay.Prefphases.phase2 = phasedPay30inw;
                        else if (phase == "phase3")
                            copay.Prefphases.phase3 = phasedPay30inw;
                        else if (phase == "phase4")
                            copay.Prefphases.phase4 = phasedPay30inw;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    CoPayAmountMetadata coPay = new CoPayAmountMetadata
                    {
                        Tier = item.Tier,
                        Inw = copay30inw,
                        Description = item.FullId
                    };

                    PrefPhases prefphases = new PrefPhases();
                    prefphases.phase1 = phase == "phase1" ? phasedPay30inw : "N/A - contact plan administrator";
                    prefphases.phase2 = phase == "phase2" ? phasedPay30inw : "N/A - contact plan administrator";
                    prefphases.phase3 = phase == "phase3" ? phasedPay30inw : "N/A - contact plan administrator";
                    prefphases.phase4 = phase == "phase4" ? phasedPay30inw : "N/A - contact plan administrator";

                    coPay.Prefphases = prefphases;
                    coPayAmountMetadata.Add(coPay);
                }
            }
            return coPayAmountMetadata;
        }
    }
}
