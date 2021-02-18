using System.Collections.Generic;

namespace A2_Project.DBObjects
{
	public static class DB
	{
		public static Table[] Tables { get; private set; }

		public static void Initialise()
		{
			List<string> tableNames = DBMethods.MetaRequests.GetAllTableNames();
			Tables = new Table[tableNames.Count];

			for (int i = 0; i < tableNames.Count; i++)
			{
				Tables[i] = new Table(tableNames[i]);
			}
		}
	}
}
