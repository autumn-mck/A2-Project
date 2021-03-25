using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace A2_Project.ContentWindows
{
	/// <summary>
	/// Interaction logic for AddingWindowTest.xaml
	/// </summary>
	public partial class AddNewClientWindow : Window
	{
		private Grid[] grids;
		private int selIndex = 0;

		private DBObjects.Column[] clientColumns;
		private FilterableDataGrid dtgClientSel;
		private DataEditingSidebar edtClient;

		private DBObjects.Column[] contactColumns;
		private FilterableDataGrid dtgContactSel;
		private DataEditingSidebar edtContact;

		private DBObjects.Column[] dogColumns;
		private FilterableDataGrid dtgDogSel;
		private DataEditingSidebar edtDog;

		private const double circleHeight = 58;

		public AddNewClientWindow()
		{
			InitializeComponent();
			grids = new Grid[] { grdClient, grdContacts, grdDogs };
			rctProgress.Height = circleHeight;

			clientColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Client");
			dtgClientSel = new FilterableDataGrid(clientColumns, this);
			lblClientSelection.Content = dtgClientSel.Content;
			edtClient = new DataEditingSidebar(clientColumns, "Client", this);
			lblClientEditing.Content = edtClient.Content;
			CreateNewUI(grdClientNew, clientColumns);

			contactColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Contact");
			dtgContactSel = new FilterableDataGrid(contactColumns, this);
			lblContactSelection.Content = dtgContactSel.Content;
			edtContact = new DataEditingSidebar(contactColumns, "Contact", this);
			lblContactEditing.Content = edtContact.Content;
			CreateNewUI(grdContactsNew, contactColumns);

			dogColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Dog");
			dtgDogSel = new FilterableDataGrid(dogColumns, this);
			lblDogSelection.Content = dtgDogSel.Content;
			edtDog = new DataEditingSidebar(dogColumns, "Dog", this);
			lblDogEditing.Content = edtDog.Content;
			CreateNewUI(grdDogsNew, dogColumns);
		}

		private static void CreateNewUI(Grid grdContainer, DBObjects.Column[] columns)
		{
			StackPanel stpAll = new StackPanel()
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Orientation = Orientation.Horizontal
			};
			//stpAll.LayoutTransform = new ScaleTransform(0.5, 0.5);

			List<StackPanel> panels = new List<StackPanel>();
			StackPanel currentPanel = new StackPanel()
			{
				VerticalAlignment = VerticalAlignment.Center,
				Orientation = Orientation.Vertical
			};
			panels.Add(currentPanel);

			for (int i = 0; i < columns.Length; i++)
			{
				if (currentPanel.Children.Count >= 8 || currentPanel.Children.Count >= columns.Length / 2.0)
				{
					byte b = (byte)(i * 50);
					currentPanel = new StackPanel()
					{
						Orientation = Orientation.Vertical,
						VerticalAlignment = VerticalAlignment.Center,
						Margin = new Thickness(0, 0, 0, 0),
						Background = new SolidColorBrush(Color.FromRgb(b, b, b)),
						Opacity = 0.3
					};
					panels.Add(currentPanel);
				}

				Label lblColName = new Label()
				{
					Foreground = new SolidColorBrush(Color.FromRgb(241, 241, 241)),
					Margin = new Thickness(0, 0, 0, 0),
					HorizontalAlignment = HorizontalAlignment.Left,
					FontSize = 30,
					Background = Brushes.Orange,
				};
				currentPanel.Children.Add(lblColName);
				FrameworkElement elem = UIMethods.GenAppropriateElement(columns[i], out string title, true, true);
				if (elem is UserControls.ValidatedItem v) v.Background = Brushes.Purple;
				elem.Width = 250;
				lblColName.Content = title;
				currentPanel.Children.Add(elem);
			}

			foreach (StackPanel p in panels)
			{
				stpAll.Children.Add(p);
			}
			StackPanel stpTest = new StackPanel()
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Orientation = Orientation.Vertical
			};
			stpTest.Children.Add(stpAll);
			grdContainer.Children.Add(stpTest);


			Button btnAdd = new Button()
			{
				Content = "Add item",
				Margin = new Thickness(0, 20, 0, 0),
				Width = double.NaN
			};
			stpTest.Children.Add(btnAdd);
		}


		/// <summary>
		/// Widens the menu bar to reveal the hidden text
		/// </summary>
		private void Move(double yAmp, double xAmp, FrameworkElement[] elements, Rectangle progressRectangle = null)
		{
			double tMax = 0.2; // The time taken for the transition in seconds
			double tPassed = 0; // The time passed since the start of the animation
			double prevT = 0; // The time passed the previous time the loop completed
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while ((tPassed = stopwatch.Elapsed.TotalSeconds) <= tMax)
			{
				double yDiff = CalcAnimatedMovement(tPassed, prevT, tMax, yAmp);
				double xDiff = CalcAnimatedMovement(tPassed, prevT, tMax, xAmp);
				// Sinusoidal animation
				foreach (FrameworkElement e in elements)
				{
					Dispatcher.Invoke(() =>
					{
						e.Margin = new Thickness(e.Margin.Left + xDiff,
						e.Margin.Top + yDiff,
						0, 0);
					});
				}
				if (progressRectangle is not null)
				{
					Dispatcher.Invoke(() =>
					{
						double rctHeightDiff = CalcAnimatedMovement(tPassed, prevT, tMax, (grdProgress.Height - circleHeight) / (grids.Length - 1) * yAmp / Math.Abs(yAmp));
						progressRectangle.Height = Math.Max(0, rctProgress.Height - rctHeightDiff);
					});
				}

				prevT = tPassed;
				Thread.Sleep(10);
			}
			stopwatch.Stop();
		}

		private static double CalcAnimatedMovement(double tPassed, double prevT, double tMax, double a)
		{
			return (Math.Sin((tPassed / tMax - 0.5) * Math.PI) - Math.Sin((prevT / tMax - 0.5) * Math.PI)) * a / 2;
		}

		private async void BtnUp_Click(object sender, RoutedEventArgs e)
		{
			if (selIndex <= 0) return;
			selIndex--;
			await Task.Run(() => Move(1000, 0, grids, rctProgress));
		}

		private async void BtnDown_Click(object sender, RoutedEventArgs e)
		{
			if (selIndex >= grids.Length - 1) return;
			selIndex++;
			await Task.Run(() => Move(-1000, 0, grids, rctProgress));
		}

		private async void BtnAddNew_Click(object sender, RoutedEventArgs e)
		{
			Grid g = (Grid)((FrameworkElement)((FrameworkElement)((FrameworkElement)sender).Parent).Parent).Parent;
			FrameworkElement[] elements = g.Children.OfType<FrameworkElement>().ToArray();
			await Task.Run(() => Move(0, -1500, elements));
		}

		private async void BtnSelectExisting_Click(object sender, RoutedEventArgs e)
		{
			Grid g = (Grid)((FrameworkElement)((FrameworkElement)((FrameworkElement)sender).Parent).Parent).Parent;
			FrameworkElement[] elements = g.Children.OfType<FrameworkElement>().ToArray();
			await Task.Run(() => Move(0, 1500, elements));
		}

		internal void StartAddNew(string mode, string clientID)
		{
			// TODO
		}
	}
}
