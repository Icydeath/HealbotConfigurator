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
	/// Interaction logic for CustomMessageDialog.xaml
	/// </summary>
	public partial class CustomMessageDialog : Window
	{
		public CustomMessageDialog(string title, string defaultMsg = "")
		{
			InitializeComponent();
			Title = title;
			Label_Message.Content = !string.IsNullOrEmpty(defaultMsg) ? defaultMsg : "Error: Unable to render the error message.";
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Left = Application.Current.MainWindow.Left + 100;
			Top = Application.Current.MainWindow.Top + 100;
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			Button_OK.Focus();
		}
	}
}
