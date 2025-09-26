namespace KYH_IoT
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var thingspeak = new ThingSpeak();
                int temperature = new Random().Next(0, 40);
                int humidity = new Random().Next(0, 100);
                int rpm = new Random().Next(1800, 7000);
                int speedKmH = new Random().Next(0, 110);
                int fuelLevel = new Random().Next(0, 100);
                Console.WriteLine($"Temperature: {temperature} °C, Humidity: {humidity} %, RPM: {rpm}, Speed: {speedKmH} km/h, Fuel Level: {fuelLevel} %");

                thingspeak.SendData(temperature, humidity, rpm, speedKmH, fuelLevel);
                System.Threading.Thread.Sleep(15000);

            }

        }
    }
}
