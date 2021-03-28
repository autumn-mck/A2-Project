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
	}
}
