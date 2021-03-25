using System;
using System.Windows;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for ClientManagement.xaml
	/// </summary>
	public partial class ClientManagement : Window
	{
		private bool shouldUpdate = true;

		private FilterableDataGrid dtgContacts;
		private DBObjects.Column[] contactsColumns;
		private DataEditingSidebar contactEditing;

		private FilterableDataGrid dtgDogs;
		private DBObjects.Column[] dogsColumns;
		private DataEditingSidebar dogsEditing;

		private FilterableDataGrid dtgClients;
		private DBObjects.Column[] clientsColumns;
		private DataEditingSidebar clientsEditing;

		private const string contactString = "Contacts";
		private const string clientString = "Clients";
		private const string dogString = "Dogs";

		private string mode = "";

		public ClientManagement()
		{
			InitializeComponent();

			contactsColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Contact");
			dtgContacts = new FilterableDataGrid(contactsColumns, this);
			dtgContacts.SetMaxHeight(300);
			lblContacts.Content = dtgContacts.Content;

			dogsColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Dog");
			dtgDogs = new FilterableDataGrid(dogsColumns, this);
			dtgDogs.SetMaxHeight(300);
			lblDogs.Content = dtgDogs.Content;

			clientsColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Client");
			dtgClients = new FilterableDataGrid(clientsColumns, this);
			dtgClients.SetMaxHeight(300);
			lblClients.Content = dtgClients.Content;
		}

		internal void UpdateFromSidebar(string[] data, bool isNew)
		{
			switch (mode)
			{
				case contactString: dtgContacts.UpdateData(data, isNew); break;
				case dogString: dtgDogs.UpdateData(data, isNew); break;
				case clientString: dtgClients.UpdateData(data, isNew); break;
			}
		}

		internal void DeleteItem(bool deleteRef = false)
		{
			try
			{
				switch (mode)
				{
					case contactString: dtgContacts.TryDeleteSelected(deleteRef); break;
					case dogString: dtgDogs.TryDeleteSelected(deleteRef); break;
					case clientString: dtgClients.TryDeleteSelected(deleteRef); break;
				}
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

		public void TableSelectionChanged(FilterableDataGrid sender, string[] newData)
		{
			if (!shouldUpdate) return;
			shouldUpdate = false;
			if (sender == dtgContacts)
			{
				UpdateMode(contactString);
			}
			else if (sender == dtgDogs)
			{
				UpdateMode(dogString);
			}
			else if (sender == dtgClients)
			{
				UpdateMode(clientString);
			}
			UpdateEditingSidebar(newData);
			shouldUpdate = true;
		}

		private void UpdateMode(string newMode)
		{
			if (newMode == mode)
			{
				if (mode == contactString)
				{
					string[] selectedData = dtgContacts.GetSelectedData();
					if (selectedData is null) return;
					dtgDogs.ChangeSearch(1, selectedData[1]);
					dtgClients.ChangeSearch(0, selectedData[1]);
				}
				else if (mode == dogString)
				{
					string[] selectedData = dtgDogs.GetSelectedData();
					if (selectedData is null) return;
					dtgContacts.ChangeSearch(1, selectedData[1]);
					dtgClients.ChangeSearch(0, selectedData[1]);
				}
				else if (mode == clientString)
				{
					string[] selectedData = dtgClients.GetSelectedData();
					if (selectedData is null) return;
					dtgContacts.ChangeSearch(1, selectedData[0]);
					dtgDogs.ChangeSearch(1, selectedData[0]);
				}
				return;
			}

			mode = newMode;
			
			double notSelMax = 150;
			double selMax = 650;

			lblEditingTitle.Content = $"Editing {mode}:";

			if (mode == contactString)
			{
				dtgContacts.ClearSearch();
				string[] selectedData = dtgContacts.GetSelectedData();
				if (selectedData is null) return;
				dtgDogs.ChangeSearch(1, selectedData[1]);
				dtgClients.ChangeSearch(0, selectedData[1]);
				dtgContacts.SetMaxHeight(selMax);
			}
			else if (mode == dogString)
			{
				dtgDogs.ClearSearch();
				string[] selectedData = dtgDogs.GetSelectedData();
				if (selectedData is null) return;
				dtgContacts.ChangeSearch(1, selectedData[1]);
				dtgClients.ChangeSearch(0, selectedData[1]);
				dtgDogs.SetMaxHeight(selMax);
			}
			else if (mode == clientString)
			{
				dtgClients.ClearSearch();
				string[] selectedData = dtgClients.GetSelectedData();
				if (selectedData is null) return;
				dtgContacts.ChangeSearch(1, selectedData[0]);
				dtgDogs.ChangeSearch(1, selectedData[0]);
				dtgClients.SetMaxHeight(selMax);
			}

			if (mode != contactString) dtgContacts.SetMaxHeight(notSelMax);
			if (mode != dogString) dtgDogs.SetMaxHeight(notSelMax);
			if (mode != clientString) dtgClients.SetMaxHeight(notSelMax);
		}

		private void UpdateEditingSidebar(string[] newData)
		{
			if (mode == contactString)
			{
				if (contactEditing is null) contactEditing = new DataEditingSidebar(contactsColumns, "Contact", this);
				contactEditing.ChangeSelectedData(newData);
				dtgDogs.UpdateSelectedIndex(-1);
				dtgClients.UpdateSelectedIndex(-1);
			}
			else if (mode == dogString)
			{
				if (dogsEditing is null) dogsEditing = new DataEditingSidebar(dogsColumns, "Dog", this);
				dogsEditing.ChangeSelectedData(newData);
				dtgContacts.UpdateSelectedIndex(-1);
				dtgClients.UpdateSelectedIndex(-1);
			}
			else if (mode == clientString)
			{
				if (clientsEditing is null) clientsEditing = new DataEditingSidebar(clientsColumns, "Client", this);
				clientsEditing.ChangeSelectedData(newData);
				dtgDogs.UpdateSelectedIndex(-1);
				dtgContacts.UpdateSelectedIndex(-1);
			}

			UpdateClientEditor();
		}

		private void UpdateClientEditor()
		{
			if (mode == contactString)
			{
				lblEditing.Content = contactEditing.Content;
			}
			else if (mode == dogString)
			{
				lblEditing.Content = dogsEditing.Content;
			}
			else if (mode == clientString)
			{
				lblEditing.Content = clientsEditing.Content;
			}
		}

		/// <summary>
		/// The user wants to delete the item and anything else that references it
		/// </summary>
		private void BtnFkeyErrorAccept_Click(object sender, RoutedEventArgs e)
		{
			DeleteItem(true);
			grdFKeyErrorOuter.Visibility = Visibility.Collapsed;
		}

		/// <summary>
		/// The user does not want to delete the selected item
		/// </summary>
		private void BtnFkeyErrorDecline_Click(object sender, RoutedEventArgs e)
		{
			grdFKeyErrorOuter.Visibility = Visibility.Collapsed;
		}
	}
}
