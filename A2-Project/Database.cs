using System.Data.SqlClient;

namespace A2_Project
{
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
				AttachDBFilename = "C:\\Users\\James\\Desktop\\Projects\\A2-Project\\A2-Project\\Database1.mdf",
				IntegratedSecurity = true
			};
			string scStr = scStrBuild.ToString();
			Conn = new SqlConnection(scStr);
			try
			{
				Conn.Open();
				return true;
			}
			catch
			{
				//MessageBox.Show(ex.Message);
				return false;
			}
		}
	}
}
