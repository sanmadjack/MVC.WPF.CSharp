﻿using System;
using System.Text;
using System.Windows;
using Email;
using Email.WPF;
using Translator;
using Translator.WPF;
using MVC.Communication;

namespace Communication.WPF {
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : ACommunicationWindow {

        public MessageBox(string title, string message, ACommunicationWindow owner, Config.ASettings settings)
            : base(owner,settings) {
                this.Icon = owner.Icon;
            InitializeComponent();
            TranslationHelpers.translateWindow(this);
            this.Title = title;
            messageLabel.Content = message;
            if (owner != null)
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            else
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        public MessageBox(RequestEventArgs e, ACommunicationWindow owner, Config.ASettings settings)
            : this(e.title, e.message, owner, settings) {

            switch(e.info_type) {
                case RequestType.Question:
                    cancelButton.Visibility = System.Windows.Visibility.Visible;
                    submitButton.Visibility = System.Windows.Visibility.Collapsed;
                    TranslationHelpers.translate(okButton,"Yes");
                    TranslationHelpers.translate(cancelButton,"No");
                    questionIcon.Visibility = System.Windows.Visibility.Visible;
                    exceptionExpander.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                default:
                    this.DialogResult = false;
                    throw new NotImplementedException();
            }
        }

        public MessageBox(MessageEventArgs e, ACommunicationWindow owner, Config.ASettings settings)
            : this(e.type, e.title, e.message, e.exception, owner, settings) {

        }
        public MessageBox(MessageTypes type, string title, string message, ACommunicationWindow owner, Config.ASettings settings)
            : this(type, title, message, null, owner, settings) {
        }

        public MessageBox(MessageTypes type, string title, string message, Exception e, ACommunicationWindow owner, Config.ASettings settings)
            : this(title, message, owner, settings) {
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

        public bool Suppressable {
            get {
                return this.Suppress.Visibility == System.Windows.Visibility.Visible;
            }
            set {
                if (value)
                    this.Suppress.Visibility = System.Windows.Visibility.Visible;
                else
                    this.Suppress.Visibility = System.Windows.Visibility.Collapsed;
            }
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
