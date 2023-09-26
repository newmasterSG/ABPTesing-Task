using System.Data.SqlClient;
using ABP.Domain.Repository;

namespace ABP.Infrastructure.Repository
{
    public class ExperimentRepository : IExperimentRepository
    {
        private readonly string connectionString;

        public ExperimentRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<List<string>> GetAllExperimentsAsync()
        {
            List<string> experiments = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"SELECT * FROM Experiments";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            experiments.Add(reader.GetString(reader.GetOrdinal("Name")));
                        }
                    }
                }
            }
            return experiments;
        }
    }
}
