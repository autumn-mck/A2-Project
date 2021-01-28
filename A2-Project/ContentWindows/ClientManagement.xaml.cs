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
	}
}
