using A2_Project.DBObjects;
using A2_Project.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A2_Project.DBMethods
{
	public static class MiscRequests
	{
		public static List<List<string>> AppTypes { get; set; }

		public static void UpdateColumn(string table, string newData, string columnToUpdate, string idColumn, string id)
		{
			DBAccess.ExecuteNonQuery($"UPDATE [{table}] SET [{columnToUpdate}] = '{newData}' WHERE [{idColumn}] = '{id}';");
		}

		public static List<List<string>> GetAllAppointmentsOnDay(DateTime day, string[] headers)
		{
			return DBAccess.GetListStringsWithQuery("SELECT * FROM [Appointment] WHERE [Appointment Date] = '" + day.ToString("yyyy-MM-dd") + "' AND [Cancelled] = 'False';", headers);
		}

		public static List<List<string>> GetByColumnData(string table, string column, string toMatch, string[] headers = null)
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
			List<string> res =  DBAccess.GetStringsWithQuery($"SELECT TOP 1 t1.[{col}]+1 FROM [{table}] t1 WHERE NOT EXISTS(SELECT * FROM [{table}] t2 WHERE t2.[{col}] = t1.[{col}] + 1) ORDER BY t1.[{col}]");
			if (res.Count == 0) return "0";
			else return res[0];
		}

		internal static string GetMinKeyNotUsed(string table, string col, List<BookingCreator> booking)
		{
			string res = DBAccess.GetStringsWithQuery($"SELECT Max([{col}]) FROM [{table}];")[0];
			if (res == "") return (booking.Select(b => b.GetData().Count).Sum() + 1).ToString();
			int id = Convert.ToInt32(res);
			id += booking.Select(b => b.GetData().Count).Sum() + 1;
			return id.ToString();
		}

		public static bool IsAppointmentInitial(string[] data, List<BookingCreator> booking)
		{
			string dogID = data[1];
			if (dogID == "") return false;

			List<List<string>> results = DBAccess.GetListStringsWithQuery($"SELECT TOP 1 [Appointment].[Appointment ID], [Appointment].[Appointment Date], [Appointment].[Appointment Time] FROM [Appointment] INNER JOIN [Dog] ON [Dog].[Dog ID] = [Appointment].[Dog ID] WHERE [Dog].[Dog ID] = {dogID} AND [Appointment].[Cancelled] = 'False' ORDER BY [Appointment].[Appointment Date], [Appointment].[Appointment Time];");
			if (results.Count == 0) return true;


			if (data[9] == "" || data[10] == "") return false;
			DateTime compDateTime = DateTime.Parse(data[9]).Add(TimeSpan.Parse(data[10]));

			if (booking is not null)
			{
				foreach (BookingCreator b in booking)
				{
					List<string[]> bkData = b.GetData();
					foreach (string[] bk in bkData)
					{
						if (bk[9] == "" || bk[10] == "") continue;
						if (bk[1] == data[1])
						{
							DateTime bkDateTime = DateTime.Parse(bk[9]).Add(TimeSpan.Parse(bk[10]));
							if (compDateTime > bkDateTime) return false;
						}
					}
				}
			}

			if (data[0] == results[0][0]) return true;

			DateTime initDateTime = DateTime.Parse(results[0][1]).Add(TimeSpan.Parse(results[0][2]));

			return compDateTime <= initDateTime;
		}

		internal static bool DoesAppointmentClash(string[] data, List<BookingCreator> bookings, out string errMessage)
		{
			return DoesAppointmentClash(data, Convert.ToInt32(data[5]), DateTime.Parse(data[9]), TimeSpan.Parse(data[10]), bookings, out errMessage);
		}

		public static bool DoesAppointmentClash(string[] oldData, int roomID, DateTime date, TimeSpan time, List<BookingCreator> bookings, out string errMessage)
		{
			int thisAppLength = GetAppLength(oldData, bookings);
			TimeSpan appEnd = time.Add(new TimeSpan(0, thisAppLength, 0));

			foreach(BookingCreator booking in bookings)
			{
				if (!booking.IsAdded) continue;
				List<string[]> bkData = booking.GetData();
				foreach (string[] bk in bkData)
				{
					if (bk[0] == oldData[0]) // Cannot clash with itself
					{
						if (date < DateTime.Now.Date)
						{
							errMessage = "An appointment cannot be booked in the past!";
							return true;
						}
						continue;
					}
					
					if (bk is null) continue; 
					if (bk[9] == "" || bk[10] == "") continue; // Booking has not yet been made

					if (
					(bk[5] == roomID.ToString() // Same room
					|| bk[1] == oldData[1]		// Same dog
					|| bk[3] == oldData[3])		// Same staff
					&& DateTime.Parse(bk[9]).Date == date)
					{
						TimeSpan bkStart = TimeSpan.Parse(bk[10]);
						int bkLength = GetAppLength(bk, bookings);
						TimeSpan bkEnd = bkStart.Add(new TimeSpan(0, bkLength, 0));
						if ((bkEnd > time && bkStart < time) || (bkStart < appEnd && bkStart >= time))
						{
							errMessage = "Clashes with a new appointment!";
							return true;
						}
					}
				}
			}

			string query = $"SELECT * FROM [Appointment] WHERE [Appointment].[Appointment Date] = '{date:yyyy-MM-dd}' " +
			$"AND [Appointment].[Appointment Time] < '{appEnd}' AND [Appointment].[Cancelled] = 'False';";
			List<List<string>> allOnDay = DBAccess.GetListStringsWithQuery(query);
			// An appointment cannot clash with itself, so remove the appointment with the same unique ID (If it exists)
			allOnDay.Remove(allOnDay.Where(a => a[0] == oldData[0]).FirstOrDefault());

			List<List<string>> potentialCollisions = new List<List<string>>();

			foreach (List<string> ls in allOnDay)
			{
				int appLength = GetAppLength(ls.ToArray(), bookings);
				TimeSpan localAppEnd = TimeSpan.Parse(ls[10]).Add(new TimeSpan(0, appLength, 0));
				if (localAppEnd > time) potentialCollisions.Add(ls);
			}

			foreach (List<string> ls in potentialCollisions)
			{
				if (ls[1] == oldData[1])
				{
					errMessage = "That dog is at another appointment at the same time!";
					return true; // A dog cannot be in 2 appointments at once
				}

				if (ls[3] == oldData[3])
				{
					errMessage = "A staff member cannot be at 2 simultanious appointments!";
					return true; // A staff member cannot be at 2 appointments at once
				}
			}

			List<List<string>> inRoom = potentialCollisions.Where(a => a[5] == roomID.ToString()).ToList();
			if (inRoom.Count > 0)
			{
				errMessage = "There is another appointment in that room at that time!";
				return true;
			}

			// Check if the staff member is available
			string staffID = oldData[3];
			int dow = (int)(date.DayOfWeek + 6) % 7;

			
			if (!IsAppInShift(dow, staffID, time, appEnd, date.Date))
			{
				errMessage = "That staff member's shift does not cover that time!";
				return true;
			}

			errMessage = "";
			return false;
		}

		public static bool IsAppInShift(string[] data, List<BookingCreator> booking)
		{
			if (data[9] == "" || data[10] == "") return true;
			DateTime appDate = DateTime.Parse(data[9]);
			int dow = ((int)appDate.DayOfWeek + 6) % 7;
			TimeSpan appStart = TimeSpan.Parse(data[10]);
			int len = GetAppLength(data, booking);
			return IsAppInShift(dow, data[3], appStart, appStart.Add(TimeSpan.FromMinutes(len)), appDate);
		}

		public static bool IsAppInShift(int dow, string staffID, TimeSpan appStart, TimeSpan appEnd, DateTime appDate)
		{
			bool isInShift = false;

			string shiftQuery = $"SELECT [Shift Start Time], [Shift End Time] FROM [Shift] WHERE [Shift].[Staff ID] = {staffID} AND [Shift].[Shift Day] = {dow};";
			List<List<string>> shiftData = DBAccess.GetListStringsWithQuery(shiftQuery);

			foreach (List<string> shift in shiftData)
			{
				TimeSpan shiftStart = TimeSpan.Parse(shift[0]);
				TimeSpan shiftEnd = TimeSpan.Parse(shift[1]);

				isInShift = (appStart >= shiftStart && appEnd <= shiftEnd) || isInShift;

				// Note: If a shift starts in shift A and ends in shift B, and shift A and B have no time gap between them,
				// The result will still be marked as clashing. This is an unsupported use case.
			}

			string shiftExcQuery = $"SELECT [Shift Exception ID] FROM [Shift Exception] WHERE [Staff ID] = {staffID} AND [Start Date] <= '{appDate:yyyy-MM-dd}' AND [End Date] >= '{appDate:yyyy-MM-dd}';";
			List<string> shiftExcData = DBAccess.GetStringsWithQuery(shiftExcQuery);

			return isInShift && shiftExcData.Count == 0;
		}

		public static int GetAppLength(string[] data, List<BookingCreator> booking)
		{
			if (AppTypes is null) AppTypes = MetaRequests.GetAllFromTable("Appointment Type");

			// appLength is in minutes
			int appLength = (int)(Convert.ToDouble(AppTypes[Convert.ToInt32(data[2])][1]) * 60);
			if (data[6] == "True") appLength += 15;
			if (IsAppointmentInitial(data, booking)) appLength += 15;

			return appLength;
		}

		public static List<List<string>> GetInvoiceData(string clientID)
		{
			double[] basePrices = DBAccess.GetStringsWithQuery("SELECT [Base Price] FROM [Appointment Type];").Select(double.Parse).ToArray();
			string query = "SELECT [Appointment].[Booking ID], [Appointment].[Appointment ID], [Dog].[Dog Name], " +
			"[Appointment Type].[Description], [Staff].[Staff Name], [Appointment].[Nails And Teeth], " +
			"[Appointment].[Appointment Date], [Appointment].[Appointment Time], " +
			"[Appointment].[Appointment Type ID] " +
			"FROM [Appointment] INNER JOIN [Staff] ON [Staff].[Staff ID] = [Appointment].[Staff ID] " +
			"INNER JOIN [Dog] ON [Dog].[Dog ID] = [Appointment].[Dog ID] " +
			"INNER JOIN [Appointment Type] ON [Appointment Type].[Appointment Type ID] = [Appointment].[Appointment Type ID] " +
			$"WHERE [Dog].[Client ID] = {clientID} AND " +
			$"[Appointment].[Appointment Date] BETWEEN '{DateTime.Now.AddMonths(-12):yyyy-MM-dd}' AND '{DateTime.Now:yyyy-MM-dd}' " +
			"AND [Appointment].[Cancelled] = 'False' ORDER BY [Appointment].[Appointment Date];";

			List<List<string>> results = DBAccess.GetListStringsWithQuery(query);

			foreach (List<string> ls in results)
			{
				int appTypeID = Convert.ToInt32(ls[^1]);
				ls.Remove(ls[^1]);
				double price = basePrices[appTypeID];
				if (ls[5] == "True") price += 10;
				if (IsAppointmentInitial(ls.ToArray(), null)) price += 5;
				price = price * (100.0 - GraphingRequests.GetBookingDiscount(ls[0])) / 100.0;
				ls.Add('£' + Math.Round(price, 2).ToString());
			}

			return results;
		}

		public static List<List<string>> GetContactDataFromClient(string clientID)
		{
			return DBAccess.GetListStringsWithQuery($"SELECT [Contact].[Contact Name], [Contact].[Contact Phone No], [Contact].[Contact Email], 'Test' FROM [Contact] WHERE [Contact].[Client ID] = {clientID};");
		}
	}
}