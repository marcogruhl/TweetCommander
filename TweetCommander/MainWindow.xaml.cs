using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TweetCommander.ViewModels;

namespace TweetCommander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();
            InitializeComponent();
        }

        private void BearerToken_KeyEnterUpdate(object sender, KeyEventArgs e)
        {
            UpdateSourceOnEnter(sender, e);
        }

        private static void UpdateSourceOnEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tBox = (TextBox)sender;
                DependencyProperty prop = TextBox.TextProperty;

                BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                if (binding != null)
                {
                    binding.UpdateSource();
                }
            }
        }
    }
}
