using System;
using System.Collections.Generic;
using System.Linq;

namespace A2_Project.DBBuilder
{
	public class MainGeneration
	{
		private double currentCustCount = 1;
		private double prevCustCount = 1;
		private int daysPassed = 0;
		private Random random = new Random(0);
		private int[] appointCountOverTime = new int[2000];
		private readonly DateTime startTime = DateTime.Now.AddYears(-5);
		private DateTime currentSimDate;

		private double[] appLengths = new double[] { 1, 1.5, 2 , 1.5};
		private int[] roomIDs = new int[] { 0, 1, 2 };

		public MainGeneration()
		{
			RandGen.InitialiseDataFromFile();
		}

		public void Run(int seed)
		{
			currentCustCount = 1;
			prevCustCount = 1;
			daysPassed = 0;
			random = new Random(seed);
			appointCountOverTime = new int[2000];
			currentSimDate = startTime;

			RandGen.Reset(seed);
			AllData.Reset();

			for (int i = 0; i < 365 * 6; i++)
				SimulateDay();

		}

		public string GetSQL()
		{
			return AllData.GetSQL();
		}

		public void WriteToFile()
		{
			AllData.WriteSQLToFile();
		}

		private void SimulateDay()
		{
			if (currentSimDate > DateTime.Now) return;
			daysPassed++;
			currentSimDate = startTime.AddDays(daysPassed).Date;

			prevCustCount = currentCustCount;
			currentCustCount = 1030 / (1 + Math.Pow(Math.E, (-daysPassed + 2000) / 500f)) - 25;
			currentCustCount += 200 / (1 + Math.Pow(Math.E, (-daysPassed + 100) / 40f));
			currentCustCount += 200 / (1 + Math.Pow(Math.E, (-daysPassed + 600) / 50f));
			currentCustCount += 300 / (1 + Math.Pow(Math.E, (-daysPassed + 1300) / 70f));
			for (int i = 0; i < HowManyNewClients(); i++)
			{
				AddNewClient();
			}

			// Make some clients occasionally leave
			foreach (Client c in AllData.Clients.Where(c => c.IsReturn))
			{
				if (random.NextDouble() > 0.9999) c.IsReturn = false;
			}

			List<Appointment> appToday = AllData.Appointments.Where(a => a.AppointmentDate.Date == currentSimDate).ToList();
			foreach (Appointment a in appToday)
			{
				Booking booking = AllData.Bookings.Where(b => b.BookingID == a.BookingID).First();
				bool hasDogFinishedAppointments = a.AppointmentDate == booking.Appointments.Where(a => a.DogID == a.DogID).Max(a => a.AppointmentDate);
				bool haveAllDogsFinishedAppointments = a.AppointmentDate == booking.Appointments.Max(booking => booking.AppointmentDate);
				bool shouldBookAnyway = random.NextDouble() < 0.7;
				if (hasDogFinishedAppointments && (haveAllDogsFinishedAppointments || shouldBookAnyway))
				{
					// Book a new appointment
					Dog d = AllData.Dogs.Where(d => d.DogID == a.DogID).First();
					Client c = AllData.Clients.Where(c => c.ClientID == d.ClientID).First();
					if (!c.IsReturn) continue;

					List<Dog> toBookWith;
					if (!haveAllDogsFinishedAppointments)
					{
						toBookWith = c.Dogs.Where(d => booking.Appointments.Where(a => a.AppointmentDate < currentSimDate).Select(a => a.DogID).Contains(d.DogID)).ToList();
						if (toBookWith.Count < 2) continue;
					}
					else
						toBookWith = c.Dogs;

					MakeBooking(toBookWith);
				}
			}

			// Pay for some appointments
			foreach (Appointment a in AllData.Appointments.Where(a => !a.IsPaid && !a.IsCancelled && a.AppointmentDate < currentSimDate.AddDays(20)))
			{
				if (random.NextDouble() < 0.1) a.IsPaid = true;
			}

			appointCountOverTime[daysPassed] = AllData.Appointments.Where(a => a.AppointmentDate.Date == currentSimDate).ToList().Count;
		}

		private int HowManyNewClients()
		{
			return (int)(Math.Floor(currentCustCount) - Math.Floor(prevCustCount));
		}

		private void AddNewClient()
		{
			Client c = new Client(AllData.Clients.Count, "", currentSimDate, random);
			AllData.Clients.Add(c);
			MakeBooking(c.Dogs);
		}

		private void MakeBooking(List<Dog> dogs)
		{
			Booking booking = new Booking(AllData.Bookings.Count, currentSimDate);
			foreach (Dog d in dogs)
			{
				int appToMake;
				double rand = random.NextDouble();
				if (rand < 0.6) appToMake = 1;
				else if (rand < 0.9) appToMake = 2;
				else appToMake = 3;


				int prevDiff = 0;
				for (int i = 0; i < appToMake; i++)
				{
					int appTypeID = RandGen.GetRandAppTypeID();
					if (appTypeID == 3)
					{
						int count = random.Next(4, 6);
						List<Appointment> appts = new List<Appointment>();
						for (int j = 0; j < count; j++)
						{
							int diff = random.Next(15, 30);
							Appointment app = BookAppointment(d, booking.BookingID, prevDiff + j * diff, appTypeID);
							appts.Add(app);
							booking.Appointments.Add(app);
						}
						prevDiff = (int)(currentSimDate - appts[0].AppointmentDate).TotalDays;
					}
					else
					{
						Appointment app = BookAppointment(d, booking.BookingID, prevDiff, appTypeID);
						booking.Appointments.Add(app);
						prevDiff = (int)(currentSimDate - app.AppointmentDate).TotalDays;
					}
				}
			}
			AllData.Bookings.Add(booking);
		}

		private Appointment BookAppointment(Dog d, int bookingID, int aOffset, int appTypeID)
		{
			bool isCancelled = random.NextDouble() < 0.1;
			bool isPaidFor = false;

			bool includesNailAndTeeth = random.NextDouble() > 0.6;
			GetNextAvailableAppointment(d.DogID, WhenTryNextBookOffset(aOffset), appTypeID, includesNailAndTeeth, out bool isInitial, out DateTime date, out TimeSpan time, out int roomID, out int staffID);
			Appointment a = new Appointment(AllData.Appointments.Count, d.DogID, appTypeID, staffID,
			bookingID, roomID, includesNailAndTeeth, isCancelled, isPaidFor, date, time, isInitial && aOffset == 0);
			AllData.Appointments.Add(a);
			return a;
		}

		/// <summary>
		/// Returns how many days into the future the next appointment should be booked for
		/// </summary>
		private int WhenTryNextBookOffset(int aOffset)
		{
			int offset = (int)Math.Max(50 * Math.Log(1 / (1 - random.NextDouble()) - 1) + 150, 1);
			offset += Math.Abs(aOffset);

			if (random.NextDouble() < 0.3)
			{
				while (!IsWeekend(currentSimDate.AddDays(offset)))
				{
					offset++;
				}
			}
			return offset;
		}

		private bool IsWeekend(DateTime date)
		{
			DayOfWeek d = date.DayOfWeek;
			if (d == DayOfWeek.Saturday) return true;
			else if (d == DayOfWeek.Sunday) return true;
			else return false;
		}

		private void GetNextAvailableAppointment(int dogID, int timeOffset, int appTypeID, bool includesNailAndTeeth, out bool isInitial, out DateTime date, out TimeSpan time, out int roomID, out int staffID)
		{
			int count = 0;
			double randDouble = random.NextDouble();

			double startTime;
			if (randDouble < 0.2) startTime = 8;
			else if (randDouble < 0.4) startTime = 8.25;
			else if (randDouble < 0.6) startTime = 8.5;
			else if (randDouble < 0.8) startTime = 8.75;
			else startTime = 9;

			double endTime = 17.5;


			while (true)
			{
				DateTime dateConsid = currentSimDate.AddDays(count + timeOffset).Date;
				List<Appointment> appOnDate = AllData.Appointments.Where(a => a.AppointmentDate.Date == dateConsid && !a.IsCancelled).ToList();
				for (int minuteOffset = (int)(startTime * 60); minuteOffset < endTime * 60; minuteOffset += 15)
				{
					isInitial = AllData.Appointments.Where(a =>
						!a.IsCancelled &&
						a.DogID == dogID &&
						a.AppointmentDate <= dateConsid
						).Count() == 0;

					int appLength = GetAppLength(appTypeID, includesNailAndTeeth, isInitial);
					if (appLength + minuteOffset > endTime * 60) break;

					List<Appointment> atTime = appOnDate.Where(a => a.AppointmentTime.TotalHours + GetAppLength(a) > minuteOffset / 60.0).ToList();

					List<Appointment> withSameDog = atTime.Where(a => a.DogID == dogID).ToList();
					if (withSameDog.Count != 0) continue;

					int[] staffIDs = StaffIDArr(dateConsid, minuteOffset, GetAppLength(appTypeID, includesNailAndTeeth, isInitial));
					for (int staffCount = 0; staffCount < staffIDs.Length; staffCount++)
					{
						List<Appointment> withStaff = atTime.Where(a => a.StaffID == staffIDs[staffCount]).ToList();
						int[] randOrderedRoom = roomIDs.OrderBy(x => random.Next()).ToArray();
						for (int roomCount = 0; roomCount < 3; roomCount++)
						{
							List<Appointment> inRoom = atTime.Where(a => a.GroomingRoomID == randOrderedRoom[roomCount]).ToList();
							bool endsAfter = minuteOffset / 60 > 17;
							if (inRoom.Count == 0 && withStaff.Count == 0 && !endsAfter)
							{
								date = dateConsid;
								time = TimeSpan.FromMinutes(minuteOffset);
								roomID = randOrderedRoom[roomCount];
								staffID = staffIDs[staffCount];

								return;
							}
						}
					}
				}
				count++;
			}
		}

		private int GetAppLength(int typeID, bool includesNailAndTeeth, bool isInitial)
		{
			// appLength is in minutes
			int appLength = (int)(appLengths[typeID] * 60);
			if (includesNailAndTeeth) appLength += 15;
			if (isInitial) appLength += 15;

			return appLength;
		}

		private double GetAppLength(Appointment app)
		{
			double baseTime = appLengths[app.AppointmentTypeID];
			if (app.IncludesNailAndTeeth) baseTime += 15.0 / 60.0;
			if (app.IsInitial) baseTime += 15.0 / 60.0;
			return baseTime;
		}

		private int[] StaffIDArr(DateTime date, int time, int length)
		{
			int dow = ((int)date.DayOfWeek + 6) % 7;

			int end = time + length;

			List<int> ids = new List<int>();

			if (dow != 0)
			{
				if (!IsWeekend(date) && time >= 9.0 * 60 && end <= 17.0 * 60)
				{
					ids.Add(0);
				}
				else if (IsWeekend(date) && end <= 17.0 * 60)
				{
					ids.Add(0);
				}
			}

			if (!IsWeekend(date))
			{
				if (time >= 9.0 * 60 && end <= 17.0 * 60)
				{
					ids.Add(1);
				}
			}

			if (dow != 3 && dow != 4)
			{
				if (!IsWeekend(date) && time >= 9.0 * 60 && end <= 17.0 * 60)
				{
					ids.Add(2);
				}
				else if (IsWeekend(date) && end <= 16.0 * 60)
				{
					ids.Add(2);
				}
			}

			if (dow != 0 && dow != 1)
			{
				if (!IsWeekend(date) && time >= 9.0 * 60 && end <= 17.0 * 60)
				{
					ids.Add(3);
				}
				else if (IsWeekend(date) && end <= 16.0 * 60)
				{
					ids.Add(3);
				}
			}

			if (IsWeekend(date))
			{
				if (time >= 9.0 * 60 && end <= 17.0 * 60)
				{
					ids.Add(4);
				}
			}

			return ids.OrderBy(x => random.Next()).ToArray();
		}
	}
}
