using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for ContactManagement.xaml
	/// </summary>
	public partial class ContactManagement : Window
	{
		private string tableName;

		private DataEditingSidebar editingSidebar;

		private DBObjects.Column[] columns;

		private DataTable dataTable;
		private List<List<string>> currentData;

		public ContactManagement()
		{
			InitializeComponent();

			cmbTable.ItemsSource = DBMethods.MetaRequests.GetTableNames();
			tableName = cmbTable.Items[0].ToString();
			Setup();
			try { dtgContacts.SelectedIndex = 0; }
			catch { }
			cmbTable.SelectedIndex = 0;
		}

		public void UpdateFromSidebar(string[] newData, bool isNew)
		{
			DBMethods.DBAccess.UpdateTable(tableName, columns.Select(c => c.Name).ToArray(), newData, isNew);

			// If it is an existing item that needs updates, update the table displayed to the user without requesting all data from the database again
			if (!isNew)
			{
				((DataRowView)dtgContacts.SelectedCells[0].Item).Row.ItemArray = newData;
				if (tableName == "Contact") ((DataRowView)dtgContactsToClient.SelectedCells[0].Item).Row.ItemArray = newData;
			}
			// Otherwise if the item needs to be inserted
			else
			{
				// Add the new item to the list of all current data
				currentData.Add(newData.ToList());
				// Add it to the DataTable
				dataTable.Rows.Add(newData);
				dtgContacts.ItemsSource = dataTable.DefaultView;
				// Select the item in the DataGrid and scroll to it
				dtgContacts.SelectedIndex = dataTable.Rows.Count - 1;
				dtgContacts.ScrollIntoView(dtgContacts.SelectedItem);
			}
		}

		/// <summary>
		/// Tries to delete the selected row
		/// </summary>
		public void DeleteRow(bool deleteRef = false)
		{
			// Get the selected row
			DataRowView drv = (DataRowView)dtgContacts.SelectedItem;
			if (drv == null) return;
			// Try to remove the selected row from the database
			try
			{
				DBMethods.MiscRequests.DeleteItem(tableName, columns[0].Name, drv.Row.ItemArray[0].ToString(), deleteRef);
				dataTable.Rows.Remove(drv.Row);
			}
			// An exception is thrown if there are other items which reference the item to be deleted
			catch (Exception ex)
			{
				// Allow the user to make the choice to delete the item and everything which references it (And everything that references the references, and so on)
				// Or cancel the deletion
				grdFKeyErrorOuter.Visibility = Visibility.Visible;
				tblFKeyRefError.Text = ex.Message;
			}
		}

		private void Search()
		{
			DtgMethods.UpdateSearch(currentData, cmbColumn.SelectedIndex, tbxSearch.Text, tableName, ref dtgContacts, columns, ref dataTable);
		}

		/// <summary>
		/// Set-up for changing to a new table
		/// </summary>
		private void Setup()
		{
			columns = DBMethods.MetaRequests.GetColumnDataFromTable(tableName);
			currentData = DBMethods.MetaRequests.GetAllFromTable(tableName, columns.Select(c => c.Name).ToArray());
			if (tableName == "Contact")
				dtgContactsToClient.Visibility = Visibility.Visible;
			else dtgContactsToClient.Visibility = Visibility.Hidden;

			DtgMethods.CreateTable(currentData, tableName, ref dtgContacts, columns, ref dataTable, true);

			// Allow the user to search through all columns or a specific column for the table
			List<string> colSearch = new List<string> { "All Columns" };
			colSearch.AddRange(columns.Select(c => c.Name));
			cmbColumn.SelectedIndex = 0;
			cmbColumn.ItemsSource = colSearch;
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
		private void DtgContacts_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
		{
			if (e.AddedCells.Count == 0) return;
			if (tableName == "Contact") // If the user is looking at contacts, they should be able to see other contacts from the same client.
			{
				try
				{
					// Gets the selected contact's ID and relevant contact ID
					DataRowView drv = (DataRowView)dtgContacts.SelectedItems[0];
					string contactID = (string)drv.Row.ItemArray[0];
					string clientID = (string)drv.Row.ItemArray[1];
					// Gets other contacts with the same client ID
					List<List<string>> data = DBMethods.MiscRequests.GetByColumnData(tableName, "Client ID", clientID, columns.Select(c => c.Name).ToArray());
					DataTable dt = new DataTable();
					DtgMethods.CreateTable(data, tableName, ref dtgContactsToClient, columns, ref dt, true);
					// In dtgContactsToClient, select the contact that is selected in the main DataGrid
					for (int i = 0; i < data.Count; i++)
						if (data[i][0] == contactID)
							dtgContactsToClient.SelectedIndex = i;
				}
				catch { }
			}
			else
			{
				// Update which data is selected and display this change to the user
				editingSidebar.ChangeSelectedData(((DataRowView)dtgContacts.SelectedItems[0]).Row.ItemArray.OfType<string>().ToArray());
			}
		}

		/// <summary>
		/// Update the selected data when the user selected a different item
		/// </summary>
		private void DtgContactsToClient_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
		{
			if (e.AddedCells.Count == 0) return;
			editingSidebar.ChangeSelectedData(((DataRowView)dtgContactsToClient.SelectedItems[0]).Row.ItemArray.OfType<string>().ToArray());
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

		private void BtnEmail_Click(object sender, RoutedEventArgs e)
		{
			EmailManagement.SendInvoiceEmail("atempmailfortestingcsharp@gmail.com", dataTable, columns.Select(c => c.Name).ToArray());
		}

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

		/// <summary>
		/// Updates the UI and the DataTables whenever the selected  table is changed
		/// </summary>
		private void CmbTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			tableName = cmbTable.SelectedItem.ToString();
			editingSidebar = null;
			Setup();

			if (editingSidebar != null)
				editingSidebar.Close();

			editingSidebar = new DataEditingSidebar(columns, tableName, this);
			lblSidebar.Content = editingSidebar.Content;

			try { dtgContacts.SelectedIndex = 0; }
			catch { }
		}

		/// <summary>
		/// The user wants to delete the item and anything else that references it
		/// </summary>
		private void BtnFkeyErrorAccept_Click(object sender, RoutedEventArgs e)
		{
			DeleteRow(true);
			grdFKeyErrorOuter.Visibility = Visibility.Hidden;
		}

		/// <summary>
		/// The user does not want to delete the selected item
		/// </summary>
		private void BtnFkeyErrorDecline_Click(object sender, RoutedEventArgs e)
		{
			grdFKeyErrorOuter.Visibility = Visibility.Hidden;
		}
		#endregion Events

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (editingSidebar != null) editingSidebar.Close();
		}
	}
}