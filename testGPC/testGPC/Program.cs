using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GridPointCode;

namespace testGPC
{
    class Program
    {
        static void Main(string[] args)
        {
            Random Rnd = new Random();
            double Latitude = Rnd.Next(-90, 90) + Rnd.NextDouble();
            double Longitude = Rnd.Next(-180, 180) + Rnd.NextDouble();
            Console.WriteLine("Latitude: " + Latitude + "\tLongitude: " + Longitude);

            string GridPointCode = GPC.GetGridPointCode(Latitude, Longitude);
            double DecodedLatitude;
            double DecodedLongitude;
            GPC.GetCoordinates(GridPointCode, out DecodedLatitude, out DecodedLongitude);

            Console.WriteLine("GPC: " + GridPointCode);
            Console.WriteLine("Latitude: " + DecodedLatitude + "\tLongitude: " + DecodedLongitude);
            Console.ReadLine();

        }
    }
}
