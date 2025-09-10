using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using M36Backend.Models;

namespace M36Backend.Services
{
    public class ZebraPrinterService
    {
        private readonly string _printerIP;
        private readonly int _printerPort;

        public ZebraPrinterService()
        {
            // Nastavte IP adresu a port vaší Zebra tiskárny
            _printerIP = "192.168.1.100"; // Změňte na IP vaší tiskárny
            _printerPort = 9100; // Standardní port pro Zebra tiskárny
        }

        public async Task<bool> PrintAllLabels(OrderDetails order)
        {
            try
            {
                foreach (var product in order.Products)
                {
                    await PrintSingleLabel(product.PartNumber, order);
                    await Task.Delay(500); // Krátká pauza mezi tisky
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při tisku všech etiket: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PrintSingleLabel(string partNumber, OrderDetails order)
        {
            try
            {
                var zplCommand = GenerateZPLLabel(partNumber, order);
                await SendToPrinter(zplCommand);
                Console.WriteLine($"Etiketa pro díl {partNumber} byla odeslána na tiskárnu");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při tisku etikety pro díl {partNumber}: {ex.Message}");
                return false;
            }
        }

        private string GenerateZPLLabel(string partNumber, OrderDetails order)
        {
            // ZPL příkaz pro tisk etikety
            var zpl = new StringBuilder();
            
            zpl.AppendLine("^XA"); // Začátek formátu
            zpl.AppendLine("^CF0,30"); // Nastavení fontu
            
            // Hlavička s číslem zakázky
            zpl.AppendLine($"^FO50,50^FD Zakazka: {order.Number}^FS");
            
            // Číslo dílu (větší font)
            zpl.AppendLine("^CF0,40");
            zpl.AppendLine($"^FO50,100^FD Dil: {partNumber}^FS");
            
            // PCN
            zpl.AppendLine("^CF0,25");
            zpl.AppendLine($"^FO50,150^FD PCN: {order.PCN}^FS");
            
            // Typ zakázky
            zpl.AppendLine($"^FO50,180^FD Typ: {order.Type}^FS");
            
            // Čárový kód
            zpl.AppendLine($"^FO50,220^BC^FD{partNumber}^FS");
            
            // Datum a čas
            var now = DateTime.Now;
            zpl.AppendLine("^CF0,20");
            zpl.AppendLine($"^FO50,300^FD {now:dd.MM.yyyy HH:mm}^FS");
            
            zpl.AppendLine("^XZ"); // Konec formátu
            
            return zpl.ToString();
        }

        private async Task SendToPrinter(string zplCommand)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(_printerIP, _printerPort);
                
                using var stream = client.GetStream();
                var data = Encoding.UTF8.GetBytes(zplCommand);
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při komunikaci s tiskárnou: {ex.Message}");
                
                // Pro testování - uložení ZPL příkazu do souboru
                var fileName = $"label_{DateTime.Now:yyyyMMdd_HHmmss}.zpl";
                await File.WriteAllTextAsync(fileName, zplCommand);
                Console.WriteLine($"ZPL příkaz uložen do souboru: {fileName}");
                
                throw;
            }
        }

        public async Task<bool> TestPrinterConnection()
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(_printerIP, _printerPort);
                
                // Odeslání testovacího příkazu
                var testCommand = "^XA^FO50,50^ADN,36,20^FDTEST PRINT^FS^XZ";
                using var stream = client.GetStream();
                var data = Encoding.UTF8.GetBytes(testCommand);
                await stream.WriteAsync(data, 0, data.Length);
                
                Console.WriteLine("Testovací tisk byl odeslán na tiskárnu");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při testování připojení k tiskárně: {ex.Message}");
                return false;
            }
        }
    }
}
