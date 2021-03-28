using A2_Project.DBObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for FilterableDataGrid.xaml
	/// </summary>
	public partial class FilterableDataGrid : Window
	{
		private Column[] columns;
		private string tableName;

		private DataTable dataTable;
		private List<List<string>> currentData;

		private Window containingWindow;

		private UserControls.FilterManager filterManager;

		string[] selItem;

		public FilterableDataGrid(string tableName, Window _containingWindow)
		{
			Initialise(DBMethods.MetaRequests.GetColumnDataFromTable(tableName), _containingWindow);
		}

		public FilterableDataGrid(Column[] _columns, Window _containingWindow)
		{
			Initialise(_columns, _containingWindow);
		}

		private void Initialise(Column[] _columns, Window _containingWindow)
		{
			InitializeComponent();
			columns = _columns;
			tableName = columns[0].TableName;
			lblManageFilters.Content = $"Manage {tableName} Filters (0)";

			containingWindow = _containingWindow;

			currentData = DBMethods.MetaRequests.GetAllFromTable(tableName, columns.Select(c => c.Name).ToArray());

			DtgMethods.CreateTable(currentData, tableName, ref dtg, columns, ref dataTable, true);
			lblCount.Content = $"Count: {currentData.Count}";

			//try { dtgData.SelectedIndex = 0; }
			//catch { }

			filterManager = new UserControls.FilterManager(columns, this)
			{
				Margin = new Thickness(0, 21.21320344, 0, 0)
			};
			grdFiltersOuter.Children.Add(filterManager);
		}

		public string[] GetSelectedData()
		{
			System.Collections.IList selectedItems = dtg.SelectedItems;
			if (selectedItems.Count == 0) return null;
			return ((DataRowView)selectedItems[0]).Row.ItemArray.OfType<string>().ToArray();
		}

		public void UpdateData(string[] data, bool isNew)
		{
			DBMethods.DBAccess.UpdateTable(tableName, columns.Select(c => c.Name).ToArray(), data, isNew);

			// If it is an existing item that needs updates, update the table displayed to the user without requesting all data from the database again
			if (!isNew)
			{
				List<string> oldData = currentData.Where(x => x[0] == data[0]).First();
				int index = currentData.IndexOf(oldData);
				currentData[index] = data.ToList();
				// Note: It's probably fine, but this code assumes the data you're changing is in the currently selected row.
				((DataRowView)dtg.SelectedItem).Row.ItemArray = data;
			}
			// Otherwise if the item needs to be inserted
			else
			{
				// Just get all the data again - otherwise stuff breaks.
				FiltersSaved();

				int newIndex = currentData.IndexOf(currentData.Where(x => x[0] == data[0]).First());
				dtg.SelectedIndex = newIndex;
				dtg.ScrollIntoView(dtg.SelectedItem);
			}
		}

		public void ChangeSearch(int columnIndex, string value)
		{
			filterManager.ChangeSearch(columnIndex, value);
			lblCount.Content = $"Count: {currentData.Count}";
		}

		internal void FiltersSaved()
		{
			lblManageFilters.Content = $"Manage {tableName} Filters ({filterManager.GetFilterCount()})";
			grdFiltersOuter.Visibility = Visibility.Collapsed;

			List<string> tablesReferenced = filterManager.GetTablesReferenced();

			try
			{
				string filterText = filterManager.GetFilterText();
			
				string sql = ConstructSQL(filterText, tablesReferenced);

				//MessageBox.Show(sql);

				currentData = DBMethods.DBAccess.GetListStringsWithQuery(sql);

				DtgMethods.CreateTable(currentData, tableName, ref dtg, columns, ref dataTable, true);

				lblCount.Content = $"Count: {currentData.Count}";
			}
			catch
			{
				MessageBox.Show("Error: Invalid filter(s)!");
			}
		}

		private static string ConstructSQL(string filterText, List<string> tablesReferenced)
		{
			List<Table> tables = new List<Table>();
			foreach (string s in tablesReferenced)
			{
				List<Table> toConsider = GetShortestPath(tablesReferenced[0], s);
				foreach (Table t in toConsider)
				{
					if (!tables.Contains(t)) tables.Add(t);
				}
			}

			string sql = "SET DATEFORMAT dmy; SELECT ";

			string selectedColumns = "";

			for (int i = 0; i < tables[0].Columns.Length; i++)
			{
				Column c = tables[0].Columns[i];

				selectedColumns += $"[{c.TableName}].[{c.Name}]";

				if (i == tables[0].Columns.Length - 1) selectedColumns += " ";
				else selectedColumns += ", ";
			}

			sql += selectedColumns;

			sql += $"FROM [{tables[0]}] ";

			for (int i = 0; i < tables.Count - 1; i++)
			{
				Column col = FindRefColumn(tables.GetRange(0, i + 2), i);
				sql += $"INNER JOIN [{tables[i + 1]}] ON [{col.Constraints.ForeignKey.ReferencedTable}].[{col.Name}] = [{col.TableName}].[{col.Name}] ";
			}

			if (filterText != "")
			{
				sql += $"WHERE {filterText}";
			}

			sql += $" GROUP BY {selectedColumns};";

			return sql;
		}

		internal DataTable GetDataTable()
		{
			return dataTable;
		}

		private static Column FindRefColumn(List<Table> tables, int index)
		{
			// Does A ref B?
			Column col = DoesTableRefTable(tables[index], tables[index + 1]);
			// Does B ref A?
			if (col is null) col = DoesTableRefTable(tables[index + 1], tables[index]);
			// A isn't directly related to B?
			if (col is null)
			{
				for (int i = 0; (i < index && col is null); i++)
				{
					col = DoesTableRefTable(tables[i], tables[index + 1]);
					if (col is null) col = DoesTableRefTable(tables[index + 1], tables[i]);
				}
			}

			// If col is still null, something has probably gone badly wrong, as tables should be connected in the correct order
			if (col is null)
				throw new NotImplementedException();

			return col;
		}

		internal void ClearSearch()
		{
			DataRowView o = (DataRowView)dtg.SelectedItem;
			object[] prevSelection = null;
			if (o is not null) prevSelection =  o.Row.ItemArray;
			filterManager.ClearFilters();
			dtg.SelectedIndex = currentData.IndexOf(currentData.Where(x => x[0] == (string)prevSelection[0]).FirstOrDefault());
			dtg.ScrollIntoView(dtg.SelectedItem);
			lblCount.Content = $"Count: {currentData.Count}";
		}

		/// <summary>
		/// Gets the shortest path between the start and end locations
		/// </summary>
		public static List<Table> GetShortestPath(string start, string end)
		{
			Pathfind(start, end);
			Table tEnd = GetTableWithName(end);
			List<Table> shortestPath = new List<Table> { tEnd };
			BuildShortestPath(shortestPath, tEnd);
			shortestPath.Reverse();

			foreach (Table t in DB.Tables)
			{
				t.Visited = false;
				t.DistFromStart = int.MaxValue;
				t.NearestToStart = null;
			}

			return shortestPath;
		}

		internal void UpdateSelectedIndex(int _newIndex)
		{
			dtg.SelectedIndex = _newIndex;
		}

		/// <summary>
		/// Builds the shortest path after the Pathfind method has finished
		/// </summary>
		private static void BuildShortestPath(List<Table> list, Table t)
		{
			if (t.NearestToStart == null)
				return;
			list.Add(t.NearestToStart);
			BuildShortestPath(list, t.NearestToStart);
		}

		private static void Pathfind(string tableFrom, string tableTo)
		{
			Table from = GetTableWithName(tableFrom);
			Table to = GetTableWithName(tableTo);

			if (from == to) return;

			List<Table> queue = new List<Table>();

			from.DistFromStart = 0;
			Table current = from;

			do
			{
				List<Table> neighbours = GetNeighboursOfTable(current);

				foreach (Table t in neighbours)
				{
					if (t.Visited)
					{
						//do
						//{
						//	queue.Remove(loc);
						//}
						//while (queue.Contains(loc));
						continue;
					}
					if (t.DistFromStart == 0 || current.DistFromStart + 1 < t.DistFromStart)
					{
						t.DistFromStart = current.DistFromStart + 1;
						t.NearestToStart = current;
						if (!t.Visited && !queue.Contains(t))
							queue.Add(t);
					}
				}
				if (current == to)
					break;

				current.Visited = true;
				queue.Remove(current);
				current = queue[0];
			}
			while (queue.Any());
		}

		private static List<Table> GetNeighboursOfTable(Table table)
		{
			List<Table> neighbours = new List<Table>();

			foreach (Column c in table.Columns)
			{
				if (c.Constraints.ForeignKey is not null)
				{
					Table toAdd = GetTableWithName(c.Constraints.ForeignKey.ReferencedTable);
					if (!neighbours.Contains(toAdd)) neighbours.Add(toAdd);
				}
			}

			foreach (Table t in DB.Tables)
			{
				foreach (Column c in t.Columns)
				{
					if (c.Constraints.ForeignKey is not null
					&& c.Constraints.ForeignKey.ReferencedTable ==  table.Name)
					{
						if (!neighbours.Contains(t)) neighbours.Add(t);
					}
				}
			}

			return neighbours;
		}

		private static Column DoesTableRefTable(Table t1, Table t2)
		{
			return t1.Columns.Where(col => col.Constraints.ForeignKey is not null && col.Constraints.ForeignKey.ReferencedTable == t2.Name).FirstOrDefault();
		}

		private static Table GetTableWithName(string name)
		{
			return DB.Tables.Where(t => t.Name == name).FirstOrDefault();
		}

		public void TryDeleteSelected(bool deleteRef)
		{
			// Get the selected row
			DataRowView drv = (DataRowView)dtg.SelectedItem;
			if (drv == null) return;
			// Try to remove the selected row from the database
			// Note that an exception is thrown if there are other items which reference the item to be deleted
			// This means this method must be called from within a try/catch block
			DBMethods.MiscRequests.DeleteItem(tableName, columns[0].Name, drv.Row.ItemArray[0].ToString(), deleteRef);

			string id = drv.Row.ItemArray[0].ToString();
			List<string> oldData = currentData.Where(x => x[0] == id).First();
			currentData.Remove(oldData);

			dataTable.Rows.Remove(drv.Row);
		}

		#region DataGrid Events
		/// <summary>
		/// Ensures that text in a column wraps properly
		/// </summary>
		private void Dtg_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
		{
			// Cancel the auto generated column
			e.Cancel = true;
			DataGridTextColumn dgTextC = (DataGridTextColumn)e.Column;
			//Create a new template column 
			DataGridTemplateColumn dgtc = new DataGridTemplateColumn();
			DataTemplate dataTemplate = new DataTemplate(typeof(DataGridCell));
			FrameworkElementFactory tb = new FrameworkElementFactory(typeof(TextBlock));
			// Ensure the text wraps properly when the column is resized
			tb.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
			dataTemplate.VisualTree = tb;

			dgtc.Header = dgTextC.Header;
			dgtc.CellTemplate = dataTemplate;
			tb.SetBinding(TextBlock.TextProperty, dgTextC.Binding);

			dgtc.MinWidth = 70;
			dgtc.MaxWidth = 160;

			// Add column back to data grid
			if (sender is DataGrid dg) dg.Columns.Add(dgtc);
		}

		/// <summary>
		/// Update the selected data when the user selected a different item
		/// </summary>
		private void Dtg_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
		{
			if (e.AddedCells.Count == 0) return;
			string[] newData = ((DataRowView)dtg.SelectedItems[0]).Row.ItemArray.OfType<string>().ToArray();
			selItem = newData;

			if (containingWindow is AllTableManger contactManagement) contactManagement.TableSelectionChanged(newData);
			else if (containingWindow is ClientManagement clientManagement) clientManagement.TableSelectionChanged(this, newData);
		}

		/// <summary>
		/// Whenever a new row is being loaded, attempt to colour it based on the data it contains
		/// </summary>
		private void Dtg_LoadingRow(object sender, DataGridRowEventArgs e)
		{
			// Get the data being loaded
			DataGridRow row = e.Row;
			DataRowView drv = (DataRowView)row.Item;
			string[] strArr = drv.Row.ItemArray.Cast<string>().ToArray();

			switch (tableName)
			{
				case "Dog":
				case "Contact": // Group contacts together by their ClientID
					if (strArr.Length == 1) { row.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#0A0A0A"); break; }
					if (Convert.ToInt32(strArr[1]) % 2 == 0) row.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#0A0A0A");
					else row.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#252526");
					break;
				default:
					if (strArr.Length == 1) { row.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#0A0A0A"); break; }
					if (Convert.ToInt32(strArr[0]) % 2 == 0) row.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#0A0A0A");
					else row.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#252526");
					break;
			}

			// Set the text colour to slightly less bright than white
			row.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#EEEEEE");
		}

		#endregion DataGrid Events

		private void StpFilterBtn_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (grdFiltersOuter.Visibility == Visibility.Visible) grdFiltersOuter.Visibility = Visibility.Collapsed;
			else grdFiltersOuter.Visibility = Visibility.Visible;
		}

		public DataGrid GetDataGrid()
		{
			return dtg;
		}

		public void SetMaxHeight(double newMax)
		{
			dtg.Height = newMax - dtg.Margin.Top;
			grd.Height = newMax;
		}

		internal void SelectItem(string text)
		{
			DataRow row = dataTable.Rows.OfType<DataRow>().Where(r => r.ItemArray[0].ToString() == text).FirstOrDefault();
			int index = dataTable.Rows.IndexOf(row);
			dtg.SelectedIndex = index;
			if (index == -1) return;
			dtg.ScrollIntoView(dtg.SelectedItem);
		}

		public string GetClientID()
		{
			if (currentData.Count == 1) return currentData[0][0];
			else return selItem[0].ToString();
		}

		public void HideCount()
		{
			lblCount.Visibility = Visibility.Collapsed;
		}
	}
}
