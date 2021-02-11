using System.Windows;
using System.Windows.Controls;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for ItemSelectionWindow.xaml
	/// </summary>
	public partial class ItemSelectionWindow : Window
	{
		SearchableDataGrid dtg;
		UserControls.ValidatedItem parent;

		public ItemSelectionWindow(UserControls.ValidatedItem _parent)
		{
			InitializeComponent();
			parent = _parent;
			string refTable = parent.Column.Constraints.ForeignKey.ReferencedTable;
			dtg = new SearchableDataGrid(600, 400, refTable,
			DBMethods.MetaRequests.GetColumnDataFromTable(refTable), this);
			lblDtg.Content = dtg.Content;
			
			Height = 660;
			Width = 700;

			Button btnConfirm = new Button()
			{
				Content = "Confirm Selection"
			};
			btnConfirm.Click += BtnConfirm_Click;
			grd.Children.Add(btnConfirm);
		}

		private void BtnConfirm_Click(object sender, RoutedEventArgs e)
		{
			parent.Text = dtg.GetSelectedData()[0];
			Close();
		}
	}
}
