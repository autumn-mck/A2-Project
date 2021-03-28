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
	}
}
