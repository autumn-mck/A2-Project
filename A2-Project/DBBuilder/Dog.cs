using System;

namespace A2_Project.DBBuilder
{
	public class Dog
	{
		public Dog(int dogID, int clientID, string dogName, DateTime dogDOB, string dogType, string dogGender)
		{
			DogID = dogID;
			ClientID = clientID;
			DogName = dogName;
			DogDOB = dogDOB;
			DogType = dogType;
			DogGender = dogGender;
			DogNotes = "";
		}

		public int DogID { get; set; }
		public int ClientID { get; set; }
		public string DogName { get; set; }
		public DateTime DogDOB { get; set; }
		public string DogType { get; set; }
		public string DogGender { get; set; }
		public string DogNotes { get; set; }
		public Appointment LastApp { get; set; }

		public string ToSQL()
		{
			return $"INSERT INTO [Dog] VALUES ({DogID}, {ClientID}, '{DogName}', '{DogDOB:yyyy-MM-dd}', '{DogGender}', '{DogType}', '{DogNotes}'); ";
		}

		public Microsoft.Data.Sqlite.SqliteCommand ToSqliteCommand(Microsoft.Data.Sqlite.SqliteConnection connection)
		{
			var command = connection.CreateCommand();
			command.CommandText = @"
                INSERT INTO Dog (DogID, ClientID, DogName, DogDOB, DogGender, DogType, DogNotes)
                VALUES ($dogID, $clientID, $dogName, $dogDOB, $dogGender, $dogType, $dogNotes);
            ";
			command.Parameters.AddWithValue("$dogID", DogID);
			command.Parameters.AddWithValue("$clientID", ClientID);
			command.Parameters.AddWithValue("$dogName", DogName);
			command.Parameters.AddWithValue("$dogDOB", DogDOB.ToString("yyyy-MM-dd"));
			command.Parameters.AddWithValue("$dogGender", DogGender);
			command.Parameters.AddWithValue("$dogType", DogType ?? (object)DBNull.Value); // Assuming DogType can be null
			command.Parameters.AddWithValue("$dogNotes", DogNotes ?? (object)DBNull.Value); // Assuming DogNotes can be null
			return command;
		}
	}
}
