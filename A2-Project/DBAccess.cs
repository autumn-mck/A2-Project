using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace A2_Project
{
	public class DBAccess
	{
		// TODO: https://stackoverflow.com/questions/14376473/what-are-good-ways-to-prevent-sql-injection
		// Because https://xkcd.com/327/
		// TODO: Can/Should this class be static?
		
		private static string[] months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

		public DBAccess(Database db)
		{
			Db = db;
		}

		public Database Db { get; set; }

		/// <summary>
		/// Returns the contents of SqlDataReader as a list of strings.
		/// </summary>
		public static List<string> GetStringsFromReader(SqlDataReader reader)
		{
			List<string> results = new List<string>();
			for (int i = 0; i < reader.FieldCount; i++)
			{
				object obj = reader.GetValue(i);
				// Ensures dates are formatted correctly
				/*if (obj is DateTime time) results.Add(time.ToString("dd/MM/yyyy"));
				else*/
				results.Add(obj.ToString());
			}
			return results;
		}


		/// <summary>
		/// The standard set-up for most of my SQL commands
		/// </summary>
		private void StandardSetup(string command)
		{
			Db.Cmd = Db.Conn.CreateCommand();
			Db.Cmd.CommandText = command;
			Db.Rdr = Db.Cmd.ExecuteReader();
		}

		/// <summary>
		/// Returns a list of strings from the given query
		/// </summary>
		private List<string> GetStringsWithQuery(string query)
		{
			StandardSetup(query);
			List<string> results = new List<string>();
			while (Db.Rdr.Read())
				results.Add(GetStringsFromReader(Db.Rdr)[0]);
			Db.Rdr.Close();
			return results;
		}

		/// <summary>
		/// Returns a list of list of strings from the given query
		/// </summary>
		private List<List<string>> GetListStringsWithQuery(string query)
		{
			StandardSetup(query);
			List<List<string>> results = new List<List<string>>();
			while (Db.Rdr.Read())
				results.Add(GetStringsFromReader(Db.Rdr));
			Db.Rdr.Close();
			return results;
		}

		#region Get Requests
		/// <summary>
		/// Returns the names of all tables
		/// </summary>
		public List<string> GetTableNames()
		{
			return GetStringsWithQuery("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';");
		}

		/// <summary>
		/// Gets all data from the specified table
		/// </summary>
		public List<List<string>> GetAllFromTable(string tableName)
		{
			return GetListStringsWithQuery("SELECT * FROM [" + tableName + "];");
		}

		/// <summary>
		/// Gets the column headers of the specified table
		/// </summary>
		public List<string> GetHeadersFromTable(string tableName)
		{
			return GetStringsWithQuery("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "';");
		}

		/// <summary>
		/// Gets the data-types of all columns in the specified table
		/// </summary>
		public List<string> GetColumnsType(string tableName)
		{
			return GetStringsWithQuery("SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "';");
		}

		// TODO: AppointmentTime should be called DateTime
		public List<List<string>> GetAllAppointmentsOnDay(DateTime day)
		{
			return GetListStringsWithQuery("SELECT * FROM Appointment WHERE CONVERT(DATE, AppointmentDateTime) = '" + day.ToString("yyyy-MM-dd") + "';");
		}

		// TODO: StaffName is not currently guaranteed to be unique
		public string DoesUse2FA(string username)
		{
			return GetStringsWithQuery("SELECT [StaffEmail] FROM [Staff] WHERE [Staff].StaffName = '" + username + "';")[0];
		}

		public List<string> GetIsPrimaryKey(string tableName)
		{
			// Method from a previous project that sort of? gets the primary keys
			return GetStringsWithQuery("SELECT C.CONSTRAINT_TYPE FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS K ON C.TABLE_NAME = K.TABLE_NAME AND C.CONSTRAINT_CATALOG = K.CONSTRAINT_CATALOG AND C.CONSTRAINT_SCHEMA = K.CONSTRAINT_SCHEMA AND C.CONSTRAINT_NAME = K.CONSTRAINT_NAME WHERE C.TABLE_NAME = '" + tableName + "';");
		}

		public bool IsNameTaken(string name)
		{
			// TODO: StaffName should be enforced as unique
			return GetStringsWithQuery("SELECT COUNT(StaffID) FROM [Staff] WHERE [Staff].StaffName = '" + name + "';")[0] == "0";
		}

		public List<List<string>> GetContactsByClientID(string clientID)
		{
			return GetListStringsWithQuery("SELECT * FROM [Contact] WHERE ClientID = " + clientID + ";");
		}

		public bool IsColumnPrimaryKey(string columnName, string tableName)
		{
			string query = "SELECT K.TABLE_NAME, K.COLUMN_NAME, K.CONSTRAINT_NAME " +
			"FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS K " +
			"ON C.TABLE_NAME = K.TABLE_NAME AND C.CONSTRAINT_CATALOG = K.CONSTRAINT_CATALOG AND C.CONSTRAINT_SCHEMA = K.CONSTRAINT_SCHEMA " +
			$"AND C.CONSTRAINT_NAME = K.CONSTRAINT_NAME WHERE C.CONSTRAINT_TYPE = 'PRIMARY KEY' AND K.COLUMN_NAME = '{columnName}' AND K.TABLE_NAME = '{tableName}';";
			return (GetListStringsWithQuery(query).Count > 0);
		}

		#region Graphs
		public void GetCountOfAppointmentTypes(ref int[][] data, ref string[] headers)
		{
			data[0] = GetStringsWithQuery("SELECT Count(AppointmentTypeID) FROM [Appointment] GROUP BY AppointmentTypeID;").Select(int.Parse).ToArray();
			headers = GetStringsWithQuery("SELECT Description FROM [AppointmentType] ORDER BY AppointmentTypeID;").ToArray();
		}

		public void GetBusinessOfStaff(ref int[][] data, ref string[] headers)
		{
			data[0] = GetStringsWithQuery("SELECT Count(StaffID) FROM [Appointment] GROUP BY StaffID ORDER BY StaffID;").Select(int.Parse).ToArray();
			headers = GetStringsWithQuery("SELECT StaffName FROM [Staff] ORDER BY StaffID;").ToArray();
		}

		public void GetGrowthOverTime(ref int[][] data, ref string[] headers)
		{
			DateTime startDate = Convert.ToDateTime(GetStringsWithQuery("SELECT MIN(ClientJoinDate) FROM Client")[0]);
			DateTime endDate = Convert.ToDateTime(GetStringsWithQuery("SELECT MAX(ClientJoinDate) FROM Client")[0]);
			int diff = (int)(endDate - startDate).TotalDays;
			List<int> growth = new List<int>();
			for (int i = 0; i < diff; i += 50)
			{
				growth.Add(Convert.ToInt32(GetStringsWithQuery("SELECT COUNT(ClientID) FROM [Client] WHERE ClientJoinDate <= '" + startDate.AddDays(i).ToString("yyyy-MM-dd") + "';")[0]));
			}
			data[0] = growth.ToArray();
			headers = InterpolateDates(startDate, diff);
		}

		public void GetAppsByDayOfWeek(ref int[][] data, ref string[] headers)
		{
			data[0] = new int[7];
			for (int i = 0; i < data[0].Length; i++)
				data[0][i] = Convert.ToInt32(GetStringsWithQuery("SELECT COUNT(AppointmentID) FROM [Appointment] WHERE DATEPART(WEEKDAY, AppointmentDateTime) =" + (i + 1) + ";")[0]);
			headers = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
		}

		public void GetBookingsInMonths(ref int[][] data, ref string[] headers)
		{
			data[0] = new int[12];
			for (int i = 0; i < data[0].Length; i++)
			{
				data[0][i] = Convert.ToInt32(GetStringsWithQuery("SELECT COUNT(AppointmentID) FROM [Appointment] WHERE DATEPART(MONTH, AppointmentDateTime) =" + (i + 1) + ";")[0]);
			}
			headers = months;
		}

		public void GetAppCancelRate(ref int[][] data, ref string[] headers)
		{
			DateTime startDate = Convert.ToDateTime(GetStringsWithQuery("SELECT MIN(AppointmentDateTime) FROM Appointment")[0]);
			DateTime endDate = Convert.ToDateTime(GetStringsWithQuery("SELECT MAX(AppointmentDateTime) FROM Appointment")[0]);
			int diff = (int)(endDate - startDate).TotalDays;
			List<int> growth = new List<int>();
			for (int i = 0; i < diff; i += 25)
			{
				int totalInTime = Convert.ToInt32(GetStringsWithQuery("SELECT COUNT(AppointmentID) FROM [Appointment] WHERE AppointmentDateTime <= '" + startDate.AddDays(i).ToString("yyyy-MM-dd") + "' AND AppointmentDateTime > '" + startDate.AddDays(i - 100).ToString("yyyy-MM-dd") + "';")[0]);
				int cancelledInTime = Convert.ToInt32(GetStringsWithQuery("SELECT COUNT(AppointmentID) FROM [Appointment] WHERE IsCancelled = 1 AND AppointmentDateTime <= '" + startDate.AddDays(i).ToString("yyyy-MM-dd") + "' AND AppointmentDateTime > '" + startDate.AddDays(i - 100).ToString("yyyy-MM-dd") + "';")[0]);
				if (cancelledInTime == 0 || totalInTime == 0)
					growth.Add(0);
				else growth.Add((int)((float)cancelledInTime * 100 / totalInTime));
			}
			data[0] = growth.ToArray();
			headers = InterpolateDates(startDate, diff);
		}

		// TODO: Currently classifies clients that have > 1 dogs and booked them all in one go as repeat customers, even if they never book after that.
		public void GetCustReturns(ref int[][] data, ref string[] headers)
		{
			DateTime startDate = Convert.ToDateTime(GetStringsWithQuery("SELECT MIN(ClientJoinDate) FROM Client")[0]);
			DateTime endDate = Convert.ToDateTime(GetStringsWithQuery("SELECT MAX(AppointmentDateTime) FROM Appointment")[0]);
			int diff = (int)(endDate - startDate).TotalDays;
			List<int> growth = new List<int>();
			for (int i = 0; i < diff; i += 50)
			{
				string query = "SELECT COUNT([Appointment].AppointmentID) FROM [Client] " +
				"INNER JOIN [Dog] ON [Dog].ClientID = [Client].ClientID INNER JOIN [Appointment] ON [Appointment].DogID = [Dog].DogID " +
				"WHERE [Client].ClientJoinDate < '" + startDate.AddDays(i).ToString("yyyy-MM-dd") + "' AND [Appointment].IsInitialAppointment = 1 " +
				"AND [Client].ClientJoinDate > '" + startDate.AddDays(i - 50).ToString("yyyy-MM-dd") + "';";
				List<string> result = GetStringsWithQuery(query);
				growth.Add(Convert.ToInt32(result[0]));
			}
			data[0] = growth.ToArray();
			headers = InterpolateDates(startDate, diff);
		}

		public void GetDogTypesOverTime(ref int[][] counts, ref string[] headers)
		{
			DateTime startDate = Convert.ToDateTime(GetStringsWithQuery("SELECT MIN(ClientJoinDate) FROM Client")[0]);
			DateTime endDate = Convert.ToDateTime(GetStringsWithQuery("SELECT MAX(ClientJoinDate) FROM Client")[0]);
			int diff = (int)(endDate - startDate).TotalDays;
			List<int[]> growth = new List<int[]>();
			for (int i = 0; i < diff; i += 10)
			{
				growth.Add(GetStringsWithQuery("SELECT COUNT(DogID) FROM [Dog] INNER JOIN [Client] ON [Client].ClientID = [Dog].DogID WHERE [Client].ClientJoinDate <= '" + startDate.AddDays(i).ToString("yyyy-MM-dd") + "' GROUP BY DogType;").Select(int.Parse).ToArray());
			}
			counts = growth.ToArray();
			headers = InterpolateDates(startDate, diff);
		}

		// TODO: BookedInAdvancedDiscount should be its own table?
		public void GetIncomeLastYear(ref int[][] data, ref string[] headers)
		{
			headers = new string[12];
			DateTime endDate = DateTime.Now.Date;
			DateTime startDate = endDate.AddMonths(-12);
			List<List<List<string>>> growth = new List<List<List<string>>>();
			for (int i = 0; i < 12; i++)
			{
				headers[i] = months[startDate.AddMonths(i).Month - 1];
				string query = "SELECT AppointmentTypeID, IncludesNailAndTeeth, BookedInAdvanceDiscount FROM [Appointment] WHERE IsPaid = 1 AND AppointmentDateTime BETWEEN '" + startDate.AddMonths(i).ToString("yyyy-MM-dd") + "' AND '" + startDate.AddMonths(i + 1).ToString("yyyy-MM-dd") + "';";
				growth.Add(GetListStringsWithQuery(query));
			}
			List<List<string>> appTypeData = GetListStringsWithQuery("SELECT * FROM [AppointmentType]");
			List<int> income = new List<int>(); // TODO: Should probably be double, not int
			for (int i = 0; i < growth.Count; i++)
			{
				income.Add(0);
				List<List<string>> lls = growth[i];
				foreach (List<string> ls in lls)
				{
					int t = Convert.ToInt32(appTypeData[Convert.ToInt32(ls[0])][2]);
					if (ls[1] == "1") t += 5;
					//t *= 100 - Convert.ToInt32(ls[2]);
					income[i] += t - 38	;
				}
			}
			data[0] = income.ToArray();

		}

		#endregion Graphs

		private static string[] InterpolateDates(DateTime startDate, int diff)
		{
			List<string> dates = new List<string>();
			for (double i = 0; i <= diff; i += (double)diff / 6)
			{
				dates.Add(startDate.AddDays(i).ToString("dd/MM/yyyy"));
			}
			return dates.ToArray();
		}
		#endregion Get Requests

		#region Set Requests
		/// <summary>
		/// Updates the specified table with the given data.
		/// Note: Does not currently manage creating new entries
		/// </summary>
		public void UpdateTable(string table, List<List<string>> data)
		{
			foreach (List<string> strArr in data)
			{
				Db.Cmd = Db.Conn.CreateCommand();
				Db.Cmd.CommandText = GenerateText(table, strArr);
				Db.Cmd.ExecuteNonQuery();
			}
		}

		private static string GenerateText(string table, List<string> data)
		{
			// Code from a previous project that will be adapted to this one
			return table switch
			{
				"Customer" => "UPDATE Customer SET CustomerName = '" + data[1] + "' WHERE CustomerID = " + data[0] + ";",
				"Order" => "UPDATE [Order] SET OrderDate = '" + DateTime.Parse(data[1]).ToString("yyyy-MM-dd") + "', StaffID = " + data[2] + ", CustomerID = " + data[3] + " WHERE OrderID = " + data[0] + ";",
				"OrderProduct" => "UPDATE OrderProduct SET ProductID = " + data[1] + ", Quantity = " + data[2] + " WHERE OrderID = " + data[0] + ";",
				"Product" => "UPDATE Product SET ProductName = '" + data[1] + "', ProductPrice = " + data[2] + " WHERE ProductID = " + data[0] + ";",
				_ => "",
			};
		}

		public void CreateStaffAccount(string staffName, string staffPassword, string staffEmail, string staffPhoneNo, bool uses2FA)
		{
			string str;
			if (uses2FA) str = "1";
			else str = "0";
			Db.Cmd = Db.Conn.CreateCommand();
			Db.Cmd.CommandText = $"INSERT INTO [Staff] VALUES ((SELECT Max(StaffID) FROM [Staff]) + 1, '{staffName}', '{staffPassword}', '{staffEmail}', '{staffPhoneNo}', {str});";
			Db.Cmd.ExecuteNonQuery();
		}
		#endregion Set Requests

		#region Validation Requests
		public bool IsLoginDataCorrect(string name, string password)
		{
			// TODO: Password should be called StaffPassword, same with email
			password = ComputeHash(ComputeHash(password) + name);
			List<List<string>> results = GetListStringsWithQuery("SELECT * FROM [Staff] WHERE [Staff].StaffName = '" + name + "' AND [Staff].StaffPassword = '" + password + "';");
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

		#endregion Validation Requests

	}
}
