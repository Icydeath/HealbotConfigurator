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
	/// Interaction logic for CustomInputDialog.xaml
	/// </summary>
	public partial class CustomInputDialog : Window
	{
		public CustomInputDialog(string title, string value = "")
		{
			InitializeComponent();
			Title = title;
			Text_CustomInput.Text = value;
		}

		private void Button_CustomInput_OK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			Text_CustomInput.SelectAll();
			Text_CustomInput.Focus();
		}

		public string DialogValue
		{
			get { return Text_CustomInput.Text; }
		}
	}
}
