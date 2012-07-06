using System;
using System.Text;
using System.Windows;
using Email;
using Email.WPF;
using Translator;
using Translator.WPF;
namespace Communication.WPF {
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : ACommunicationWindow {

        public MessageBox(string title, string message, ICommunicationReceiver owner, EmailHandler email)
            : base(owner) {
            InitializeComponent();
            TranslationHelpers.translateWindow(this);
            this.Title = title;
            messageLabel.Content = message;
            this.email = email;
            if (owner != null)
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            else
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        public MessageBox(string title, string message, RequestType type, ICommunicationReceiver owner, EmailHandler email)
            : this(title, message, owner, email) {
            if (type == RequestType.Question) {
                cancelButton.Visibility = System.Windows.Visibility.Visible;
                submitButton.Visibility = System.Windows.Visibility.Collapsed;
                TranslationHelpers.translate(okButton,"Yes");
                TranslationHelpers.translate(cancelButton,"No");
                questionIcon.Visibility = System.Windows.Visibility.Visible;
                exceptionExpander.Visibility = System.Windows.Visibility.Collapsed;
            } else {
                this.DialogResult = false;
                throw new NotImplementedException();
            }
        }


        public MessageBox(string title, string message, Exception e, MessageTypes type, ICommunicationReceiver owner, EmailHandler email)
            : this(title, message, owner, email) {
            switch (type) {
                case MessageTypes.Error:
                    cancelButton.Visibility = System.Windows.Visibility.Collapsed;
                    if (e != null) {
                        exceptionExpander.Visibility = System.Windows.Visibility.Visible;
                        exceptionText.Text = recurseExceptions(e);
                        if (e.GetType() == typeof(CommunicatableException)) {
                            submitButton.Visibility = System.Windows.Visibility.Visible;
                        } else {
                            submitButton.Visibility = System.Windows.Visibility.Visible;
                        }
                    } else {
                        submitButton.Visibility = System.Windows.Visibility.Collapsed;
                        exceptionExpander.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    TranslationHelpers.translate(okButton,"Close");
                    errorIcon.Visibility = System.Windows.Visibility.Visible;
                    break;
                case MessageTypes.Info:
                    cancelButton.Visibility = System.Windows.Visibility.Collapsed;
                    exceptionExpander.Visibility = System.Windows.Visibility.Collapsed;
                    submitButton.Visibility = System.Windows.Visibility.Collapsed;
                    TranslationHelpers.translate(okButton,"OK");
                    infoIcon.Visibility = System.Windows.Visibility.Visible;
                    break;
                case MessageTypes.Warning:
                    cancelButton.Visibility = System.Windows.Visibility.Collapsed;
                    exceptionExpander.Visibility = System.Windows.Visibility.Collapsed;
                    submitButton.Visibility = System.Windows.Visibility.Collapsed;
                    TranslationHelpers.translate(okButton,"OK");
                    warningIcon.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }

        public static string recurseExceptions(Exception e) {
            StringBuilder return_me = new StringBuilder(e.Message);
            return_me.AppendLine();
            return_me.AppendLine();

            return_me.AppendLine(e.StackTrace);
            if (e.InnerException != null) {
                return_me.AppendLine(recurseExceptions(e.InnerException));
                return_me.AppendLine();
            }
            return return_me.ToString();
            ;
        }


        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }

        private void okButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }
        
        private EmailHandler email;
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if (submitButton.Visibility == System.Windows.Visibility.Visible) {
                email.checkAvailability(checkAvailabilityDone);
                TranslationHelpers.translate(submitButton,"CheckingConnection");
            }
        }

        private void checkAvailabilityDone(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) {
            if ((EmailResponse)e.Result == EmailResponse.ServerReachable) {
                submitButton.IsEnabled = true;
                TranslationHelpers.translate(submitButton,"SendReport");
            } else {
                submitButton.IsEnabled = false;
                TranslationHelpers.translate(submitButton,"CantSendReport");
            }

        }

        private void submitButton_Click(object sender, RoutedEventArgs e) {
            string address = Email.WPF.EmailWPFHelper.getEmail(this, null);
            if (address == null)
                return;

            StringBuilder body = new StringBuilder();
            body.AppendLine(this.Title);
            body.AppendLine();
            body.AppendLine(messageLabel.Content.ToString());
            body.AppendLine();
            body.AppendLine(exceptionText.Text);
            body.AppendLine();
            body.AppendLine(Application.Current.Properties.ToString());
            body.AppendLine();
            body.AppendLine();

            submitButton.IsEnabled = false;
            TranslationHelpers.translate(submitButton,"SendingReport");
            email.sendEmail("MASGAU Error - " + this.Title, body.ToString(), sendEmailDone);
        }

        private void sendEmailDone(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) {
            if (e.Error != null) {
                TranslationHelpers.translate(submitButton,"SendFailed");
                displayError("Error time", e.Error.Message);
            } else {
                TranslationHelpers.translate(submitButton,"ReportSent");
            }
            submitButton.IsEnabled = false;
        }
    }
}
