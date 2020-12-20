using System;
using System.Collections.Generic;
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
			grdDogTypes.Children.Clear();
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
			GraphDogTypes();
			GraphIncome();
		}

		private static void GenerateBarGraph(GetData getData, Grid grid, string title, string prefix = "", string suffix = "")
		{
			int[][] data = new int[1][];
			string[] xAxisLabels = Array.Empty<string>();
			// TODO: await?
			getData(ref data, ref xAxisLabels, minDate);

			int max = GetMax(data);
			LabelGraph(grid, data[0].Length, xAxisLabels, title, prefix, suffix, max);
			GenerateBars(grid, data, max);
		}

		private static void GenerateLineGraph(GetData getData, Grid grid, string title, string prefix = "", string suffix = "")
		{
			int[][] data = new int[1][];
			string[] xAxisLabels = Array.Empty<string>();
			getData(ref data, ref xAxisLabels, minDate);

			int max = GetMax(data);
			LabelGraph(grid, data[0].Length, xAxisLabels, title, prefix, suffix, max);
			GenerateLines(grid, data, max, true);
		}

		private static void LabelGraph(Grid grid, int length, string[] xAxisLabels, string title, string prefix, string suffix, int max)
		{
			GenerateTitle(grid, title);
			LabelXAxis(grid, length, xAxisLabels);
			LabelYAxis(grid, max, prefix, suffix);
		}

		private static int GetMax(int[][] arr)
		{
			List<int> maxes = new List<int>();
			foreach (int[] inArr in arr)
			{
				maxes.Add(inArr.Max());
			}
			return maxes.Max();
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

		private static void LabelYAxis(Grid grid, int max, string prefix, string suffix)
		{
			if (max > 20)
			{
				for (double i = 0; i <= 1; i += 0.2)
				{
					int height = (int)RoundToSigFigs(max * i, 2);
					TextBlock tbl = new TextBlock
					{
						Text = prefix + height + suffix + " -",
						Margin = new Thickness(0, 0, 463, (float)height / max * 200f + 28f),
						Foreground = Brushes.White,
						TextWrapping = TextWrapping.Wrap,
						VerticalAlignment = VerticalAlignment.Bottom,
						HorizontalAlignment = HorizontalAlignment.Right
					};
					grid.Children.Add(tbl);
				}
			}
			else if (max == 0)
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
				for (int i = 0; i <= max; i += 2)
				{
					TextBlock tbl = new TextBlock
					{
						Text = prefix + i + suffix + " -",
						Margin = new Thickness(0, 0, 463, (float)i / max * 200f + 28f),
						Foreground = Brushes.White,
						TextWrapping = TextWrapping.Wrap,
						VerticalAlignment = VerticalAlignment.Bottom,
						HorizontalAlignment = HorizontalAlignment.Right
					};
					grid.Children.Add(tbl);
				}
			}
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

		private static void GenerateBars(Grid grid, int[][] data, int max)
		{
			DrawFullArea(grid);
			if (data.Length == 1)
			{
				int[] arr = data[0];
				for (int i = 0; i < arr.Length; i++)
				{
					Rectangle newRect = new Rectangle
					{
						Width = 400f / arr.Length,
						Height = (float)arr[i] / max * 200f,
						Margin = new Thickness(i * (400f / arr.Length) + 40, 0, 0, 35),
						Fill = Brushes.White,
						Stroke = Brushes.Black,
						StrokeThickness = 1,
						VerticalAlignment = VerticalAlignment.Bottom,
						HorizontalAlignment = HorizontalAlignment.Left
					};
					grid.Children.Add(newRect);
				}
			}
		}

		private static void GenerateLines(Grid grid, int[][] data, int max, bool isColoured)
		{
			DrawFullArea(grid);
			foreach (int[] inArr in data)
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

				for (int i = 0; i < inArr.Length - 1; i++)
				{
					Line line = new Line
					{
						Margin = new Thickness(0, 0, 0, 0),
						StrokeThickness = 2,
						Stroke = new SolidColorBrush(colour),
						X1 = 40f + i * (400f / (inArr.Length - 1)),
						X2 = 40f + (i + 1) * (400f / (inArr.Length - 1)),
						Y1 = 200f - ((float)inArr[i] / max * 200f) + 35f,
						Y2 = 200f - ((float)inArr[i + 1] / max * 200f) + 35f,
						HorizontalAlignment = HorizontalAlignment.Left
					};
					grid.Children.Add(line);
				}
			}
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

		private delegate void GetData(ref int[][] data, ref string[] xAxisLabels, DateTime min);

		private void GraphAppTypes()
		{
			GetData getData = new GetData(DBMethods.GraphingRequests.GetCountOfAppointmentTypes);
			GenerateBarGraph(getData, grdAppTypes, "Appointment Types");
		}

		private void GraphStaffBusiness()
		{
			GetData getData = new GetData(DBMethods.GraphingRequests.GetBusinessOfStaff);
			GenerateBarGraph(getData, grdStaffBusiness, "Staff Time Spent Working");
		}

		private void GraphAppByDayOfWeek()
		{
			GetData getData = new GetData(DBMethods.GraphingRequests.GetAppsByDayOfWeek);
			GenerateBarGraph(getData, grdDaysOfWeek, "Appointments By Day Of Week");
		}

		private void GraphAppByMonth()
		{
			GetData getData = new GetData(DBMethods.GraphingRequests.GetBookingsInMonths);
			GenerateBarGraph(getData, grdAppByMonth, "Appointments By Month");
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
			GenerateLineGraph(getData, grdRepeatCustomers, "Return Customers");
		}

		private void GraphDogTypes()
		{
			GetData getData = new GetData(DBMethods.GraphingRequests.GetDogTypesOverTime);
			GenerateLineGraph(getData, grdDogTypes, "Dog Types");
		}

		private void GraphIncome()
		{
			GetData getData = new GetData(DBMethods.GraphingRequests.GetIncomeLastYear);
			GenerateBarGraph(getData, grdIncome, "Income", "£");
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
