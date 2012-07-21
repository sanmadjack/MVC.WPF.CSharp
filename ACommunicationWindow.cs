using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Translator;
using Translator.WPF;
namespace Communication.WPF {
    public abstract class ACommunicationWindow : System.Windows.Window, ICommunicationReceiver, ITranslateableWindow {
        protected static Brush default_progress_color;
        
        protected SynchronizationContext _context;
        public SynchronizationContext context {
            get {
                return _context;
            }
        }
        protected bool _available = true;
        public bool available {
            get {
                return _available;
            }
        }


        public void setTranslatedTitle(string name, params string[] variables) {
            this.Title = Strings.GetLabelString(name, variables);
        }

        protected Config.ASettings settings;

        public ACommunicationWindow() { }

        protected ACommunicationWindow(ICommunicationReceiver owner, Config.ASettings settings)
            : base() {
            this.Owner = owner as System.Windows.Window;
            this.settings = settings;
            this.Closing += new CancelEventHandler(Window_Closing);

            //These intitialize the contexts of the CommunicationHandlers
            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(this.Dispatcher));
            _context = SynchronizationContext.Current;

            CommunicationHandler.addReceiver(this);
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
        }


        #endregion

        public void sendMessage(MessageEventArgs e) {
            bool response = false;
            switch (e.type) {
                case MessageTypes.Error:
                    response = displayError(e.title, e.message, e.exception);
                    break;
                case MessageTypes.Info:
                    response = displayInfo(e.title, e.message);
                    break;
                case MessageTypes.Warning:
                    response = displayWarning(e.title, e.message);
                    break;
            }
            e.response = ResponseType.OK;
        }


        public virtual void requestInformation(RequestEventArgs e) {
            switch (e.info_type) {
                case RequestType.Question:
                    if (displayQuestion(e.title, e.message)) {
                        e.result.selected_option = "Yes";
                        e.result.selected_index = 1;
                        e.response = ResponseType.OK;
                    } else {
                        e.response = ResponseType.Cancel;
                    }
                    return;
                case RequestType.Choice:
                    ChoiceWindow choice = new ChoiceWindow(e.title, e.message, e.options, e.default_option, this,this.settings);
                    if ((bool)choice.ShowDialog()) {
                        choice.Close();
                        e.result.selected_index = choice.selected_index;
                        e.result.selected_option = choice.selected_item;
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
        public bool displayQuestion(string title, string message) {
            MessageBox box = new MessageBox(title, message, RequestType.Question, this, this.settings);
            return (bool)box.ShowDialog();
        }
        public bool displayError(string title, string message) {
            return displayError(title, message, null);
        }
        public bool displayError(string title, string message, Exception e) {
            return displayMessage(title, message, MessageTypes.Error, e);
        }
        public bool displayWarning(string title, string message) {
            return displayMessage(title, message, MessageTypes.Warning, null);
        }
        public bool displayInfo(string title, string message) {
            return displayMessage(title, message, MessageTypes.Info, null);
        }
        private bool displayMessage(string title, string message, MessageTypes type, Exception e) {
            MessageBox box = new MessageBox(title, message, e, type, this, this.settings);
            return (bool)box.ShowDialog();
        }
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
