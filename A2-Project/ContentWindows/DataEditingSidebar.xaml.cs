using A2_Project.UserControls;
using System;
using System.Collections.Generic;
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
		private readonly object container;
		private FrameworkElement[] displayElements;
		private readonly DBObjects.Column[] columns;
		private string[] selectedData;
		private readonly string tableName;
		private Label lblIsAppInitial;
		private Label lblIsNewBooking;
		private Label lblErrMessage;

		private TextBlock tbcErr1;
		private TextBlock tbcErr2;

		Grid grdEditMode;
		Grid grdAddMode;

		public DataEditingSidebar(DBObjects.Column[] _columns, string _tableName, object _container, bool canAddOrDelete = true)
		{
			columns = _columns;
			tableName = _tableName;
			container = _container;

			tbcErr1 = new TextBlock()
			{
				MaxHeight = 300,
				Margin = new Thickness(5, -20, 0, 0),
				IsHitTestVisible = false
			};
			tbcErr2 = new TextBlock()
			{
				IsHitTestVisible = false
			};

			InitializeComponent();

			GenUI(canAddOrDelete);
		}

		private bool UpdateToOwner(string[] data, bool isNew)
		{
			if (container is AllTableManger contact)
			{
				contact.UpdateFromSidebar(data, isNew);
				return true;
			}
			else if (container is CalandarView calandar)
			{
				return calandar.UpdateFromSidebar(data, isNew);
			}
			else if (container is ClientManagement cliMan)
			{
				cliMan.UpdateFromSidebar(data, isNew);
				return true;
			}
			else throw new NotImplementedException();
		}

		private void DeleteItemOwner()
		{
			if (container is AllTableManger contactManagement)
			{
				contactManagement.DeleteItem();
				EmptySidebar();
			}
			else if (container is CalandarView calanderView)
			{
				calanderView.CancelApp();
			}
			else if (container is ClientManagement cliMan)
			{
				cliMan.DeleteItem();
				EmptySidebar();
			}
			else throw new NotImplementedException();
		}

		/// <summary>
		/// Returns true if the user entered data is valid
		/// </summary>
		public bool IsValid(out string errMessage)
		{
			bool isAllValid = true;

			for (int i = 0; i < columns.Length; i++)
			{
				if (displayElements[i] is Label lbl) isAllValid = isAllValid && int.TryParse(lbl.Content.ToString(), out _);

				if (displayElements[i] is ValidatedItem item)
				{
					bool isItemValid = item.IsValid;

					try
					{
						if (!isItemValid && container is CalandarView cal && item.Column.Name == "Booking ID")
						{
							if (DBMethods.MiscRequests.IsPKeyFree("Appointment", "Appointment ID", ((Label)displayElements[0]).Content.ToString()))
							{
								if (Convert.ToInt32(item.Text) == Convert.ToInt32(cal.GetNewBookingID()))
								{
									item.IsValid = true;
									isItemValid = true;
									continue;
								}
							}
						}
					}
					catch { }
					isAllValid = isAllValid && isItemValid;
				}
			}
			errMessage = UpdateErrorMessages();
			return isAllValid;
		}

		public void UpdateErrorEvent(object sender, EventArgs e)
		{
			UpdateErrorMessages();
		}

		private string UpdateErrorMessages()
		{
			// Used to display error messages to the user
			string errCol1 = "";
			string errCol2 = "";
			for (int i = 0; i < columns.Length; i++)
			{
				string instErr = "";
				if (displayElements[i] is ValidatedItem item)
				{
					if (item.IsValid) continue;
					instErr = item.ErrorMessage;

					try
					{
						if (container is CalandarView cal && item.Column.Name == "Booking ID")
						{
							if (DBMethods.MiscRequests.IsPKeyFree("Appointment", "Appointment ID", ((Label)displayElements[0]).Content.ToString()))
							{
								if (Convert.ToInt32(item.Text) == Convert.ToInt32(cal.GetNewBookingID()))
								{
									item.IsValid = true;
									continue;
								}
							}
						}
					}
					catch { }
				}
				// Allows the error messages to be readable when there are more than 6 of them by dividing them into 2 columns
				if (errCol1.Count(x => x == '\n') < 6) errCol1 += instErr;
				else errCol2 += instErr;
			}
			tbcErr1.Text = errCol1;
			tbcErr2.Text = errCol2;
			return errCol1 + errCol2;
		}

		internal string[] GetData()
		{
			List<string> data = new List<string>();

			foreach (FrameworkElement elem in displayElements)
			{
				if (elem is Label lbl) data.Add(lbl.Content.ToString());
				else if (elem is ValidatedItem valItem) data.Add(valItem.Text);
				else if (elem is ComboBox cbxCombo) data.Add(cbxCombo.SelectedIndex.ToString());
				else if (elem is CheckBox cbxCheck) data.Add(cbxCheck.IsChecked.Value ? "True" : "False");
				else throw new NotImplementedException();
			}

			return data.ToArray();
		}

		/// <summary>
		/// Move from edit mode to add mode
		/// </summary>
		public void EditToAdd()
		{
			grdEditMode.Visibility = Visibility.Collapsed;
			grdAddMode.Visibility = Visibility.Visible;

			for (int i = 0; i < displayElements.Length; i++)
			{
				FrameworkElement c = displayElements[i];
				// Reset the values in the controls
				if (c is ValidatedTextbox tbx) tbx.Text = UIMethods.GetSuggestedValue(columns[i]).ToString();
				else if (c is ComboBox cmb) cmb.SelectedIndex = -1;
				else if (c is DatePicker d) d.SelectedDate = (DateTime)UIMethods.GetSuggestedValue(columns[i]);
				else if (c is CheckBox cbx) cbx.IsChecked = (bool)UIMethods.GetSuggestedValue(columns[i]);
				else if (c is Label l)
				{
					l.Content = DBMethods.MiscRequests.GetMinKeyNotUsed(tableName, columns[i].Name);
				}
			}
		}

		internal void StartAddNew(string clientID)
		{
			EditToAdd();
			if (tableName != "Client") ((ValidatedItem)displayElements[1]).Text = clientID;
			else ((ValidatedDatePicker)displayElements[2]).SelectedDate = DateTime.Now.Date;
		}

		/// <summary>
		/// Move from add mode to edit mode
		/// </summary>
		public void AddToEdit()
		{
			grdEditMode.Visibility = Visibility.Visible;
			grdAddMode.Visibility = Visibility.Collapsed;

			for (int i = 0; i < displayElements.Length; i++)
			{
				FrameworkElement c = displayElements[i];
				// Reset the displayed values
				if (c is ValidatedTextbox t) t.Text = "";
				else if (displayElements[i] is ComboBox cmb) cmb.SelectedIndex = -1;
				// If the element is to display a primary key, it should not be editable, so a label is used instead of a text box
				if (c.Tag != null && c.Tag.ToString() == "Primary Key")
				{
					Panel cOwner = (Panel)c.Parent;
					cOwner.Children.Remove(c);
					c = new Label()
					{
						Margin = new Thickness(c.Margin.Left - 5, c.Margin.Top, 0, 0)
					};
					displayElements[i] = c;
					cOwner.Children.Add(c);
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

			if (selectedData is not null && selectedData[0] == "No Results!") EmptySidebar();

			for (int i = 0; i < selectedData.Length; i++)
			{
				if (displayElements[i] is Label l) l.Content = selectedData[i];
				else if (displayElements[i] is ValidatedTextbox t) t.Text = selectedData[i];
				else if (displayElements[i] is ComboBox cmb) cmb.SelectedIndex = int.Parse(selectedData[i]);
				else if (displayElements[i] is CheckBox c) c.IsChecked = selectedData[i] == "True";
				else if (displayElements[i] is ValidatedDatePicker cDP) cDP.SelectedDate = DateTime.Parse(selectedData[i]);
			}

			CheckIsInitialApp();
			CheckIsNewApp();
			CheckIsInShift();
		}

		#region Programmatic UI Generation
		private void GenUI(bool canAddOrDelete)
		{
			int count = columns.Length;
			displayElements = new FrameworkElement[count];
			selectedData = new string[count];

			GenDataEntry(count);

			GenAddEditBtns(0, canAddOrDelete);
		}

		/// <summary>
		/// Generate the items used for entering data
		/// </summary>
		private void GenDataEntry(int count)
		{
			List<StackPanel> panels = new List<StackPanel>();
			StackPanel currentPanel = new StackPanel()
			{
				Orientation = Orientation.Vertical,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top
			};
			panels.Add(currentPanel);

			for (int i = 0; i < count; i++)
			{
				// Split the items into multiple columns if needed
				if (currentPanel.Children.Count > 10)
				{
					currentPanel = new StackPanel()
					{
						Orientation = Orientation.Vertical,
						HorizontalAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Top,
						Margin = new Thickness(-200, 0, 0, 0)
					};
					panels.Add(currentPanel);
				}

				// Generate the label used for displaying the column name
				Label lbl = new Label()
				{
					Margin = new Thickness(0, 20, 0, 0)
				};

				FrameworkElement elem = UIMethods.GenAppropriateElement(columns[i], out string title);
				lbl.Content = title;
				if (elem is ValidatedItem v)
				{
					v.AddTextChangedEvent(UpdateErrorEvent);
				}

				if (elem is ValidatedTextbox tbx && tableName == "Appointment" && columns[i].Name == "Dog ID")
				{
					tbx.AddTextChangedEvent(TbxDogId_TextChanged);
				}
				
				displayElements[i] = elem;
				currentPanel.Children.Add(lbl);
				currentPanel.Children.Add(elem);
			}

			StackPanel panel = new StackPanel()
			{
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top
			};
			foreach (StackPanel p in panels)
			{
				panel.Children.Add(p);
			}
			stp.Children.Add(panel);
		}

		/// <summary>
		/// Generate the buttons used in add mode and edit mode
		/// </summary>
		private void GenAddEditBtns(double yOffset, bool canAddOrDelete)
		{
			grdEditMode = new Grid()
			{
				Margin = new Thickness(5, yOffset + 20, 0, 0),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top
			};

			grdAddMode = new Grid()
			{
				Margin = new Thickness(5, yOffset + 20, 0, 0),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Visibility = Visibility.Collapsed
			};

			stp.Children.Add(grdAddMode);
			stp.Children.Add(grdEditMode);
			stp.Children.Add(tbcErr1);

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

			lblIsAppInitial = new Label()
			{
				Content = "Note: This is this dog's first appointment,\nso it will take an extra 15 minutes.",
				Visibility = Visibility.Collapsed
			};
			stp.Children.Add(lblIsAppInitial);

			lblIsNewBooking = new Label()
			{
				Content = "Note: This is a new appointment.\nGo to Booking to confirm it.",
				Visibility = Visibility.Collapsed
			};
			stp.Children.Add(lblIsNewBooking);

			lblErrMessage = new Label()
			{
				Content = "",
				Visibility = Visibility.Collapsed,
				Foreground = new SolidColorBrush(Color.FromRgb(222, 24, 39))
			};
			stp.Children.Add(lblErrMessage);

			if (tableName == "Appointment")
			{
				Button btnCancelApp = new Button()
				{
					Content = "Cancel Appt.",
					Margin = new Thickness(0, 45, 0, 0)
				};
				Button btnCancelBooking = new Button()
				{
					Content = "Cancel Booking",
					Margin = new Thickness(180, 45, 0, 0)
				};
				btnCancelApp.Click += BtnCancelApp_Click;
				btnCancelBooking.Click += BtnCancelBooking_Click;
				grdEditMode.Children.Add(btnCancelApp);
				grdEditMode.Children.Add(btnCancelBooking);
			}

			if (canAddOrDelete)
			{
				if (container is ClientManagement)
				{
					btnDeleteItem.Margin = new Thickness(0, 45, 0, 0);
					grdEditMode.Children.Add(btnDeleteItem);
				}
				else
				{
					grdEditMode.Children.Add(btnDeleteItem);
					grdEditMode.Children.Add(btnAddNew);
				}
			}

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

		private void BtnCancelBooking_Click(object sender, RoutedEventArgs e)
		{
			if (container is CalandarView cal) cal.CancelBooking(GetData());
			else throw new NotImplementedException();
			EmptySidebar();
		}

		private void BtnCancelApp_Click(object sender, RoutedEventArgs e)
		{
			if (container is CalandarView cal)
			{
				if (cal.CancelApp()) EmptySidebar();
			}
		}

		public void EmptySidebar()
		{
			foreach (FrameworkElement elem in displayElements)
			{
				if (elem is ValidatedItem validItem) validItem.Text = "";
				else if (elem is ComboBox combo) combo.SelectedIndex = -1;
				else if (elem is CheckBox check) check.IsChecked = false;
				else if (elem is Label lbl) lbl.Content = "";
				else throw new NotImplementedException();
			}
			GetData();
		}

		private void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
		{
			DeleteItemOwner();
		}
		#endregion Programmatic UI Generation

		#region Edit Mode
		/// <summary>
		/// Saves the users changes or adds the item to the database if it does not yet exist
		/// </summary>
		private void BtnSave_Click(object sender, RoutedEventArgs e)
		{
			SaveChanges((Button)sender);
		}

		private async void SaveChanges(Button b)
		{
			// Checks if the entered data is valid before allowing the user to make changes
			if (IsValid(out _))
			{
				// Moves the data entered into the text boxes to an array
				for (int i = 0; i < selectedData.Length; i++)
				{
					if (displayElements[i] is Label l) selectedData[i] = l.Content.ToString();
					else if (displayElements[i] is ValidatedTextbox tbx) selectedData[i] = tbx.Text;
					else if (displayElements[i] is ComboBox cmb) selectedData[i] = cmb.SelectedIndex.ToString();
					else if (displayElements[i] is CheckBox c) selectedData[i] = c.IsChecked.ToString();
					else if (displayElements[i] is ValidatedDatePicker d) selectedData[i] = d.SelectedDate.ToString("dd/MM/yyyy");
				}

				// Allows the user to be alerted if an error occurred while trying to save their changes
				// Checks if the item already exists and needs updated or is new and needs inserted by checking if the primary key is taken
				bool isNew = DBMethods.MiscRequests.IsPKeyFree(tableName, columns[0].Name, selectedData[0]);
				bool succeeded = UpdateToOwner(selectedData, isNew);

				if (b is not null)
				{
					if (succeeded)
					{
						// Tell the user their changes have been saved without displaying an intrusive message
						b.Content = "Changes saved!";
						await Task.Delay(2000);
						b.Content = "Save Changes";
					}
					else
					{
						b.Content = "Error occurred!";
						await Task.Delay(2000);
						b.Content = "Save Changes";
					}
				}
			}
		}

		internal void DisplayError(string errMessage)
		{
			if (errMessage == "")
			{
				lblErrMessage.Visibility = Visibility.Collapsed;
			}
			else
			{
				lblErrMessage.Content = errMessage;
				lblErrMessage.Visibility = Visibility.Visible;
			}
		}

		/// <summary>
		/// Reverts the user's changes to the selected item
		/// </summary>
		private void BtnRevert_Click(object sender, RoutedEventArgs e)
		{
			// Note: Does not work after the "save changes" button is clicked.
			ChangeSelectedData();
		}

		private void BtnAddNew_Click(object sender, RoutedEventArgs e)
		{
			if (container is ClientManagement)
			{
				throw new NotImplementedException();
			}
			else EditToAdd();
		}
		#endregion Edit Mode

		private void CheckIsInitialApp()
		{
			if (tableName == "Appointment" && container is CalandarView calView && DBMethods.MiscRequests.IsAppointmentInitial(GetData(), calView.BookingParts))
			{
				lblIsAppInitial.Visibility = Visibility.Visible;
			}
			else lblIsAppInitial.Visibility = Visibility.Collapsed;
		}

		private void CheckIsNewApp()
		{
			if (tableName == "Appointment" && container is CalandarView && DBMethods.MiscRequests.IsPKeyFree("Appointment", "Appointment ID", GetData()[0]))
			{
				lblIsNewBooking.Visibility = Visibility.Visible;
			}
			else lblIsNewBooking.Visibility = Visibility.Collapsed;
		}

		private void CheckIsInShift()
		{
			if (tableName == "Appointment" && container is CalandarView calView && !DBMethods.MiscRequests.IsAppInShift(selectedData, calView.BookingParts))
			{
				DisplayError("This appointment does not fit into staff schedules!");
			}
		}

		private void TbxDogId_TextChanged(object sender, TextChangedEventArgs e)
		{
			CheckIsInitialApp();
		}

		#region AddMode
		private void BtnCancelAddition_Click(object sender, RoutedEventArgs e)
		{
			if (container is ClientManagement) EditToAdd();
			else AddToEdit();
		}
		#endregion AddMode
	}
}
