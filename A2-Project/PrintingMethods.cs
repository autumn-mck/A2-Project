using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace A2_Project
{
	public static class PrintingMethods
	{
		public static void Print(FrameworkElement toPrint)
		{
			PrintDialog prntDlg = new PrintDialog();
			if (prntDlg.ShowDialog() == true)
			{
				// Save old properties so toPrint can be returned to its original size/shape when printing is done
				Transform oldTransform = toPrint.LayoutTransform;
				Size oldSize = toPrint.DesiredSize;
				System.Printing.PrintCapabilities cap = prntDlg.PrintQueue.GetPrintCapabilities(prntDlg.PrintTicket);
				// Get the size of the printer page
				Size sz = new Size(cap.PageImageableArea.ExtentWidth, cap.PageImageableArea.ExtentHeight);
				Point origin = new Point(cap.PageImageableArea.OriginWidth, cap.PageImageableArea.OriginHeight);
				double scale = Math.Min(sz.Width / toPrint.ActualWidth, sz.Height / toPrint.ActualHeight);

				// Prepare the Element to be printed by resizing it so that it fits fully onto the page
				//toPrint.LayoutTransform = new ScaleTransform(scale, scale);
				//toPrint.Measure(sz);
				//toPrint.Arrange(new Rect(origin, sz));

				prntDlg.PrintVisual(toPrint, "Invoice");

				// Return the element to its original size and shape
				//toPrint.LayoutTransform = oldTransform;
				//toPrint.Measure(oldSize);
				//toPrint.Arrange(new Rect(new Point(0, 0), oldSize));
			}
		}
	}
}
