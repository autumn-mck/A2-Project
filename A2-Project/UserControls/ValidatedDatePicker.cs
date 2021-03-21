using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace A2_Project.UserControls
{
	public class ValidatedDatePicker : ValidatedItem
	{
		private CustomizableDatePicker datePicker;
		private double scale = 1.5;

		public override string Text 
		{
			get { return datePicker.Text; }
			set { datePicker.Text = value; } 
		}

		public DateTime SelectedDate
		{
			get { return (DateTime)datePicker.SelectedDate; }
			set { datePicker.SelectedDate = value; }
		}

		public ValidatedDatePicker(DBObjects.Column column) : base (column)
		{
			datePicker = new CustomizableDatePicker()
			{
				LayoutTransform = new ScaleTransform(scale, scale),
				Width = 200 / scale,
			};
			stpContent.Children.Add(datePicker);
			datePicker.AddNewTextChanged(IsValidDate_TextChanged);
			stpContent.Children.Add(img);
		}

		public override void SetHeight(double newHeight)
		{
			base.SetHeight(newHeight);
			datePicker.Height = newHeight / scale;
		}

		public override void SetWidth(double newWidth)
		{
			base.SetWidth(newWidth);
			datePicker.Width = newWidth / scale;
		}

		public override void AddTextChangedEvent(TextChangedEventHandler ev)
		{
			datePicker.AddNewTextChanged(ev);
		}

		public void AddDateChangedEvent(EventHandler<SelectionChangedEventArgs> handler)
		{
			datePicker.SelectedDateChanged += handler;
		}

		private void IsValidDate_TextChanged(object sender, TextChangedEventArgs e)
		{
			// TODO: This bit should probably be integrated into Validation.Validate
			IsValid = Validation.Validate(Text, Column, out string errorMessage);
			ErrorMessage = errorMessage;
		}
	}

	public class CustomizableDatePicker : DatePicker
	{
		protected DatePickerTextBox _datePickerTextBox;

		public void AddNewTextChanged(TextChangedEventHandler handler)
		{
			if (_datePickerTextBox is null) ApplyTemplate();
			_datePickerTextBox.TextChanged += handler;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_datePickerTextBox = Template.FindName("PART_TextBox", this) as DatePickerTextBox;
			_datePickerTextBox.Background = null;
			_datePickerTextBox.Foreground = new SolidColorBrush(Color.FromRgb(241, 241, 241));
		}
	}
}
