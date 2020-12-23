using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2_Project.DBObjects
{
	public class ForeignKey
	{
		public ForeignKey(string referencedTable, string referencedColumn)
		{
			ReferencedTable = referencedTable;
			ReferencedColumn = referencedColumn;
		}

		public string ReferencedTable { get; set; }
		public string ReferencedColumn { get; set; }
	}
}
