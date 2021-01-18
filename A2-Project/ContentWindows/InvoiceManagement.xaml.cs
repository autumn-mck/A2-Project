using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for InvoiceManagement.xaml
	/// </summary>
	public partial class InvoiceManagement : Window
	{
		DataTable table;
		List<List<string>> data;
		string[] columns = { "Booking ID", "ID", "Dog", "App Type", "Staff", "Nails and Teeth", "Date", "Time", "Price" };
		public InvoiceManagement()
		{
			InitializeComponent();
			//DtgMethods.CreateTable
		}

		private void BtnPrint_Click(object sender, RoutedEventArgs e)
		{
			PrintingMethods.Print(grdPaper);
		}

		private void BtnEmail_Click(object sender, RoutedEventArgs e)
		{
			string[] contactData = new string[] { "Test1", "Test2", "Test3", "Test4" };
			EmailManagement.SendInvoiceEmail("atempmailfortestingcsharp@gmail.com", table, columns, contactData);
		}

		private void TbxClientID_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			if (tbxClientID.Text == "") return;
			data = DBMethods.MiscRequests.GetInvoiceData(tbxClientID.Text);
			if (data == null) return;
			table = new DataTable();
			foreach (string str in columns)
				table.Columns.Add(str);
			for (int i = 0; i < data.Count; i++)
				table.Rows.Add(data[i].ToArray());
			dtgData.ItemsSource = table.DefaultView;
		}

		private void TbxClientID_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			if (!int.TryParse(e.Text, out _)) e.Handled = true;
		}
	}
}
