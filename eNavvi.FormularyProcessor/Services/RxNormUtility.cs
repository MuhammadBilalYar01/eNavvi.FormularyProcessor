using eNavvi.FormularyProcessor.Interfaces;
using eNavvi.FormularyProcessor.Models.RxNorm;
using Serilog;

namespace eNavvi.FormularyProcessor.Services
{
    public class RxNormUtility : IRxNormUtility
    {
        private readonly IBlobStorage _blobStorage;
        private const string _ContainerID = "rxnorm-raw-data";
        private static List<RxNormSat> s_RxNormSat = null;
        private static List<RxNormConcept> s_rxNormConcepts = null;
        public RxNormUtility(IBlobStorage blobStorage)
        {
            this._blobStorage = blobStorage;
        }
        public async Task<List<T>> GetData<T>(string blobId) where T : class
        {
            if (blobId.ToLower() == "RXNSAT.RRF".ToLower())
            {
                if (null == s_RxNormSat)
                {
                    string txtData = string.Empty;
#if DEBUG
                    txtData = await File.ReadAllTextAsync(@"D:\eNavvi\rxnorm-raw-data\RXNSAT.RRF");
#else
                    txtData = await this._blobStorage.DownloadBlob(_ContainerID, blobId);
#endif

                    var data = Parser.ParseRxNormSat(txtData.Split("\n"));
                    s_RxNormSat = data;
                }
                return s_RxNormSat.Cast<T>().ToList();
            }
            else if (blobId.ToLower() == "RXNCONSO.RRF".ToLower())
            {
                if (null == s_rxNormConcepts)
                {
                    string txtData = string.Empty;
#if DEBUG
                    txtData = await File.ReadAllTextAsync(@"D:\eNavvi\rxnorm-raw-data\RXNCONSO.RRF");
#else
                    txtData = await this._blobStorage.DownloadBlob(_ContainerID, blobId);
#endif

                    s_rxNormConcepts = Parser.ParseRxNormConcept(txtData.Split("\n"));
                }
                return
                    s_rxNormConcepts.Cast<T>().ToList();
            }
            return default(List<T>);
        }
    }

    class Parser
    {
        public static List<RxNormSat> ParseRxNormSat(string[] data)
        {
            List<RxNormSat> rxNormSat = new List<RxNormSat>();
            Parallel.ForEach(data, (item) =>
            {
                if (!String.IsNullOrEmpty(item))
                {
                    var line = item.Split("|");
                    double.TryParse(line[10], out double atv);
                    var sat = new RxNormSat
                    {
                        RXCUI = line[0],
                        //LUI = line[1],
                        //SUI = line[2],
                        //RXAUI = line[3],
                        //STYPE = line[4],
                        //CODE = line[5],
                        //ATUI = line[6],
                        //SATUI = line[7],
                        ATN = line[8],
                        SAB = line[9],
                        ATV = atv.ToString(),
                        //SUPPRESS = line[11],
                        //CVF = line[12]
                    };
                    lock (rxNormSat)
                        rxNormSat.Add(sat);
                }
                else
                    Log.Warning("Skipping: " + item);
            });

            //foreach (var item in data)
            //{
            //    if (String.IsNullOrEmpty(item))
            //        continue;
            //    var line = item.Split("|");
            //    var sat = new RxNormSat
            //    {
            //        RXCUI = line[0],
            //        LUI = line[1],
            //        SUI = line[2],
            //        RXAUI = line[3],
            //        STYPE = line[4],
            //        CODE = line[5],
            //        ATUI = line[6],
            //        SATUI = line[7],
            //        ATN = line[8],
            //        SAB = line[9],
            //        ATV = line[10],
            //        SUPPRESS = line[11],
            //        CVF = line[12]
            //    };
            //    rxNormSat.Add(sat);
            //}
            return rxNormSat;
        }
        public static List<RxNormConcept> ParseRxNormConcept(string[] data)
        {
            List<RxNormConcept> rxNormConcepts = new List<RxNormConcept>();
            Parallel.ForEach(data, (item) =>
            {
                if (!String.IsNullOrEmpty(item))
                {
                    var line = item.Split("|");
                    RxNormConcept concept = new RxNormConcept
                    {
                        RXCUI = line[0],
                        //LAT = line[1],
                        //TS = line[2],
                        //LUI = line[3],
                        //STT = line[4],
                        //SUI = line[5],
                        //ISPREF = line[6],
                        //RXAUI = line[7],
                        //SAUI = line[8],
                        //SCUI = line[9],
                        //SDUI = line[10],
                        SAB = line[11],
                        TTY = line[12],
                        //CODE = line[13],
                        STR = line[14],
                        //SRL = line[15],
                        //SUPPRESS = line[16],
                        //CVF = line[17],
                    };
                    lock (rxNormConcepts)
                        rxNormConcepts.Add(concept);
                }
                else
                    Log.Warning("Skipping: " + item);
            });

            //foreach (var item in data)
            //{
            //    var line = item.Split("|");
            //    RxNormConcept concept = new RxNormConcept
            //    {
            //        RXCUI = line[0],
            //        LAT = line[1],
            //        TS = line[2],
            //        LUI = line[3],
            //        STT = line[4],
            //        SUI = line[5],
            //        ISPREF = line[6],
            //        RXAUI = line[7],
            //        SAUI = line[8],
            //        SCUI = line[9],
            //        SDUI = line[10],
            //        SAB = line[11],
            //        TTY = line[12],
            //        CODE = line[13],
            //        STR = line[14],
            //        SRL = line[15],
            //        SUPPRESS = line[16],
            //        CVF = line[17],
            //    };
            //    rxNormConcepts.Add(concept);
            //}
            return rxNormConcepts;
        }
    }
}
