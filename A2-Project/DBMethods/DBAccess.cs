using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace A2_Project.DBMethods
{
	public static class DBAccess
	{
		// TODO: https://stackoverflow.com/questions/14376473/what-are-good-ways-to-prevent-sql-injection
		// Because https://xkcd.com/327/

		public static Database Db { get; set; }

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
		private static void StandardSetup(string command)
		{
			Db.Cmd = Db.Conn.CreateCommand();
			Db.Cmd.CommandText = command;
			Db.Rdr = Db.Cmd.ExecuteReader();
		}

		public static void ExecuteNonQuery(string command)
		{
			Db.Cmd = Db.Conn.CreateCommand();
			Db.Cmd.CommandText = command;
			Db.Cmd.ExecuteNonQuery();
		}

		/// <summary>
		/// Returns a list of strings from the given query
		/// </summary>
		public static List<string> GetStringsWithQuery(string query)
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
		public static List<List<string>> GetListStringsWithQuery(string query)
		{
			StandardSetup(query);
			List<List<string>> results = new List<List<string>>();
			while (Db.Rdr.Read())
				results.Add(GetStringsFromReader(Db.Rdr));
			Db.Rdr.Close();
			return results;
		}

		#region Set Requests
		/// <summary>
		/// Updates the specified table with the given data.
		/// Note: Does not currently manage creating new entries
		/// </summary>
		public static void UpdateTable(string table, List<List<string>> data)
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
		#endregion Set Requests
	}
}
