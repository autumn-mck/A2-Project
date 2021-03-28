using System;
using System.Collections.Generic;
using System.Linq;

namespace A2_Project.DBMethods
{
	public static class GraphingRequests
	{
		private static readonly string[] months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
		private static readonly List<List<string>> appTypeData = MetaRequests.GetAllFromTable("Appointment Type");

		public static void GetCountOfAppointmentTypes(ref double[][] data, ref string[] headers, DateTime minDate)
		{
			string dataQuery = "SELECT Count([Appointment Type ID]) FROM [Appointment] " +
			$"WHERE [Appointment Date] BETWEEN '{minDate:yyyy-MM-dd}' AND '{DateTime.Now:yyyy-MM-dd}' " +
			"GROUP BY [Appointment Type ID] ORDER BY [Appointment Type ID];";
			data[0] = DBAccess.GetStringsWithQuery(dataQuery).Select(double.Parse).ToArray();
			headers = DBAccess.GetStringsWithQuery("SELECT [Description] FROM [Appointment Type] ORDER BY [Appointment Type ID];").ToArray();
		}

		public static void GetBusinessOfStaff(ref double[][] data, ref string[] headers, DateTime minDate)
		{
			string dataQuery = "SELECT Count([Staff ID]) FROM [Appointment] " +
			$"WHERE [Appointment Date] BETWEEN '{minDate:yyyy-MM-dd}' AND '{DateTime.Now:yyyy-MM-dd}' " +
			"GROUP BY [Staff ID] ORDER BY [Staff ID];";
			data[0] = DBAccess.GetStringsWithQuery(dataQuery).Select(double.Parse).ToArray();
			headers = DBAccess.GetStringsWithQuery("SELECT [Staff Name] FROM [Staff] ORDER BY [Staff ID];").ToArray();
		}

		/// <summary>
		/// Gets the number of clients over time
		/// </summary>
		public static void GetGrowthOverTime(ref double[][] data, ref string[] headers, DateTime minDate)
		{
			DateTime startDate = MaxDate(Convert.ToDateTime(DBAccess.GetStringsWithQuery("SELECT MIN([Client Join Date]) FROM [Client]")[0]), minDate);
			DateTime endDate = DateTime.Now.Date;
			int diff = (int)(endDate - startDate).TotalDays;
			List<double> growth = new List<double>();
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
		public static void GetAppsByDayOfWeek(ref double[][] data, ref string[] headers, DateTime minDate)
		{
			string query = "SET DATEFIRST 1; SELECT Count([Appointment ID]) FROM [Appointment] " +
			$"WHERE [Appointment Date] BETWEEN '{minDate:yyyy-MM-dd}' AND '{DateTime.Now:yyyy-MM-dd}' " +
			"GROUP BY DatePart(WeekDay, [Appointment Date]) ORDER BY DatePart(WeekDay, [Appointment Date]);";
			data[0] = DBAccess.GetStringsWithQuery(query).Select(x => Convert.ToDouble(x)).ToArray();
			headers = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
		}

		/// <summary>
		/// Gets the number of appointments in each month of the last year
		/// </summary>
		public static void GetBookingsInMonths(ref double[][] data, ref string[] headers, DateTime minDate)
		{
			string query = "SELECT Count([Appointment ID]) FROM [Appointment] " +
			$"WHERE [Appointment Date] BETWEEN '{DateTime.Now.AddYears(-1):yyyy-MM-dd}' AND '{DateTime.Now:yyyy-MM-dd}' " +
			"GROUP BY DatePart(Month, [Appointment Date]) ORDER BY DatePart(Month, [Appointment Date]);";
			data[0] = DBAccess.GetStringsWithQuery(query).Select(x => Convert.ToDouble(x)).ToArray();
			headers = months;
		}

		/// <summary>
		/// Gets a rolling average of what % of appointments have been cancelled over time
		/// </summary>
		public static void GetAppCancelRate(ref double[][] data, ref string[] headers, DateTime minDate)
		{
			DateTime startDate = MaxDate(Convert.ToDateTime(DBAccess.GetStringsWithQuery("SELECT MIN([Appointment Date]) FROM Appointment")[0]), minDate);
			DateTime endDate = DateTime.Now;
			int diff = (int)(endDate - startDate).TotalDays;
			double increment = diff / 75.0;
			List<double> cancelRate = new List<double>();
			for (double i = 0; i < diff; i += increment)
			{
				DateTime currentDate = startDate.AddDays(i);

				string totalInTimeQuery = "SELECT COUNT([Appointment ID]) FROM [Appointment] " +
				$"WHERE [Appointment Date] BETWEEN '{currentDate.AddDays(-increment * 10):yyyy-MM-dd}' AND '{currentDate:yyyy-MM-dd}';";
				double totalInTime = Convert.ToInt32(DBAccess.GetStringsWithQuery(totalInTimeQuery)[0]);

				string cancelledInTimeQuery = "SELECT COUNT([Appointment ID]) FROM [Appointment] " +
				$"WHERE [Cancelled] = 1 AND [Appointment Date] BETWEEN '{currentDate.AddDays(-increment * 10):yyyy-MM-dd}' AND '{currentDate:yyyy-MM-dd}';";
				double cancelledInTime = Convert.ToInt32(DBAccess.GetStringsWithQuery(cancelledInTimeQuery)[0]);
				if (cancelledInTime == 0 || totalInTime == 0)
					cancelRate.Add(0);
				else cancelRate.Add(cancelledInTime * 100 / totalInTime);
			}
			data[0] = cancelRate.ToArray();
			headers = InterpolateDates(startDate, diff);
		}

		// No longer used, but kept around as it could be useful some day.
		public static void GetCustReturns(ref double[][] data, ref string[] headers, DateTime minDate)
		{
			DateTime startDate = MaxDate(Convert.ToDateTime(DBAccess.GetStringsWithQuery("SELECT MIN([Client Join Date]) FROM [Client]")[0]), minDate);
			DateTime endDate = DateTime.Now;
			double diff = (endDate - startDate).TotalDays;
			List<double> returns = new List<double>();
			int count = 0;
			for (double i = 0; i < diff; i += diff / 40.0)
			{
				string query = "SELECT b.[Appointment Date] FROM [Appointment] AS a " +
				"CROSS APPLY (SELECT TOP 1 [Appointment Date] From [Appointment] WHERE [Dog ID] = a.[Dog ID] ORDER BY [Appointment Date] desc) as b " +
				$"WHERE b.[Appointment Date] BETWEEN '{startDate.Add(TimeSpan.FromDays(i - diff / 40.0)):yyyy-MM-dd}' AND '{startDate.Add(TimeSpan.FromDays(i)):yyyy-MM-dd}';";

				List<List<string>> result = DBAccess.GetListStringsWithQuery(query);

				returns.Add(result.Count);
				count++;
			}
			data[0] = returns.ToArray();
			headers = InterpolateDates(startDate, (int)diff);
		}

		public static void GetGrossProfitLastYear(ref double[][] data, ref string[] headers, DateTime minDate)
		{
			headers = new string[12];
			data[0] = new double[12];
			DateTime endDate = DateTime.Now.Date;
			DateTime startDate = endDate.AddMonths(-12);
			for (int i = 0; i < 12; i++)
			{
				headers[i] = months[startDate.AddMonths(i).Month - 1];
				// Note: Does not consider if is first booking and booking discount
				string query = "SELECT " +
				"CASE " +
					"WHEN [Appointment Type ID] = 0 THEN 35 " +
					"WHEN [Appointment Type ID] = 1 THEN 40 " +
					"WHEN [Appointment Type ID] = 2 THEN 50 " +
					"ELSE 0 " +
				"END, " +
				"CASE " +
					"WHEN [Nails And Teeth] = 'True' THEN 10 " +
					"ELSE 0 " +
				"END " +
				$"FROM [Appointment] WHERE [Paid] = 1 AND [Appointment Date] BETWEEN '{startDate.AddMonths(i):yyyy-MM-dd}' AND '{startDate.AddMonths(i + 1):yyyy-MM-dd}';";
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
				incomeFromMonth = (incomeFromMonth * 20 + 2500) * 1.3;
				data[0][i] = incomeFromMonth;
			}
		}

		public static void GetIncomeLastYear(ref double[][] data, ref string[] headers, DateTime minDate)
		{
			headers = new string[12];
			data[0] = new double[12];
			DateTime endDate = DateTime.Now.Date;
			DateTime startDate = endDate.AddMonths(-12);
			for (int i = 0; i < 12; i++)
			{
				headers[i] = months[startDate.AddMonths(i).Month - 1];
				// Note: Does not consider if is first booking and booking discount
				string query = "SELECT " +
				"CASE " +
					"WHEN [Appointment Type ID] = 0 THEN 35 " +
					"WHEN [Appointment Type ID] = 1 THEN 40 " +
					"WHEN [Appointment Type ID] = 2 THEN 50 " +
					"ELSE 0 " +
				"END, " +
				"CASE " +
					"WHEN [Nails And Teeth] = 'True' THEN 10 " +
					"ELSE 0 " +
				"END " +
				$"FROM [Appointment] WHERE [Paid] = 1 AND [Appointment Date] BETWEEN '{startDate.AddMonths(i):yyyy-MM-dd}' AND '{startDate.AddMonths(i + 1):yyyy-MM-dd}';";
				List<List<string>> dataFromMonth = DBAccess.GetListStringsWithQuery(query);
				double incomeFromMonth = 0;
				foreach (List<string> appData in dataFromMonth)
				{
					double appIncome = Convert.ToDouble(appData[0]);
					appIncome += Convert.ToDouble(appData[1]);
					//appIncome = appIncome * (100.0 - GetBookingDiscount(appData[2]));

					//income += CalculateAppointmentPrice(app.ToArray());
					incomeFromMonth += appIncome;
				}
				data[0][i] = Math.Round(incomeFromMonth);
			}
		}

		public static double GetBookingDiscount(string bookingID)
		{
			return Convert.ToInt32(DBAccess.GetStringsWithQuery($"SELECT CASE WHEN Count([Appointment ID]) > 2 THEN 5 ELSE 0 END FROM [Appointment] WHERE [Appointment].[Booking ID] = {bookingID};")[0]);
		}

		/// <summary>
		/// Returns 7 dates linearly between the startDate and end date.
		/// diff represents the difference between the start date and the end date in days
		/// </summary>
		private static string[] InterpolateDates(DateTime startDate, int diff)
		{
			List<string> dates = new List<string>();
			if (diff == 0) return new string[] { startDate.ToString("dd/MM/yyyy"), startDate.ToString("dd/MM/yyyy") };
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

		public static double GetIncomeSince(DateTime minDate, DateTime maxDate)
		{
			// Note: Does not take into account is appointment initial, discount based on booking count
			string query = "SELECT " +
			"CASE " + 
				"WHEN [Appointment Type ID] = 0 THEN 35 " +
				"WHEN [Appointment Type ID] = 1 THEN 40 " +
				"WHEN [Appointment Type ID] = 2 THEN 50 " +
				"ELSE 0 " +
			"END, " +
			"CASE " +
				"WHEN [Nails And Teeth] = 'True' THEN 10 " +
				"ELSE 0 " +
			"END " +
			$"FROM [Appointment] WHERE [Paid] = 1 AND [Appointment Date] BETWEEN '{minDate:yyyy-MM-dd}' AND '{maxDate:yyyy-MM-dd}';";
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
