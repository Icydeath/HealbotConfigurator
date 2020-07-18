using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HealbotConfigurator
{
	/// <summary>
	/// Interaction logic for PrioritiesWindow.xaml
	/// </summary>
	public partial class PrioritiesWindow : Window
	{
		public Dictionary<string, Dictionary<string, int>> ModifiedPrioritySets { get; }

		public PrioritiesWindow(Dictionary<string, Dictionary<string, int>> mps)
		{
			InitializeComponent();
			if (mps == null)
				ModifiedPrioritySets = new Dictionary<string, Dictionary<string, int>>();

			ModifiedPrioritySets = mps;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Left = Application.Current.MainWindow.Left + 100;
			Top = Application.Current.MainWindow.Top + 100;
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			SetupSetsCombobox();
		}

		private void SetupSetsCombobox()
		{
			cb_Sets.Items.Clear();
			foreach (var set in ModifiedPrioritySets.Keys)
				cb_Sets.Items.Add(set);
		}

		private void cb_Sets_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SetupActionsCombobox();
		}

		private void SetupActionsCombobox()
		{
			cb_Actions.Items.Clear();

			var set = cb_Sets.SelectedItem != null ? cb_Sets.SelectedItem.ToString() : !string.IsNullOrEmpty(cb_Sets.Text) ? cb_Sets.Text : string.Empty;
			if (ModifiedPrioritySets.ContainsKey(set))
			{
				foreach (var kvp in ModifiedPrioritySets[set].OrderBy(x => x.Value))
				{
					cb_Actions.Items.Add(kvp.Key);
				}
			}
		}

		private void cb_Actions_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var set = cb_Sets.SelectedItem != null ? cb_Sets.SelectedItem.ToString() : !string.IsNullOrEmpty(cb_Sets.Text) ? cb_Sets.Text : string.Empty;
			var action = cb_Actions.SelectedItem != null ? cb_Actions.SelectedItem.ToString() : !string.IsNullOrEmpty(cb_Actions.Text) ? cb_Actions.Text : string.Empty;

			if (ModifiedPrioritySets.ContainsKey(set) && ModifiedPrioritySets[set].ContainsKey(action))
			{
				num_Value.Value = ModifiedPrioritySets[set][action];
			}
		}

		private void Button_SetPriority_Click(object sender, RoutedEventArgs e)
		{
			var set = cb_Sets.SelectedItem != null ? cb_Sets.SelectedItem.ToString() : !string.IsNullOrEmpty(cb_Sets.Text) ? cb_Sets.Text : string.Empty;
			var action = cb_Actions.SelectedItem != null ? cb_Actions.SelectedItem.ToString() : !string.IsNullOrEmpty(cb_Actions.Text) ? cb_Actions.Text : string.Empty;

			if (string.IsNullOrEmpty(set) || string.IsNullOrEmpty(action))
				return;

			if (ModifiedPrioritySets.ContainsKey(set) && ModifiedPrioritySets[set].ContainsKey(action))
			{
				ModifiedPrioritySets[set][action] = (int)num_Value.Value;
			}
			else if (ModifiedPrioritySets.ContainsKey(set) && !ModifiedPrioritySets[set].ContainsKey(action))
			{
				ModifiedPrioritySets[set].Add(action, (int)num_Value.Value);
				SetupActionsCombobox();
				cb_Actions.SelectedItem = action;
			}
		}

		private void Button_Save_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
