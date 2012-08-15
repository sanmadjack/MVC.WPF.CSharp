using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Translator;
using Translator.WPF;
using MVC.Communication;
using MVC.Communication.Interface;
using Email;
using SMJ.WPF.Effects;
namespace MVC.WPF {
    public abstract class ACommunicationWindow : System.Windows.Window, ICommunicationReceiver, ITranslateableWindow {
        public bool isSameContext() {
            return Dispatcher.FromThread(Thread.CurrentThread) != null;
        }

        protected static Brush default_progress_color;
        
        protected SynchronizationContext _context;
        public SynchronizationContext context {
            get {
                return _context;
            }
        }
        protected bool _available = true;
        public bool Available {
            get {
                return _available;
            }
        }


        public void setTranslatedTitle(string name, params string[] variables) {
            this.Title = Strings.GetLabelString(name, variables);
        }


        public ACommunicationWindow() {
            this.Closing += new CancelEventHandler(Window_Closing);
            //These intitialize the contexts of the CommunicationHandlers
            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(this.Dispatcher));
            _context = SynchronizationContext.Current;

            CommunicationHandler.addReceiver(this);
        }

        private IEmailSource email_source;

        protected ACommunicationWindow(ICommunicationReceiver owner, IEmailSource email_source)
            : this() {
            this.Owner = owner as System.Windows.Window;
            this.email_source = email_source;

        }

        #region Interface effectors
        protected Boolean disable_close = false;
        public virtual void disableInterface() {
            disable_close = true;
        }
        public virtual void enableInterface() {
            disable_close = false;
        }
        protected void enableInterface(object sender, RunWorkerCompletedEventArgs e) {
            this.enableInterface();
        }
        public void closeInterface() {
            this.Close();
        }

        protected double timing = 5.0;

        public bool toggleVisibility() {
            if (this.Visibility == System.Windows.Visibility.Visible) {
                FadeEffect fade = new FadeOutEffect(timing);
                fade.Start(this);

               // this.Visibility = System.Windows.Visibility.Hidden;

                return false;
            } else {
                FadeEffect fade = new FadeInEffect(timing);
                fade.Start(this);
                this.Activate();
             //   this.Visibility = System.Windows.Visibility.Visible;

                return true;
            }
        }
        public void hideInterface() {
            this.Visibility = System.Windows.Visibility.Hidden;
        }
        public void showInterface() {
            this.Visibility = System.Windows.Visibility.Visible;
        }

        protected override void OnClosing(CancelEventArgs e) {
            if (disable_close) {
                e.Cancel = true;
            } else {
                base.OnClosing(e);
            }

        }
        void Window_Closing(object sender, CancelEventArgs e) {
            _available = false;
            if(this.Visibility == System.Windows.Visibility.Visible)
                toggleVisibility();

        }


        public void applyProgress(SMJ.WPF.SuperProgressBar progress, ProgressUpdatedEventArgs e) {
            if (e.message != null) {
                progress.Message = e.message;
            }

            progress.Value = e.value;
            progress.Max = e.max;

            progress.IsIndeterminate = e.state == ProgressState.Indeterminate;

            switch (e.state) {
                case ProgressState.Error:
                    progress.State = SMJ.WPF.SuperProgressBarState.Error;
                    break;
                case ProgressState.None:
                case ProgressState.Normal:
                    progress.State = SMJ.WPF.SuperProgressBarState.Normal;
                    break;
                case ProgressState.Wait:
                    progress.State = SMJ.WPF.SuperProgressBarState.Wait;
                    break;
            }

        }

        #endregion

        public ResponseType sendMessage(MessageEventArgs e) {
            ResponseType response = ResponseType.OK;
            switch (e.type) {
                case MessageTypes.Error:
                     displayError(e.title, e.message, e.exception);
                    break;
                case MessageTypes.Info:
                    displayInfo(e.title, e.message);
                    break;
                case MessageTypes.Warning:
                    response = displayWarning(e.title, e.message, e.Suppressable);
                    break;
            }
            e.response = response;
            return response;
        }


        public virtual void requestInformation(RequestEventArgs e) {
            switch (e.info_type) {
                case RequestType.Question:
                    displayQuestion(e);
                    return;
                case RequestType.Choice:
                    ChoiceWindow choice = new ChoiceWindow(e, this);
                    if ((bool)choice.ShowDialog()) {
                        choice.Close();
                        e.result.SelectedIndex = choice.selected_index;
                        e.result.SelectedOption = choice.selected_item;
                        e.response = ResponseType.OK;
                    } else {
                        e.response = ResponseType.Cancel;
                    }
                    return;
                default:
                    throw new NotImplementedException("The specified request type " + e.info_type.ToString() + " is not supported in this GUI toolkit.");
            }
        }

        #region Progress stuff
        public virtual void updateProgress(ProgressUpdatedEventArgs e) {
        }
        protected static void applyProgress(ProgressBar progress, ProgressUpdatedEventArgs e) {
            progress.IsEnabled = e.state != ProgressState.None;
            progress.IsIndeterminate = e.state == ProgressState.Indeterminate;
            switch (e.state) {
                case ProgressState.Normal:
                    progress.Foreground = default_progress_color;
                    break;
                case ProgressState.Error:
                    progress.Foreground = Brushes.Red;
                    break;
                case ProgressState.Wait:
                    progress.Foreground = Brushes.Yellow;
                    break;
            }

            progress.Visibility = System.Windows.Visibility.Visible;
            if (e.max == 0)
                progress.Value = 0;
            else {
                progress.Maximum = e.max;
                progress.Value = e.value;
            }
        }

        #endregion


        #region MessageBox showing things
        public bool displayQuestion(RequestEventArgs e) {
            MessageBox box = new MessageBox(e, this, this.email_source);
            bool result = box.ShowDialog() == true;
            if (result) {
                e.result.SelectedOption = "Yes";
                e.result.SelectedIndex = 1;
                e.response = ResponseType.OK;
            } else {
                e.response = ResponseType.Cancel;
            }
            e.result.Suppressed = box.Suppressed;
            return result;
        }
        public void displayError(string title, string message) {
            displayError(title, message, null);
        }
        public void displayError(string title, string message, Exception e) {
            displayMessage(title, message, MessageTypes.Error, e, false);
        }

        public ResponseType displayWarning(string title, string message, bool suppressable) {
            return displayMessage(title, message, MessageTypes.Warning, null, suppressable);
        }

        public void displayInfo(string title, string message) {
           displayMessage(title, message, MessageTypes.Info, null, false);
        }
        private ResponseType displayMessage(string title, string message, MessageTypes type, Exception e, bool suppressable) {
            MessageBox box = new MessageBox(type, title, message, e, suppressable, this, this.email_source);
            box.ShowDialog();
            if (box.Suppressed)
                return ResponseType.OKSuppressed;
            return ResponseType.OK;

        }
        #endregion

        #region TranslatedMessageBoxes
        public bool askTranslatedQuestion(String string_name, bool suppressable, params string[] variables) {
            StringCollection mes = Strings.getStrings(string_name);
            string title, message;

            if (mes.ContainsKey(StringType.Title))
                title = mes[StringType.Title].interpret(variables);
            else
                title = string_name;

            if (mes.ContainsKey(StringType.Message))
                message = mes[StringType.Message].interpret(variables);
            else
                message = string_name;

            RequestEventArgs e = new RequestEventArgs(RequestType.Question, title, message, null, null, new RequestReply(), suppressable);
            return displayQuestion(e);
        }
        public ResponseType showTranslatedWarning(String string_name, params string[] variables) {
            StringCollection mes = Strings.getStrings(string_name);
            return displayWarning(mes[StringType.Title].interpret(variables),
                mes[StringType.Message].interpret(variables), false);
        }
        public void showTranslatedError(String string_name, params string[] variables) {
            showTranslatedError(string_name, null, variables);
        }
        public void showTranslatedError(String string_name, Exception ex, params string[] variables) {
            StringCollection mes = Strings.getStrings(string_name);
            string title, message;

            if (mes.ContainsKey(StringType.Title))
                title = mes[StringType.Title].interpret(variables);
            else
                title = string_name;

            if (mes.ContainsKey(StringType.Message))
                message = mes[StringType.Message].interpret(variables);
            else
                message = string_name;

            displayError(title,
                message, ex);
        }
        //public static bool showTranslatedInfo(ITranslateableWindow window, String string_name, params string[] variables) {
        //    StringCollection mes = Strings.getStrings(string_name);
        //    return displayInfo(mes[StringType.Title].interpret(variables),
        //        mes[StringType.Message].interpret(variables));
        //}
        #endregion



        #region stuff for interacting with windows.forms controls
        // Ruthlessly stolen from http://stackoverflow.com/questions/315164/how-to-use-a-folderbrowserdialog-from-a-wpf-application
        public System.Windows.Forms.IWin32Window GetIWin32Window() {
            var source = System.Windows.PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);
            return win;
        }

        private class OldWindow : System.Windows.Forms.IWin32Window {
            private readonly System.IntPtr _handle;
            public OldWindow(System.IntPtr handle) {
                _handle = handle;
            }

            #region IWin32Window Members
            System.IntPtr System.Windows.Forms.IWin32Window.Handle {
                get { return _handle; }
            }
            #endregion
        }
        #endregion
    }
}
