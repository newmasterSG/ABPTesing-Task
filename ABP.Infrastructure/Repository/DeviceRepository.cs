using ABP.Domain.Entities;
using ABP.Domain.Repository;
using System.Data.SqlClient;

namespace ABP.Infrastructure.Repository
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly string connectionString;

        public DeviceRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task AddDeviceAsync(string deviceToken, string experimentName, string receivedValue)
        {
            DateTime firstRequest = DateTime.Now;

            //Searching for experiment Id
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlGetExperimentId = "SELECT Id FROM Experiments WHERE Name = @Name";
                int experimentId = -1;
                using (SqlCommand commandGetId = new SqlCommand(sqlGetExperimentId, connection))
                {
                    commandGetId.Parameters.AddWithValue("@Name", experimentName);

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await commandGetId.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            experimentId = reader.GetInt32(reader.GetOrdinal("Id"));
                        }
                    }
                }

                //Adding new experiment
                if (experimentId == -1)
                {
                    string sqlInsertExperiment = "INSERT INTO Experiments (Name) VALUES (@Name)";

                    using (SqlCommand commandInsert = new SqlCommand(sqlInsertExperiment, connection))
                    {
                        commandInsert.Parameters.AddWithValue("@Name", experimentName);
                        await commandInsert.ExecuteNonQueryAsync();
                    }

                    // Execute the command to get the newly inserted experiment ID
                    using (SqlCommand commandGetId = new SqlCommand(sqlGetExperimentId, connection))
                    {
                        commandGetId.Parameters.AddWithValue("@Name", experimentName);
                        experimentId = (int)await commandGetId.ExecuteScalarAsync();
                    }
                }

                //Creating new Device
                string sqlInsertDevice = "INSERT INTO Devices (DeviceToken, ExperimentId, ReceivedValue, FirstRequest) VALUES (@DeviceToken, @ExperimentId, @ReceivedValue, @FirstRequest)";
                using (SqlCommand commandInsert = new SqlCommand(sqlInsertDevice, connection))
                {
                    commandInsert.Parameters.AddWithValue("@DeviceToken", deviceToken);
                    commandInsert.Parameters.AddWithValue("@ExperimentId", experimentId);
                    commandInsert.Parameters.AddWithValue("@ReceivedValue", receivedValue);
                    commandInsert.Parameters.AddWithValue("@FirstRequest", firstRequest);
                    await commandInsert.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task<Device> GetDeviceAsync(string deviceToken)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                //Replacing ExperimentId by ExperimentName
                string sql = @"SELECT d.*, e.Name
                                FROM Devices d
                                JOIN Experiments e ON d.ExperimentId = e.Id
                                WHERE d.DeviceToken = @DeviceToken";

                //Creating Device instance
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@DeviceToken", deviceToken);
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Device device = new Device
                            {
                                DeviceToken = reader.GetString(reader.GetOrdinal("DeviceToken")),
                                Experiment = reader.GetString(reader.GetOrdinal("Name")),
                                ReceivedValue = reader.IsDBNull(reader.GetOrdinal("ReceivedValue")) ? null : reader.GetString(reader.GetOrdinal("ReceivedValue")),
                                FirstRequest = reader.GetDateTime(reader.GetOrdinal("FirstRequest"))
                            };
                            return device;
                        }
                    }
                }
            }

            //If there is no device which has this device token, return null
            return null;
        }

        public async Task<List<Device>> GetAllDevicesAsync()
        {
            List<Device> devices = new List<Device>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                //Replacing ExperimentId by ExperimentName
                string sql = @"SELECT d.*, e.Name
                                FROM Devices d
                                JOIN Experiments e ON d.ExperimentId = e.Id";

                //Filling the list with device instances
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Device device = new Device
                            {
                                DeviceToken = reader.GetString(reader.GetOrdinal("DeviceToken")),
                                Experiment = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name")),
                                ReceivedValue = reader.IsDBNull(reader.GetOrdinal("ReceivedValue")) ? null : reader.GetString(reader.GetOrdinal("ReceivedValue")),
                                FirstRequest = reader.GetDateTime(reader.GetOrdinal("FirstRequest"))
                            };
                            devices.Add(device);
                        }
                    }
                }
            }
            return devices;
        }
    }
}
