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
		private string[] selectedData;
		private Control[] displayElements;
		private FrameworkElement[] labelElements;

		DBObjects.Column[] columns;

		Grid grdEditMode;
		Grid grdAddMode;

		private DataTable dataTable;
		private List<List<string>> currentData;

		public ContactManagement()
		{
			InitializeComponent();

			cmbTable.ItemsSource = DBMethods.MetaRequests.GetTableNames();
			tableName = cmbTable.Items[0].ToString();
			Setup();
			CreateUI();
			try { dtgContacts.SelectedIndex = 0; }
			catch { }
			cmbTable.SelectedIndex = 0;
		}

		private void Tbx_TextChanged(object sender, TextChangedEventArgs e)
		{
			IsValid();
		}

		private bool IsValid()
		{
			bool isValid = true;
			string errCol1 = "";
			string errCol2 = "";
			for (int i = 0; i < columns.Length; i++)
			{
				string str = "";
				if (displayElements[i] is TextBox tbx) str = tbx.Text;
				else if (displayElements[i] is Label l) continue;
				bool patternReq = true;
				bool typeReq = true;
				bool fKeyReq = true;
				bool pKeyReq = true;

				if (i == 0)
				{ }

				string patternError = "";
				DBObjects.Column col = columns[i];
				if (col.Name.Contains("Email"))
				{
					patternReq = PatternValidation.IsValidEmail(str);
					patternError = "Please enter a valid email address.";
				}
				else if (col.Name.Contains("Postcode"))
				{
					patternReq = PatternValidation.IsValidPostcode(str);
					patternError = "Please enter a valid postcode.";
				}
				else if (col.Name.Contains("PhoneNo"))
				{
					patternReq = PatternValidation.IsValidPhoneNo(str);
					patternError = "Please enter a valid phone number.";
				}
				else if (col.Name.Contains("DogGender"))
				{
					patternReq = PatternValidation.IsValidDogGender(str);
					patternError = "Please enter a valid dog gender. (M/F)";
				}

				switch (col.Constraints.Type)
				{
					case "int":
						typeReq = !string.IsNullOrEmpty(str) && str.All(char.IsDigit);
						break;
					case "bit":
						typeReq = PatternValidation.IsBit(str);
						break;
					case "datetime":
						typeReq = DateTime.TryParse(str, out DateTime d);
						break;
					case "date":
						patternReq = PatternValidation.IsValidDate(str);
						break;
				}

				if (col.Constraints.ForeignKey != null && typeReq)
					fKeyReq = DBMethods.MiscRequests.DoesMeetForeignKeyReq(col.Constraints.ForeignKey, str);
				if (col.Constraints.IsPrimaryKey && typeReq)
					pKeyReq = DBMethods.MiscRequests.IsPKeyFree(tableName, col.Name, str);

				// Note: No good way to validate names/addresses

				bool isInstanceValid = patternReq && typeReq && fKeyReq && pKeyReq;
				isValid = isValid && isInstanceValid;

				if (!isInstanceValid)
				{
					string instErr = $"\n{col.Name}: ";
					if (!patternReq) instErr += patternError;

					if (!typeReq)
					{
						switch (col.Constraints.Type)
						{
							case "int": instErr += "Please enter a number!"; break;
							case "bit": instErr += "Please enter True/False/1/0!"; break;
							case "date": instErr += "Please enter a valid date!"; break;
							case "datetime": instErr += "Please enter a valid date & time!"; break;
						}
					}

					if (!fKeyReq) instErr += $"References a non-existent {col.Constraints.ForeignKey.ReferencedTable}.";
					if (!pKeyReq) instErr += "This ID is already taken!";

					if (errCol1.Count(x => x == '\n') < 6) errCol1 += instErr;
					else errCol2 += instErr;
				}

				if (isInstanceValid)
					displayElements[i].Background = Brushes.White;
				else
					displayElements[i].Background = Brushes.Red;
			}
			tbcErr1.Text = errCol1;
			tbcErr2.Text = errCol2;
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
				bool isNew = DBMethods.MiscRequests.IsPKeyFree(tableName, columns[0].Name, selectedData[0]);
				try
				{
					DBMethods.DBAccess.UpdateTable(tableName, columns.Select(c => c.Name).ToArray(), selectedData, isNew);
					succeeded = true;
				}
				catch
				{
					succeeded = false;
				}

				if (succeeded)
				{
					if (!isNew)
					{
						((DataRowView)dtgContacts.SelectedCells[0].Item).Row.ItemArray = selectedData;
						if (tableName == "Contact") ((DataRowView)dtgContactsToClient.SelectedCells[0].Item).Row.ItemArray = selectedData;

						b.Content = "Changes saved!";
						await Task.Delay(2000);
						b.Content = "Save Changes";
					}
					else
					{
						currentData.Add(selectedData.ToList());
						dataTable.Rows.Add(selectedData);
						dtgContacts.ItemsSource = dataTable.DefaultView;
						dtgContacts.SelectedIndex = dataTable.Rows.Count - 1;
						dtgContacts.ScrollIntoView(dtgContacts.SelectedItem);
					}
				}
				else
				{
					selectedData = oldData;
					b.Content = "Error occurred!";
				}
			}
		}

		private void BtnRevert_Click(object sender, RoutedEventArgs e)
		{
			ChangeSelectedData();
		}

		private void BtnAddNew_Click(object sender, RoutedEventArgs e)
		{
			EditToAdd();
		}

		private void EditToAdd()
		{
			grdEditMode.Visibility = Visibility.Hidden;
			grdAddMode.Visibility = Visibility.Visible;
			dtgContacts.SelectedIndex = -1;
			dtgContactsToClient.SelectedIndex = -1;
			dtgContacts.IsEnabled = false;
			dtgContactsToClient.IsEnabled = false;

			for (int i = 0; i < displayElements.Length; i++)
			{
				Control c = displayElements[i];
				if (c is TextBox t) t.Text = "";
				else if (c is Label l)
				{
					grd.Children.Remove(l);
					c = new TextBox()
					{
						Height = 34,
						Margin = new Thickness(l.Margin.Left + 5, l.Margin.Top, 0, 0),
						Tag = "Label"
					};
					((TextBox)c).TextChanged += Tbx_TextChanged;
					displayElements[i] = c;
					grd.Children.Add(c);
				}
			}
		}

		private void AddToEdit()
		{
			grdEditMode.Visibility = Visibility.Visible;
			grdAddMode.Visibility = Visibility.Hidden;
			dtgContacts.SelectedIndex = 0;
			dtgContactsToClient.SelectedIndex = 0;
			dtgContacts.IsEnabled = true;
			dtgContactsToClient.IsEnabled = true;

			for (int i = 0; i < displayElements.Length; i++)
			{
				Control c = displayElements[i];
				if (c is TextBox t) t.Text = "";
				if (c.Tag != null && c.Tag.ToString() == "Label")
				{
					grd.Children.Remove(c);
					c = new Label()
					{
						Margin = new Thickness(c.Margin.Left - 5, c.Margin.Top, 0, 0)
					};
					displayElements[i] = c;
					grd.Children.Add(c);
				}
			}
		}

		private void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
		{
			DeleteRow();
		}

		private void DeleteRow(bool deleteRef = false)
		{
			DataRowView drv = (DataRowView)dtgContacts.SelectedItem;
			if (drv == null) return;
			try
			{
				DBMethods.MiscRequests.DeleteItem(tableName, columns[0].Name, drv.Row.ItemArray[0].ToString(), deleteRef);
				dataTable.Rows.Remove(drv.Row);
			}
			catch (Exception ex)
			{
				grdFKeyErrorOuter.Visibility = Visibility.Visible;
				tblFKeyRefError.Text = ex.Message;
			}
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
			EmailManagement.SendInvoiceEmail("atempmailfortestingcsharp@gmail.com", dataTable, columns.Select(c => c.Name).ToArray());
		}

		private void DtgTest_LoadingRow(object sender, DataGridRowEventArgs e)
		{
			DataGridRow r = e.Row;
			DataRowView v = (DataRowView)r.Item;
			string[] strArr = v.Row.ItemArray.Cast<string>().ToArray();

			if (tableName == "Contact")
			{
				if (Convert.ToInt32(strArr[1]) % 2 == 0) r.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#161616");
				else r.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#252526");
			}
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
			DtgMethods.UpdateSearch(currentData, cmbColumn.SelectedIndex, tbxSearch.Text, tableName, ref dtgContacts, columns, ref dataTable);
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
			if (e.AddedCells.Count == 0) return;
			if (tableName == "Contact")
			{
				try
				{
					DataRowView drv = (DataRowView)dtgContacts.SelectedItems[0];
					string contactID = (string)drv.Row.ItemArray[0];
					string clientID = (string)drv.Row.ItemArray[1];
					List<List<string>> data = DBMethods.MiscRequests.GetByColumnData(tableName, "ClientID", clientID, columns.Select(c => c.Name).ToArray());
					DataTable dt = new DataTable();
					DtgMethods.CreateTable(data, tableName, ref dtgContactsToClient, columns, ref dt, true);
					for (int i = 0; i < data.Count; i++)
						if (data[i][0] == contactID)
							dtgContactsToClient.SelectedIndex = i;
				}
				catch { }
			}
			else
			{
				selectedData = ((DataRowView)dtgContacts.SelectedItems[0]).Row.ItemArray.OfType<string>().ToArray();
				ChangeSelectedData();
			}
		}

		private void DtgContactsToClient_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
		{
			if (e.AddedCells.Count == 0) return;
			selectedData = ((DataRowView)dtgContactsToClient.SelectedItems[0]).Row.ItemArray.OfType<string>().ToArray();
			ChangeSelectedData();
		}

		private void CmbTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			tableName = cmbTable.SelectedItem.ToString();
			ClearUI();
			Setup();
			CreateUI();
			try { dtgContacts.SelectedIndex = 0; }
			catch { }
		}

		private void ClearUI()
		{
			if (displayElements == null) return;
			foreach (Control c in displayElements) grd.Children.Remove(c);
			foreach (FrameworkElement l in labelElements) grd.Children.Remove(l);
		}

		private void Setup()
		{
			// TODO: Spaces in column names
			columns = DBMethods.MetaRequests.GetColumnDataFromTable(tableName);
			currentData = DBMethods.MetaRequests.GetAllFromTable(tableName, columns.Select(c => c.Name).ToArray());
			if (tableName == "Contact")
				dtgContactsToClient.Visibility = Visibility.Visible;
			else dtgContactsToClient.Visibility = Visibility.Hidden;

			DtgMethods.CreateTable(currentData, tableName, ref dtgContacts, columns, ref dataTable, true);

			List<string> colSearch = new List<string> { "All Columns" };
			colSearch.AddRange(columns.Select(c => c.Name));
			cmbColumn.SelectedIndex = 0;
			cmbColumn.ItemsSource = colSearch;
		}

		private void CreateUI()
		{
			int count = columns.Length;
			displayElements = new Control[count];
			labelElements = new FrameworkElement[count];
			selectedData = new string[count];
			GenerateUI(count);
			DisplayUI();
		}

		private void GenerateUI(int count)
		{
			double yOffset = 40;
			double xOffset = 0;
			for (int i = 0; i < count; i++)
			{
				if (yOffset > 600)
				{
					yOffset = 40;
					xOffset += 250;
				}

				Label lbl = new Label()
				{
					Content = columns[i].Name,
					Margin = new Thickness(900 + xOffset, yOffset, 0, 0)
				};
				labelElements[i] = lbl;
				yOffset += 35;

				Control c;
				if (columns[i].Constraints.IsPrimaryKey)
				{
					c = new Label()
					{
						Content = "",
						Margin = new Thickness(900 + xOffset, yOffset, 0, 0)
					};
					yOffset += 35;
				}
				else
				{
					c = new TextBox()
					{
						Height = 34,
						Margin = new Thickness(905 + xOffset, yOffset, 0, 0)
					};
					((TextBox)c).TextChanged += Tbx_TextChanged;
					if (columns[i].Constraints.Type == "varchar")
						if (Convert.ToInt32(columns[i].Constraints.MaxSize) > 50)
							c.Height *= 2;
					yOffset += c.Height + 10;
				}
				displayElements[i] = c;
			}

			grd.Children.Remove(grdEditMode);
			grdEditMode = new Grid()
			{
				Margin = new Thickness(905 + xOffset, yOffset, 0, 0),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top
			};

			grd.Children.Remove(grdAddMode);
			grdAddMode = new Grid()
			{
				Margin = new Thickness(905 + xOffset, yOffset, 0, 0),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Visibility = Visibility.Hidden
			};

			grd.Children.Add(grdAddMode);
			grd.Children.Add(grdEditMode);

			Button btnSave = new Button()
			{
				Content = "Save Changes",
				Margin = new Thickness(0, 0, 0, 0)
			};

			Button btnRevert = new Button()
			{
				Content = "Revert Changes",
				Margin = new Thickness(180, 0, 0, 0)
			};

			Button btnAddNew = new Button()
			{
				Content = "Add New",
				Margin = new Thickness(0, 45, 0, 0)
			};

			Button btnDeleteItem = new Button()
			{
				Content = "Delete Item",
				Margin = new Thickness(180, 45, 0, 0)
			};

			btnSave.Click += BtnSave_Click;
			btnRevert.Click += BtnRevert_Click;
			btnAddNew.Click += BtnAddNew_Click;
			btnDeleteItem.Click += BtnDeleteItem_Click; ;
			grdEditMode.Children.Add(btnSave);
			grdEditMode.Children.Add(btnRevert);
			grdEditMode.Children.Add(btnAddNew);
			grdEditMode.Children.Add(btnDeleteItem);

			Button btnInsertNew = new Button()
			{
				Content = "Add new item",
				Margin = new Thickness(0, 0, 0, 0)
			};

			Button btnCancelAddition = new Button()
			{
				Content = "Cancel Addition",
				Margin = new Thickness(180, 0, 0, 0)
			};

			btnInsertNew.Click += BtnSave_Click;
			btnCancelAddition.Click += BtnCancelAddition_Click;
			grdAddMode.Children.Add(btnInsertNew);
			grdAddMode.Children.Add(btnCancelAddition);
		}

		private void BtnCancelAddition_Click(object sender, RoutedEventArgs e)
		{
			AddToEdit();
		}

		private void DisplayUI()
		{
			foreach (UIElement e in labelElements) grd.Children.Add(e);
			foreach (UIElement e in displayElements) grd.Children.Add(e);
		}

		private void BtnFkeyErrorAccept_Click(object sender, RoutedEventArgs e)
		{
			DeleteRow(true);
			grdFKeyErrorOuter.Visibility = Visibility.Hidden;
		}

		private void BtnFkeyErrorDecline_Click(object sender, RoutedEventArgs e)
		{
			grdFKeyErrorOuter.Visibility = Visibility.Hidden;
		}
	}
}
