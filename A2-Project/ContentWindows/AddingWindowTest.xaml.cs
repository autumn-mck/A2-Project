using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using A2_Project.UserControls;

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
			Grid g = new Grid()
			{
				Background = null,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			double xOffset = 100;
			double yOffset = 100;
			for (int i = 0; i < columns.Length; i++)
			{
				Label lblColName = new Label()
				{
					Content = columns[i].Name,
					Foreground = new SolidColorBrush(Color.FromRgb(241, 241, 241)),
					Margin = new Thickness(xOffset - 5, yOffset, 0, 0),
					FontSize = 30
				};
				yOffset += 40;
				g.Children.Add(lblColName);
				if (columns[i].Constraints.IsPrimaryKey)
				{
					Label lblPKey = new Label()
					{
						Content = "Test",
						Foreground = new SolidColorBrush(Color.FromRgb(241, 241, 241)),
						Margin = new Thickness(xOffset - 5, yOffset, 0, 0),
						FontSize = 30
					};
					g.Children.Add(lblPKey);
				}
				else
				{
					ValidatedItem v;
					if (columns[i].Constraints.Type == "date")
					{
						v = new ValidatedDatePicker(columns[i])
						{
							SelectedDate = DateTime.Now.Date
						};
					}
					else
					{
						v = new ValidatedTextbox(columns[i])
						{
						};
					}
					v.Margin = new Thickness(xOffset, yOffset, 0, 0);
					v.VerticalAlignment = VerticalAlignment.Top;
					v.HorizontalAlignment = HorizontalAlignment.Left;
					v.FontSize = 16;
					g.Children.Add(v);
				}
				yOffset += 40;
			}
			grdContainer.Children.Add(g);
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
