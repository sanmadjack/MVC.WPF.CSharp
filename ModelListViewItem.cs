using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace MVC.WPF {
    public class ModelListViewItem : UserControl {
        public ModelListView ParentList;

        protected IModelItem _data;
        public IModelItem DataSource {
            set {
                if (_data != null)
                    _data.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(DataSource_PropertyChanged);
                _data = value;
                LoadSourceData();
                _data.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(DataSource_PropertyChanged);
            }
            get {
                return _data;
            }
        }

        public new object Content {
            get {
                return contentLabel.Content;
            }
            set {
                contentLabel.Content = value;
            }
        }


        protected Rectangle BackgroundRectangle = new Rectangle();
        protected Rectangle Highlight = new Rectangle();
        protected Label contentLabel = new Label();

        public ModelListViewItem() {
            this.Padding = new Thickness(1);
            this.Background = new SolidColorBrush(Colors.Transparent);
            BackgroundRectangle.RadiusX = 5;
            BackgroundRectangle.RadiusY = BackgroundRectangle.RadiusX;

            Highlight.RadiusX = 5;
            Highlight.RadiusY = BackgroundRectangle.RadiusX;
            Highlight.Stroke = Brushes.Black;
            Highlight.StrokeThickness = 1;
            Highlight.Opacity = 0;

            contentLabel.Background = new SolidColorBrush(Colors.Transparent);
            contentLabel.Padding = new Thickness(5);
            contentLabel.Margin = new Thickness(0);
            contentLabel.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            contentLabel.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;

            //            contentLabel.BorderBrush = new SolidColorBrush(Colors.Black);
            //          contentLabel.BorderThickness = new System.Windows.Thickness(1);


            Grid grid = new Grid();


            grid.Children.Add(BackgroundRectangle);
            grid.Children.Add(Highlight);

            grid.Children.Add(contentLabel);

            base.Content = grid;
            this.MouseDown += new MouseButtonEventHandler(UserControl_MouseLeftButtonDown);
            this.MouseEnter += new MouseEventHandler(ModelListViewItem_MouseEnter);
            this.MouseLeave += new MouseEventHandler(ModelListViewItem_MouseLeave);
        }

        void ModelListViewItem_MouseLeave(object sender, MouseEventArgs e) {
            Highlight.Opacity = 0;
        }

        void ModelListViewItem_MouseEnter(object sender, MouseEventArgs e) {
            Highlight.Opacity = 0.5;
        }

        protected void setSelected() {
            if (DataSource.IsSelected) {
                BackgroundRectangle.Fill = SelectedBackgroundColor;
                changeForegroundColor(SelectedTextColor);
            } else {
                BackgroundRectangle.Fill = BackgroundColor;
                changeForegroundColor(TextColor);
            }
        }

        protected virtual void DataSource_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "IsSelected":
                    setSelected();
                    break;
            }
        }

        protected virtual void changeForegroundColor(Brush brush) {
            contentLabel.Foreground = brush;

        }
        protected virtual Brush SelectedTextColor {
            get {
                return new SolidColorBrush(Colors.Black);

                SolidColorBrush brush = SelectedBackgroundColor as SolidColorBrush;
                Color color = brush.Color;

                int total = color.R + color.G + color.B;
                if (total > 400)
                    return new SolidColorBrush(Colors.Black);
                else
                    return new SolidColorBrush(Colors.White);
            }
        }
        protected virtual Brush TextColor {
            get {
                return new SolidColorBrush(Colors.Black);
            }
        }

        protected virtual Brush SelectedBackgroundColor {
            get {
                return new SolidColorBrush(Colors.DarkBlue);
            }
        }

        protected virtual Brush BackgroundColor {
            get {
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public bool IsSelected {
            get {
                return DataSource.IsSelected;
            }
            set {
                DataSource.IsSelected = value;
            }
        }


        public virtual ModelListViewItem CreateItem() {
            return new ModelListViewItem();
        }


        protected void TopLevelGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        }

        protected void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (ParentList != null) {
                ParentList.ItemWasClicked(this);
            } else {
                IsSelected = !IsSelected;
            }
        }



        public virtual void LoadSourceData() {
            BackgroundRectangle.Fill = BackgroundColor;
            this.ToolTip = _data.ToolTip;
            setSelected();
            LoadContent();
        }

        protected virtual void LoadContent() {
            Content = DataSource;

        }

        protected System.Windows.Media.Color convertColor(System.Drawing.Color color) {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
