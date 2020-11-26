using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2_Project.SQLObjects
{
	public class Client
	{
		public Client(int iD, string clientNotes, DateTime joinDate)
		{
			ID = iD;
			ClientNotes = clientNotes;
			JoinDate = joinDate;
		}

		public Client(List<string> fromArr)
		{
			ID = Int32.Parse(fromArr[0]);
			ClientNotes = fromArr[1];
			JoinDate = DateTime.Parse(fromArr[2]);
		}

		public int ID { get; set; }
		public string ClientNotes { get; set; }
		public DateTime JoinDate { get; set; }
	}
}
