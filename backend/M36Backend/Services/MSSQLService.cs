using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace M36Backend.Services
{
    public class MSSQLService
    {
        private readonly string _connectionString;

        public MSSQLService()
        {
            // Nastavte připojovací řetězec podle vaší MS SQL databáze
            _connectionString = "Server=localhost;Database=M36Database;Integrated Security=true;TrustServerCertificate=true;";
        }

        public async Task<bool> InsertRow(string tableName, string columns, Dictionary<string, object> data)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var columnList = columns.Replace("[", "").Replace("]", "");
                var columnNames = columnList.Split(',');
                
                var values = new StringBuilder();
                var parameters = new List<SqlParameter>();

                for (int i = 0; i < columnNames.Length; i++)
                {
                    var columnName = columnNames[i].Trim();
                    var paramName = $"@param{i}";
                    
                    if (i > 0) values.Append(", ");
                    values.Append(paramName);

                    var value = data.ContainsKey(columnName) ? data[columnName] : DBNull.Value;
                    parameters.Add(new SqlParameter(paramName, value));
                }

                var query = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddRange(parameters.ToArray());

                await command.ExecuteNonQueryAsync();
                Console.WriteLine($"Úspěšně vložen záznam do tabulky {tableName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při vkládání do MS SQL: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateTableIfNotExists()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var createTableQuery = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='M36' AND xtype='U')
                    CREATE TABLE M36 (
                        ID int IDENTITY(1,1) PRIMARY KEY,
                        [ORDER] nvarchar(50),
                        BARCODE nvarchar(100),
                        DATE nvarchar(20),
                        TIME nvarchar(20),
                        OPERATOR nvarchar(50),
                        SETPOINT float,
                        LEAK float,
                        RESULT nvarchar(10),
                        PCN nvarchar(50),
                        TYPE nvarchar(50),
                        CREATED_AT datetime DEFAULT GETDATE()
                    )";

                using var command = new SqlCommand(createTableQuery, connection);
                await command.ExecuteNonQueryAsync();
                
                Console.WriteLine("Tabulka M36 je připravena");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při vytváření tabulky: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Dictionary<string, object>>> GetTestResults(string orderNumber)
        {
            var results = new List<Dictionary<string, object>>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM M36 WHERE [ORDER] = @OrderNumber ORDER BY CREATED_AT DESC";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderNumber", orderNumber);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }
                    results.Add(row);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při načítání výsledků testů: {ex.Message}");
            }

            return results;
        }
    }
}
