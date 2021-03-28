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

		private static string BoolAsOneOrZero(bool eval)
		{
			if (eval) return "'1'";
			else return "'0'";
		}
	}
}
