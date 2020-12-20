using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using A2_Project.ContentWindows;

namespace A2_Project
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		// Represents the direction the sidebar should go in when next animating. Oscillates between 1 and -1.
		private int menuDirection = 1;
		// Used to ensure any extra threads properly stop when the application is closing
		private bool toExit = false;

		// The name of the currently logged in user
		public string CurrentUser { get; set; }

		private readonly Database db;

		// The content windows used for displaying things inside the main window
		private RegStaff regWindow;
		private Calander calWindow;
		private InvoiceManagement invoiceManWindow;
		private ContactManagement contactManWindow;
		private Stats statsWindow;
		private readonly Login loginWindow;

		public MainWindow()
		{
			InitializeComponent();

			db = new Database();

			// Lets the user know if there is an error connecting to the database
			if (db.Connect()) DBMethods.DBAccess.Db = db;
			else MessageBox.Show("Database Connection Unsuccessful.", "Error");

			// DEBUG: Allows easy access to the content windows. Currently in place to make testing easier
			grdAccounts.MouseDown += GrdAccounts_MouseDown;
			grdCalander.MouseDown += GrdCalander_MouseDown;
			grdInvoices.MouseDown += GrdInvoices_MouseDown;
			grdAddStaff.MouseDown += GrdAddStaff_MouseDown;
			grdViewStats.MouseDown += GrdViewStats_MouseDown;

			// Tries to get the user to log in
			loginWindow = new Login();
			lblContents.Content = loginWindow.Content;
		}

		/// <summary>
		/// Widens the menu bar to reveal the hidden text
		/// </summary>
		private void MenuTransition()
		{
			// A local copy of the menuDirection must be kept, as otherwise clicking the expand button repeatedly would cause errors
			int lclMenuDir = menuDirection;
			menuDirection = -menuDirection;
			double tMax = 0.2; // The time taken for the transition in seconds
			double a = 220; // The amplitude of the movement
			double tPassed = 0; // The time passed since the start of the animation
			double prevT = 0; // The time passed the previous time the loop completed
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (!toExit && (tPassed = stopwatch.Elapsed.TotalSeconds) < tMax)
			{
				// Sinusoidal animation
				Dispatcher.Invoke(() => grdMenuButtons.Width += Math.Sin(tPassed / tMax * Math.PI / 2) * lclMenuDir * a - Math.Sin(prevT / tMax * Math.PI / 2) * lclMenuDir * a);
				prevT = tPassed;
				Thread.Sleep(10);
			}
			stopwatch.Stop();
		}

		/// <summary>
		/// Allows the user to access all content windows whenever they have logged in
		/// </summary>
		public void HasLoggedIn()
		{
			lblContents.Content = null;
			grdAccounts.MouseDown += GrdAccounts_MouseDown;
			grdCalander.MouseDown += GrdCalander_MouseDown;
			grdInvoices.MouseDown += GrdInvoices_MouseDown;
			grdAddStaff.MouseDown += GrdAddStaff_MouseDown;
			grdViewStats.MouseDown += GrdViewStats_MouseDown;
		}

		#region Events
		/// <summary>
		/// Closes any sub-windows to allow the application to fully close.
		/// </summary>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (regWindow != null) regWindow.Close();
			if (calWindow != null) calWindow.Close();
			if (invoiceManWindow != null) invoiceManWindow.Close();
			if (loginWindow != null) loginWindow.Close();
			toExit = true;
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			// This allows the login window to report back to the main window when the user has logged in
			loginWindow.Owner = this;
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
				calWindow = new Calander() { Owner = this };
			lblContents.Content = calWindow.Content;
		}

		private void GrdAccounts_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (contactManWindow == null)
				contactManWindow = new ContactManagement() { Owner = this };
			lblContents.Content = contactManWindow.Content;
		}

		private void GrdInvoices_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (invoiceManWindow == null)
				invoiceManWindow = new InvoiceManagement() { Owner = this };
			lblContents.Content = invoiceManWindow.Content;
		}

		private void GrdAddStaff_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (regWindow == null)
				regWindow = new RegStaff() { Owner = this };
			lblContents.Content = regWindow.Content;
		}

		private void GrdViewStats_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (statsWindow == null)
				statsWindow = new Stats() { Owner = this };
			lblContents.Content = statsWindow.Content;
		}
		#endregion MouseDown Events
		#endregion Events
	}
}
