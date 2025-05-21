using System;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Security.Cryptography;
using System.Text;

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
			bool dbExists = File.Exists("DogCareDB.sqlite");
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
				if (!dbExists)
				{
					CreateTablesIfNotExists();
					InsertInitialData();
				}
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

		public void InsertInitialData()
		{
			// Ensure connection is open
			if (Conn == null || Conn.State != System.Data.ConnectionState.Open)
			{
				Console.WriteLine("Connection is not open. Cannot insert initial data.");
				// Optionally, throw an exception or attempt to open the connection.
				// For this context, assuming Connect() has already been called and succeeded.
				return;
			}

			string adminUsername = "admin";
			string password = "password123"; // Default password, should be changed by user

			// Generate Salt
			byte[] salt = new byte[16];
			RandomNumberGenerator.Fill(salt);
			string saltString = Convert.ToBase64String(salt);

			// Hash Password
			var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
			byte[] hash = pbkdf2.GetBytes(20); // Using 20 bytes for the hash as per example
			string hashString = Convert.ToBase64String(hash);

			// Insert admin user into Staff table
			// StaffID is INTEGER PRIMARY KEY, so it should auto-increment.
			string sql = $"INSERT INTO Staff (StaffName, PasswordHash, Salt, StaffEmail, StaffPhoneNo, Uses2FA) VALUES (@StaffName, @PasswordHash, @Salt, @StaffEmail, @StaffPhoneNo, @Uses2FA);";

			using (var command = Conn.CreateCommand())
			{
				command.CommandText = sql;
				command.Parameters.AddWithValue("@StaffName", adminUsername);
				command.Parameters.AddWithValue("@PasswordHash", hashString);
				command.Parameters.AddWithValue("@Salt", saltString);
				command.Parameters.AddWithValue("@StaffEmail", "admin@example.com"); // Default email
				command.Parameters.AddWithValue("@StaffPhoneNo", "0000000000");    // Default phone
				command.Parameters.AddWithValue("@Uses2FA", 0);                   // Default 2FA status
				command.ExecuteNonQuery();
			}

			// Default Appointment Types
			string sqlAppType = "INSERT INTO AppointmentTypes (Description, BasePrice, DefaultDurationHours) VALUES (@Description, @BasePrice, @DefaultDurationHours);";

			// Type 1
			using (var command = Conn.CreateCommand())
			{
				command.CommandText = sqlAppType;
				command.Parameters.AddWithValue("@Description", "Standard Wash");
				command.Parameters.AddWithValue("@BasePrice", 25.0);
				command.Parameters.AddWithValue("@DefaultDurationHours", 1.0);
				command.ExecuteNonQuery();
			}

			// Type 2
			using (var command = Conn.CreateCommand())
			{
				command.CommandText = sqlAppType; // Re-use the same SQL string
				command.Parameters.Clear(); // Clear previous parameters
				command.Parameters.AddWithValue("@Description", "Wash and Trim");
				command.Parameters.AddWithValue("@BasePrice", 35.0);
				command.Parameters.AddWithValue("@DefaultDurationHours", 1.5);
				command.ExecuteNonQuery();
			}

			// Type 3
			using (var command = Conn.CreateCommand())
			{
				command.CommandText = sqlAppType;
				command.Parameters.Clear();
				command.Parameters.AddWithValue("@Description", "Full Groom");
				command.Parameters.AddWithValue("@BasePrice", 50.0);
				command.Parameters.AddWithValue("@DefaultDurationHours", 2.0);
				command.ExecuteNonQuery();
			}

			// Type 4
			using (var command = Conn.CreateCommand())
			{
				command.CommandText = sqlAppType;
				command.Parameters.Clear();
				command.Parameters.AddWithValue("@Description", "Puppy Groom (intro)");
				command.Parameters.AddWithValue("@BasePrice", 20.0);
				command.Parameters.AddWithValue("@DefaultDurationHours", 1.0);
				command.ExecuteNonQuery();
			}
		}
	}
}
