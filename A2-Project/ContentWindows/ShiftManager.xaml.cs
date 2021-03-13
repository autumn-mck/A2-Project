using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for ShiftManager.xaml
	/// </summary>
	public partial class ShiftManager : Window
	{
		private double dayStart = 8;
		private double dayEnd = 17;
		private double dayWidth = 55;

		public ShiftManager()
		{
			InitializeComponent();

			List<List<string>> staffData = DBMethods.MetaRequests.GetAllFromTable("Staff");

			foreach (List<string> staff in staffData)
			{
				GenForStaff(staff.ToArray());
			}
		}

		private void GenForStaff(string[] staffData)
		{
			Grid grdBg = new Grid()
			{
				Margin = new Thickness(10, 10, 30, 10)
			};

			Grid grdResults = new Grid()
			{
				Width = dayWidth * 7,
				Height = 400,
				Background = new SolidColorBrush(Color.FromRgb(11, 11, 11)),
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left,
				Margin = new Thickness(50, 65, 0, 0)
			};
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

				//// Generate all the rectangles to represent the appointments on that day
				//List<List<string>> results = DBMethods.MiscRequests.GetAllAppointmentsOnDay(currentDay, columns.Select(x => x.Name).ToArray());
				//foreach (List<string> ls in results)
				//{
				//	GenRectFromData(ls.ToArray());
				//}
			}

			for (double i = dayStart; i <= dayEnd; i += 1)
			{
				Label lblTime = new Label()
				{
					Content = TimeSpan.FromHours(i).ToString(@"hh\:mm"),
					IsHitTestVisible = false,
					Foreground = new SolidColorBrush(Color.FromRgb(241, 241, 241)),
					Margin = new Thickness(0, grdResults.Margin.Top - grdResults.Height * (i - dayStart) / (dayStart - dayEnd) - 15, 0, 0)
				};

				// Horizontal translucent line to mark hour
				Rectangle hourLine = new Rectangle()
				{
					Height = 2,
					Width = dayWidth * 7 + 10,
					Margin = new Thickness(grdResults.Margin.Left -  10, grdResults.Margin.Top - grdResults.Height * (i - dayStart) / (dayStart - dayEnd) - 1, 0, 0),
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

			stpStaff.Children.Add(grdBg);
		}

		private string IntToDOWStr(int index)
		{
			string[] dow = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
			return dow[index];
		}
	}
}
