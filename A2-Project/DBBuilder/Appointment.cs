using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2_Project.DBBuilder
{
	public class Appointment
	{
		public Appointment(int appointmentID, int dogID, int appointmentTypeID,
		int staffID, int bookingID, int groomingRoomID,
		bool includesNailAndTeeth, bool isCancelled, bool isPaid,
		DateTime appointmentDate, TimeSpan appointmentTime, bool isInital)
		{
			AppointmentID = appointmentID;
			DogID = dogID;
			AppointmentTypeID = appointmentTypeID;
			StaffID = staffID;
			BookingID = bookingID;
			GroomingRoomID = groomingRoomID;

			IncludesNailAndTeeth = includesNailAndTeeth;
			IsCancelled = isCancelled;
			IsPaid = isPaid;

			AppointmentDate = appointmentDate;
			AppointmentTime = appointmentTime;

			IsInitial = isInital;
		}

		public int AppointmentID { get; set; }
		public int DogID { get; set; }
		public int AppointmentTypeID { get; set; }
		public int StaffID { get; set; }
		public int BookingID { get; set; }
		public int GroomingRoomID { get; set; }

		public bool IncludesNailAndTeeth { get; set; }
		public bool IsCancelled { get; set; }
		public bool IsPaid { get; set; }

		public bool IsInitial { get; set; }

		public DateTime AppointmentDate { get; set; }
		public TimeSpan AppointmentTime { get; set; }

		public string ToSQL()
		{
			return $"INSERT INTO [Appointment] VALUES ({AppointmentID}, {DogID}, {AppointmentTypeID}, {StaffID}, {BookingID}, {GroomingRoomID}, " +
			$"{BoolAsOneOrZero(IncludesNailAndTeeth)}, {BoolAsOneOrZero(IsCancelled)}, {BoolAsOneOrZero(IsPaid)}, " +
			$"'{AppointmentDate:yyyy-MM-dd}', '{AppointmentTime:hh\\:mm}'); ";
		}

		public Microsoft.Data.Sqlite.SqliteCommand ToSqliteCommand(Microsoft.Data.Sqlite.SqliteConnection connection)
		{
			var command = connection.CreateCommand();
			command.CommandText = @"
                INSERT INTO Appointment (AppointmentID, DogID, AppointmentTypeID, StaffID, BookingID, GroomingRoomID, IncludesNailAndTeeth, IsCancelled, IsPaid, AppointmentDateTime, IsInitial)
                VALUES ($appointmentID, $dogID, $appointmentTypeID, $staffID, $bookingID, $groomingRoomID, $includesNailAndTeeth, $isCancelled, $isPaid, $appointmentDateTime, $isInitial);
            ";
			command.Parameters.AddWithValue("$appointmentID", AppointmentID);
			command.Parameters.AddWithValue("$dogID", DogID);
			command.Parameters.AddWithValue("$appointmentTypeID", AppointmentTypeID);
			command.Parameters.AddWithValue("$staffID", StaffID);
			command.Parameters.AddWithValue("$bookingID", BookingID);
			command.Parameters.AddWithValue("$groomingRoomID", GroomingRoomID);
			command.Parameters.AddWithValue("$includesNailAndTeeth", IncludesNailAndTeeth ? 1 : 0);
			command.Parameters.AddWithValue("$isCancelled", IsCancelled ? 1 : 0);
			command.Parameters.AddWithValue("$isPaid", IsPaid ? 1 : 0);
			
			// Combine Date and Time into a single DateTime object for SQLite
			DateTime appointmentDateTime = AppointmentDate.Date + AppointmentTime;
			command.Parameters.AddWithValue("$appointmentDateTime", appointmentDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
			
			command.Parameters.AddWithValue("$isInitial", IsInitial ? 1 : 0); // Assuming IsInitial is the C# property for IsInitialAppointment
			return command;
		}

		private static string BoolAsOneOrZero(bool eval)
		{
			if (eval) return "'1'";
			else return "'0'";
		}
	}
}
