using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2_Project.DBObjects
{
	public class Column
	{
		public Column(string name)
		{
			Name = name;
		}

		public Constraint Constraints { get; set; }
		public string Name { get; set; }
	}
}
