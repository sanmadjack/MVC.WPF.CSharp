using System.Windows;
using System.Windows.Controls;
using Email;
using Translator.WPF;
namespace Communication.WPF {
    /// <summary>
    /// Interaction logic for EmailWindow.xaml
    /// </summary>
    public partial class EmailWindow : ACommunicationWindow {
        public EmailWindow(ICommunicationReceiver owner)
            : base(owner) {
            InitializeComponent();
            TranslationHelpers.translateWindow(this);
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }


        private void button2_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;

        }
        public string email {
            get {
                return emailTxt.Text;
            }
        }

        private void emailTxt_TextChanged(object sender, TextChangedEventArgs e) {
            saveBtn.IsEnabled = EmailHandler.validateEmailAddress(emailTxt.Text);
        }
    }
}
