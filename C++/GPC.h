#pragma once

/*
 * Copyright 2017 Pranavkumar Patel
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#include <vector>
#include <algorithm>
#include <sstream>

using namespace std;

namespace GridPointCode {
	class GPC final {
	private:
		static const string CHARACTERS; //base27
		static constexpr unsigned long long ELEVEN = 205881132094649; //For Uniformity

	public:
		class Coordinates {
		public:
			const double Latitude;
			const double Longitude;
			Coordinates(double latitude, double longitude);
			Coordinates() = default;
		};

	private:
		class CoordinateSeven {
		public:
			vector<int> LatitudeSeven;
			vector<int> LongitudeSeven;
			CoordinateSeven(vector<int> &latitudeSeven, vector<int> &longitudeSeven);
			CoordinateSeven() = default;
		};

		class CoordinateSignWhole {
		public:
			int LatitudeSign = 0;
			int LatitudeWhole = 0;
			int LongitudeSign = 0;
			int LongitudeWhole = 0;
			CoordinateSignWhole(int latitudeSign, int latitudeWhole, int longitudeSign, int longitudeWhole);
			CoordinateSignWhole() = default;
		};
		
		/*  PART 1 : ENCODE */
		
		//Get a Grid Point Code
	public:
		static string GetGridPointCode(double latitude, double longitude);
		static string GetGridPointCode(double latitude, double longitude, bool formatted);

		//Get Point from Coordinates
	private:
		static unsigned long long GetPointNumber(double latitude, double longitude);

		//Break down coordinate into seven parts
		static vector<int> GetCoordinateSeven(double coordinate);

		//Get Combination Number of Whole-Numbers
		static int GetCombinationNumber(CoordinateSignWhole signWhole);

		//Encode Point to GPC
		static string EncodePoint(unsigned long long point);

		//Format GPC
		static string FormatGPC(const string &gridPointCode);

		/*  PART 2 : DECODE */
		
		//Get Coordinates from GPC
	public:
		static Coordinates GetCoordinates(string gridPointCode);

		//Remove format and validate GPC
	private:
		static string UnformatNValidateGPC(string gridPointCode);

		//Decode string to Point
		static unsigned long long DecodeToPoint(const string &gridPointCode);

		//Get a Coordinates from Point
		static Coordinates GetCoordinates(unsigned long long point);

		//Combine Seven Parts to Coordinate
		static Coordinates GetCoordinates(CoordinateSeven CSeven);

		//Get Seven Parts of Coordinate
		static CoordinateSeven GetCoordinateSeven(int combination, unsigned long long fractional);

		//Get Whole-Numbers from Combination number
		static CoordinateSignWhole GetWholesFromCombination(int combination);

	};
}