using System;

namespace A2_Project.DBBuilder
{
	public class Contact
	{
		public Contact(int contactID, int clientID, string contactName, string contactEmail, string contactAddress, string contactCounty, string contactTown, string postcode, string contactPhoneNo)
		{
			ContactID = contactID;
			ClientID = clientID;
			ContactName = contactName;
			ContactEmail = contactEmail;
			ContactAddress = contactAddress;
			ContactCounty = contactCounty;
			ContactTown = contactTown;
			ContactPostcode = postcode;
			ContactPhoneNo = contactPhoneNo;
		}

		public int ContactID { get; set; }
		public int ClientID { get; set; }
		public string ContactName { get; set; }
		public string ContactEmail { get; set; }
		public string ContactPhoneNo { get; set; }
		public string ContactAddress { get; set; }
		public string ContactCounty { get; set; }
		public string ContactTown { get; set; }
		public string ContactPostcode { get; set; }

		public string ToSQL()
		{
			return $"INSERT INTO [Contact] VALUES ({ContactID}, {ClientID}, '{ContactName}', '{ContactEmail}', " +
			$"'{ContactAddress}', '{ContactTown}', '{ContactCounty}', '{ContactPostcode}', '{ContactPhoneNo}'); ";
		}

		public Microsoft.Data.Sqlite.SqliteCommand ToSqliteCommand(Microsoft.Data.Sqlite.SqliteConnection connection)
		{
			var command = connection.CreateCommand();
			command.CommandText = @"
                INSERT INTO Contact (ContactID, ClientID, ContactName, ContactEmail, ContactAddress, ContactCounty, ContactTown, ContactPostcode, ContactPhoneNo)
                VALUES ($contactID, $clientID, $contactName, $contactEmail, $contactAddress, $contactCounty, $contactTown, $contactPostcode, $contactPhoneNo);
            ";
			command.Parameters.AddWithValue("$contactID", ContactID);
			command.Parameters.AddWithValue("$clientID", ClientID);
			command.Parameters.AddWithValue("$contactName", ContactName);
			command.Parameters.AddWithValue("$contactEmail", ContactEmail ?? (object)DBNull.Value);
			command.Parameters.AddWithValue("$contactAddress", ContactAddress ?? (object)DBNull.Value);
			command.Parameters.AddWithValue("$contactCounty", ContactCounty ?? (object)DBNull.Value);
			command.Parameters.AddWithValue("$contactTown", ContactTown ?? (object)DBNull.Value);
			command.Parameters.AddWithValue("$contactPostcode", ContactPostcode ?? (object)DBNull.Value);
			command.Parameters.AddWithValue("$contactPhoneNo", ContactPhoneNo ?? (object)DBNull.Value);
			return command;
		}
	}
}
