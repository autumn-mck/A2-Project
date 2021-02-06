using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

		public AddingWindowTest()
		{
			InitializeComponent();
			grids = new Grid[] { grdClient, grdContacts, grdDogs };

			clientColumns = DBMethods.MetaRequests.GetColumnDataFromTable("Client");
			dtgClientSel = new SearchableDataGrid(600, 100, "Client", clientColumns, this);
			lblClientSelection.Content = dtgClientSel.Content;
			edtClient = new DataEditingSidebar(clientColumns, "Client", this);
			lblClientEditing.Content = edtClient.Content;
		}

		/// <summary>
		/// Widens the menu bar to reveal the hidden text
		/// </summary>
		private void Move(int dir)
		{
			double tMax = 1; // The time taken for the transition in seconds
			double a = 1000; // The amplitude of the movement
			double tPassed = 0; // The time passed since the start of the animation
			double prevT = 0; // The time passed the previous time the loop completed
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while ((tPassed = stopwatch.Elapsed.TotalSeconds) <= tMax)
			{
				// Sinusoidal animation
				foreach (Grid g in grids)
				{
					Dispatcher.Invoke(() =>
					{
						g.Margin = new Thickness(g.Margin.Left,
						g.Margin.Top + (Math.Sin(tPassed / tMax * Math.PI / 2) * dir * a - Math.Sin(prevT / tMax * Math.PI / 2) * dir * a),
						0, 0);
					});
				}
				Dispatcher.Invoke(() =>
				{
					double rctHeightDiff = (Math.Sin(tPassed / tMax * Math.PI / 2) - Math.Sin(prevT / tMax * Math.PI / 2)) * dir * grdProgress.Height / (grids.Length - 1);
					rctProgress.Height = Math.Max(0, rctProgress.Height - rctHeightDiff);
				});

				prevT = tPassed;
				Thread.Sleep(10);
			}
			stopwatch.Stop();
		}

		private async void BtnUp_Click(object sender, RoutedEventArgs e)
		{
			if (selIndex <= 0) return;
			selIndex--;
			await Task.Run(() => Move(1));
		}

		private async void BtnDown_Click(object sender, RoutedEventArgs e)
		{
			if (selIndex >= grids.Length - 1) return;
			selIndex++;
			await Task.Run(() => Move(-1));
		}
	}
}
