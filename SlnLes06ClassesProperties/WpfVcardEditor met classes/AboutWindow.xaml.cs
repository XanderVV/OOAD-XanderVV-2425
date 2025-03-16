using System.Windows;

namespace WpfVcardEditor
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void Sluitenbtn_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
