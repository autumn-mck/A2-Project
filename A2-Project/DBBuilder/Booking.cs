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

		public Microsoft.Data.Sqlite.SqliteCommand ToSqliteCommand(Microsoft.Data.Sqlite.SqliteConnection connection)
		{
			var command = connection.CreateCommand();
			command.CommandText = @"
                INSERT INTO Booking (BookingID, BookingDateMade)
                VALUES ($bookingID, $bookingDateMade);
            ";
			// The prompt mentions ClientID and TotalCost for Booking, but these are not properties of the Booking class.
			// I will only insert the properties that exist in the class.
			command.Parameters.AddWithValue("$bookingID", BookingID);
			command.Parameters.AddWithValue("$bookingDateMade", BookingDateMade.ToString("yyyy-MM-dd"));
			return command;
		}
	}
}
