using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        var client = new HttpClient(handler);
        
        // Let's create a bogus request to trigger a 400 Bad Request
        var json = @"{ ""slotName"": ""Test"", ""capacity"": 50 }";
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        try 
        {
            var response = await client.PutAsync("https://localhost:7179/api/manager/slots/1", content);
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Body: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
