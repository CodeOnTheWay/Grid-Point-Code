/*
    Copyright 2017 Pranavkumar Patel
    
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

    http: //www.apache.org/licenses/LICENSE-2.0
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

package GridPointCode;

import java.util.Locale;
import java.math.BigDecimal;
import java.math.RoundingMode;;

public final class GPC
{
	private static final String CHARACTERS = "0123456789CDFGHJKLMNPRTVWXY"; //base27
	private static final long ELEVEN = 205881132094649L; //For Uniformity

	public final static class Coordinates
	{
		public double Latitude;
		public double Longitude;
		public Coordinates()
		{
		}

		public Coordinates(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		public Coordinates clone()
		{
			Coordinates varCopy = new Coordinates();
			varCopy.Latitude = this.Latitude;
			varCopy.Longitude = this.Longitude;
			return varCopy;
		}
	}

	private final static class CoordinateSeven
	{
		public int[] LatitudeSeven;
		public int[] LongitudeSeven;
		public CoordinateSeven()
		{
		}

		public CoordinateSeven(int[] latitudeSeven, int[] longitudeSeven)
		{
			LatitudeSeven = latitudeSeven;
			LongitudeSeven = longitudeSeven;
		}

		public CoordinateSeven clone()
		{
			CoordinateSeven varCopy = new CoordinateSeven();
			varCopy.LatitudeSeven = this.LatitudeSeven;
			varCopy.LongitudeSeven = this.LongitudeSeven;
			return varCopy;
		}
	}

	private final static class CoordinateSignWhole
	{
		public int LatitudeSign;
		public int LatitudeWhole;
		public int LongitudeSign;
		public int LongitudeWhole;
		public CoordinateSignWhole()
		{
		}

		public CoordinateSignWhole(int latitudeSign, int latitudeWhole, int longitudeSign, int longitudeWhole)
		{
			LatitudeSign = latitudeSign;
			LatitudeWhole = latitudeWhole;
			LongitudeSign = longitudeSign;
			LongitudeWhole = longitudeWhole;
		}

		public CoordinateSignWhole clone()
		{
			CoordinateSignWhole varCopy = new CoordinateSignWhole();
			varCopy.LatitudeSign = this.LatitudeSign;
			varCopy.LatitudeWhole = this.LatitudeWhole;
			varCopy.LongitudeSign = this.LongitudeSign;
			varCopy.LongitudeWhole = this.LongitudeWhole;
			return varCopy;
		}
	}
	
	//Math Truncate Method
	private static double MathTruncate(double value)
	{
		if(value < 0)
		{
			return Math.ceil(value);
		} else {
			return Math.floor(value);
		}
	}
	
	private static BigDecimal MathTruncate(BigDecimal value) {
		if(value.compareTo(BigDecimal.ZERO) < 0) {
			return value.setScale(0, RoundingMode.CEILING);
		} else {
			return value.setScale(0, RoundingMode.FLOOR);
		}
	}

						/*  PART 1 : ENCODE */

	//Get a Grid Point Code
	public static String GetGridPointCode(double latitude, double longitude)
	{
		return GetGridPointCode(latitude, longitude, true);
	}

	public static String GetGridPointCode(double latitude, double longitude, boolean formatted)
	{
		double Latitude = latitude; //Latitude
		double Longitude = longitude; //Longitude

		/*  Validating Latitude and Longitude values */
		if (Latitude <= -90 || Latitude >= 90)
		{
			throw new IllegalArgumentException("Latitude value must be between -90 to 90.");
		}

		if (Longitude <= -180 || Longitude >= 180)
		{
			throw new IllegalArgumentException("Longitude value must be between -180 to 180.");
		}

		/*  Getting a Point Number  */
		long Point = GetPointNumber(Latitude, Longitude);

		/*  Encode Point    */
		String GridPointCode = EncodePoint(Point + ELEVEN);

		/*  Format GridPointCode   */
		if (formatted)
		{
			GridPointCode = FormatGPC(GridPointCode);
		}

		return GridPointCode;
	}

	//Get Point from Coordinates
	private static long GetPointNumber(double latitude, double longitude)
	{
		int[] LatitudeSeven = GetCoordinateSeven(latitude);
		int[] LongitudeSeven = GetCoordinateSeven(longitude);

		//Whole-Number Part
		CoordinateSignWhole SignWhole = new CoordinateSignWhole(LatitudeSeven[0], LatitudeSeven[1], LongitudeSeven[0], LongitudeSeven[1]);
		long Point = (long)(Math.pow(10,10) * GetCombinationNumber(SignWhole.clone()));

		//Fractional Part
		int Power = 9;
		for (int index = 2; index <= 6; index++)
		{
			Point = Point + (long)(Math.pow(10,Power--) * LongitudeSeven[index]);
			Point = Point + (long)(Math.pow(10,Power--) * LatitudeSeven[index]);
		}
		return Point;
	}

	//Break down coordinate into seven parts
	private static int[] GetCoordinateSeven(double coordinate)
	{
		int[] Result = new int[7];

		//Sign
		Result[0] = (coordinate < 0 ? -1 : 1);

		//Whole-Number
		Result[1] = (int)MathTruncate(Math.abs(coordinate));

		//Fractional
		BigDecimal AbsCoordinate = BigDecimal.valueOf(Math.abs(coordinate));
		BigDecimal Fractional = AbsCoordinate.subtract(MathTruncate(AbsCoordinate));
		BigDecimal Power10 = new BigDecimal(0);
		for (int x = 1; x <= 5; x++)
		{
			Power10 = Fractional.multiply(new BigDecimal(10));
			BigDecimal temp = MathTruncate(Power10); 
			Result[x + 1] = temp.intValue();
			Fractional = Power10.subtract(temp);
		}

		return Result;
	}

	//Get Combination Number of Whole-Numbers
	private static int GetCombinationNumber(CoordinateSignWhole signWhole)
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
	private static String EncodePoint(long point)
	{
		long Point = point;
		String Result = "";

		long Base = (long)CHARACTERS.length();

		if (Point == 0)
		{
			Result += CHARACTERS.charAt(0);
		}
		else
		{
			while (Point > 0)
			{
				Result = CHARACTERS.charAt((int)(Point % Base)) + Result;
				Point /= Base;
			}
		}

		return Result;
	}

	//Format GPC
	private static String FormatGPC(String gridPointCode)
	{
		String Result = "#";

		for (int index = 0; index < gridPointCode.length(); index++)
		{
			if (index == 4 || index == 8)
			{
				Result += "-";
			}
			Result += gridPointCode.substring(index, index + 1);
		}

		return Result;
	}

						/*  PART 2 : DECODE */

	//Get Coordinates from GPC
	public static Coordinates GetCoordinates(String gridPointCode)
	{
		/*  Unformatting and Validating GPC  */
		String GridPointCode = UnformatNValidateGPC(gridPointCode);

		/*  Getting a Point Number  */
		long Point = DecodeToPoint(GridPointCode) - ELEVEN;

		/* Getting Coordinates from Point  */
		return GetCoordinates(Point);
	}

	//Remove format and validate GPC
	private static String UnformatNValidateGPC(String gridPointCode)
	{
		String GridPointCode = gridPointCode.replace(" ", "").replace("-", "").replace("#", "").trim().toUpperCase(Locale.ROOT);

		if (GridPointCode.length() != 11)
		{
			throw new IllegalArgumentException("Length of GPC must be 11.");
		}

		for (char character : GridPointCode.toCharArray())
		{
			if (!CHARACTERS.contains(String.valueOf(character)))
			{
				throw new IllegalArgumentException("Invalid character in GPC.");
			}
		}

		return GridPointCode;
	}

	//Decode string to Point
	private static long DecodeToPoint(String gridPointCode)
	{
		long Result = 0;
		long Base = (long)CHARACTERS.length();

		for (int i = 0; i < gridPointCode.length(); i++)
		{
			Result *= Base;
			char character = gridPointCode.charAt(i);
			Result += (long)CHARACTERS.indexOf(character);
		}
		return Result;
	}

	//Get a Coordinates from Point
	private static Coordinates GetCoordinates(long point)
	{
		int Combination = (int)MathTruncate((point / Math.pow(10, 10)));
		long Fractional = (long)(point - (Combination * Math.pow(10, 10)));

		return GetCoordinates(GetCoordinateSeven(Combination, Fractional));
	}

	//Combine Seven Parts to Coordinate
	private static Coordinates GetCoordinates(CoordinateSeven CSeven)
	{
		int Power = 0;
		int TempLatitude = 0;
		int TempLongitude = 0;
		for (int x = 6; x >= 1; x--)
		{
			TempLatitude += (int)(CSeven.LatitudeSeven[x] * Math.pow(10, Power));
			TempLongitude += (int)(CSeven.LongitudeSeven[x] * Math.pow(10, Power++));
		}

		double Latitude = (TempLatitude / Math.pow(10, 5)) * CSeven.LatitudeSeven[0];
		double Longitude = (TempLongitude / Math.pow(10, 5)) * CSeven.LongitudeSeven[0];

		return new Coordinates(Latitude, Longitude);
	}

	//Get Seven Parts of Coordinate
	private static CoordinateSeven GetCoordinateSeven(int combination, long fractional)
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
			LongitudeSeven[x] = (int)(((long)(MathTruncate(fractional / Math.pow(10, Power--)))) % 10);
			LatitudeSeven[x] = (int)(((long)(MathTruncate(fractional / Math.pow(10, Power--)))) % 10);
		}

		return new CoordinateSeven(LatitudeSeven, LongitudeSeven);
	}

	//Get Whole-Numbers from Combination number
	private static CoordinateSignWhole GetWholesFromCombination(int combination)
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

			boolean IsLast = (combination - XCombination) % 180 == 0;
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
