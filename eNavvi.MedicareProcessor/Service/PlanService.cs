using eNavvi.MedicareProcessor.Parser;

namespace eNavvi.MedicareProcessor.Service
{
    internal class PlanService
    {
        private readonly string _planExist;
        private readonly string _planInsert;

        public PlanService()
        {
            this._planExist = @"IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'MedicareNewPlans')
                                    BEGIN
                                        CREATE TABLE [dbo].[MedicareNewPlans](
                                            [Name] [varchar](max) NOT NULL,
                                            [State] [varchar](10) NOT NULL,
                                            [County] [varchar](max) NULL,
                                            [Type] [int] NOT NULL,
                                            [BlobName] [varchar](max) NOT NULL,
                                            [PublishDate] [datetime] NOT NULL,
                                            [PlanGuid] [char](36) NOT NULL,
                                            [StateId] INT NULL,
                                            [Processed] INT NULL)
                                    END
                                GO

                                TRUNCATE TABLE [MedicareNewPlans]

                                GO";
            this._planInsert = @"IF NOT EXISTS (SELECT 1 FROM [MedicareNewPlans] WHERE [Name] = '{0}' AND [State] = '{1}')
                                    INSERT INTO [dbo].[MedicareNewPlans] ([Name] , [State], [County] , [Type] ,[BlobName] ,[PlanGuid] , [PublishDate], [Processed] ) VALUES ('{2}','{3}','{4}','{5}','{6}','{7}','{8}',{9});";
        }
        public List<PlanDTO> Execute(List<PlanDTO> plans)
        {

            List<string> sql = new List<string>();
            sql.Add(this._planExist);
            foreach (PlanDTO plan in plans)
            {
                string name = plan.PlanName.Replace("'", "''");
                string county = plan.CountyCode.Replace("'", "''");
                string query = string.Format(_planInsert, name, plan.State, name, plan.State, county, 3, plan.FullId, Guid.NewGuid(), Program.config.PublishDate, plan.Processed);
                sql.Add(query);
            }
            Directory.CreateDirectory("SQL");
            File.WriteAllLines("SQL/Medicare.SQL", sql);
            return plans;
        }
    }
}
