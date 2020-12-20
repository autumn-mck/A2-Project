using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace A2_Project
{
	/// <summary>
	/// Interaction logic for InvoiceTesting.xaml
	/// </summary>
	public partial class InvoiceTesting : Window
	{
		private DataTable table;
		private List<string> tableHeaders;
		private List<List<string>> data;

		public InvoiceTesting()
		{
			InitializeComponent();
			cmbTables.ItemsSource = DBMethods.MetaRequests.GetTableNames();
			cmbTables.SelectedIndex = 0;
		}

		private void BtnPrint_Click(object sender, RoutedEventArgs e)
		{
			PrintingMethods.Print(dtgTest);
		}

		private void BtnEmail_Click(object sender, RoutedEventArgs e)
		{
			EmailManagement.SendInvoiceEmail("atempmailfortestingcsharp@gmail.com", table, tableHeaders.ToArray());
		}

		private void CmbTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			List<List<string>> data = DBMethods.MetaRequests.GetAllFromTable(cmbTables.SelectedItem.ToString());
			DtgMethods.CreateTable(data, cmbTables.SelectedItem.ToString(), ref dtgTest, ref tableHeaders, ref table);
			List<string> colSearch = new List<string> { "All Columns" };
			colSearch.AddRange(tableHeaders);
			cmbColumn.SelectedIndex = 0;
			cmbColumn.ItemsSource = colSearch;
			this.data = data;
		}

		private void DtgTest_LoadingRow(object sender, DataGridRowEventArgs e)
		{
			DataGridRow r = e.Row;
			DataRowView v = (DataRowView)r.Item;
			string[] strArr = v.Row.ItemArray.Cast<string>().ToArray();
			if (strArr.Contains("F")) r.Background = Brushes.PaleVioletRed;
			else r.Background = Brushes.LightBlue;
		}

		private void TbxSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			Search();
		}

		private void CmbColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Search();
		}

		private void Search()
		{
			DtgMethods.UpdateSearch(data, cmbColumn.SelectedIndex, tbxSearch.Text, cmbTables.Text, ref dtgTest, ref tableHeaders, ref table, false);
		}
	}
}
