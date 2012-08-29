using System;
using System.Windows.Input;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Controls;
namespace MVC.WPF {
    /// <summary>
    /// Interaction logic for ModelListView.xaml
    /// </summary>
    public partial class ModelListView : UserControl {
        public ModelListView() {
            InitializeComponent();
        }

        public event SelectionChangedEventHandler SelectionChanged;

        public ModelListViewItem TemplateItem = new ModelListViewItem();

        protected INotifyingCollection _model;
        public INotifyingCollection Model {
            get {
                return _model;
            }
            set {
                if (_model != null) {
                    _model.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_model_CollectionChanged);
                    _model.ItemPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(_model_ItemPropertyChanged);
                    _model.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(_model_PropertyChanged);
                }
                _model = value;

                refreshFromModel();

                _model.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_model_CollectionChanged);
                _model.ItemPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_model_ItemPropertyChanged);
                _model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_model_PropertyChanged);
            }
        }


        private void refreshFromModel() {
            ItemStack.Children.Clear();
            foreach (IModelItem item in _model.GenericList) {
                addItem(item, -1);
            }
        }

        private void addItems(IList items, int index) {
            foreach (IModelItem item in items) {
                addItem(item, index);
            }
        }
        private void addItem(IModelItem item, int index) {
            ModelListViewItem view_item = TemplateItem.CreateItem();
            view_item.DataSource = item;
            view_item.Parent = this;
            ItemStack.Children.Add(view_item);
        }

        void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            throw new NotImplementedException();
        }

        void _model_ItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //throw new NotImplementedException();
        }

        void _model_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch(e.Action) {
                case NotifyCollectionChangedAction.Reset:
                    refreshFromModel();
                    break;
                case NotifyCollectionChangedAction.Add:
                    addItems(e.NewItems, e.NewStartingIndex);
                    break;
                default:
                throw new NotImplementedException();
            }
        }

        private void UserControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            throw new NotImplementedException();

        }

        private ModelListViewItem last_clicked = null;

        public void ItemWasClicked(ModelListViewItem clicked) {
            lock (ItemStack.Children) {
                bool set_to = false;
                switch (Keyboard.Modifiers) {
                    case ModifierKeys.Control:
                        clicked.IsSelected = !clicked.IsSelected;
                        if(clicked.IsSelected)
                            last_clicked = clicked;
                        break;
                    case ModifierKeys.Shift:
                        foreach (ModelListViewItem item in ItemStack.Children) {
                            if (item == last_clicked) {
                                item.IsSelected = true;
                                set_to = !set_to;
                            } else   if (item == clicked) {
                                item.IsSelected = true;


                                set_to = !set_to;

                            } else {
                                item.IsSelected = set_to;
                            }
                        }
                        break;
                    default:
                        foreach (ModelListViewItem item in ItemStack.Children) {
                            item.IsSelected = item == clicked;
                        }
                        last_clicked = clicked;
                        break;
                }
            }
        }


    }
}
