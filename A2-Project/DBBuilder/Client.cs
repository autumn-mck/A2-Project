using System;
using System.Collections.Generic;

namespace A2_Project.DBBuilder
{
	public class Client
	{
		public Client(int clientID, string clientNotes, DateTime joinDate, Random random)
		{
			ClientID = clientID;
			ClientNotes = clientNotes;
			JoinDate = joinDate;

			IsReturn = RandGen.ShouldBeReturnClient();

			Contacts = new List<Contact>();
			int contactsCount = random.Next(1, 4);
			string surname = RandGen.GetRandomSurname();
			string address = RandGen.GenAddress();
			string town = RandGen.GenTown();
			string county = RandGen.GenCounty();
			string postcode = RandGen.GenPostcode();
			for (int i = 0; i < contactsCount; i++)
			{
				string forename = RandGen.GetRandomForename();
				Contacts.Add(new Contact(AllData.GetNextContactID() + Contacts.Count, ClientID, forename + " " + surname, RandGen.GenEmail(forename, surname).ToLower(), address, county, town, postcode, RandGen.GenPhoneNo()));
			}

			// TODO: Could improve over just generating between 1 and 3
			Dogs = new List<Dog>();
			int dogCount = random.Next(1, 4);
			for (int i = 0; i < dogCount; i++)
			{
				string dogGender;
				if (random.Next(2) == 0) dogGender = "M";
				else dogGender = "F";

				Dogs.Add(new Dog(AllData.GetNextDogID() + Dogs.Count, ClientID, RandGen.GenDogName(dogGender), RandGen.GenDogAge(JoinDate), RandGen.GenDogType(), dogGender));
			}
			AllData.Dogs.AddRange(Dogs);
		}

		public List<Dog> Dogs { get; set; }
		public List<Contact> Contacts { get; set; }
		public int ClientID { get; set; }
		public string ClientNotes { get; set; }
		public DateTime JoinDate { get; set; }
		public bool PrefersWeekends { get; set; }
		public bool IsReturn { get; set; }

		public override string ToString()
		{
			string toReturn = String.Format("Client ID {0}\n", ClientID);
			foreach (Contact c in Contacts)
			{
				toReturn += $"\nContact ID: {c.ContactID}\nContact Name: {c.ContactName}\nClient Email: {c.ContactEmail}\nClient Address: {c.ContactAddress}\nPhone No: {c.ContactPhoneNo}\n";
			}

			foreach (Dog d in Dogs)
			{
				toReturn += $"\nDog ID: {d.DogID}\nDog Name: {d.DogName}\nDog Gender: {d.DogGender}\nDog DOB: {d.DogDOB:dd/MM/yyyy}\n";
			}
			return toReturn;
		}

		public string ToSQL()
		{
			return $"INSERT INTO [Client] VALUES ({ClientID}, '{ClientNotes}', '{JoinDate:yyyy-MM-dd}'); ";
		}
	}
}
