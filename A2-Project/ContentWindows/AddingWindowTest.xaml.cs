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
	public partial class AddingWindowTest : Window
	{
		private Grid[] grids;
		private int selIndex = 0;

		private DBObjects.Column[] clientColumns;
		private SearchableDataGrid dtgClientSel;
		private DataEditingSidebar edtClient;

		private DBObjects.Column[] contactColumns;
		private SearchableDataGrid dtgContactSel;
		private DataEditingSidebar edtContact;

		private DBObjects.Column[] dogColumns;
		private SearchableDataGrid dtgDogSel;
		private DataEditingSidebar edtDog;

		private const double circleHeight = 58;

		public AddingWindowTest()
		{
			InitializeComponent();
			grids = new Grid[] { grdClient, grdContacts, grdDogs };
			rctProgress.Height = circleHeight;

			clientColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Client");
			dtgClientSel = new SearchableDataGrid(600, 100, "Client", clientColumns, this);
			lblClientSelection.Content = dtgClientSel.Content;
			edtClient = new DataEditingSidebar(clientColumns, "Client", this);
			lblClientEditing.Content = edtClient.Content;
			CreateNewUI(grdClientNew, clientColumns);

			contactColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Contact");
			dtgContactSel = new SearchableDataGrid(600, 100, "Contact", contactColumns, this);
			lblContactSelection.Content = dtgContactSel.Content;
			edtContact = new DataEditingSidebar(contactColumns, "Contact", this);
			lblContactEditing.Content = edtContact.Content;
			CreateNewUI(grdContactsNew, contactColumns);

			dogColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Dog");
			dtgDogSel = new SearchableDataGrid(600, 100, "Dog", dogColumns, this);
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
					currentPanel = new StackPanel()
					{
						Orientation = Orientation.Vertical,
						VerticalAlignment = VerticalAlignment.Center,
						Margin = new Thickness(-200, 0, 0, 0)
					};
					panels.Add(currentPanel);
				}

				Label lblColName = new Label()
				{
					Foreground = new SolidColorBrush(Color.FromRgb(241, 241, 241)),
					Margin = new Thickness(-5, 0, 0, 0),
					FontSize = 30
				};
				currentPanel.Children.Add(lblColName);
				FrameworkElement elem = UIMethods.GenAppropriateElement(columns[i], out string title, true);
				lblColName.Content = title;
				currentPanel.Children.Add(elem);
			}

			foreach (StackPanel p in panels)
			{
				stpAll.Children.Add(p);
			}
			grdContainer.Children.Add(stpAll);
		}


		/// <summary>
		/// Widens the menu bar to reveal the hidden text
		/// </summary>
		private void Move(double yAmp, double xAmp, FrameworkElement[] elements, Rectangle progressRectangle = null)
		{
			double tMax = 0.4; // The time taken for the transition in seconds
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
	}
}
