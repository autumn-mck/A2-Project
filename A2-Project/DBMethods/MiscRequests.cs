using A2_Project.DBObjects;
using System;
using System.Collections.Generic;

namespace A2_Project.DBMethods
{
	public static class MiscRequests
	{
		public static void UpdateColumn(string table, string newData, string columnToUpdate, string idColumn, string id)
		{
			DBAccess.ExecuteNonQuery($"UPDATE [{table}] SET [{columnToUpdate}] = '{newData}' WHERE [{idColumn}] = '{id}';");
		}

		public static List<List<string>> GetAllAppointmentsOnDay(DateTime day, string[] headers)
		{
			return DBAccess.GetListStringsWithQuery("SELECT * FROM [Appointment] WHERE [Appointment Date] = '" + day.ToString("yyyy-MM-dd") + "';", headers);
		}

		public static List<List<string>> GetByColumnData(string table, string column, string toMatch, string[] headers)
		{
			return DBAccess.GetListStringsWithQuery($"SELECT * FROM [{table}] WHERE [{column}] = '{toMatch}';", headers);
		}

		public static bool DoesMeetForeignKeyReq(ForeignKey fKey, string data)
		{
			return DBAccess.GetStringsWithQuery($"SELECT COUNT([{fKey.ReferencedColumn}]) FROM [{fKey.ReferencedTable}] WHERE [{fKey.ReferencedColumn}] = '{data}';")[0] != "0";
		}

		public static bool IsPKeyFree(string table, string column, string value)
		{
			return DBAccess.GetStringsWithQuery($"SELECT COUNT([{column}]) FROM [{table}] WHERE [{column}] = '{value}';")[0] == "0";
		}

		public static void DeleteItem(string table, string col, string dataCondition, bool deleteRef = false)
		{
			if (!deleteRef)
			{
				bool isFKeyRef = IsInstReferenced(table, col, dataCondition);
				if (isFKeyRef) throw new Exception("Other objects reference the object you want to delete. Do you wish to delete them too?");
			}
			DBAccess.ExecuteNonQuery($"DELETE FROM [{table}] WHERE [{col}] = '{dataCondition}';");
		}

		public static bool IsInstReferenced(string table, string col, string dataCondition)
		{
			ForeignKey[] fKeysToTable = MetaRequests.GetFKeyToTable(table);
			bool isFKeyRef = false;
			foreach (ForeignKey fKey in fKeysToTable)
			{
				isFKeyRef = isFKeyRef || IsFKeyRefUsed(table, col, fKey, dataCondition);
			}
			return isFKeyRef;
		}

		public static bool IsFKeyRefUsed(string table, string col, ForeignKey fKey, string dataCondition)
		{
			return Convert.ToInt32(DBAccess.GetStringsWithQuery($"SELECT Count([{table}].[{col}]) FROM [{table}] INNER JOIN [{fKey.ReferencedTable}] ON [{fKey.ReferencedTable}].[{col}] = [{table}].[{col}] WHERE [{table}].[{col}] = '{dataCondition}';")[0]) > 0;
		}

		public static string GetMinKeyNotUsed(string table, string col)
		{
			return DBAccess.GetStringsWithQuery($"SELECT TOP 1 t1.[{col}]+1 FROM [{table}] t1 WHERE NOT EXISTS(SELECT * FROM [{table}] t2 WHERE t2.[{col}] = t1.[{col}] + 1) ORDER BY t1.[{col}]")[0];
		}
	}
}