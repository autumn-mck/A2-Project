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
	/// Interaction logic for ClientManagement.xaml
	/// </summary>
	public partial class ClientManagement : Window
	{
		private DataTable table;
		private List<string> tableHeaders;
		private List<List<string>> originalData;

		List<UIElement> display = new List<UIElement>();

		public ClientManagement()
		{
			InitializeComponent();

			List<List<string>> data = DBMethods.MetaRequests.GetAllFromTable("Contact");
			DtgMethods.CreateTable(data, "Contact", ref dtgContacts, ref tableHeaders, ref table);
			List<string> colSearch = new List<string> { "All Columns" };
			colSearch.AddRange(tableHeaders);
			cmbColumn.SelectedIndex = 0;
			cmbColumn.ItemsSource = colSearch;
			originalData = data;


			for (int i = 0; i < tableHeaders.Count; i++)
			{
				Label lbl = new Label()
				{
					Content = tableHeaders[i],
					Margin = new Thickness(900, i * 75 + 40, 0, 0),
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top
				};
				grd.Children.Add(lbl);
				if (DBMethods.MetaRequests.IsColumnPrimaryKey(tableHeaders[i], "Contact"))
				{
					lbl = new Label()
					{
						Content = "test",
						Margin = new Thickness(900, i * 75 + 75, 0, 0),
						HorizontalAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Top
					};
					display.Add(lbl);
					grd.Children.Add(lbl);
				}
				else
				{
					TextBox tbx = new TextBox()
					{
						Width = 300,
						Height = 34,
						Margin = new Thickness(905, i * 75 + 75, 0, 0),
						FontSize = 24,
						HorizontalAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Top
					};
					display.Add(tbx);
					grd.Children.Add(tbx);
				}
			}
		}

		private void BtnEmail_Click(object sender, RoutedEventArgs e)
		{
			EmailManagement.SendInvoiceEmail("atempmailfortestingcsharp@gmail.com", table, tableHeaders.ToArray());
		}

		private void DtgTest_LoadingRow(object sender, DataGridRowEventArgs e)
		{
			DataGridRow r = e.Row;
			DataRowView v = (DataRowView)r.Item;
			string[] strArr = v.Row.ItemArray.Cast<string>().ToArray();
			if (strArr.Contains("F")) r.Background = Brushes.PaleVioletRed;
			else r.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#252526");
			r.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#EEEEEE");
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
			DtgMethods.UpdateSearch(originalData, cmbColumn.SelectedIndex, tbxSearch.Text, "Contact", ref dtgContacts, ref tableHeaders, ref table);
		}

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

		private void DtgContacts_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
		{
			try
			{
				DataRowView drv = (DataRowView)dtgContacts.SelectedItems[0];
				string contactID = (string)drv.Row.ItemArray[1];
				dtgContactsToClient.Columns.Clear(); // TODO: Why is this needed here, but not elsewhere????
				DtgMethods.CreateTable(DBMethods.MiscRequests.GetContactsByClientID(contactID), "Contact", ref dtgContactsToClient, ref tableHeaders, ref table);
				dtgContactsToClient.SelectedIndex = 0;
			}
			catch { }
		}

		private void DtgContactsToClient_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
		{
			string[] selected = ((DataRowView)dtgContactsToClient.SelectedItems[0]).Row.ItemArray.OfType<string>().ToArray(); ;
			for (int i = 0; i < selected.Length; i++)
			{
				if (display[i] is Label l) l.Content = selected[i];
				else if (display[i] is TextBox t) t.Text = selected[i];
			}
		}
	}
}
