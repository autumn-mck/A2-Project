using System.IO;

namespace A2_Project
{
	/// <summary>
	/// The static class used for storing methods related to accessing files
	/// </summary>
	public static class FileAccess
	{
		private static readonly string dataLocation = Directory.GetCurrentDirectory() + "\\Resources\\";

		/// <param name="file">The name of the file to be opened</param>
		/// <returns>The contents of the file as a string</returns>
		private static string GetAllFromFile(string file)
		{
			FileStream s = new FileStream(dataLocation + file, FileMode.Open);
			StreamReader sr = new StreamReader(s);
			string str = sr.ReadToEnd();
			sr.Close();
			return str;
		}

		public static string GetEmailPassword()
		{
			return GetAllFromFile("emailPassword.txt");
		}
	}
}
