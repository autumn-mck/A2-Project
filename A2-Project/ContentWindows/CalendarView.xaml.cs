using System;
using System.Collections.Generic;
using System.Linq;
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
		private List<List<string>> appTypes;

		private CustomDatePicker datePicker;

		private string tableName = "Appointment";

		private DBObjects.Column[] columns;

		private DataEditingSidebar editingSidebar;

		public CalandarView()
		{
			colours[0] = Color.FromRgb(183, 28, 28);
			colours[1] = Color.FromRgb(62, 39, 35);
			colours[2] = Color.FromRgb(27, 94, 32);
			colours[3] = Color.FromRgb(13, 71, 161);
			colours[4] = Color.FromRgb(49, 27, 146);
			appTypes = DBMethods.MetaRequests.GetAllFromTable("Appointment Type");

			columns = DBMethods.MetaRequests.GetColumnDataFromTable(tableName);

			InitializeComponent();
			Thread thread = new Thread(Loop);
			thread.Start();
			datePicker = new CustomDatePicker()
			{
				Margin = new Thickness(10, 100, 0, 0),
				Width = 200 / 1.5,
				FontSize = 16,
				RenderTransform = new ScaleTransform(1.5, 1.5),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top
			};
			grd.Children.Add(datePicker);
			//c.AddNewTextChanged(CustomDatePicker_TextChanged);
			datePicker.SelectedDateChanged += DatePicker_SelectedDateChanged;
			datePicker.SelectedDate = DateTime.Today;

			editingSidebar = new DataEditingSidebar(columns, tableName, this);
			lblSidebar.Content = editingSidebar.Content;
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

			if (currentlySelected is Rectangle rct)
				rct.Stroke = Brushes.Black;

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
						Stroke = Brushes.AliceBlue,
						StrokeThickness = rect.StrokeThickness,
						Tag = rect.Tag,
						VerticalAlignment = rect.VerticalAlignment,
						HorizontalAlignment = rect.HorizontalAlignment
					};

					int appID = Convert.ToInt32(rect.Tag);
					editingSidebar.ChangeSelectedData(DBMethods.MiscRequests.GetByColumnData(tableName, "Appointment ID", rect.Tag.ToString(), columns.Select(columns => columns.Name).ToArray())[0].ToArray());

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

				string idColumnName = "Appointment ID";

				DateTime startOfWeek = GetStartOfWeek();
				DateTime appDate = startOfWeek.AddDays(f.Margin.Left / 120);
				DBMethods.MiscRequests.UpdateColumn(tableName, appDate.ToString("yyyy-MM-dd"), "Appointment Date", idColumnName, f.Tag.ToString());

				TimeSpan t = new TimeSpan(7, (int)(f.Margin.Top * 1.5), 0);
				DBMethods.MiscRequests.UpdateColumn(tableName, t.ToString("hh\\:mm"), "Appointment Time", idColumnName, f.Tag.ToString());

				int roomID = (int)(f.Margin.Left % 120 / 40);
				DBMethods.MiscRequests.UpdateColumn(tableName, roomID.ToString(), "Grooming Room ID", idColumnName, f.Tag.ToString());

				editingSidebar.ChangeSelectedData(DBMethods.MiscRequests.GetByColumnData(tableName, idColumnName, f.Tag.ToString(), columns.Select(x => x.Name).ToArray())[0].ToArray());
			}
		}

		/// <summary>
		/// Ensures that the looping method exits whenever the application closes.
		/// </summary>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			toExit = true;
		}

		private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			CustomDatePicker c = (CustomDatePicker)sender;

			grdResults.Children.Clear();
			int days = 7;
			for (int i = 0; i < days; i++)
			{
				Rectangle rct = new Rectangle
				{
					Width = 1,
					Height = 600,
					Margin = new Thickness(i * 120 - 0.5, 0, 0, 0),
					Fill = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
					StrokeThickness = 0,
					VerticalAlignment = VerticalAlignment.Top,
					HorizontalAlignment = HorizontalAlignment.Left
				};
				grdResults.Children.Add(rct);

				DateTime startOfWeek = GetStartOfWeek();
				DateTime currentDay = startOfWeek.AddDays(i);

				Label lblDayOfWeek = new Label()
				{
					Content = currentDay.DayOfWeek,
					Margin = new Thickness(i * 120, 0, 0, 0),
					Foreground = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
					FontSize = 20
				};
				grdResults.Children.Add(lblDayOfWeek);


				List<List<string>> results = DBMethods.MiscRequests.GetAllAppointmentsOnDay(currentDay);
				foreach (List<string> ls in results)
				{
					GenRectFromData(ls.ToArray());
				}
			}
		}

		public DateTime GetStartOfWeek()
		{
			DateTime picked = (DateTime)datePicker.SelectedDate;
			return picked.AddDays(-DayOfWeekToInt(picked.DayOfWeek));
		}

		public void UpdateFromSidebar(string[] data, bool isNew)
		{
			Rectangle r = grdResults.Children.OfType<Rectangle>().Where(r => r.Tag != null && r.Tag.ToString() == data[0]).First();
			grdResults.Children.Remove(r);
			GenRectFromData(data);
			DBMethods.DBAccess.UpdateTable(tableName, columns.Select(x => x.Name).ToArray(), data, isNew);
		}

		public void GenRectFromData(string[] data)
		{
			int roomID = Convert.ToInt32(data[14]);
			int typeID = Convert.ToInt32(data[2]);
			DateTime d = DateTime.Parse(data[9]).Add(TimeSpan.Parse(data[10]));
			int dDiff = (d.Date - (DateTime)datePicker.SelectedDate).Days;
			dDiff += DayOfWeekToInt(((DateTime)datePicker.SelectedDate).DayOfWeek);
			SolidColorBrush brush = new SolidColorBrush(colours[Convert.ToInt32(data[3])]);
			
			Rectangle newRect = new Rectangle
			{
				Width = 40,
				Height = 40 * Convert.ToDouble(appTypes[typeID][1]),
				Margin = new Thickness(dDiff * 120 + roomID * 40, (d.TimeOfDay.TotalHours - 7) * 40, 0, 0),
				Fill = brush,
				Stroke = Brushes.Black,
				StrokeThickness = 1,
				Tag = data[0],
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left
			};
			newRect.MouseDown += Rectangle_MouseDown;
			newRect.MouseUp += RctRect_MouseUp;
			grdResults.Children.Add(newRect);
		}

		private static int DayOfWeekToInt(DayOfWeek day)
		{
			return day switch
			{
				DayOfWeek.Monday => 0,
				DayOfWeek.Tuesday => 1,
				DayOfWeek.Wednesday => 2,
				DayOfWeek.Thursday => 3,
				DayOfWeek.Friday => 4,
				DayOfWeek.Saturday => 5,
				DayOfWeek.Sunday => 6,
				_ => throw new Exception(),
			};
		}
	}
}
