using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using A2_Project.DBMethods;
using A2_Project.UserControls;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for ShiftManager.xaml
	/// </summary>
	public partial class ShiftManager : Window
	{
		private bool toExit = false;

		private double dayStart = 8;
		private double dayEnd = 19;
		private double dayWidth = 45;
		private double hourHeight = 38;

		private Color[] colours;

		private double resizeHeight = 20;

		private int resizeDir = 0;

		private bool mouseDown = false;

		private Panel currentlySelected;
		// The difference between the position of the mouse and the currently selected element
		private Point diffMouseAndElem;
		private Point mousePos;

		private DBObjects.Column[] shiftColumns;
		private DBObjects.Column[] shiftExcColumns;

		public ShiftManager()
		{
			InitializeComponent();

			shiftColumns = DBObjects.DB.Tables.Where(t => t.Name == "Shift").First().Columns;
			shiftExcColumns = DBObjects.DB.Tables.Where(t => t.Name == "Shift Exception").First().Columns;

			// Get the colours for filters
			colours = new Color[]
			{
				Color.FromRgb(183, 28, 28), // Red
				Color.FromRgb(13, 71, 161), // Blue
				Color.FromRgb(190, 96, 0), // Dark orange? Not quite brown
				Color.FromRgb(27, 94, 32), // Green
				Color.FromRgb(49, 27, 146) // Deep Purple
			};

			rctBase.Width = dayWidth;
			rctBase.Height = hourHeight * 7;

			List<List<string>> staffData = MetaRequests.GetAllFromTable("Staff");
			List<Panel> staffPanels = new List<Panel>();
			foreach (List<string> staff in staffData)
			{
				staffPanels.Add(GenForStaff(staff.ToArray()));
			}

			List<List<string>> shiftData = MetaRequests.GetAllFromTable("Shift");
			foreach (List<string> shift in shiftData)
			{
				Panel shiftPanel = GenShiftWithData(shift.ToArray());
				staffPanels[Convert.ToInt32(shift[1])].Children.Add(shiftPanel);
			}

			UpdateShiftExcs();

			ValidatedTextbox tbxNewExStaff = new ValidatedTextbox(shiftExcColumns[1]);
			tbxNewExStaff.SetWidth(100);
			tbxNewExStaff.ToggleImage();
			tbxNewExStaff.Width = 90;
			tbxNewExStaff.Text = "0";
			stpNewExc.Children.Add(tbxNewExStaff);

			ValidatedDatePicker dtpNewExStart = new ValidatedDatePicker(shiftExcColumns[2]);
			dtpNewExStart.ToggleImage();
			dtpNewExStart.SetWidth(160);
			dtpNewExStart.SelectedDate = DateTime.Now.Date;
			stpNewExc.Children.Add(dtpNewExStart);

			Label lbl = new Label()
			{
				Content = " to ",
				FontSize = 20
			};
			stpNewExc.Children.Add(lbl);

			ValidatedDatePicker dtpNewExEnd = new ValidatedDatePicker(shiftExcColumns[3]);
			dtpNewExEnd.ToggleImage();
			dtpNewExEnd.SetWidth(160);
			dtpNewExEnd.SelectedDate = DateTime.Now.Date.AddDays(7);
			stpNewExc.Children.Add(dtpNewExEnd);

			Button btnConfirmNewEx = new Button()
			{
				Content = " Confirm ",
				FontSize = 20,
				HorizontalAlignment  = HorizontalAlignment.Right
			};
			btnConfirmNewEx.Click += BtnConfirmNewEx_Click;
			grdNewExc.Children.Add(btnConfirmNewEx);

			Thread loopThread = new Thread(Loop)
			{ IsBackground = true };
			loopThread.Start();
		}

		internal void RemoveShiftExc(ShiftException shiftException)
		{
			string excID = shiftException.Tag.ToString();
			MiscRequests.DeleteItem("Shift Exception", "Shift Exception ID", excID, true);
			IEnumerable<Panel> panels = stpShiftExc.Children.OfType<Panel>();
			foreach (Panel p in panels)
			{
				p.Children.Remove(shiftException);
			}
			shiftException = null;
		}

		private void BtnConfirmNewEx_Click(object sender, RoutedEventArgs e)
		{
			ValidatedTextbox tbxNewExStaff = stpNewExc.Children.OfType<ValidatedTextbox>().First();
			ValidatedDatePicker[] dates = stpNewExc.Children.OfType<ValidatedDatePicker>().ToArray();

			if (tbxNewExStaff.IsValid && dates[0].IsValid && dates[1].IsValid && dates[0].SelectedDate <= dates[1].SelectedDate)
			{
				string[] newData = new string[4];
				newData[0] = MiscRequests.GetMinKeyNotUsed("Shift Exception", "Shift Exception ID");
				newData[1] = tbxNewExStaff.Text;
				newData[2] = dates[0].SelectedDate.ToString("dd-MM-yyyy");
				newData[3] = dates[1].SelectedDate.ToString("dd-MM-yyyy");

				DBAccess.UpdateTable("Shift Exception", shiftExcColumns.Select(c => c.Name).ToArray(), newData, true);

				UpdateShiftExcs();
			}
		}

		private void Loop()
		{
			while (!toExit)
			{
				Dispatcher.Invoke(() =>
				{
					if (mouseDown && Mouse.LeftButton == MouseButtonState.Released)
					{
						OnMouseUp();
					}

					if (mouseDown && currentlySelected is not null && currentlySelected.Parent is Panel parent)
					{
						mousePos = Mouse.GetPosition(parent);

						if (resizeDir == 0)
						{
							double newX = mousePos.X - dayWidth / 2;
							double newY = mousePos.Y - diffMouseAndElem.Y;

							if (parent.Name.Contains("grdRes"))
							{
								newX = mousePos.X - mousePos.X % dayWidth;
								newY -= newY % (hourHeight / 2);

								newX = Math.Min(Math.Max(newX, 0), parent.Width - currentlySelected.Width);
								newY = Math.Min(Math.Max(newY, 0) - resizeHeight / 2, parent.Height - currentlySelected.Height + resizeHeight / 2);
							}

							if (!DoesClash(currentlySelected, parent, currentlySelected.Height, newX, newY))
							{
								currentlySelected.Margin = new Thickness(newX, newY, 0, 0);
							}
						}
						else if (resizeDir == -1)
						{
							double diff = mousePos.Y - currentlySelected.Margin.Top;
							if (Math.Abs(diff) > resizeHeight)
							{
								double change = Math.Abs(diff) / diff * hourHeight / 2;
								double newTop = currentlySelected.Margin.Top + change;
								if (currentlySelected.Height - change >= hourHeight / 2 + resizeHeight && newTop > -resizeHeight / 2 - 0.01)
								{
									if (!DoesClash(currentlySelected, parent, currentlySelected.Height - change, currentlySelected.Margin.Left, newTop))
									{
										currentlySelected.Height -= change;
										currentlySelected.Margin = new Thickness(currentlySelected.Margin.Left, newTop, 0, 0);
									}
								}
							}
						}
						else if (resizeDir == 1)
						{
							double diff = mousePos.Y - currentlySelected.Margin.Top;
							double newHeight = diff - (diff) % (hourHeight / 2) + resizeHeight;
							if (newHeight >= hourHeight / 2 + resizeHeight && newHeight + currentlySelected.Margin.Top - resizeHeight < parent.Height + 0.01)
							{
								if (!DoesClash(currentlySelected, parent, newHeight, currentlySelected.Margin.Left, currentlySelected.Margin.Top))
								{
									currentlySelected.Height = newHeight;
								}
							}
						}
					}
				});
				Thread.Sleep(5);
			}
		}

		private bool DoesClash(Panel toCheck, Panel parent, double rctHeight, double marginX, double marginY)
		{
			List<Panel> rcts = parent.Children.OfType<Panel>().Where(p =>
				p != toCheck && 
				p.Margin.Left == marginX && 
				p.Margin.Top + p.Height - resizeHeight >= marginY + resizeHeight / 2 
				&& marginY + rctHeight - resizeHeight / 2 > p.Margin.Top + resizeHeight / 2
				).ToList();
			return rcts.Count > 0;
		}

		private Panel GenShiftWithData(string[] shift)
		{
			TimeSpan start = TimeSpan.Parse(shift[3]);
			TimeSpan end = TimeSpan.Parse(shift[4]);
			Grid grdShift = new Grid()
			{
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left,
				Width = dayWidth,
				Height = hourHeight * (end - start).TotalHours + resizeHeight,
				Margin = new Thickness(dayWidth * Convert.ToInt32(shift[2]), (start.TotalHours - dayStart) * hourHeight - resizeHeight / 2, 0, 0),
				Tag = shift[0]
			};

			double arrowMargin = 5;
			Brush arrowBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
			Rectangle rctArrowUp = new Rectangle()
			{
				Height = 20,
				Width = 20,
				RenderTransform = new RotateTransform(45),
				RenderTransformOrigin = new Point(0.5, 0.5),
				VerticalAlignment = VerticalAlignment.Top,
				Margin = new Thickness(0, arrowMargin, 0, 0),
				Fill = arrowBrush,
				IsHitTestVisible = false
			};
			grdShift.Children.Add(rctArrowUp);
			Rectangle rctArrowDown = new Rectangle()
			{
				Height = 20,
				Width = 20,
				RenderTransform = new RotateTransform(45),
				RenderTransformOrigin = new Point(0.5, 0.5),
				VerticalAlignment = VerticalAlignment.Bottom,
				Margin = new Thickness(0, 0, 0, arrowMargin),
				Fill = arrowBrush,
				IsHitTestVisible = false
			};
			grdShift.Children.Add(rctArrowDown);

			Rectangle rct = new Rectangle()
			{
				Width = dayWidth,
				Stroke = Brushes.Black,
				StrokeThickness = 1,
				VerticalAlignment = VerticalAlignment.Stretch,
				HorizontalAlignment = HorizontalAlignment.Left,
				Fill = new SolidColorBrush(colours[Convert.ToInt32(shift[1])]),
				Name = "rctBase",
				Margin = new Thickness(0, resizeHeight / 2, 0, resizeHeight / 2)
			};
			rct.MouseDown += RctShift_MouseDown;
			grdShift.Children.Add(rct);

			Rectangle resizeTop = new Rectangle()
			{
				Height = resizeHeight,
				VerticalAlignment = VerticalAlignment.Top,
				Cursor = Cursors.SizeNS,
				Fill = Brushes.Transparent
			};
			Rectangle resizeBottom = new Rectangle()
			{
				Height = resizeHeight,
				VerticalAlignment = VerticalAlignment.Bottom,
				Cursor = Cursors.SizeNS,
				Fill = Brushes.Transparent
			};

			resizeTop.MouseDown += Resize_MouseDown;
			resizeBottom.MouseDown += Resize_MouseDown;
			grdShift.Children.Add(resizeTop);
			grdShift.Children.Add(resizeBottom);

			return grdShift;
		}

		private void OnMouseUp()
		{
			resizeDir = 0;
			mouseDown = false;
			if (currentlySelected is null) return;

			Panel parent = (Panel)currentlySelected.Parent;
			if (parent == grd)
			{
				parent.Children.Remove(currentlySelected);
				if (currentlySelected.Tag is string id)
				{
					MiscRequests.DeleteItem("Shift", "Shift ID", id);
				}
				currentlySelected = null;
				return;
			}

			bool isNew = false;
			// If this is a new shift
			if (currentlySelected.Tag is string sTag && sTag == "")
			{
				string id = MiscRequests.GetMinKeyNotUsed("Shift", "Shift ID");
				currentlySelected.Tag = id;
				isNew = true;
			}

			string[] data = GetDataFromShift(currentlySelected);
			DBAccess.UpdateTable("Shift", shiftColumns.Select(c => c.Name).ToArray(), data, isNew);

			currentlySelected.IsHitTestVisible = true;
			currentlySelected.Children.OfType<Rectangle>().Where(r => r.Name == "rctBase").First().IsHitTestVisible = true;
		}

		private string[] GetDataFromShift(Panel from)
		{
			string[] data = new string[5];
			data[0] = from.Tag.ToString();
			data[1] = ((Panel)from.Parent).Name.Substring(6);
			data[2] = ((int)Math.Round(from.Margin.Left / dayWidth)).ToString();
			TimeSpan start = TimeSpan.FromHours((from.Margin.Top + resizeHeight / 2) / hourHeight + dayStart);
			TimeSpan end = start.Add(TimeSpan.FromHours((from.Height - resizeHeight) / hourHeight));
			data[3] = start.ToString("hh\\:mm");
			data[4] = end.ToString("hh\\:mm");

			return data;
		}

		private Panel GenForStaff(string[] staffData)
		{
			Grid grdBg = new Grid()
			{
				Margin = new Thickness(10, 10, 30, 10)
			};

			Grid grdResults = new Grid()
			{
				Width = dayWidth * 7,
				Height = hourHeight * (dayEnd - dayStart),
				Background = new SolidColorBrush(Color.FromRgb(11, 11, 11)),
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left,
				Margin = new Thickness(50, 65, 0, 0),
				Name = "grdRes" + staffData[0]
			};
			grdResults.MouseEnter += GrdResults_MouseEnter;
			grdResults.MouseLeave += GrdResults_MouseLeave;
			grdBg.Children.Add(grdResults);

			Label lblStaffName = new Label()
			{
				IsHitTestVisible = false,
				Width = dayWidth * 7,
				Margin = new Thickness(grdResults.Margin.Left, 0, 0, 0),
				Content = staffData[1],
				HorizontalContentAlignment = HorizontalAlignment.Center,
				FontSize = 24
			};
			grdBg.Children.Add(lblStaffName);

			for (int i = 0; i < 7; i++)
			{
				// Label each day of the week
				Label lblDayOfWeek = new Label()
				{
					Content = IntToDOWStr(i),
					Margin = new Thickness(i * dayWidth + grdResults.Margin.Left, grdResults.Margin.Top - 25, 0, 0),
					Width = dayWidth,
					HorizontalContentAlignment = HorizontalAlignment.Center,
					IsHitTestVisible = false
				};
				grdBg.Children.Add(lblDayOfWeek);
			}

			for (double i = dayStart; i <= dayEnd; i += 1)
			{
				Label lblTime = new Label()
				{
					Content = TimeSpan.FromHours(i).ToString(@"hh\:mm"),
					IsHitTestVisible = false,
					Foreground = new SolidColorBrush(Color.FromRgb(241, 241, 241)),
					Margin = new Thickness(0, grdResults.Margin.Top + (i - dayStart) * hourHeight - 15, 0, 0)
				};

				// Horizontal translucent line to mark hour
				Rectangle hourLine = new Rectangle()
				{
					Height = 2,
					Width = dayWidth * 7 + 10,
					Margin = new Thickness(grdResults.Margin.Left -  10, grdResults.Margin.Top + (i - dayStart) * hourHeight - 1, 0, 0),
					Fill = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
					Opacity = 0.15,
					IsHitTestVisible = false,
					VerticalAlignment = VerticalAlignment.Top,
					HorizontalAlignment = HorizontalAlignment.Left
				};

				Panel.SetZIndex(lblTime, 1);
				Panel.SetZIndex(hourLine, 1);
				grdBg.Children.Add(lblTime);
				grdBg.Children.Add(hourLine);
			}

			wrpStaff.Children.Add(grdBg);

			return grdResults;
		}

		private string IntToDOWStr(int index)
		{
			string[] dow = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
			return dow[index];
		}

		private void RctBase_MouseDown(object sender, MouseButtonEventArgs e)
		{
			TimeSpan start = TimeSpan.FromHours(dayStart);
			Panel grdShift = GenShiftWithData(new string[] { "", "0", "0", start.ToString("hh\\:mm"), start.Add(TimeSpan.FromHours(8)).ToString("hh\\:mm") });
			diffMouseAndElem = new Point(grdShift.Width / 2, grdShift.Height / 2);
			grdShift.Children.OfType<Rectangle>().Where(r => r.Name == "rctBase").First().IsHitTestVisible = false;
			currentlySelected = grdShift;
			grd.Children.Add(grdShift);
			mouseDown = true;
		}

		private void Resize_MouseDown(object sender, MouseButtonEventArgs e)
		{
			FrameworkElement fr = (FrameworkElement)sender;
			if (fr.VerticalAlignment == VerticalAlignment.Top) resizeDir = -1;
			else if (fr.VerticalAlignment == VerticalAlignment.Bottom) resizeDir = 1;
			else throw new NotImplementedException();
			currentlySelected = (Panel)fr.Parent;
			mouseDown = true;
		}

		private void RctShift_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Rectangle rctSender = (Rectangle)sender;
			Panel parent = (Panel)rctSender.Parent;
			diffMouseAndElem = Mouse.GetPosition(rctSender);
			rctSender.IsHitTestVisible = false;
			parent.IsHitTestVisible = false;
			currentlySelected = parent;
			mouseDown = true;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			toExit = true;
		}

		private void GrdResults_MouseEnter(object sender, MouseEventArgs e)
		{
			if (mouseDown && currentlySelected is not null && currentlySelected.Parent == grd)
			{
				Grid grdSender = (Grid)sender;
				grd.Children.Remove(currentlySelected);

				if (currentlySelected is Grid grdSel)
				{
					Rectangle r = grdSel.Children.OfType<Rectangle>().Where(r => r.Name == "rctBase").First();
					r.Fill = new SolidColorBrush(colours[Convert.ToInt32(grdSender.Name.Substring(6))]);
				}

				grdSender.Children.Add(currentlySelected);
			}
		}

		private void GrdResults_MouseLeave(object sender, MouseEventArgs e)
		{
			if (mouseDown && resizeDir == 0 && currentlySelected is not null && currentlySelected.Parent == sender)
			{
				Grid grdSender = (Grid)sender;
				grdSender.Children.Remove(currentlySelected);

				if (currentlySelected is Grid grdSel)
				{
					rctBase.Fill = Brushes.Orange;
					Rectangle r = grdSel.Children.OfType<Rectangle>().Where(r => r.Name == "rctBase").First();
					r.Fill = Brushes.Orange;
				}

				grd.Children.Add(currentlySelected);
			}
		}

		internal void UpdateShiftExc(ShiftException shiftException)
		{
			DBAccess.UpdateTable("Shift Exception", shiftExcColumns.Select(s => s.Name).ToArray(), shiftException.GetData(), false);
			UpdateShiftExcs();
			// TODO: Scroll to updated item or something?
		}

		private void UpdateShiftExcs()
		{
			stpShiftExc.Children.Clear();
			List<List<string>> pastShiftExcData = DBAccess.GetListStringsWithQuery("SELECT * FROM [Shift Exception] WHERE [End Date] < GETDATE() ORDER BY [Start Date];");
			List<List<string>> currentShiftExcData = DBAccess.GetListStringsWithQuery("SELECT * FROM [Shift Exception] WHERE [End Date] >= GETDATE() AND [Start Date] <= GETDATE() ORDER BY [Start Date];");
			List<List<string>> futureShiftExcData = DBAccess.GetListStringsWithQuery("SELECT * FROM [Shift Exception] WHERE [Start Date] >= GETDATE() ORDER BY [Start Date];");

			StackPanel stpPast = new StackPanel()
			{
				Orientation = Orientation.Vertical
			};

			StackPanel stpCurrent = new StackPanel()
			{
				Orientation = Orientation.Vertical
			};

			StackPanel stpFuture = new StackPanel()
			{
				Orientation = Orientation.Vertical
			};

			foreach (List<string> excData in pastShiftExcData)
			{
				ShiftException shiftException = new ShiftException(excData, this);
				stpPast.Children.Add(shiftException);
			}

			foreach (List<string> excData in currentShiftExcData)
			{
				ShiftException shiftException = new ShiftException(excData, this);
				stpCurrent.Children.Add(shiftException);
			}

			foreach (List<string> excData in futureShiftExcData)
			{
				ShiftException shiftException = new ShiftException(excData, this);
				stpFuture.Children.Add(shiftException);
			}

			Label lblPast = new Label()
			{
				Content = "Past:",
				FontSize = 24
			};

			Label lblCurrent = new Label()
			{
				Content = "Current:",
				FontSize = 24
			};

			Label lblFuture = new Label()
			{
				Content = "Future:",
				FontSize = 24
			};

			stpShiftExc.Children.Add(lblPast);
			stpShiftExc.Children.Add(stpPast);

			stpShiftExc.Children.Add(lblCurrent);
			stpShiftExc.Children.Add(stpCurrent);


			stpShiftExc.Children.Add(lblFuture);
			stpShiftExc.Children.Add(stpFuture);

			scvShiftExc.ScrollToVerticalOffset(pastShiftExcData.Count * 50 + 40);
		}

		private void BtnClashCount_Click(object sender, RoutedEventArgs e)
		{
			List<List<string>> staffData = MetaRequests.GetAllFromTable("Staff");

			int count = 0;

			foreach (List<string> staff in staffData)
			{
				List<List<string>> staffShiftData = MiscRequests.GetByColumnData("Shift", "Staff ID", staff[0]);

				string queryAppts = $"Set DATEFIRST 1; SELECT *, DATEPART(dw, [Appointment Date]) - 1 FROM [Appointment] WHERE [Appointment].[Staff ID] = {staff[0]} AND GETDATE() < [Appointment].[Appointment Date]";
				List<List<string>> appResults = DBAccess.GetListStringsWithQuery(queryAppts);

				string queryShiftExcs = $"SELECT [Start Date], [End Date] FROM [Shift Exception] WHERE [Staff ID] = {staff[0]};";
				List<List<string>> excResults = DBAccess.GetListStringsWithQuery(queryShiftExcs);

				foreach (List<string> app in appResults)
				{
					bool isInShift = false;
					TimeSpan appStart = TimeSpan.Parse(app[10]);
					int thisAppLength = MiscRequests.GetAppLength(app.ToArray(), null);
					TimeSpan appEnd = appStart.Add(TimeSpan.FromMinutes(thisAppLength));
					DateTime appDate = DateTime.Parse(app[9]);
					foreach (List<string> shift in staffShiftData)
					{
						if (shift[2] != app[11]) continue;

						TimeSpan shiftStart = TimeSpan.Parse(shift[3]);
						TimeSpan shiftEnd = TimeSpan.Parse(shift[4]);

						isInShift = (appStart >= shiftStart && appEnd <= shiftEnd) || isInShift;
					}

					bool isInShiftExc = false;
					foreach (List<string> shiftExc in excResults)
					{
						DateTime excStart = DateTime.Parse(shiftExc[0]);
						DateTime excEnd = DateTime.Parse(shiftExc[1]);

						isInShiftExc = isInShiftExc || (appDate <= excEnd && appDate >= excStart);
					}

					if (!isInShift || isInShiftExc) count++;
				}
			}
			MessageBox.Show($"Warning: There are {count} appointments that clash with staff schedules");
		}
	}
}
