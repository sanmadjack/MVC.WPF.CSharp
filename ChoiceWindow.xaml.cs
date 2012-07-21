using System.Collections.Generic;
using System.Windows;
using Translator.WPF;
namespace Communication.WPF {
    /// <summary>
    /// Interaction logic for ChoiceWindow.xaml
    /// </summary>
    public partial class ChoiceWindow : ACommunicationWindow {
        public ChoiceWindow(string title, string message, List<string> options, string default_option, ICommunicationReceiver owner, Config.ASettings settings)
            : base(owner, settings) {
            InitializeComponent();
            TranslationHelpers.translateWindow(this);
            int selected = 0;
            this.Title = title;
            messageGrp.Header = message;
            foreach (string add_me in options) {
                choiceCombo.Items.Add(add_me);
                if (add_me == default_option)
                    choiceCombo.SelectedIndex = selected;
                selected++;
            }
        }


        public string selected_item {
            get {
                return choiceCombo.Text;
            }
        }

        public int selected_index {
            get {
                return choiceCombo.SelectedIndex;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }
    }
}
