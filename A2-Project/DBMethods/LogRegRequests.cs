using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace A2_Project.DBMethods
{
	public static class LogRegRequests
	{
		public static bool DoesUse2FA(string username, ref string email)
		{
			bool uses2FA = DBAccess.GetStringsWithQuery("SELECT [Staff Uses 2FA] FROM [Staff] WHERE [Staff].[Staff Name] = '" + username + "';")[0] == "True";
			if (uses2FA) email = DBAccess.GetStringsWithQuery("SELECT [Staff Email] FROM [Staff] WHERE [Staff].[Staff Name] = '" + username + "';")[0];
			return uses2FA;
		}

		public static bool IsNameTaken(string name)
		{
			// TODO: StaffName should be enforced as unique
			return DBAccess.GetStringsWithQuery("SELECT COUNT([Staff ID]) FROM [Staff] WHERE [Staff].[Staff Name] = '" + name + "';")[0] == "0";
		}

		public static void CreateStaffAccount(string staffName, string staffPassword, string staffEmail, string staffPhoneNo, bool uses2FA)
		{
			string str;
			if (uses2FA) str = "1";
			else str = "0";
			DBAccess.ExecuteNonQuery($"INSERT INTO [Staff] VALUES ((SELECT Max([Staff ID]) FROM [Staff]) + 1, '{staffName}', '{staffPassword}', '{staffEmail}', '{staffPhoneNo}', {str});");
		}

		public static bool IsLoginDataCorrect(string name, string password)
		{
			// TODO: Password should be called StaffPassword, same with email
			password = ComputeHash(ComputeHash(password) + name);
			List<List<string>> results = DBAccess.GetListStringsWithQuery("SELECT * FROM [Staff] WHERE [Staff].[Staff Name] = '" + name + "' AND [Staff].[Staff Password] = '" + password + "';");
			return results.Count == 1;
		}

		/// <summary>
		/// Computes and returns the SHA256 hash of the input
		/// </summary>
		private static string ComputeHash(string password)
		{
			// TODO: This method should probably be moved somewhere else
			byte[] bytes = Array.Empty<byte>();
			using (SHA256 sha256Hash = SHA256.Create())
			{
				bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
			}
			string hashedPassword = "";
			for (int i = 0; i < bytes.Length; i++)
				hashedPassword += bytes[i].ToString("x2");
			return hashedPassword;
		}
	}
}
