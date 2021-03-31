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
			return DBAccess.GetStringsWithQuery("SELECT COUNT([Staff ID]) FROM [Staff] WHERE [Staff].[Staff Name] = '" + name + "';")[0] == "0";
		}

		public static void CreateStaffAccount(string staffName, string staffPassword, string staffEmail, string staffPhoneNo, bool uses2FA)
		{
			string strUses2FA;
			if (uses2FA) strUses2FA = "1";
			else strUses2FA = "0";
			string salt = EmailManagement.GenerateRandomKey(32);
			string hash = GetSecureHash(staffPassword, salt);
			DBAccess.ExecuteNonQuery($"INSERT INTO [Staff] VALUES ((SELECT Max([Staff ID]) FROM [Staff]) + 1, '{staffName}', '{hash}', '{salt}', '{staffEmail}', '{staffPhoneNo}', {strUses2FA});");
		}

		public static bool IsLoginDataCorrect(string name, string password)
		{
			List<List<string>> userData = DBAccess.GetListStringsWithQuery($"SELECT * FROM [Staff] WHERE [Staff].[Staff Name] = '{name}';");
			if (userData.Count == 0) return false;
			else return GetSecureHash(password, userData[0][3]) == userData[0][2];
		}

		private static string GetSecureHash(string password, string salt)
		{
			// Password, salt and pepper
			string hash = ComputeHash(password + salt + "gTcZB660KJZLZTPNI4VJWG0pX0OqVpNK");

			for (int i = 0; i < 10; i++) hash = ComputeHash(hash);

			return hash;
		}

		/// <summary>
		/// Computes and returns the SHA256 hash of the input
		/// </summary>
		private static string ComputeHash(string toHash)
		{
			byte[] bytes = Array.Empty<byte>();
			using (SHA512 sha256Hash = SHA512.Create())
			{
				bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(toHash));
			}
			string hashedPassword = "";
			for (int i = 0; i < bytes.Length; i++)
				hashedPassword += bytes[i].ToString("x2");
			return hashedPassword;
		}
	}
}
