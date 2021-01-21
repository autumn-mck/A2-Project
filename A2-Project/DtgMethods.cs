using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;

namespace A2_Project
{
	public static class DtgMethods
	{
		/// <summary>
		/// Searches through originalData by searchTerm and displays it on dtg
		/// </summary>
		public static List<List<string>> UpdateSearch(List<List<string>> originalData, int selectedIndex, string searchTerm,
		string tableName, ref DataGrid dtg, DBObjects.Column[] columns, ref DataTable table, bool shouldReset = true, bool isExact = false, string[] prevSelection = null)
		{
			if (originalData == null) return null;
			int columnSearch = selectedIndex;
			List<List<string>> searched = new List<List<string>>();
			if (columnSearch == 0)
			{
				foreach (List<string> ls in originalData)
				{
					bool contained = false;

					foreach (string s in ls)
					{
						if (isExact)
						{
							if (s == searchTerm) contained = true;
						}
						else
						{
							if (s.ToLower().Contains(searchTerm)) contained = true;
						}
					}

					if (contained) searched.Add(ls);
				}
			}
			else
			{
				foreach (List<string> ls in originalData)
				{
					if (isExact)
					{
						if (ls[columnSearch - 1] == searchTerm) searched.Add(ls);
					}
					else
					{
						if (ls[columnSearch - 1].ToLower().Contains(searchTerm)) searched.Add(ls);
					}
				}
			}

			int newSelIndex;
			if (prevSelection is null)
			{
				newSelIndex = -1;
			}
			else
			{
				newSelIndex = searched.FindIndex(x => x[0] == prevSelection[0]);
			}

			CreateTable(searched, tableName, ref dtg, columns, ref table, shouldReset, newSelIndex);
			return searched;
		}

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
			foreach (string str in columns.Select(c => c.Name))
				table.Columns.Add(str);
			for (int i = 0; i < data.Count; i++)
				table.Rows.Add(data[i].ToArray());

			dtg.DataContext = table.DefaultView;
			dtg.SelectedIndex = newSelIndex;
			if (newSelIndex > -1) dtg.ScrollIntoView(dtg.SelectedItem);
		}
	}
}
