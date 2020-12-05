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
		private readonly DBAccess dbAccess;
		private DataTable table;
		private List<string> tableHeaders;
		private List<List<string>> originalData;

		public ClientManagement(DBAccess _dbAccess)
		{
			InitializeComponent();
			dbAccess = _dbAccess;

			List<List<string>> data = dbAccess.GetAllFromTable("Contact");
			DtgMethods.CreateTable(data, "Contact", dbAccess, ref dtgTest, ref tableHeaders, ref table);
			List<string> colSearch = new List<string> { "All Columns" };
			colSearch.AddRange(tableHeaders);
			cmbColumn.SelectedIndex = 0;
			cmbColumn.ItemsSource = colSearch;
			originalData = data;
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
			DtgMethods.UpdateSearch(originalData, cmbColumn.SelectedIndex, tbxSearch.Text, "Contact", dbAccess, ref dtgTest, ref tableHeaders, ref table);
		}

		private void DtgTest_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
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
	}
}
