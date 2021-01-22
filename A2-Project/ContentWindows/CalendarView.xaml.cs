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
		// The hour of the day work starts at (e.g. 9 being 9:00)
		private const double dayStartTime = 8;
		private const double dayEndTime = 19; 
		// How much extra space there should be between days (e.g. 1.3 being 30% the width of 1 day)
		private const double spaceBetweenDays = 1.3;
		// The number of rooms available (Assuming room IDs start at 0 and count up by 1)
		private int appRoomCount = 3;
		// The width 1 day should have
		private const int dayWidth = 120;
		// The height 1 hour should have
		private const int hourHeight = 80;
		// TODO: Does not properly work
		private const int baseYOffset = 30;

		// The colours used for colouring appointments based on the applied filter
		private readonly Color[] colours;

		// The difference between the position of the mouse and the currently selected element
		private Point diffMouseAndElem;
		// Is the mouse currently held down?
		private bool mouseDown = false;
		// The currently selected element to be moved with the mouse
		private FrameworkElement currentlySelected;

		private bool toExit = false;

		// The date picker used for selecting a week to display
		private CustomDatePicker datePicker;
		// The name of the appointment table
		private readonly string tableName = "Appointment";
		// The columns of the appointment table
		private DBObjects.Column[] columns;

		// The sidebar used for editing the selected appointment
		private DataEditingSidebar editingSidebar;

		// The text of the appointment key labels
		private string[] keyLabels;
		// The index of the key to next be added
		private int keyLoadedCount = 0;
		// The ComboBox used for selecting which colour filter will be used
		private ComboBox cmbKey;

		// Used to store if the user has just moved to the previous/next week
		private bool hasMoved = false;

		private List<FrameworkElement> labelElements = new List<FrameworkElement>();

		public CalandarView()
		{
			// Get the number of rooms
			appRoomCount = Convert.ToInt32(DBMethods.MiscRequests.GetMinKeyNotUsed("Grooming Room", "Grooming Room ID"));
			// For now, filtering by staff is default
			// TODO: This requests more data than is needed, just to discard it
			keyLabels = DBMethods.MetaRequests.GetAllFromTable("Staff").Select(x => x[1]).ToArray();
			// Get the column data for the appointment table
			columns = DBMethods.MetaRequests.GetColumnDataFromTable(tableName);

			// Get the colours for filters
			colours = new Color[]
			{
				Color.FromRgb(183, 28, 28), // Red
				Color.FromRgb(13, 71, 161), // Blue
				Color.FromRgb(190, 96, 0), // Dark orange? Not quite brown
				Color.FromRgb(27, 94, 32), // Green
				Color.FromRgb(49, 27, 146) // Deep Purple
			};

			InitializeComponent();

			// Start the thread for moving the selected element to the mouse when needed
			Thread loopThread = new Thread(Loop);
			loopThread.IsBackground = true;
			loopThread.Start();

			// Create a DatePicker used for selecting a date to display
			datePicker = new CustomDatePicker()
			{
				Margin = new Thickness(544, 10, 0, 0),
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

			Label lblPrevWeek = new Label()
			{
				Content = "<",
				FontSize = 30,
				FontWeight = FontWeights.Bold,
				Margin = new Thickness(500, 0, 0, 0)
			};
			lblPrevWeek.MouseDown += LblPrevWeek_MouseDown;
			grd.Children.Add(lblPrevWeek);
			Label lblNextWeek = new Label()
			{
				Content = ">",
				FontSize = 30,
				FontWeight = FontWeights.Bold,
				Margin = new Thickness(760, 0, 0, 0)
			};
			lblNextWeek.MouseDown += LblNextWeek_MouseDown;
			grd.Children.Add(lblNextWeek);

			// Instantiate the editing sidebar
			editingSidebar = new DataEditingSidebar(columns, tableName, this);
			lblSidebar.Content = editingSidebar.Content;

			// Start the process of adding the key for the appointment view
			AddKey();
		}

		private void LblNextWeek_MouseDown(object sender, MouseButtonEventArgs e)
		{
			datePicker.SelectedDate = ((DateTime)datePicker.SelectedDate).AddDays(7);
		}

		private void LblPrevWeek_MouseDown(object sender, MouseButtonEventArgs e)
		{
			datePicker.SelectedDate = ((DateTime)datePicker.SelectedDate).AddDays(-7);
		}

		private void AddKey(FrameworkElement frPrev = null)
		{
			// The base X offset
			int baseX = 0;
			// The base Y offset
			int baseY = 5;
			int gapBetweenKeys = 25;

			// The first element to be added is the checkbox that allows other filters to be selected
			if (keyLoadedCount == 0)
			{
				// Get the selected index of the previous ComboBox, if it exists
				int selIndex;
				if (cmbKey != null) selIndex = cmbKey.SelectedIndex;
				else selIndex = 0;

				Label lblColourBy = new Label()
				{
					Margin = new Thickness(baseX, baseY - 12, 0, 0),
					Content = "Colour By:"
				};
				grdKey.Children.Add(lblColourBy);

				// Creates the new ComboBox
				cmbKey = new ComboBox()
				{
					Margin = new Thickness(baseX + 130, baseY - 5, 0, 0),
					Width = 240,
					FontSize = 20,
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top,
					ItemsSource = new string[] { "Staff Member", "Is Paid", "Appointment Type", "Includes Nail And Teeth" },
					SelectedIndex = selIndex
				};
				cmbKey.SelectionChanged += CmbKeyOptions_SelectionChanged;
				// Allow the next key to be added
				cmbKey.Loaded += LblLoaded_LoadNextKey;

				grdKey.Children.Add(cmbKey);
				keyLoadedCount++;
				return;
			}

			Rectangle r = new Rectangle
			{
				Width = 20,
				Height = 20,
				Margin = new Thickness(baseX + (keyLoadedCount - 1) * 120, baseY, 0, 0),
				Fill = new SolidColorBrush(colours[keyLoadedCount - 1]),
				Stroke = Brushes.Black,
				StrokeThickness = 1,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left
			};
			grdKey.Children.Add(r);

			Label l = new Label()
			{
				Content = keyLabels[keyLoadedCount - 1],
				Margin = new Thickness(baseX + r.Width + (keyLoadedCount - 1) * 120, baseY - 12, 0, 0)
			};

			// Allow the current key to be placed relative to the previous one
			if (!(frPrev is null))
			{
				r.Margin = new Thickness(frPrev.Margin.Left + frPrev.ActualWidth + gapBetweenKeys, r.Margin.Top, 0, 0);
				l.Margin = new Thickness(frPrev.Margin.Left + frPrev.ActualWidth + gapBetweenKeys + r.Width, l.Margin.Top, 0, 0);
			}

			if (keyLoadedCount == keyLabels.Length)
			{
				// If this is the final element to be created, reset the keyCount so it can be used in the future, and exit.
				keyLoadedCount = 0;
				grdKey.Children.Add(l);
			}
			else
			{
				// Otherwise, when the next label is loaded, this method must be called again
				l.Loaded += LblLoaded_LoadNextKey;
				grdKey.Children.Add(l);
				keyLoadedCount++;
			}
		}

		/// <summary>
		/// Once this label has been loaded, try to add the next key.
		/// </summary>
		private void LblLoaded_LoadNextKey(object sender, RoutedEventArgs e)
		{
			FrameworkElement fr = (FrameworkElement)sender;
			fr.Loaded -= LblLoaded_LoadNextKey;
			AddKey(fr);
		}

		/// <summary>
		/// When the selected filter is changed, update the displayed keys and the colours of the appointment rectangles
		/// </summary>
		private void CmbKeyOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox cmb = (ComboBox)sender;
			switch (cmb.SelectedItem.ToString())
			{
				case "Staff Member":
					keyLabels = DBMethods.MetaRequests.GetAllFromTable("Staff").Select(x => x[1]).ToArray();
					break;
				case "Is Paid":
					keyLabels = new string[] { "Not Paid For", "Paid For" };
					break;
				case "Appointment Type":
					keyLabels = DBMethods.MetaRequests.GetAllFromTable("Appointment Type").Select(x => x[3]).ToArray();
					break;
				case "Includes Nail And Teeth":
					keyLabels = new string[] { "Does Not Include Nails And Teeth", "Includes Nails And Teeth" };
					break;
			}
			grdKey.Children.Clear();
			AddKey();

			IEnumerable<Rectangle> rects = grdResults.Children.OfType<Rectangle>();
			foreach (Rectangle r in rects)
			{
				string[] tag = (string[])r.Tag;
				if (tag is null) continue;
				r.Fill = GetColourForRect(tag);
			}
		}

		/// <summary>
		/// Get the colour a rectangle should have based on its data and the current filter
		/// </summary>
		private SolidColorBrush GetColourForRect(string[] rectData)
		{
			string cmbCase;
			if (cmbKey is null) cmbCase = "Staff Member";
			else cmbCase = cmbKey.SelectedItem.ToString();
			return cmbCase switch
			{
				"Staff Member" => new SolidColorBrush(colours[Convert.ToInt32(rectData[3])]),
				"Is Paid" => new SolidColorBrush(colours[Convert.ToInt32(Convert.ToBoolean(rectData[8]))]),
				"Appointment Type" => new SolidColorBrush(colours[Convert.ToInt32(rectData[2])]),
				"Includes Nail And Teeth" => new SolidColorBrush(colours[Convert.ToInt32(Convert.ToBoolean(rectData[6]))]),
				_ => throw new NotImplementedException(),
			};
		}

		/// <summary>
		/// A looping method to move the currently selected rectangle to the mouse when needed.
		/// </summary>
		private void Loop()
		{
			while (!toExit)
			{
				Dispatcher.Invoke(() => {
					if (mouseDown && Mouse.LeftButton == MouseButtonState.Released)
					{
						UpdateAfterMouseUp(currentlySelected);
					}

					if (mouseDown && currentlySelected is FrameworkElement elem && elem.Parent is FrameworkElement parent)
					{
						Point mousePos = Mouse.GetPosition(parent);

						GetNewMargin(elem, mousePos, out Thickness newMargin, out int roomID, out int dDiff);
						
						// If the element should be moved
						if (newMargin != elem.Margin)
						{
							string[] data = (string[])elem.Tag;
							DateTime day = GetStartOfWeek().AddDays(dDiff);
							TimeSpan appStart = TimeSpan.FromHours(dayStartTime - 1 + newMargin.Top / hourHeight);
							bool doesClash = DBMethods.MiscRequests.DoesAppointmentClash(data, roomID, day, appStart);

							int appLength = DBMethods.MiscRequests.GetAppLength((string[])elem.Tag);

								if (doesClash)
								{
									TimeSpan oldStart = TimeSpan.FromHours(dayStartTime - 1 + elem.Margin.Top / hourHeight);

									if (!DBMethods.MiscRequests.DoesAppointmentClash(data, roomID, day, oldStart))
									{
										elem.Margin = new Thickness(dDiff * dayWidth * spaceBetweenDays + roomID * dayWidth / appRoomCount, elem.Margin.Top, 0, 0);
									}

									int direction = (int)((newMargin.Top - elem.Margin.Top) / Math.Abs(newMargin.Top - elem.Margin.Top));
									int count = 0;

									while (true)
									{
										count += direction;
										TimeSpan toCheck = oldStart.Add(TimeSpan.FromMinutes(count * 15));
										if (toCheck.TotalHours > 18 || toCheck.TotalHours < 9) break;
										bool doesNewClash = DBMethods.MiscRequests.DoesAppointmentClash(data, roomID, day, toCheck);
										if (!doesNewClash)
										{
											TimeSpan halfway = oldStart.Add(TimeSpan.FromMinutes((count * 15 + appLength) / 2));

											TimeSpan mouseTime = TimeSpan.FromMinutes((dayStartTime - 1 + mousePos.Y / hourHeight) * 60);
											bool shouldUpdate = false;
											if (direction == -1 && mouseTime < halfway) shouldUpdate = true;
											else if (direction == 1 && mouseTime > halfway) shouldUpdate = true;

											if (shouldUpdate)
											{
												newMargin.Top = (toCheck.TotalHours - dayStartTime + 1) * hourHeight;
												elem.Margin = newMargin;
											}
											break;
										}
									}

								}
								else elem.Margin = newMargin;
						}
					}
				});
				Thread.Sleep(10);
			}
		}

		private void GetNewMargin(FrameworkElement elem, Point mousePos, out Thickness newMargin, out int roomID, out int dDiff)
		{
			Point diff = (Point)(mousePos - diffMouseAndElem);

			int ySnap = hourHeight / 4;

			// Get the middle of the selected element
			double midLeft = diff.X + elem.Width / 2;
			double midTop = diff.Y + ySnap / 2;

			// Gets the difference in days between the start of the week and the day the selected appointment should be on
			dDiff = (int)(midLeft / (dayWidth * spaceBetweenDays));
			// Gets the x offset that can be used to calculate which room/day the appointment should be placed into.

			// TODO: Should be based on mousePos.X, not midLeft
			double roomIDOffset = (midLeft % (dayWidth * spaceBetweenDays)) / (dayWidth / appRoomCount);

			// Place the appointment in the correct room/day whenever the user tries to place it into the gap between the days
			if (roomIDOffset > appRoomCount && roomIDOffset < appRoomCount + 0.5)
			{
				roomID = appRoomCount - 1;
			}
			else if (roomIDOffset > appRoomCount + 0.5)
			{
				roomID = 0;
				dDiff++;
			}
			else
			{
				roomID = (int)roomIDOffset;
			}

			// Allow the selected week to be changed if the currently selected element is moved to either side of the grid
			if (mousePos.X > dayWidth * spaceBetweenDays * 6 + dayWidth && !hasMoved)
			{
				datePicker.SelectedDate = datePicker.SelectedDate.Value.AddDays(7);
				hasMoved = true;
			}
			else if (mousePos.X < 0 && !hasMoved)
			{
				datePicker.SelectedDate = datePicker.SelectedDate.Value.AddDays(-7);
				hasMoved = true;
			}
			else if (mousePos.X > 0 && mousePos.X < dayWidth * spaceBetweenDays * 6 + dayWidth) hasMoved = false;

			// 'Snap' the appointment to a grid to represent where it should be
			newMargin = new Thickness(dDiff * dayWidth * spaceBetweenDays + roomID * dayWidth / appRoomCount, midTop - midTop % ySnap, 0, 0);

			newMargin.Top = Math.Max(hourHeight, newMargin.Top);
			newMargin.Top = Math.Min((dayEndTime + 1 - dayStartTime) * hourHeight - elem.Height, newMargin.Top);
		}

		/// <summary>
		/// Creates a duplicate of the rectangle clicked on to be moved around.
		/// </summary>
		private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
		{
			mouseDown = true;

			// Reset the stroke around the previously selected element
			if (currentlySelected is Rectangle rct)
			{
				rct.Stroke = Brushes.Black;
				rct.StrokeThickness /= 2;
			}

			// Update which element is currently selected
			currentlySelected = (FrameworkElement)sender;

			if (currentlySelected is FrameworkElement element && element.Parent is FrameworkElement parent)
			{
				diffMouseAndElem = (Point)(Mouse.GetPosition(parent) - new Point(element.Margin.Left, element.Margin.Top));
				if (element is Rectangle rect)
				{
					// Create a new rectangle with the same properties as the old one
					Rectangle newRect = new Rectangle
					{
						Width = rect.Width,
						Height = rect.Height,
						Margin = rect.Margin,
						Fill = rect.Fill,
						Stroke = Brushes.AliceBlue, // A different stroke help marks out which appointment is currently selected
						StrokeThickness = rect.StrokeThickness * 2,
						Tag = rect.Tag,
						VerticalAlignment = rect.VerticalAlignment,
						HorizontalAlignment = rect.HorizontalAlignment
					};

					// The new rectangle should be displayed over the hour line markings for now
					Panel.SetZIndex(newRect, 1);
					editingSidebar.ChangeSelectedData((string[])rect.Tag);

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
			if (sender != currentlySelected) return;

			if (sender is FrameworkElement f)
			{
				UpdateAfterMouseUp(f);
			}
		}

		private void UpdateAfterMouseUp(FrameworkElement f)
		{
			mouseDown = false;
			Panel.SetZIndex(f, 0);

			// Get the middle of the selected element
			double midLeft = f.Margin.Left + f.Width / 2;

			// Gets the difference in days between the start of the week and the day the selected appointment should be on
			int dDiff = (int)(midLeft / (dayWidth * spaceBetweenDays));
			// Gets the x offset that can be used to calculate which room/day the appointment should be placed into.
			int roomID = (int)(midLeft % (dayWidth * spaceBetweenDays) / (dayWidth / appRoomCount));

			string idColumnName = "Appointment ID";
			DateTime startOfWeek = GetStartOfWeek();
			DateTime appDate = startOfWeek.AddDays(dDiff);
			// Save the user's changes to the database
			string appID = ((string[])f.Tag)[0];
			DBMethods.MiscRequests.UpdateColumn(tableName, appDate.ToString("yyyy-MM-dd"), "Appointment Date", idColumnName, appID);

			TimeSpan t = TimeSpan.FromHours(dayStartTime - 1 + f.Margin.Top / hourHeight);
			DBMethods.MiscRequests.UpdateColumn(tableName, t.ToString("hh\\:mm"), "Appointment Time", idColumnName, appID);

			DBMethods.MiscRequests.UpdateColumn(tableName, roomID.ToString(), "Grooming Room ID", idColumnName, appID);

			string[] newData = DBMethods.MiscRequests.GetByColumnData(tableName, idColumnName, appID, columns.Select(x => x.Name).ToArray())[0].ToArray();
			f.Tag = newData;
			editingSidebar.ChangeSelectedData(newData);
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
			// Clear the previous results
			grdResults.Children.Clear();

			foreach (FrameworkElement fr in labelElements)
			{
				grd.Children.Remove(fr);
			}
			labelElements.Clear();

			// If something is currently selected and the mouse is down, the selected element should be carried over to the next week
			if (!(currentlySelected is null) && mouseDown)
			{
				// TODO: For now, this just adds it in the same position it was in before. Is there anything I can do about this?
				grdResults.Children.Add(currentlySelected);
			}

			// Add labels to show hour of day
			for (double i = dayStartTime; i < dayStartTime + 12; i += 1)
			{
				Label lblTime = new Label()
				{
					Content = TimeSpan.FromHours(i).ToString(@"hh\:mm") + "   ",
					IsHitTestVisible = false,
					HorizontalAlignment = HorizontalAlignment.Right,
					Margin = new Thickness(0, (i - dayStartTime + 1) * hourHeight - 24 + baseYOffset, grdResults.Width + grdResults.Margin.Right, 0)
				};

				// Horizontal translucent line to mark hour
				Rectangle hourLine = new Rectangle()
				{
					Height = 2,
					Width = dayWidth * 7 * spaceBetweenDays * 0.98,
					Margin = new Thickness(0, (i - dayStartTime + 1) * hourHeight + baseYOffset - 1, grdResults.Margin.Right + 4, 0),
					Fill = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
					Opacity = 0.15,
					IsHitTestVisible = false,
					VerticalAlignment = VerticalAlignment.Top,
					HorizontalAlignment = HorizontalAlignment.Right
				};

				Panel.SetZIndex(lblTime, 1);
				Panel.SetZIndex(hourLine, 1);
				grd.Children.Add(lblTime);
				grd.Children.Add(hourLine);
				labelElements.Add(lblTime);
				labelElements.Add(hourLine);
			}


			int days = 7;
			for (int i = 0; i < days; i++)
			{
				// Add a background to mark out each day
				Rectangle dayBackground = new Rectangle
				{
					Width = dayWidth + 4,
					Height = hourHeight * 11,
					Margin = new Thickness(i * dayWidth * spaceBetweenDays - 2, hourHeight, 0, 0),
					Fill = new SolidColorBrush(Color.FromRgb(11, 11, 11)),
					StrokeThickness = 0,
					VerticalAlignment = VerticalAlignment.Top,
					HorizontalAlignment = HorizontalAlignment.Left
				};
				Panel.SetZIndex(dayBackground, -1);
				grdResults.Children.Add(dayBackground);

				DateTime startOfWeek = GetStartOfWeek();
				DateTime currentDay = startOfWeek.AddDays(i);

				// Label each day of the week
				Label lblDayOfWeek = new Label()
				{
					Content = currentDay.DayOfWeek,
					Margin = new Thickness(i * dayWidth * spaceBetweenDays - 10, hourHeight - 40, 0, 0),
					Foreground = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
					Width = dayWidth + 20,
					HorizontalContentAlignment = HorizontalAlignment.Center
				};
				grdResults.Children.Add(lblDayOfWeek);

				// Generate all the rectangles to represent the appointments on that day
				List<List<string>> results = DBMethods.MiscRequests.GetAllAppointmentsOnDay(currentDay, columns.Select(x => x.Name).ToArray());
				foreach (List<string> ls in results)
				{
					GenRectFromData(ls.ToArray());
				}
			}
		}

		/// <summary>
		/// Gets the date that is at the start of the selected week
		/// </summary>
		private DateTime GetStartOfWeek()
		{
			DateTime picked = (DateTime)datePicker.SelectedDate;
			return picked.AddDays(-DayOfWeekToInt(picked.DayOfWeek));
		}

		/// <summary>
		/// Update the selected data from the rectangle
		/// </summary>
		public void UpdateFromSidebar(string[] data, bool isNew)
		{
			if (!isNew)
			{
				Rectangle r = grdResults.Children.OfType<Rectangle>().Where(r => !(r.Tag is null) && ((string[])r.Tag)[0] == data[0]).First();
				grdResults.Children.Remove(r);
			}
			GenRectFromData(data);
			DBMethods.DBAccess.UpdateTable(tableName, columns.Select(x => x.Name).ToArray(), data, isNew);
		}

		internal void CancelApp()
		{
			// TODO: Should the appointment be deleted, or just cancelled?
			string[] data = (string[])currentlySelected.Tag;
			DBMethods.MiscRequests.DeleteItem(tableName, columns[0].Name, data[0].ToString());
			grdResults.Children.Remove(currentlySelected);
		}

		/// <summary>
		/// Generate a rectangle from the passed in data
		/// </summary>
		private void GenRectFromData(string[] data)
		{
			// Do not generate rectangles for cancelled appointments
			if (data[7] == "True") return;

			int roomID = Convert.ToInt32(data[5]);
			int typeID = Convert.ToInt32(data[2]);
			DateTime d = DateTime.Parse(data[9]).Add(TimeSpan.Parse(data[10]));
			int dDiff = (d.Date - (DateTime)datePicker.SelectedDate).Days;
			dDiff += DayOfWeekToInt(((DateTime)datePicker.SelectedDate).DayOfWeek);

			// appLength is in minutes
			double appLength = DBMethods.MiscRequests.GetAppLength(data);

			Rectangle newRect = new Rectangle
			{
				Width = dayWidth / appRoomCount,
				Height = appLength / 60 * hourHeight,
				Margin = new Thickness(dDiff * dayWidth * spaceBetweenDays + roomID * dayWidth / appRoomCount, (d.TimeOfDay.TotalHours - dayStartTime + 1) * hourHeight, 0, 0),
				Fill = GetColourForRect(data),
				Stroke = Brushes.Black,
				StrokeThickness = 1,
				Tag = data,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left
			};
			newRect.MouseDown += Rectangle_MouseDown;
			newRect.MouseUp += RctRect_MouseUp;
			grdResults.Children.Add(newRect);
		}

		/// <summary>
		/// Turn a DayOfWeek into an integer
		/// </summary>
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
				_ => throw new NotImplementedException(),
			};
		}
	}
}