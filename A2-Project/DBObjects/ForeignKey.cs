namespace A2_Project.DBObjects
{
	public class ForeignKey
	{
		public string LocalColumn { get; set; } // Name of the FK column in the current table
		public string ReferencedTable { get; set; } // Name of the table this FK references
		public string ReferencedPKColumn { get; set; } // Name of the PK column in the ReferencedTable

		public ForeignKey(string referencedTable, string localColumn, string referencedPKColumn)
		{
			LocalColumn = localColumn;
			ReferencedTable = referencedTable;
			ReferencedPKColumn = referencedPKColumn;
		}
	}
}
