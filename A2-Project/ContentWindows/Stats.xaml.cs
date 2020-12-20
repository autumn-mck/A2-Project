using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for Stats.xaml
	/// </summary>
	public partial class Stats : Window
	{
		private static Random random = new Random();

		public Stats()
		{
			InitializeComponent();
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
			getData(ref data, ref xAxisLabels);

			int max = GetMax(data);
			LabelGraph(grid, data[0].Length, xAxisLabels, title, prefix, suffix, max);
			GenerateBars(grid, data, max);
		}

		private static void GenerateLineGraph(GetData getData, Grid grid, string title, string prefix = "", string suffix = "")
		{
			int[][] data = new int[1][];
			string[] xAxisLabels = Array.Empty<string>();
			getData(ref data, ref xAxisLabels);

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
					header.RenderTransform = new RotateTransform(90, 0, 0);
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
				if (isColoured)
				{
					Byte[] b = new Byte[3];
					random.NextBytes(b);
					colour = Color.FromRgb(b[0], b[1], b[2]);
				}
				else colour = Colors.White;
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

		private delegate void GetData(ref int[][] data, ref string[] xAxisLabels);

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
	}
}
