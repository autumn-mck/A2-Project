using System;
using System.Collections.Generic;

namespace A2_Project.DBBuilder
{
	public class Booking
	{
		public Booking(int bookingID, DateTime bookingDateMade)
		{
			BookingID = bookingID;
			BookingDateMade = bookingDateMade;
			Appointments = new List<Appointment>();
		}

		public int BookingID { get; set; }
		public DateTime BookingDateMade { get; set; }
		public List<Appointment> Appointments { get; set; }

		public string ToSQL()
		{
			return $"INSERT INTO [Booking] VALUES ({BookingID}, '{BookingDateMade:yyyy-MM-dd}'); ";
		}
	}
}
