using System;
using System.Linq;
using System.Windows;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for ClientManagement.xaml
	/// </summary>
	public partial class ClientManagement : Window
	{
		private bool shouldUpdate = true;

		private SearchableDataGrid dtgContacts;
		private DBObjects.Column[] contactsColumns;
		private DataEditingSidebar contactEditing;

		private SearchableDataGrid dtgDogs;
		private DBObjects.Column[] dogsColumns;
		private DataEditingSidebar dogsEditing;

		private SearchableDataGrid dtgClients;
		private DBObjects.Column[] clientsColumns;
		private DataEditingSidebar clientsEditing;

		private string mode = "";

		public ClientManagement()
		{
			InitializeComponent();

			contactsColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Contact");
			dtgContacts = new SearchableDataGrid(300, 950, "Contact", contactsColumns, this);
			lblContacts.Content = dtgContacts.Content;

			dogsColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Dog");
			dtgDogs = new SearchableDataGrid(300, 600, "Dog", dogsColumns, this);
			lblDogs.Content = dtgDogs.Content;

			clientsColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Client");
			dtgClients = new SearchableDataGrid(200, 600, "Client", clientsColumns, this);
			lblClients.Content = dtgClients.Content;
		}

		internal void UpdateFromSidebar(string[] data, bool isNew)
		{
			switch (mode)
			{
				case "Contacts": dtgContacts.UpdateData(data, isNew); break;
				case "Clients": dtgClients.UpdateData(data, isNew); break;
				case "Dogs": dtgDogs.UpdateData(data, isNew); break;
			}
		}

		internal void DeleteItem(bool deleteRef = false)
		{
			try
			{
				switch (mode)
				{
					case "Contacts": dtgContacts.TryDeleteSelected(deleteRef); break;
					case "Clients": dtgClients.TryDeleteSelected(deleteRef); break;
					case "Dogs": dtgDogs.TryDeleteSelected(deleteRef); break;
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

		public void TableSelectionChanged(SearchableDataGrid sender, string[] newData)
		{
			if (!shouldUpdate) return;
			shouldUpdate = false;
			if (sender == dtgContacts)
			{
				UpdateMode("Contacts");
			}
			else if (sender == dtgDogs)
			{
				UpdateMode("Dogs");
			}
			else if (sender == dtgClients)
			{
				UpdateMode("Clients");
			}
			UpdateEditingSidebar(newData);
			shouldUpdate = true;
		}

		private void UpdateMode(string newMode)
		{
			if (mode == newMode) return;
			mode = newMode;
			
			double notSelMax = 100;
			double selMax = 600;

			if (mode == "Contacts")
			{
				dtgContacts.ClearSearch();
				string[] selectedData = dtgContacts.GetSelectedData();
				if (selectedData is null) return;
				dtgDogs.ChangeSearch(1, selectedData[1]);
				dtgClients.ChangeSearch(0, selectedData[1]);
				dtgContacts.SetMaxHeight(selMax);
			}
			else if (mode == "Dogs")
			{
				dtgDogs.ClearSearch();
				string[] selectedData = dtgDogs.GetSelectedData();
				if (selectedData is null) return;
				dtgContacts.ChangeSearch(1, selectedData[1]);
				dtgClients.ChangeSearch(0, selectedData[1]);
				dtgDogs.SetMaxHeight(selMax);
			}
			else if (mode == "Clients")
			{
				dtgClients.ClearSearch();
				string[] selectedData = dtgClients.GetSelectedData();
				if (selectedData is null) return;
				dtgContacts.ChangeSearch(1, selectedData[0]);
				dtgDogs.ChangeSearch(1, selectedData[0]);
				dtgClients.SetMaxHeight(selMax);
			}


			if (mode != "Dogs") dtgDogs.SetMaxHeight(notSelMax);
			if (mode != "Clients") dtgClients.SetMaxHeight(notSelMax);
			if (mode != "Contacts") dtgContacts.SetMaxHeight(notSelMax);
		}

		private void UpdateEditingSidebar(string[] newData)
		{
			if (mode == "Dogs")
			{
				if (dogsEditing is null) dogsEditing = new DataEditingSidebar(dogsColumns, "Dog", this);
				dogsEditing.ChangeSelectedData(newData);
				dtgContacts.UpdateSelectedIndex(-1);
				dtgClients.UpdateSelectedIndex(-1);
			}
			else if (mode == "Contacts")
			{
				if (contactEditing is null) contactEditing = new DataEditingSidebar(contactsColumns, "Contact", this);
				contactEditing.ChangeSelectedData(newData);
				dtgDogs.UpdateSelectedIndex(-1);
				dtgClients.UpdateSelectedIndex(-1);
			}
			else if (mode == "Clients")
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
			if (mode == "Dogs")
			{
				lblEditing.Content = dogsEditing.Content;
			}
			else if (mode == "Contacts")
			{
				lblEditing.Content = contactEditing.Content;
			}
			else if (mode == "Clients")
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
			grdFKeyErrorOuter.Visibility = Visibility.Hidden;
		}

		/// <summary>
		/// The user does not want to delete the selected item
		/// </summary>
		private void BtnFkeyErrorDecline_Click(object sender, RoutedEventArgs e)
		{
			grdFKeyErrorOuter.Visibility = Visibility.Hidden;
		}
	}
}
