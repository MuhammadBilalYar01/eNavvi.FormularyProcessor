using eNavvi.FormularyProcessor.Entities;

namespace eNavvi.FormularyProcessor.Interfaces
{
    public class DrugsDTO
    {
        public string DrugName { get; set; }
        public int? Rxcui { get; set; }

    }
    public interface ITableStorage
    {
        IEnumerable<Plan> GetAllUnProcessedPlans(bool isProcessed = false);
        IEnumerable<Plan> GetAllUnImportedPlans(bool isImported = false);
        void UpdatePlanProcessed(int? planId = null, bool? isProcessed = null, int? Processed = null);
        void UpdatePlanImported(int planId, bool isImported, int imported);
        List<string> GetUnProcessedExcui(List<string> unProcessed);
    }
}
