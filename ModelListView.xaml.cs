using System;
using System.Windows.Input;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
namespace MVC.WPF {
    /// <summary>
    /// Interaction logic for ModelListView.xaml
    /// </summary>
    public partial class ModelListView : UserControl {
        public ModelListView() {
            InitializeComponent();
            SelectedItems = new List<IModelItem>();
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
            SelectedItems.Clear();
            lock (ItemStack.Children) {
                ItemStack.Children.Clear();
            }
            foreach (IModelItem item in _model.GenericList) {
                addItem(item, -1);
            }
        }


        private void removeItems(IList items, int index) {
            foreach (IModelItem item in items) {
                removeItem(item, index);
            }
        }
        private void removeItem(IModelItem item, int index) {
            lock (ItemStack.Children) {
                ModelListViewItem match = null;
                foreach(ModelListViewItem i in ItemStack.Children) {
                    if(i.DataSource==item) {
                        match = i;
                        break;
                    }
                }
                item.IsSelected = false;
                lock (SelectedItems) {
                    if (SelectedItems.Contains(item))
                        SelectedItems.Remove(item);
                }
                if (match != null) {
                    ItemStack.Children.Remove(match);
                }
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
            lock (ItemStack.Children) {
                if(index>-1)
                    ItemStack.Children.Insert(index, view_item);
                else
                    ItemStack.Children.Add(view_item);
            }
            if (item.IsSelected)
                SelectedItems.Add(item);
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
                case NotifyCollectionChangedAction.Remove:
                    removeItems(e.OldItems, e.OldStartingIndex);
                    break;
                default:
                throw new NotImplementedException();
            }
        }

        public void DeselectAll() {
            lock (Model) {
                foreach (IModelItem item in Model.GenericList) {
                    item.IsSelected = false;
                }
            }
            updatedSelectedItems();
        }

        public IModelItem SelectedItem {
            get {
                lock(SelectedItems) {
                    if (SelectedItems.Count != 1)
                        return null;

                    return SelectedItems[0];
                }
            }
        }

        public IList<IModelItem> SelectedItems { get; protected set; }


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
            all_selected = false;
            updatedSelectedItems();
        }

        private void updatedSelectedItems() {
            IList removed = new ArrayList();
            IList added = new ArrayList();

            SelectionChangedEventArgs e = new SelectionChangedEventArgs(SelectionChangedEvent, removed, added);
            lock (SelectedItems) {
                lock (Model) {
                    foreach (IModelItem item in Model.GenericList) {
                        if (item.IsSelected) {
                            if (!SelectedItems.Contains(item)) {
                                SelectedItems.Add(item);
                                added.Add(item);
                            }
                        } else {
                            if (SelectedItems.Contains(item)) {
                                SelectedItems.Remove(item);
                                removed.Add(item);
                            }
                        }
                    }
                }
            }
            if (removed.Count > 0 || added.Count > 0) {
                SelectionChanged(this, e);
            }

        }

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
        "SelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ModelListView));

        bool all_selected = false;

        private void UserControl_KeyDown(object sender, KeyEventArgs e) {
            lock (ItemStack.Children) {
                switch (e.Key) {
                    case Key.A:
                        if (Keyboard.Modifiers == ModifierKeys.Control) {
                            foreach (ModelListViewItem item in ItemStack.Children) {
                                item.IsSelected = !all_selected;
                            }
                            all_selected = !all_selected;
                            updatedSelectedItems();
                        }
                        break;

                }
            }
            e.Handled = true;
        }

    }
}
