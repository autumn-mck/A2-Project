using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace A2_Project.DBBuilder
{
	public static class AllData
	{
		private static List<Client> clients = new List<Client>();
		private static List<Appointment> appointments = new List<Appointment>();
		private static List<Booking> bookings = new List<Booking>();
		private static List<Dog> dogs = new List<Dog>();

		public static List<Client> Clients
		{
			get { return clients; }
			set { clients = value; }
		}

		public static List<Appointment> Appointments
		{
			get { return appointments; }
			set { appointments = value; }
		}

		public static List<Booking> Bookings
		{
			get { return bookings; }
			set { bookings = value; }
		}

		public static List<Dog> Dogs
		{
			get { return dogs; }
			set { dogs = value; }
		}

		internal static void Reset()
		{
			clients = new List<Client>();
			appointments = new List<Appointment>();
			bookings = new List<Booking>();
			dogs = new List<Dog>();
		}

		/// <summary>
		/// Note: This method currently assumes that no accounts are deleted.
		/// </summary>
		public static int GetNextContactID()
		{
			int total = 0;
			foreach (Client cl in clients)
				total += cl.Contacts.Count;
			return total;
		}

		public static int GetNextDogID()
		{
			int id = -1;
			foreach (Dog d in dogs)
				id = Math.Max(id, d.DogID);

			return id + 1;
		}

		public static void WriteSQLToFile()
		{
			FileStream s = new FileStream(Directory.GetCurrentDirectory() + "\\output.txt", FileMode.Create);
			StreamWriter sw = new StreamWriter(s);
			foreach (Client c in clients)
			{
				sw.WriteLine(c.ToSQL());
				foreach (Contact co in c.Contacts) sw.WriteLine(co.ToSQL());
				foreach (Dog d in c.Dogs) sw.WriteLine(d.ToSQL());
			}

			foreach (Booking b in Bookings)
			{
				sw.WriteLine(b.ToSQL());
				foreach (Appointment a in b.Appointments) sw.WriteLine(a.ToSQL());
			}
			sw.Close();
		}

		public static string GetSQL()
		{
			StringBuilder builder = new StringBuilder();

			foreach (Client c in clients)
			{
				builder.Append(c.ToSQL());
				foreach (Contact co in c.Contacts) builder.Append(co.ToSQL());
				foreach (Dog d in c.Dogs) builder.Append(d.ToSQL());
			}

			foreach (Booking b in Bookings)
			{
				builder.Append(b.ToSQL());
				foreach (Appointment a in b.Appointments) builder.Append(a.ToSQL());
			}

			FileStream s = new FileStream(Directory.GetCurrentDirectory() + "\\output.txt", FileMode.Create);
			StreamWriter sw = new StreamWriter(s);
			sw.Write(builder);
			sw.Close();

			return builder.ToString();
		}
	}
}
