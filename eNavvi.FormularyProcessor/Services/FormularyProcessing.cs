using eNavvi.FormularyProcessor.Interfaces;
using eNavvi.FormularyProcessor.Models;
using eNavvi.FormularyProcessor.Utilities;
using Newtonsoft.Json.Linq;
using Serilog;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using eNavvi.FormularyProcessor.Entities;
using eNavvi.FormularyProcessor.Models.RxNorm;

namespace eNavvi.FormularyProcessor.Services
{
    public class FormularyProcessing : IFormularyProcessing
    {
        private readonly IBlobStorage _blobStorage = null;
        private readonly ITableStorage _tableStorage = null;
        private readonly IRxNormUtility _rxNormUtility = null;
        private ExternalService _service = null;
        private readonly Configurations _config = null;
        private bool _isTimeOut = false;

        static readonly object _object = new object();

        public FormularyProcessing(IBlobStorage blobStorage, ITableStorage tableStorage, IConfiguration config, IRxNormUtility rxNormUtility)
        {
            this._blobStorage = blobStorage;
            this._tableStorage = tableStorage;
            Configurations option = new Configurations();
            config.Bind(option);
            this._config = option;
            this._rxNormUtility = rxNormUtility;
        }

        public async Task Run()
        {
            Log.Information($"FormularyProcessing executed at: {DateTime.Now}");
            DateTime start = DateTime.Now;
            Log.Logger = Log.ForContext("RunID", Guid.NewGuid());

            List<Plan> allPlan = this._tableStorage.GetAllUnProcessedPlans().Where(x => x.Processed == 0).ToList();
            if (allPlan.Count == 0)
            {
                Log.Information("No plan for processing");
                return;
            }

            this._service = new ExternalService(this._config);
            Log.Information($"Total Unprocessed plans: {allPlan.Count}");
            int i = 1;
            foreach (var item in allPlan)
            {
                try
                {
                    Log.Logger = Log.ForContext("ActivityId", Guid.NewGuid());
                    Log.Information($"Proceeing Plan No:{i++}, ID: {item.Id}");
                    Log.Logger = Log.ForContext("Plan", JsonConvert.SerializeObject(item));

                    if (this._isTimeOut)
                    {
                        Log.Information("Stopping function as it will timeout soon.");
                        break;
                    }
                    Log.Information($"processing plan: {item.Name}");
                    string previousPlan = await this._blobStorage.DownloadBlob(this._config.Plan_Container, item.Name);

                    string newPlan = this._service.MakeHttpRequest(item.Url);//File.ReadAllText(@"C:\Users\Muhammad Bilal\Desktop\AZ_Medicaid.json");// 
                    if (previousPlan == newPlan && item.Processed == 0)
                    {
                        Log.Information($"{ item.Name}: Previous and new plan data is same.");
                        this._tableStorage.UpdatePlanProcessed(item.Id, true, 0);
                        continue;
                    }

                    JArray newPlanData = this.ParseResult(item, newPlan);

                    List<StandardizePlan> plans = new List<StandardizePlan>();

                    Log.Information($"Total: {newPlanData.Count}, Skipping: {item.Processed} drugs, as its alaredy processed.");
                    if (item.IsSpecial)
                    {
                        plans = await ProcessNdcBasedPlan(newPlanData);
                    }
                    else if (item.Type == PlanType.Medicare)
                    {
                        plans = await ProcessMedicarePlan(newPlanData);
                    }
                    else
                    {
                        plans = ProcessNormalPlan(newPlanData, item);
                    }

                    //File.WriteAllText("D:/test/processed.json", JsonConvert.SerializeObject(plans));

                    if (plans.Count > 0)
                    {
                        Log.Information($"Converting into Json format.");

                        await this._blobStorage.MergeFormulary($"{item.Name}/Formulary.json", plans);
                        Log.Information($"Formulary Uploaded");

                        Log.Information($"Uploading unprocessed Rxcui.");
                        await this._blobStorage.MergeRxcui(this._tableStorage.GetUnProcessedRxcui(plans.Select(x => x.Rxcui).ToList()).ToList());
                        Log.Information($"Unprocessed Rxcui uploading completed.");

                        await this._blobStorage.UploadBlob(this._config.Plan_Container, item.Name, newPlan);

                        this._tableStorage.UpdatePlanProcessed(item.Id, true, 0);
                        Log.Information($"Formulary Processing Completed.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, ex.Message);
                }
            }
        }
        private async Task<List<StandardizePlan>> ProcessNdcBasedPlan(JArray newPlanData)
        {
            var drugTypes = new List<string>() { "GPCK", "SCDC", "BPCK", "SCD", "SBD" };
            var rxNormSat = (await this._rxNormUtility.GetData<RxNormSat>("RXNSAT.RRF")).Where(x => x.ATN == "NDC").GroupBy(x => x.ATV).ToList();
            var test = await this._rxNormUtility.GetData<RxNormConcept>("RXNCONSO.RRF");
            var rxNormConcept = test.Where(x => drugTypes.Contains(x.TTY));
            var plans = (from x in newPlanData
                         join y in rxNormSat on double.Parse(x["ndc"].ToString()).ToString() equals y.Key.ToString() into matched
                         from y in matched.DefaultIfEmpty()
                         select new StandardizePlan
                         {
                             Ndc = x["ndc"].ToString(),
                             DrugName = x["ndc_label_name"].ToString(),
                             Rxcui = y == null ? "0" : GetRxcui(y),
                             QuantityLimit = x["quantity_limit"] != null && x["quantity_limit"].ToString() != "" ? (bool)x["quantity_limit"] : false,
                             StepTherapy = x["step_therapy"] != null && x["step_therapy"].ToString() != "" ? (bool)x["step_therapy"] : false,
                             PriorAuthorization = x["prior_authorization"] != null && x["prior_authorization"].ToString() != "" ? (bool)x["prior_authorization"] : false,
                             TierLow = x["drug_tier"].ToString(),
                             Extra = x["extra"] != null ? x["extra"].ToString() : null
                         }).ToList();

            plans = (from x in plans
                     join y in rxNormConcept on x.Rxcui.Split("_")[0] equals y.RXCUI into matched
                     from y in matched.DefaultIfEmpty()
                     select new StandardizePlan
                     {
                         Ndc = x.Ndc,
                         DrugName = GetDrugName(x.DrugName, new List<RxNormConcept> { y }),
                         Rxcui = x.Rxcui,
                         QuantityLimit = x.QuantityLimit,
                         StepTherapy = x.StepTherapy,
                         PriorAuthorization = x.PriorAuthorization,
                         TierLow = x.TierLow,
                         Extra = x.Extra
                     }).ToList();


            List<StandardizePlan> ppp = new List<StandardizePlan>();
            foreach (var item1 in plans.GroupBy(x => x.DrugName.ToLower()))
            {
                if (item1.Count() > 1)
                {
                    var xc = item1.GroupBy(x => x.Rxcui).Where(x => x.Count() > 1).ToList();
                    if (xc.Count() > 1)
                    {
                        var cc = xc.Where(x => x.Key.Contains("RXNORM")).FirstOrDefault();
                        if (null != cc)
                        {
                            var p = new StandardizePlan
                            {
                                Ndc = item1.Count().ToString(),
                                DrugName = item1.Key,
                                Rxcui = cc.Key.Split("_")[0],
                                QuantityLimit = item1.FirstOrDefault().QuantityLimit,
                                StepTherapy = item1.FirstOrDefault().StepTherapy,
                                PriorAuthorization = item1.FirstOrDefault().PriorAuthorization,
                                TierLow = item1.FirstOrDefault().TierLow,
                                Extra = string.Join(",", item1.Select(x => x.Extra).Where(x => !string.IsNullOrEmpty(x)).ToArray())
                            };
                            if (!ppp.Any(x => x.DrugName == item1.Key))
                                ppp.Add(p);
                        }
                        else
                        {
                            string rxcui1 = xc.Where(x => x.Key != "0").FirstOrDefault().Key.Split("_")[0];
                            var p = new StandardizePlan
                            {
                                Ndc = item1.Count().ToString(),
                                DrugName = item1.Key,
                                Rxcui = rxcui1,
                                QuantityLimit = item1.FirstOrDefault().QuantityLimit,
                                StepTherapy = item1.FirstOrDefault().StepTherapy,
                                PriorAuthorization = item1.FirstOrDefault().PriorAuthorization,
                                TierLow = item1.FirstOrDefault().TierLow,
                                Extra = string.Join(",", item1.Select(x => x.Extra).Where(x => !string.IsNullOrEmpty(x)).ToArray())
                            };
                            if (!ppp.Any(x => x.DrugName == item1.Key))
                                ppp.Add(p);

                        }
                    }
                    else
                    {
                        var p = new StandardizePlan
                        {
                            Ndc = item1.Count().ToString(),
                            DrugName = item1.Key,
                            Rxcui = xc.Count() == 0 ? "0" : xc.FirstOrDefault().Key.Split("_")[0],
                            QuantityLimit = item1.FirstOrDefault().QuantityLimit,
                            StepTherapy = item1.FirstOrDefault().StepTherapy,
                            PriorAuthorization = item1.FirstOrDefault().PriorAuthorization,
                            TierLow = item1.FirstOrDefault().TierLow,
                            Extra = string.Join(",", item1.Select(x => x.Extra).Where(x => !string.IsNullOrEmpty(x)).ToArray())
                        };
                        if (!ppp.Any(x => x.DrugName == item1.Key))
                            ppp.Add(p);
                    }
                }
                else
                {
                    var p = new StandardizePlan
                    {
                        Ndc = item1.Count().ToString(),
                        DrugName = item1.Key,
                        Rxcui = item1.FirstOrDefault().Rxcui.Split("_")[0],
                        QuantityLimit = item1.FirstOrDefault().QuantityLimit,
                        StepTherapy = item1.FirstOrDefault().StepTherapy,
                        PriorAuthorization = item1.FirstOrDefault().PriorAuthorization,
                        TierLow = item1.FirstOrDefault().TierLow,
                        Extra = string.Join(",", item1.Select(x => x.Extra).Where(x => !string.IsNullOrEmpty(x)).ToArray())
                    };

                    if (!ppp.Any(x => x.DrugName == item1.Key))
                        ppp.Add(p);
                }
            }

            Parallel.ForEach(ppp.Where(x => x.Rxcui == "0" || x.Rxcui == null).ToList(), x =>
            {
                var xx = this._service.GetRxcuiByDrugName(x.DrugName);
                lock (_object)
                {
                    if (!string.IsNullOrEmpty(xx))
                    {
                        x.Rxcui = xx;
                    }
                    else
                        x.Rxcui = null;
                }
            });

            return ppp;
        }

        private async Task<List<StandardizePlan>> ProcessMedicarePlan(JArray newPlanData)
        {
            var drugTypes = new List<string>() { "GPCK".ToLower(), "SCDC".ToLower(), "BPCK".ToLower(), "SCD".ToLower(), "SBD".ToLower() };
            var test = await this._rxNormUtility.GetData<RxNormConcept>("RXNCONSO.RRF");

            var rxNormConcept = test.Where(x => drugTypes.Contains(x.TTY.Trim().ToLower())).ToList();
            var plans = (from x in newPlanData
                         join y in rxNormConcept on x["rxnorm_id"].ToString() equals y.RXCUI into matched
                         from y in matched.DefaultIfEmpty()
                         select new StandardizePlan
                         {
                             Ndc = x["ndc"].ToString(),
                             DrugName = y == null ? null : y.STR,
                             Rxcui = x["rxnorm_id"].ToString(),
                             QuantityLimit = x["plans"][0]["quantity_limit"] != null && x["plans"][0]["quantity_limit"].ToString() != "" ? (bool)x["plans"][0]["quantity_limit"] : false,
                             StepTherapy = x["plans"][0]["step_therapy"] != null && x["plans"][0]["step_therapy"].ToString() != "" ? (bool)x["plans"][0]["step_therapy"] : false,
                             PriorAuthorization = x["plans"][0]["prior_authorization"] != null && x["plans"][0]["prior_authorization"].ToString() != "" ? (bool)x["plans"][0]["prior_authorization"] : false,
                             TierLow = x["plans"][0]["drug_tier"].ToString(),
                             Extra = x["plans"][0]["extraInfo"] != null ? x["plans"][0]["extraInfo"].ToString() : null
                         }).ToList();
            var emptyPlan = plans.Where(x => string.IsNullOrEmpty(x.DrugName)).ToList();
            if (emptyPlan.Count > 0)
            {
                Log.Warning($"There are {emptyPlan.Count} empty drugs. {JsonConvert.SerializeObject(emptyPlan)}");
            }
            return plans;
        }

        private List<StandardizePlan> ProcessNormalPlan(JArray newPlanData, Plan item)
        {
            List<StandardizePlan> plans = new List<StandardizePlan>();
            foreach (var x in newPlanData)
            {
                JToken plan = this.GetPlanBlock(item, x);

                var standardizePlan = PopulateStandardProperties(x);
                if (standardizePlan != null && !string.IsNullOrEmpty(standardizePlan.DrugName))
                {
                    standardizePlan.QuantityLimit = plan["quantity_limit"] != null && plan["quantity_limit"].ToString() != "" ? (bool)plan["quantity_limit"] : false;
                    standardizePlan.StepTherapy = plan["step_therapy"] != null && plan["step_therapy"].ToString() != "" ? (bool)plan["step_therapy"] : false;
                    standardizePlan.PriorAuthorization = plan["prior_authorization"] != null && plan["prior_authorization"].ToString() != "" ? (bool)plan["prior_authorization"] : false;
                    //standardizePlan.TierHigh = plan["drug_tier"].ToString();
                    standardizePlan.TierLow = plan["drug_tier"].ToString();
                    standardizePlan.Extra = plan["extra"] != null ? plan["extra"].ToString() : null;

                    int count = plans.Where(x => x.DrugName.Trim().ToLower() == standardizePlan.DrugName.Trim().ToLower() && x.Rxcui.Trim().ToLower() == standardizePlan.Rxcui.Trim().ToLower()).Count();
                    if (count == 0)
                        plans.Add(standardizePlan);
                }
            }
            var uniquePlan = new List<StandardizePlan>();
            foreach (var duplicate in plans.GroupBy(x => x.DrugName.ToLower().Trim()))
            {
                if (duplicate.Count() == 1)
                    uniquePlan.Add(duplicate.First());
                else
                {
                    bool matched = false;
                    foreach (var drug in duplicate)
                    {
                        string drugName = _service.GetDrugNameByRxcui(drug.Rxcui);
                        if (!string.IsNullOrEmpty(drugName) && drugName.Trim().ToLower() == drug.DrugName.Trim().ToLower())
                        {
                            uniquePlan.Add(drug);
                            matched = true;
                            break;
                        }
                    }
                    if (!matched)
                    {
                        foreach (var drug in duplicate)
                        {
                            string rxcui = _service.GetRxcuiByDrugName(drug.DrugName);
                            if (!string.IsNullOrEmpty(rxcui) && rxcui.Trim().ToLower() == drug.Rxcui.Trim().ToLower())
                            {
                                uniquePlan.Add(drug);
                                matched = true;
                                break;
                            }
                        }
                    }
                    if (!matched)
                    {
                        var drug = duplicate.FirstOrDefault();
                        drug.Rxcui = null;
                        uniquePlan.Add(drug);
                    }
                }
            }
            return uniquePlan;
        }

        #region Utilities
        private string GetRxcui(IGrouping<string, RxNormSat> y)
        {
            string rxnorm = y.Where(x => x.SAB == "RXNORM").Select(x => x.RXCUI).FirstOrDefault();
            if (!string.IsNullOrEmpty(rxnorm))
                //return rxnorm;
                return rxnorm + "_RXNORM";
            string mmsl = y.Where(x => x.SAB == "MMSL").Select(x => x.RXCUI).FirstOrDefault();
            if (!string.IsNullOrEmpty(mmsl))
                //return mmsl;
                return mmsl + "_MMSL";
            string vandf = y.Where(x => x.SAB == "VANDF").Select(x => x.RXCUI).FirstOrDefault();
            if (!string.IsNullOrEmpty(vandf))
                return vandf + "_VANDF";
            //return vandf;
            return y.FirstOrDefault().RXCUI;
        }
        private string GetDrugName(string defaultDrugName, List<RxNormConcept> drugs)
        {
            try
            {
                if (drugs.FirstOrDefault() == null)
                    return defaultDrugName;
                string drug = drugs.FirstOrDefault().STR;
                if (string.IsNullOrEmpty(drug))
                    drug = defaultDrugName;
                return drug;
            }
            catch (Exception)
            {
                throw;
            }

        }
        private JArray ParseResult(Plan item, string newPlan)
        {
            JArray newPlanData = null;
            if (item.IsSpecial)
            {
                if (newPlan.Contains("Drug_Information"))
                {
                    var drugInfo = JObject.Parse(newPlan)["Drug_Information"].ToString();
                    newPlanData = JArray.Parse(drugInfo);
                }
                else
                {
                    newPlanData = JArray.Parse(newPlan);
                }
            }
            else
            {
                newPlan = newPlan.Replace("\r\n\t", "");
                newPlanData = JArray.Parse(newPlan);
            }

            return newPlanData;
        }
        private JToken GetPlanBlock(Plan item, JToken token)
        {
            JToken plan;
            if (item.IsSpecial)
            {
                plan = token;
            }
            else
                plan = token["plans"][0];

            return plan;
        }
        private StandardizePlan PopulateStandardProperties(JToken plan)
        {
            string drugName = null != plan["drug_name"] ? plan["drug_name"].ToString() : string.Empty;
            string rxcui = (null != plan["rxnorm_id"] && plan["rxnorm_id"].ToString() != "NaN") ? plan["rxnorm_id"].ToString() : string.Empty;
            string ndc = null != plan["ndc"] ? plan["ndc"].ToString() : null != plan["NDC_FORMAT_CODE"] ? plan["NDC_FORMAT_CODE"].ToString() : String.Empty;

            // Case 1: Drug Name available but rxcui is missing
            if (!string.IsNullOrEmpty(drugName) && string.IsNullOrEmpty(rxcui))
            {
                var newDrugName = drugName.Replace("[", "").Replace("]", "");
                rxcui = this._service.GetRxcuiByDrugName(newDrugName);
            }
            // Case 2: Rxcui is available but Drug Name is missing
            else if (!string.IsNullOrEmpty(rxcui) && string.IsNullOrEmpty(drugName))
            {
                drugName = this._service.GetDrugNameByRxcui(rxcui);
            }
            // Case 3: Ndc is avaible but Drug Name is missing
            if (!string.IsNullOrEmpty(ndc) && string.IsNullOrEmpty(drugName))
            {
                (string drugName, string rxcui) result = this._service.GetDrugNameByNdc(ndc);
                drugName = result.drugName;
                if (string.IsNullOrEmpty(rxcui))
                    rxcui = result.rxcui;
            }
            // Case 3.1: Ndc is avaible but rxcui is missing
            if (!string.IsNullOrEmpty(ndc) && string.IsNullOrEmpty(rxcui))
            {
                rxcui = this._service.GetRxcuiByNdc(ndc);
            }


            if (string.IsNullOrEmpty(drugName) && string.IsNullOrEmpty(rxcui))
                return null;
            if (string.IsNullOrEmpty(rxcui))
                return null;

            return new StandardizePlan { DrugName = drugName, Rxcui = rxcui, Ndc = ndc };
        }
        #endregion
    }
}
