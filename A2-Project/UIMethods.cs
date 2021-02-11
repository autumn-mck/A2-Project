using A2_Project.DBObjects;
using A2_Project.UserControls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace A2_Project
{
	public static class UIMethods
	{
		public static FrameworkElement GenAppropriateElement(Column c, out string title, bool isPKeyEditable = false)
		{
			FrameworkElement elem;
			title = c.Name;
			// If the item displays a primary key, it is not editable by the user, so a label is used to display the data without it being editable
			if (c.Constraints.IsPrimaryKey && !isPKeyEditable)
			{
				elem = new Label()
				{
					Content = "",
					Margin = new Thickness(0, -5, 0, 0)
				};
			}
			// An SQL bit is a boolean value, so a checkbox can be used to help prevent insertion of invalid data
			else if (c.Constraints.Type == "bit")
			{
				elem = new CheckBox()
				{
					Margin = new Thickness(5, 0, 0, 0),
					RenderTransform = new ScaleTransform(2, 2)
				};
			}
			else if (c.Constraints.Type == "date")
			{
				elem = new ValidatedDatePicker(c)
				{
					Margin = new Thickness(5, 0, 0, 0),
					FontSize = 16,
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top
				};
			}
			else if (c.Name == "Appointment Type ID" && !c.Constraints.IsPrimaryKey)
			{
				elem = new ComboBox()
				{
					ItemsSource = DBMethods.MetaRequests.GetAllFromTable("Appointment Type").Select(x => x[3]),
					Margin = new Thickness(5, 0, 0, 0)
				};
				title = "Appointment Type";
			}
			else if (c.Name == "Staff ID" && !c.Constraints.IsPrimaryKey)
			{
				elem = new ComboBox()
				{
					ItemsSource = DBMethods.MetaRequests.GetAllFromTable("Staff").Select(x => x[1]),
					Margin = new Thickness(5, 0, 0, 0)
				};
				title = "Staff Member";
			}
			else if (c.Name == "Grooming Room ID" && !c.Constraints.IsPrimaryKey)
			{
				elem = new ComboBox()
				{
					ItemsSource = DBMethods.MetaRequests.GetAllFromTable("Grooming Room").Select(x => x[1]),
					Margin = new Thickness(5, 0, 0, 0)
				};
				title = "Grooming Room";
			}
			// Otherwise, a text box is used to allow the user to enter data
			else
			{
				elem = new ValidatedTextbox(c)
				{
					Margin = new Thickness(5, 0, 0, 0),
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top
				};

				// If the text box has the potential of containing a lot of data, double its height to allow the text it contains to be easier to read.
				// TODO: Enforce max length
				if (c.Constraints.Type == "varchar")
					if (Convert.ToInt32(c.Constraints.MaxSize) > 50)
						((ValidatedTextbox)elem).SetHeight(elem.Height * 2);
			}


			return elem;
		}
	}
}
