using System;
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
		private object parent;

		public ItemSelectionWindow(UserControls.ValidatedTextbox _parent)
		{
			Initialise(_parent, _parent.Column.Constraints.ForeignKey.ReferencedTable);
		}

		public ItemSelectionWindow(object _parent, string _tableName)
		{
			Initialise(_parent, _tableName);
		}

		private void Initialise(object _parent, string _tableName)
		{
			InitializeComponent();
			parent = _parent;
			dtg = new FilterableDataGrid(_tableName, this);
			dtg.SetMaxHeight(600);
			lblDtg.Content = dtg.Content;

			Height = double.NaN;
			Width = double.NaN;

			Button btnConfirm = new Button()
			{
				Content = "Confirm Selection",
				FontSize = 20,
				HorizontalAlignment = HorizontalAlignment.Center,
				Margin = new Thickness(0, 5, 0, 10)
			};
			btnConfirm.Click += BtnConfirm_Click;
			stp.Children.Add(btnConfirm);
		}

		private void BtnConfirm_Click(object sender, RoutedEventArgs e)
		{
			string[] data = dtg.GetSelectedData();

			if (data is null) Close();

			if (parent is UserControls.ValidatedTextbox tbx)
				tbx.Text = data[0];
			else if (parent is CalandarView calView)
				calView.SelectSpecificAppointment(data);
			else
				throw new NotImplementedException();

			Close();
		}

		internal void SelectItem(string text)
		{
			dtg.SelectItem(text);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (parent is CalandarView && IsVisible)
			{
				// To maintain the user's previous selection, hide the window so it can be shown again later instead of closing it.
				e.Cancel = true;
				Hide();
			}
		}
	}
}
