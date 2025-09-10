using Microsoft.AspNetCore.Mvc;
using M36Backend.Services;
using M36Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace M36Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IBMSQLService _ibmSqlService;
        private readonly MSSQLService _msSqlService;

        public OrdersController(IBMSQLService ibmSqlService, MSSQLService msSqlService)
        {
            _ibmSqlService = ibmSqlService;
            _msSqlService = msSqlService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Order>>> GetOrders()
        {
            try
            {
                var orders = await _ibmSqlService.GetAvailableOrders();
                return Ok(orders);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Chyba při načítání zakázek: {ex.Message}");
            }
        }

        [HttpGet("{orderNumber}")]
        public async Task<ActionResult<OrderDetails>> GetOrderDetails(string orderNumber)
        {
            try
            {
                var orderDetails = await _ibmSqlService.GetOrderDetails(orderNumber);
                if (orderDetails == null)
                {
                    return NotFound($"Zakázka {orderNumber} nebyla nalezena");
                }
                return Ok(orderDetails);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Chyba při načítání detailů zakázky: {ex.Message}");
            }
        }

        [HttpPost("test-result")]
        public async Task<ActionResult> SaveTestResult([FromBody] TestResult testResult)
        {
            try
            {
                var data = new Dictionary<string, object>
                {
                    ["ORDER"] = testResult.Order,
                    ["BARCODE"] = testResult.Barcode,
                    ["DATE"] = testResult.Date,
                    ["TIME"] = testResult.Time,
                    ["OPERATOR"] = testResult.Operator,
                    ["SETPOINT"] = testResult.Setpoint,
                    ["LEAK"] = testResult.Leak,
                    ["RESULT"] = testResult.Result,
                    ["PCN"] = testResult.PCN,
                    ["TYPE"] = testResult.Type
                };

                await _msSqlService.InsertRow("M36", "[ORDER],BARCODE,DATE,TIME,OPERATOR,SETPOINT,LEAK,RESULT,PCN,TYPE", data);
                return Ok(new { success = true, message = "Výsledek testu byl úspěšně uložen" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Chyba při ukládání výsledku testu: {ex.Message}");
            }
        }
    }
}
