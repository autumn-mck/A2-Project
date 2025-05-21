using A2_Project.DBObjects;
using System;
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
			// SQLite specific query
			return DBAccess.GetStringsWithQuery("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';");
		}

		/// <summary>
		/// Gets all data from the specified table
		/// </summary>
		public static List<List<string>> GetAllFromTable(string tableName, string[] headers = null)
		{
			// This query is generic enough, but ensure tableName is not injectable.
			return DBAccess.GetListStringsWithQuery($"SELECT * FROM [{tableName}];", headers);
		}

		// Helper method to get PRAGMA table_info results
		private static List<List<string>> GetPragmaTableInfo(string tableName)
		{
			// It's important that tableName is properly sanitized or comes from a trusted source
			// to prevent SQL injection if this method were to be used more broadly.
			// For current usage, tableName comes from GetTableNames or internal calls.
			return DBAccess.GetListStringsWithQuery($"PRAGMA table_info('{tableName}');");
		}

		public static bool IsColumnPrimaryKey(string columnName, string tableName)
		{
			List<List<string>> tableInfo = GetPragmaTableInfo(tableName);
			foreach (List<string> columnInfo in tableInfo)
			{
				// PRAGMA table_info columns: cid, name, type, notnull, dflt_value, pk
				if (columnInfo.Count > 1 && columnInfo[1] == columnName) // columnInfo[1] is 'name'
				{
					if (columnInfo.Count > 5 && int.TryParse(columnInfo[5], out int pkValue)) // columnInfo[5] is 'pk'
					{
						return pkValue > 0;
					}
				}
			}
			return false;
		}

		public static bool CanBeNull(string columnName, string tableName)
		{
			List<List<string>> tableInfo = GetPragmaTableInfo(tableName);
			foreach (List<string> columnInfo in tableInfo)
			{
				// PRAGMA table_info columns: cid, name, type, notnull, dflt_value, pk
				if (columnInfo.Count > 1 && columnInfo[1] == columnName) // columnInfo[1] is 'name'
				{
					if (columnInfo.Count > 3 && int.TryParse(columnInfo[3], out int notNullValue)) // columnInfo[3] is 'notnull'
					{
						return notNullValue == 0; // 'notnull' is 1 if NOT NULL, 0 if NULLABLE
					}
				}
			}
			// Default to true (nullable) if info not found, or handle error
			return true; 
		}

		public static Column[] GetColumnDataFromTable(string tableName)
		{
			List<List<string>> tableInfo = GetPragmaTableInfo(tableName); // cid, name, type, notnull, dflt_value, pk
			ForeignKey[] foreignKeys = GetFKeyOfTable(tableName); // Get foreign keys for this table

			Column[] columns = new Column[tableInfo.Count];
			for (int i = 0; i < tableInfo.Count; i++)
			{
				List<string> columnPragmaInfo = tableInfo[i];
				string colName = columnPragmaInfo[1];
				string colType = columnPragmaInfo[2];
				// SQLite PRAGMA table_info doesn't give max length directly. Pass null or empty.
				string colMaxLength = null; 

				columns[i] = new Column(colName, tableName)
				{
					// IsColumnPrimaryKey and CanBeNull will re-query PRAGMA table_info.
					// This can be optimized by parsing pk and notnull directly from columnPragmaInfo.
					// pk is columnPragmaInfo[5], notnull is columnPragmaInfo[3]
					Constraints = new Constraint(
						int.TryParse(columnPragmaInfo[5], out int pkVal) && pkVal > 0, // IsPrimaryKey
						!(int.TryParse(columnPragmaInfo[3], out int nnVal) && nnVal == 1), // CanBeNull (true if notnull is 0)
						colType, 
						colMaxLength 
					)
				};
				
				// Find if this column is a foreign key 'from' column
				var fk = foreignKeys.FirstOrDefault(f => f.ReferencedPKColumn == colName); // This seems wrong. ReferencedColumn is the column in the *other* table.
                                                                                     // We need to check if colName is a 'from' column in the foreign_key_list.
                                                                                     // The ForeignKey object stores ReferencedTable and ReferencedColumn (which is the PK of the referenced table).
                                                                                     // The current ForeignKey structure might need adjustment or this logic needs to use the 'from' column.
                                                                                     // Let's assume for now that GetFKeyOfTable returns FKs where ForeignKey.ReferencedColumn is the name of the column *in the current table* that is the FK.
                                                                                     // This is how it seemed to be used before: foreignKeys.Where(x => x.ReferencedColumn == columns[i].Name)
                                                                                     // However, PRAGMA foreign_key_list gives 'from' and 'to'. We need to map 'from' to columns[i].Name
                                                                                     // And then populate ForeignKey with 'table' (referenced table) and 'to' (referenced column in other table)
                columns[i].Constraints.ForeignKey = foreignKeys.FirstOrDefault(f_key => f_key.LocalColumn == colName);


			}
			return columns;
		}

		public static List<List<string>> GetDataTypesFromTable(string tableName)
		{
			// This method returned Column_Name, Data_Type, Character_Maximum_Length
			// PRAGMA table_info returns: cid, name, type, notnull, dflt_value, pk
			// We need to adapt the output to List<List<string>> where each inner list is [name, type, maxLength (or null)]
			List<List<string>> results = new List<List<string>>();
			List<List<string>> tableInfo = GetPragmaTableInfo(tableName);
			foreach (List<string> columnPragmaInfo in tableInfo)
			{
				string colName = columnPragmaInfo[1];
				string colType = columnPragmaInfo[2];
				// SQLite PRAGMA table_info doesn't give max length directly. Pass null.
				results.Add(new List<string> { colName, colType, null });
			}
			return results;
		}

		public static ForeignKey[] GetFKeyToTable(string tableName)
		{
			// This method finds which other tables have FKs pointing to *this* tableName.
			// This is complex with SQLite PRAGMA as you need to scan all tables.
			// For now, returning empty or not implementing fully if not critical.
			// The call stack seems to rely more on GetFKeyOfTable.
			Console.WriteLine("Warning: MetaRequests.GetFKeyToTable is not fully implemented for SQLite.");
			return Array.Empty<ForeignKey>();
		}

		public static ForeignKey[] GetFKeyOfTable(string tableName)
		{
			// PRAGMA foreign_key_list('tableName')
			// Result columns: id, seq, table (referenced_table), from (fk_column_in_this_table), to (pk_column_in_referenced_table), on_update, on_delete, match
			List<List<string>> fkInfoList = DBAccess.GetListStringsWithQuery($"PRAGMA foreign_key_list('{tableName}');");
			List<ForeignKey> foreignKeys = new List<ForeignKey>();
			foreach (List<string> fkInfo in fkInfoList)
			{
				if (fkInfo.Count > 4)
				{
					string referencedTable = fkInfo[2]; // 'table' column - the table this FK points to
					string localColumnName = fkInfo[3];   // 'from' column - the FK column in 'tableName'
					string referencedColumnInOtherTable = fkInfo[4]; // 'to' column - the PK column in 'referencedTable'
					
					// The ForeignKey constructor is ForeignKey(string referencedTable, string referencedColumn)
					// It seems 'referencedColumn' was used as the name of the column *in the current table* that IS the foreign key.
					// This is confusing. Let's adjust.
					// ForeignKey should store:
					// 1. The table it references (referencedTable)
					// 2. The column in the *referenced* table it points to (referencedColumnInOtherTable)
					// 3. We also need the actual column in *this* table that is the FK (localColumnName).
					// The existing ForeignKey class only has ReferencedTable and ReferencedColumn.
					// If ReferencedColumn is meant to be the column *in this table*, then new ForeignKey(referencedTable, localColumnName)
					// But then we lose what column it points *to* in the other table.
					// Let's assume the old system used ReferencedColumn as the name of the FK column in the current table.
					// And ReferencedTable as the table it points to. This is incomplete for full FK info.
					// The line `columns[i].Constraints.ForeignKey = foreignKeys.Where(x => x.ReferencedColumn == columns[i].Name).FirstOrDefault();`
					// implies ReferencedColumn was indeed the name of the column in the current table.

					// For the purpose of GetColumnDataFromTable, we need to associate the FK constraint with the local column.
					// So, we need a ForeignKey object that stores which local column it is, and what remote table/column it points to.
					// Let's make ForeignKey store:
					//   LocalColumn (string): The name of the FK column in `tableName`.
					//   ReferencedTable (string): The name of the table this FK points to.
					//   ReferencedPKColumn (string): The name of the PK column in `ReferencedTable`.
					// I'll need to modify ForeignKey.cs for this. For now, I'll adapt to the existing structure as best as possible.
					// The existing `ForeignKey(string referencedTable, string referencedColumn)`
					// Let's assume referencedColumn means the local FK column name.
					foreignKeys.Add(new ForeignKey(referencedTable, localColumnName, referencedColumnInOtherTable));
				}
			}
			return foreignKeys.ToArray();
		}

		public static List<string> GetAllTableNames()
		{
			// SQLite specific query - same as GetTableNames
			return DBAccess.GetStringsWithQuery("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';");
		}
	}
}
// Need to adjust DBObjects.ForeignKey to include LocalColumn, ReferencedTable, and ReferencedPKColumn for clarity
// For now, I made a temporary constructor ForeignKey(string referencedTable, string localColumn, string referencedPKCol)
// but this will require changing ForeignKey.cs
