using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace A2_Project.UserControls
{
	/// <summary>
	/// Interaction logic for ValidatedItem.xaml
	/// </summary>
	public abstract partial class ValidatedItem : UserControl
	{
		private static readonly BitmapImage invalidImage = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/circle-invalid.png"));
		private static readonly BitmapImage validImage = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/circle-valid.png"));

		public DBObjects.Column Column { get; set; }

		public string ErrorMessage { get; set; }

		protected Image img;

		private bool isValid = false;
		public bool IsValid
		{
			get { return isValid; }
			set
			{
				if (value) img.Source = validImage;
				else img.Source = invalidImage;
				isValid = value;
			}
		}

		public abstract string Text { get; set; }

		public ValidatedItem(DBObjects.Column column)
		{
			InitializeComponent();
			Column = column;
			img = new Image()
			{
				Height = 40,
				Width = 40,
				Margin = new System.Windows.Thickness(10, 0, 0, 0)
			};

			IsValid = Validation.Validate("", Column, out string errMessage);
			ErrorMessage = errMessage;
		}

		public virtual void SetWidth(double newWidth)
		{
			Width = newWidth;
		}

		public virtual void SetHeight(double newHeight)
		{
			Height = newHeight;
			//img.Width = newHeight;
			//img.Height = newHeight;
		}

		public abstract void AddTextChangedEvent(TextChangedEventHandler ev);

		public void UpdateValidImage()
		{
			img.Visibility = System.Windows.Visibility.Hidden;
			img.Source = validImage;
			img.Source = invalidImage;
			if (IsValid) img.Source = validImage;
			else img.Source = invalidImage;
			img.Visibility = System.Windows.Visibility.Visible;

		}

		public void ToggleImage()
		{
			if (img.Visibility == Visibility.Visible)
				img.Visibility = Visibility.Collapsed;
			else img.Visibility = Visibility.Visible;
		}
	}
}
