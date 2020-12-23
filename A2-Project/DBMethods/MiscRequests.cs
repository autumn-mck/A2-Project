using System;
using System.Collections.Generic;

namespace A2_Project.DBMethods
{
	public static class MiscRequests
	{
		public static List<List<string>> GetAllAppointmentsOnDay(DateTime day)
		{
			return DBAccess.GetListStringsWithQuery("SELECT * FROM Appointment WHERE CONVERT(DATE, AppointmentDateTime) = '" + day.ToString("yyyy-MM-dd") + "';");
		}

		public static List<List<string>> GetContactsByClientID(string clientID)
		{
			return DBAccess.GetListStringsWithQuery("SELECT * FROM [Contact] WHERE ClientID = " + clientID + ";");
		}

		public static bool DoesMeetForeignKeyReq(DBObjects.ForeignKey fKey, string data)
		{
			return DBAccess.GetStringsWithQuery($"SELECT COUNT({fKey.ReferencedColumn}) FROM [{fKey.ReferencedTable}] WHERE {fKey.ReferencedColumn} = '{data}';")[0] != "0";
		}
	}
}
