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

namespace GridPointCode
{
    public static class GPC
    {
        const string CHARACTERS = "0123456789CDFGHJKLMNPRTVWXY";   //base27
        const ulong ELEVEN = 205881132094649;   //For Uniformity

                            /*  PART 1 : ENCODE */

        //Get a Grid Point Code
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

            // IMPORTANT: Checking floating point equality with exact values and not using a range instead.
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
            int[] LatitudeSeven = GetCoordinateSeven(latitude);
            int[] LongitudeSeven = GetCoordinateSeven(longitude);
            
            //Whole-Number Part
            ulong Point = (ulong)(Math.Pow(10,10) * GetCombinationNumber(LatitudeSeven[0],LatitudeSeven[1],LongitudeSeven[0],LongitudeSeven[1]));

            //Fractional Part
            int Power = 9;
            for (int index = 2; index <= 6; index++)
            {
                Point = Point + (ulong)(Math.Pow(10,Power--) * LongitudeSeven[index]);
                Point = Point + (ulong)(Math.Pow(10,Power--) * LatitudeSeven[index]);
            }
            return Point;
        }

        //Break down coordinate into seven parts
        static int[] GetCoordinateSeven(double coordinate)
        {
            int[] Result = new int[7];

            //Sign
            Result[0] = (coordinate < 0 ? -1 : 1);

            //Whole-Number
            Result[1] = (int)Math.Truncate(Math.Abs(coordinate));

            //Fractional
            decimal AbsCoordinate = (decimal)Math.Abs(coordinate);
            decimal Fractional = AbsCoordinate - (int)Math.Truncate(AbsCoordinate);
            decimal Power10;
            for (int x = 1; x <= 5; x++)
            {
                Power10 = Fractional * 10;
                Result[x + 1] = (int)Math.Truncate(Power10);
                Fractional = Power10 - Result[x + 1];
            }

            return Result;
        }

        //Get Combination Number of Whole-Numbers
        static int GetCombinationNumber(int latitudeSign, int latitudeWhole, int longitudeSign, int longitudeWhole)
        {
            int AssignedLongitude = (longitudeWhole * 2) + (longitudeSign == -1 ? 1 : 0);
            int AssignedLatitude = (latitudeWhole * 2) + (latitudeSign == -1 ? 1 : 0);

            //# of Combinations for that particular sum
            int Sum = AssignedLongitude + AssignedLatitude;
            int Difference = AssignedLongitude - AssignedLatitude;
            bool IsOdd = Sum % 2 != 0;

            int D1 = 0;
            int D2 = 0;
            int DCombi = 0;

            if (Sum <= 181)
            {
                D1 = (IsOdd ? ((Sum - 1) / 2) + 1 : Sum / 2);
                D2 = (IsOdd ? ((Sum - 1) / 2) : Sum / 2);

                for (int x = Sum - 1; x >= 0; x--)
                {
                    DCombi += x + 1;
                }

                DCombi += (IsOdd ? ((Sum + 1) / 2) + 1 : (Sum / 2) + 1);

            }
            else if (Sum <= 360)
            {
                DCombi += 16653;
                D1 = Sum - 181;
                D2 = 181;
                for (int x = Sum - 1; x >= 182; x--)
                {
                    DCombi += 182;
                }
                DCombi++;
            }
            else if (Sum <= 542)
            {
                DCombi += 49231;
                D1 = 361;
                D2 = Sum - 361;
                for (int x = Sum; x >= 361; x--)
                {
                    DCombi += (543 - x);
                }

            }

            //Correcting
            if (Sum > 0 && Sum <= 181 && Math.Sign(Difference) < 0)
            {
                int X = D1;
                int Y = D2;
                int Z = DCombi;

                while (true)
                {
                    X--;
                    Y++;
                    Z--;
                    if ((X + Y) != Sum)
                    {
                        DCombi = 66000; //
                        break;
                    }
                    if (X == AssignedLongitude)
                    {
                        DCombi = Z;
                        break;
                    }
                }
            }
            else if (Sum > 0 && Sum <= 181 && Math.Sign(Difference) >= 0)
            {
                int X = D1;
                int Y = D2;
                int Z = DCombi;

                while (true)
                {

                    if ((X + Y) != Sum)
                    {
                        DCombi = 66000; //
                        break;
                    }
                    if (X == AssignedLongitude)
                    {
                        DCombi = Z;
                        break;
                    }
                    X++;
                    Y--;
                    Z++;
                }
            }
            ////
            if (Sum >= 182 && Sum <= 360)
            {
                int X = D1;
                int Y = D2;
                int Z = DCombi;

                while (true)
                {

                    if ((X + Y) != Sum)
                    {
                        DCombi = 66000; //
                        break;
                    }
                    if (X == AssignedLongitude)
                    {
                        DCombi = Z;
                        break;
                    }
                    X++;
                    Y--;
                    Z++;
                }
            }
            ////
            if (Sum >= 361)
            {
                int X = D1;
                int Y = D2;
                int Z = DCombi;
                while (true)
                {

                    if ((X + Y) != Sum)
                    {
                        DCombi = 66000; //
                        break;
                    }
                    if (X == AssignedLongitude)
                    {
                        DCombi = Z;
                        break;
                    }
                    X--;
                    Y++;
                    Z--;
                }
            }

            return DCombi;
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
            string Result = "#";

            for (int index = 0; index < gridPointCode.Length; index++)
            {
                if (index == 4 || index == 8)
                {
                    Result += "-";
                }
                Result += gridPointCode.Substring(index, 1);
            }

            return Result;
        }

                            /*  PART 2 : DECODE */

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
            int Combination = (int)Math.Truncate((point / Math.Pow(10, 10)));
            ulong Fractional = (ulong)(point - (Combination * Math.Pow(10, 10)));

            GetCoordinateSeven(Combination, Fractional, out int[] LongitudeSeven, out int[] LatitudeSeven);

            GetCoordinates(LongitudeSeven, LatitudeSeven, out double Longitude, out double Latitude);

            latitude = Latitude;
            longitude = Longitude;
        }

        //Combine Seven Parts to Coordinate
        private static void GetCoordinates(int[] longitudeSeven, int[] latitudeSeven, out double longitude, out double latitude)
        {
            int Power = 0;
            int TempLatitude = 0;
            int TempLongitude = 0;
            for (int x = 6; x >= 1; x--)
            {
                TempLatitude += (int)(latitudeSeven[x] * Math.Pow(10, Power));
                TempLongitude += (int)(longitudeSeven[x] * Math.Pow(10, Power++));
            }

            double Latitude = (TempLatitude / Math.Pow(10, 5)) * latitudeSeven[0];
            double Longitude = (TempLongitude / Math.Pow(10, 5)) * longitudeSeven[0];

            latitude = Latitude;
            longitude = Longitude;
        }

        //Get Seven Parts of Coordinate
        private static void GetCoordinateSeven(int combination, ulong fractional, out int[] longitudeSeven, out int[] latitudeSeven)
        {
            int[] LongitudeSeven = new int[7];
            int[] LatitudeSeven = new int[7];

            GetWholesFromCombination(combination, out LatitudeSeven[0], out LatitudeSeven[1], out LongitudeSeven[0], out LongitudeSeven[1]);

            int Power = 9;
            for (int x = 2; x <= 6; x++)
            {
                LongitudeSeven[x] = (int)(((ulong)(Math.Truncate(fractional / Math.Pow(10, Power--)))) % 10);
                LatitudeSeven[x] = (int)(((ulong)(Math.Truncate(fractional / Math.Pow(10, Power--)))) % 10);
            }

            longitudeSeven = LongitudeSeven;
            latitudeSeven = LatitudeSeven;
        }

        //Get Whole-Numbers from Combination number
        static void GetWholesFromCombination(int combinationNumber, out int latitudeSign, out int latitudeWhole, out int longitudeSign, out int longitudeWhole)
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
            bool IsOdd = Sum % 2 != 0;

            int D1 = 0;
            int D2 = 0;
            int DCombi = CombinationRange;

            if (Sum <= 181)
            {
                D1 = (IsOdd ? ((Sum - 1) / 2) + 1 : Sum / 2);
                D2 = (IsOdd ? ((Sum - 1) / 2) : Sum / 2);

                DCombi -= Sum + 1;

                DCombi += (IsOdd ? ((Sum + 1) / 2) + 1 : (Sum / 2) + 1);
            }
            else if (Sum <= 360)
            {
                D1 = Sum - 181;
                D2 = 181;

                DCombi -= 182;

                DCombi++;
            }
            else if (Sum <= 542)
            {
                D1 = 361;
                D2 = Sum - 361;
            }

            ////=>Correcting
            Difference = combinationNumber - DCombi;
            if (Sum > 0 && Sum <= 181 && Math.Sign(Difference) < 0)
            {
                int X = D1;
                int Y = D2;
                int Z = DCombi;

                while (true)
                {
                    X--;
                    Y++;
                    Z--;
                    if ((X + Y) != Sum)
                    {
                        AssignedLongitude = 999;
                        AssignedLatitude = 999;
                        break;
                    }
                    if (Z == combinationNumber)
                    {
                        AssignedLongitude = X;
                        AssignedLatitude = Y;
                        break;
                    }
                }
            }
            else if (Sum > 0 && Sum <= 181 && Math.Sign(Difference) >= 0)
            {
                int X = D1;
                int Y = D2;
                int Z = DCombi;

                while (true)
                {
                    if ((X + Y) != Sum)
                    {
                        AssignedLongitude = 999;
                        AssignedLatitude = 999;
                        break;
                    }
                    if (Z == combinationNumber)
                    {
                        AssignedLongitude = X;
                        AssignedLatitude = Y;
                        break;
                    }
                    X++;
                    Y--;
                    Z++;
                }
            }
            ////
            if (Sum >= 182 && Sum <= 360)
            {
                int X = D1;
                int Y = D2;
                int Z = DCombi;

                while (true)
                {
                    if ((X + Y) != Sum)
                    {
                        AssignedLongitude = 999;
                        AssignedLatitude = 999;
                        break;
                    }
                    if (Z == combinationNumber)
                    {
                        AssignedLongitude = X;
                        AssignedLatitude = Y;
                        break;
                    }
                    X++;
                    Y--;
                    Z++;
                }
            }
            ////
            if (Sum >= 361)
            {
                int X = D1;
                int Y = D2;
                int Z = DCombi;
                while (true)
                {
                    if ((X + Y) != Sum)
                    {
                        AssignedLongitude = 999;
                        AssignedLatitude = 999;
                        break;
                    }
                    if (Z == combinationNumber)
                    {
                        AssignedLongitude = X;
                        AssignedLatitude = Y;
                        break;
                    }
                    X--;
                    Y++;
                    Z--;
                }
            }
            //
            longitudeSign = (AssignedLongitude % 2 != 0 ? -1 : 1);
            longitudeWhole = (longitudeSign == -1 ? (--AssignedLongitude / 2)  : (AssignedLongitude / 2));
            latitudeSign = (AssignedLatitude % 2 != 0 ? -1 : 1);
            latitudeWhole = (latitudeSign == -1 ? (--AssignedLatitude / 2) : (AssignedLatitude / 2));
        }

    }
}