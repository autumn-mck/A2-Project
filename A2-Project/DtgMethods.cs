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
		string tableName, ref DataGrid dtg, DBObjects.Column[] columns, ref DataTable table, bool shouldReset = true)
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
						if (s.Contains(searchTerm)) contained = true;
					}
					if (contained) searched.Add(ls);
				}
			}
			else
			{
				foreach (List<string> ls in originalData)
				{
					if (ls[columnSearch - 1].Contains(searchTerm)) searched.Add(ls);
				}
			}
			CreateTable(searched, tableName, ref dtg, columns, ref table, shouldReset);
			return searched;
		}

		/// <summary>
		/// Updates dtg to contain data
		/// </summary>
		public static void CreateTable(List<List<string>> data, string tableName, ref DataGrid dtg, DBObjects.Column[] columns, ref DataTable table, bool shouldReset = false)
		{
			if (data == null) return;
			table = new DataTable();
			if (shouldReset)
			{
				dtg.Columns.Clear();
				dtg.DataContext = table.DefaultView;
			}
			foreach (string str in columns.Select(c => c.Name))
				table.Columns.Add(str);
			for (int i = 0; i < data.Count; i++)
				table.Rows.Add(data[i].ToArray());

			dtg.DataContext = table.DefaultView;
		}
	}
}
