using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using M36Backend.Models;

namespace M36Backend.Services
{
    public class IBMSQLService
    {
        private readonly string _connectionString;

        public IBMSQLService()
        {
            // Pro demonstraci používáme SQL Server místo IBM DB2
            // V produkci nahraďte skutečným IBM DB2 connection stringem
            _connectionString = "Server=localhost;Database=M36Orders;Integrated Security=true;TrustServerCertificate=true;";
        }

        public async Task<List<Order>> GetAvailableOrders()
        {
            var orders = new List<Order>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        ORDER_NUMBER,
                        ORDER_DESCRIPTION,
                        STATUS
                    FROM ORDERS 
                    WHERE STATUS = 'ACTIVE'
                    ORDER BY ORDER_NUMBER";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    orders.Add(new Order
                    {
                        Number = reader.GetString("ORDER_NUMBER"),
                        Description = reader.GetString("ORDER_DESCRIPTION"),
                        Status = reader.GetString("STATUS")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při načítání zakázek z IBM SQL: {ex.Message}");
                // Pro demonstraci vrátíme testovací data
                orders.AddRange(GetTestOrders());
            }

            return orders;
        }

        public async Task<OrderDetails?> GetOrderDetails(string orderNumber)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Načtení základních informací o zakázce
                var orderQuery = @"
                    SELECT 
                        ORDER_NUMBER,
                        ORDER_TYPE,
                        PCN,
                        DESCRIPTION
                    FROM ORDERS 
                    WHERE ORDER_NUMBER = @OrderNumber";

                OrderDetails? orderDetails = null;

                using (var command = new SqlCommand(orderQuery, connection))
                {
                    command.Parameters.AddWithValue("@OrderNumber", orderNumber);
                    using var reader = await command.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        orderDetails = new OrderDetails
                        {
                            Number = reader.GetString("ORDER_NUMBER"),
                            Type = reader.GetString("ORDER_TYPE"),
                            PCN = reader.GetString("PCN"),
                            Description = reader.GetString("DESCRIPTION")
                        };
                    }
                }

                if (orderDetails != null)
                {
                    // Načtení produktů pro zakázku
                    var productsQuery = @"
                        SELECT 
                            PART_NUMBER,
                            PART_DESCRIPTION,
                            QUANTITY
                        FROM ORDER_PARTS 
                        WHERE ORDER_NUMBER = @OrderNumber";

                    using var productCommand = new SqlCommand(productsQuery, connection);
                    productCommand.Parameters.AddWithValue("@OrderNumber", orderNumber);
                    using var productReader = await productCommand.ExecuteReaderAsync();

                    while (await productReader.ReadAsync())
                    {
                        orderDetails.Products.Add(new Product
                        {
                            PartNumber = productReader.GetString("PART_NUMBER"),
                            Description = productReader.GetString("PART_DESCRIPTION"),
                            Quantity = productReader.GetInt32("QUANTITY")
                        });
                    }
                }

                return orderDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při načítání detailů zakázky z IBM SQL: {ex.Message}");
                // Pro demonstraci vrátíme testovací data
                return GetTestOrderDetails(orderNumber);
            }
        }

        private List<Order> GetTestOrders()
        {
            return new List<Order>
            {
                new Order { Number = "ORD001", Description = "Testovací zakázka 1", Status = "ACTIVE" },
                new Order { Number = "ORD002", Description = "Testovací zakázka 2", Status = "ACTIVE" },
                new Order { Number = "ORD003", Description = "Testovací zakázka 3", Status = "ACTIVE" }
            };
        }

        private OrderDetails GetTestOrderDetails(string orderNumber)
        {
            return new OrderDetails
            {
                Number = orderNumber,
                Type = "PRODUCTION",
                PCN = "PCN12345",
                Description = $"Testovací zakázka {orderNumber}",
                Products = new List<Product>
                {
                    new Product { PartNumber = "PART001", Description = "Díl 1", Quantity = 10 },
                    new Product { PartNumber = "PART002", Description = "Díl 2", Quantity = 5 },
                    new Product { PartNumber = "PART003", Description = "Díl 3", Quantity = 8 }
                }
            };
        }
    }
}
