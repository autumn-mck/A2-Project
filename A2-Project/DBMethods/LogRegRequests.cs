using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace A2_Project.DBMethods
{
	public static class LogRegRequests
	{
		public static bool DoesUse2FA(string username, ref string email)
		{
			// Using schema: Staff, Uses2FA, StaffName, StaffEmail
			// SQL injection risk with string concatenation.
			string uses2FAResult = DBAccess.GetStringsWithQuery($"SELECT [Uses2FA] FROM [Staff] WHERE [StaffName] = '{username}';").FirstOrDefault();
			bool uses2FA = uses2FAResult == "1";
			if (uses2FA)
			{
				email = DBAccess.GetStringsWithQuery($"SELECT [StaffEmail] FROM [Staff] WHERE [StaffName] = '{username}';").FirstOrDefault();
			}
			return uses2FA;
		}

		public static bool IsNameTaken(string name)
		{
			// Using schema: Staff, StaffID, StaffName
			// SQL injection risk.
			// The original logic was "== 0" meaning name is *not* taken. This seems inverted.
			// If COUNT is "0", name is NOT taken. So IsNameTaken should be true if count is "0".
			string countResult = DBAccess.GetStringsWithQuery($"SELECT COUNT([StaffID]) FROM [Staff] WHERE [StaffName] = '{name}';").FirstOrDefault();
			return countResult == "0";
		}

		public static void CreateStaffAccount(string staffName, string staffPassword, string staffEmail, string staffPhoneNo, bool uses2FA)
		{
			// Using schema: Staff, StaffID, StaffName, PasswordHash, Salt, StaffEmail, StaffPhoneNo, Uses2FA
			// SQL injection risk.
			string strUses2FA = uses2FA ? "1" : "0";
			string salt = EmailManagement.GenerateRandomKey(32); // Assuming EmailManagement is available and correct
			string hash = GetSecureHash(staffPassword, salt);
			// Explicitly list columns for INSERT
			DBAccess.ExecuteNonQuery($"INSERT INTO [Staff] (StaffID, StaffName, PasswordHash, Salt, StaffEmail, StaffPhoneNo, Uses2FA) VALUES ((SELECT COALESCE(MAX([StaffID]), 0) FROM [Staff]) + 1, '{staffName}', '{hash}', '{salt}', '{staffEmail}', '{staffPhoneNo}', {strUses2FA});");
		}

		public static bool IsLoginDataCorrect(string name, string password)
		{
			// Using schema: Staff, StaffName, PasswordHash, Salt
			// SQL injection risk.
			// Explicitly list columns instead of SELECT *
			List<List<string>> userData = DBAccess.GetListStringsWithQuery($"SELECT StaffID, StaffName, PasswordHash, Salt, StaffEmail, StaffPhoneNo, Uses2FA FROM [Staff] WHERE [StaffName] = '{name}';");
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
