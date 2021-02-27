using A2_Project.DBObjects;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace A2_Project.UserControls
{
	/// <summary>
	/// Interaction logic for FilterManager.xaml
	/// </summary>
	public partial class FilterManager : UserControl
	{
		private List<Filter> filters = new List<Filter>();

		private Column[] columns;

		private int layerLevel;

		private object container;

		public FilterManager(Column[] _columns, object _container, int _layerLevel = 0)
		{
			InitializeComponent();

			columns = _columns;
			container = _container;
			layerLevel = _layerLevel;

			// My thought while writing below:
			// "I'll colour this stackpanel red to symbolise my hatred for it"
			// I think I went mad debugging this
			//stpFiltersInner.Background = Brushes.Red;

			if (layerLevel > 0) lblSave.Visibility = Visibility.Collapsed;

			if (layerLevel % 2 == 0) stp.Background = new SolidColorBrush(Color.FromRgb(37, 37, 37));
			else stp.Background = new SolidColorBrush(Color.FromRgb(21, 21, 21));
		}

		private void AddNewFilter_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Filter filter = new Filter(columns, this, layerLevel);
			filters.Add(filter);
			stpFilters.Children.Add(filter.Panel);
		}

		internal void RemoveFilter(Filter filter)
		{
			filters.Remove(filter);
			stpFilters.Children.Remove(filter.Panel);
			filter.Panel = null;
			filter = null;
		}

		public int GetFilterCount()
		{
			return filters.Count;
		}

		private void SaveChanges_MouseDown(object sender, MouseButtonEventArgs e)
		{
			SaveFilterChanges();
		}

		private void SaveFilterChanges()
		{
			if (container is ContentWindows.FilterableDataGrid dtg) dtg.FiltersSaved();
			else throw new NotImplementedException();
		}

		public List<string> GetTablesReferenced()
		{
			List<string> tables = new List<string>() { columns[0].TableName };

			foreach (Filter f in filters)
			{
				List<string> tablesFromFilter = f.GetReferencedTables();
				foreach (string s in tablesFromFilter)
				{
					if (!tables.Contains(s)) tables.Add(s);
				}
			}

			return tables;
		}

		public string GetFilterText()
		{
			string text = "";

			for (int i = 0; i < filters.Count; i++)
			{
				string toAdd = filters[i].GetFilterText();
				if (toAdd == "") continue;

				// TODO: OR?
				if (i != 0) text += " AND ";

				text += toAdd;
			}

			return text;
		}

		public void ClearFilters(bool saveChanges = true)
		{
			if (filters.Count == 0) return;
			stpFilters.Children.Clear();
			filters = new List<Filter>();
			if (saveChanges) SaveFilterChanges();
		}

		internal void ChangeSearch(int columnIndex, string value)
		{
			ClearFilters(false);
			Filter f = new Filter(columns, this, 0);
			filters.Add(f);
			stpFilters.Children.Add(f.Panel);
			f.ChangeSearch(columnIndex, value);
			SaveFilterChanges();
		}
	}
}
