using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for ContactManagement.xaml
	/// </summary>
	public partial class ContactManagement : Window
	{
		private static string tableName;
		private string[] selectedData;
		private static FrameworkElement[] displayElements;
		private static FrameworkElement[] labelElements;
		private static string errCol1;
		private static string errCol2;
		private static TextBlock errOut1;
		private static TextBlock errOut2;

		private static DBObjects.Column[] columns;

		Grid grdEditMode;
		Grid grdAddMode;

		private DataTable dataTable;
		private List<List<string>> currentData;

		public ContactManagement()
		{
			InitializeComponent();

			cmbTable.ItemsSource = DBMethods.MetaRequests.GetTableNames();
			tableName = cmbTable.Items[0].ToString();
			errOut1 = tbcErr1;
			errOut2 = tbcErr2;
			Setup();
			GenUI();
			try { dtgContacts.SelectedIndex = 0; }
			catch { }
			cmbTable.SelectedIndex = 0;
		}

		/// <summary>
		/// Returns true if the user entered data is valid
		/// </summary>
		private static bool IsValid()
		{
			bool isAllValid = true;

			// Used to display error messages to the user
			errCol1 = "";
			errCol2 = "";

			for (int i = 0; i < columns.Length; i++)
			{
				// TODO: Allow empty if null values allowed
				// Gets the data to be validated in string form
				string str = "";
				bool patternReq = true;
				bool typeReq = true;
				bool fKeyReq = true;
				bool pKeyReq = true;

				if (displayElements[i] is TextBox tbx) str = tbx.Text;
				else if (displayElements[i] is Label) continue; // If the item to be checked is a label, it is not user editable, so it is assumed to already contain valid data.
				else if (displayElements[i] is CheckBox) continue;
				else if (displayElements[i] is CustomDatePicker cd)
				{
					typeReq = cd.IsValid;
				}
				// TODO: Test if this allows invalid data


				string patternError = "";
				DBObjects.Column col = columns[i];

				if (displayElements[i] is TextBox)
				{

					// Check if the string meets a specific pattern
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

					// Checks if the data meets the requirements for the type of data it should be
					switch (col.Constraints.Type)
					{
						case "int":
							typeReq = !string.IsNullOrEmpty(str) && str.All(char.IsDigit);
							break;
						case "time":
							typeReq = TimeSpan.TryParse(str, out TimeSpan t);
							typeReq = typeReq && t.TotalMinutes % 1.0 == 0;
							break;
					}

					// Checks if the data meets foreign key requirements if needed
					if (col.Constraints.ForeignKey != null && typeReq)
						fKeyReq = DBMethods.MiscRequests.DoesMeetForeignKeyReq(col.Constraints.ForeignKey, str);

					// Checks if the data meets primary key requirements if needed
					if (col.Constraints.IsPrimaryKey && typeReq)
						pKeyReq = DBMethods.MiscRequests.IsPKeyFree(tableName, col.Name, str);
				}
				// Note: No good way to validate names/addresses

				bool isInstanceValid = patternReq && typeReq && fKeyReq && pKeyReq;
				isAllValid = isAllValid && isInstanceValid;

				// If the current part is invalid, let the user know what the issue is.
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
							case "time": instErr += "Please enter a valid time! (hh:mm)"; break;
						}
					}

					if (!fKeyReq) instErr += $"References a non-existent {col.Constraints.ForeignKey.ReferencedTable}.";
					if (!pKeyReq) instErr += "This ID is already taken!";

					// Allows the error messages to be readable when there are more than 6 of them by dividing them into 2 columns
					if (errCol1.Count(x => x == '\n') < 6) errCol1 += instErr;
					else errCol2 += instErr;
				}

				// Allow the user to clearly see which data is incorrect
				if (displayElements[i] is Control c)
				{
					if (isInstanceValid)
						c.Background = Brushes.White;
					else
						c.Background = Brushes.Red;
				}
				else if (displayElements[i] is Grid g)
				{
					if (isInstanceValid)
						g.Background = Brushes.White;
					else
						g.Background = Brushes.Red;
				}
			}
			errOut1.Text = errCol1;
			errOut2.Text = errCol2;
			return isAllValid;
		}

		/// <summary>
		/// Move from edit mode to add mode
		/// </summary>
		private void EditToAdd()
		{
			grdEditMode.Visibility = Visibility.Hidden;
			grdAddMode.Visibility = Visibility.Visible;

			// Deselect the selected item and disable the DataGrids to prevent new items from being selected
			// TODO: Change so that DataGrids can still be scrolled through, but not selected?
			dtgContacts.SelectedIndex = -1;
			dtgContactsToClient.SelectedIndex = -1;
			dtgContacts.IsEnabled = false;
			dtgContactsToClient.IsEnabled = false;

			for (int i = 0; i < displayElements.Length; i++)
			{
				FrameworkElement c = displayElements[i];
				// Reset the values in the controls
				// TODO: Insert suggested values instead
				if (c is TextBox t) t.Text = GetSuggestedValue(columns[i]).ToString();
				else if (c is DatePicker d) d.SelectedDate = (DateTime)GetSuggestedValue(columns[i]);
				else if (c is CheckBox cbx) cbx.IsChecked = (bool)GetSuggestedValue(columns[i]);
				// Any labels which were used to display primary keys now need to be editable
				else if (c is Label l)
				{
					grd.Children.Remove(l);
					c = new TextBox()
					{
						Height = 34,
						Margin = new Thickness(l.Margin.Left + 5, l.Margin.Top, 0, 0),
						Tag = "Primary Key",
						Text = GetSuggestedValue(columns[i]).ToString()
					};
					((TextBox)c).TextChanged += Tbx_TextChanged;
					displayElements[i] = c;
					grd.Children.Add(c);
				}
			}
		}

		private static object GetSuggestedValue(DBObjects.Column column)
		{
			if (column.Constraints.Type == "date") return DateTime.Now.Date;
			else if (column.Constraints.Type == "bit") return false;
			else if (column.Constraints.IsPrimaryKey && column.Constraints.ForeignKey == null) return DBMethods.MiscRequests.GetMinKeyNotUsed(tableName, column.Name);
			else return "";
		}

		/// <summary>
		/// Move from add mode to edit mode
		/// </summary>
		private void AddToEdit()
		{
			grdEditMode.Visibility = Visibility.Visible;
			grdAddMode.Visibility = Visibility.Hidden;

			// TODO: This may cause errors
			dtgContacts.SelectedIndex = 0;
			dtgContactsToClient.SelectedIndex = 0;

			// Re-enable the datagrids
			dtgContacts.IsEnabled = true;
			dtgContactsToClient.IsEnabled = true;

			for (int i = 0; i < displayElements.Length; i++)
			{
				FrameworkElement c = displayElements[i];
				// Reset the displayed values
				if (c is TextBox t) t.Text = "";
				// If the element is to display a primary key, it should not be editable, so a label is used instead of a textbox
				// TODO: Is that fixable?
				if (c.Tag != null && c.Tag.ToString() == "Primary Key")
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

		/// <summary>
		/// Tries to delete the selected row
		/// </summary>
		private void DeleteRow(bool deleteRef = false)
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

		/// <summary>
		/// Updates the data displayed to the user
		/// </summary>
		private void ChangeSelectedData()
		{
			for (int i = 0; i < selectedData.Length; i++)
			{
				if (displayElements[i] is Label l) l.Content = selectedData[i];
				else if (displayElements[i] is TextBox t) t.Text = selectedData[i];
				else if (displayElements[i] is CheckBox c) c.IsChecked = selectedData[i] == "True";
				else if (displayElements[i] is CustomDatePicker cDP) cDP.SelectedDate = DateTime.Parse(selectedData[i]);
			}
		}

		private void Search()
		{
			DtgMethods.UpdateSearch(currentData, cmbColumn.SelectedIndex, tbxSearch.Text, tableName, ref dtgContacts, columns, ref dataTable);
		}

		#region Programmatic UI Generation
		/// <summary>
		/// Removes the UI Elements generated for the previous table
		/// </summary>
		private void ClearUI()
		{
			if (displayElements == null) return;
			// Remove the user-editable controls
			foreach (FrameworkElement fr in displayElements) grd.Children.Remove(fr);
			// Remove the column labels
			foreach (FrameworkElement l in labelElements) grd.Children.Remove(l);
			// Remove the grids used for displaying button options
			grd.Children.Remove(grdAddMode);
			grd.Children.Remove(grdEditMode);
		}

		/// <summary>
		/// Setup for changing to a new table
		/// </summary>
		private void Setup()
		{
			// TODO: Spaces in column names
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

		private void GenUI()
		{
			int count = columns.Length;
			displayElements = new FrameworkElement[count];
			labelElements = new FrameworkElement[count];
			selectedData = new string[count];

			// A yOffset and xOffset are used to display all elements in the correct position
			double yOffset = 40;
			double xOffset = 0;
			GenDataEntry(count, ref yOffset, ref xOffset, out double maxYOffset);

			// Display the already generated elements
			foreach (UIElement e in labelElements) grd.Children.Add(e);
			foreach (UIElement e in displayElements) grd.Children.Add(e);

			GenAddEditBtns(maxYOffset);
		}

		/// <summary>
		/// Generate the items used for entering data
		/// </summary>
		private void GenDataEntry(int count, ref double yOffset, ref double xOffset, out double maxYOffset)
		{
			maxYOffset = 0;
			for (int i = 0; i < count; i++)
			{
				// Split the items into multiple columns if needed
				if (yOffset > 600)
				{
					yOffset = 40;
					xOffset += 250;
				}

				// Generate the label used for displaying the column name
				Label lbl = new Label()
				{
					Content = columns[i].Name,
					Margin = new Thickness(900 + xOffset, yOffset, 0, 0)
				};
				labelElements[i] = lbl;
				yOffset += 35;

				FrameworkElement c;
				// If the item displays a primary key, it is not editable by the user, so a label is used to display the data without it being editable
				if (columns[i].Constraints.IsPrimaryKey)
				{
					c = new Label()
					{
						Content = "",
						Margin = new Thickness(900 + xOffset, yOffset, 0, 0)
					};
					yOffset += 35;
				}
				// An SQL bit is a boolean value, so a checkbox can be used to help prevent insertion of invalid data
				else if (columns[i].Constraints.Type == "bit")
				{
					c = new CheckBox()
					{
						Margin = new Thickness(905 + xOffset, yOffset, 0, 0),
						RenderTransform = new ScaleTransform(2, 2)
					};
					yOffset += 30;
				}
				else if (columns[i].Constraints.Type == "date")
				{
					c = new CustomDatePicker()
					{
						Margin = new Thickness(905 + xOffset, yOffset, 0, 0),
						Width = 200/1.5,
						Height = 40,
						FontSize = 16,
						RenderTransform = new ScaleTransform(1.5, 1.5),
						HorizontalAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Top
					};
					yOffset += 100;
				}
				// Otherwise, a text box is used to allow the user to enter data
				else
				{
					c = new TextBox()
					{
						Height = 34,
						Margin = new Thickness(905 + xOffset, yOffset, 0, 0)
					};
					((TextBox)c).TextChanged += Tbx_TextChanged;

					// If the text box has the potential of containing a lot of data, double its height to allow the text it contains to be easier to read.
					// TODO: Enforce max length
					if (columns[i].Constraints.Type == "varchar")
						if (Convert.ToInt32(columns[i].Constraints.MaxSize) > 50)
							c.Height *= 2;
					yOffset += c.Height + 10;
				}
				displayElements[i] = c;
				maxYOffset = Math.Max(maxYOffset, yOffset);
			}
		}

		private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			IsValid();
		}

		/// <summary>
		/// Generate the buttons used in add mode and edit mode
		/// </summary>
		private void GenAddEditBtns(double yOffset)
		{
			grdEditMode = new Grid()
			{
				Margin = new Thickness(905, yOffset, 0, 0),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top
			};

			grdAddMode = new Grid()
			{
				Margin = new Thickness(905, yOffset, 0, 0),
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
		#endregion Programmatic UI Generation

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
			if (tableName == "Contact") // If the user is looking at contacts, they should be able to see other contacts from the same client. TODO: Consider removing
			{
				try
				{
					// Gets the selected contact's ID and relevant contact ID
					DataRowView drv = (DataRowView)dtgContacts.SelectedItems[0];
					string contactID = (string)drv.Row.ItemArray[0];
					string clientID = (string)drv.Row.ItemArray[1];
					// Gets other contacts with the same client ID
					List<List<string>> data = DBMethods.MiscRequests.GetByColumnData(tableName, "ClientID", clientID, columns.Select(c => c.Name).ToArray());
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
				selectedData = ((DataRowView)dtgContacts.SelectedItems[0]).Row.ItemArray.OfType<string>().ToArray();
				ChangeSelectedData();
			}
		}

		/// <summary>
		/// Update the selected data when the user selected a different item
		/// </summary>
		private void DtgContactsToClient_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
		{
			if (e.AddedCells.Count == 0) return;
			selectedData = ((DataRowView)dtgContactsToClient.SelectedItems[0]).Row.ItemArray.OfType<string>().ToArray();
			ChangeSelectedData();
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

		#region Edit Mode
		/// <summary>
		/// Saves the users changes or adds the item to the database if it does not yet exist
		/// </summary>
		private async void BtnSave_Click(object sender, RoutedEventArgs e)
		{
			// Checks if the entered data is valid before allowing the user to make changes
			if (IsValid())
			{
				Button b = (Button)sender;

				// Moves the data entered into the text boxes to an array
				for (int i = 0; i < selectedData.Length; i++)
				{
					if (displayElements[i] is Label l) selectedData[i] = l.Content.ToString();
					else if (displayElements[i] is TextBox t) selectedData[i] = t.Text;
					else if (displayElements[i] is CheckBox c) selectedData[i] = c.IsChecked.ToString();
					else if (displayElements[i] is CustomDatePicker d) selectedData[i] = ((DateTime)d.SelectedDate).ToString("dd/MM/yyyy");
				}

				// Allows the user to be alerted if an error occurred while trying to save their changes
				bool succeeded;
				// Checks if the item already exists and needs updated or is new and needs inserted by checking if the primary key is taken
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
					// If it is an existing item that needs updates, update the table displayed to the user without requesting all data from the database again
					if (!isNew)
					{
						((DataRowView)dtgContacts.SelectedCells[0].Item).Row.ItemArray = selectedData;
						if (tableName == "Contact") ((DataRowView)dtgContactsToClient.SelectedCells[0].Item).Row.ItemArray = selectedData;

						// Tell the user their changes have been saved without displaying an intrusive message
						b.Content = "Changes saved!";
						await Task.Delay(2000);
						b.Content = "Save Changes";
					}
					// Otherwise if the item needs to be inserted
					else
					{
						// Add the new item to the list of all current data
						currentData.Add(selectedData.ToList());
						// Add it to the DataTable
						dataTable.Rows.Add(selectedData);
						dtgContacts.ItemsSource = dataTable.DefaultView;
						// Select the item in the DataGrid and scroll to it
						dtgContacts.SelectedIndex = dataTable.Rows.Count - 1;
						dtgContacts.ScrollIntoView(dtgContacts.SelectedItem);
					}
				}
				// Note: This should never occur, but if something does go wrong then notify the user
				else
				{
					b.Content = "Error occurred!";
				}
			}
		}

		/// <summary>
		/// Reverts the user's changes to the selected item
		/// </summary>
		private void BtnRevert_Click(object sender, RoutedEventArgs e)
		{
			ChangeSelectedData();
		}

		private void BtnAddNew_Click(object sender, RoutedEventArgs e)
		{
			EditToAdd();
		}
		#endregion Edit Mode

		#region AddMode
		private void BtnCancelAddition_Click(object sender, RoutedEventArgs e)
		{
			AddToEdit();
		}
		#endregion AddMode

		/// <summary>
		/// Allows the user to be given feedback on the validity of their changes as they types
		/// </summary>
		private void Tbx_TextChanged(object sender, TextChangedEventArgs e)
		{
			IsValid();
		}

		/// <summary>
		/// Updates the UI and the DataTables whenever the selected  table is changed
		/// </summary>
		private void CmbTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			tableName = cmbTable.SelectedItem.ToString();
			ClearUI();
			Setup();
			GenUI();
			try { dtgContacts.SelectedIndex = 0; }
			catch { }
		}

		public static void CustomDatePicker_TextChanged(object sender, TextChangedEventArgs e)
		{
			IsValid();
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
	}

	public class CustomDatePicker : DatePicker
	{
		protected DatePickerTextBox _datePickerTextBox;
		public bool IsValid { get; set; }

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_datePickerTextBox = Template.FindName("PART_TextBox", this) as DatePickerTextBox;
			if (_datePickerTextBox != null)
			{
				_datePickerTextBox.TextChanged += Dptb_TextChanged;
				_datePickerTextBox.TextChanged += ContactManagement.CustomDatePicker_TextChanged;
			}
		}

		private void Dptb_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (DateTime.TryParse(_datePickerTextBox.Text, out DateTime dt) && _datePickerTextBox.Text.Length > 5)
			{
				//SelectedDate = dt;
				IsValid = true;
			}
			else
			{
				IsValid = false;
			}
		}
	}
}