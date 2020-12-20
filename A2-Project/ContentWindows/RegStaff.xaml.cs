using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for RegStaff.xaml
	/// </summary>
	public partial class RegStaff : Window
	{
		private static readonly SolidColorBrush isValidBrush = new SolidColorBrush(Color.FromRgb(241, 241, 241));
		private static readonly SolidColorBrush isInvalidBrush = new SolidColorBrush(Color.FromRgb(182, 24, 39));

		public RegStaff()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Checks if the passwords are valid while registering a new account, and returns all issues found.
		/// </summary>
		private static string IsValidPass(string pswOne, string pswTwo)
		{
			string issues = "";
			if (pswOne == "") issues += "You must enter a password. ";
			if (pswOne == "") issues += "You must re-enter your password. ";
			if (issues.Length > 0) return issues;
			if (pswOne != pswTwo) issues += "Your have incorrectly re-entered your password. ";
			int countCaps = 0, countNums = 0, countSymb = 0;
			foreach (char c in pswOne)
			{
				if (Char.IsUpper(c)) countCaps++;
				else if (Char.IsNumber(c)) countNums++;
				else if (Char.IsPunctuation(c) || Char.IsSymbol(c)) countSymb++;
			}
			if (countCaps < 1) issues += "Your password must contain at least 1 capital letter. ";
			if (countNums < 1) issues += "Your password must contain at least 1 number. ";
			if (countSymb < 1) issues += "Your password must contain at least 1 symbol. ";
			if (pswOne.Length < 7) issues += "Your password must be at least 8 characters long.";
			return issues;
		}

		private static bool IsValidEmail(string email)
		{
			string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
			Regex regex = new Regex(pattern);
			return regex.IsMatch(email);
		}

		private static bool IsPhoneNoValid(string phoneNo)
		{
			string regexStr = @"^(?:(?:\(?(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?(?:\(?0\)?[\s-]?)?)|(?:\(?0))(?:(?:\d{5}\)?[\s-]?\d{4,5})|(?:\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3}))|(?:\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4})|(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}))(?:[\s-]?(?:x|ext\.?|\#)\d{3,4})?$";
			Regex regex = new Regex(regexStr);
			return regex.IsMatch(phoneNo);
		}

		private string IsInputValid()
		{
			if (txtName.Text != "")
			{
				try
				{
					string ispassValid = IsValidPass(pswPassword.Password, pswRePassword.Password);
					if (ispassValid == "")
					{
						if (txtEmail.Text == "" || IsValidEmail(txtEmail.Text))
						{
							if (txtPhoneNo.Text == "" || IsPhoneNoValid(txtPhoneNo.Text))
							{
								if (IsNameTaken(txtName.Text))
								{
									return "";
								}
								else return "Name already taken!";
							}
							else return "Invalid phone number!";
						}
						else return "Invalid email address!";
					}
					else
					{
						return ispassValid;
					}
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			}
			else return "Please enter a name!";
		}

		private void BtnRegister_Click(object sender, RoutedEventArgs e)
		{
			string str = IsInputValid();
			if (str == "")
			{
				tblOutput.Text = "Account created!";
				tblOutput.Foreground = isValidBrush;
				txtName.Text = "";
				txtEmail.Text = "";
				txtPhoneNo.Text = "";
				pswPassword.Password = "";
				pswRePassword.Password = "";
				DBMethods.LogRegRequests.CreateStaffAccount(txtName.Text, pswPassword.Password, txtEmail.Text, txtPhoneNo.Text, (bool)cbx2FA.IsChecked);
			}
			else
			{
				tblOutput.Foreground = isInvalidBrush;
				tblOutput.Text = str;
			}
		}

		private bool IsNameTaken(string name)
		{
			return DBMethods.LogRegRequests.IsNameTaken(name);
		}

		private void UpdateOutput()
		{
			tblOutput.Foreground = isInvalidBrush;
			tblOutput.Text = IsInputValid();
		}

		private void TxtBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			UpdateOutput();
		}

		private void PswBox_PasswordChanged(object sender, RoutedEventArgs e)
		{
			UpdateOutput();
		}
	}
}
