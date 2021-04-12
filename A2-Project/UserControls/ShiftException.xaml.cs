using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace A2_Project.UserControls
{
	/// <summary>
	/// Interaction logic for Shift_Exception.xaml
	/// </summary>
	public partial class ShiftException : UserControl
	{
		private readonly DBObjects.Column[] columns;
		private readonly ContentWindows.ShiftManager container;

		private string[] prevData;

		public ShiftException(List<string> data, ContentWindows.ShiftManager _container)
		{
			InitializeComponent();
			container = _container;

			columns = DBObjects.DB.Tables.Where(t => t.Name == "Shift Exception").First().Columns;

			Tag = data[0];

			ComboBox cbxExStaff = (ComboBox)UIMethods.GenAppropriateElement(columns[1], out _);
			cbxExStaff.LayoutTransform = new ScaleTransform(1.5, 1.5);
			cbxExStaff.Margin = new Thickness(0, 0, 10, 0);
			cbxExStaff.SelectedIndex = int.Parse(data[1]);
			cbxExStaff.SelectionChanged += CbxExStaff_SelectionChanged;
			stp.Children.Add(cbxExStaff);

			ValidatedDatePicker dtpExStart = new ValidatedDatePicker(columns[2]);
			dtpExStart.ToggleImage();
			dtpExStart.SetWidth(160);
			dtpExStart.Text = data[2];
			dtpExStart.AddTextChangedEvent(TextChangedEvent);
			stp.Children.Add(dtpExStart);

			Label lbl = new Label()
			{
				Content = " to ",
				FontSize = 20
			};
			stp.Children.Add(lbl);

			ValidatedDatePicker dtpExEnd = new ValidatedDatePicker(columns[3]);
			dtpExEnd.ToggleImage();
			dtpExEnd.SetWidth(160);
			dtpExStart.Height = 40;
			dtpExEnd.Text = data[3];
			dtpExEnd.AddTextChangedEvent(TextChangedEvent);
			stp.Children.Add(dtpExEnd);

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
			ComboBox cbxStaff = stp.Children.OfType<ComboBox>().First();
			ValidatedDatePicker[] dates = stp.Children.OfType<ValidatedDatePicker>().ToArray();
			isValid = dates[0].IsValid && dates[1].IsValid;
			return new string[] { id, cbxStaff.SelectedIndex.ToString(), dates[0].SelectedDate.ToString("yyyy-MM-dd"), dates[1].SelectedDate.ToString("yyyy-MM-dd") };
		}

		private void TextChangedEvent(object sender, TextChangedEventArgs e)
		{
			OnChanged();
		}

		private void CbxExStaff_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			OnChanged();
		}

		private void OnChanged()
		{
			string[] newData = GetNewData(out bool isValid);
			if (isValid && !newData.SequenceEqual(prevData))
			{
				btnSave.Visibility = Visibility.Visible;
			}
			else btnSave.Visibility = Visibility.Collapsed;
		}

		private void BtnSave_Click(object sender, RoutedEventArgs e)
		{
			string[] newData = GetNewData(out _);
			prevData = newData;
			container.UpdateShiftExc(this);
		}
	}
}
