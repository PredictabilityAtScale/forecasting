using ICSharpCode.AvalonEdit;
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

namespace FocusedObjective.KanbanSim
{
    /// <summary>
    /// Interaction logic for FindPanelUserControl.xaml
    /// </summary>
    public partial class FindPanelUserControl : UserControl
    {
        public FindPanelUserControl()
        {
            InitializeComponent();
        }

        private void hideButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        private int lastSearchIndex = 0;

        public void Find(string searchQuery)
        {

            if (string.IsNullOrEmpty(searchQuery))
            {
                lastSearchIndex = 0;
                return;
            }

            string editorText = TextEditorControl.Text.ToLowerInvariant();

            if (string.IsNullOrEmpty(editorText))
            {
                lastSearchIndex = 0;
                return;
            }

            if (lastSearchIndex >= editorText.Count())
            {
                lastSearchIndex = 0;
            }


            int nIndex = editorText.IndexOf(searchQuery.ToLowerInvariant(), lastSearchIndex);

            if (nIndex != -1)
            {
                var area = TextEditorControl.TextArea;
                TextEditorControl.Select(nIndex, searchQuery.Length);
                lastSearchIndex = nIndex + searchQuery.Length;

                // count the new lines to get vertical offset and make selection visible...
                int lineNumber = editorText.Substring(0, nIndex).Split('\n').Length -1;

                if (lineNumber <= TextEditorControl.LineCount)
                    TextEditorControl.ScrollToLine(lineNumber);
            }
            else
            {
                lastSearchIndex = 0;
                MessageBox.Show(string.Format("'{0}' not found.", searchQuery));
            }
        }

        public TextEditor TextEditorControl { get; set; }

        public void Replace(string s, string replacement, bool selectedonly)
        {
            int nIndex = -1;

            if (selectedonly)
                nIndex = TextEditorControl.Text.IndexOf(s, TextEditorControl.SelectionStart, TextEditorControl.SelectionLength);
            else
                nIndex = TextEditorControl.Text.IndexOf(s);

            if (nIndex != -1)
            {
                TextEditorControl.Document.Replace(nIndex, s.Length, replacement);
                TextEditorControl.Select(nIndex, replacement.Length);
            }
            else
            {
                lastSearchIndex = 0;
                MessageBox.Show(string.Format("'{0}' Not found.", s));
            }
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (TextEditorControl != null)
                Find(searchText.Text);
        }

        public void SetFocusToFindBox()
        {
            this.searchText.Focusable = true;
            this.searchText.Focus();
            Keyboard.Focus(searchText);
        }

        private void searchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (TextEditorControl != null)
            {
                if (e.Key == Key.Return)
                    Find(searchText.Text);
                else
                {
                    if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        if (this.Visibility == System.Windows.Visibility.Collapsed)
                            this.Visibility = System.Windows.Visibility.Visible;
                        else
                            this.Visibility = System.Windows.Visibility.Collapsed;

                        TextEditorControl.Focus();
                        
                    }
                }
            }
        }
    }
}
