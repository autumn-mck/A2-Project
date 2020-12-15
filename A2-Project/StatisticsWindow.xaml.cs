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
		DBAccess dbAccess;
		public StatisticsWindow(DBAccess _dbAccess)
		{
			InitializeComponent();
			dbAccess = _dbAccess;
			GraphAppTypes();
			GraphStaffBusiness();
			GraphGrowth();
		}

		private static void GenerateGraph(Grid grid, int[] data, string[] headers)
		{
			
			for (int i = 0; i < data.Length; i++)
			{
				Rectangle newRect = new Rectangle
				{
					Width = 400f / data.Length,
					Height = (float)data[i] / data.Max() * 200f,
					Margin = new Thickness(i * (400f / data.Length), 0, 0, 35),
					Fill = Brushes.White,
					Stroke = Brushes.Black,
					StrokeThickness = 2,
					VerticalAlignment = VerticalAlignment.Bottom,
					HorizontalAlignment = HorizontalAlignment.Left
				};
				grid.Children.Add(newRect);

				if (headers != Array.Empty<string>())
				{
					TextBlock label = new TextBlock
					{
						Text = headers[i],
						Margin = new Thickness(i * (400f / data.Length), 0, 0, 0),
						Width = 100,
						Foreground = Brushes.White,
						TextWrapping = TextWrapping.Wrap,
						VerticalAlignment = VerticalAlignment.Bottom,
						HorizontalAlignment = HorizontalAlignment.Left
					};
					grid.Children.Add(label);
				}
			}
		}

		private void GraphAppTypes()
		{
			int[] typesCount = Array.Empty<int>();
			string[] types = Array.Empty<string>();
			dbAccess.GetCountOfAppointmentTypes(ref typesCount, ref types);
			GenerateGraph(grdAppTypes, typesCount, types);
		}

		private void GraphStaffBusiness()
		{
			int[] typesCount = Array.Empty<int>();
			string[] types = Array.Empty<string>();
			dbAccess.GetBusinessOfStaff(ref typesCount, ref types);
			GenerateGraph(grdStaffBusiness, typesCount, types);
		}

		private void GraphGrowth()
		{
			int[] typesCount = Array.Empty<int>();
			string[] types = Array.Empty<string>();
			dbAccess.GetGrowthOverTime(ref typesCount, ref types);
			GenerateGraph(grdGrowth, typesCount, types);
		}
	}
}
