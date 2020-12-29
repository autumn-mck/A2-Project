using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for DataEditingSidebar.xaml
	/// </summary>
	public partial class DataEditingSidebar : Window
	{
		private readonly Window containingWindow;
		private FrameworkElement[] displayElements;
		private FrameworkElement[] labelElements;
		private string errCol1;
		private string errCol2;
		private readonly DBObjects.Column[] columns;
		private string[] selectedData;
		private readonly string tableName;

		Grid grdEditMode;
		Grid grdAddMode;

		public DataEditingSidebar(DBObjects.Column[] _columns, string _tableName, Window _containingWindow)
		{
			columns = _columns;
			tableName = _tableName;
			containingWindow = _containingWindow;

			InitializeComponent();

			GenUI();
		}

		private void UpdateToOwner(string[] data, bool isNew)
		{
			if (containingWindow is ContactManagement contact)
			{
				contact.UpdateFromSidebar(data, isNew);
			}
			else if (containingWindow is CalandarView calandar)
			{
				calandar.UpdateFromSidebar(data, isNew);
			}
		}

		private void DeleteRowOwner()
		{
			if (containingWindow is ContactManagement c)
			{
				c.DeleteRow();
			}
		}

		/// <summary>
		/// Returns true if the user entered data is valid
		/// </summary>
		private bool IsValid()
		{
			bool isAllValid = true;

			// Used to display error messages to the user
			errCol1 = "";
			errCol2 = "";

			for (int i = 0; i < columns.Length; i++)
			{
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
						patternError = "Please enter a valid postcode. ";
					}
					else if (col.Name.Contains("PhoneNo"))
					{
						patternReq = PatternValidation.IsValidPhoneNo(str);
						patternError = "Please enter a valid phone number. ";
					}
					else if (col.Name.Contains("DogGender"))
					{
						patternReq = PatternValidation.IsValidDogGender(str);
						patternError = "Please enter a valid dog gender. (M/F)";
					}

					if (col.Constraints.CanBeNull && str == "") patternReq = true;
					else if (!col.Constraints.CanBeNull && str == "")
					{
						patternReq = false;
						patternError = "This value cannot be left empty! ";
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
			tbcErr1.Text = errCol1;
			tbcErr2.Text = errCol2;
			return isAllValid;
		}

		/// <summary>
		/// Move from edit mode to add mode
		/// </summary>
		private void EditToAdd()
		{
			grdEditMode.Visibility = Visibility.Hidden;
			grdAddMode.Visibility = Visibility.Visible;

			for (int i = 0; i < displayElements.Length; i++)
			{
				FrameworkElement c = displayElements[i];
				// Reset the values in the controls
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

		private object GetSuggestedValue(DBObjects.Column column)
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

			for (int i = 0; i < displayElements.Length; i++)
			{
				FrameworkElement c = displayElements[i];
				// Reset the displayed values
				if (c is TextBox t) t.Text = "";
				// If the element is to display a primary key, it should not be editable, so a label is used instead of a text box
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

		/// <summary>
		/// Updates the data displayed to the user
		/// </summary>
		public void ChangeSelectedData(string[] data = null)
		{
			if (data != null)
				selectedData = data;
			for (int i = 0; i < selectedData.Length; i++)
			{
				if (displayElements[i] is Label l) l.Content = selectedData[i];
				else if (displayElements[i] is TextBox t) t.Text = selectedData[i];
				else if (displayElements[i] is CheckBox c) c.IsChecked = selectedData[i] == "True";
				else if (displayElements[i] is CustomDatePicker cDP) cDP.SelectedDate = DateTime.Parse(selectedData[i]);
			}
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
					Margin = new Thickness(xOffset, yOffset, 0, 0)
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
						Margin = new Thickness(xOffset, yOffset, 0, 0)
					};
					yOffset += 35;
				}
				// An SQL bit is a boolean value, so a checkbox can be used to help prevent insertion of invalid data
				else if (columns[i].Constraints.Type == "bit")
				{
					c = new CheckBox()
					{
						Margin = new Thickness(5 + xOffset, yOffset, 0, 0),
						RenderTransform = new ScaleTransform(2, 2)
					};
					yOffset += 30;
				}
				else if (columns[i].Constraints.Type == "date")
				{
					c = new CustomDatePicker()
					{
						Margin = new Thickness(5 + xOffset, yOffset, 0, 0),
						Width = 200 / 1.5,
						Height = 40,
						FontSize = 16,
						RenderTransform = new ScaleTransform(1.5, 1.5),
						HorizontalAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Top
					};
					((CustomDatePicker)c).AddNewTextChanged(CustomDatePicker_TextChanged);
					yOffset += 100;
				}
				// Otherwise, a text box is used to allow the user to enter data
				else
				{
					c = new TextBox()
					{
						Height = 34,
						Margin = new Thickness(5 + xOffset, yOffset, 0, 0)
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
				Margin = new Thickness(5, yOffset, 0, 0),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top
			};

			grdAddMode = new Grid()
			{
				Margin = new Thickness(5, yOffset, 0, 0),
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

		private void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
		{
			DeleteRowOwner();
		}
		#endregion Programmatic UI Generation

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
					UpdateToOwner(selectedData, isNew);
					succeeded = true;
				}
				catch
				{
					succeeded = false;
				}

				if (succeeded)
				{
					// Tell the user their changes have been saved without displaying an intrusive message
					b.Content = "Changes saved!";
					await Task.Delay(2000);
					b.Content = "Save Changes";
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

		public void CustomDatePicker_TextChanged(object sender, TextChangedEventArgs e)
		{
			IsValid();
		}

		/// <summary>
		/// Allows the user to be given feedback on the validity of their changes as they types
		/// </summary>
		private void Tbx_TextChanged(object sender, TextChangedEventArgs e)
		{
			IsValid();
		}
	}
}
