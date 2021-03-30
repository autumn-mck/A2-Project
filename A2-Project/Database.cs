using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

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
				IntegratedSecurity = true
			};
			scStrBuild.AttachDBFilename = Directory.GetCurrentDirectory() + "\\DogCareDB.mdf";
			string scStr = scStrBuild.ToString();
			Conn = new SqlConnection(scStr);
			// Try to connect to the database. If a connection cannot be made, something has probably gone badly wrong
			try
			{
				Conn.Open();
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public void Close()
		{
			Conn.Close();
		}
	}
}
