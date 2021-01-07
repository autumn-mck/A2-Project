using System;
using System.Data.SqlClient;
using System.IO;

namespace A2_Project
{
	/// <summary>
	/// Represents the database itself
	/// </summary>
	public class Database
	{
		public SqlCommand Cmd { get; set; }
		public SqlConnection Conn { get; set; }
		public SqlDataReader Rdr { get; set; }

		public bool Connect()
		{
			string debugPath = Path.GetDirectoryName(Environment.CurrentDirectory);
			string datadiretoryPath = Path.GetDirectoryName(debugPath);
			AppDomain.CurrentDomain.SetData("DataDirectory", datadiretoryPath);
			SqlConnectionStringBuilder scStrBuild = new SqlConnectionStringBuilder
			{
				DataSource = "(LocalDB)\\MSSQLLocalDB",
				// TODO: An absolute file path should not be used here.
				//AttachDBFilename = "|DataDirectory|DogCareDB.mdf",
				IntegratedSecurity = true
			};
			scStrBuild.AttachDBFilename = "C:\\Users\\james\\Desktop\\Projects\\A2-Project\\A2-Project\\DogCareDB.mdf";
			string scStr = scStrBuild.ToString();
			Conn = new SqlConnection(scStr);
			// Try to connect to the database. If a connection cannot be made, something has probably gone badly wrong
			try
			{
				Conn.Open();
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
