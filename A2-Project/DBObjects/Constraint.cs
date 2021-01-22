namespace A2_Project.DBObjects
{
	public class Constraint
	{
		public Constraint(bool isPrimaryKey, bool canBeNull, string type, string maxSize)
		{
			IsPrimaryKey = isPrimaryKey;
			CanBeNull = canBeNull;
			Type = type;
			int.TryParse(maxSize, out int size);
			MaxSize = size;
		}

		public ForeignKey ForeignKey { get; set; }
		public bool CanBeNull { get; set; }
		public bool IsPrimaryKey { get; set; }
		public string Type { get; set; }
		public int MaxSize { get; set; }
	}
}
