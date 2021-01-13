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

		private string mode = "";

		public ClientManagement()
		{
			InitializeComponent();

			contactsColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Contact");
			dtgContacts = new SearchableDataGrid(400, 950, "Contact", contactsColumns, this);
			lblContacts.Content = dtgContacts.Content;

			dogsColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Dog");
			dtgDogs = new SearchableDataGrid(400, 600, "Dog", dogsColumns, this);
			lblDogs.Content = dtgDogs.Content;
		}

		public void TableSelectionChanged(SearchableDataGrid sender, string[] newData)
		{
			if (!shouldUpdate) return;
			if (sender == dtgContacts)
			{
				UpdateMode("Contacts");
			}
			else if (sender == dtgDogs)
			{
				UpdateMode("Dogs");
			}
			UpdateEditingSidebar(newData);
		}

		private void UpdateMode(string newMode)
		{
			shouldUpdate = false;
			mode = newMode;

			if (mode == "Contacts")
			{
				dtgContacts.ClearSearch();
				string[] selectedData = dtgContacts.GetSelectedData();
				if (selectedData is null) return;
				dtgDogs.ChangeSearch(1, selectedData[1]);
			}
			else
			{
				dtgDogs.ClearSearch();
				string[] selectedData = dtgDogs.GetSelectedData();
				if (selectedData is null) return;
				dtgContacts.ChangeSearch(1, selectedData[1]);
			}
			shouldUpdate = true;
		}

		private void UpdateEditingSidebar(string[] newData)
		{
			if (mode == "Dogs")
			{
				if (dogsEditing is null) dogsEditing = new DataEditingSidebar(dogsColumns, "Dog", this);
				dogsEditing.ChangeSelectedData(newData);
				lblEditing.Content = dogsEditing.Content;
				dtgContacts.UpdateSelectedIndex(-1);
			}
			else
			{
				if (contactEditing is null) contactEditing = new DataEditingSidebar(contactsColumns, "Contact", this);
				contactEditing.ChangeSelectedData(newData);
				lblEditing.Content = contactEditing.Content;
				dtgDogs.UpdateSelectedIndex(-1);
			}
		}
	}
}
