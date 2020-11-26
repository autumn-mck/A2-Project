using A2_Project.SQLObjects;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2_Project
{
	public class DBAccess
	{
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
				if (obj is DateTime time) results.Add(time.ToString("dd/MM/yyyy"));
				else results.Add(obj.ToString());
			}
			return results;
		}

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

		private string GenerateText(string table, List<string> data)
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
		/// Returns the names of all tables
		/// </summary>
		public List<string> GetAllTables()
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
		public List<string> GetColumnHeadersFromTable(string tableName)
		{
			return GetStringsWithQuery("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "' ORDER BY ORDINAL_POSITION;");
		}

		/// <summary>
		/// Gets the data-types of all columns in the specified table
		/// </summary>
		public List<string> GetColumnsType(string tableName)
		{
			return GetStringsWithQuery("SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "';");
		}

		public List<string> GetIsPrimaryKey(string tableName)
		{
			// Method from a previous project that sort of gets the primary keys
			return GetStringsWithQuery("SELECT C.CONSTRAINT_TYPE FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS K ON C.TABLE_NAME = K.TABLE_NAME AND C.CONSTRAINT_CATALOG = K.CONSTRAINT_CATALOG AND C.CONSTRAINT_SCHEMA = K.CONSTRAINT_SCHEMA AND C.CONSTRAINT_NAME = K.CONSTRAINT_NAME WHERE C.TABLE_NAME = '" + tableName + "';");
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
	}
}
