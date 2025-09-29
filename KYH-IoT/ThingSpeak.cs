using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KYH_IoT
{
    internal class ThingSpeak
    {
        internal void SendData(int temperature, int humidity, int fuelLevel, int speedKmH, int rpm )
        {
            // GET https://api.thingspeak.com/update?api_key=D7HQNNWLRGS38LCN&field1=0
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://api.thingspeak.com");
            var response = httpClient.GetAsync($"/update?api_key=D7HQNNWLRGS38LCN&field1={temperature}&field2={humidity}&field3={fuelLevel}%&field4={speedKmH}&field5={rpm}").Result;
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Data sent to ThingSpeak successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to send data to ThingSpeak. Status code: {response.StatusCode}");
            }

           // throw new NotImplementedException();
        }
    }
}
