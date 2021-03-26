using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace A2_Project
{
	public static class DtgMethods
	{
		/// <summary>
		/// Updates dtg to contain data
		/// </summary>
		public static void CreateTable(List<List<string>> data, string tableName, ref DataGrid dtg, DBObjects.Column[] columns, ref DataTable table, bool shouldReset = false, int newSelIndex = -1)
		{
			if (data == null) return;
			table = new DataTable();
			if (shouldReset)
			{
				//dtg.DataContext = table.DefaultView;
				dtg.Columns.Clear();
			}

			if (data.Count > 0)
			{
				foreach (string str in columns.Select(c => c.Name))
					table.Columns.Add(str);
				for (int i = 0; i < data.Count; i++)
					table.Rows.Add(data[i].ToArray());

			}
			else
			{
				// TODO: Display "No results!" or similar
				table.Columns.Add("Note");
				table.Rows.Add(new string[] { "No Results!" });
			}

			dtg.DataContext = table.DefaultView;
			dtg.SelectedIndex = newSelIndex;
			if (newSelIndex > -1) dtg.ScrollIntoView(dtg.SelectedItem);
		}
	}
}
