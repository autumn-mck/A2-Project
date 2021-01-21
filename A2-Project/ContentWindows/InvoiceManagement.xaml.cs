using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for InvoiceManagement.xaml
	/// </summary>
	public partial class InvoiceManagement : Window
	{
		private DataTable table;
		private List<List<string>> data;
		private string[][] contactData;
		private string[] columns = { "Booking ID", "ID", "Dog", "App Type", "Staff", "Nails and Teeth", "Date", "Time", "Price" };

		public InvoiceManagement()
		{
			InitializeComponent();
		}

		private void BtnPrint_Click(object sender, RoutedEventArgs e)
		{
			PrintingMethods.Print(grdPaper);
		}

		private void BtnEmail_Click(object sender, RoutedEventArgs e)
		{
			EmailManagement.SendInvoiceEmail("atempmailfortestingcsharp@gmail.com", table, columns, contactData);
		}

		private void TbxClientID_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			if (tbxClientID.Text == "") return;
			contactData = DBMethods.MiscRequests.GetContactDataFromClient(tbxClientID.Text).Select(x => x.ToArray()).ToArray();
			lblContactDetails.Content = $"{contactData[0][0]}\n{contactData[0][1]}\n{contactData[0][2]}";

			data = DBMethods.MiscRequests.GetInvoiceData(tbxClientID.Text);
			if (data == null) return;
			table = new DataTable();
			foreach (string str in columns)
				table.Columns.Add(str);
			for (int i = 0; i < data.Count; i++)
				table.Rows.Add(data[i].ToArray());

			dtgData.Visibility = Visibility.Visible;
			dtgData.ItemsSource = table.DefaultView;
		}

		private void TbxClientID_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			if (!int.TryParse(e.Text, out _)) e.Handled = true;
		}
	}
}
