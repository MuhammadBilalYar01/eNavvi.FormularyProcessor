﻿namespace eNavvi.FormularyProcessor.Models
{
    public class Configurations
    {
        public string AzureStorageConnection { get; set; }
        public string SQLConnection { get; set; }
        public string Plan_Container { get; set; }
        public string Formulary_Container { get; set; }
        public string Related_Container { get; set; }
        public string Medicare_Container { get; set; }
        public bool TraceEnabled { get; set; }
    }
}
