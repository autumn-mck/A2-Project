using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for Stats.xaml
	/// </summary>
	public partial class Stats : Window
	{
		// Used for generating random colours for some graphs
		private static Random random = new Random();
		private static DateTime minDate = DateTime.Parse("01/01/1753");

		public Stats()
		{
			InitializeComponent();
			cmbTimescale.ItemsSource = new string[] { "All Time", "Last 4 Years", "Last 2 Years", "Last Year", "Last 6 Months", "Last 3 Months", "Last 4 Weeks", "Last Week" };
			cmbTimescale.SelectedIndex = 0;
			cmbTimescale.SelectionChanged += CmbTimescale_SelectionChanged;
			GraphAllGraphs();
		}

		private void ClearAllGraphs()
		{
			grdAppByMonth.Children.Clear();
			grdAppCancelRate.Children.Clear();
			grdAppTypes.Children.Clear();
			grdDaysOfWeek.Children.Clear();
			grdGrowth.Children.Clear();
			grdIncome.Children.Clear();
			grdRepeatCustomers.Children.Clear();
			grdStaffBusiness.Children.Clear();
		}

		private void GraphAllGraphs()
		{
			GraphAppTypes();
			GraphStaffBusiness();
			GraphGrowth();
			GraphAppByDayOfWeek();
			GraphAppByMonth();
			GraphAppCancelRate();
			GraphCustReturn();
			GraphIncome();
			ShowMiscStats();
		}

		private void ShowMiscStats()
		{
			// TODO: Any other misc stats?
			DrawFullArea(grdMiscStats);
			string timePeriod = cmbTimescale.SelectedItem.ToString().ToLower();

			StackPanel stpMisc = new StackPanel()
			{
				Orientation = Orientation.Vertical,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Margin = new Thickness(40, 30, 0, 0)
			};

			double sales = DBMethods.GraphingRequests.GetIncomeSinceDate(minDate, DateTime.Now);
			Label lblSalesLast = new Label()
			{
				Content = $"Sales {timePeriod}: £{sales}"
			};
			stpMisc.Children.Add(lblSalesLast);

			string gpp = "";
			Label lblGPP = new Label()
			{
				Content = $"Gross profit % {timePeriod}: {gpp}%"
			};
			//stpMisc.Children.Add(lblGPP);

			string newCustCount = DBMethods.GraphingRequests.GetNewCusts(minDate);
			Label lblNewCliCount = new Label()
			{
				Content = $"No. new customers over {timePeriod}: {newCustCount}"
			};
			stpMisc.Children.Add(lblNewCliCount);

			string percCap = "";
			Label lblPercCap = new Label()
			{
				Content = $"% capacity over {timePeriod}: {percCap}%"
			};
			//stpMisc.Children.Add(lblPercCap);

			grdMiscStats.Children.Add(stpMisc);
		}

		private static void GenerateBarGraph(GetData getData, Grid grid, string title, string prefix = "", string suffix = "", Brush[] brushes = null)
		{
			double[][] data = new double[1][];
			string[] xAxisLabels = Array.Empty<string>();
			getData(ref data, ref xAxisLabels, minDate);

			double max = GetMax(data);
			double min = GetMin(data);
			LabelGraph(grid, data[0].Length, xAxisLabels, title, prefix, suffix, max, min);
			GenerateBars(grid, data, max, min, brushes, prefix);
		}

		private static void GenerateLineGraph(GetData getData, Grid grid, string title, string prefix = "", string suffix = "")
		{
			double[][] data = new double[1][];
			string[] xAxisLabels = Array.Empty<string>();
			getData(ref data, ref xAxisLabels, minDate);

			double max = GetMax(data);
			double min = GetMin(data);
			LabelGraph(grid, data[0].Length, xAxisLabels, title, prefix, suffix, max, min);
			GenerateLines(grid, data, max, data.Length > 1, min);
		}

		private static void GeneratePieChart(GetData getData, Grid grid, string title)
		{
			double[][] data = new double[1][];
			string[] headers = Array.Empty<string>();
			getData(ref data, ref headers, minDate);

			GenerateTitle(grid, title);
			GeneratePie(grid, data, headers);
		}

		private static void LabelGraph(Grid grid, int length, string[] xAxisLabels, string title, string prefix, string suffix, double max, double min)
		{
			GenerateTitle(grid, title);
			LabelXAxis(grid, length, xAxisLabels);
			LabelYAxis(grid, max, min, prefix, suffix);
		}

		private static double GetMax(double[][] arr)
		{
			double max = 0;
			foreach (double[] inArr in arr)
			{
				max = Math.Max(max, inArr.Max());
			}
			return max;
		}

		private static double GetMin(double[][] arr)
		{
			double min = 0;
			foreach (double[] inArr in arr)
			{
				min = Math.Min(inArr.Min(), min);
			}
			return min;
		}

		private static void DrawFullArea(Grid grid)
		{
			Rectangle newRect = new Rectangle
			{
				Width = 400f,
				Height = 200f,
				Margin = new Thickness(40, 0, 0, 35),
				Fill = new SolidColorBrush(Color.FromRgb(10, 10, 10)),
				VerticalAlignment = VerticalAlignment.Bottom,
				HorizontalAlignment = HorizontalAlignment.Left
			};
			grid.Children.Add(newRect);
		}

		private static void LabelYAxis(Grid grid, double max, double min, string prefix, string suffix)
		{
			if (max == 0 && min == 0)
			{
				TextBlock tbl = new TextBlock
				{
					Text = prefix + 0 + suffix + " -",
					Margin = new Thickness(0, 0, 463, 28f),
					Foreground = Brushes.White,
					TextWrapping = TextWrapping.Wrap,
					VerticalAlignment = VerticalAlignment.Bottom,
					HorizontalAlignment = HorizontalAlignment.Right
				};
				grid.Children.Add(tbl);
			}
			else
			{
				double maxHeight = (double)RoundToSigFigs(max, 2);
				double minHeight = (double)RoundToSigFigs(min, 2);

				double aboveZero = (double)maxHeight / (maxHeight - minHeight);
				double belowZero = 1 - aboveZero;

				double zeroMarginTop = 34 + aboveZero * 200;

				double countAbove = (int)(aboveZero / 0.2) + 1;
				for (double i = 0; i <= countAbove; i++)
				{
					double marginTop = (zeroMarginTop - (i / countAbove) * 200.0 * aboveZero);
					Rectangle rct = new Rectangle()
					{
						Width = 400,
						Height = 3,
						Fill = new LinearGradientBrush(Colors.White, Colors.Black, new Point(0.5, 0.4999), new Point(0.5, 0.5)),
						Margin = new Thickness(40, marginTop, 0, 0),
						Opacity = 0.3,
						SnapsToDevicePixels = true,
						VerticalAlignment = VerticalAlignment.Top,
						HorizontalAlignment = HorizontalAlignment.Left
					};
					if (i == 0) rct.Opacity = 1;

					TextBlock tbl = new TextBlock
					{
						Text = prefix + Math.Round(RoundToSigFigs(i / countAbove * maxHeight, 2), 2, MidpointRounding.AwayFromZero) + suffix + "  ",
						Margin = new Thickness(0, marginTop - 7, 463, 0),
						Foreground = Brushes.White,
						TextWrapping = TextWrapping.Wrap,
						VerticalAlignment = VerticalAlignment.Top,
						HorizontalAlignment = HorizontalAlignment.Right
					};
					Panel.SetZIndex(rct, 1);
					grid.Children.Add(rct);
					grid.Children.Add(tbl);

					GenYLabel(grid, prefix, suffix, maxHeight, countAbove, i, marginTop);
				}

				double countBelow = -(int)(belowZero / 0.2) - 1;
				if (belowZero == 0) countBelow = 0;
				prefix = "-" + prefix;
				for (double i = -1; i >= countBelow; i--)
				{
					double marginTop = (zeroMarginTop + (i / countBelow) * 200.0 * belowZero);
					GenYLabel(grid, prefix, suffix, minHeight, countBelow, i, marginTop);
				}
			}
		}

		private static void GenYLabel(Grid grid, string prefix, string suffix, double maxValue, double countDirection, double i, double marginTop)
		{
			Rectangle rct = new Rectangle()
			{
				Width = 400,
				Height = 2,
				Fill = new LinearGradientBrush(Colors.White, Colors.Black, new Point(0.5, 0.4999), new Point(0.5, 0.5)),
				Margin = new Thickness(40, marginTop, 0, 0),
				Opacity = 0.3,
				SnapsToDevicePixels = true,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left
			};

			TextBlock tbl = new TextBlock
			{
				Text = prefix + Math.Abs(Math.Round(RoundToSigFigs(i / countDirection * maxValue, 2), 2, MidpointRounding.AwayFromZero)) + suffix + "  ",
				Margin = new Thickness(0, marginTop - 7, 463, 0),
				Foreground = Brushes.White,
				TextWrapping = TextWrapping.Wrap,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Right
			};
			Panel.SetZIndex(rct, 1);
			grid.Children.Add(rct);
			grid.Children.Add(tbl);
		}

		private static void LabelXAxis(Grid grid, int length, string[] labels)
		{
			if (labels.Length == length)
			{
				for (int i = 0; i < length; i++)
				{
					TextBlock label = new TextBlock
					{
						Text = labels[i],
						Margin = new Thickness(i * (400f / length) + 40 + 10, 236, 0, 0),
						Width = 400f / length - 20,
						Foreground = Brushes.White,
						TextWrapping = TextWrapping.Wrap,
						TextAlignment = TextAlignment.Center,
						VerticalAlignment = VerticalAlignment.Top,
						HorizontalAlignment = HorizontalAlignment.Left
					};

					if (label.Width < 24)
					{
						label.Width = 24;
						label.Margin = new Thickness(i * (400f / length) + 35 + 10, 236, 0, 0);
					}
					grid.Children.Add(label);
				}
			}
			else
			{
				for (int i = 0; i < labels.Length; i++)
				{
					TextBlock header = new TextBlock
					{
						Text = labels[i],
						Margin = new Thickness(i * (400f / (labels.Length - 1)) + 55 - 15 * i / (labels.Length - 1), 240, 0, 0),
						Foreground = Brushes.White,
						FontSize = 11,
						TextAlignment = TextAlignment.Left,
						VerticalAlignment = VerticalAlignment.Top,
						HorizontalAlignment = HorizontalAlignment.Left
					};
					header.RenderTransform = new RotateTransform(30, 0, 0);
					grid.Children.Add(header);
				}
			}
		}

		private static void GenerateBars(Grid grid, double[][] data, double max, double min, Brush[] brushes, string prefix)
		{
			DrawFullArea(grid);

			double maxHeight = RoundToSigFigs(max, 2);
			double minHeight = RoundToSigFigs(min, 2);

			double aboveZero = (double)maxHeight / (maxHeight - minHeight);
			double belowZero = 1 - aboveZero;

			double zeroMarginTop = 34 + aboveZero * 200;

			if (data.Length == 1)
			{
				double[] arr = data[0];
				for (int i = 0; i < arr.Length; i++)
				{
					double height;
					if (arr[i] < 0) height = (double)arr[i] / minHeight * belowZero * 200;
					else height = (double)arr[i] / maxHeight * aboveZero * 200;

					double marginTop;
					if (arr[i] < 0) marginTop = zeroMarginTop;
					else marginTop = zeroMarginTop - height;

					Brush b;
					if (prefix == "£")
					{

						if (arr[i] < 0)
						{
							double fr = (Math.Min((double)arr[i] / (double)minHeight, 1) + 0.6) / 1.6;
							b = new SolidColorBrush(Color.FromRgb((byte)(210.0 * fr), 0, 0));
						}
						else
						{
							double fr = (Math.Min((double)arr[i] / (double)maxHeight, 1) + 0.6) / 1.6;
							b = new SolidColorBrush(Color.FromRgb(0, (byte)(130.0 * fr), 0));
						}
					}
					else if (brushes is null) b = Brushes.White;
					else
					{
						if (i < brushes.Length) b = brushes[i];
						else b = new SolidColorBrush(GenerateRandomColour());
					}

					Rectangle newRect = new Rectangle
					{
						Width = 400f / arr.Length,
						Height = height,
						Margin = new Thickness(i * (400f / arr.Length) + 40, marginTop, 0, 0),
						Fill = b,
						Stroke = Brushes.Black,
						StrokeThickness = 1,
						VerticalAlignment = VerticalAlignment.Top,
						HorizontalAlignment = HorizontalAlignment.Left
					};
					grid.Children.Add(newRect);
				}
			}
		}

		private static void GenerateLines(Grid grid, double[][] data, double max, bool isColoured, double min)
		{
			DrawFullArea(grid);
			foreach (double[] inArr in data)
			{
				Color colour;
				if (isColoured) colour = GenerateRandomColour();
				else colour = Colors.White;

				if (max == 0)
				{
					Line line = new Line
					{
						Margin = new Thickness(0, 0, 0, 0),
						StrokeThickness = 2,
						Stroke = new SolidColorBrush(colour),
						X1 = 40f,
						X2 = 40f + 400f,
						Y1 = 200f + 35f,
						Y2 = 200f + 35f,
						HorizontalAlignment = HorizontalAlignment.Left
					};
					grid.Children.Add(line);
					break;
				}

				double maxHeight = RoundToSigFigs(max, 2);
				double minHeight = RoundToSigFigs(min, 2);

				double aboveZero = (double)maxHeight / (maxHeight - minHeight);
				double belowZero = 1 - aboveZero;

				double zeroMarginTop = 34 + aboveZero * 200;

				for (int i = 0; i < inArr.Length - 1; i++)
				{
					// TODO: Allow line graphs to graph below 0 properly
					//double height;
					//if (inArr[i] < 0) height = (double)inArr[i] / minHeight * belowZero * 200;
					//else height = (double)inArr[i] / maxHeight * aboveZero * 200;

					//double marginTop;
					//if (inArr[i] < 0) marginTop = zeroMarginTop;
					//else marginTop = zeroMarginTop - height;

					Line line = new Line
					{
						Margin = new Thickness(0, 0, 0, 0),
						StrokeThickness = 2,
						Stroke = new SolidColorBrush(colour),
						X1 = 40f + i * (400f / (inArr.Length - 1)),
						X2 = 40f + (i + 1) * (400f / (inArr.Length - 1)),
						Y1 = zeroMarginTop - ((float)inArr[i] / max * 200f),
						Y2 = zeroMarginTop - ((float)inArr[i + 1] / max * 200f),
						HorizontalAlignment = HorizontalAlignment.Left
					};
					grid.Children.Add(line);
				}
			}
		}

		private static void GeneratePie(Grid grid, double[][] data, string[] headers)
		{
			DrawFullArea(grid);

			// A stack panel which contains the pie chart and the key
			StackPanel stpAll = new StackPanel()
			{
				Orientation = Orientation.Horizontal,
				Margin = new Thickness(65, 10, 0, 0),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top
			};

			// Note: For now, only generates 1 pie chart, regardless of how much data it is given.
			double[] arr = data[0];
			double sum = arr.Sum();
			double totalSoFar = 0;

			// A stack panel to contain the keys
			StackPanel stpKey = new StackPanel()
			{
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Left
			};

			// The brushes used to colour the appointment types
			Brush[] brushes = new Brush[]
			{
				new SolidColorBrush(Color.FromRgb(183, 28, 28)), // Red
				new SolidColorBrush(Color.FromRgb(13, 71, 161)), // Blue
				new SolidColorBrush(Color.FromRgb(190, 96, 0)), // Dark orange? Not quite brown
				new SolidColorBrush(Color.FromRgb(27, 94, 32)), // Green
				new SolidColorBrush(Color.FromRgb(49, 27, 146)) // Deep Purple
			};

			// A grid to contain the pie chart
			Grid pie = new Grid();

			// Radius of the pie chart
			double radius = 90;
			// The centre of the pie chart
			Point startPos = new Point(radius + 15, radius + 35);

			for (int i = 0; i < arr.Length; i++)
			{
				// The angle travelled through so far, in radians
				double angleSoFar = totalSoFar / sum * Math.PI * 2;
				// The first point on the corner between the line between the centre and the arc
				Point p1 = new Point(Math.Sin(angleSoFar) * radius + startPos.X, Math.Cos(angleSoFar) * radius - startPos.Y);
				totalSoFar += arr[i];

				angleSoFar = totalSoFar / sum * Math.PI * 2;
				// The second point on the corner between the line between the centre and the arc
				Point p2 = new Point(Math.Sin(angleSoFar) * radius + startPos.X, Math.Cos(angleSoFar) * radius - startPos.Y);

				// The path will try to take the shorter arc unless told otherwise.
				// However, the longer path is needed instead if the sector needs to take up > 1/2 the circle
				string isLargeArcStr = arr[i] / sum > 0.5 ? "1" : "0";

				// Creates a circle sector for the pie chart to represent arr[i]

				Brush brush;
				if (i < brushes.Length) brush = brushes[i];
				else brush = new SolidColorBrush(GenerateRandomColour());

				Path sector = new Path()
				{
					Fill = brush,
					Data = Geometry.Parse($"M{startPos.X},{startPos.Y} L{p1.X},{-p1.Y} A{radius},{radius} 0 {isLargeArcStr} 1 {p2.X},{-p2.Y} z")
				};
				pie.Children.Add(sector);

				// Creates a line to create a boundary between the sectors
				Path sectorDiv = new Path()
				{
					StrokeThickness = 1.5,
					Stroke = Brushes.Black,
					Data = Geometry.Parse($"M{startPos.X},{startPos.Y} L{p1.X},{-p1.Y}")
				};
				// The divider should appear on top of the sectors
				Panel.SetZIndex(sectorDiv, 1);
				pie.Children.Add(sectorDiv);

				// A stack panel used to give a key for the current sector
				StackPanel label = new StackPanel()
				{
					Orientation = Orientation.Horizontal
				};

				// Creates a rectangle with the colour of the sector as part of the key
				Rectangle rct = new Rectangle()
				{
					Fill = brush,
					Height = 20,
					Width = 20,
					Stroke = Brushes.Black,
					StrokeThickness = 1.5,
					VerticalAlignment = VerticalAlignment.Top,
					Margin = new Thickness(0, 8, 0, 0)
				};

				// Creates a label to show what the key is marking, and gives the actual value the sector is representing 
				Label lbl = new Label()
				{
					Content = headers[i] + $"\n{arr[i]}",
					Foreground = Brushes.White
				};

				label.Children.Add(rct);
				label.Children.Add(lbl);

				stpKey.Children.Add(label);
			}

			// Scale the pie chart size by a factor of this to get its background
			double bgScale = 1.02;
			// Draws a slightly larger circle to act as a background to the pie chart
			Path backPath = new Path()
			{
				Fill = Brushes.Black,
				Data = Geometry.Parse($"M{startPos.X},{startPos.Y - radius * bgScale} " +
				$"A{radius * bgScale},{radius * bgScale} 0 1 1 {startPos.X},{startPos.Y + radius * bgScale} " +
				$"A{radius * bgScale},{radius * bgScale} 0 1 1 {startPos.X},{startPos.Y - radius * bgScale} " +
				$"z")
			};
			// Place the background behind the pie chart
			Panel.SetZIndex(backPath, -1);
			pie.Children.Add(backPath);

			stpAll.Children.Add(stpKey);
			stpAll.Children.Add(pie);
			grid.Children.Add(stpAll);
		}

		private static Color GenerateRandomColour()
		{
			Byte[] b = new Byte[3];
			random.NextBytes(b);
			return Color.FromRgb(b[0], b[1], b[2]);
		}

		private static void GenerateTitle(Grid grid, string title)
		{
			TextBlock tblTitle = new TextBlock
			{
				Text = title,
				Margin = new Thickness(0, 0, 0, 0),
				Foreground = Brushes.White,
				FontSize = 20,
				TextWrapping = TextWrapping.Wrap,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			Panel.SetZIndex(tblTitle, 1);
			grid.Children.Add(tblTitle);
		}

		public static double RoundToSigFigs(double value, int sigDigits)
		{
			if (value == 0.0) return value;
			bool neg = value < 0;
			if (neg) value = -value;
			double m10 = Math.Log10(value);
			double scale = Math.Pow(10, Math.Floor(m10) - sigDigits + 1);
			value = Math.Round(value / scale) * scale;
			if (neg) value = -value;
			return value;
		}

		private delegate void GetData(ref double[][] data, ref string[] xAxisLabels, DateTime min);

		private void GraphAppTypes()
		{
			GetData getData = new GetData(DBMethods.GraphingRequests.GetCountOfAppointmentTypes);
			GeneratePieChart(getData, grdAppTypes, "Appointment Types");
		}

		private void GraphStaffBusiness()
		{
			GetData getData = new GetData(DBMethods.GraphingRequests.GetBusinessOfStaff);
			Color[] colours = new Color[]
			{
				Color.FromRgb(183, 28, 28), // Red
				Color.FromRgb(13, 71, 161), // Blue
				Color.FromRgb(190, 96, 0), // Dark orange? Not quite brown
				Color.FromRgb(27, 94, 32), // Green
				Color.FromRgb(49, 27, 146) // Deep Purple
			};
			GenerateBarGraph(getData, grdStaffBusiness, "No. Of Appointments Per Staff Member", "", "", colours.Select(c => new SolidColorBrush(c)).ToArray());
		}

		private void GraphAppByDayOfWeek()
		{
			GetData getData = new GetData(DBMethods.GraphingRequests.GetAppsByDayOfWeek);
			GenerateBarGraph(getData, grdDaysOfWeek, "Appointments By Day Of Week");
		}

		private void GraphAppByMonth()
		{
			GetData getData = new GetData(DBMethods.GraphingRequests.GetBookingsInMonths);
			GenerateBarGraph(getData, grdAppByMonth, "Appointments By Month (Last Year)");
		}

		private void GraphGrowth()
		{
			GetData getData = new GetData(DBMethods.GraphingRequests.GetGrowthOverTime);
			GenerateLineGraph(getData, grdGrowth, "Clients Over Time");
		}

		private void GraphAppCancelRate()
		{
			GetData getData = new GetData(DBMethods.GraphingRequests.GetAppCancelRate);
			GenerateLineGraph(getData, grdAppCancelRate, "Appointment Cancel Rate Over Time", "", "%");
		}

		private void GraphCustReturn()
		{
			GetData getData = new GetData(DBMethods.GraphingRequests.GetCustReturns);
			GenerateLineGraph(getData, grdRepeatCustomers, "Dog Last Appointment Date");
		}

		private void GraphIncome()
		{
			GetData getData = new GetData(DBMethods.GraphingRequests.GetIncomeLastYear);
			GenerateBarGraph(getData, grdIncome, "Income (Last Year)", "£");
		}

		private void CmbTimescale_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (cmbTimescale.SelectedIndex)
			{
				case 0: minDate = DateTime.Parse("01/01/1753"); break;
				case 1: minDate = DateTime.Now.AddMonths(-48); break;
				case 2: minDate = DateTime.Now.AddMonths(-24); break;
				case 3: minDate = DateTime.Now.AddMonths(-12); break;
				case 4: minDate = DateTime.Now.AddMonths(-6); break;
				case 5: minDate = DateTime.Now.AddMonths(-3); break;
				case 6: minDate = DateTime.Now.AddMonths(-1); break;
				case 7: minDate = DateTime.Now.AddDays(-7); break;
			}
			ClearAllGraphs();
			GraphAllGraphs();
		}
	}
}
