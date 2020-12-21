using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2_Project.DBMethods
{
	/// <summary>
	/// SQL Requests that return information about tables or columns themselves
	/// </summary>
	public static class MetaRequests
	{
		/// <summary>
		/// Returns the names of all tables
		/// </summary>
		public static List<string> GetTableNames()
		{
			return DBAccess.GetStringsWithQuery("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';");
		}

		/// <summary>
		/// Gets all data from the specified table
		/// </summary>
		public static List<List<string>> GetAllFromTable(string tableName)
		{
			return DBAccess.GetListStringsWithQuery("SELECT * FROM [" + tableName + "];");
		}

		/// <summary>
		/// Gets the column headers of the specified table
		/// </summary>
		public static List<string> GetHeadersFromTable(string tableName)
		{
			return DBAccess.GetStringsWithQuery("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "';");
		}

		/// <summary>
		/// Gets the data-types of all columns in the specified table
		/// </summary>
		public static List<string> GetColumnsType(string tableName)
		{
			return DBAccess.GetStringsWithQuery("SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "';");
		}

		public static bool IsColumnPrimaryKey(string columnName, string tableName)
		{
			string query = "SELECT K.CONSTRAINT_NAME " +
			"FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS K " +
			"ON C.TABLE_NAME = K.TABLE_NAME AND C.CONSTRAINT_CATALOG = K.CONSTRAINT_CATALOG AND C.CONSTRAINT_SCHEMA = K.CONSTRAINT_SCHEMA " +
			$"AND C.CONSTRAINT_NAME = K.CONSTRAINT_NAME WHERE C.CONSTRAINT_TYPE = 'PRIMARY KEY' AND K.COLUMN_NAME = '{columnName}' AND K.TABLE_NAME = '{tableName}';";
			return (DBAccess.GetListStringsWithQuery(query).Count > 0);
		}

		public static List<List<string>> GetColumnData(string tableName)
		{
			string query = $"SELECT Data_Type, Character_Maximum_Length FROM INFORMATION_SCHEMA.COLUMNS WHERE Table_Name = '{tableName}';";
			return DBAccess.GetListStringsWithQuery(query);
		}
	}
}
