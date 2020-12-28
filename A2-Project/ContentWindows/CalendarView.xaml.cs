using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for Calander.xaml
	/// </summary>
	public partial class CalandarView : Window
	{
		private Color[] colours = new Color[5];
		private Point diffMouseAndElem;
		private bool mouseDown = false;
		private object currentlySelected;
		private bool toExit = false;
		private Point diff;

		public CalandarView()
		{
			colours[0] = Color.FromRgb(183, 28, 28);
			colours[1] = Color.FromRgb(62, 39, 35);
			colours[2] = Color.FromRgb(27, 94, 32);
			colours[3] = Color.FromRgb(13, 71, 161);
			colours[4] = Color.FromRgb(49, 27, 146);

			InitializeComponent();
			Thread thread = new Thread(Loop);
			thread.Start();
			calPick.SelectedDate = DateTime.Now;
		}

		/// <summary>
		/// A looping method to move the currently selected rectangle to the mouse.
		/// </summary>
		private void Loop()
		{
			while (!toExit)
			{
				Dispatcher.Invoke(() => {
					if (mouseDown && currentlySelected is FrameworkElement elem && elem.Parent is FrameworkElement parent)
					{
						diff = (Point)(Mouse.GetPosition(parent) - diffMouseAndElem);
						elem.Margin = new Thickness(diff.X, diff.Y, 0, 0);
					}
				});
				Thread.Sleep(10);
			}
		}

		/// <summary>
		/// Creates a duplicate of the rectangle clicked on to be moved around.
		/// </summary>
		private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
		{
			mouseDown = true;
			currentlySelected = sender;
			if (currentlySelected is FrameworkElement element && element.Parent is FrameworkElement parent)
			{
				diffMouseAndElem = (Point)(Mouse.GetPosition(parent) - new Point(element.Margin.Left, element.Margin.Top));
				if (element is Rectangle rect)
				{
					Rectangle newRect = new Rectangle
					{
						Width = rect.Width,
						Height = rect.Height,
						Margin = rect.Margin,
						Fill = rect.Fill,
						Stroke = rect.Stroke,
						StrokeThickness = rect.StrokeThickness,
						VerticalAlignment = rect.VerticalAlignment,
						HorizontalAlignment = rect.HorizontalAlignment
					};
					newRect.MouseDown += Rectangle_MouseDown;
					newRect.MouseUp += RctRect_MouseUp;
					currentlySelected = newRect;
					((Grid)parent).Children.Add(newRect);
				}

				if (element.Tag == null || element.Tag.ToString() != "Duplicate")
				{
					((Grid)parent).Children.Remove(element);
				}
			}
		}

		/// <summary>
		/// Tries to place the rectangle in an appropriate grid spot one the mouse is lifted.
		/// Note: Is not currently called if the mouse is released while not over the moving rectangle.
		/// </summary>
		private void RctRect_MouseUp(object sender, MouseButtonEventArgs e)
		{
			mouseDown = false;
			if (sender != currentlySelected) return;
			if (sender is FrameworkElement f)
			{
				int ySnap = 20;
				double midLeft = f.Margin.Left + f.Width / 2;
				double midTop = f.Margin.Top + ySnap / 2;
				f.Margin = new Thickness(midLeft - midLeft % f.Width, midTop - midTop % ySnap, 0, 0);
			}
		}

		/// <summary>
		/// Ensures that the looping method exits whenever the application closes.
		/// </summary>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			toExit = true;
		}

		private void CalPick_DisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
		{
			List<List<string>> appTypes = DBMethods.MetaRequests.GetAllFromTable("Appointment Type");
			Calendar c = (Calendar)sender;
			if (c.DisplayMode == CalendarMode.Month) c.DisplayMode = CalendarMode.Year;

			grdResults.Children.Clear();
			DateTime picked = c.DisplayDate;
			int days = DateTime.DaysInMonth(picked.Year, picked.Month);
			for (int i = 1; i < days; i++)
			{
				DateTime startOfMonth = picked.AddDays(-picked.Day + i);
				List<List<string>> results = DBMethods.MiscRequests.GetAllAppointmentsOnDay(startOfMonth);
				int count = 0;
				foreach (List<string> ls in results)
				{
					int typeID = Convert.ToInt32(ls[2]);
					DateTime d = DateTime.Parse(ls[9]).Add(TimeSpan.Parse(ls[10]));
					count++;
					Rectangle newRect = new Rectangle
					{
						Width = 40,
						Height = 40 * Convert.ToDouble(appTypes[typeID][1]),
						Margin = new Thickness(i * 40, (d.TimeOfDay.TotalHours - 7) * 40, 0, 0),
						Fill = new SolidColorBrush(colours[Convert.ToInt32(ls[3])]),
						Stroke = Brushes.Black,
						StrokeThickness = 2,
						VerticalAlignment = VerticalAlignment.Top,
						HorizontalAlignment = HorizontalAlignment.Left
					};
					newRect.MouseDown += Rectangle_MouseDown;
					newRect.MouseUp += RctRect_MouseUp;
					grdResults.Children.Add(newRect);
				}
			}
		}
	}
}
