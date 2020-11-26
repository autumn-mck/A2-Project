using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace A2_Project
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private int menuDirection = 1;
		private bool toExit = false;

		private Database db;
		private DBAccess dBAccess;

		public MainWindow()
		{
			InitializeComponent();

			db = new Database();

			// Lets the user know if there is an error connecting to the database
			if (db.Connect()) dBAccess = new DBAccess(db);
			else MessageBox.Show("Database Connection Unsuccessful.", "Error");
		}

		ClientManagement clientManagement;
		CalanderTest calWindow;

		/// <summary>
		/// Widens the menu bar to reveal the hidden text
		/// </summary>
		private void MenuTransition()
		{
			int lclMenuDir = menuDirection;
			menuDirection = -menuDirection;
			double tMax = 0.25; // The time taken for the transition in seconds
			double a = 200; // The amplitude of the movement
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

		#region Events
		/// <summary>
		/// Closes any sub-windows to allow the application to fully close.
		/// </summary>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (clientManagement != null) clientManagement.Close();
			if (calWindow != null) calWindow.Close();
			toExit = true;
		}
		#region MouseDown events
		private void GrdToggleMenu_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Thread thread = new Thread(MenuTransition);
			thread.Start();
		}

		private void GrdCalander_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (calWindow == null)
				calWindow = new CalanderTest { Owner = this };
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

		}
		#endregion MouseDown Events
		#endregion Events
	}
}
