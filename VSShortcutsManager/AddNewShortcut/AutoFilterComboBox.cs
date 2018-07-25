using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CustomComboBox 
{
  /// <summary>
  /// ComboBox with filtering capabilities.
  /// </summary>
  /// <remarks>
  /// Based on Omer van Kloeten's .NET Zen blog post (http://weblogs.asp.net/okloeten/archive/2007/11/12/5088649.aspx).
  /// 
  /// Changes:
  /// 2011-04-20    Add keyboard support when filtering
  /// 
  /// 
  /// License
  /// 
  /// Copyright (c) 2011, Ido Ran
  /// All rights reserved.
  /// 
  /// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
  /// 
  /// * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
  /// 
  /// * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
  /// 
  /// * Neither the name of Ido Ran nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
  /// 
  /// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  ///
  /// </remarks>
  public class AutoFilteredComboBox : ComboBox {
    private int silenceEvents = 0;
    private bool isFilterActive = false;

    /// <summary>
    /// Static ctor to override CombBox properties to allow user to search.
    /// </summary>
    static AutoFilteredComboBox() {
      IsTextSearchEnabledProperty.OverrideMetadata(typeof(AutoFilteredComboBox), new FrameworkPropertyMetadata(true));
      IsEditableProperty.OverrideMetadata(typeof(AutoFilteredComboBox), new FrameworkPropertyMetadata(true));
      DisplayMemberPathProperty.OverrideMetadata(typeof(AutoFilteredComboBox), new FrameworkPropertyMetadata(null, DisplayMemberPathChanged));
    }

    private static void DisplayMemberPathChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
      var self = (AutoFilteredComboBox)o;
      self.CoerceValue(SearchTextPathProperty);
    }

    /// <summary>
    /// Creates a new instance of <see cref="AutoFilteredComboBox" />.
    /// </summary>
    public AutoFilteredComboBox() {
      DependencyPropertyDescriptor textProperty = DependencyPropertyDescriptor.FromProperty(
          ComboBox.TextProperty, typeof(AutoFilteredComboBox));
      textProperty.AddValueChanged(this, this.OnTextChanged);

      this.RegisterIsCaseSensitiveChangeNotification();
    }

    #region IsCaseSensitive Dependency Property
    /// <summary>
    /// The <see cref="DependencyProperty"/> object of the <see cref="IsCaseSensitive" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsCaseSensitiveProperty =
        DependencyProperty.Register("IsCaseSensitive", typeof(bool), typeof(AutoFilteredComboBox), new UIPropertyMetadata(false));

    /// <summary>
    /// Gets or sets the way the combo box treats the case sensitivity of typed text.
    /// </summary>
    /// <value>The way the combo box treats the case sensitivity of typed text.</value>
    [System.ComponentModel.Description("The way the combo box treats the case sensitivity of typed text.")]
    [System.ComponentModel.Category("AutoFiltered ComboBox")]
    [System.ComponentModel.DefaultValue(true)]
    public bool IsCaseSensitive {
      [System.Diagnostics.DebuggerStepThrough]
      get {
        return (bool)this.GetValue(IsCaseSensitiveProperty);
      }
      [System.Diagnostics.DebuggerStepThrough]
      set {
        this.SetValue(IsCaseSensitiveProperty, value);
      }
    }

    protected virtual void OnIsCaseSensitiveChanged(object sender, EventArgs e) {
      if (this.IsCaseSensitive)
        this.IsTextSearchEnabled = false;

      this.RefreshFilter();
    }

    private void RegisterIsCaseSensitiveChangeNotification() {
      System.ComponentModel.DependencyPropertyDescriptor.FromProperty(IsCaseSensitiveProperty, typeof(AutoFilteredComboBox)).AddValueChanged(
          this, this.OnIsCaseSensitiveChanged);
    }
    #endregion

    #region DropDownOnFocus Dependency Property
    /// <summary>
    /// The <see cref="DependencyProperty"/> object of the <see cref="DropDownOnFocus" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty DropDownOnFocusProperty =
        DependencyProperty.Register("DropDownOnFocus", typeof(bool), typeof(AutoFilteredComboBox), new UIPropertyMetadata(true));

    /// <summary>
    /// Gets or sets the way the combo box behaves when it receives focus.
    /// </summary>
    /// <value>The way the combo box behaves when it receives focus.</value>
    [System.ComponentModel.Description("The way the combo box behaves when it receives focus.")]
    [System.ComponentModel.Category("AutoFiltered ComboBox")]
    [System.ComponentModel.DefaultValue(true)]
    public bool DropDownOnFocus {
      [System.Diagnostics.DebuggerStepThrough]
      get {
        return (bool)this.GetValue(DropDownOnFocusProperty);
      }
      [System.Diagnostics.DebuggerStepThrough]
      set {
        this.SetValue(DropDownOnFocusProperty, value);
      }
    }
    #endregion

    #region | Handle selection |

    protected override void OnPreviewKeyDown(KeyEventArgs e) {

      if (!isFilterActive &&
        ((e.Key > Key.A && e.Key < Key.Z) ||
        (e.Key > Key.OemSemicolon && e.Key < Key.Oem102))) {

        // Start activating the filter on the first key stroke.
        isFilterActive = true;
      }
      else if (isFilterActive && e.Key == Key.Return) {
        // When the filter is active and the user press return we handle it
        // by closing the drop-down and select the whole text.
        // If the auto-complete feature of the combo box had select an item this
        // item will be left selected, otherwise the text the user enter will
        // remain in the editable text box.
        // If we do not handle this case the combo box will clear the combo box because
        // during filtering no item is selected in the drop-down so the combo box does
        // not use auto-complete's selected item.
        object selectedValue = SelectedValue;
        ClearFilter();
        e.Handled = true;
        IsDropDownOpen = false;
        EditableTextBox.SelectAll();
        SelectedValue = selectedValue;
        return;
      }

      base.OnPreviewKeyDown(e);
    }

    protected override void OnDropDownClosed(EventArgs e) {
      base.OnDropDownClosed(e);
      ClearFilter();
    }

    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) {
      base.OnLostKeyboardFocus(e);
      ClearFilter();
    }

    /// <summary>
    /// Called when <see cref="ComboBox.ApplyTemplate()"/> is called.
    /// </summary>
    public override void OnApplyTemplate() {
      base.OnApplyTemplate();

      if (EditableTextBox != null) {
        this.EditableTextBox.SelectionChanged += this.EditableTextBox_SelectionChanged; ;
        this.EditableTextBox.PreviewKeyUp += new KeyEventHandler(EditableTextBox_PreviewKeyUp);
      }
    }

    void EditableTextBox_PreviewKeyUp(object sender, KeyEventArgs e) {
      if (isFilterActive) {
        if (e.Key == Key.Down) {
          isFilterActive = false;
          e.Handled = true;
          SelectedIndex = -1;
          SelectedIndex = 0;
        }
        else if (e.Key == Key.Up) {
          isFilterActive = false;
          e.Handled = true;
          SelectedIndex = -1;
          SelectedIndex = Items.Count - 1;
        }
      }
    }

    /// <summary>
    /// Gets the text box in charge of the editable portion of the combo box.
    /// </summary>
    protected TextBox EditableTextBox {
      get {
        return ((TextBox)Template.FindName("PART_EditableTextBox", this));
      }
    }

    private int start = 0, length = 0;

    private void EditableTextBox_SelectionChanged(object sender, RoutedEventArgs e) {
      if (this.silenceEvents == 0 && isFilterActive) {
        this.silenceEvents++;
        int newStart = ((TextBox)(e.OriginalSource)).SelectionStart;
        int newLength = ((TextBox)(e.OriginalSource)).SelectionLength;

        if (newStart != start || newLength != length) {
          start = newStart;
          length = newLength;
          this.RefreshFilter();
        }

        this.silenceEvents--;
      }
    }
    #endregion

    #region | Handle focus |
    /// <summary>
    /// Invoked whenever an unhandled <see cref="UIElement.GotFocus" /> event
    /// reaches this element in its route.
    /// </summary>
    /// <param name="e">The <see cref="RoutedEventArgs" /> that contains the event data.</param>
    protected override void OnGotFocus(RoutedEventArgs e) {
      base.OnGotFocus(e);

      if (this.ItemsSource != null && this.DropDownOnFocus) {
        this.IsDropDownOpen = true;
      }
    }
    #endregion

    #region | Handle filtering |


    /// <summary>
    /// Get or set path to a string based property that will be used to search for item.
    /// If this property is not set DisplayMemberPath is used instead.
    /// </summary>
    public string SearchTextPath {
      get { return (string)GetValue(SearchTextPathProperty); }
      set { SetValue(SearchTextPathProperty, value); }
    }

    // Using a DependencyProperty as the backing store for SearchTextPath.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SearchTextPathProperty =
        DependencyProperty.Register("SearchTextPath", typeof(string), typeof(AutoFilteredComboBox), new FrameworkPropertyMetadata(null, null, CoerceSearchTextPath));

    private static object CoerceSearchTextPath(DependencyObject o, object baseValue) {
      var self = (AutoFilteredComboBox)o;
      if (baseValue == null) return self.DisplayMemberPath;
      else return baseValue;
    }

    private void ClearFilter() {
      isFilterActive = false;
      RefreshFilter();
    }

    private void RefreshFilter() {
      if (this.ItemsSource != null) {
        ICollectionView view = CollectionViewSource.GetDefaultView(this.ItemsSource);
        if (view is BindingListCollectionView) {
          BindingListCollectionView blcv = (BindingListCollectionView)view;
          if (blcv.CanCustomFilter) {
            if (string.IsNullOrEmpty(FilterPrefix)) {
              blcv.CustomFilter = string.Empty;
            }
            else {
              // Keep the currently selected item in the combobox.
              object currItem = this.SelectedItem;

              // Change the filter on the view (which is a RepositoryView)
              blcv.CustomFilter = "//" + FilterPrefix;

              // If we have a selected item in the combobox, select it in the view too.
              if (currItem != null) {
                // Reselect the previous item
                blcv.MoveCurrentTo(currItem);
              }
            }
          }
        }
        else {
          view.Refresh();
        }

        //this.IsDropDownOpen = true;
      }
    }

    private bool FilterPredicate(object value) {
      // We don't like nulls.
      if (value == null)
        return false;

      // If there is no text, there's no reason to filter.
      if (this.Text.Length == 0 || !isFilterActive)
        return true;

      string prefix = this.Text;

      // If the end of the text is selected, do not mind it.
      if (this.length > 0 && this.start + this.length == this.Text.Length) {
        prefix = prefix.Substring(0, this.start);
      }

      return GetItemText(value).ToLower().Contains(prefix.ToLower());
    }

    private string FilterPrefix {
      get {
        // If there is no text, there's no reason to filter.
        if (this.Text.Length == 0)
          return string.Empty;

        string prefix = this.Text;

        // If the end of the text is selected, do not mind it.
        if (this.length > 0 && this.start + this.length == this.Text.Length) {
          prefix = prefix.Substring(0, this.start);
        }

        return prefix;
      }
    }

    private string GetItemText(object item) {
      if (string.IsNullOrEmpty(SearchTextPath)) {
        return item.ToString();
      }

      Type t = item.GetType();
      object objValue = t.GetProperty(SearchTextPath).GetValue(item, null);

      string strValue = string.Empty;
      if (objValue != null) strValue = objValue.ToString();

      return strValue;
    }
    #endregion

    /// <summary>
    /// Called when the source of an item in a selector changes.
    /// </summary>
    /// <param name="oldValue">Old value of the source.</param>
    /// <param name="newValue">New value of the source.</param>
    protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue) {
      if (newValue != null) {
        ICollectionView view = CollectionViewSource.GetDefaultView(newValue);
        if (!(view is BindingListCollectionView)) {
          view.Filter += this.FilterPredicate;
        }
      }

      if (oldValue != null) {
        ICollectionView view = CollectionViewSource.GetDefaultView(oldValue);
        if (!(view is BindingListCollectionView)) {
          view.Filter -= this.FilterPredicate;
        }
      }

      base.OnItemsSourceChanged(oldValue, newValue);
    }

    private void OnTextChanged(object sender, EventArgs e) {
      if (!this.IsTextSearchEnabled && this.silenceEvents == 0) {
        this.RefreshFilter();

        // Manually simulate the automatic selection that would have been
        // available if the IsTextSearchEnabled dependency property was set.
        if (this.Text.Length > 0) {
          foreach (object item in CollectionViewSource.GetDefaultView(this.ItemsSource)) {
            string itemText = GetItemText(item);
            int text = itemText.Length, prefix = this.Text.Length;
            this.SelectedItem = item;

            this.silenceEvents++;
            this.EditableTextBox.Text = itemText;
            this.EditableTextBox.Select(prefix, text - prefix);
            this.silenceEvents--;
            break;
          }
        }
      }
    }
  }
}
