using eNavvi.MedicareProcessor.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eNavvi.MedicareProcessor.Service
{
    internal class BenefitService
    {
        static readonly object _object = new object();
        private readonly string _exist;
        private readonly string _insert;

        public BenefitService()
        {
            this._exist = @"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TempCoPayAmount')
                                    BEGIN                                        
										CREATE TABLE [dbo].[TempCoPayAmount]
										(
											[Inw] [varchar](50) NOT NULL,
											[Onw] [varchar](50) NOT NULL,
											[Descriptions] [varchar](max) NOT NULL,
											[PrefPhases] [varchar](max) NOT NULL,
											[TierName] [varchar](max) NOT NULL,
											[PlanUrlFolder] [varchar](max) NOT NULL,
											[TierID] [int] NULL,
											[PlanID] [int] NULL,
											[PublishDate] [datetime] NOT NULL
										)
                                    END
                                GO

                                TRUNCATE TABLE [TempCoPayAmount]

                                GO";
            this._insert = @"IF NOT EXISTS (SELECT 1 FROM [TempCoPayAmount] WHERE [Inw] = '{0}' AND [PrefPhases] = '{1}' AND [TierName] = '{2}' AND [PlanUrlFolder] = '{3}')
                                INSERT INTO [dbo].[TempCoPayAmount] ([Inw], [Onw] ,[Descriptions] ,[PrefPhases] ,[TierName] ,[PlanUrlFolder] ,[PublishDate]) 
								VALUES ('{4}', '{5}' ,'{6}' ,'{7}' ,'{8}' ,'{9}' ,'{10}')";
        }
        public void Execute(List<PlanDTO> plans)
        {
            var copays = BenefitParser.Parse();
            List<string> sql = new List<string>();
            sql.Add(this._exist);

            foreach (var item in copays)
            {
                string name = plans.Where(x => x.FullId == item.Description).Select(x => x.PlanName).FirstOrDefault();

                name = string.IsNullOrEmpty(name) ? "" : name.Replace("'", "''");
                string prefPhases = JsonConvert.SerializeObject(item.Prefphases);
                string query = string.Format(this._insert, item.Inw, prefPhases, item.Tier, name, item.Inw, "Not Covered", "", prefPhases, item.Tier, name, Program.PublishDate);
                sql.Add(query);
            }
            if (!Directory.Exists("SQL"))
                Directory.CreateDirectory("SQL");
            File.WriteAllLines("SQL/TempCoPayAmount.SQL", sql);
        }
    }
}
