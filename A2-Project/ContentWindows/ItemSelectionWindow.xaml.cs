using System.Windows;
using System.Windows.Controls;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for ItemSelectionWindow.xaml
	/// </summary>
	public partial class ItemSelectionWindow : Window
	{
		private FilterableDataGrid dtg;
		private UserControls.ValidatedItem parent;

		public ItemSelectionWindow(UserControls.ValidatedItem _parent)
		{
			InitializeComponent();
			parent = _parent;
			string refTable = parent.Column.Constraints.ForeignKey.ReferencedTable;
			dtg = new FilterableDataGrid(refTable, this);
			dtg.SetMaxHeight(600);
			lblDtg.Content = dtg.Content;
			
			Height = double.NaN;
			Width = double.NaN;

			Button btnConfirm = new Button()
			{
				Content = "Confirm Selection",
				FontSize = 20,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			btnConfirm.Click += BtnConfirm_Click;
			stp.Children.Add(btnConfirm);
		}

		private void BtnConfirm_Click(object sender, RoutedEventArgs e)
		{
			parent.Text = dtg.GetSelectedData()[0];
			Close();
		}
	}
}
