using System.Windows;
using MVC.Communication;
using Translator.WPF;
namespace MVC.WPF {
    /// <summary>
    /// Interaction logic for ChoiceWindow.xaml
    /// </summary>
    public partial class ChoiceWindow : AViewWindow {
        public ChoiceWindow(RequestEventArgs e, AViewWindow owner)
            : base(owner, null) {
            InitializeComponent();
            this.Icon = owner.Icon;
            TranslationHelpers.translateWindow(this);
            int selected = 0;
            this.Title = e.title;
            messageGrp.Header = e.message;
            foreach (string add_me in e.options) {
                choiceCombo.Items.Add(add_me);
                if (add_me == e.default_option)
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
