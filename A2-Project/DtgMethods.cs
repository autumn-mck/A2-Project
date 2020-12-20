using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;

namespace A2_Project
{
	public static class DtgMethods
	{
		public static List<List<string>> UpdateSearch(List<List<string>> originalData, int selectedIndex, string searchTerm,
		string tableName, ref DataGrid dtg, ref List<string> tableHeaders, ref DataTable table, bool shouldReset = true)
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
			CreateTable(searched, tableName, ref dtg, ref tableHeaders, ref table, shouldReset);
			return searched;
		}

		public static void CreateTable(List<List<string>> data, string tableName, ref DataGrid dtg, ref List<string> tableHeaders, ref DataTable table, bool shouldReset = false)
		{
			if (data == null) return;
			table = new DataTable();
			if (shouldReset) dtg.DataContext = table.DefaultView;
			tableHeaders = DBMethods.MetaRequests.GetHeadersFromTable(tableName);
			foreach (string str in tableHeaders)
				table.Columns.Add(str);
			for (int i = 0; i < data.Count; i++)
				table.Rows.Add(data[i].ToArray());

			dtg.DataContext = table.DefaultView;
		}
	}
}
