using A2_Project.DBObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A2_Project.DBMethods
{
	public static class MiscRequests
	{
		public static List<List<string>> AppTypes;

		public static void UpdateColumn(string table, string newData, string columnToUpdate, string idColumn, string id)
		{
			DBAccess.ExecuteNonQuery($"UPDATE [{table}] SET [{columnToUpdate}] = '{newData}' WHERE [{idColumn}] = '{id}';");
		}

		public static List<List<string>> GetAllAppointmentsOnDay(DateTime day, string[] headers)
		{
			return DBAccess.GetListStringsWithQuery("SELECT * FROM [Appointment] WHERE [Appointment Date] = '" + day.ToString("yyyy-MM-dd") + "' AND [Is Cancelled] = 'False';", headers);
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

		public static bool IsAppointmentInitial(string[] data)
		{
			string dogID = data[1];
			string clientFirstAppID = DBAccess.GetStringsWithQuery($"SELECT TOP 1 [Appointment].[Appointment ID] FROM [Appointment] INNER JOIN [Dog] ON [Dog].[Dog ID] = [Appointment].[Dog ID] WHERE [Dog].[Dog ID] = {dogID} ORDER BY [Appointment].[Appointment Date], [Appointment].[Appointment Time];")[0];
			return data[0] == clientFirstAppID;
		}

		public static bool IsAppointmentInitial(string appID)
		{
			string dogID = DBAccess.GetStringsWithQuery($"SELECT [Dog].[Dog ID] FROM [Dog] INNER JOIN [Appointment] ON [Appointment].[Dog ID] = [Dog].[Dog ID] WHERE [Appointment].[Appointment ID] = {appID};")[0];
			string clientFirstAppID = DBAccess.GetStringsWithQuery($"SELECT TOP 1 [Appointment].[Appointment ID] FROM [Appointment] INNER JOIN [Dog] ON [Dog].[Dog ID] = [Appointment].[Dog ID] WHERE [Dog].[Dog ID] = {dogID} ORDER BY [Appointment].[Appointment Date], [Appointment].[Appointment Time];")[0];
			return appID == clientFirstAppID;
		}

		public static bool DoesAppointmentClash(string[] oldData, int roomID, DateTime date, TimeSpan time)
		{
			//return true;
			TimeSpan appEnd = time.Add(new TimeSpan(0, GetAppLength(Convert.ToInt32(oldData[2]), oldData[6] == "True", oldData[0]), 0));
			
			List<List<string>> allOnDay = DBAccess.GetListStringsWithQuery($"SELECT * FROM [Appointment] WHERE [Appointment].[Appointment Date] = '{date:yyyy-MM-dd}' AND [Appointment].[Appointment Time] < '{appEnd}' AND [Appointment].[Is Cancelled] = 'False';");
			// An appointment cannot clash with itself, so remove the appointment with the same unique ID (If it exists)
			allOnDay.Remove(allOnDay.Where(a => a[0] == oldData[0]).FirstOrDefault());

			List<List<string>> potentialCollisions = new List<List<string>>();

			foreach (List<string> ls in allOnDay)
			{
				int appLength = GetAppLength(ls.ToArray());
				TimeSpan localAppEnd = TimeSpan.Parse(ls[10]).Add(new TimeSpan(0, appLength, 0));
				if (localAppEnd > time) potentialCollisions.Add(ls);
			}

			foreach (List<string> ls in potentialCollisions)
			{
				if (ls[1] == oldData[1]) return true; // A dog cannot be in 2 appointments at once
				if (ls[3] == oldData[3]) return true; // A staff member cannot be at 2 appointments at once
			}

			List<List<string>> inRoom = potentialCollisions.Where(a => a[5] == roomID.ToString()).ToList();
			if (inRoom.Count > 0) return true;
			return false;
		}

		public static int GetAppLength(string[] data)
		{
			if (AppTypes is null) AppTypes = MetaRequests.GetAllFromTable("Appointment Type");

			// appLength is in minutes
			int appLength = (int)(Convert.ToDouble(AppTypes[Convert.ToInt32(data[2])][1]) * 60);
			if (data[6] == "True") appLength += 15;
			if (IsAppointmentInitial(data)) appLength += 15;

			return appLength;
		}

		public static int GetAppLength(int typeID, bool includesNailAndTeeth, string appID)
		{
			if (AppTypes is null) AppTypes = MetaRequests.GetAllFromTable("Appointment Type");

			// appLength is in minutes
			int appLength = (int)(Convert.ToDouble(AppTypes[typeID][1]) * 60);
			if (includesNailAndTeeth) appLength += 15;
			if (IsAppointmentInitial(appID)) appLength += 15;

			return appLength;
		}
	}
}