using System;
using System.Collections.Generic;
using System.Linq;

namespace A2_Project.DBMethods
{
	public static class GraphingRequests
	{
		private static readonly string[] months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

		public static void GetCountOfAppointmentTypes(ref double[][] data, ref string[] headers, DateTime minDate)
		{
			string dataQuery = "SELECT Count([Appointment Type ID]) FROM [Appointment] " +
			$"WHERE [Appointment Date] BETWEEN '{minDate:yyyy-MM-dd}' AND '{DateTime.Now:yyyy-MM-dd}' AND [Cancelled] = 0" +
			"GROUP BY [Appointment Type ID] ORDER BY [Appointment Type ID];";
			data[0] = DBAccess.GetStringsWithQuery(dataQuery).Select(double.Parse).ToArray();
			headers = DBAccess.GetStringsWithQuery("SELECT [Description] FROM [Appointment Type] ORDER BY [Appointment Type ID];").ToArray();
		}

		public static void GetBusinessOfStaff(ref double[][] data, ref string[] headers, DateTime minDate)
		{
			string dataQuery = "SELECT Count([Staff ID]) FROM [Appointment] " +
			$"WHERE [Appointment Date] BETWEEN '{minDate:yyyy-MM-dd}' AND '{DateTime.Now:yyyy-MM-dd}' AND [Cancelled] = 0 " +
			"GROUP BY [Staff ID] ORDER BY [Staff ID];";
			data[0] = DBAccess.GetStringsWithQuery(dataQuery).Select(double.Parse).ToArray();
			headers = DBAccess.GetStringsWithQuery("SELECT [Staff Name] FROM [Staff] ORDER BY [Staff ID];").ToArray();
		}

		/// <summary>
		/// Gets the number of clients over time
		/// </summary>
		public static void GetGrowthOverTime(ref double[][] data, ref string[] headers, DateTime minDate)
		{
			List<string> ls = DBAccess.GetStringsWithQuery("SELECT MIN([Client Join Date]) FROM [Client]");
			if (ls[0] == "") return;
			DateTime startDate = MaxDate(Convert.ToDateTime(ls[0]), minDate);
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
			// SQLite's strftime('%w', ...) gives Sunday=0, Monday=1, ..., Saturday=6
			// We want the output to correspond to Mon, Tue, ..., Sun
			// So we'll order by strftime('%w', ...) and adjust the header mapping if needed,
			// or adjust the value so Monday is 1, Sunday is 7 for ordering.
			// Let's make Monday=1, ..., Sunday=7 for ordering to match the header intent.
			// (strftime('%w', field) + 6) % 7 + 1 might work if Sunday is 0.
			// Or, more simply: CASE strftime('%w', field) WHEN '0' THEN '7' ELSE strftime('%w', field) END as WeekDaySortKey
			// The AppointmentDateTime field stores full date and time. We need to extract date part for this.
			// Assuming AppointmentDateTime is stored as 'YYYY-MM-DD HH:MM:SS'
			string query = "SELECT Count([AppointmentID]) FROM [Appointments] " + // Table name is Appointments (plural) based on CreateTablesIfNotExists
			$"WHERE date([AppointmentDateTime]) BETWEEN date('{minDate:yyyy-MM-dd}') AND date('{DateTime.Now:yyyy-MM-dd}') AND [IsCancelled] = 0 " + // IsCancelled based on schema
			"GROUP BY strftime('%w', [AppointmentDateTime]) ORDER BY CASE strftime('%w', [AppointmentDateTime]) WHEN '0' THEN 7 ELSE CAST(strftime('%w', [AppointmentDateTime]) AS INTEGER) END;";
			// The output will be ordered Sun (0), Mon (1), ..., Sat (6) if simply GROUP BY strftime('%w', ...).
			// If the above ORDER BY CASE works, it will be Mon, Tue, ..., Sun
			// The current headers are "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun".
			// If SQLite returns counts for Sunday (0) first, then Monday (1) etc., the C# code needs to handle this.
			// Let's try to get the order Mon-Sun from SQL.
			// Sunday=0, Monday=1, ..., Saturday=6.
			// ORDER BY (strftime('%w', [AppointmentDateTime]) + 6) % 7
			// This makes Monday=0, Tuesday=1, ..., Sunday=6. Then the headers array { "Mon", ..., "Sun" } would match.
			query = "SELECT Count([AppointmentID]) FROM [Appointments] " +
			$"WHERE date([AppointmentDateTime]) BETWEEN date('{minDate:yyyy-MM-dd}') AND date('{DateTime.Now:yyyy-MM-dd}') AND [IsCancelled] = 0 " +
			"GROUP BY strftime('%w', [AppointmentDateTime]) ORDER BY (strftime('%w', [AppointmentDateTime]) + 6) % 7;";

			data[0] = DBAccess.GetStringsWithQuery(query).Select(x => Convert.ToDouble(x)).ToArray();
			// Headers are already { "Mon", ..., "Sun" }. The query now returns counts ordered Monday to Sunday.
			headers = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
		}

		/// <summary>
		/// Gets the number of appointments in each month of the last year
		/// </summary>
		public static void GetBookingsInMonths(ref double[][] data, ref string[] headers, DateTime minDate)
		{
			// DatePart(Month, ...) becomes strftime('%m', ...)
			// Assuming AppointmentDateTime is 'YYYY-MM-DD HH:MM:SS'
			string query = "SELECT Count([AppointmentID]) FROM [Appointments] " + // Table name is Appointments
			$"WHERE date([AppointmentDateTime]) BETWEEN date('{DateTime.Now.AddYears(-1):yyyy-MM-dd}') AND date('{DateTime.Now:yyyy-MM-dd}') AND [IsCancelled] = 0 " + // IsCancelled
			"GROUP BY strftime('%m', [AppointmentDateTime]) ORDER BY strftime('%m', [AppointmentDateTime]);";
			data[0] = DBAccess.GetStringsWithQuery(query).Select(x => Convert.ToDouble(x)).ToArray();
			headers = months;
		}

		/// <summary>
		/// Gets a rolling average of what % of appointments have been cancelled over time
		/// </summary>
		public static void GetAppCancelRate(ref double[][] data, ref string[] headers, DateTime minDate)
		{
			// Corrected table: Appointments, column: AppointmentDateTime, IsCancelled
			string ls = DBAccess.GetStringsWithQuery("SELECT MIN(date([AppointmentDateTime])) FROM Appointments")[0]; // Table name
			if (ls == "" || ls == null) return; // Handle case where table might be empty or MIN returns NULL
			DateTime startDate = MaxDate(Convert.ToDateTime(ls), minDate);
			DateTime endDate = DateTime.Now;
			int diff = (int)(endDate - startDate).TotalDays;
			if (diff <= 0) return; // Avoid division by zero or negative increment
			double increment = diff / 75.0;
			if (increment <= 0) increment = 1; // Ensure increment is positive
			
			List<double> cancelRate = new List<double>();
			for (double i = 0; i < diff; i += increment)
			{
				DateTime currentDate = startDate.AddDays(i);
				DateTime rangeStartDate = currentDate.AddDays(-increment * 10);

				string totalInTimeQuery = "SELECT COUNT([AppointmentID]) FROM [Appointments] " + // Table, Column
				$"WHERE date([AppointmentDateTime]) BETWEEN date('{rangeStartDate:yyyy-MM-dd}') AND date('{currentDate:yyyy-MM-dd}');"; // Column, date()
				string totalInTimeString = DBAccess.GetStringsWithQuery(totalInTimeQuery).FirstOrDefault();
				double totalInTime = string.IsNullOrEmpty(totalInTimeString) ? 0 : Convert.ToInt32(totalInTimeString);

				string cancelledInTimeQuery = "SELECT COUNT([AppointmentID]) FROM [Appointments] " + // Table, Column
				$"WHERE [IsCancelled] = 1 AND date([AppointmentDateTime]) BETWEEN date('{rangeStartDate:yyyy-MM-dd}') AND date('{currentDate:yyyy-MM-dd}');"; // Column, IsCancelled, date()
				string cancelledInTimeString = DBAccess.GetStringsWithQuery(cancelledInTimeQuery).FirstOrDefault();
				double cancelledInTime = string.IsNullOrEmpty(cancelledInTimeString) ? 0 : Convert.ToInt32(cancelledInTimeString);
				
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
			string minClientJoinDateStr = DBAccess.GetStringsWithQuery("SELECT MIN(date([JoinDate])) FROM [Clients]").FirstOrDefault();
			if (string.IsNullOrEmpty(minClientJoinDateStr)) return;
			DateTime startDate = MaxDate(Convert.ToDateTime(minClientJoinDateStr), minDate);
			DateTime endDate = DateTime.Now;
			double diff = (endDate - startDate).TotalDays;
			if (diff <= 0) return; // Avoid issues with loop/division by zero

			List<double> returns = new List<double>();
			double increment = diff / 40.0;
			if (increment <= 0) increment = 1; // Ensure positive increment

			for (double i = 0; i < diff; i += increment)
			{
				DateTime rangeStart = startDate.Add(TimeSpan.FromDays(i - increment));
				DateTime rangeEnd = startDate.Add(TimeSpan.FromDays(i));

				// Selects appointments that are the latest for their dog AND fall within the date range.
				string query = $"SELECT COUNT(DISTINCT b.DogID) FROM Appointments AS b WHERE b.AppointmentDateTime = (SELECT MAX(sub_b.AppointmentDateTime) FROM Appointments AS sub_b WHERE sub_b.DogID = b.DogID) AND date(b.AppointmentDateTime) BETWEEN date('{rangeStart:yyyy-MM-dd}') AND date('{rangeEnd:yyyy-MM-dd}');";
				
				string countStr = DBAccess.GetStringsWithQuery(query).FirstOrDefault();
				returns.Add(string.IsNullOrEmpty(countStr) ? 0 : Convert.ToInt32(countStr));
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
				DateTime curDate = startDate.AddMonths(i);
				headers[i] = months[curDate.Month - 1];
				// Note: Does not consider if is first booking and booking discount
				// Using schema: Appointments, AppointmentTypeID, IncludesNailAndTeeth, IsPaid, IsCancelled, AppointmentDateTime
				string query = "SELECT " +
				"CASE " +
					"WHEN [AppointmentTypeID] = 0 THEN 35 " + // Column name
					"WHEN [AppointmentTypeID] = 1 THEN 40 " + // Column name
					"WHEN [AppointmentTypeID] = 2 THEN 50 " + // Column name
					"ELSE 0 " +
				"END, " +
				"CASE " +
					"WHEN [IncludesNailAndTeeth] = 1 THEN 10 " + // Column name and value
					"ELSE 0 " +
				"END " +
				$"FROM [Appointments] WHERE [IsPaid] = 1 AND [IsCancelled] = 0 AND strftime('%m', date([AppointmentDateTime])) = '{curDate.Month:00}' AND strftime('%Y', date([AppointmentDateTime])) = '{curDate.Year}';"; // Table, Columns, date functions
				List<List<string>> dataFromMonth = DBAccess.GetListStringsWithQuery(query);
				double incomeFromMonth = 0;
				foreach (List<string> appData in dataFromMonth)
				{
					double basePrice = string.IsNullOrEmpty(appData[0]) ? 0 : Convert.ToDouble(appData[0]);
					double extrasPrice = string.IsNullOrEmpty(appData[1]) ? 0 : Convert.ToDouble(appData[1]);
					double appIncome = basePrice + extrasPrice;
					//appIncome = appIncome * (100.0 - GetBookingDiscount(appData[2])); // BookingID not selected

					//income += CalculateAppointmentPrice(app.ToArray());
					incomeFromMonth += appIncome - 43.4; // This 43.4 is a magic number, leaving as is.
				}
				incomeFromMonth = (incomeFromMonth * 15 + 4000) * 1.3; // This calculation is also magic, leaving as is.
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
				DateTime curDate = startDate.AddMonths(i);
				headers[i] = months[curDate.Month - 1];
				// Note: Does not consider if is first booking and booking discount
				// Using schema: Appointments, AppointmentTypeID, IncludesNailAndTeeth, IsPaid, IsCancelled, AppointmentDateTime
				string query = "SELECT " +
				"CASE " +
					"WHEN [AppointmentTypeID] = 0 THEN 35 " + // Column name
					"WHEN [AppointmentTypeID] = 1 THEN 40 " + // Column name
					"WHEN [AppointmentTypeID] = 2 THEN 50 " + // Column name
					"ELSE 0 " +
				"END, " +
				"CASE " +
					"WHEN [IncludesNailAndTeeth] = 1 THEN 10 " + // Column name and value
					"ELSE 0 " +
				"END " +
				$"FROM [Appointments] WHERE [IsPaid] = 1 AND [IsCancelled] = 0 AND strftime('%m', date([AppointmentDateTime])) = '{curDate.Month:00}' AND strftime('%Y', date([AppointmentDateTime])) = '{curDate.Year}';"; // Table, Columns, date functions
				List<List<string>> dataFromMonth = DBAccess.GetListStringsWithQuery(query);
				double incomeFromMonth = 0;
				foreach (List<string> appData in dataFromMonth)
				{
					double basePrice = string.IsNullOrEmpty(appData[0]) ? 0 : Convert.ToDouble(appData[0]);
					double extrasPrice = string.IsNullOrEmpty(appData[1]) ? 0 : Convert.ToDouble(appData[1]);
					double appIncome = basePrice + extrasPrice;
					//appIncome = appIncome * (100.0 - GetBookingDiscount(appData[2])); // BookingID not selected
					
					incomeFromMonth += appIncome;
				}
				data[0][i] = Math.Round(incomeFromMonth);
			}
		}

		public static double GetBookingDiscount(string bookingID)
		{
			// Table: Appointments, Column: AppointmentID, BookingID
			// Parameterize bookingID if possible, for now direct interpolation.
			string query = $"SELECT CASE WHEN Count([AppointmentID]) > 2 THEN 5 ELSE 0 END FROM [Appointments] WHERE [BookingID] = {bookingID};";
			string result = DBAccess.GetStringsWithQuery(query).FirstOrDefault();
			return string.IsNullOrEmpty(result) ? 0 : Convert.ToInt32(result);
		}

		/// <summary>
		/// Returns 7 dates linearly between the startDate and end date.
		/// diff represents the difference between the start date and the end date in days
		/// </summary>
		private static string[] InterpolateDates(DateTime startDate, int diff) // No SQL here
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
			// Table: Clients, Column: ClientID, JoinDate
			string query = $"SELECT Count([ClientID]) FROM [Clients] WHERE date([JoinDate]) BETWEEN date('{minDate:yyyy-MM-dd}') AND date('{DateTime.Now:yyyy-MM-dd}');";
			return DBAccess.GetStringsWithQuery(query).FirstOrDefault();
		}

		public static double GetIncomeSince(DateTime minDate, DateTime maxDate)
		{
			// Note: Does not take into account is appointment initial, discount based on booking count
			// Table: Appointments, Columns: AppointmentTypeID, IncludesNailAndTeeth, IsPaid, IsCancelled, AppointmentDateTime
			string query = "SELECT " +
			"CASE " + 
				"WHEN [AppointmentTypeID] = 0 THEN 35 " + // Column name
				"WHEN [AppointmentTypeID] = 1 THEN 40 " + // Column name
				"WHEN [AppointmentTypeID] = 2 THEN 50 " + // Column name
				"ELSE 0 " +
			"END, " +
			"CASE " +
				"WHEN [IncludesNailAndTeeth] = 1 THEN 10 " + // Column name and value
				"ELSE 0 " +
			"END " +
			$"FROM [Appointments] WHERE [IsPaid] = 1 AND [IsCancelled] = 0 AND date([AppointmentDateTime]) BETWEEN date('{minDate:yyyy-MM-dd}') AND date('{maxDate:yyyy-MM-dd}');"; // Table, Columns, date functions
			List<List<string>> allPriceData = DBAccess.GetListStringsWithQuery(query);
			double income = 0;
			foreach (List<string> appData in allPriceData)
			{
				double basePrice = string.IsNullOrEmpty(appData[0]) ? 0 : Convert.ToDouble(appData[0]);
				double extrasPrice = string.IsNullOrEmpty(appData[1]) ? 0 : Convert.ToDouble(appData[1]);
				double appIncome = basePrice + extrasPrice;
				//appIncome = appIncome * (100.0 - GetBookingDiscount(appData[2])); // BookingID not selected
				income += appIncome;
			}
			return Math.Round(income, 2);
		}
	}
}
