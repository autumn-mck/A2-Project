using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for SearchableDataGrid.xaml
	/// </summary>
	public partial class SearchableDataGrid : Window
	{
		private DBObjects.Column[] columns;
		private string tableName;

		private DataTable dataTable;
		private List<List<string>> currentData;

		private Window containingWindow;

		public SearchableDataGrid(string _tableName, DBObjects.Column[] _columns, Window _containingWindow)
		{
			InitializeComponent();

			tableName = _tableName;
			columns = _columns;
			containingWindow = _containingWindow;

			currentData = DBMethods.MetaRequests.GetAllFromTable(tableName, columns.Select(c => c.Name).ToArray());

			DtgMethods.CreateTable(currentData, tableName, ref dtgData, columns, ref dataTable, true);

			// Allow the user to search through all columns or a specific column for the table
			List<string> colSearch = new List<string> { "All Columns" };
			colSearch.AddRange(columns.Select(c => c.Name));
			cmbColumn.SelectedIndex = 0;
			cmbColumn.ItemsSource = colSearch;

			try { dtgData.SelectedIndex = 0; }
			catch { }
		}

		private void Search()
		{
			DtgMethods.UpdateSearch(currentData, cmbColumn.SelectedIndex, tbxSearch.Text, tableName, ref dtgData, columns, ref dataTable);
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
				((DataRowView)dtgData.SelectedItem).Row.ItemArray = data;
			}
			// Otherwise if the item needs to be inserted
			else
			{
				// Add the new item to the list of all current data
				currentData.Add(data.ToList());
				// Add it to the DataTable
				dataTable.Rows.Add(data);
				dtgData.ItemsSource = dataTable.DefaultView;
				// Select the item in the DataGrid and scroll to it
				dtgData.SelectedIndex = dataTable.Rows.Count - 1;
				dtgData.ScrollIntoView(dtgData.SelectedItem);
			}
		}

		public void TryDeleteSelected(bool deleteRef)
		{
			// Get the selected row
			DataRowView drv = (DataRowView)dtgData.SelectedItem;
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

		public DataTable GetDataTable()
		{
			return dataTable;
		}

		public FrameworkElement GetDataGrid()
		{
			return dtgData;
		}

		#region Events

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

			// Add column back to data grid
			if (sender is DataGrid dg) dg.Columns.Add(dgtc);
		}

		/// <summary>
		/// Update the selected data when the user selected a different item
		/// </summary>
		private void Dtg_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
		{
			if (e.AddedCells.Count == 0) return;
			string[] newData = ((DataRowView)dtgData.SelectedItems[0]).Row.ItemArray.OfType<string>().ToArray();

			if (containingWindow is ContactManagement contactManagement) contactManagement.TableSelectionChanged(newData);
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
				case "Contact": // Group contacts together by their ClientID
					if (Convert.ToInt32(strArr[1]) % 2 == 0) row.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#161616");
					else row.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#252526");
					break;
				default:
					row.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#252526");
					break;
			}

			// Set the text colour to slightly less bright than white
			row.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#EEEEEE");
		}

		#endregion DataGrid Events

		/// <summary>
		/// Update the search whenever the user changes the text to be searched
		/// </summary>
		private void TbxSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			Search();
		}

		/// <summary>
		/// Update the search whenever the column being searched through is changed by the user
		/// </summary>
		private void CmbColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Search();
		}
		#endregion Events
	}
}
