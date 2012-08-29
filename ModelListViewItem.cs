using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace MVC.WPF {
    public class ModelListViewItem : UserControl {
        public ModelListView Parent;

        private IModelItem _data;
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
        protected Label contentLabel = new Label();

        public ModelListViewItem() {
            this.Margin = new Thickness(1);
            this.Background = BackgroundColor;
            BackgroundRectangle.Fill = BackgroundColor;
            BackgroundRectangle.RadiusX = 5;
            BackgroundRectangle.RadiusY = BackgroundRectangle.RadiusX;

            contentLabel.Background = new SolidColorBrush(Colors.Transparent);

            Grid grid = new Grid();
            grid.Children.Add(BackgroundRectangle);
            grid.Children.Add(contentLabel);

            base.Content = grid;
            this.MouseDown += new MouseButtonEventHandler(UserControl_MouseLeftButtonDown);
        }

        protected virtual void DataSource_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "IsSelected":
                    if (DataSource.IsSelected) {
                        BackgroundRectangle.Fill = SelectedBackgroundColor;
                        changeForegroundColor(SelectedTextColor);
                    } else {
                        BackgroundRectangle.Fill = BackgroundColor;
                        changeForegroundColor(TextColor);
                    }
                    break;
            }
        }

        protected virtual void changeForegroundColor(Brush brush) {
            contentLabel.Foreground = brush;

        }

        protected virtual Brush SelectedTextColor {
            get {
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
            if (Parent != null) {
                Parent.ItemWasClicked(this);
            } else {
                IsSelected = !IsSelected;
            }
        }



        public virtual void LoadSourceData() {
            Content = DataSource;
        }

        protected System.Windows.Media.Color convertColor(System.Drawing.Color color) {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
