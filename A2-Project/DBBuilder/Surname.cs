namespace A2_Project.DBBuilder
{
	public class Surname
	{
		public Surname(string surnameStr, float probabilityReq)
		{
			SurnameStr = surnameStr;
			ProbabilityReq = probabilityReq;
		}

		public Surname(string fromString)
		{
			string name = "";
			float probabilityReq = 0;
			for (int i = 0; i < fromString.Length; i++)
			{
				if (fromString[i] == ',')
				{
					probabilityReq = float.Parse(fromString[(i + 1)..]);
					break;
				}
				else
				{
					name += fromString[i];
				}
			}
			ProbabilityReq = probabilityReq;
			SurnameStr = name;
		}

		public string SurnameStr { get; set; }
		public float ProbabilityReq { get; set; }
	}
}
