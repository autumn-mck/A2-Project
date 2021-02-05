using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace A2_Project.UserControls
{
	/// <summary>
	/// Interaction logic for ValidatedItem.xaml
	/// </summary>
	public abstract partial class ValidatedItem : UserControl
	{
		private static BitmapImage invalidImage = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/circle-invalid.png"));
		private static BitmapImage validImage = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/circle-valid.png"));

		protected DBObjects.Column col;

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
			col = column;
			img = new Image()
			{
				Height = 40,
				Width = 40,
				Margin = new System.Windows.Thickness(10, 0, 0, 0)
			};
			IsValid = false;
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
	}
}
