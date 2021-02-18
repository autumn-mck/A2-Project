using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

		FilterableDataGrid filterableDataGrid;

		public ContactManagement()
		{
			InitializeComponent();

			cmbTable.ItemsSource = DBMethods.MetaRequests.GetTableNames();

			tableName = cmbTable.Items[0].ToString();

			cmbTable.SelectedIndex = 0;
		}

		public void UpdateFromSidebar(string[] newData, bool isNew)
		{
			filterableDataGrid.UpdateData(newData, isNew);
		}

		/// <summary>
		/// Tries to delete the selected row
		/// </summary>
		public void DeleteItem(bool deleteRef = false)
		{
			try
			{
				filterableDataGrid.TryDeleteSelected(deleteRef);
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

		/// <summary>
		/// Set-up for changing to a new table
		/// </summary>
		private void Setup()
		{
			tableName = cmbTable.SelectedItem.ToString();
			editingSidebar = null;

			columns = DBMethods.MetaRequests.GetColumnDataFromTable(tableName);

			// Allow the user to search through all columns or a specific column for the table
			List<string> colSearch = new List<string> { "All Columns" };
			colSearch.AddRange(columns.Select(c => c.Name));

			if (editingSidebar != null)
				editingSidebar.Close();

			editingSidebar = new DataEditingSidebar(columns, tableName, this);
			lblSidebar.Content = editingSidebar.Content;

			if (!(filterableDataGrid is null)) filterableDataGrid.Close();
			filterableDataGrid = new FilterableDataGrid(columns, this);
			filterableDataGrid.SetMaxHeight(700);
			lblSearchData.Content = filterableDataGrid.Content;
		}

		#region Events

		/// <summary>
		/// Updates the UI and the DataTables whenever the selected  table is changed
		/// </summary>
		private void CmbTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Setup();
		}

		public void TableSelectionChanged(string[] newData)
		{
			editingSidebar.ChangeSelectedData(newData);
		}

		/// <summary>
		/// The user wants to delete the item and anything else that references it
		/// </summary>
		private void BtnFkeyErrorAccept_Click(object sender, RoutedEventArgs e)
		{
			DeleteItem(true);
			grdFKeyErrorOuter.Visibility = Visibility.Collapsed;
		}

		/// <summary>
		/// The user does not want to delete the selected item
		/// </summary>
		private void BtnFkeyErrorDecline_Click(object sender, RoutedEventArgs e)
		{
			grdFKeyErrorOuter.Visibility = Visibility.Collapsed;
		}
		#endregion Events

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (editingSidebar != null) editingSidebar.Close();
		}

		private void BtnPrint_Click(object sender, RoutedEventArgs e)
		{
			PrintingMethods.Print(filterableDataGrid.GetDataGrid());
		}

		private void BtnEmail_Click(object sender, RoutedEventArgs e)
		{
			//EmailManagement.SendInvoiceEmail("atempmailfortestingcsharp@gmail.com", searchableDataGrid.GetDataTable(), columns.Select(c => c.Name).ToArray());
		}
	}
}