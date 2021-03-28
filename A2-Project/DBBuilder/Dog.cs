using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		}

		public int DogID { get; set; }
		public int ClientID { get; set; }
		public string DogName { get; set; }
		public DateTime DogDOB { get; set; }
		public string DogType { get; set; }
		public string DogGender { get; set; }
		public Appointment LastApp { get; set; }

		public string ToSQL()
		{
			return String.Format("INSERT INTO [Dog] VALUES ({0}, {1}, '{2}', '{3}', '{4}', '{5}'); ", DogID, ClientID, DogName, DogDOB.ToString("yyyy-MM-dd"), DogGender, DogType);
		}
	}
}
