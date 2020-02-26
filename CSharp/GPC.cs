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

        public struct Coordinates
        {
            public readonly double Latitude;
            public readonly double Longitude;
            public Coordinates(double latitude, double longitude)
            {
                Latitude = latitude;
                Longitude = longitude;
            }
        };

        struct CoordinateSeven
        {
            public int[] LatitudeSeven;
            public int[] LongitudeSeven;
            public CoordinateSeven(int[] latitudeSeven, int[] longitudeSeven)
            {
                LatitudeSeven = latitudeSeven;
                LongitudeSeven = longitudeSeven;
            }
        };

        struct CoordinateSignWhole
        {
            public int LatitudeSign;
            public int LatitudeWhole;
            public int LongitudeSign;
            public int LongitudeWhole;
            public CoordinateSignWhole(int latitudeSign,int latitudeWhole, int longitudeSign, int longitudeWhole)
            {
                LatitudeSign = latitudeSign;
                LatitudeWhole = latitudeWhole;
                LongitudeSign = longitudeSign;
                LongitudeWhole = longitudeWhole;
            }
        };

                            /*  PART 1 : ENCODE */

        //Get a Grid Point Code
        public static string GetGridPointCode(double latitude, double longitude)
        {
            return GetGridPointCode(latitude, longitude, true);
        }

        public static string GetGridPointCode(double latitude, double longitude, bool formatted)
        {
            double Latitude = latitude; //Latitude
            double Longitude = longitude;   //Longitude

            /*  Validating Latitude and Longitude values */
            if (Latitude <= -90 || Latitude >= 90)
            {
                throw new ArgumentOutOfRangeException("Latitude", Latitude, "Latitude value must be between -90 to 90.");
            }
            if (Longitude <= -180 || Longitude >= 180)
            {
                throw new ArgumentOutOfRangeException("Longitude", Longitude, "Longitude value must be between -180 to 180.");
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
            CoordinateSignWhole SignWhole = new CoordinateSignWhole(LatitudeSeven[0], LatitudeSeven[1], LongitudeSeven[0], LongitudeSeven[1]);
            ulong Point = (ulong)(Math.Pow(10,10) * GetCombinationNumber(SignWhole));

            //Fractional Part
            int Power = 9;
            for (int index = 2; index <= 6; index++)
            {
                Point += (ulong)(Math.Pow(10,Power--) * LongitudeSeven[index]);
                Point += (ulong)(Math.Pow(10,Power--) * LatitudeSeven[index]);
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
        static int GetCombinationNumber(CoordinateSignWhole signWhole)
        {
            int AssignedLongitude = (signWhole.LongitudeWhole * 2) + (signWhole.LongitudeSign == -1 ? 1 : 0);
            int AssignedLatitude = (signWhole.LatitudeWhole * 2) + (signWhole.LatitudeSign == -1 ? 1 : 0);

            int MaxSum = 538;
            int Sum = AssignedLongitude + AssignedLatitude;
            int Combination = 0;

            if (Sum <= 179)
            {
                for (int xSum = (Sum - 1); xSum >= 0; xSum--)
                {
                    Combination += xSum + 1;
                }

                Combination += AssignedLongitude + 1;

            }
            else if (Sum <= 359)
            {
                Combination += 16290;

                Combination += (Sum - 180) * 180;

                Combination += 180 - AssignedLatitude;

            }
            else if (Sum <= 538)
            {
                Combination += 48690;

                for (int xSum = (Sum - 1); xSum >= 360; xSum--)
                {
                    Combination += MaxSum - xSum + 1;
                }

                Combination += 180 - AssignedLatitude;
            }

            return Combination;
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
        public static Coordinates GetCoordinates(string gridPointCode)
        {
            /*  Unformatting and Validating GPC  */
            string GridPointCode = UnformatNValidateGPC(gridPointCode);

            /*  Getting a Point Number  */
            ulong Point = DecodeToPoint(GridPointCode) - ELEVEN;

            /* Getting Coordinates from Point  */
            return GetCoordinates(Point);
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
        static Coordinates GetCoordinates(ulong point)
        {
            int Combination = (int)Math.Truncate((point / Math.Pow(10, 10)));
            ulong Fractional = (ulong)(point - (Combination * Math.Pow(10, 10)));

            return GetCoordinates(GetCoordinateSeven(Combination, Fractional));
        }

        //Combine Seven Parts to Coordinate
        static Coordinates GetCoordinates(CoordinateSeven CSeven)
        {
            int Power = 0;
            int TempLatitude = 0;
            int TempLongitude = 0;
            for (int x = 6; x >= 1; x--)
            {
                TempLatitude += (int)(CSeven.LatitudeSeven[x] * Math.Pow(10, Power));
                TempLongitude += (int)(CSeven.LongitudeSeven[x] * Math.Pow(10, Power++));
            }

            double Latitude = (TempLatitude / Math.Pow(10, 5)) * CSeven.LatitudeSeven[0];
            double Longitude = (TempLongitude / Math.Pow(10, 5)) * CSeven.LongitudeSeven[0];

            return new Coordinates(Latitude, Longitude);
        }

        //Get Seven Parts of Coordinate
        static CoordinateSeven GetCoordinateSeven(int combination, ulong fractional)
        {
            int[] LongitudeSeven = new int[7];
            int[] LatitudeSeven = new int[7];

            CoordinateSignWhole SignWhole = GetWholesFromCombination(combination);
            LongitudeSeven[0] = SignWhole.LongitudeSign;
            LongitudeSeven[1] = SignWhole.LongitudeWhole;
            LatitudeSeven[0] = SignWhole.LatitudeSign;
            LatitudeSeven[1] = SignWhole.LatitudeWhole;

            int Power = 9;
            for (int x = 2; x <= 6; x++)
            {
                LongitudeSeven[x] = (int)(((ulong)(Math.Truncate(fractional / Math.Pow(10, Power--)))) % 10);
                LatitudeSeven[x] = (int)(((ulong)(Math.Truncate(fractional / Math.Pow(10, Power--)))) % 10);
            }

            return new CoordinateSeven(LatitudeSeven, LongitudeSeven);
        }

        //Get Whole-Numbers from Combination number
        static CoordinateSignWhole GetWholesFromCombination(int combination)
        {
            int MaxSum = 538;

            int XSum = 0;
            int XCombination = 0;

            int Sum = 0;
            int MaxCombination = 0;
            int AssignedLongitude = 0;
            int AssignedLatitude = 0;
            
            if (combination <= 16290)
            {
                for (XSum = 0; MaxCombination < combination; XSum++)
                {
                    MaxCombination += XSum + 1;
                }
                Sum = XSum - 1;
                XCombination = MaxCombination - (Sum + 1);

                AssignedLongitude = combination - XCombination - 1;
                AssignedLatitude = Sum - AssignedLongitude;

            }
            else if (combination <= 48690)
            {
                XCombination = 16290;
                XSum = 179;

                bool IsLast = (combination - XCombination) % 180 == 0;
                int Pre = ((combination - XCombination) / 180) - (IsLast ? 1 : 0);

                XSum += Pre;
                XCombination += Pre * 180;

                Sum = XSum + 1;
                AssignedLatitude = 180 - (combination - XCombination);
                AssignedLongitude = Sum - AssignedLatitude;

            }
            else if (combination <= 64800)
            {
                XCombination = 48690;
                XSum = 359;
                MaxCombination += XCombination;
                for (XSum = 360; MaxCombination < combination; XSum++)
                {
                    MaxCombination += (MaxSum - XSum + 1);
                }
                Sum = XSum - 1;
                XCombination = MaxCombination - (MaxSum - Sum + 1);

                AssignedLatitude = 180 - (combination - XCombination);
                AssignedLongitude = Sum - AssignedLatitude;
            }

            //
            CoordinateSignWhole Result = new CoordinateSignWhole();
            Result.LongitudeSign = (AssignedLongitude % 2 != 0 ? -1 : 1);
            Result.LongitudeWhole = (Result.LongitudeSign == -1 ? (--AssignedLongitude / 2) : (AssignedLongitude / 2));
            Result.LatitudeSign = (AssignedLatitude % 2 != 0 ? -1 : 1);
            Result.LatitudeWhole = (Result.LatitudeSign == -1 ? (--AssignedLatitude / 2) : (AssignedLatitude / 2));
            return Result;
        }

    }
}