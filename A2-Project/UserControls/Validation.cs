using System;

namespace A2_Project.UserControls
{
	public static class Validation
	{
		public static bool Validate(string str, DBObjects.Column col, out string errorMessage)
		{
			bool isValid;
			bool patternReq = true;
			bool typeReq = true;
			bool fKeyReq = true;
			bool pKeyReq = true;

			string patternError = "";
			// Check if the string meets a specific pattern
			string name = col.Name.ToLower();
			if (name.Contains("email"))
			{
				patternReq = PatternValidation.IsValidEmail(str);
				patternError = "Please enter a valid email address. ";
			}
			else if (name.Contains("postcode"))
			{
				patternReq = PatternValidation.IsValidPostcode(str);
				patternError = "Please enter a valid postcode. ";
			}
			else if (name.Contains("phone"))
			{
				patternReq = PatternValidation.IsValidPhoneNo(str);
				patternError = "Please enter a valid phone number. ";
			}
			else if (name.Contains("dog gender"))
			{
				patternReq = PatternValidation.IsValidDogGender(str);
				patternError = "Please enter a valid dog gender. (M/F) ";
			}

			if (col.Constraints.CanBeNull && str == "")
			{
				isValid = true;
				errorMessage = "";
				return isValid;
			}
			else if (!col.Constraints.CanBeNull && str == "")
			{
				patternReq = false;
				patternError = "This value cannot be left empty! ";
			}

			// Checks if the data meets the requirements for the type of data it should be
			switch (col.Constraints.Type)
			{
				case "int":
					typeReq = !string.IsNullOrEmpty(str) && int.TryParse(str, out _);
					break;
				case "date":
					typeReq = DateTime.TryParse(str, out DateTime d);
					typeReq = typeReq && d.TimeOfDay.TotalSeconds == 0;
					break;
				case "time":
					typeReq = TimeSpan.TryParse(str, out TimeSpan t);
					typeReq = typeReq && t.TotalMinutes % 1.0 == 0;
					break;
				case "varchar":
					typeReq = str.Length <= col.Constraints.MaxSize;
					break;
			}

			// Checks if the data meets foreign key requirements if needed
			if (col.Constraints.ForeignKey != null && typeReq)
				fKeyReq = DBMethods.MiscRequests.DoesMeetForeignKeyReq(col.Constraints.ForeignKey, str);

			// Checks if the data meets primary key requirements if needed
			if (col.Constraints.IsPrimaryKey && typeReq)
				pKeyReq = DBMethods.MiscRequests.IsPKeyFree(col.TableName, col.Name, str);
			// Note: No good way to validate names/addresses

			isValid = patternReq && typeReq && fKeyReq && pKeyReq;

			// If the current part is invalid, let the user know what the issue is.
			string instErr = "";
			if (!isValid)
			{
				instErr = $"\n{col.Name}: ";
				if (!patternReq) instErr += patternError;

				if (!typeReq)
				{
					switch (col.Constraints.Type)
					{
						case "int": instErr += "Please enter a number! "; break;
						case "bit": instErr += "Please enter True/False/1/0! "; break;
						case "date": instErr += "Please enter a valid date!" ; break;
						case "time": instErr += "Please enter a valid time! (hh:mm) "; break;
						case "varchar": instErr += $"Input too long, it must be less than {col.Constraints.MaxSize} characters! "; break;
					}
				}

				if (!fKeyReq) instErr += $"References a non-existent {col.Constraints.ForeignKey.ReferencedTable}. ";
				if (!pKeyReq) instErr += "This ID is already taken! ";
			}
			errorMessage = instErr;
			return isValid;
		}
	}
}
