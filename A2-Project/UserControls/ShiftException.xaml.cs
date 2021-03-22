using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace A2_Project.UserControls
{
	/// <summary>
	/// Interaction logic for Shift_Exception.xaml
	/// </summary>
	public partial class ShiftException : UserControl
	{
		private DBObjects.Column[] columns;
		private ContentWindows.ShiftManager container;

		private string[] prevData;

		public ShiftException(List<string> data, ContentWindows.ShiftManager _container)
		{
			InitializeComponent();
			container = _container;

			columns = DBObjects.DB.Tables.Where(t => t.Name == "Shift Exception").First().Columns;

			Tag = data[0];

			ValidatedTextbox tbxNewExStaff = new ValidatedTextbox(columns[1]);
			tbxNewExStaff.SetWidth(100);
			tbxNewExStaff.ToggleImage();
			tbxNewExStaff.Width = 90;
			tbxNewExStaff.Text = data[1];
			tbxNewExStaff.AddTextChangedEvent(TextChangedEvent);
			stp.Children.Add(tbxNewExStaff);

			ValidatedDatePicker dtpNewExStart = new ValidatedDatePicker(columns[2]);
			dtpNewExStart.ToggleImage();
			dtpNewExStart.SetWidth(160);
			dtpNewExStart.Text = data[2];
			dtpNewExStart.AddTextChangedEvent(TextChangedEvent);
			stp.Children.Add(dtpNewExStart);

			Label lbl = new Label()
			{
				Content = " to ",
				FontSize = 20
			};
			stp.Children.Add(lbl);

			ValidatedDatePicker dtpNewExEnd = new ValidatedDatePicker(columns[3]);
			dtpNewExEnd.ToggleImage();
			dtpNewExEnd.SetWidth(160);
			dtpNewExStart.Height = 40;
			dtpNewExEnd.Text = data[3];
			dtpNewExEnd.AddTextChangedEvent(TextChangedEvent);
			stp.Children.Add(dtpNewExEnd);

			prevData = GetNewData(out _);
		}

		private void LblDelete_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			container.RemoveShiftExc(this);
		}

		public string[] GetData()
		{
			return prevData;
		}

		private string[] GetNewData(out bool isValid)
		{
			string id = Tag.ToString();
			ValidatedTextbox tbxStaff = stp.Children.OfType<ValidatedTextbox>().First();
			ValidatedDatePicker[] dates = stp.Children.OfType<ValidatedDatePicker>().ToArray();
			isValid = tbxStaff.IsValid && dates[0].IsValid && dates[1].IsValid;
			return new string[] { id, tbxStaff.Text, dates[0].SelectedDate.ToString("yyyy-MM-dd"), dates[1].SelectedDate.ToString("yyyy-MM-dd") };
		}

		private void TextChangedEvent(object sender, TextChangedEventArgs e)
		{
			string[] newData = GetNewData(out bool isValid);
			if (isValid && !newData.SequenceEqual(prevData))
			{
				btnSave.Visibility = System.Windows.Visibility.Visible;
			}
			else btnSave.Visibility = System.Windows.Visibility.Collapsed;
		}

		private void BtnSave_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string[] newData = GetNewData(out _);
			prevData = newData;
			container.UpdateShiftExc(this);
		}
	}
}
