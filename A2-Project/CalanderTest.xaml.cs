using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
	/// Interaction logic for CalanderTest.xaml
	/// </summary>
	public partial class CalanderTest : Window
	{
		Point diffMouseAndElem;
		private bool mouseDown = false;
		object currentlySelected;
		private bool toExit = false;
		Point diff;

		public CalanderTest()
		{
			InitializeComponent();
			Thread thread = new Thread(Loop);
			thread.Start();
		}

		/// <summary>
		/// A looping method to move the currently selected rectangle to the mouse.
		/// </summary>
		private void Loop()
		{
			while (!toExit)
			{
				Dispatcher.Invoke(() => {
					if (mouseDown && currentlySelected is FrameworkElement elem)
					{
						diff = (Point)(Mouse.GetPosition(Owner) - diffMouseAndElem);
						elem.Margin = new Thickness(diff.X, diff.Y, 0, 0);
						lblCoOrds.Content = new Point(diff.X + diffMouseAndElem.X, diff.Y + diffMouseAndElem.Y);
					}
					lblCoOrds.Content = ((MainWindow)Owner).ActualWidth + ", " + ((MainWindow)Owner).ActualHeight;
				});
				Thread.Sleep(10);
			}
		}

		/// <summary>
		/// Creates a duplicate of the rectangle clicked on to be moved around.
		/// </summary>
		private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
		{
			mouseDown = true;
			currentlySelected = sender;
			if (currentlySelected is FrameworkElement element)
			{
				diffMouseAndElem = (Point)(Mouse.GetPosition(Owner) - new Point(element.Margin.Left, element.Margin.Top));
				if (element is Rectangle rect)
				{
					Rectangle newRect = new Rectangle
					{
						Width = rect.Width,
						Height = rect.Height,
						Margin = rect.Margin,
						Fill = rect.Fill,
						Stroke = rect.Stroke,
						VerticalAlignment = rect.VerticalAlignment,
						HorizontalAlignment = rect.HorizontalAlignment
					};
					newRect.MouseDown += Rectangle_MouseDown;
					newRect.MouseUp += RctRect_MouseUp;
					currentlySelected = newRect;
					grd.Children.Add(newRect);
				}
			}
		}

		/// <summary>
		/// Tries to place the rectangle in an appropriate grid spot one the mouse is lifted.
		/// Note: Is not currently called if the mouse is released while not over the moving rectangle.
		/// </summary>
		private void RctRect_MouseUp(object sender, MouseButtonEventArgs e)
		{
			mouseDown = false;
			// Note: Currently needs a lot of work, as viewboxes and windows in windows seem to confuse this
			Point mPos = new Point(diff.X + diffMouseAndElem.X - 35, diff.Y + diffMouseAndElem.Y);
			if (sender is FrameworkElement f) f.Margin = new Thickness(mPos.X - mPos.X % f.Width, mPos.Y - mPos.Y % f.Height, 0, 0);
		}

		/// <summary>
		/// Ensures that the looping method exits whenever the application closes.
		/// </summary>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			toExit = true;
		}
	}
}
