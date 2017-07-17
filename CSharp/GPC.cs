/*
    Copyright 2017 Pranavkumar Patel
    
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

using System;
using System.Globalization;
using System.Text;

namespace GridPointCode
{
    static class GPC
    {
        const string CHARACTERS = "0123456789CDFGHJKLMNPRTVWXY";   //base27
        const ulong ELEVEN = 205881132094649;   //For Uniformity

        // Get a Grid Point Code
        public static string GetGridPointCode(double latitude, double longitude, bool formatted = true)
        {
            double Latitude = latitude; //Latitude
            double Longitude = longitude;   //Longitude

            /*  Validating Latitude and Longitude values */
            if (Latitude < -90 || Latitude > 90)
            {
                throw new ArgumentOutOfRangeException("Latitude", Latitude, "Latitude value must be between -90 to 90.");
            }
            if (Longitude < -180 || Longitude > 180)
            {
                throw new ArgumentOutOfRangeException("Longitude", Longitude, "Longitude value must be between -180 to 180.");
            }

            // IGNORE ERROR : Do not check floating point equality with exact values, use a range instead.
            // As no mathematical operations are performed before on Latitude and Longitude.
            if (Latitude == -90 || Latitude == 90)
            {
                Longitude = 0.00;
            }

            /*  Getting a Point Number  */
            ulong Point = GetPointNumber(Latitude, Longitude);

            /*  Encode Point    */
            string GridPointCode = EncodePoint(Point + ELEVEN);

            /*  Format GridPointCode   */
            if (formatted)
            {
                GridPointCode = FormatGPC(GridPointCode);
            }

            return GridPointCode;
        }

        //Get Point from Coordinates
        static ulong GetPointNumber(double latitude, double longitude)
        {
            //Splitting Degree and Decimal Parts
            SplitWholeFractional(latitude, out string LatitudeWhole, out string LatitudeFractional);
            SplitWholeFractional(longitude, out string LongitudeWhole, out string LongitudeFractional);

            //Degree Part
            StringBuilder Result = new StringBuilder();
            Result.Append(GetCombinationNumber(LatitudeWhole, LongitudeWhole).ToString());

            //Decimal Part
            for (int index = 0; index < 5; index++)
            {
                Result.Append(LongitudeFractional.Substring(index, 1));
                Result.Append(LatitudeFractional.Substring(index, 1));
            }

            return Convert.ToUInt64(Result.ToString());
        }

        //Split Degree and Decimal Parts
        static void SplitWholeFractional(double coordinate, out string whole, out string fractional)
        {
            string[] Coordinate = coordinate.ToString("F10", CultureInfo.InvariantCulture).Split('.');
            whole = Coordinate[0];
            fractional = Coordinate[1];
        }

        //Get Combination Number of degrees
        static int GetCombinationNumber(string latitudeWhole, string longitudeWhole)
        {
            int AssignedLongitude = AssignPositive(longitudeWhole);
            int AssignedLatitude = AssignPositive(latitudeWhole);
            
            //# of Combinations for that particular sum
            int Sum = AssignedLongitude + AssignedLatitude;
            int Difference = AssignedLongitude - AssignedLatitude;
            bool IsOdd = Sum % 2 != 0;

            int d1 = 0;
            int d2 = 0;
            int dCombi = 0;

            if (Sum <= 181)
            {
                d1 = (IsOdd ? ((Sum - 1) / 2) + 1 : Sum / 2);
                d2 = (IsOdd ? ((Sum - 1) / 2) : Sum / 2);

                for (int x = Sum - 1; x >= 0; x--)
                {
                    dCombi += x + 1;
                }

                dCombi += (IsOdd ? ((Sum + 1) / 2) + 1 : (Sum / 2) + 1);

            }
            else if (Sum <= 360)
            {
                dCombi += 16653;
                d1 = Sum - 181;
                d2 = 181;
                for (int x = Sum - 1; x >= 182; x--)
                {
                    dCombi += 182;
                }
                dCombi++;
            }
            else if (Sum <= 542)
            {
                dCombi += 49231;
                d1 = 361;
                d2 = Sum - 361;
                for (int x = Sum; x >= 361; x--)
                {
                    dCombi += (543 - x);
                }

            }

            //Correcting
            if (Sum > 0 && Sum <= 181 && Math.Sign(Difference) < 0)
            {
                int x = d1;
                int y = d2;
                int z = dCombi;

                while (true)
                {
                    x--;
                    y++;
                    z--;
                    if ((x + y) != Sum)
                    {
                        dCombi = 66000; //
                        break;
                    }
                    if (x == AssignedLongitude)
                    {
                        dCombi = z;
                        break;
                    }
                }
            }
            else if (Sum > 0 && Sum <= 181 && Math.Sign(Difference) >= 0)
            {
                int x = d1;
                int y = d2;
                int z = dCombi;

                while (true)
                {

                    if ((x + y) != Sum)
                    {
                        dCombi = 66000; //
                        break;
                    }
                    if (x == AssignedLongitude)
                    {
                        dCombi = z;
                        break;
                    }
                    x++;
                    y--;
                    z++;
                }
            }
            ////
            if (Sum >= 182 && Sum <= 360)
            {
                int x = d1;
                int y = d2;
                int z = dCombi;

                while (true)
                {

                    if ((x + y) != Sum)
                    {
                        dCombi = 66000; //
                        break;
                    }
                    if (x == AssignedLongitude)
                    {
                        dCombi = z;
                        break;
                    }
                    x++;
                    y--;
                    z++;
                }
            }
            ////
            if (Sum >= 361)
            {
                int x = d1;
                int y = d2;
                int z = dCombi;
                while (true)
                {

                    if ((x + y) != Sum)
                    {
                        dCombi = 66000; //
                        break;
                    }
                    if (x == AssignedLongitude)
                    {
                        dCombi = z;
                        break;
                    }
                    x--;
                    y++;
                    z--;
                }
            }

            return dCombi;
        }

        //Get Positive integer to Whole-Number
        static ushort AssignPositive(string whole)
        {
            int Result;

            // If Zero
            if (whole == "0")
            {
                Result = 0;
            }
            else if (whole == "-0")
            {
                Result = 1;
            }
            else
            {
                short WholeNumber = Convert.ToInt16(whole);
                Result = (Math.Abs(WholeNumber) * 2) + (WholeNumber < 0 ? 1 : 0);
            }

            return Convert.ToUInt16(Result);
        }

        //Encode Point to GPC
        static string EncodePoint(ulong point)
        {
            ulong Point = point;
            string Result = string.Empty;

            ulong Base = Convert.ToUInt64(CHARACTERS.Length);

            if (Point == 0)
            {
                Result += CHARACTERS[0];
            }
            else
            {
                while (Point > 0)
                {
                    Result = CHARACTERS[Convert.ToInt32(Point % Base)] + Result;
                    Point /= Base;
                }
            }

            return Result;
        }

        //Format GPC
        static string FormatGPC(string gridPointCode)
        {
            StringBuilder Result = new StringBuilder();
            Result.Append("#");

            for (int index = 0; index < gridPointCode.Length; index++)
            {
                if (index == 4 || index == 8)
                {
                    Result.Append("-");
                }
                Result.Append(gridPointCode.Substring(index, 1));
            }

            return Result.ToString();
        }

        //Get Coordinates from GPC
        public static void GetCoordinates(string gridPointCode, out double latitude, out double longitude)
        {
            /*  Unformatting and Validating GPC  */
            string GridPointCode = UnformatNValidateGPC(gridPointCode);

            /*  Getting a Point Number  */
            ulong Point = DecodeToPoint(GridPointCode) - ELEVEN;

            /* Getting Coordinates from Point  */
            GetCoordinates(Point, out latitude, out longitude);
        }

        //Remove format and validate GPC
        static string UnformatNValidateGPC(string gridPointCode)
        {
            string GridPointCode = gridPointCode.Replace(" ", "").Replace("-", "").Replace("#", "").Trim().ToUpperInvariant();

            if (GridPointCode.Length != 11)
            {
                throw new ArgumentOutOfRangeException("GridPointCode",GridPointCode,"Length of GPC must be 11.");
            }

            foreach (char character in GridPointCode)
            {
                if (!CHARACTERS.Contains(character.ToString()))
                {
                    throw new ArgumentOutOfRangeException("character",character,"Invalid character in GPC.");
                }
            }

            return GridPointCode;
        }

        //Decode string to Point
        static ulong DecodeToPoint(string gridPointCode)
        {
            ulong Result = 0;
            ulong Base = Convert.ToUInt64(CHARACTERS.Length);

            for (int i = 0; i < gridPointCode.Length; i++)
            {
                Result *= Base;
                char character = gridPointCode[i];
                Result += Convert.ToUInt64(CHARACTERS.IndexOf(character));
            }
            return Result;
        }

        //Get a Coordinates from Point
        static void GetCoordinates(ulong point, out double latitude, out double longitude)
        {
            string Point = point.ToString();

            string[] WholeFractional = new string[2];
            WholeFractional[0] = Point.Substring(0, Point.Length - 10);   //Whole-Number part
            WholeFractional[1] = Point.Substring(Point.Length - 10);  //Fractional Part

            StringBuilder LatitudeFractional = new StringBuilder();
            StringBuilder LongitudeFractional = new StringBuilder();

            GetWholesFromCombination(Convert.ToInt32(WholeFractional[0]), out string LatitudeWhole, out string LongitudeWhole);

            for (int x = 0; x < WholeFractional[1].Length; x++)
            {
                if (x % 2 == 0)
                {
                    LongitudeFractional.Append(WholeFractional[1].Substring(x, 1));
                }
                else
                {
                    LatitudeFractional.Append(WholeFractional[1].Substring(x, 1));
                }
            }

            latitude = Convert.ToDouble(LatitudeWhole + "." + LatitudeFractional.ToString());
            longitude = Convert.ToDouble(LongitudeWhole + "." + LongitudeFractional.ToString());
        }

        //Get Assigned Positive Integers from Combination number
        static void GetWholesFromCombination(int combinationNumber, out string latitudeWhole, out string longitudeWhole)
        {
            int AssignedLongitude = 0;
            int AssignedLatitude = 0;
            int CombinationRange = 0;
            int Sum = 0;

            for (int i = 0; i <= 543; i++)
            {
                if (i <= 181)
                {
                    CombinationRange += i + 1;
                }
                else if (i <= 360)
                {
                    CombinationRange += 182;
                }
                else if (i <= 542)
                {
                    CombinationRange += (543 - i);
                }

                if (CombinationRange >= combinationNumber)
                {
                    Sum = i;
                    break;
                }
            }
            ////
            int Difference = 0;
            bool isOdd = Sum % 2 != 0;

            int d1 = 0;
            int d2 = 0;
            int dCombi = CombinationRange;

            if (Sum <= 181)
            {
                d1 = (isOdd ? ((Sum - 1) / 2) + 1 : Sum / 2);
                d2 = (isOdd ? ((Sum - 1) / 2) : Sum / 2);

                dCombi -= Sum + 1;

                dCombi += (isOdd ? ((Sum + 1) / 2) + 1 : (Sum / 2) + 1);
            }
            else if (Sum <= 360)
            {
                d1 = Sum - 181;
                d2 = 181;

                dCombi -= 182;

                dCombi++;
            }
            else if (Sum <= 542)
            {
                d1 = 361;
                d2 = Sum - 361;
            }

            ////=>Correcting
            Difference = combinationNumber - dCombi;
            if (Sum > 0 && Sum <= 181 && Math.Sign(Difference) < 0)
            {
                int x = d1;
                int y = d2;
                int z = dCombi;

                while (true)
                {
                    x--;
                    y++;
                    z--;
                    if ((x + y) != Sum)
                    {
                        AssignedLongitude = 999;
                        AssignedLatitude = 999;
                        break;
                    }
                    if (z == combinationNumber)
                    {
                        AssignedLongitude = x;
                        AssignedLatitude = y;
                        break;
                    }
                }
            }
            else if (Sum > 0 && Sum <= 181 && Math.Sign(Difference) >= 0)
            {
                int x = d1;
                int y = d2;
                int z = dCombi;

                while (true)
                {

                    if ((x + y) != Sum)
                    {
                        AssignedLongitude = 999;
                        AssignedLatitude = 999;
                        break;
                    }
                    if (z == combinationNumber)
                    {
                        AssignedLongitude = x;
                        AssignedLatitude = y;
                        break;
                    }
                    x++;
                    y--;
                    z++;
                }
            }
            ////
            if (Sum >= 182 && Sum <= 360)
            {
                int x = d1;
                int y = d2;
                int z = dCombi;

                while (true)
                {
                    if ((x + y) != Sum)
                    {
                        AssignedLongitude = 999;
                        AssignedLatitude = 999;
                        break;
                    }
                    if (z == combinationNumber)
                    {
                        AssignedLongitude = x;
                        AssignedLatitude = y;
                        break;
                    }
                    x++;
                    y--;
                    z++;
                }
            }
            ////
            if (Sum >= 361)
            {
                int x = d1;
                int y = d2;
                int z = dCombi;
                while (true)
                {
                    if ((x + y) != Sum)
                    {
                        AssignedLongitude = 999;
                        AssignedLatitude = 999;
                        break;
                    }
                    if (z == combinationNumber)
                    {
                        AssignedLongitude = x;
                        AssignedLatitude = y;
                        break;
                    }
                    x--;
                    y++;
                    z--;
                }
            }
            //
            longitudeWhole = GetWholeNumber(AssignedLongitude);
            latitudeWhole = GetWholeNumber(AssignedLatitude);
        }

        //Get Degree from Assigned Positive Integer
        static string GetWholeNumber(int assignedPositive)
        {
            string Result;

            bool isNegative = false;
            if (assignedPositive % 2 != 0)
            {
                isNegative = true;
                assignedPositive--;
            }

            Result = (assignedPositive / 2).ToString();

            if (isNegative)
            {
                Result = "-" + Result;
            }

            return Result;
        }
    }
}