using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

		private string[] selectedData;
		private UIElement[] displayElements;

		DBObjects.Column[] columns;

		private DataTable table;
		// TODO: Merge tableHeaders and columnData
		//private List<string> tableHeaders;
		//private List<List<string>> columnData;
		private List<List<string>> originalData;

		public ContactManagement()
		{
			InitializeComponent();
			// TODO: Spaces in column names
			columns = DBMethods.MetaRequests.GetColumnDataFromTable("Contact");
			//columnData = DBMethods.MetaRequests.GetDataTypesFromTable("Contact");
			List<List<string>> data = DBMethods.MetaRequests.GetAllFromTable("Contact");
			DtgMethods.CreateTable(data, "Contact", ref dtgContacts, columns, ref table);
			List<string> colSearch = new List<string> { "All Columns" };
			colSearch.AddRange(columns.Select(c => c.Name));
			cmbColumn.SelectedIndex = 0;
			cmbColumn.ItemsSource = colSearch;
			originalData = data;


			GenerateElements();
		}

		private void GenerateElements()
		{
			double offset = 40;
			int count = columns.Length;
			displayElements = new UIElement[count];
			selectedData = new string[count];

			for (int i = 0; i < count; i++)
			{
				Label lbl = new Label()
				{
					Content = columns[i].Name,
					Margin = new Thickness(900, offset, 0, 0),
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top
				};
				grd.Children.Add(lbl);
				offset += 35;

				if (columns[i].Constraints.IsPrimaryKey)
				{
					lbl = new Label()
					{
						Content = "test",
						Margin = new Thickness(900, i * 75 + 75, 0, 0),
						HorizontalAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Top
					};
					offset += 34;
					displayElements[i] = lbl;
					grd.Children.Add(lbl);
				}
				else
				{
					TextBox tbx = new TextBox()
					{
						Width = double.NaN,
						MinWidth = 200,
						MaxWidth = 350,
						Height = 34,
						Margin = new Thickness(905, offset, 0, 0),
						FontSize = 24,
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Top
					};
					//i * 75 + 75
					if (columns[i].Constraints.Type == "varchar")
					{
						if (Convert.ToInt32(columns[i].Constraints.MaxSize) > 50)
						{
							tbx.Height *= 2;
						}
					}
					tbx.TextChanged += Tbx_TextChanged;
					offset += tbx.Height + 10;
					displayElements[i] = tbx;
					grd.Children.Add(tbx);
				}
			}

			Button btnSave = new Button()
			{
				Height = 37,
				Width = 160,
				FontSize = 24,
				Content = "Save Changes",
				Name = "btnSave",
				Margin = new Thickness(905, offset, 0, 0),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top
			};
			btnSave.Click += BtnSave_Click;
			grd.Children.Add(btnSave);

			Button btnRevert = new Button()
			{
				Height = btnSave.Height,
				Width = btnSave.Width,
				FontSize = btnSave.FontSize,
				Content = "Revert Changes",
				Name = "btnRevert",
				Margin = new Thickness(btnSave.Margin.Left + 170, btnSave.Margin.Top, 0, 0),
				HorizontalAlignment = btnSave.HorizontalAlignment,
				VerticalAlignment = btnSave.VerticalAlignment
			};
			btnRevert.Click += BtnRevert_Click;
			grd.Children.Add(btnRevert);
		}

		private void Tbx_TextChanged(object sender, TextChangedEventArgs e)
		{
			IsValid();
		}

		private bool IsValid()
		{
			bool isValid = true;
			for (int i = 0; i < columns.Length; i++)
			{
				if (displayElements[i] is TextBox tbx)
				{
					bool patternReq = true;
					bool typeReq = true;
					bool fKeyReq = true;

					if (columns[i].Name.Contains("Email"))
						patternReq = RegExValidation.IsValidEmail(tbx.Text);
					else if (columns[i].Name.Contains("Postcode"))
						patternReq = RegExValidation.IsValidPostcode(tbx.Text);
					else if (columns[i].Name.Contains("PhoneNo"))
						patternReq = RegExValidation.IsValidPhoneNo(tbx.Text);

					if (columns[i].Constraints.Type == "int")
						typeReq = !string.IsNullOrEmpty(tbx.Text) && tbx.Text.All(Char.IsDigit);

					if (columns[i].Constraints.ForeignKey != null && typeReq)
						fKeyReq = DBMethods.MiscRequests.DoesMeetForeignKeyReq(columns[i].Constraints.ForeignKey, tbx.Text);
					// Note: No good way to validate names/addresses

					bool isInstanceValid = patternReq && typeReq && fKeyReq;

					isValid = isValid && isInstanceValid;

					if (isInstanceValid)
						tbx.Background = Brushes.White;
					else
						tbx.Background = Brushes.Red;
				}
			}
			return isValid;
		}

		private async void BtnSave_Click(object sender, RoutedEventArgs e)
		{
			if (IsValid() && sender is Button b)
			{
				string[] oldData = (string[])selectedData.Clone();
				for (int i = 0; i < selectedData.Length; i++)
				{
					if (displayElements[i] is Label l) selectedData[i] = l.Content.ToString();
					else if (displayElements[i] is TextBox t) selectedData[i] = t.Text;
				}

				bool succeeded;
				try
				{
					DBMethods.DBAccess.UpdateTable("Contact", columns.Select(c => c.Name).ToList(), selectedData);
					succeeded = true;
				}
				catch
				{
					succeeded = false;
				}

				if (succeeded)
				{
					((DataRowView)dtgContacts.SelectedCells[0].Item).Row.ItemArray = selectedData;
					((DataRowView)dtgContactsToClient.SelectedCells[0].Item).Row.ItemArray = selectedData;

					b.Content = "Changes saved!";
					await Task.Delay(2000);
					b.Content = "Save Changes";
				}
				else
				{
					selectedData = oldData;
					MessageBox.Show("AAAAAAAAA");
				}
			}
		}

		private void BtnRevert_Click(object sender, RoutedEventArgs e)
		{
			ChangeSelectedData();
		}

		private void ChangeSelectedData()
		{
			for (int i = 0; i < selectedData.Length; i++)
			{
				if (displayElements[i] is Label l) l.Content = selectedData[i];
				else if (displayElements[i] is TextBox t) t.Text = selectedData[i];
			}
		}

		private void BtnEmail_Click(object sender, RoutedEventArgs e)
		{
			EmailManagement.SendInvoiceEmail("atempmailfortestingcsharp@gmail.com", table, columns.Select(c => c.Name).ToArray());
		}

		private void DtgTest_LoadingRow(object sender, DataGridRowEventArgs e)
		{
			DataGridRow r = e.Row;
			DataRowView v = (DataRowView)r.Item;
			string[] strArr = v.Row.ItemArray.Cast<string>().ToArray();

			if (Convert.ToInt32(strArr[1]) % 2 == 0) r.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#161616");
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
			DtgMethods.UpdateSearch(originalData, cmbColumn.SelectedIndex, tbxSearch.Text, "Contact", ref dtgContacts, columns, ref table);
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
				string contactID = (string)drv.Row.ItemArray[0];
				string clientID = (string)drv.Row.ItemArray[1];
				dtgContactsToClient.Columns.Clear(); // TODO: Why is this needed here, but not elsewhere????
				List<List<string>> data = DBMethods.MiscRequests.GetContactsByClientID(clientID);
				DtgMethods.CreateTable(data, "Contact", ref dtgContactsToClient, columns, ref table);
				for (int i = 0; i < data.Count; i++)
					if (data[i][0] == contactID)
						dtgContactsToClient.SelectedIndex = i;
			}
			catch { }
		}

		private void DtgContactsToClient_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
		{
			selectedData = ((DataRowView)dtgContactsToClient.SelectedItems[0]).Row.ItemArray.OfType<string>().ToArray();
			ChangeSelectedData();
		}
	}
}
