﻿using eNavvi.FormularyProcessor.Entities;
using eNavvi.FormularyProcessor.Models;

using Newtonsoft.Json.Linq;

using RestSharp;
using Serilog;
using System.Net;

namespace eNavvi.FormularyProcessor.Utilities
{
    public class ExternalService
    {
        const string RXNAV_BASE_REST_URL = "https://rxnav.nlm.nih.gov/REST/";
        private readonly Configurations _config;
        public ExternalService(Configurations config)
        {
            this._config = config;
        }

        public RelatedInfo GetRelatedInfo(string rxcui)
        {
            RelatedInfo data = this.RxNavGetRelatedConcept(rxcui);
            var classes = this.RxATCClassFromRxcuiLookupBestEffort(rxcui);
            if (null != classes)
                data.DrugClasses = string.Join(";", classes.OrderBy(x => x));
            return data;
        }

        private List<string> RxATCClassFromRxcuiLookupBestEffort(string rxcui)
        {
            string url = $"rxclass/class/byRxcui.json?rxcui={rxcui}&relaSource=ATC";
            string content = string.Empty;
            try
            {
                content = MakeHttpRequest(url);
                JObject result = JObject.Parse(content);
                var prop1 = (JObject)result["rxclassDrugInfoList"];
                if (prop1 == null)
                    return null;

                var prop2 = prop1["rxclassDrugInfo"];
                if (prop2 != null)
                {
                    return prop2.Select(x => x["rxclassMinConceptItem"]["className"].ToString()).Distinct().ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
            return null;
        }

        private RelatedInfo RxNavGetRelatedConcept(string rxcui)
        {
            string content = string.Empty;
            RelatedInfo info = new RelatedInfo { Rxcui = int.Parse(rxcui) };
            try
            {
                string url = $"{RXNAV_BASE_REST_URL}rxcui/{rxcui}/related.json?tty=BN+IN+DFG";
                content = MakeHttpRequest(url);
                JObject result = JObject.Parse(content);

                if ((string)result["relatedGroup"]["conceptGroup"][0]["tty"] == "BN")
                {
                    JArray props = (JArray)result["relatedGroup"]["conceptGroup"][0]["conceptProperties"];
                    if (props != null)
                        info.BrandName = string.Join(";", props.Select(x => x["name"].ToString()).Distinct().ToList().OrderBy(x => x));
                }

                if ((string)result["relatedGroup"]["conceptGroup"][1]["tty"] == "IN")
                {
                    JArray props = (JArray)result["relatedGroup"]["conceptGroup"][1]["conceptProperties"];
                    if (props != null)
                        info.Ingredients = string.Join(";", props.Select(x => x["name"].ToString()).Distinct().ToList().OrderBy(x => x));
                }

                if ((string)result["relatedGroup"]["conceptGroup"][2]["tty"] == "DFG")
                {
                    JArray props = (JArray)result["relatedGroup"]["conceptGroup"][2]["conceptProperties"];
                    if (props != null)
                        info.DosageFromGroup = string.Join(";", props.Select(x => x["name"].ToString()).Distinct().ToList().OrderBy(x => x));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
            return info;
        }

        public string MakeHttpRequest(string url)
        {
            int retry = 0;
            LogInformation(url);
            do
            {
                var client = new RestClient(RXNAV_BASE_REST_URL);
                var request = new RestRequest(url, Method.Get);
                request.Timeout = -1;
                RestResponse response = client.ExecuteAsync(request).GetAwaiter().GetResult();
                Console.WriteLine($"{DateTime.Now}: Status : {response.StatusCode}");
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                    return response.Content;
                else if (response.StatusCode == 0)
                {
                    LogInformation("Target machine refused connection will retry after 10 seconds");
                    retry++;
                    Thread.Sleep(5000);
                }
                else
                    throw new Exception(response.Content);
            } while (retry > 0 & retry < 3);
            return string.Empty;
        }


        /* 
         * get the drug name from an rxcui
         * https://rxnav.nlm.nih.gov/REST/rxcui/1000001/property.json?propName=RxNorm%20Name
         */
        public string GetDrugNameByRxcui(string rxcui)
        {
            string drugName = null;
            string url = $"rxcui/{rxcui}/property.json?propName=RxNorm Name";

            string content = this.MakeHttpRequest(url);
            if (string.IsNullOrEmpty(content) || content == "null")
                return drugName;
            JObject result = JObject.Parse(content);

            if (null != result["propConceptGroup"] && !string.IsNullOrEmpty(result["propConceptGroup"].ToString()) && null != result["propConceptGroup"]["propConcept"])
                drugName = result["propConceptGroup"]["propConcept"][0]["propValue"].ToString();

            LogInformation($"GetDrugNameByRxcui: Rxcui: {rxcui}, DrugName: {drugName}");
            return drugName;
        }

        /* 
         * request to get drug name from ndc number
         * https://rxnav.nlm.nih.gov/REST/ndcstatus.json?ndc=00071015723
         */
        public (string, string) GetDrugNameByNdc(string ndc)
        {
            string drugName = string.Empty;
            string rxcui = string.Empty;
            string url = $"ndcstatus.json?ndc={ndc}";

            string content = this.MakeHttpRequest(url);
            if (string.IsNullOrEmpty(content) || content == "null")
                return (drugName, rxcui);

            JObject result = JObject.Parse(content);

            if (null != result["ndcStatus"])
            {
                if (null != result["ndcStatus"]["conceptName"])
                    drugName = result["ndcStatus"]["conceptName"].ToString();

                if (null != result["ndcStatus"]["rxcui"])
                    rxcui = result["ndcStatus"]["rxcui"].ToString();
            }
            LogInformation($"GetDrugNameByNdc: Ndc: {ndc}, DrugName: {drugName}, Rxcui: {rxcui}");
            return (drugName, rxcui);
        }

        /* 
         * attempts to look up the rxcui number from a drug name using the
         * "approximate" search function RxNorm provides
         * https://rxnav.nlm.nih.gov/REST/approximateTerm.json?term=amlodipine%205%20MG%20/%20hydrochlorothiazide%2025%20MG%20/%20olmesartan%20medoxomil%2040%20MG%20Oral%20Table&maxEntries=1&option=1
         */
        public string GetRxcuiByDrugName(string drugName)
        {
            string rxcui = string.Empty;
            string url = $"approximateTerm.json?term={drugName}&maxEntries=1&option=1";

            string content = this.MakeHttpRequest(url);
            if (string.IsNullOrEmpty(content) || content == "null")
                return rxcui;
            JObject result = JObject.Parse(content);
            if (null != result["approximateGroup"] && null != result["approximateGroup"]["candidate"] && null != result["approximateGroup"]["candidate"][0])
                rxcui = result["approximateGroup"]["candidate"][0]["rxcui"].ToString();

            LogInformation($"GetRxcuiByDrugName: DrugName: {drugName}, Rxcui: {rxcui}");
            return rxcui;
        }

        /* 
         * get the rxcui number from an ndc number
         * https://rxnav.nlm.nih.gov/REST/ndcproperties.json?id=1000001
         */
        public string GetRxcuiByNdc(string ndc)
        {
            string rxcui = string.Empty;
            string url = $"ndcproperties.json?id={ndc}";

            string content = this.MakeHttpRequest(url);
            if (string.IsNullOrEmpty(content) || content == "null")
                return rxcui;

            JObject result = JObject.Parse(content);

            if (null != result["ndcPropertyList"] && null != result["ndcPropertyList"]["ndcProperty"] && null != result["ndcPropertyList"]["ndcProperty"][0])
                rxcui = result["ndcPropertyList"]["ndcProperty"][0]["rxcui"].ToString();

            LogInformation($"GetRxcuiByNdc: Ndc: {ndc}, Rxcui: {rxcui}");
            return rxcui;
        }

        private void LogInformation(string message)
        {
            if (this._config.TraceEnabled)
                Log.Information(message);
        }
    }
}