using A2_Project.DBObjects;
using System.Collections.Generic;
using System.Linq;

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
		public static List<List<string>> GetAllFromTable(string tableName, string[] headers)
		{
			// TODO: Should be in MiscRequests
			return DBAccess.GetListStringsWithQuery("SELECT * FROM [" + tableName + "];", headers);
		}

		public static bool IsColumnPrimaryKey(string columnName, string tableName)
		{
			string query = "SELECT K.CONSTRAINT_NAME " +
			"FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS K " +
			"ON C.TABLE_NAME = K.TABLE_NAME AND C.CONSTRAINT_CATALOG = K.CONSTRAINT_CATALOG AND C.CONSTRAINT_SCHEMA = K.CONSTRAINT_SCHEMA " +
			$"AND C.CONSTRAINT_NAME = K.CONSTRAINT_NAME WHERE C.CONSTRAINT_TYPE = 'PRIMARY KEY' AND K.COLUMN_NAME = '{columnName}' AND K.TABLE_NAME = '{tableName}';";
			return (DBAccess.GetListStringsWithQuery(query).Count > 0);
		}

		public static Column[] GetColumnDataFromTable(string tableName)
		{
			List<List<string>> types = GetDataTypesFromTable(tableName);
			ForeignKey[] foreignKeys = GetFKeyOfTable(tableName);
			Column[] columns = new Column[types.Count];
			for (int i = 0; i < types.Count; i++)
			{
				columns[i] = new Column(types[i][0])
				{
					Constraints = new Constraint(IsColumnPrimaryKey(types[i][0], tableName), types[i][1], types[i][2])
				};

				columns[i].Constraints.ForeignKey = foreignKeys.Where(x => x.ReferencedColumn == columns[i].Name).FirstOrDefault();
			}
			return columns;
		}

		public static List<List<string>> GetDataTypesFromTable(string tableName)
		{
			string query = $"SELECT Column_Name, Data_Type, Character_Maximum_Length FROM INFORMATION_SCHEMA.COLUMNS WHERE Table_Name = '{tableName}';";
			return DBAccess.GetListStringsWithQuery(query);
		}

		public static ForeignKey[] GetFKeyToTable(string tableName)
		{
			string query = "SELECT OBJECT_NAME(f.parent_object_id) TableName, COL_NAME(fc.parent_object_id, fc.parent_column_id) ColName " +
			"FROM sys.foreign_keys AS f INNER JOIN sys.foreign_key_columns AS fc ON f.OBJECT_ID = fc.constraint_object_id " +
			$"INNER JOIN sys.tables t ON t.OBJECT_ID = fc.referenced_object_id WHERE OBJECT_NAME(f.referenced_object_id) = '{tableName}';";
			List<List<string>> results = DBAccess.GetListStringsWithQuery(query);
			ForeignKey[] toReturn = new ForeignKey[results.Count];
			for (int i = 0; i < results.Count; i++)
				toReturn[i] = new ForeignKey(results[i][0], results[i][1]);
			return toReturn;
		}

		public static ForeignKey[] GetFKeyOfTable(string tableName)
		{
			// TODO: Remove unnecessary data from queries
			string query = $"SELECT obj.name AS FK_NAME, sch.name AS [schema_name], tab1.name AS [table], col1.name AS [column], tab2.name AS [referenced_table], " +
			"col2.name AS [referenced_column] FROM sys.foreign_key_columns fkc INNER JOIN sys.objects obj ON obj.object_id = fkc.constraint_object_id " +
			"INNER JOIN sys.tables tab1 ON tab1.object_id = fkc.parent_object_id INNER JOIN sys.schemas sch ON tab1.schema_id = sch.schema_id " +
			"INNER JOIN sys.columns col1 ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id " +
			"INNER JOIN sys.tables tab2 ON tab2.object_id = fkc.referenced_object_id INNER JOIN sys.columns col2 ON " +
			$"col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id WHERE tab1.name = '{tableName}';";
			List<List<string>> results = DBAccess.GetListStringsWithQuery(query);
			ForeignKey[] toReturn = new ForeignKey[results.Count];
			for (int i = 0; i < results.Count; i++)
				toReturn[i] = new ForeignKey(results[i][4], results[i][5]);
			return toReturn;
		}
	}
}
