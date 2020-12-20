using System.Data.SqlClient;

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
			SqlConnectionStringBuilder scStrBuild = new SqlConnectionStringBuilder
			{
				DataSource = "(LocalDB)\\MSSQLLocalDB",
				// TODO: An absolute file path should not be used here.
				AttachDBFilename = "C:\\Users\\James\\Desktop\\Projects\\A2-Project\\A2-Project\\Database1.mdf",
				IntegratedSecurity = true
			};
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
