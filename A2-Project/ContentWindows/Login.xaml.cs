using System.Windows;
using System.Windows.Media;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for Login.xaml
	/// </summary>
	public partial class Login : Window
	{
		public Login()
		{
			InitializeComponent();
		}

		private int attemptsLeft = 3;
		private string authKey;
		private string email;

		private void TryLogin()
		{
			if (attemptsLeft < 1) return;

			if (txtLogUser.Text == "" || pswLogPassword.Password == "") { }
			if (IsLoginDataCorrect(txtLogUser.Text, pswLogPassword.Password))
			{
				if (!DBMethods.LogRegRequests.DoesUse2FA(txtLogUser.Text, ref email))
				{
					FinishLoggingIn();
				}
				else
				{
					grd2FA.Visibility = Visibility.Visible;
					grdLog.HorizontalAlignment = HorizontalAlignment.Left;
					grdLog.Margin = new Thickness(100, 0, 0, 0);
					lblOutput.Foreground = new SolidColorBrush(Color.FromRgb(241, 241, 241));
					lblOutput.Content = "Name/Password correct!\nPlease enter the authentication key\nwhich has just been emailed to you.";
					pswLogPassword.IsEnabled = false;
					txtLogUser.IsEnabled = false;
					btnLogIn.Content = "Resend code";
					btnLogIn.Click -= BtnLogIn_Click;
					btnLogIn.Click += ResendCode_Click;
					authKey = EmailManagement.Send2FAEmail(email);

				}
			}
			else
			{
				if (attemptsLeft < 2)
				{
					txtLogUser.IsEnabled = false;
					txtLogUser.Text = "";
					pswLogPassword.IsEnabled = false;
					pswLogPassword.Password = "";
					lblOutput.Content = "You have been locked out from logging in.";
					btnLogIn.IsEnabled = false;
					return;
				}

				attemptsLeft--;
				lblOutput.Content = "Incorrect username/password!\nYou have " + attemptsLeft + " attempts left.";
			}
		}

		/// <summary>
		/// Checks if the name and password passed in are both true.
		/// </summary>
		public bool IsLoginDataCorrect(string _name, string _password)
		{
			return DBMethods.LogRegRequests.IsLoginDataCorrect(_name, _password);
		}

		private void BtnLogIn_Click(object sender, RoutedEventArgs e)
		{
			TryLogin();
		}

		private void ResendCode_Click(object sender, RoutedEventArgs e)
		{
			authKey = EmailManagement.Send2FAEmail(email);
		}

		private void BtnConfirmKey_Click(object sender, RoutedEventArgs e)
		{
			if (txtKey.Text == authKey)
				FinishLoggingIn();
			else
			{
				lblOutput.Content = "Incorrect key.\nPlease try again!";
				lblOutput.Foreground = new SolidColorBrush(Color.FromRgb(182, 24, 39));
			}
		}

		private void FinishLoggingIn()
		{
			lblOutput.Content = "Logged in!";
			((MainWindow)Owner).CurrentUser = txtLogUser.Text;
			((MainWindow)Owner).LoggedIn();
		}
	}
}
