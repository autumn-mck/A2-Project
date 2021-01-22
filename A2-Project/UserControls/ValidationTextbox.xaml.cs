using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace A2_Project.UserControls
{
	/// <summary>
	/// Interaction logic for ValidationTextbox.xaml
	/// </summary>
	public partial class ValidationTextbox : UserControl
	{
		private DBObjects.Column col;

		public string ErrorMessage { get; set; }

		public string Text
		{
			get
			{
				if (col.Constraints.Type == "date") return DateTime.Parse(tbx.Text).ToString("yyyy-MM-dd");
				else return tbx.Text;
			}
			set
			{
				tbx.Text = value;
				Validate();
			}
		}

		private bool isValid = false;
		public bool IsValid
		{
			get { return isValid; }
			set
			{
				if (value) img.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/circle-valid.png"));
				else img.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/Resources/circle-invalid.png"));
				isValid = value;
			}
		}

		public ValidationTextbox(DBObjects.Column _column)
		{
			InitializeComponent();
			col = _column;
			if (col.Constraints.Type == "int") tbx.PreviewTextInput += Tbx_PreventInvalidInt;
		}

		private void Tbx_PreventInvalidInt(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			if (!int.TryParse(e.Text, out _)) e.Handled = true;
		}

		private void Tbx_TextChanged(object sender, TextChangedEventArgs e)
		{
			Validate();
		}

		public void SetWidth(double newWidth)
		{
			Width = newWidth;
			tbx.Width = newWidth - img.Width - img.Margin.Left;
		}

		public void SetHeight(double newHeight)
		{
			Height = newHeight;
			tbx.Height = newHeight;
		}

		public void Validate()
		{
			bool patternReq = true;
			bool typeReq = true;
			bool fKeyReq = true;
			bool pKeyReq = true;

			string str = tbx.Text;

			string patternError = "";
			// Check if the string meets a specific pattern
			if (col.Name.Contains("Email"))
			{
				patternReq = PatternValidation.IsValidEmail(str);
				patternError = "Please enter a valid email address.";
			}
			else if (col.Name.Contains("Postcode"))
			{
				patternReq = PatternValidation.IsValidPostcode(str);
				patternError = "Please enter a valid postcode. ";
			}
			else if (col.Name.Contains("PhoneNo"))
			{
				patternReq = PatternValidation.IsValidPhoneNo(str);
				patternError = "Please enter a valid phone number. ";
			}
			else if (col.Name.Contains("DogGender"))
			{
				patternReq = PatternValidation.IsValidDogGender(str);
				patternError = "Please enter a valid dog gender. (M/F)";
			}

			if (col.Constraints.CanBeNull && str == "")
			{
				IsValid = true;
				ErrorMessage = "";
				return;
			}
			else if (!col.Constraints.CanBeNull && str == "")
			{
				patternReq = false;
				patternError = "This value cannot be left empty! ";
			}

			// Checks if the data meets the requirements for the type of data it should be
			switch (col.Constraints.Type)
			{
				case "int":
					typeReq = !string.IsNullOrEmpty(str) && int.TryParse(str, out _);
					break;
				case "time":
					typeReq = TimeSpan.TryParse(str, out TimeSpan t);
					typeReq = typeReq && t.TotalMinutes % 1.0 == 0;
					break;
				case "varchar":
					typeReq = Text.Length <= col.Constraints.MaxSize;
					break;
			}

			// Checks if the data meets foreign key requirements if needed
			if (col.Constraints.ForeignKey != null && typeReq)
				fKeyReq = DBMethods.MiscRequests.DoesMeetForeignKeyReq(col.Constraints.ForeignKey, str);

			// Checks if the data meets primary key requirements if needed
			if (col.Constraints.IsPrimaryKey && typeReq)
				pKeyReq = DBMethods.MiscRequests.IsPKeyFree(col.TableName, col.Name, str);
			// Note: No good way to validate names/addresses

			IsValid = patternReq && typeReq && fKeyReq && pKeyReq;

			// If the current part is invalid, let the user know what the issue is.
			string instErr = "";
			if (!IsValid)
			{
				instErr = $"\n{col.Name}: ";
				if (!patternReq) instErr += patternError;

				if (!typeReq)
				{
					switch (col.Constraints.Type)
					{
						case "int": instErr += "Please enter a number!"; break;
						case "bit": instErr += "Please enter True/False/1/0!"; break;
						case "date": instErr += "Please enter a valid date!"; break;
						case "time": instErr += "Please enter a valid time! (hh:mm)"; break;
						case "varchar": instErr += $"Input too long, it must be less than {col.Constraints.MaxSize} characters!"; break;
					}
				}

				if (!fKeyReq) instErr += $"References a non-existent {col.Constraints.ForeignKey.ReferencedTable}.";
				if (!pKeyReq) instErr += "This ID is already taken!";
			}
			ErrorMessage = instErr;
		}

		public void AddChangedEvent(TextChangedEventHandler ev)
		{
			tbx.TextChanged += ev;
		}
	}
}
