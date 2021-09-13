using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Xaml.Behaviors;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Behavior with support for binding to a list of selected items for a <see cref="MultiSelector"/>, such as a <see cref="DataGrid"/>
    /// </summary>
    public class MultiSelectorSelectedItemsBehavior : Behavior<MultiSelector>
    {
        #region Overrides

        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += CustomDataGrid_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= CustomDataGrid_SelectionChanged;
        }

        #endregion

        #region Event Handlers

        private void CustomDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItems = AssociatedObject.SelectedItems;
        }

        #endregion

        #region Dependency Properties

        /// <summary>
        /// The collection of selected items
        /// </summary>
        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(MultiSelectorSelectedItemsBehavior));

        #endregion
    }
}