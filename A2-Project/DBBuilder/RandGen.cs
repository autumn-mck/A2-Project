using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace A2_Project.DBBuilder
{
	static class RandGen
	{
		private static readonly string dataLocation = Directory.GetCurrentDirectory() + "\\csvs\\";

		private static Random random = new Random(0);

		private static List<Surname> surnames = new List<Surname>();
		private static List<string> maleForenames = new List<string>();
		private static List<string> femaleForenames = new List<string>();
		private static List<string> streetNames = new List<string>();
		private static List<string> townNames = new List<string>();
		private static List<string> countyNames = new List<string>();

		private static List<string> femaleDogNames = new List<string>();
		private static List<string> maleDogNames = new List<string>();
		private static List<string> dogBreeds = new List<string>();

		#region Initialising
		public static void InitialiseDataFromFile()
		{
			GetForenamesFromFile();
			GetSurnamesFromFile();
			GetStreetNamesFromFile();
			GetTownNamesFromFile();
			GetDogInfoFromFile();
		}

		public static void Reset(int seed)
		{
			random = new Random(seed);
		}

		private static void GetTownNamesFromFile()
		{
			FileStream s = new FileStream(dataLocation + "places.csv", FileMode.Open);
			StreamReader sr = new StreamReader(s);
			string line;
			string currentCounty = "";
			while ((line = sr.ReadLine()) != null)
			{
				if (line[0] != '-') currentCounty = line;
				else
				{
					townNames.Add(line[1..]);
					countyNames.Add(currentCounty);
				}
			}
			sr.Close();
		}

		private static void GetFromFile(string file, ref List<string> toAdd)
		{
			FileStream s = new FileStream(dataLocation + file, FileMode.Open);
			StreamReader sr = new StreamReader(s);
			string line;
			while ((line = sr.ReadLine()) != null)
				toAdd.Add(line);
			sr.Close();
		}

		private static void GetStreetNamesFromFile()
		{
			GetFromFile("roadnames.csv", ref streetNames);
		}

		private static void GetForenamesFromFile()
		{
			GetFromFile("girlsnames.csv", ref femaleForenames);
			GetFromFile("boysnames.csv", ref maleForenames);
		}

		private static void GetSurnamesFromFile()
		{
			FileStream s = new FileStream(dataLocation + "surnames.csv", FileMode.Open);
			StreamReader sr = new StreamReader(s);
			string line;
			while ((line = sr.ReadLine()) != null)
				surnames.Add(new Surname(line));
			sr.Close();
		}

		private static void GetDogInfoFromFile()
		{
			GetFromFile("femaledognames.csv", ref femaleDogNames);
			GetFromFile("maledognames.csv", ref maleDogNames);
			GetFromFile("dogbreeds.csv", ref dogBreeds);
		}

		#endregion Initialising

		#region Name
		public static string GetRandomName()
		{
			return GetRandomForename() + " " + GetRandomSurname();
		}

		public static string GetRandomForename()
		{
			if (random.Next(0, 2) == 1)
				return maleForenames[random.Next(maleForenames.Count)];
			else return femaleForenames[random.Next(femaleForenames.Count)];
		}

		public static string GetRandomSurname()
		{
			float result = (float)random.NextDouble();

			foreach (Surname surname in surnames)
				if (result > surname.ProbabilityReq) return surname.SurnameStr;
			return "Something's gone wrong!";
		}
		#endregion Name

		public static string GenEmail(string forename, string surname)
		{
			string email = "";
			double randDouble = random.NextDouble();
			if (randDouble < 0.4) email += forename[0];
			else if (randDouble < 0.77) email += forename.Substring(0, 3);
			else email += forename;

			//randDouble = random.NextDouble();
			if (randDouble < 0.4) email += surname;
			else if (randDouble < 0.7) email += surname.Substring(0, 3);
			else email += surname[0];

			int numsToAdd = random.Next(3);
			for (int i = 0; i < numsToAdd; i++)
				email += random.Next(10);

			email += "@";
			int emailProvider = random.Next(6);
			switch (emailProvider)
			{
				case 0: email += "mail.abc"; break;
				case 1: email += "neutron.abc"; break;
				case 2: email += "ymail.abc"; break;
				case 3: email += "mycloud.abc"; break;
				case 4: email += "email.abc"; break;
				case 5: email += "coolmail.abc"; break;
			}

			return email;
		}

		public static string GenAddress()
		{
			return random.Next(1, 40) + " " + streetNames[random.Next(streetNames.Count)];
		}

		public static string GenTown()
		{
			return townNames[random.Next(townNames.Count)];
		}

		public static string GenCounty()
		{
			return countyNames[random.Next(countyNames.Count)];
		}

		public static string GenPostcode()
		{
			string pattern = @"^(([A-Z]{1,2}\d[A-Z\d]?|ASCN|STHL|TDCU|BBND|[BFS]IQQ|PCRN|TKCA) ?\d[A-Z]{2}|BFPO ?\d{1,4}|(KY\d|MSR|VG|AI)[ -]?\d{4}|[A-Z]{2} ?\d{2}|GE ?CX|GIR ?0A{2}|SAN ?TA1)$";
			pattern = @"(GIR 0AA)|((([A-Z-[QVX]][0-9][0-9]?)|(([A-Z-[QVX]][A-Z-[IJZ]][0-9][0-9]?)|(([A-Z-[QVX]][0-9][A-HJKSTUW])|([A-Z-[QVX]][A-Z-[IJZ]][0-9][ABEHMNPRVWXY])))) [0-9][A-Z-[CIKMOV]]{2})";
			Regex rg = new Regex(pattern);
			string toConsider;
			do
			{
				toConsider = "BT" + random.Next(1, 95) + " " + random.Next(1, 10) + GenRandLetter() + GenRandLetter();
			}
			while (!rg.IsMatch(toConsider));
			return toConsider;
		}

		private static char GenRandLetter()
		{
			return (char)('A' + random.Next(0, 26));
		}

		public static bool ShouldBeReturnClient()
		{
			return random.NextDouble() < 0.9;
		}

		/// <summary>
		/// Generates a phone number within the Ofcom reserved numbers.
		/// See https://www.ofcom.org.uk/phones-telecoms-and-internet/information-for-industry/numbering/numbers-for-drama
		/// </summary>
		public static string GenPhoneNo()
		{
			string phoneNo;
			if (random.NextDouble() > 0.5) phoneNo = "028 9649 6";
			else phoneNo = "07700 900";
			for (int i = 0; i < 3; i++) phoneNo += random.Next(0, 10);

			return phoneNo;
		}

		#region Dog
		public static int GenNumDogs()
		{
			double r = random.NextDouble();
			if (r > 0.4) return 1;
			else if (r > 0.1) return 2;
			else if (r > 0.03) return 3;
			else return 4;
		}

		public static DateTime GenDogAge(DateTime date)
		{
			double resultRand = random.NextDouble();
			double age = Math.Log(resultRand / (1 - resultRand)) + 2;
			age = Math.Max(1 / 12, age);

			return date.AddYears(-(int)age);
		}

		public static string GenDogName(string gender)
		{
			if (gender == "M") return maleDogNames[random.Next(maleDogNames.Count)];
			else return femaleDogNames[random.Next(femaleDogNames.Count)];
		}

		public static string GenDogType()
		{
			return dogBreeds[random.Next(dogBreeds.Count)];
		}
		#endregion Dog

		/// <summary>
		/// Gets a random appointment type ID.
		/// 0 - 50%,
		/// 1 - 30%,
		/// 2 - 20%
		/// </summary>
		public static int GetRandAppTypeID()
		{
			double rand = random.NextDouble();
			if (rand < 0.5) return 0;
			else if (rand < 0.8) return 1;
			else return 2;
		}

		public static int GetRandStaffID()
		{
			if (random.NextDouble() > 0.5)
				return random.Next(5);
			else return random.Next(4);
		}

		public static string GetRandPaymentMethod()
		{
			int randInt = random.Next(0, 3);
			if (randInt == 0) return "Standing Order";
			else if (randInt == 1) return "Cash";
			else return "Card";
		}
	}
}
