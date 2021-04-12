using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using A2_Project.ContentWindows;

namespace A2_Project
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private double _aspectRatio;
		private bool? _adjustingHeight = null;
		internal enum SWP
		{
			NOMOVE = 0x0002
		}
		internal enum WM
		{
			WINDOWPOSCHANGING = 0x0046,
			EXITSIZEMOVE = 0x0232,
		}

		// Represents the direction the sidebar should go in when next animating. Oscillates between 1 and -1.
		private int menuDirection = 1;
		// Used to ensure any extra threads properly stop when the application is closing
		private bool toExit = false;

		// The name of the currently logged in user
		public string CurrentUser { get; set; }

		private Database db;

		// The content windows used for displaying things inside the main window
		private RegStaff regWindow;
		private CalandarView calWindow;
		private AllTableManger allTableManager;
		private Stats statsWindow;
		private ClientManagement cliWindow;
		private InvoiceManagement invoiceWindow;
		private ShiftManager shiftWindow;
		private DBManager dbManager;
		private readonly Login loginWindow;

		private readonly SolidColorBrush notHighlighted = new SolidColorBrush(Color.FromRgb(161, 161, 161));
		private readonly SolidColorBrush highlighted = new SolidColorBrush(Color.FromRgb(250, 250, 250));
		private readonly SolidColorBrush selected = new SolidColorBrush(Color.FromRgb(33, 150, 243));

		private readonly Grid[] grdButtons;

		public MainWindow()
		{
			InitializeComponent();

			InitialiseDBConnection();

			grdButtons = grdMenuButtons.Children.OfType<Grid>().ToArray();
			// DEBUG: Allows easy access to the content windows. Currently in place to make testing easier
			// TODO: Remove
			grdAllTables.MouseDown += GrdAllTables_MouseDown;
			grdCalander.MouseDown += GrdCalander_MouseDown;
			grdClientManagement.MouseDown += GrdClientManagement_MouseDown;
			grdAddStaff.MouseDown += GrdAddStaff_MouseDown;
			grdViewStats.MouseDown += GrdViewStats_MouseDown;
			grdInvoiceManagement.MouseDown += GrdInvoiceManagement_MouseDown;
			grdShift.MouseDown += GrdShift_MouseDown;
			grdDBManagement.MouseDown += GrdDBManagement_MouseDown;

			foreach (Grid g in grdButtons)
			{
				Rectangle r = new Rectangle
				{
					Fill = notHighlighted
				};
				Panel.SetZIndex(r, -1);
				g.Children.Add(r);

				g.MouseEnter += GrdHighlight_MouseEnter;
				g.MouseLeave += GrdHighlight_MouseLeave;
				g.MouseDown += GrdHighlight_MouseDown;
			}

			// Tries to get the user to log in
			loginWindow = new Login();
			lblContents.Content = loginWindow.Content;

			this.SourceInitialized += Window_SourceInitialized;
		}

		#region Resizing
		[StructLayout(LayoutKind.Sequential)]
		internal struct WINDOWPOS
		{
			public IntPtr hwnd;
			public IntPtr hwndInsertAfter;
			public int x;
			public int y;
			public int cx;
			public int cy;
			public int flags;
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetCursorPos(ref Win32Point pt);

		[StructLayout(LayoutKind.Sequential)]
		internal struct Win32Point
		{
			public Int32 X;
			public Int32 Y;
		};

		public static Point GetMousePosition() // mouse position relative to screen
		{
			Win32Point w32Mouse = new Win32Point();
			GetCursorPos(ref w32Mouse);
			return new Point(w32Mouse.X, w32Mouse.Y);
		}

		private void Window_SourceInitialized(object sender, EventArgs ea)
		{
			HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
			hwndSource.AddHook(DragHook);

			_aspectRatio = this.Width / this.Height;
		}

		private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch ((WM)msg)
			{
				case WM.WINDOWPOSCHANGING:
					{
						WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

						if ((pos.flags & (int)SWP.NOMOVE) != 0)
							return IntPtr.Zero;

						Window wnd = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
						if (wnd == null)
							return IntPtr.Zero;

						// determine what dimension is changed by detecting the mouse position relative to the 
						// window bounds. if gripped in the corner, either will work.
						if (!_adjustingHeight.HasValue)
						{
							Point p = GetMousePosition();

							double diffWidth = Math.Min(Math.Abs(p.X - pos.x), Math.Abs(p.X - pos.x - pos.cx));
							double diffHeight = Math.Min(Math.Abs(p.Y - pos.y), Math.Abs(p.Y - pos.y - pos.cy));

							_adjustingHeight = diffHeight > diffWidth;
						}

						if (_adjustingHeight.Value)
							pos.cy = (int)(pos.cx / _aspectRatio); // adjusting height to width change
						else
							pos.cx = (int)(pos.cy * _aspectRatio); // adjusting width to heigth change

						Marshal.StructureToPtr(pos, lParam, true);
						handled = true;
					}
					break;
				case WM.EXITSIZEMOVE:
					_adjustingHeight = null; // reset adjustment dimension and detect again next time window is resized
					break;
			}

			return IntPtr.Zero;
		}
		#endregion Resizing

		private void InitialiseDBConnection()
		{
			db = new Database();

			if (db.Connect())
			{
				DBMethods.DBAccess.Db = db;
			}
			// Lets the user know if there is an error connecting to the database
			else
			{
				MessageBox.Show("Database Connection Unsuccessful.", "Error");
				Application.Current.Shutdown();
			}

			DBObjects.DB.Initialise();
		}

		private void GrdHighlight_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Grid grdSender = (Grid)sender;
			if (grdSender == grdToggleMenu) return;
			foreach (Grid g in grdButtons) g.Children.OfType<Rectangle>().First().Fill = notHighlighted;
			grdSender.Children.OfType<Rectangle>().First().Fill = selected;
		}

		private void GrdHighlight_MouseLeave(object sender, MouseEventArgs e)
		{
			Grid g = (Grid)sender;
			Rectangle r = g.Children.OfType<Rectangle>().First();
			if (((SolidColorBrush)r.Fill).Color == highlighted.Color) r.Fill = notHighlighted;
		}

		private void GrdHighlight_MouseEnter(object sender, MouseEventArgs e)
		{
			Grid g = (Grid)sender;
			Rectangle r = g.Children.OfType<Rectangle>().First();
			if (((SolidColorBrush)r.Fill).Color == notHighlighted.Color) r.Fill = highlighted;
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
			double a = 250; // The amplitude of the movement
			double tPassed = 0; // The time passed since the start of the animation
			double prevT = 0; // The time passed the previous time the loop completed
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (!toExit && (tPassed = stopwatch.Elapsed.TotalSeconds) <= tMax)
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
		public void LoggedIn()
		{
			lblContents.Content = null;
			grdAllTables.MouseDown += GrdAllTables_MouseDown;
			grdCalander.MouseDown += GrdCalander_MouseDown;
			grdClientManagement.MouseDown += GrdClientManagement_MouseDown;
			grdAddStaff.MouseDown += GrdAddStaff_MouseDown;
			grdViewStats.MouseDown += GrdViewStats_MouseDown;
			grdInvoiceManagement.MouseDown += GrdInvoiceManagement_MouseDown;
			grdShift.MouseDown += GrdShift_MouseDown;
			grdDBManagement.MouseDown += GrdDBManagement_MouseDown;

			GrdCalander_MouseDown(null, null);
		}

		#region Events
		/// <summary>
		/// Closes any sub-windows to allow the application to fully close.
		/// </summary>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (regWindow != null) regWindow.Close();
			if (calWindow != null) calWindow.Close();
			if (loginWindow != null) loginWindow.Close();
			if (allTableManager != null) allTableManager.Close();
			if (statsWindow != null) statsWindow.Close();
			toExit = true;
			db.Close();
			Application.Current.Shutdown();
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			// This allows the login window to report back to the main window when the user has logged in
			loginWindow.Owner = this;
		}

		#region MouseDown Events
		private void GrdToggleMenu_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Thread thread = new Thread(MenuTransition)
			{ IsBackground = true };
			thread.Start();
		}

		private void GrdCalander_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (calWindow is null)
				calWindow = new CalandarView() { Owner = this };
			lblContents.Content = calWindow.Content;
			Title = "Calendar Window";
		}

		private void GrdAllTables_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (allTableManager is null)
				allTableManager = new AllTableManger() { Owner = this };
			lblContents.Content = allTableManager.Content;
			Title = "Manage All Tables Window";
		}

		private void GrdClientManagement_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (cliWindow is null)
				cliWindow = new ClientManagement() { Owner = this };
			lblContents.Content = cliWindow.Content;
			Title = "Client Management Window";
		}

		private void GrdAddStaff_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (regWindow is null)
				regWindow = new RegStaff() { Owner = this };
			lblContents.Content = regWindow.Content;
			Title = "Staff Registration Window";
		}

		private void GrdViewStats_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (statsWindow is null)
				statsWindow = new Stats() { Owner = this };
			lblContents.Content = statsWindow.Content;
			Title = "Statistics Window";
		}

		private void GrdInvoiceManagement_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (invoiceWindow is null)
				invoiceWindow = new InvoiceManagement() { Owner = this };
			lblContents.Content = invoiceWindow.Content;
			Title = "Invoice Management Window";
		}

		private void GrdShift_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (shiftWindow is null)
				shiftWindow = new ShiftManager() { Owner = this };
			lblContents.Content = shiftWindow.Content;
			Title = "Shift Management Window";
		}

		private void GrdDBManagement_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (dbManager is null)
				dbManager = new DBManager() { Owner = this };
			lblContents.Content = dbManager.Content;
			Title = "Database Management Window";
		}
		#endregion MouseDown Events
		#endregion Events
	}
}
