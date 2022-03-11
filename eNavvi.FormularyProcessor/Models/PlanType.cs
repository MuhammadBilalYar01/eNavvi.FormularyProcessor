using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNavvi.FormularyProcessor.Models
{
    public enum PlanType
    {
        [Description("Medi-Cal")]
        MediCal = 1,
        [Description("Medicaid")]
        Medicaid = 2,
        [Description("Medicare")]
        Medicare = 3,
        [Description("Marketplace")]
        Marketplace = 4,
    }

    public enum LastUpdatedMethod
    {
        [Description("Default")]
        Default = 0,
        [Description("Lookup")]
        Lookup = 1,
        [Description("Ingestion")]
        Ingestion = 2,
        [Description("Manual")]
        Manual = 3,
    }
}
