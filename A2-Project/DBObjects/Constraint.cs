namespace A2_Project.DBObjects
{
	public class Constraint
	{
		public Constraint(bool isPrimaryKey, bool canBeNull, string type, string maxSize)
		{
			IsPrimaryKey = isPrimaryKey;
			CanBeNull = canBeNull;
			Type = type;
			MaxSize = maxSize;
		}

		public ForeignKey ForeignKey { get; set; }
		public bool CanBeNull { get; set; }
		public bool IsPrimaryKey { get; set; }
		public string Type { get; set; }
		public string MaxSize { get; set; }
	}
}
