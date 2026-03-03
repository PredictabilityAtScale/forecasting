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
using System.Xml.Linq;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit;
using System.Collections.ObjectModel;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Folding;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.Text.RegularExpressions;
using FocusedObjective.Contract.Data;
using System.Reflection;

namespace FocusedObjective.KanbanSim
{
    /// <summary>
    /// Interaction logic for ModelEditorUserControl.xaml
    /// </summary>
    public partial class ModelEditorUserControl : UserControl
    {
        private ObservableCollection<SimMLError> _errors = new ObservableCollection<SimMLError>();
        private ObservableCollection<SimMLError> _warnings = new ObservableCollection<SimMLError>();
        
        private FoldingManager foldingManager;
        private XmlFoldingStrategy foldingStrategy;

        private FoldingManager foldingManagerResults;
        private XmlFoldingStrategy foldingStrategyResults;

        private string _lastValidated = string.Empty;
        private XElement _lastValidateErrors = null;

        public ModelEditorUserControl()
        {
            InitializeComponent();

            setupEditor();
        }

        internal ProjectState State { get; set; }

        private void setupEditor()
        {
            /*
            textEditorModel.TextArea.TextView.BackgroundRenderers.Add(
                new HighlightCurrentLineBackgroundRenderer(textEditorModel));

            textEditorModel.TextArea.Caret.PositionChanged += (sender, e) =>
                textEditorModel.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
            */

            // add folding ...
            foldingStrategy = new XmlFoldingStrategy();
            textEditorModel.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();

            if (foldingStrategy != null)
            {
                if (foldingManager == null)
                    foldingManager = FoldingManager.Install(textEditorModel.TextArea);
                
                foldingStrategy.UpdateFoldings(foldingManager, textEditorModel.Document);
            }
            else
            {
                if (foldingManager != null)
                {
                    FoldingManager.Uninstall(foldingManager);
                    foldingManager = null;
                }
            }

            // folding for results viewer
            foldingStrategyResults = new XmlFoldingStrategy();
            textEditorResults.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();

            if (foldingStrategyResults != null)
            {
                if (foldingManagerResults == null)
                    foldingManagerResults = FoldingManager.Install(textEditorResults.TextArea);
                foldingStrategyResults.UpdateFoldings(foldingManagerResults, textEditorResults.Document);
            }
            else
            {
                if (foldingManagerResults != null)
                {
                    FoldingManager.Uninstall(foldingManagerResults);
                    foldingManagerResults = null;
                }
            }

            DispatcherTimer foldingUpdateTimer = new DispatcherTimer();
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(4);
            foldingUpdateTimer.Tick -= foldingUpdateTimer_Tick;
            foldingUpdateTimer.Tick += foldingUpdateTimer_Tick;
            foldingUpdateTimer.Start();

            // error timer and validation...
            DispatcherTimer errorListUpdateTimer = new DispatcherTimer();
            errorListUpdateTimer.Interval = TimeSpan.FromSeconds(7);
            errorListUpdateTimer.Tick -= errorListUpdateTimer_Tick;
            errorListUpdateTimer.Tick += errorListUpdateTimer_Tick;
            errorListUpdateTimer.Start();


            buildCompletionLists();

            textEditorModel.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            textEditorModel.TextArea.TextEntered += textEditor_TextArea_TextEntered;

            FindPanel.TextEditorControl = textEditorModel;
        }

        CompletionWindow completionWindow;

        void foldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (this != null && this.State != null)
            {
                if (!this.State.AlreadySimulating)
                {
                    if (foldingStrategy != null)
                        foldingStrategy.UpdateFoldings(foldingManager, textEditorModel.Document);

                    if (foldingStrategyResults != null)
                        foldingStrategyResults.UpdateFoldings(foldingManagerResults, textEditorResults.Document);
                }
            }
        }

        internal bool CurrentlyValidSimML
        {
            get
            {
                return State.ParseCurrentSimML();
            }
        }

        internal XElement ValidateSimML()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textEditorModel.Text))
                {
                    // don't error an empty file!
                    Errors.Clear();
                    Warnings.Clear();
                    return null;
                }

                //skip if the same as last time
                if (_lastValidated == textEditorModel.Text)
                    return _lastValidateErrors;

                _lastValidated = textEditorModel.Text;


                // get the current simml
                XElement doc;
                try
                {
                    doc = XElement.Parse(textEditorModel.Text, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
                }
                catch (Exception exc)
                {
                    Errors.Clear();
                    Warnings.Clear();
                    Errors.Add(new SimMLError
                    {
                        Severity = SimMLErrorSeverity.Error,
                        Line = 0,
                        Position = 0,
                        ErrorText = string.Format("Model is not validly formatted. Message: {0}", exc.Message)
                    });

                    _lastValidateErrors = null;
                    return _lastValidateErrors;
                }

                // fix if it is a partial model
                //TODO: Support the partial model format...
                // make the execute section pass ALWAYS

                // create a contract
                Simulation.Simulator sim = new Simulation.Simulator(textEditorModel.Text);

                // call validate
                bool passed = sim.Execute(true);


                _lastValidateErrors = sim.Errors;
                return _lastValidateErrors;
            }
            catch
            {
                // we never want this to stop progress!!!
            }


            _lastValidateErrors = null;
            return _lastValidateErrors;
        }

        void errorListUpdateTimer_Tick(object sender, EventArgs e)
        {
            XElement errors = ValidateSimML();

            if (errors != null)
            {
                lock (this)
                {
                    SetErrors(errors);
                }
            }

            tabItemErrors.Header = string.Format("Errors ({0})", _errors.Count);
            if (_errors.Count > 0)
            {
                tabItemErrors.Background = Brushes.Red;
            }
            else
            {
                tabItemErrors.Background = TabItemModel.Background; // use the tab next to this one!
            }

        }

        internal string CurrentSimML
        {
            get
            {
                return textEditorModel.Text;
            }
            set
            {
                textEditorModel.Text = value;
            }
        }

        internal string CurrentResults
        {
            get
            {
                return textEditorResults.Text;
            }
            set
            {
                textEditorResults.Text = value;
            }
        }
        
        internal void UpdateFileNameAndStatus()
        {
            this.TabItemModel.Header =
                string.Format("Model - {0}{1}",
                System.IO.Path.GetFileNameWithoutExtension(State.CurrentFileName),
                State.HasChangedSinceLastSave ? " (unsaved)" : "");
        }

        internal void SetErrors(XElement errors)
        {
            _errors.Clear();
            _warnings.Clear();

            if (errors != null)
            {
                foreach (var e in
                    errors.Descendants()
                    .Where(error => error.Name == "error")
                    .OrderBy(error => int.Parse(error.Attribute("line").Value)))
                {
                    int line = int.Parse(e.Attribute("line").Value);
                    int pos = int.Parse(e.Attribute("pos").Value);

                    _errors.Add(new SimMLError(textEditorModel, textEditorModel.Document.GetOffset(line, pos))
                    {
                        ErrorText = e.Value,
                        Line = line,
                        Position = pos,
                        Severity = SimMLErrorSeverity.Error
                    });
                }

                //warnings and info
                foreach (var e in
                    errors.Descendants()
                    .Where(error => error.Name == "warning" || error.Name == "information")
                    .OrderBy(error => error.Attribute("line") != null ? int.Parse(error.Attribute("line").Value) : 0))
                {
                    int line = e.Attribute("line") != null ? int.Parse(e.Attribute("line").Value) : 1;
                    int pos = e.Attribute("pos") != null ? int.Parse(e.Attribute("pos").Value) : 1;

                    _warnings.Add(new SimMLError(textEditorModel, textEditorModel.Document.GetOffset(line, pos))
                    {
                        ErrorText = e.Value,
                        Line = line,
                        Position = pos,
                        Severity = e.Name == "warning" ? SimMLErrorSeverity.Warning : SimMLErrorSeverity.Information
                    });
                }

            }
        }

        public ObservableCollection<SimMLError> Errors
        {
            get { return _errors; }
        }

        public ObservableCollection<SimMLError> Warnings
        {
            get { return _warnings; }
        }
        
        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //TODO: Get this working. Focus is wrong!

            
            ListBox lb = sender as ListBox;

            if (lb.SelectedItem != null)
            {
                EditorTabControl.SelectedIndex = 0;

                this.textEditorModel.Focus(); 
                Keyboard.Focus(textEditorModel);

                SimMLError error = lb.SelectedItem as SimMLError;
                if (error != null)
                {
                    textEditorModel.UpdateLayout();

                    if (error.Anchor != null)
                    {
                        textEditorModel.ScrollTo(
                            error.Anchor.Line, error.Anchor.Column);

                        textEditorModel.CaretOffset = error.Anchor.Offset;
                    }

                    this.textEditorModel.Focus();
                    textEditorModel.ForceCursor = true;
                }
            }
            

            e.Handled = true;
        }

        private void TabItemModel_GotFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void textEditorModel_DocumentChanged(object sender, EventArgs e)
        {
            
        }

        private void textEditorModel_TextChanged(object sender, EventArgs e)
        {
            UpdateFileNameAndStatus();
        }



        private string getParentElementName (string contentPriorToCaret, out bool insideTag, out string parentOfThisParent)
        {
            string result = "";
            insideTag = true;
            parentOfThisParent = "";

            var matches = Regex.Matches(contentPriorToCaret, @"\<(.+?)([\s\>])", RegexOptions.Singleline);

            if (matches != null && matches.Count > 0)
            {
                string lastCloseElement = "";
                int lastOpenIndex = contentPriorToCaret.Length;

                // hunt backwards
                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    string thisElement = matches[i].Groups[1].Value;
                    string thisElementEndingChar = matches[i].Groups[2].Value;

                    if (thisElement != "!--" && !thisElement.StartsWith("?")) // ignore comment and parameter elements
                    {
                        // hunting back to the open of the previously closed tag. ignore everything in-between
                        if (!string.IsNullOrWhiteSpace(lastCloseElement))
                        {
                            if (thisElement == lastCloseElement)
                                lastCloseElement = "";
                        }
                        else
                        {
                            // not hunting for close, looking for the next previous

                            // if this is a close tag. remember its name
                            if (thisElement.StartsWith(@"/"))
                            {
                                lastCloseElement = thisElement.Substring(1);
                            }
                            else
                            {
                                // is there a /> between us and the open tag, its not our open tage, keep huntin back
                                string sub = contentPriorToCaret.Substring(matches[i].Groups[1].Index, lastOpenIndex - matches[i].Groups[1].Index);
                                if (sub.TrimEnd().EndsWith(@"/>"))
                                {
                                    // not in this tag, its closed out. Go back to prior
                                }
                                else
                                {
                                    // to the left or the right of the most recent >  (left = inside, right means outside)
                                    if (sub.Contains(@">"))
                                        insideTag = false;

                                    result = thisElement;

                                    // get the previous opening parent
                                    bool x;
                                    string y;
                                    parentOfThisParent = getParentElementName(contentPriorToCaret.Substring(0, matches[i].Index), out x, out y);
                                    
                                    

                                    break;
                                }
                            }
                        }
                    }

                    // remember the prior open so we can extract all the text from the start of this back to the prior openeing tag
                    lastOpenIndex = matches[i].Groups[1].Index;
                }
            }

            return result;
        }

        // regex for getting all the elenent content @"\<(.+?)[\>]"


        private Dictionary<string, List<SimMLCompletionData>> elementList = new Dictionary<string, List<SimMLCompletionData>>();
        private Dictionary<string, List<SimMLCompletionData>> attributeList = new Dictionary<string, List<SimMLCompletionData>>();  

        private void buildCompletionLists()
        {
            foreach (Type thisType in typeof(FocusedObjective.Contract.SimulationData).Assembly.GetTypes())
                buildCodeCompleteListForType(thisType);
        }


        private void buildCodeCompleteListForType(Type thisType)
        {
            var result = new List<string>();

            var elList = thisType.GetCustomAttributes(typeof(SimMLElement), true);

            foreach (var element in elList)
            {

                // this type is an element itself.
                string rootElement = ((SimMLElement)element).Name;


                // if this is an element of a sub-collection element (not a specific contract type) add it as a chile of its parent
                if (!string.IsNullOrWhiteSpace(((SimMLElement)element).ParentElement))
                {
                    string keyName = ((SimMLElement)element).ParentParentElement + ((SimMLElement)element).ParentElement;

                    if (elementList.ContainsKey(keyName))
                    {
                        elementList[keyName].AddRange(

                            new List<SimMLCompletionData> {
                            new SimMLCompletionData("</" + ((SimMLElement)element).ParentElement + ">", "Closing element for parent <" + ((SimMLElement)element).ParentElement + "> tag.", false),
                            new SimMLCompletionData("<" + rootElement + " ", ((SimMLElement)element).Description, ((SimMLElement)element).Mandatory)
                            }
                            
                            
                            );
                    }
                    else
                    {

                        elementList.Add(keyName, new List<SimMLCompletionData> {
                            new SimMLCompletionData("</" + ((SimMLElement)element).ParentElement + ">", "Closing element for parent <" + ((SimMLElement)element).ParentElement + "> tag.", false),
                            new SimMLCompletionData("<" + rootElement + " ", ((SimMLElement)element).Description, ((SimMLElement)element).Mandatory)
                        });
                    }
                }


                // add closing element
                List<SimMLCompletionData> subElementList = new List<SimMLCompletionData>();
                subElementList.Add(new SimMLCompletionData("</" + rootElement + ">", "Closing element for parent <" + rootElement + "> tag.", false));

                // do sub elements
                var elements = from p in thisType.GetProperties()
                                let attr = p.GetCustomAttributes(typeof(SimMLElement), true)
                                where attr.Length > 0
                                select new { Property = p, Attribute = attr.First() as SimMLElement };

                foreach (var subElement in elements)
                {
                    if (subElement.Attribute.HasAnyAttributes)
                        subElementList.Add(new SimMLCompletionData("<" + subElement.Attribute.Name + " ", subElement.Attribute.Description, subElement.Attribute.Mandatory));

                    // if no mandatory elements, also add the tag without space for attributes.
                    if (!subElement.Attribute.HasMandatoryAttributes)
                        subElementList.Add(new SimMLCompletionData("<" + subElement.Attribute.Name + ">", subElement.Attribute.Description, subElement.Attribute.Mandatory));

                }

                if (elementList.ContainsKey(((SimMLElement)element).ParentElement + rootElement))
                    elementList[((SimMLElement)element).ParentElement + rootElement].AddRange(subElementList);
                else
                    elementList.Add(((SimMLElement)element).ParentElement + rootElement, subElementList);
                

                // do sub attributes
               List<SimMLCompletionData> subAttributeList = new List<SimMLCompletionData>();

               subAttributeList.Add(new SimMLCompletionData(" />", "Closing element for parent <" + rootElement + "> tag.", false));

               foreach (var prop in thisType.GetProperties())
                   foreach (SimMLAttribute attr in prop.GetCustomAttributes(typeof(SimMLAttribute), true))
                       subAttributeList.Add(new SimMLCompletionData(attr.Name + "=\"", attr.Description, attr.Mandatory));
                   
              attributeList.Add(((SimMLElement)element).ParentElement + rootElement, subAttributeList);
            }
        }

        private List<SimMLCompletionData> buildCodeCompleteListForElement(string parentElementName, string parentOfThisParent, bool insideTag)
        {
            if (insideTag)
            {
                if (attributeList.ContainsKey(parentOfThisParent + parentElementName))
                {
                    // this is a compound key to avoid name collision - parent + element
                    return attributeList[parentOfThisParent + parentElementName];
                }
                else
                {

                    if (!attributeList.ContainsKey(parentElementName))
                        return null;
                    else
                        return attributeList[parentElementName];
                }
            }
            else
            {
                if (elementList.ContainsKey(parentOfThisParent + parentElementName))
                {
                    // this is a compound key to avoid name collision - parent + element
                    return elementList[parentOfThisParent + parentElementName];
                }
                else
                {
                    if (!elementList.ContainsKey(parentElementName))
                        return null;
                    else
                        return elementList[parentElementName];
                }
            }
        }

        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            // define trigger
           
            // not in a element = allowed elements for parent element or root element and close element </...>  - need to find parent element
            // in an element = allowed attributes + close tags  /> or > - need to find parent element and allowed attribute for this element
            // in an element and " = allowed values need to find the allowable values for the element and attribute

            // whilst in beta, ctrl+space used to popup the intellisense.

            if (e.Text == " " && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                bool insideTag;
                string parentOfThisParent = "";
                string parentElementName = getParentElementName(textEditorModel.Text.Substring(0, textEditorModel.CaretOffset), out insideTag, out parentOfThisParent);

                /* debug
                if (!string.IsNullOrWhiteSpace(parentElementName))
                    data.Add(new MyCompletionData(parentElementName + (insideTag ? " attributes" : " elements")));
                */

                if (!string.IsNullOrWhiteSpace(parentElementName))
                {
                    completionWindow = new CompletionWindow(textEditorModel.TextArea);
                    IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;



                    var list = buildCodeCompleteListForElement(parentElementName, parentOfThisParent, insideTag);

                    if (list != null)
                    {


                        foreach (var item in list)
                             data.Add(item);

                        completionWindow.Show();

                        completionWindow.Closed += delegate
                        {
                            completionWindow = null;
                        };
                    }
                }
            }
        }

        void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open, insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }

            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        public void AddTextAtCursor(string text)
        {
            int offset = textEditorModel.CaretOffset;

            textEditorModel.Text = textEditorModel.Text.Insert(offset, text);
            textEditorModel.CaretOffset = offset + text.Length;
        }

        private void textEditorModel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (FindPanel.Visibility == System.Windows.Visibility.Collapsed)
                    FindPanel.Visibility = System.Windows.Visibility.Visible;
                else
                    FindPanel.Visibility = System.Windows.Visibility.Collapsed;

                if (FindPanel.Visibility == System.Windows.Visibility.Visible)
                {
                    FindPanel.Focusable = true;
                    FindPanel.SetFocusToFindBox();
                }
            }
        }
    }

    public class HighlightCurrentLineBackgroundRenderer : IBackgroundRenderer
    {
        private TextEditor _editor;

        public HighlightCurrentLineBackgroundRenderer(TextEditor editor)
        {
            _editor = editor;
        }

        public KnownLayer Layer
        {
            get { return KnownLayer.Caret; }
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (_editor.Document == null || textView.ActualWidth <= 0)
                return;

            textView.EnsureVisualLines();
            var currentLine = _editor.Document.GetLineByOffset(_editor.CaretOffset);
            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
            {
                drawingContext.DrawRectangle(
                    new SolidColorBrush(Color.FromArgb(0x40, 0, 0, 0xFF)), null,
                    new Rect(rect.Location, new Size(textView.ActualWidth - 32, rect.Height)));
            }
        }
    }

    public enum SimMLErrorSeverity
    {
        Information,
        Warning,
        Error
    }

    public class SimMLError
    {
        public SimMLError()
        { }

        public SimMLError(TextEditor editor, int offset)
        {
            _editor = editor;
            _anchor = editor.Document.CreateAnchor(offset);
        }

        private TextEditor _editor;

        private SimMLErrorSeverity _severity;
        private string _errorText;
        private int _line;
        private int _position;
        private TextAnchor _anchor;

        public SimMLErrorSeverity Severity
        {
            get { return _severity; }
            set { _severity = value; }
        }
        
        public string ErrorText
        {
            get { return _errorText; }
            set { _errorText = value; }
        }
         
        public int Line
        {
            get { return _line; }
            set { _line = value; }
        }
        
        public int Position
        {
            get { return _position; }
            set { _position = value; }
        }
        
        public  TextAnchor Anchor
        {
            get { return _anchor; }
        }

        public override string ToString()
        {
            return string.Format("{0} - line:{1}, pos:{2} - {3}",
                Severity.ToString(),
                Line,
                Position,
                ErrorText);
        }
    }

    public class SimMLCompletionData : ICompletionData
    {
        public SimMLCompletionData(string text, string description, bool mandatory)
        {
            this.Text = text;
            this.Description = description;
            this.Mandatory = mandatory;
        }

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        public string Text { get; private set; }
        public bool Mandatory { get; private set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content
        {
            get { return this.Text; }
        }

        public object Description { get; private set; }

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }


        public double Priority
        {
            get { return 1.0; }
        }
    }
}
