using System.IO;

namespace A2_Project
{
	public static class FileAccess
	{
		private static string dataLocation = Directory.GetCurrentDirectory() + "\\Resources\\";

		private static string GetAllFromFile(string file)
		{
			FileStream s = new FileStream(dataLocation + file, FileMode.Open);
			StreamReader sr = new StreamReader(s);
			return sr.ReadToEnd();
		}

		public static string GetEmailPassword()
		{
			return GetAllFromFile("emailPassword.txt");
		}
	}
}
