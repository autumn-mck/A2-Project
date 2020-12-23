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
		private static List<string> GetStringsFromReader(SqlDataReader reader, string[] headers)
		{
			List<string> results = new List<string>();
			for (int i = 0; i < reader.FieldCount; i++)
			{
				object obj = reader.GetValue(i);
				if (headers != null)
				{
					// Ensures dates are formatted correctly
					if (obj is DateTime time && headers[i].Contains("DOB")) obj = time.ToString("dd/MM/yyyy");
				}
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
		public static List<string> GetStringsWithQuery(string query, string[] headers = null)
		{
			StandardSetup(query);
			List<string> results = new List<string>();
			while (Db.Rdr.Read())
				results.Add(GetStringsFromReader(Db.Rdr, headers)[0]);
			Db.Rdr.Close();
			return results;
		}

		/// <summary>
		/// Returns a list of list of strings from the given query
		/// </summary>
		public static List<List<string>> GetListStringsWithQuery(string query, string[] headers = null)
		{
			StandardSetup(query);
			List<List<string>> results = new List<List<string>>();
			while (Db.Rdr.Read())
				results.Add(GetStringsFromReader(Db.Rdr, headers));
			Db.Rdr.Close();
			return results;
		}

		public static void UpdateTable(string table, string[] headers, string[] newData)
		{
			string command = $"SET DATEFORMAT dmy; UPDATE {table} SET {headers[1]} = '{newData[1]}'";
			for (int i = 2; i < headers.Length; i++)
			{
				command += $", {headers[i]} = '{newData[i]}'";
			}
			command += $" WHERE {headers[0]} = '{newData[0]}';";
			ExecuteNonQuery(command);
		}
	}
}
