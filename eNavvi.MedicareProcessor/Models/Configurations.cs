using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNavvi.MedicareProcessor.Models
{
    public class Configurations
    {
        public string PublishDate { get; set; }
        public Sourcess Sourcess { get; set; }
        public string AzureStorageConnection { get; set; }
    }

    public class Sourcess
    {
        public string Benefit { get; set; }
        public string County { get; set; }
        public string DrugName { get; set; }
        public string Drug { get; set; }
        public string Plan { get; set; }
        public string Price { get; set; }
    }

}
