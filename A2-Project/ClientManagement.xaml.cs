using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace A2_Project
{
	/// <summary>
	/// Interaction logic for ClientManagement.xaml
	/// </summary>
	public partial class ClientManagement : Window
	{
		private DBAccess dbAccess;
		private DataTable table;
		private List<string> tableHeaders;

		public ClientManagement(DBAccess _dbAccess)
		{
			InitializeComponent();
			dbAccess = _dbAccess;
			CreateTableForResults(dbAccess.GetAllFromTable("Contact"));
		}

		private void CreateTableForResults(List<List<string>> data)
		{
			table = new DataTable();
			tableHeaders = dbAccess.GetColumnHeadersFromTable("Contact");
			foreach (string str in tableHeaders)
				table.Columns.Add(str);
			for (int i = 0; i < data.Count; i++)
				table.Rows.Add(data[i].ToArray());

			data_Order.DataContext = table.DefaultView;
		}
	}
}
