using A2_Project.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for DataEditingSidebar.xaml
	/// </summary>
	public partial class DataEditingSidebar : Window
	{
		private readonly Window containingWindow;
		private FrameworkElement[] displayElements;
		// TODO: What is this for?
		private FrameworkElement[] labelElements;
		private readonly DBObjects.Column[] columns;
		private string[] selectedData;
		private readonly string tableName;

		private TextBlock tbcErr1;
		private TextBlock tbcErr2;

		Grid grdEditMode;
		Grid grdAddMode;

		public DataEditingSidebar(DBObjects.Column[] _columns, string _tableName, Window _containingWindow, bool canAddOrDelete = true)
		{
			columns = _columns;
			tableName = _tableName;
			containingWindow = _containingWindow;

			tbcErr1 = new TextBlock()
			{
				MaxHeight = 300,
				Margin = new Thickness(5, -20, 0, 0)
			};
			tbcErr2 = new TextBlock();

			InitializeComponent();

			GenUI(canAddOrDelete);
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
			else if (containingWindow is ClientManagement cliMan)
			{
				cliMan.UpdateFromSidebar(data, isNew);
			}
		}

		private void DeleteItemOwner()
		{
			if (containingWindow is ContactManagement contactManagement)
			{
				contactManagement.DeleteItem();
			}
			else if (containingWindow is CalandarView calanderView)
			{
				calanderView.CancelApp();
			}
			else if (containingWindow is ClientManagement cliMan)
			{
				cliMan.DeleteItem();
			}
		}

		/// <summary>
		/// Returns true if the user entered data is valid
		/// </summary>
		private bool IsValid()
		{
			bool isAllValid = true;

			for (int i = 0; i < columns.Length; i++)
			{
				if (displayElements[i] is ValidatedItem item)
				{
					isAllValid = isAllValid && item.IsValid;
					continue;
				}
			}
			UpdateErrorMessages();
			return isAllValid;
		}

		public void UpdateErrorEvent(object sender, EventArgs e)
		{
			UpdateErrorMessages();
		}

		private void UpdateErrorMessages()
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
				}
				// Allows the error messages to be readable when there are more than 6 of them by dividing them into 2 columns
				if (errCol1.Count(x => x == '\n') < 6) errCol1 += instErr;
				else errCol2 += instErr;
			}
			tbcErr1.Text = errCol1;
			tbcErr2.Text = errCol2;
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
				// Any labels which were used to display primary keys now need to be editable
				else if (c is Label l)
				{
					Panel cOwner = (Panel)l.Parent;
					cOwner.Children.Remove(l);
					c = new ValidatedTextbox(columns[i])
					{
						Margin = new Thickness(l.Margin.Left + 5, l.Margin.Top, 0, 0),
						Tag = "Primary Key",
						Text = UIMethods.GetSuggestedValue(columns[i]).ToString(),
						HorizontalAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Top
					};
					((ValidatedTextbox)c).AddTextChangedEvent(UpdateErrorEvent);
					displayElements[i] = c;
					cOwner.Children.Add(c);
				}
			}
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
			for (int i = 0; i < selectedData.Length; i++)
			{
				if (displayElements[i] is Label l) l.Content = selectedData[i];
				else if (displayElements[i] is ValidatedTextbox t) t.Text = selectedData[i];
				else if (displayElements[i] is ComboBox cmb) cmb.SelectedIndex = int.Parse(selectedData[i]);
				else if (displayElements[i] is CheckBox c) c.IsChecked = selectedData[i] == "True";
				else if (displayElements[i] is ValidatedDatePicker cDP) cDP.SelectedDate = DateTime.Parse(selectedData[i]);
			}
		}

		#region Programmatic UI Generation
		private void GenUI(bool canAddOrDelete)
		{
			int count = columns.Length;
			displayElements = new FrameworkElement[count];
			labelElements = new FrameworkElement[count];
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
				labelElements[i] = lbl;

				FrameworkElement elem = UIMethods.GenAppropriateElement(columns[i], out string title);
				lbl.Content = title;
				if (elem is ValidatedItem v)
				{
					v.AddTextChangedEvent(UpdateErrorEvent);
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

			// TODO: Does this bit work properly?
			//stp.Children.Add(grdAddMode);
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

			if (canAddOrDelete)
			{
				grdEditMode.Children.Add(btnAddNew);
				grdEditMode.Children.Add(btnDeleteItem);
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

		private void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
		{
			DeleteItemOwner();
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
					else if (displayElements[i] is ValidatedTextbox tbx) selectedData[i] = tbx.Text;
					else if (displayElements[i] is ComboBox cmb) selectedData[i] = cmb.SelectedIndex.ToString();
					else if (displayElements[i] is CheckBox c) selectedData[i] = c.IsChecked.ToString();
					else if (displayElements[i] is ValidatedDatePicker d) selectedData[i] = ((DateTime)d.SelectedDate).ToString("dd/MM/yyyy");
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
					//if (isNew) AddToEdit();
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

		public void HideButtons()
		{
			// TODO: Why is this method here?
			grdAddMode.Visibility = Visibility.Collapsed;
			grdEditMode.Visibility = Visibility.Collapsed;
		}

		#region AddMode
		private void BtnCancelAddition_Click(object sender, RoutedEventArgs e)
		{
			AddToEdit();
		}
		#endregion AddMode
	}
}
