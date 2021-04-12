using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using A2_Project.UserControls;
using A2_Project.DBObjects;

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
		private readonly string[] columns = { "Booking ID", "ID", "Dog", "App Type", "Staff", "Nails and Teeth", "Date", "Time", "Price" };

		private readonly ValidatedTextbox tbxClientID;

		public InvoiceManagement()
		{
			InitializeComponent();

			Column staffIDCol = DB.Tables.Where(t => t.Name == "Dog").First().Columns[1];
			tbxClientID = (ValidatedTextbox)UIMethods.GenAppropriateElement(staffIDCol, out _, false, true);
			tbxClientID.Margin = new Thickness(110, 5, 0, 0);
			tbxClientID.AddTextChangedEvent(TbxClientID_TextChanged);
			grd.Children.Add(tbxClientID);
		}

		private void BtnPrint_Click(object sender, RoutedEventArgs e)
		{
			PrintingMethods.Print(vbxPaper);
		}

		private void BtnEmail_Click(object sender, RoutedEventArgs e)
		{
			EmailManagement.SendInvoiceEmail("atempmailfortestingcsharp@gmail.com", table, columns, contactData);
		}

		private void TbxClientID_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			if (!tbxClientID.IsValid) return;
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
	}
}
