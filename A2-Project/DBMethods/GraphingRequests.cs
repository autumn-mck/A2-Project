using System;
using System.Collections.Generic;
using System.Linq;

namespace A2_Project.DBMethods
{
	public static class GraphingRequests
	{
		private static readonly string[] months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
		private static readonly List<List<string>> appTypeData = MetaRequests.GetAllFromTable("Appointment Type");

		public static void GetCountOfAppointmentTypes(ref int[][] data, ref string[] headers, DateTime minDate)
		{
			string dataQuery = "SELECT Count([Appointment Type ID]) FROM [Appointment] " +
			$"WHERE [Appointment Date] BETWEEN '{minDate:yyyy-MM-dd}' AND '{DateTime.Now:yyyy-MM-dd}' " +
			"GROUP BY [Appointment Type ID];";
			data[0] = DBAccess.GetStringsWithQuery(dataQuery).Select(int.Parse).ToArray();
			headers = DBAccess.GetStringsWithQuery("SELECT [Description] FROM [Appointment Type] ORDER BY [Appointment Type ID];").ToArray();
		}

		public static void GetBusinessOfStaff(ref int[][] data, ref string[] headers, DateTime minDate)
		{
			string dataQuery = "SELECT Count([Staff ID]) FROM [Appointment] " +
			$"WHERE [Appointment Date] BETWEEN '{minDate:yyyy-MM-dd}' AND '{DateTime.Now:yyyy-MM-dd}' " +
			"GROUP BY [Staff ID] ORDER BY [Staff ID];";
			data[0] = DBAccess.GetStringsWithQuery(dataQuery).Select(int.Parse).ToArray();
			headers = DBAccess.GetStringsWithQuery("SELECT [Staff Name] FROM [Staff] ORDER BY [Staff ID];").ToArray();
		}

		/// <summary>
		/// Gets the number of clients over time
		/// </summary>
		public static void GetGrowthOverTime(ref int[][] data, ref string[] headers, DateTime minDate)
		{
			DateTime startDate = MaxDate(Convert.ToDateTime(DBAccess.GetStringsWithQuery("SELECT MIN([Client Join Date]) FROM [Client]")[0]), minDate);
			DateTime endDate = DateTime.Now.Date;
			int diff = (int)(endDate - startDate).TotalDays;
			List<int> growth = new List<int>();
			for (double i = 0; i < diff; i += diff / 75.0)
			{
				string query = $"SELECT COUNT([Client ID]) FROM [Client] WHERE [Client Join Date] < '{startDate.AddDays(i):yyyy-MM-dd}';";
				growth.Add(Convert.ToInt32(DBAccess.GetStringsWithQuery(query)[0]));
			}
			data[0] = growth.ToArray();
			headers = InterpolateDates(startDate, diff);
		}

		/// <summary>
		/// Gets the number of appointments on each day of the week
		/// </summary>
		public static void GetAppsByDayOfWeek(ref int[][] data, ref string[] headers, DateTime minDate)
		{
			string query = "SET DATEFIRST 1; SELECT Count([Appointment ID]) FROM [Appointment] " +
			$"WHERE [Appointment Date] BETWEEN '{minDate:yyyy-MM-dd}' AND '{DateTime.Now:yyyy-MM-dd}' " +
			"GROUP BY DatePart(WeekDay, [Appointment Date]) ORDER BY DatePart(WeekDay, [Appointment Date]);";
			data[0] = DBAccess.GetStringsWithQuery(query).Select(x => Convert.ToInt32(x)).ToArray();
			headers = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
		}

		/// <summary>
		/// Gets the number of appointments in each month of the last year
		/// </summary>
		public static void GetBookingsInMonths(ref int[][] data, ref string[] headers, DateTime minDate)
		{
			string query = "SELECT Count([Appointment ID]) FROM [Appointment] " +
			$"WHERE [Appointment Date] BETWEEN '{DateTime.Now.AddYears(-1):yyyy-MM-dd}' AND '{DateTime.Now:yyyy-MM-dd}' " +
			"GROUP BY DatePart(Month, [Appointment Date]) ORDER BY DatePart(Month, [Appointment Date]);";
			data[0] = DBAccess.GetStringsWithQuery(query).Select(x => Convert.ToInt32(x)).ToArray();
			headers = months;
		}

		/// <summary>
		/// Gets a rolling average of what % of appointments have been cancelled over time
		/// </summary>
		public static void GetAppCancelRate(ref int[][] data, ref string[] headers, DateTime minDate)
		{
			DateTime startDate = MaxDate(Convert.ToDateTime(DBAccess.GetStringsWithQuery("SELECT MIN([Appointment Date]) FROM Appointment")[0]), minDate);
			DateTime endDate = DateTime.Now;
			int diff = (int)(endDate - startDate).TotalDays;
			double increment = diff / 75.0;
			List<int> cancelRate = new List<int>();
			for (double i = 0; i < diff; i += increment)
			{
				DateTime currentDate = startDate.AddDays(i);

				string totalInTimeQuery = "SELECT COUNT([Appointment ID]) FROM [Appointment] " +
				$"WHERE [Appointment Date] BETWEEN '{currentDate.AddDays(-increment * 10):yyyy-MM-dd}' AND '{currentDate:yyyy-MM-dd}';";
				int totalInTime = Convert.ToInt32(DBAccess.GetStringsWithQuery(totalInTimeQuery)[0]);

				string cancelledInTimeQuery = "SELECT COUNT([Appointment ID]) FROM [Appointment] " +
				$"WHERE [Is Cancelled] = 1 AND [Appointment Date] BETWEEN '{currentDate.AddDays(-increment * 10):yyyy-MM-dd}' AND '{currentDate:yyyy-MM-dd}';";
				int cancelledInTime = Convert.ToInt32(DBAccess.GetStringsWithQuery(cancelledInTimeQuery)[0]);
				if (cancelledInTime == 0 || totalInTime == 0)
					cancelRate.Add(0);
				else cancelRate.Add((int)((float)cancelledInTime * 100 / totalInTime));
			}
			data[0] = cancelRate.ToArray();
			headers = InterpolateDates(startDate, diff);
		}

		// TODO: Currently classifies clients that have > 1 dogs and booked them all in one go as repeat customers, even if they never book after that.
		public static void GetCustReturns(ref int[][] data, ref string[] headers, DateTime minDate)
		{
			//DateTime startDate = MaxDate(Convert.ToDateTime(DBAccess.GetStringsWithQuery("SELECT MIN([Client Join Date]) FROM [Client]")[0]), minDate);
			//DateTime endDate = DateTime.Now.Date;
			//int diff = (int)(endDate - startDate).TotalDays;
			//List<int> returnRates = new List<int>();
			//double increment = diff / 75.0;

			//for (double i = 0; i < diff; i += increment)
			//{
			//	DateTime timeConsid = startDate.AddDays(i);
			//	int clientsInTime = Convert.ToInt32(DBAccess.GetStringsWithQuery($"SELECT COUNT([Client ID]) FROM [Client] WHERE [Client Join Date] < '{timeConsid:yyyy-MM-dd}';")[0]);
			//	int clientsLeftInTime = DBAccess.GetStringsWithQuery($"SELECT Count([Booking ID]) FROM [Booking] WHERE [Booking].[Date Made] BETWEEN '{timeConsid.AddDays(-increment * 10)}' AND '{timeConsid}' H")
			//}
			//data[0] = returnRates.ToArray();
			//headers = InterpolateDates(startDate, diff);

			//List<string> ls = DBAccess.GetStringsWithQuery("SELECT TOP 1 [Booking].[Date Made] FROM [Booking] INNER JOIN [Appointment] ON [Booking].[Booking ID] = [Appointment].[Booking ID] INNER JOIN [Dog] On [Dog].[Dog ID] = [Appointment].[Dog ID] GROUP BY [Dog].[Client ID] ORDER BY [Booking].[Date Made];");


			DateTime startDate = MaxDate(Convert.ToDateTime(DBAccess.GetStringsWithQuery("SELECT MIN([Client Join Date]) FROM [Client]")[0]), minDate);
			DateTime endDate = DateTime.Now;
			double diff = (endDate - startDate).TotalDays;
			List<int> returns = new List<int>();
			int count = 0;
			for (double i = 0; i < diff; i += diff / 40.0)
			{
				string query = "SELECT SUM(1) FROM [Client] " +
				"INNER JOIN [Dog] ON [Dog].[Client ID] = [Client].[Client ID] INNER JOIN [Appointment] ON [Appointment].[Dog ID] = [Dog].[Dog ID] " +
				"INNER JOIN [Booking] ON [Booking].[Booking ID] = [Appointment].[Booking ID] " +
				$"WHERE [Client].[Client Join Date] BETWEEN '{startDate.AddDays(i - diff / 40.0):yyyy-MM-dd}' AND '{startDate.AddDays(i):yyyy-MM-dd}' " +
				"GROUP BY [Client].[Client ID] HAVING COUNT([Booking].[Booking ID]) < 2;";

				List<List<string>> result = DBAccess.GetListStringsWithQuery(query);

				if (count == 0) returns.Add(result.Count);
				else returns.Add(result.Count + returns[count - 1]);
				count++;
			}
			data[0] = returns.ToArray();
			headers = InterpolateDates(startDate, (int)diff);
		}

		public static void GetIncomeLastYear(ref int[][] data, ref string[] headers, DateTime minDate)
		{
			headers = new string[12];
			data[0] = new int[12];
			DateTime endDate = DateTime.Now.Date;
			DateTime startDate = endDate.AddMonths(-12);
			for (int i = 0; i < 12; i++)
			{
				headers[i] = months[startDate.AddMonths(i).Month - 1];
				// TODO: Consider if is first booking and booking discount
				string query = "SELECT " +
				"CASE " +
					"WHEN [Appointment Type ID] = 0 THEN 35 " +
					"WHEN [Appointment Type ID] = 1 THEN 40 " +
					"WHEN [Appointment Type ID] = 2 THEN 50 " +
					"ELSE 0 " +
				"END, " +
				"CASE " +
					"WHEN [Includes Nail And Teeth] = 'True' THEN 10 " +
					"ELSE 0 " +
				"END " +
				$"FROM [Appointment] WHERE [Is Paid] = 1 AND [Appointment Date] BETWEEN '{startDate.AddMonths(i):yyyy-MM-dd}' AND '{startDate.AddMonths(i + 1):yyyy-MM-dd}';";
				List<List<string>> dataFromMonth = DBAccess.GetListStringsWithQuery(query);
				double incomeFromMonth = 0;
				foreach (List<string> appData in dataFromMonth)
				{
					double appIncome = Convert.ToDouble(appData[0]);
					appIncome += Convert.ToDouble(appData[1]);
					//appIncome = appIncome * (100.0 - GetBookingDiscount(appData[2]));

					//income += CalculateAppointmentPrice(app.ToArray());
					incomeFromMonth += appIncome - 43.4;
				}
				data[0][i] = (int)Math.Round(incomeFromMonth);
			}
		}

		private static double CalculateAppointmentPrice(string[] data)
		{
			double price = 0;

			price += Convert.ToDouble(appTypeData[Convert.ToInt32(data[0])][2]);
			if (data[1] == "True") price += 10;
			if (MiscRequests.IsAppointmentInitial(data[3])) price += 5;
			price *= (100.0 - GetBookingDiscount(data[2])) / 100.0;

			return price;
		}

		private static double GetBookingDiscount(string bookingID)
		{
			int count = Convert.ToInt32(DBAccess.GetStringsWithQuery($"SELECT Count([Appointment ID]) FROM [Appointment] WHERE [Appointment].[Booking ID] = {bookingID};")[0]);
			if (count > 2) return 5;
			else return 0;
		}

		/// <summary>
		/// Returns 7 dates linearly between the startDate and end date.
		/// diff represents the difference between the start date and the end date in days
		/// </summary>
		private static string[] InterpolateDates(DateTime startDate, int diff)
		{
			List<string> dates = new List<string>();
			for (double i = 0; i <= diff; i += (double)diff / 6)
			{
				dates.Add(startDate.AddDays(i).ToString("dd/MM/yyyy"));
			}
			return dates.ToArray();
		}

		private static DateTime MaxDate(DateTime d1, DateTime d2)
		{
			if (d1 > d2) return d1;
			else return d2;
		}

		public static string GetNewCusts(DateTime minDate)
		{
			return DBAccess.GetStringsWithQuery($"SELECT Count([Client ID]) FROM [Client] WHERE [Client].[Client Join Date] BETWEEN '{minDate:yyyy-MM-dd}' AND '{DateTime.Now:yyyy-MM-dd}';")[0];
		}

		public static double GetIncomeSinceDate(DateTime minDate, DateTime maxDate)
		{
			// TODO: Is appointment initial, discount based on booking count
			string query = "SELECT " +
			"CASE " + 
				"WHEN [Appointment Type ID] = 0 THEN 35 " +
				"WHEN [Appointment Type ID] = 1 THEN 40 " +
				"WHEN [Appointment Type ID] = 2 THEN 50 " +
				"ELSE 0 " +
			"END, " +
			"CASE " +
				"WHEN [Includes Nail And Teeth] = 'True' THEN 10 " +
				"ELSE 0 " +
			"END " +
			$"FROM [Appointment] WHERE [Is Paid] = 1 AND [Appointment Date] BETWEEN '{minDate:yyyy-MM-dd}' AND '{maxDate:yyyy-MM-dd}';";
			List<List<string>> allPriceData = DBAccess.GetListStringsWithQuery(query);
			double income = 0;
			foreach (List<string> appData in allPriceData)
			{
				double appIncome = Convert.ToDouble(appData[0]);
				appIncome += Convert.ToDouble(appData[1]);
				//appIncome = appIncome * (100.0 - GetBookingDiscount(appData[2]));
				income += appIncome;

				//income += CalculateAppointmentPrice(app.ToArray());
			}
			return Math.Round(income, 2);
		}
	}
}
