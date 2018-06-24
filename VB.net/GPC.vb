'
'    Copyright 2017 Pranavkumar Patel
'    
'    Licensed under the Apache License, Version 2.0 (the "License");
'    you may not use this file except in compliance with the License.
'    You may obtain a copy of the License at
'
'    http://www.apache.org/licenses/LICENSE-2.0
'    Unless required by applicable law or agreed to in writing, software
'    distributed under the License is distributed on an "AS IS" BASIS,
'    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'    See the License for the specific language governing permissions and
'    limitations under the License.
'

Imports System

Namespace GridPointCode
    Public Module GPC
        Private Const CHARACTERS As String = "0123456789CDFGHJKLMNPRTVWXY" 'base27
        Private Const ELEVEN As ULong = 205881132094649 'For Uniformity

        Public Structure Coordinates
            Public ReadOnly Latitude As Double
            Public ReadOnly Longitude As Double
            Public Sub New(ByVal latitude As Double, ByVal longitude As Double)
                Me.Latitude = latitude
                Me.Longitude = longitude
            End Sub
        End Structure

        Private Structure CoordinateSeven
            Public LatitudeSeven() As Integer
            Public LongitudeSeven() As Integer
            Public Sub New(ByVal latitudeSeven() As Integer, ByVal longitudeSeven() As Integer)
                Me.LatitudeSeven = latitudeSeven
                Me.LongitudeSeven = longitudeSeven
            End Sub
        End Structure

        Private Structure CoordinateSignWhole
            Public LatitudeSign As Integer
            Public LatitudeWhole As Integer
            Public LongitudeSign As Integer
            Public LongitudeWhole As Integer
            Public Sub New(ByVal latitudeSign As Integer, ByVal latitudeWhole As Integer, ByVal longitudeSign As Integer, ByVal longitudeWhole As Integer)
                Me.LatitudeSign = latitudeSign
                Me.LatitudeWhole = latitudeWhole
                Me.LongitudeSign = longitudeSign
                Me.LongitudeWhole = longitudeWhole
            End Sub
        End Structure

        '  PART 1 : ENCODE 

        'Get a Grid Point Code
        Public Function GetGridPointCode(ByVal latitude As Double, ByVal longitude As Double) As String
            Return GetGridPointCode(latitude, longitude, True)
        End Function

        Public Function GetGridPointCode(ByVal latitude As Double, ByVal longitude As Double, ByVal formatted As Boolean) As String
            Dim Latitude_ As Double = latitude 'Latitude
            Dim Longitude_ As Double = longitude 'Longitude

            '  Validating Latitude and Longitude values 
            If Latitude_ <= -90 OrElse Latitude_ >= 90 Then
                Throw New ArgumentOutOfRangeException("Latitude", Latitude_, "Latitude value must be between -90 to 90.")
            End If
            If Longitude_ <= -180 OrElse Longitude_ >= 180 Then
                Throw New ArgumentOutOfRangeException("Longitude", Longitude_, "Longitude value must be between -180 to 180.")
            End If

            '  Getting a Point Number  
            Dim Point As ULong = GetPointNumber(Latitude_, Longitude_)

            '  Encode Point    
            Dim GridPointCode As String = EncodePoint(Point + ELEVEN)

            '  Format GridPointCode   
            If formatted Then
                GridPointCode = FormatGPC(GridPointCode)
            End If

            Return GridPointCode
        End Function

        'Get Point from Coordinates
        Private Function GetPointNumber(ByVal latitude As Double, ByVal longitude As Double) As ULong
            Dim LatitudeSeven() As Integer = GetCoordinateSeven(latitude)
            Dim LongitudeSeven() As Integer = GetCoordinateSeven(longitude)

            'Whole-Number Part
            Dim SignWhole As New CoordinateSignWhole(LatitudeSeven(0), LatitudeSeven(1), LongitudeSeven(0), LongitudeSeven(1))
            Dim Point As ULong = CULng(Math.Truncate(Math.Pow(10, 10) * GetCombinationNumber(SignWhole)))

            'Fractional Part
            Dim Power As Integer = 9
            For index As Integer = 2 To 6
                Point = Point + CULng(Math.Truncate(Math.Pow(10, Power) * LongitudeSeven(index)))
                Power -= 1
                Point = Point + CULng(Math.Truncate(Math.Pow(10, Power) * LatitudeSeven(index)))
                Power -= 1
            Next index
            Return Point
        End Function

        'Break down coordinate into seven parts
        Private Function GetCoordinateSeven(ByVal coordinate As Double) As Integer()
            Dim Result(6) As Integer

            'Sign
            Result(0) = (If(coordinate < 0, -1, 1))

            'Whole-Number
            Result(1) = CInt(Math.Truncate(Math.Truncate(Math.Abs(coordinate))))

            'Fractional
            Dim AbsCoordinate As Decimal = CDec(Math.Abs(coordinate))
            Dim Fractional As Decimal = AbsCoordinate - CInt(Math.Truncate(Math.Truncate(AbsCoordinate)))
            Dim Power10 As Decimal
            For x As Integer = 1 To 5
                Power10 = Fractional * 10
                Result(x + 1) = CInt(Math.Truncate(Math.Truncate(Power10)))
                Fractional = Power10 - Result(x + 1)
            Next x

            Return Result
        End Function

        'Get Combination Number of Whole-Numbers
        Private Function GetCombinationNumber(ByVal signWhole As CoordinateSignWhole) As Integer
            Dim AssignedLongitude As Integer = (signWhole.LongitudeWhole * 2) + (If(signWhole.LongitudeSign = -1, 1, 0))
            Dim AssignedLatitude As Integer = (signWhole.LatitudeWhole * 2) + (If(signWhole.LatitudeSign = -1, 1, 0))

            Dim MaxSum As Integer = 538
            Dim Sum As Integer = AssignedLongitude + AssignedLatitude
            Dim Combination As Integer = 0

            If Sum <= 179 Then
                For xSum As Integer = (Sum - 1) To 0 Step -1
                    Combination += xSum + 1
                Next xSum

                Combination += AssignedLongitude + 1

            ElseIf Sum <= 359 Then
                Combination += 16290

                Combination += (Sum - 180) * 180

                Combination += 180 - AssignedLatitude

            ElseIf Sum <= 538 Then
                Combination += 48690

                For xSum As Integer = (Sum - 1) To 360 Step -1
                    Combination += MaxSum - xSum + 1
                Next xSum

                Combination += 180 - AssignedLatitude
            End If

            Return Combination
        End Function

        'Encode Point to GPC
        Private Function EncodePoint(ByVal point As ULong) As String
            Dim Point_ As ULong = point
            Dim Result As String = String.Empty

            Dim Base As ULong = Convert.ToUInt64(CHARACTERS.Length)

            If Point_ = 0 Then
                Result &= CHARACTERS.Chars(0)
            Else
                Do While Point_ > 0
                    Result = CHARACTERS.Chars(Convert.ToInt32(Point_ Mod Base)) & Result
                    Point_ \= Base
                Loop
            End If

            Return Result
        End Function

        'Format GPC
        Private Function FormatGPC(ByVal gridPointCode As String) As String
            Dim Result As String = "#"

            For index As Integer = 0 To gridPointCode.Length - 1
                If index = 4 OrElse index = 8 Then
                    Result &= "-"
                End If
                Result &= gridPointCode.Substring(index, 1)
            Next index

            Return Result
        End Function

        '  PART 2 : DECODE 

        'Get Coordinates from GPC
        Public Function GetCoordinates(ByVal gridPointCode As String) As Coordinates
            '  Unformatting and Validating GPC  
            Dim GridPointCode_ As String = UnformatNValidateGPC(gridPointCode)

            '  Getting a Point Number  
            Dim Point As ULong = DecodeToPoint(GridPointCode_) - ELEVEN

            ' Getting Coordinates from Point  
            Return GetCoordinates(Point)
        End Function

        'Remove format and validate GPC
        Private Function UnformatNValidateGPC(ByVal gridPointCode As String) As String
            Dim GridPointCode_ As String = gridPointCode.Replace(" ", "").Replace("-", "").Replace("#", "").Trim().ToUpperInvariant()

            If GridPointCode_.Length <> 11 Then
                Throw New ArgumentOutOfRangeException("GridPointCode", GridPointCode_, "Length of GPC must be 11.")
            End If

            For Each character As Char In GridPointCode_
                If Not CHARACTERS.Contains(character.ToString()) Then
                    Throw New ArgumentOutOfRangeException("character", character, "Invalid character in GPC.")
                End If
            Next character

            Return GridPointCode_
        End Function

        'Decode string to Point
        Private Function DecodeToPoint(ByVal gridPointCode As String) As ULong
            Dim Result As ULong = 0
            Dim Base As ULong = Convert.ToUInt64(CHARACTERS.Length)

            For i As Integer = 0 To gridPointCode.Length - 1
                Result *= Base
                Dim character As Char = gridPointCode.Chars(i)
                Result += Convert.ToUInt64(CHARACTERS.IndexOf(character))
            Next i
            Return Result
        End Function

        'Get a Coordinates from Point
        Private Function GetCoordinates(ByVal point As ULong) As Coordinates
            Dim Combination As Integer = CInt(Math.Truncate(Math.Truncate((point / Math.Pow(10, 10)))))
            Dim Fractional As ULong = CULng(Math.Truncate(point - (Combination * Math.Pow(10, 10))))

            Return GetCoordinates(GetCoordinateSeven(Combination, Fractional))
        End Function

        'Combine Seven Parts to Coordinate
        Private Function GetCoordinates(ByVal CSeven As CoordinateSeven) As Coordinates
            Dim Power As Integer = 0
            Dim TempLatitude As Integer = 0
            Dim TempLongitude As Integer = 0
            For x As Integer = 6 To 1 Step -1
                TempLatitude += CInt(Math.Truncate(CSeven.LatitudeSeven(x) * Math.Pow(10, Power)))
                TempLongitude += CInt(Math.Truncate(CSeven.LongitudeSeven(x) * Math.Pow(10, Power)))
                Power += 1
            Next x

            Dim Latitude As Double = (TempLatitude / Math.Pow(10, 5)) * CSeven.LatitudeSeven(0)
            Dim Longitude As Double = (TempLongitude / Math.Pow(10, 5)) * CSeven.LongitudeSeven(0)

            Return New Coordinates(Latitude, Longitude)
        End Function

        'Get Seven Parts of Coordinate
        Private Function GetCoordinateSeven(ByVal combination As Integer, ByVal fractional As ULong) As CoordinateSeven
            Dim LongitudeSeven(6) As Integer
            Dim LatitudeSeven(6) As Integer

            Dim SignWhole As CoordinateSignWhole = GetWholesFromCombination(combination)
            LongitudeSeven(0) = SignWhole.LongitudeSign
            LongitudeSeven(1) = SignWhole.LongitudeWhole
            LatitudeSeven(0) = SignWhole.LatitudeSign
            LatitudeSeven(1) = SignWhole.LatitudeWhole

            Dim Power As Integer = 9
            For x As Integer = 2 To 6
                LongitudeSeven(x) = CInt((CULng(Math.Truncate(Math.Truncate(fractional / Math.Pow(10, Power))))) Mod 10)
                Power -= 1
                LatitudeSeven(x) = CInt((CULng(Math.Truncate(Math.Truncate(fractional / Math.Pow(10, Power))))) Mod 10)
                Power -= 1
            Next x

            Return New CoordinateSeven(LatitudeSeven, LongitudeSeven)
        End Function

        'Get Whole-Numbers from Combination number
        Private Function GetWholesFromCombination(ByVal combination As Integer) As CoordinateSignWhole
            Dim MaxSum As Integer = 538

            Dim XSum As Integer = 0
            Dim XCombination As Integer = 0

            Dim Sum As Integer = 0
            Dim MaxCombination As Integer = 0
            Dim AssignedLongitude As Integer = 0
            Dim AssignedLatitude As Integer = 0

            If combination <= 16290 Then
                XSum = 0
                Do While MaxCombination < combination
                    MaxCombination += XSum + 1
                    XSum += 1
                Loop
                Sum = XSum - 1
                XCombination = MaxCombination - (Sum + 1)

                AssignedLongitude = combination - XCombination - 1
                AssignedLatitude = Sum - AssignedLongitude

            ElseIf combination <= 48690 Then
                XCombination = 16290
                XSum = 179

                Dim IsLast As Boolean = (combination - XCombination) Mod 180 = 0
                Dim Pre As Integer = ((combination - XCombination) \ 180) - (If(IsLast, 1, 0))

                XSum += Pre
                XCombination += Pre * 180

                Sum = XSum + 1
                AssignedLatitude = 180 - (combination - XCombination)
                AssignedLongitude = Sum - AssignedLatitude

            ElseIf combination <= 64800 Then
                XCombination = 48690
                XSum = 359
                MaxCombination += XCombination
                XSum = 360
                Do While MaxCombination < combination
                    MaxCombination += (MaxSum - XSum + 1)
                    XSum += 1
                Loop
                Sum = XSum - 1
                XCombination = MaxCombination - (MaxSum - Sum + 1)

                AssignedLatitude = 180 - (combination - XCombination)
                AssignedLongitude = Sum - AssignedLatitude
            End If

            '
            Dim Result As New CoordinateSignWhole()
            Result.LongitudeSign = (If(AssignedLongitude Mod 2 <> 0, -1, 1))
            AssignedLongitude -= (If(Result.LongitudeSign = -1, 1, 0))
            Result.LongitudeWhole = AssignedLongitude \ 2
            Result.LatitudeSign = (If(AssignedLatitude Mod 2 <> 0, -1, 1))
            AssignedLatitude -= (If(Result.LatitudeSign = -1, 1, 0))
            Result.LatitudeWhole = AssignedLatitude \ 2
            Return Result
        End Function

    End Module
End Namespace
