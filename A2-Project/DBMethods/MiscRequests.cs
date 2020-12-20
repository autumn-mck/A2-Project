using System;
using System.Collections.Generic;

namespace A2_Project.DBMethods
{
	public static class MiscRequests
	{
		// TODO: AppointmentTime should be called DateTime
		public static List<List<string>> GetAllAppointmentsOnDay(DateTime day)
		{
			return DBAccess.GetListStringsWithQuery("SELECT * FROM Appointment WHERE CONVERT(DATE, AppointmentDateTime) = '" + day.ToString("yyyy-MM-dd") + "';");
		}

		public static List<List<string>> GetContactsByClientID(string clientID)
		{
			return DBAccess.GetListStringsWithQuery("SELECT * FROM [Contact] WHERE ClientID = " + clientID + ";");
		}
	}
}
