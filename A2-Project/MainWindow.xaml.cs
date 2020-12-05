using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace A2_Project
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private int menuDirection = 1;
		private bool toExit = false;

		public string CurrentUser { get; set; }

		private readonly Database db;
		private readonly DBAccess dBAccess;

		private RegisterNewStaffWindow regWindow;
		private CalanderTest calWindow;
		private InvoiceTesting invoiceTesting;
		private ClientManagement clientManagement;
		private readonly LoginWindow login;

		public MainWindow()
		{
			InitializeComponent();

			db = new Database();

			// Lets the user know if there is an error connecting to the database
			if (db.Connect()) dBAccess = new DBAccess(db);
			else MessageBox.Show("Database Connection Unsuccessful.", "Error");
			grdAccounts.MouseDown += GrdAccounts_MouseDown;
			grdCalander.MouseDown += GrdCalander_MouseDown;
			grdInvoices.MouseDown += GrdInvoices_MouseDown;
			grdAddStaff.MouseDown += GrdAddStaff_MouseDown;

			login = new LoginWindow(dBAccess);
			lblContents.Content = login.Content;
		}

		/// <summary>
		/// Widens the menu bar to reveal the hidden text
		/// </summary>
		private void MenuTransition()
		{
			int lclMenuDir = menuDirection;
			menuDirection = -menuDirection;
			double tMax = 0.25; // The time taken for the transition in seconds
			double a = 220; // The amplitude of the movement
			double tPassed = 0; // The time passed since the start of the animation
			double prevT = 0; // The time passed the previous time the loop completed
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (!toExit && (tPassed = stopwatch.Elapsed.TotalSeconds) < tMax)
			{
				Dispatcher.Invoke(() => grdMenuButtons.Width += Math.Sin(tPassed / tMax * Math.PI / 2) * lclMenuDir * a - Math.Sin(prevT / tMax * Math.PI / 2) * lclMenuDir * a);
				prevT = tPassed;
				Thread.Sleep(10);
			}
			stopwatch.Stop();
		}

		public void HasLoggedIn()
		{
			lblContents.Content = null;
			grdAccounts.MouseDown += GrdAccounts_MouseDown;
			grdCalander.MouseDown += GrdCalander_MouseDown;
			grdInvoices.MouseDown += GrdInvoices_MouseDown;
			grdAddStaff.MouseDown += GrdAddStaff_MouseDown;
		}

		#region Events
		/// <summary>
		/// Closes any sub-windows to allow the application to fully close.
		/// </summary>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (regWindow != null) regWindow.Close();
			if (calWindow != null) calWindow.Close();
			if (invoiceTesting != null) invoiceTesting.Close();
			if (login != null) login.Close();
			toExit = true;
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			login.Owner = this;
		}

		#region MouseDown Events
		private void GrdToggleMenu_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Thread thread = new Thread(MenuTransition);
			thread.Start();
		}
		// TODO: See if the following methods can be simplified
		private void GrdCalander_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (calWindow == null)
				calWindow = new CalanderTest(dBAccess) { Owner = this };
			lblContents.Content = calWindow.Content;
		}

		private void GrdAccounts_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (clientManagement == null)
				clientManagement = new ClientManagement(dBAccess) { Owner = this };
			lblContents.Content = clientManagement.Content;
		}

		private void GrdInvoices_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (invoiceTesting == null)
				invoiceTesting = new InvoiceTesting(dBAccess) { Owner = this };
			lblContents.Content = invoiceTesting.Content;
		}

		private void GrdAddStaff_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (regWindow == null)
				regWindow = new RegisterNewStaffWindow(dBAccess) { Owner = this };
			lblContents.Content = regWindow.Content;
		}
		#endregion MouseDown Events
		#endregion Events
	}
}
