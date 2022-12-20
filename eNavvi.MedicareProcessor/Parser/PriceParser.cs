namespace eNavvi.MedicareProcessor.Parser
{
    internal class PriceParser
    {
        static readonly string Path;
        static PriceParser()
        {
#if DEBUG
            Path = $"D:/enavvi/Medicare/DataProcessing/{Program.config.Sourcess.Price}";
#else
            Path = $"Sources/{Program.config.Sourcess.Price}";
#endif
        }
        public static Dictionary<string, string> Parse()
        {
            Dictionary<string, string> prices = new Dictionary<string, string>();

            var lines = File.ReadAllLines(Path);
            foreach (var line in lines)
            {
                var data = line.Split("|");
                if (data.Count() > 1)
                {
                    if (!prices.ContainsKey(data[0]))
                    {
                        prices.Add(data[0], data[1]);
                    }
                }
            }

            return prices;
        }
    }
}
