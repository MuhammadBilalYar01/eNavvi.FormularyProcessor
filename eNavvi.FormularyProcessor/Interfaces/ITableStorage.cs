using eNavvi.FormularyProcessor.Entities;
using Microsoft.Extensions.Logging;

namespace eNavvi.FormularyProcessor.Interfaces
{
    public interface ITableStorage
    {
        IEnumerable<Plan> GetAllUnProcessedPlans(bool isProcessed = false);
        IEnumerable<Plan> GetAllUnImportedPlans(bool isImported = false);
        void UpdatePlanProcessed(int? planId = null, bool? isProcessed = null, int? Processed = null);
        void UpdatePlanImported(int planId, bool isImported, int imported);
        List<string> GetUnProcessedRxcui(List<string> unProcessed);
    }
}
