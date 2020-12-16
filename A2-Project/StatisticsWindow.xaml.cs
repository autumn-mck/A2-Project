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

namespace A2_Project
{
	/// <summary>
	/// Interaction logic for StatisticsWindow.xaml
	/// </summary>
	public partial class StatisticsWindow : Window
	{
		private readonly DBAccess dbAccess;
		public StatisticsWindow(DBAccess _dbAccess)
		{
			InitializeComponent();
			dbAccess = _dbAccess;
			GraphAppTypes();
			GraphStaffBusiness();
			GraphGrowth();
		}

		private static void GenerateBarGraph(Grid grid, int[] data, string[] xAxisLabels, string title)
		{
			GenerateTitle(grid, title);
			int max = data.Max();
			LabelXAxis(grid, data, xAxisLabels);
			LabelYAxis(grid, max);
			GenerateBars(grid, data, max);
		}

		private static void GenerateLineGraph(Grid grid, int[] data, string[] xAxisLabels, string title)
		{
			GenerateTitle(grid, title);
			int max = data.Max();
			LabelXAxis(grid, data, xAxisLabels);
			LabelYAxis(grid, max);
			GenerateLines(grid, data, max);
		}

		private static void LabelYAxis(Grid grid, int max)
		{
			for (double i = 0; i <= 1; i += 0.2)
			{
				int height = (int)RoundToSigFigs(max * i, 2);
				TextBlock tbl = new TextBlock
				{
					Text = height + " -",
					Margin = new Thickness(0, 0, 463, (float)height / max * 200f + 28f),
					Foreground = Brushes.White,
					TextWrapping = TextWrapping.Wrap,
					VerticalAlignment = VerticalAlignment.Bottom,
					HorizontalAlignment = HorizontalAlignment.Right
				};
				grid.Children.Add(tbl);
			}
		}

		private static void LabelXAxis(Grid grid, int[] data, string[] labels)
		{
			if (labels.Length == data.Length)
			{
				for (int i = 0; i < data.Length; i++)
				{
					TextBlock header = new TextBlock
					{
						Text = labels[i],
						Margin = new Thickness(i * (400f / data.Length) + 40 + 10, 236, 0, 0),
						Width = 400f / data.Length - 20,
						Foreground = Brushes.White,
						TextWrapping = TextWrapping.Wrap,
						TextAlignment = TextAlignment.Center,
						VerticalAlignment = VerticalAlignment.Top,
						HorizontalAlignment = HorizontalAlignment.Left
					};
					grid.Children.Add(header);
				}
			}
			else if (labels.Length == 2) // TODO: Currently only works with 2 labels.
			{
				for (int i = 0; i < labels.Length; i++)
				{
					TextBlock header = new TextBlock
					{
						Text = labels[i],
						Margin = new Thickness(i * (400f / (labels.Length - 1)) + 40 - i * 60, 246, 0, 0),
						Foreground = Brushes.White,
						TextWrapping = TextWrapping.Wrap,
						TextAlignment = TextAlignment.Left,
						VerticalAlignment = VerticalAlignment.Top,
						HorizontalAlignment = HorizontalAlignment.Left
					};
					grid.Children.Add(header);
				}
			}
		}

		private static void GenerateBars(Grid grid, int[] data, int max)
		{
			for (int i = 0; i < data.Length; i++)
			{
				Rectangle newRect = new Rectangle
				{
					Width = 400f / data.Length,
					Height = (float)data[i] / max * 200f,
					Margin = new Thickness(i * (400f / data.Length) + 40, 0, 0, 35),
					Fill = Brushes.White,
					Stroke = Brushes.Black,
					StrokeThickness = 1,
					VerticalAlignment = VerticalAlignment.Bottom,
					HorizontalAlignment = HorizontalAlignment.Left
				};
				grid.Children.Add(newRect);
			}
		}

		private static void GenerateLines(Grid grid, int[] data, int max)
		{
			for (int i = 0; i < data.Length - 1; i++)
			{
				Line line = new Line
				{
					Margin = new Thickness(0, 0, 0, 0),
					StrokeThickness = 2,
					Stroke = Brushes.White,
					X1 = 400f + 30f - i * (400f / data.Length),
					X2 = 400f + 30f - (i + 1) * (400f / data.Length),
					Y1 = (float)data[i] / max * 200f + 45f,
					Y2 = (float)data[i + 1] / max * 200f + 45f,
					HorizontalAlignment = HorizontalAlignment.Left
				};
				grid.Children.Add(line);
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

		private void GraphAppTypes()
		{
			int[] typesCount = Array.Empty<int>();
			string[] types = Array.Empty<string>();
			dbAccess.GetCountOfAppointmentTypes(ref typesCount, ref types);
			GenerateBarGraph(grdAppTypes, typesCount, types, "Appointment Types");
		}

		private void GraphStaffBusiness()
		{
			int[] typesCount = Array.Empty<int>();
			string[] types = Array.Empty<string>();
			dbAccess.GetBusinessOfStaff(ref typesCount, ref types);
			GenerateBarGraph(grdStaffBusiness, typesCount, types, "Staff time spent working");
		}

		private void GraphGrowth()
		{
			int[] typesCount = Array.Empty<int>();
			string[] types = Array.Empty<string>();
			dbAccess.GetGrowthOverTime(ref typesCount, ref types);
			GenerateLineGraph(grdGrowth, typesCount, types, "Customers over time");
		}
	}
}
