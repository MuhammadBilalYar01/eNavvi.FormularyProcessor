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
    }
}
