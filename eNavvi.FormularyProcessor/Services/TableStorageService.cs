using eNavvi.FormularyProcessor.Data;
using eNavvi.FormularyProcessor.Entities;
using eNavvi.FormularyProcessor.Interfaces;

namespace eNavvi.FormularyProcessor.Services
{
   public class TableStorageService : ITableStorage
    {
        public readonly eNavviContext _db;
        public TableStorageService(eNavviContext context)
        {
            this._db = context;
        }

        public IEnumerable<Plan> GetAllUnProcessedPlans(bool isProcessed = false)
        {
            return this._db.Plan.Where(x => x.IsProcessed == isProcessed && !String.IsNullOrEmpty(x.Url) && x.Status == true);
        }

        public void UpdatePlanProcessed(int? planId = null, bool? isProcessed = null, int? processed = null)
        {
            if (null != planId && isProcessed != null && processed != null)
            {
                var plan = this._db.Plan.Where(x => x.Id == planId).FirstOrDefault();
                if (null != plan)
                {
                    plan.IsProcessed = isProcessed.Value;
                    plan.Processed = processed.Value;
                    plan.IsImported = false;
                    plan.Imported = 0;
                    this._db.SaveChanges();
                }
                else
                    throw new Exception("No plan found with id " + planId);
            }
            else
            {
                foreach (var item in this.GetAllUnProcessedPlans(true))
                {
                    item.Processed = 0;
                    item.IsProcessed = false;
                }
                this._db.SaveChanges();
            }
        }

        public void UpdatePlanImported(int planId, bool isImported, int imported)
        {
            var plan = this._db.Plan.Where(x => x.Id == planId).FirstOrDefault();
            if (null != plan)
            {
                plan.IsImported = isImported;
                plan.Imported = imported;
                this._db.SaveChanges();
            }
            else
                throw new Exception("No plan found with id " + planId);
        }

        public IEnumerable<Plan> GetAllUnImportedPlans(bool isImported = false)
        {

            var results = this._db.Plan
                          .Where(x => x.IsProcessed == true && x.IsImported == isImported);

            return results;
        }

        public List<string> GetUnProcessedRxcui(List<string> unProcessed)
        {
            List<string> processed = this._db.RelatedInfo.Select(x => x.Rxcui.ToString()).ToList();

            List<string> newUnProcessed = unProcessed.Where(x => !processed.Contains(x)).ToList();

            return newUnProcessed;
        }
    }
}
