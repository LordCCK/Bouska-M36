using Microsoft.AspNetCore.Mvc;
using M36Backend.Services;
using M36Backend.Models;
using System.Threading.Tasks;

namespace M36Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrintController : ControllerBase
    {
        private readonly ZebraPrinterService _printerService;

        public PrintController(ZebraPrinterService printerService)
        {
            _printerService = printerService;
        }

        [HttpPost("labels")]
        public async Task<ActionResult> PrintLabels([FromBody] PrintRequest request)
        {
            try
            {
                if (request.Type == "all")
                {
                    await _printerService.PrintAllLabels(request.Order);
                    return Ok(new { success = true, message = "Všechny etikety byly vytištěny" });
                }
                else if (request.Type == "single")
                {
                    await _printerService.PrintSingleLabel(request.PartNumber, request.Order);
                    return Ok(new { success = true, message = $"Etiketa pro díl {request.PartNumber} byla vytištěna" });
                }
                else
                {
                    return BadRequest("Neplatný typ tisku");
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Chyba při tisku: {ex.Message}");
            }
        }
    }
}
