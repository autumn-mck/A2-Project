namespace A2_Project.DBObjects
{
	public class Table
	{
		public Column[] Columns { get; private set; }
		public string Name { get; set; }

		// Used for pathfinding
		public int DistFromStart { get; set; } = int.MaxValue;
		public Table NearestToStart { get; set; } = null;
		public bool Visited { get; set; } = false;

		public Table(string _tableName)
		{
			Name = _tableName;
			Columns = DBMethods.MetaRequests.GetColumnDataFromTable(Name);
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
