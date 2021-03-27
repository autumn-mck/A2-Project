using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using A2_Project.ContentWindows;

namespace A2_Project.UserControls
{
	/// <summary>
	/// Interaction logic for BookingCreator.xaml
	/// </summary>
	public partial class BookingCreator : UserControl
	{
		private CalandarView container;

		private List<string[]> data;

		public bool IsAdded { get; set; }

		public string BookingID { get; set; }
		public string DogID { get; set; }
		public string StaffID { get; set; }

		public BookingCreator(CalandarView _container, string _bookingID, string _dogID, string _staffID)
		{
			InitializeComponent();
			BookingID = Convert.ToInt32(_bookingID).ToString();
			DogID = _dogID;
			StaffID = _staffID;
			container = _container;

			cbxNewBookType.ItemsSource = new string[] { "Appointment", "Recurring Appointment", "Allergy Therapy" };
			cbxNewBookType.SelectedIndex = 0;
		}

		private void LblDelete_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			container.DeleteBookingPart(this);
		}

		private void CbxNewBookType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			container.RemoveRectsWithTag(this);
			IsAdded = false;
			stpContent.Children.Clear();

			StaffID = container.GetBookingStaffID();
			DogID = container.GetBookingDogID();

			// Standard Appointment
			if (cbxNewBookType.SelectedIndex == 0)
			{
				data = new List<string[]>();
				List<string> suggestedValues = new List<string>();
				foreach (DBObjects.Column c in DBObjects.DB.Tables.Where(t => t.Name == "Appointment").First().Columns)
				{
					suggestedValues.Add(UIMethods.GetSuggestedValue(c, container.BookingParts).ToString());
				}
				data.Add(suggestedValues.ToArray());
				data[0][1] = DogID;
				data[0][3] = StaffID;
				data[0][4] = BookingID;

				Rectangle r = new Rectangle()
				{
					Fill = new SolidColorBrush(Color.FromRgb(183, 28, 28)),
					Width = 120 / 3, // Day width / count in day
					Height = 80, // Hour height
					Stroke = Brushes.Black,
					StrokeThickness = 1,
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top,
					Tag = this,
					Name = "r0"
				};
				r.MouseDown += Rct_MouseDown;
				stpContent.Children.Add(r);
			}
			// Recurring Appointment
			else if (cbxNewBookType.SelectedIndex == 1)
			{
				StackPanel stpTime = new StackPanel()
				{
					Name = "stpTime",
					Orientation = Orientation.Horizontal
				};

				Label lblRepeat = new Label()
				{
					Content = "Repeating every"
				};
				stpTime.Children.Add(lblRepeat);

				TextBox tbxTimePeriod = new TextBox()
				{
					Name = "tbxTimePeriod",
					Margin = new Thickness(10, 0, 10, 0),
					MinWidth = 30,
					MaxWidth = 100,
					FontSize = 24,
					Background = null,
					TextWrapping = TextWrapping.Wrap,
					Foreground = new SolidColorBrush(Color.FromRgb(241, 241, 241)),
					CaretBrush = new SolidColorBrush(Color.FromRgb(241, 241, 241)),
					HorizontalContentAlignment = HorizontalAlignment.Center,
					VerticalContentAlignment = VerticalAlignment.Center,
					Text = "3"
				};
				tbxTimePeriod.PreviewTextInput += Tbx_OnlyAllowNumbers;
				stpTime.Children.Add(tbxTimePeriod);

				ComboBox cbxTimeType = new ComboBox()
				{
					Name = "cbxTimeType",
					ItemsSource = new string[] { "days", "weeks", "months" },
					Width = 100,
					VerticalAlignment = VerticalAlignment.Top,
					LayoutTransform = new ScaleTransform(2, 2)
				};
				stpTime.Children.Add(cbxTimeType);

				TextBox tbxBookCount = new TextBox()
				{
					Name = "tbxBookCount",
					Margin = new Thickness(10, 0, 5, 0),
					MinWidth = 30,
					MaxWidth = 100,
					FontSize = 24,
					Background = null,
					TextWrapping = TextWrapping.Wrap,
					Foreground = new SolidColorBrush(Color.FromRgb(241, 241, 241)),
					CaretBrush = new SolidColorBrush(Color.FromRgb(241, 241, 241)),
					HorizontalContentAlignment = HorizontalAlignment.Center,
					VerticalContentAlignment = VerticalAlignment.Center,
					Text = "4"
				};
				tbxBookCount.PreviewTextInput += Tbx_OnlyAllowNumbers;
				stpTime.Children.Add(tbxBookCount);

				Label lblTimes = new Label()
				{
					Content = "times."
				};
				stpTime.Children.Add(lblTimes);

				stpContent.Children.Add(stpTime);

				StackPanel stpStart = new StackPanel()
				{
					Name = "stpStart",
					Orientation = Orientation.Horizontal,
					Margin = new Thickness(0, 10, 0, 0)
				};

				Label lblStartAt = new Label()
				{
					Content = "Starting at "
				};
				stpStart.Children.Add(lblStartAt);

				ValidatedTextbox tbxStartTime = new ValidatedTextbox(DBObjects.DB.Tables.Where(t => t.Name == "Appointment").First().Columns.Where(c => c.Name == "Appointment Time").First())
				{
					Text = "9:00",
					Width = double.NaN
				};
				stpStart.Children.Add(tbxStartTime);
				tbxStartTime.SetWidth(110);

				CustomizableDatePicker dtpDate = new CustomizableDatePicker()
				{
					LayoutTransform = new ScaleTransform(1.5, 1.5),
					FontSize = 16,
					SelectedDate = ((CalandarView)container).GetSelDate(),
					Margin = new Thickness(10, 0, 0, 0)
				};
				stpStart.Children.Add(dtpDate);

				stpContent.Children.Add(stpStart);

				Button btnUpdate = new Button()
				{
					Content = "Save Changes",
					FontSize = 24,
					HorizontalAlignment = HorizontalAlignment.Left
				};
				btnUpdate.Click += BtnUpdate_Click ;
				stpContent.Children.Add(btnUpdate);

				StackPanel stpGoTo = new StackPanel()
				{
					Orientation = Orientation.Vertical,
					Name = "stpGoTo"
				};
				stpContent.Children.Add(stpGoTo);

				cbxTimeType.SelectedIndex = 2;
			}
			// Allergy Appointment
			else if (cbxNewBookType.SelectedIndex == 2)
			{
				StackPanel stpTime = new StackPanel()
				{
					Name = "stpTime",
					Orientation = Orientation.Horizontal
				};

				Label lblRepeat = new Label()
				{
					Content = "Repeating every"
				};
				stpTime.Children.Add(lblRepeat);

				TextBox tbxTimePeriod = new TextBox()
				{
					Name = "tbxTimePeriod",
					Margin = new Thickness(10, 0, 10, 0),
					MinWidth = 30,
					MaxWidth = 100,
					FontSize = 24,
					Background = null,
					TextWrapping = TextWrapping.Wrap,
					Foreground = new SolidColorBrush(Color.FromRgb(241, 241, 241)),
					CaretBrush = new SolidColorBrush(Color.FromRgb(241, 241, 241)),
					HorizontalContentAlignment = HorizontalAlignment.Center,
					VerticalContentAlignment = VerticalAlignment.Center,
					Text = "4"
				};
				tbxTimePeriod.PreviewTextInput += Tbx_OnlyAllowNumbers;
				stpTime.Children.Add(tbxTimePeriod);

				ComboBox cbxTimeType = new ComboBox()
				{
					Name = "cbxTimeType",
					ItemsSource = new string[] { "days", "weeks", "months" },
					Width = 100,
					VerticalAlignment = VerticalAlignment.Top,
					LayoutTransform = new ScaleTransform(2, 2)
				};
				stpTime.Children.Add(cbxTimeType);

				TextBox tbxBookCount = new TextBox()
				{
					Name = "tbxBookCount",
					Margin = new Thickness(10, 0, 5, 0),
					MinWidth = 30,
					MaxWidth = 100,
					FontSize = 24,
					Background = null,
					TextWrapping = TextWrapping.Wrap,
					Foreground = new SolidColorBrush(Color.FromRgb(241, 241, 241)),
					CaretBrush = new SolidColorBrush(Color.FromRgb(241, 241, 241)),
					HorizontalContentAlignment = HorizontalAlignment.Center,
					VerticalContentAlignment = VerticalAlignment.Center,
					Text = "4"
				};
				tbxBookCount.PreviewTextInput += Tbx_OnlyAllowNumbers;
				stpTime.Children.Add(tbxBookCount);

				Label lblTimes = new Label()
				{
					Content = "times."
				};
				stpTime.Children.Add(lblTimes);

				stpContent.Children.Add(stpTime);

				StackPanel stpStart = new StackPanel()
				{
					Name = "stpStart",
					Orientation = Orientation.Horizontal,
					Margin = new Thickness(0, 10, 0, 0)
				};

				Label lblStartAt = new Label()
				{
					Content = "Starting at "
				};
				stpStart.Children.Add(lblStartAt);

				ValidatedTextbox tbxStartTime = new ValidatedTextbox(DBObjects.DB.Tables.Where(t => t.Name == "Appointment").First().Columns.Where(c => c.Name == "Appointment Time").First())
				{
					Text = "9:00",
					Width = double.NaN
				};
				stpStart.Children.Add(tbxStartTime);
				tbxStartTime.SetWidth(110);

				CustomizableDatePicker dtpDate = new CustomizableDatePicker()
				{
					LayoutTransform = new ScaleTransform(1.5, 1.5),
					FontSize = 16,
					SelectedDate = ((CalandarView)container).GetSelDate(),
					Margin = new Thickness(10, 0, 0, 0)
				};
				stpStart.Children.Add(dtpDate);

				stpContent.Children.Add(stpStart);

				Button btnUpdate = new Button()
				{
					Content = "Save Changes",
					FontSize = 24,
					HorizontalAlignment = HorizontalAlignment.Left
				};
				btnUpdate.Click += BtnUpdate_Click;
				stpContent.Children.Add(btnUpdate);

				StackPanel stpGoTo = new StackPanel()
				{
					Orientation = Orientation.Vertical,
					Name = "stpGoTo"
				};
				stpContent.Children.Add(stpGoTo);

				Label lblErr = new Label()
				{
					Name = "lblErr",
					Visibility = Visibility.Collapsed,
					Foreground = new SolidColorBrush(Color.FromRgb(100, 62, 66))
				};
				stpContent.Children.Add(lblErr);

				cbxTimeType.SelectedIndex = 1;
			}
			else throw new NotImplementedException();
		}

		private void BtnUpdate_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				bool wasPrevAdded = IsAdded;
				IsAdded = true;
				StackPanel stpTime = stpContent.Children.OfType<StackPanel>().Where(s => s.Name == "stpTime").First();
				TextBox tbxTimePeriod = stpTime.Children.OfType<TextBox>().Where(t => t.Name == "tbxTimePeriod").First();
				TextBox tbxBookCount = stpTime.Children.OfType<TextBox>().Where(t => t.Name == "tbxBookCount").First();
				ComboBox cbxTimeType = stpTime.Children.OfType<ComboBox>().Where(c => c.Name == "cbxTimeType").First();

				StackPanel stpStart = stpContent.Children.OfType<StackPanel>().Where(s => s.Name == "stpStart").First();
				ValidatedTextbox tbxStartTime = stpStart.Children.OfType<ValidatedTextbox>().First();
				CustomizableDatePicker dtpDate = stpStart.Children.OfType<CustomizableDatePicker>().First();

				Label lblErr = stpContent.Children.OfType<Label>().Where(l => l.Name == "lblErr").FirstOrDefault();
				if (lblErr is not null) lblErr.Visibility = Visibility.Collapsed;

				int count = Convert.ToInt32(tbxBookCount.Text);
				int timeGap = Convert.ToInt32(tbxTimePeriod.Text);

				data = new List<string[]>();

				// TODO: Option to add appointment to next free slot, not just put down ignoring if it clashes?
				DateTime start = dtpDate.SelectedDate.Value;
				TimeSpan betweenPeriod;
				if (cbxTimeType.SelectedIndex == 0) betweenPeriod = new TimeSpan(timeGap, 0, 0, 0);
				else if (cbxTimeType.SelectedIndex == 1) betweenPeriod = new TimeSpan(timeGap * 7, 0, 0, 0);
				else if (cbxTimeType.SelectedIndex == 2) betweenPeriod = new TimeSpan(timeGap * 28, 0, 0, 0);
				else throw new NotImplementedException();

				if (cbxNewBookType.SelectedIndex == 2 && betweenPeriod.TotalDays < 14)
				{
					IsAdded = wasPrevAdded;
					lblErr.Content = "Error: allergy appointments must be at least 2 weeks apart!";
					lblErr.Visibility = Visibility.Visible;
					return;
				}

				if (cbxNewBookType.SelectedIndex == 2 && count < 4)
				{
					IsAdded = wasPrevAdded;
					lblErr.Content = "Error: allergy appointments must have at least 3 follow ups!";
					lblErr.Visibility = Visibility.Visible;
					return;
				}

				StackPanel stpGoTo = stpContent.Children.OfType<StackPanel>().Where(s => s.Name == "stpGoTo").First();
				stpGoTo.Children.Clear();

				StaffID = container.GetBookingStaffID();
				DogID = container.GetBookingDogID();

				DBObjects.Column[] cols = DBObjects.DB.Tables.Where(t => t.Name == "Appointment").First().Columns;
				for (int i = 0; i < count; i++)
				{
					List<string> suggested = new List<string>();
					for (int j = 0; j < cols.Length; j++)
					{
						suggested.Add(UIMethods.GetSuggestedValue(cols[j], container.BookingParts).ToString());
					}
					data.Add(suggested.ToArray());
					data[i][1] = DogID;
					data[i][3] = StaffID;
					data[i][4] = BookingID;

					if (cbxNewBookType.SelectedIndex == 2) data[i][2] = "3";

					data[i][9] = start.Add(betweenPeriod * i).ToString("yyyy-MM-dd");
					data[i][10] = TimeSpan.Parse(tbxStartTime.Text).ToString("hh\\:mm");

					Label lblFind = new Label()
					{
						Content = $"Go to appointment {i + 1}",
						Name = $"l{i}",
						FontSize = 20
					};
					lblFind.MouseDown += LblFind_MouseDown;
					stpGoTo.Children.Add(lblFind);
				}

				container.RepBookingChanged(this);
			}
			catch
			{
			}
		}

		public void AddRectBack(Rectangle r)
		{
			r.Margin = new Thickness(0, 0, 0, 0);
			stpContent.Children.Clear();
			r.MouseDown += Rct_MouseDown;
			r.Stroke = Brushes.Black;
			r.StrokeThickness = 1;
			stpContent.Children.Add(r);
		}

		private void Rct_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			StaffID = container.GetBookingStaffID();
			DogID = container.GetBookingDogID();

			data[0][1] = DogID;
			data[0][3] = StaffID;

			Rectangle r = (Rectangle)sender;
			r.MouseDown -= Rct_MouseDown;
			Label lblFind = new Label()
			{
				Content = "Go to appointment",
				Name = "l0",
				FontSize = 20
			};
			lblFind.MouseDown += LblFind_MouseDown;
			stpContent.Children.Add(lblFind);
			stpContent.Children.Remove(r);
			container.StartBookAppt(r);
		}

		private void LblFind_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			List<string[]> data = GetData();
			string name = ((FrameworkElement)sender).Name;
			container.SelectSpecificAppointment(data[Convert.ToInt32(name.Substring(1))]);
		}

		internal List<string[]> GetData()
		{
			return data;
		}

		internal void SetData(string[] _data, string index)
		{
			data[Convert.ToInt32(index)] = _data;
		}

		private void Tbx_OnlyAllowNumbers(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			if (!int.TryParse(e.Text, out _)) e.Handled = true;
		}
	}
}