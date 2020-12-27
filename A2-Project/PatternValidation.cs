using System;
using System.Text.RegularExpressions;

namespace A2_Project
{
	public static class PatternValidation
	{
		public static bool IsValidPhoneNo(string phoneNo)
		{
			string regexStr = @"^(?:(?:\(?(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?(?:\(?0\)?[\s-]?)?)|(?:\(?0))(?:(?:\d{5}\)?[\s-]?\d{4,5})|(?:\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3}))|(?:\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4})|(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}))(?:[\s-]?(?:x|ext\.?|\#)\d{3,4})?$";
			Regex regex = new Regex(regexStr);
			return regex.IsMatch(phoneNo);
		}

		public static bool IsValidEmail(string email)
		{
			// https://www.youtube.com/watch?v=xxX81WmXjPg
			string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
			Regex regex = new Regex(pattern);
			return regex.IsMatch(email);
		}

		public static bool IsValidPostcode(string postCode)
		{
			// https://en.wikipedia.org/wiki/Postcodes_in_the_United_Kingdom#Validation
			string pattern2 = @"^(([A-Z]{1,2}[0-9][A-Z0-9]?|ASCN|STHL|TDCU|BBND|[BFS]IQQ|PCRN|TKCA) ?[0-9][A-Z]{2}|BFPO ?[0-9]{1,4}|(KY[0-9]|MSR|VG|AI)[ -]?[0-9]{4}|[A-Z]{2} ?[0-9]{2}|GE ?CX|GIR ?0A{2}|SAN ?TA1)$";
			Regex regex2 = new Regex(pattern2);
			return regex2.IsMatch(postCode);
		}

		public static bool IsValidDogGender(string toTest)
		{
			return toTest == "M" || toTest == "F";
		}

		public static bool IsValidDate(string toTest)
		{
			bool isValid = DateTime.TryParse(toTest, out DateTime d);
			return isValid && d.TimeOfDay.TotalSeconds == 0;
		}

		public static bool IsBit(string bit)
		{
			return bit is "True" or "False" or "1" or "0";
		}
	}
}
