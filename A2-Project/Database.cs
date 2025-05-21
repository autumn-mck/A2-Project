using System;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace A2_Project
{
	/// <summary>
	/// Represents the database itself
	/// </summary>
	public class Database
	{
		public SqliteCommand Cmd { get; set; }
		public SqliteConnection Conn { get; set; }
		public SqliteDataReader Rdr { get; set; }

		public bool Connect()
		{
			SqliteConnectionStringBuilder scStrBuild = new SqliteConnectionStringBuilder
			{
				DataSource = "DogCareDB.sqlite"
			};
			string scStr = scStrBuild.ToString();
			Conn = new SqliteConnection(scStr);
			// Try to connect to the database. If a connection cannot be made, something has probably gone badly wrong
			// Note: The connection seems to fail on some machines without visual studio installed. Further testing needed.
			try
			{
				Conn.Open();
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				return false;
			}
		}

		public void Close()
		{
			Conn.Close();
		}

		public void CreateTablesIfNotExists()
		{
			if (Conn == null || Conn.State != System.Data.ConnectionState.Open)
			{
				// Or throw an exception, or log an error
				Console.WriteLine("Connection is not open. Cannot create tables.");
				return;
			}

			string createClientsTable = @"
            CREATE TABLE IF NOT EXISTS Clients (
                ClientID INTEGER PRIMARY KEY AUTOINCREMENT,
                ClientNotes TEXT,
                JoinDate TEXT,
                PrefersWeekends INTEGER,
                IsReturn INTEGER
            );";

			string createDogsTable = @"
            CREATE TABLE IF NOT EXISTS Dogs (
                DogID INTEGER PRIMARY KEY AUTOINCREMENT,
                ClientID INTEGER,
                DogName TEXT,
                DogDOB TEXT,
                DogGender TEXT,
                DogType TEXT,
                DogNotes TEXT,
                FOREIGN KEY (ClientID) REFERENCES Clients(ClientID)
            );";

			string createContactsTable = @"
            CREATE TABLE IF NOT EXISTS Contacts (
                ContactID INTEGER PRIMARY KEY AUTOINCREMENT,
                ClientID INTEGER,
                ContactName TEXT,
                ContactEmail TEXT,
                ContactAddress TEXT,
                ContactCounty TEXT,
                ContactTown TEXT,
                ContactPostcode TEXT,
                ContactPhoneNo TEXT,
                FOREIGN KEY (ClientID) REFERENCES Clients(ClientID)
            );";

			string createBookingsTable = @"
            CREATE TABLE IF NOT EXISTS Bookings (
                BookingID INTEGER PRIMARY KEY AUTOINCREMENT,
                BookingDateMade TEXT
            );";

			string createAppointmentsTable = @"
            CREATE TABLE IF NOT EXISTS Appointments (
                AppointmentID INTEGER PRIMARY KEY AUTOINCREMENT,
                DogID INTEGER,
                AppointmentTypeID INTEGER,
                StaffID INTEGER,
                BookingID INTEGER,
                GroomingRoomID INTEGER,
                IncludesNailAndTeeth INTEGER,
                IsCancelled INTEGER,
                IsPaid INTEGER,
                AppointmentDateTime TEXT,
                IsInitial INTEGER,
                FOREIGN KEY (DogID) REFERENCES Dogs(DogID),
                FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID)
            );";
            // FOREIGN KEYs for AppointmentTypeID, StaffID, GroomingRoomID will be added later

			string createStaffTable = @"
            CREATE TABLE IF NOT EXISTS Staff (
                StaffID INTEGER PRIMARY KEY,
                StaffName TEXT UNIQUE NOT NULL,
                PasswordHash TEXT NOT NULL,
                Salt TEXT NOT NULL,
                StaffEmail TEXT,
                StaffPhoneNo TEXT,
                Uses2FA INTEGER NOT NULL DEFAULT 0
            );";

			string createAppointmentTypesTable = @"
            CREATE TABLE IF NOT EXISTS AppointmentTypes (
                AppointmentTypeID INTEGER PRIMARY KEY,
                Description TEXT,
                BasePrice REAL,
                DefaultDurationHours REAL
            );";
            // TODO: Populate this table with initial data if necessary after creation.

			string createShiftsTable = @"
            CREATE TABLE IF NOT EXISTS Shifts (
                ShiftID INTEGER PRIMARY KEY AUTOINCREMENT,
                StaffID INTEGER,
                ShiftDay INTEGER, -- 0-6 for Sunday-Saturday, consistent with strftime('%w')
                ShiftStartTime TEXT, -- HH:MM
                ShiftEndTime TEXT,   -- HH:MM
                FOREIGN KEY (StaffID) REFERENCES Staff(StaffID)
            );";

			string createShiftExceptionsTable = @"
            CREATE TABLE IF NOT EXISTS ShiftExceptions (
                ShiftExceptionID INTEGER PRIMARY KEY AUTOINCREMENT,
                StaffID INTEGER,
                StartDate TEXT, -- YYYY-MM-DD
                EndDate TEXT,   -- YYYY-MM-DD
                Description TEXT,
                FOREIGN KEY (StaffID) REFERENCES Staff(StaffID)
            );";

			using (var command = Conn.CreateCommand())
			{
				command.CommandText = createClientsTable;
				command.ExecuteNonQuery();
				command.CommandText = createDogsTable;
				command.ExecuteNonQuery();
				command.CommandText = createContactsTable;
				command.ExecuteNonQuery();
				command.CommandText = createBookingsTable;
				command.ExecuteNonQuery();
				command.CommandText = createAppointmentsTable;
				command.ExecuteNonQuery();
				command.CommandText = createStaffTable;
				command.ExecuteNonQuery();
				command.CommandText = createAppointmentTypesTable;
				command.ExecuteNonQuery();
				command.CommandText = createShiftsTable;
				command.ExecuteNonQuery();
				command.CommandText = createShiftExceptionsTable;
				command.ExecuteNonQuery();
			}
		}
	}
}
