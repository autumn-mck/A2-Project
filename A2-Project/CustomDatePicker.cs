using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace A2_Project
{
	public class CustomDatePicker : DatePicker
	{
		protected DatePickerTextBox _datePickerTextBox;
		public bool IsValid { get; set; }

		public void AddNewTextChanged(TextChangedEventHandler handler)
		{
			if (_datePickerTextBox != null)
				_datePickerTextBox.TextChanged += handler;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_datePickerTextBox = Template.FindName("PART_TextBox", this) as DatePickerTextBox;
			if (_datePickerTextBox != null)
				_datePickerTextBox.TextChanged += IsValidDate_TextChanged;
		}

		private void IsValidDate_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (DateTime.TryParse(_datePickerTextBox.Text, out DateTime dt) && _datePickerTextBox.Text.Length > 5)
				IsValid = true;
			else
				IsValid = false;
		}
	}
}
