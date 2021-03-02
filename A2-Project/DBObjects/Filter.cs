using A2_Project.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace A2_Project.DBObjects
{
	public class Filter
	{
		public Panel Panel { get; set; }
		public StackPanel stpFilter;

		private FilterManager parent;

		private Column[] columns;
		private Column currentColumn;

		private ComboBox cbxFilterType;

		private Panel filterPanel;
		private StackPanel textEntry;

		private TextBox[] textBoxes;

		private int layerLevel;

		private bool shouldUpdate = true;

		public Filter(Column[] _columns, FilterManager _parent, int _layerLevel)
		{
			columns = _columns;
			parent = _parent;
			layerLevel = _layerLevel;

			Panel = new StackPanel()
			{
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left,
				Height = double.NaN,
				Width = double.NaN,
				Orientation = Orientation.Horizontal,
				Margin = new Thickness(0, 0, 10, 0)
			};

			stpFilter = new StackPanel()
			{
				Orientation = Orientation.Horizontal,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left,
				MinWidth = 770
			};

			Label l = new Label()
			{
				Content = "Filter"
			};
			stpFilter.Children.Add(l);

			cbxFilterType = new ComboBox()
			{
				Margin = new Thickness(5),
				LayoutTransform = new ScaleTransform(1.5, 1.5),
				MinWidth = 100,
				VerticalAlignment = VerticalAlignment.Top,
				ItemsSource = new string[] { "By Value:", "By Reference:" }
			};
			stpFilter.Children.Add(cbxFilterType);
			cbxFilterType.SelectionChanged += CbxFilterType_SelectionChanged;
			cbxFilterType.SelectedIndex = 0;
			

			Panel.Children.Add(stpFilter);

			Label lblDelete = new Label()
			{
				Content = "x",
				Margin = new Thickness(0, -5, -5, 0)
			};
			lblDelete.MouseDown += LblDelete_MouseDown;
			Panel.Children.Add(lblDelete);
		}

		private void CbxFilterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			string filterCase = ((ComboBox)sender).SelectedItem.ToString();
			if (filterPanel is not null) stpFilter.Children.Remove(filterPanel);

			switch (filterCase)
			{
				case "By Value:":
					filterPanel = new StackPanel()
					{
						Orientation = Orientation.Horizontal,
						VerticalAlignment = VerticalAlignment.Top,
						HorizontalAlignment = HorizontalAlignment.Left
					};
					ComboBox cbxColumn = new ComboBox()
					{
						Name = "cbxColumn",
						Margin = new Thickness(5),
						LayoutTransform = new ScaleTransform(1.5, 1.5),
						MinWidth = 100,
						VerticalAlignment = VerticalAlignment.Top
					};
					filterPanel.Children.Add(cbxColumn);
					cbxColumn.SelectionChanged += CbxColumn_SelectionChanged;
					List<string> colSource = new List<string>() { "All columns" };
					colSource.AddRange(columns.Select(x => x.Name));
					cbxColumn.ItemsSource = colSource;
					cbxColumn.SelectedIndex = 0;

					break;
				case "By Reference:":
					filterPanel = new Grid()
					{
						VerticalAlignment = VerticalAlignment.Top,
						HorizontalAlignment = HorizontalAlignment.Left,
						Height = double.NaN,
						Width = double.NaN
					};
					ComboBox cbxTable = new ComboBox()
					{
						ItemsSource = DB.Tables.Select(t => t.Name),
						Margin = new Thickness(5),
						LayoutTransform = new ScaleTransform(1.5, 1.5),
						MinWidth = 100,
						VerticalAlignment = VerticalAlignment.Top,
						HorizontalAlignment = HorizontalAlignment.Left
					};
					cbxTable.SelectionChanged += CbxTable_SelectionChanged;
					cbxTable.SelectedIndex = 0;
					filterPanel.Children.Add(cbxTable);
					break;
			}
			stpFilter.Children.Add(filterPanel);
			
		}

		internal void ChangeSearch(int columnIndex, string value)
		{
			ComboBox cbxColumn = filterPanel.Children.OfType<ComboBox>().Where(cbx => cbx.Name == "cbxColumn").First();
			cbxColumn.SelectedIndex = columnIndex + 1;

			ComboBox cbxValueFilterType = filterPanel.Children.OfType<ComboBox>().Where(cbx => cbx.Name == "cbxValueFilterType").First();
			cbxValueFilterType.SelectedIndex = cbxValueFilterType.ItemsSource.OfType<string>().ToList().IndexOf("Equal To");

			TextBox tbx = textBoxes[0];
			tbx.Text = value;
		}

		private void CbxTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			FilterManager filterManager = filterPanel.Children.OfType<FilterManager>().FirstOrDefault();

			int tableIndex = ((ComboBox)sender).SelectedIndex;

			if (filterManager is not null) filterPanel.Children.Remove(filterManager);

			filterManager = new FilterManager(DB.Tables[tableIndex].Columns, null, layerLevel + 1)
			{
				Margin = new Thickness(-200, 45, 0, 0)
			};

			filterPanel.Children.Add(filterManager);
		}

		private void CbxColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox cbxColumn = (ComboBox)sender;
			if (cbxColumn.SelectedIndex == 0) currentColumn = null;
			else currentColumn = columns[cbxColumn.SelectedIndex - 1];

			ComboBox cbxValueFilterType = filterPanel.Children.OfType<ComboBox>().Where(cbx => cbx.Name == "cbxValueFilterType").FirstOrDefault();

			string prevSel = "";
			if (cbxValueFilterType is not null) prevSel = cbxValueFilterType.Text;
			if (cbxValueFilterType is null)
			{
				cbxValueFilterType = new ComboBox()
				{
					Name = "cbxValueFilterType",
					Margin = new Thickness(5, 5, 55, 5),
					RenderTransform = new ScaleTransform(1.5, 1.5),
					MinWidth = 100,
					VerticalAlignment = VerticalAlignment.Top,
				};
				filterPanel.Children.Add(cbxValueFilterType);
				cbxValueFilterType.SelectionChanged += CbxValueFilterType_SelectionChanged;
			}
			List<string> filterSource = new List<string> { "Contains", "Equal To" };

			if (currentColumn is not null)
			{
				// TODO: Not equal to?
				string type = currentColumn.Constraints.Type;
				if (type == "date" || type == "time" || type == "int")
				{
					filterSource = new List<string>() { "Equal To", "Between", "Less Than", "More Than" };
				}
				else if (type == "bit")
				{
					filterSource = new List<string>() { "Equal To" };
				}
				else
				{
					filterSource = new List<string>() { "Contains", "Equal To" };
				}
			}

			shouldUpdate = false;
			cbxValueFilterType.ItemsSource = filterSource;
			cbxValueFilterType.SelectedIndex = filterSource.IndexOf(prevSel);
			if (cbxValueFilterType.SelectedIndex == -1) cbxValueFilterType.SelectedIndex = 0;
			shouldUpdate = true;

			FilterTypeUpdated(cbxValueFilterType);
		}

		private void CbxValueFilterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			FilterTypeUpdated((ComboBox)sender);
		}

		private void FilterTypeUpdated(ComboBox cbxValueFilterType)
		{
			if (!shouldUpdate) return;

			string[] prevStrings = Array.Empty<string>();

			if (textEntry is not null)
			{
				prevStrings = textBoxes.Select(tbx => tbx.Text).ToArray();

				filterPanel.Children.Remove(textEntry);
			}

			textEntry = new StackPanel()
			{
				Orientation = Orientation.Horizontal
			};
			string filterCase = cbxValueFilterType.SelectedItem.ToString();

			switch (filterCase)
			{
				case "Between":
					TextBox tbx1 = GetNewTextBox(currentColumn);
					Label lbl = new Label()
					{ Content = " and " };
					TextBox tbx2 = GetNewTextBox(currentColumn);
					textEntry.Children.Add(tbx1);
					textEntry.Children.Add(lbl);
					textEntry.Children.Add(tbx2);
					textBoxes = new TextBox[] { tbx1, tbx2 };
					break;

				default:
					TextBox tbx = GetNewTextBox(currentColumn);
					textBoxes = new TextBox[] { tbx };
					textEntry.Children.Add(tbx);
					break;
			}

			for (int i = 0; i < textBoxes.Length && i < prevStrings.Length; i++)
			{
				textBoxes[i].Text = prevStrings[i];
			}

			filterPanel.Children.Add(textEntry);
		}

		private static TextBox GetNewTextBox(Column c)
		{
			TextBox tbx = new TextBox()
			{
				MinWidth = 100,
				VerticalAlignment = VerticalAlignment.Top,
				Margin = new Thickness(5)
			};
			// TODO: Validated text boxes or similar?
			return tbx;
		}

		private void LblDelete_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			parent.RemoveFilter(this);
		}

		public string GetFilterText()
		{
			string toReturn = "";

			string filterCase = cbxFilterType.SelectedItem.ToString();

			if (filterCase == "By Value:")
			{
				ComboBox cbxValueFilterType = filterPanel.Children.OfType<ComboBox>().Where(cbx => cbx.Name == "cbxValueFilterType").First();
				string colRef;
				// If a specific column is selected
				if (currentColumn is not null)
				{
					colRef = $"[{currentColumn.TableName}].[{currentColumn.Name}]";
					string type = currentColumn.Constraints.Type;
					string[] values = new string[textBoxes.Length];
					for (int i = 0; i < textBoxes.Length; i++)
					{
						string str = textBoxes[i].Text;
						// TODO: Decimal etc?
						values[i] = type switch
						{
							"int" => $"{int.Parse(str)}",
							"varchar" => $"'{str}'",
							"date" => $"'{DateTime.Parse(str):yyyy-MM-dd}'",
							"time" => $"'{TimeSpan.Parse(str):hh\\:mm}'",
							"decimal" => $"{double.Parse(str)}",
							"bit" => $"{(bool.Parse(str) ? "1" : "0")}",
							_ => throw new NotImplementedException(),
						};
					}

					toReturn = (cbxValueFilterType.SelectedItem.ToString()) switch
					{
						"Contains" => $"Lower({colRef}) LIKE Lower('%{textBoxes[0].Text}%')",
						"Equal To" => $"{colRef} = {values[0]}",
						"Between" => $"{colRef} BETWEEN {values[0]} AND {values[1]}",
						"Less Than" => $"{colRef} < {values[0]}",
						"More Than" => $"{colRef} > {values[0]}",
						_ => throw new NotImplementedException(),
					};
				}
				else
				{
					if (textBoxes[0].Text == "") return "";
					toReturn = "(";

					int countAdded = 0;

					for (int i = 0; i < columns.Length; i++)
					{
						string toAdd = "";

						string type = columns[i].Constraints.Type;

						string text = textBoxes[0].Text;

						colRef = $"[{columns[i].TableName}].[{columns[i].Name}]";
						switch (cbxValueFilterType.SelectedItem.ToString())
						{
							case "Contains":
								switch (type)
								{
									case "varchar": toAdd = $"Lower({colRef}) LIKE Lower('%{text}%')"; break;
									case "int":
										if (int.TryParse(text, out int intRes))
											toAdd = $"{colRef} = {intRes}";
										break;
									case "decimal":
										if (double.TryParse(text, out double doubleRes))
											toAdd = $"{colRef} = {doubleRes}";
										break;
									case "date":
										if (DateTime.TryParse(text, out DateTime dRes))
											toAdd = $"{colRef} = '{dRes.Date:yyyy-MM-yy}'";
										break;
									case "time":
										if (TimeSpan.TryParse(text, out TimeSpan timeSpan))
											toAdd = $"{colRef} = '{timeSpan:hh\\:mm}'";
										break;
									case "bit":
										if (text.ToLower() == "true" || text.ToLower() == "false")
											toAdd = $"{colRef} = {bool.Parse(text)}";
										break;
									default: throw new NotImplementedException();
								}
								break;
							case "Equal To":
								switch (type)
								{
									case "varchar": toAdd = $"{colRef} = '{text}'"; break;
									case "int":
										if (int.TryParse(text, out int intRes))
											toAdd = $"{colRef} = {intRes}";
										break;
									case "decimal":
										if (double.TryParse(text, out double doubleRes))
											toAdd = $"{colRef} = {doubleRes}";
										break;
									case "date":
										if (DateTime.TryParse(text, out DateTime dRes))
											toAdd = $"{colRef} = '{dRes.Date:yyyy-MM-yy}'";
										break;
									case "time":
										if (TimeSpan.TryParse(text, out TimeSpan timeSpan))
											toAdd = $"{colRef} = '{timeSpan:hh\\:mm}'";
										break;
									case "bit":
										if (text.ToLower() == "true" || text.ToLower() == "false")
											toAdd = $"{colRef} = {bool.Parse(text)}";
										break;
									default: throw new NotImplementedException();
								}
								break;
							default: throw new NotImplementedException();
						}

						if (toAdd != "")
						{
							if (countAdded != 0) toReturn += " OR ";
							toReturn += toAdd;
							countAdded++;
						}

						if (i == columns.Length - 1) toReturn += ")";
					}
				}
			}
			else if (filterCase == "By Reference:")
			{
				FilterManager filterManager = filterPanel.Children.OfType<FilterManager>().FirstOrDefault();
				toReturn = filterManager.GetFilterText();
			}

			return toReturn;
		}

		public List<string> GetReferencedTables()
		{
			string filterCase = cbxFilterType.SelectedItem.ToString();
			if (filterCase == "By Value:") return new List<string>() { columns[0].TableName };
			else if (filterCase == "By Reference:")
			{
				FilterManager filterManager = filterPanel.Children.OfType<FilterManager>().FirstOrDefault();
				return filterManager.GetTablesReferenced();
			}
			else throw new NotImplementedException();
		}
	}
}
