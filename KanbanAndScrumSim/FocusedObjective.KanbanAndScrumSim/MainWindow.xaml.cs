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
using System.Windows.Controls.Ribbon;
using System.Xml.Linq;
using System.Timers;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace FocusedObjective.KanbanSim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        private ProjectState project;
        
        public MainWindow()
        {
            InitializeComponent();

            // Insert code required on object creation below this point.
            const int minTime = 5000; // 5 seconds min to show splash
            Stopwatch timer = new Stopwatch();
            timer.Start();

            project = new ProjectState();

            bindUserControls(project);

            string appFolder = 
                System.IO.Path.GetDirectoryName(
                    System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            if (System.IO.Directory.GetDirectories(appFolder, "??--*").Any())
            {
                Ribbon.Items.Remove(KanbanExamplesTab);
                Ribbon.Items.Remove(ScrumExamplesTab);

                // get all subdirectories orderd alphabetically. Only those starting with 
                foreach (var folder in System.IO.Directory.GetDirectories(appFolder, "??--*").OrderBy(s => s))
                {
                    buildRibbonTab(folder);
                }
            }
            else
            {
                // old method for folders.
                buildExamplesRibbon(KanbanExamplesTab,
                    System.IO.Path.Combine(appFolder, "Examples", "Kanban"));

                buildExamplesRibbon(ScrumExamplesTab,
                     System.IO.Path.Combine(appFolder, "Examples", "Scrum"));

            }

            // always keep these menu tabs
            buildRibbon(ResourcesTab, "Resources");

            buildRibbon(HelpTab, "Help");

            project.ErrorTextBlock = ErrorBar;

            long toWait = minTime - timer.ElapsedMilliseconds;
            if (toWait > 0)
                System.Threading.Thread.Sleep((int)toWait);

        }

        private void bindUserControls(ProjectState state)
        {
            state.ChartUserControl = chartUserControl;
            state.ModelEditorUserControl = modelEditorUserControl;
            state.ModelEditorUserControl.State = state;
            state.BoardScrollViewer = BoardScroller; 
        }

        internal void UpdateData(bool forceRefresh = false)
        {
            string refreshOption = (string)refreshGallery.SelectedValue;
            string aggregateValueOption = (string)aggregationGallery.SelectedValue;

            if (forceRefresh || refreshOption == "change")
            {
                string s = (string)monteCarloGallery.SelectedValue;
                if (s != null)
                    project.SimulateCurrentSimML(int.Parse(s), aggregateValueOption, forceRefresh);
                else
                    project.SimulateCurrentSimML(0, aggregateValueOption, forceRefresh);
            }

        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            if (project.LoadFileCommand())
                UpdateData(true);
        }

        private void MenuItemOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Choose the base folder that contains the examples and resources you want to open...";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // clear the existing entries
                KanbanExamplesTab.Items.Clear();
                ScrumExamplesTab.Items.Clear();
                ResourcesTab.Items.Clear();

                RibbonGroup licensing = HelpTab.Items[0] as RibbonGroup;
                HelpTab.Items.Clear();
                HelpTab.Items.Add(licensing);

                // load the new resources
                buildExamplesRibbon(KanbanExamplesTab, System.IO.Path.Combine(dialog.SelectedPath, "Examples", "Kanban"));
                buildExamplesRibbon(ScrumExamplesTab, System.IO.Path.Combine(dialog.SelectedPath, "Examples", "Scrum"));
                buildRibbon(ResourcesTab, "Resources", dialog.SelectedPath);
                buildRibbon(HelpTab, "Help", dialog.SelectedPath);
            }

        }

        private void MenuItemExample_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RibbonButton && !string.IsNullOrWhiteSpace(((RibbonButton)sender).Tag.ToString()))
            {
                if (project.LoadExampleFile(((RibbonButton)sender).Tag.ToString()))
                    UpdateData(true);
            }
        }

        private void ResourceItemExample_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RibbonButton && !string.IsNullOrWhiteSpace(((RibbonButton)sender).Tag.ToString()))
                System.Diagnostics.Process.Start(((RibbonButton)sender).Tag.ToString());
        }

        private void SnippetItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RibbonButton && !string.IsNullOrWhiteSpace(((RibbonButton)sender).Tag.ToString()))
            {
                string filename = ((RibbonButton)sender).Tag.ToString();

                if (File.Exists(filename))
                {
                    string content = File.ReadAllText(filename);
                    modelEditorUserControl.AddTextAtCursor(content);

                    // bring model editor to the forefront
                    HomeTab.IsSelected = true;
                    synchronizeButtons(SimulationUserControls.ModelEditor);
                    UpdateData();
                }
            }
        }

        internal enum SimulationUserControls
        {
            BoardViewer,
            Charts,
            ModelEditor
        }

        private void MenuItemViewType_Click(object sender, RoutedEventArgs e)
        {
            RibbonToggleButton button = ((RibbonToggleButton)sender);

            switch (button.Name)
            {
                case "ButtonViewCharts":
                        synchronizeButtons(SimulationUserControls.Charts);
                        UpdateData();
                        break;
                case "ButtonViewSimML":
                        synchronizeButtons(SimulationUserControls.ModelEditor);
                        break;
                default:
                        synchronizeButtons(SimulationUserControls.BoardViewer);
                        UpdateData();
                        break;
            } 
        }

        private void synchronizeButtons(SimulationUserControls selectedControl)
        {
            switch (selectedControl)
            {
                case SimulationUserControls.Charts:
                    {
                        ButtonViewBoard.IsChecked = false;
                        ButtonViewCharts.IsChecked = true;
                        ButtonViewSimML.IsChecked = false;

                        MainTabControl.SelectedIndex = 1;
                        break;
                    }
                case SimulationUserControls.ModelEditor:
                    {
                        ButtonViewBoard.IsChecked = false;
                        ButtonViewCharts.IsChecked = false;
                        ButtonViewSimML.IsChecked = true;

                        MainTabControl.SelectedIndex = 2;
                        break;
                    }
                default:
                    {
                        ButtonViewBoard.IsChecked = true;
                        ButtonViewCharts.IsChecked = false;
                        ButtonViewSimML.IsChecked = false;

                        MainTabControl.SelectedIndex = 0;
                        break;
                    }
            }
        }

        private void buildRibbonTab(string folder)
        {
            if (System.IO.Directory.Exists(folder))
            {

                string name = System.IO.Path.GetFileName(folder);

                // needs to be in the format "##--"
                RibbonTab tab = new RibbonTab();
                tab.Header = name.Remove(0,4);

                buildRibbon(tab, folder);

                // insert at the end
                Ribbon.Items.Insert(Ribbon.Items.Count - 2, tab);

            }
        }

        private void buildExamplesRibbon(RibbonTab tab, string folder)
        {
            if (System.IO.Directory.Exists(folder))
            {
                List<RibbonGroup> groups = new List<RibbonGroup>();

                foreach (var file in System.IO.Directory.GetFiles(folder, "*.simML", System.IO.SearchOption.AllDirectories))
                    buildSimMLFileButton(tab, folder, groups, file);

                tab.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                // hide the examples ribbon group
                tab.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void buildRibbon(RibbonTab tab, string folder, string rootFolder = "")
        {
            try
            {

                string baseFolder =
                    System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName),
                    folder);

                if (rootFolder != "")
                {
                    baseFolder =
                        System.IO.Path.Combine(
                        rootFolder,
                        folder);
                }

                if (System.IO.Directory.Exists(baseFolder))
                {
                    List<RibbonGroup> groups = new List<RibbonGroup>();

                    foreach (var file in System.IO.Directory.GetFiles(baseFolder, "*.*", System.IO.SearchOption.AllDirectories))
                    {
                        // ignore any non-"normal" file
                        if (!System.IO.File.GetAttributes(file).HasFlag(System.IO.FileAttributes.Hidden | FileAttributes.System))
                        {

                            if (System.IO.Path.GetExtension(file).ToLower() == ".simml")
                                buildSimMLFileButton(tab, baseFolder, groups, file);
                            else
                                buildButton(tab, baseFolder, groups, file);
                        }
                    }

                    tab.Visibility = System.Windows.Visibility.Visible;
                }
            }
            catch (Exception e)
            {
                var e1 = e;
                // debugging....
                //MessageBox.Show(string.Format("Exception in build ribbon. Source: {2}. Message: {0}. Stack trace: {1}", e.Message ?? "", e.StackTrace ?? "", e.Source ?? ""));

                //if (e.InnerException != null)
                //    MessageBox.Show(string.Format("Inner Exception in build ribbon. Source: {2}. Message: {0}. Stack trace: {1}", e.InnerException.Message ?? "", e.InnerException.StackTrace ?? "", e.InnerException.Source ?? ""));

            }
        }

        private void buildSimMLFileButton(RibbonTab tab, string folder, List<RibbonGroup> groups, string file)
        {
            XDocument doc = null;
            try
            {
                doc = XDocument.Load(file);

                var sim = doc.Element("simulation");
                if (sim != null)
                {
                    var exampleElement = sim.Element("example");

                    string title = exampleElement != null && exampleElement.Attribute("title") != null
                        ? exampleElement.Attribute("title").Value
                        : System.IO.Path.GetFileNameWithoutExtension(file);

                    string groupString = exampleElement != null && exampleElement.Attribute("group") != null
                        ? exampleElement.Attribute("group").Value
                        : System.IO.Path.GetDirectoryName(file).Split(new char[] { '\\' }).Last();

                    string description = exampleElement != null && exampleElement.Value != null
                        ? exampleElement.Value
                        : "";

                    // create a group if one doesn't exist
                    if (!groups.Any(g => g.Header.ToString() == groupString))
                    {
                        RibbonGroup g = new RibbonGroup();
                        g.Header = groupString;
                        g.LargeImageSource = new BitmapImage(new Uri(@"Images\FolderOpen_32x32_72.png", UriKind.Relative));
                        groups.Add(g);

                        var gsdL = new RibbonGroupSizeDefinition();
                        gsdL.IsCollapsed = true;
                        gsdL.ControlSizeDefinitions.Add(new RibbonControlSizeDefinition { ImageSize = RibbonImageSize.Small, IsLabelVisible = true });
                        gsdL.ControlSizeDefinitions.Add(new RibbonControlSizeDefinition { ImageSize = RibbonImageSize.Small, IsLabelVisible = true });
                        gsdL.ControlSizeDefinitions.Add(new RibbonControlSizeDefinition { ImageSize = RibbonImageSize.Small, IsLabelVisible = true });
                        g.GroupSizeDefinitions.Add(gsdL);

                        tab.Items.Add(g);
                    }

                    RibbonButton button = new RibbonButton();
                    button.Tag = file;
                    button.Label = title;

                    string iconPath = System.IO.Path.Combine(
                        folder,
                        @"example.png");

                    if (!System.IO.File.Exists(iconPath))
                    {
                        button.SmallImageSource = new BitmapImage(new Uri(
                        @"Images\example.png", UriKind.Relative));
                    }
                    else
                    {
                        button.SmallImageSource = new BitmapImage(new Uri(
                        iconPath));
                    }

                    button.ToolTip = description;
                    button.Click += new RoutedEventHandler(MenuItemExample_Click);

                    // checked and created above.
                    RibbonGroup group = groups.First(g => g.Header.ToString() == groupString);
                    group.Items.Add(button);
                }
                else
                {
                    // no simulation tag, treat as snippet
                    buildButton(tab, folder, groups, file, true);
                }
            }
            catch
            {
                // if there are any formatting errors, not valid XML, just add it as a snippet general file...
                buildButton(tab, folder, groups, file, true);
            }
        }

        private void buildButton(RibbonTab tab, string folder, List<RibbonGroup> groups, string file, bool snippet = false)
        {


            // create a group if one doesn't exist
            string groupString = System.IO.Path.GetDirectoryName(file).Split(new char[] { '\\' }).Last();
            if (!groups.Any(g => g.Header.ToString() == groupString))
            {
                RibbonGroup g = new RibbonGroup();
                g.Header = groupString;
                g.LargeImageSource = new BitmapImage(new Uri(@"Images\FolderOpen_32x32_72.png", UriKind.Relative));
                groups.Add(g);

                // make small...
                var gsdL = new RibbonGroupSizeDefinition();
                gsdL.IsCollapsed = true;
                gsdL.ControlSizeDefinitions.Add(new RibbonControlSizeDefinition { ImageSize = RibbonImageSize.Small, IsLabelVisible = true });
                gsdL.ControlSizeDefinitions.Add(new RibbonControlSizeDefinition { ImageSize = RibbonImageSize.Small, IsLabelVisible = true });
                gsdL.ControlSizeDefinitions.Add(new RibbonControlSizeDefinition { ImageSize = RibbonImageSize.Small, IsLabelVisible = true });
                g.GroupSizeDefinitions.Add(gsdL);

                tab.Items.Add(g);
            }

            RibbonButton button = new RibbonButton();
            button.Tag = file;
            button.Label = System.IO.Path.GetFileNameWithoutExtension(file);

            try
            {
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(file);
                var bmp = icon.ToBitmap();

                button.LargeImageSource = ExtractIconFromFile.BitmapToBitmapImage(bmp);
                button.SmallImageSource = ExtractIconFromFile.BitmapToBitmapImage(
                    ExtractIconFromFile.ExtractSmallBitmap(file));
            }
            catch
            {
                // for some reason the extracting icons fails on a mac running parallels.
                // going to use a default example icon instead. Tacky, but allows more to be added....

                string iconPath = System.IO.Path.Combine(
                    folder,
                    @"example.png");

                if (!System.IO.File.Exists(iconPath))
                {
                    button.SmallImageSource = new BitmapImage(new Uri(
                        @"Images\example.png", UriKind.Relative));

                    button.LargeImageSource = new BitmapImage(new Uri(
                        @"Images\example.png", UriKind.Relative));
                }
                else
                {
                    button.SmallImageSource = new BitmapImage(new Uri(iconPath));
                    button.LargeImageSource = new BitmapImage(new Uri(iconPath));
                }

            }


            if (snippet)
            {
                button.Click += new RoutedEventHandler(SnippetItem_Click);
                button.ToolTip = System.IO.File.ReadAllText(file);


            }
            else
            {
                button.Click += new RoutedEventHandler(ResourceItemExample_Click);
            }

            // checked and created above.
            RibbonGroup group = groups.First(g => g.Header.ToString() == groupString);
            group.Items.Add(button);
        }
        
        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            project.SaveFileCommand();
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            project.SaveAsFileCommand();
        }

        private void MenuItemNew_Click(object sender, RoutedEventArgs e)
        {
            if (project.NewFileCommand())
            {
                TextBlock text = new TextBlock()
                {
                    Text = "To begin, choose an example file from the Example ribbon tab."
                };

                text.SetValue(HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center);
                text.SetValue(VerticalAlignmentProperty, System.Windows.VerticalAlignment.Center);

                BoardScroller.Content = text;

                synchronizeButtons(SimulationUserControls.BoardViewer);
            }
        }

        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Confirm unsaved work returns "safeToContinue" so, this needs to be negated for cancel...

            if (project != null)
                e.Cancel = !project.ConfirmUnsavedWork();
            else
                e.Cancel = false;
        }

        private void ErrorBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            project.ClearError();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateData(true); // force a refresh!
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
           // new LicenseWindow(project.LastSimulator).ShowDialog();
        }

        private void buttonUpgradeLicensing_Click(object sender, RoutedEventArgs e)
        {
            // create the hyperlink.
            string link = @"http://www.focusedobjective.com/licensing";

            /*
            if (LicenseSettings != null)
                link = string.Format(@"http://focusedobjective.com/licensing/upgrade{0}", "_from_" + LicenseSettings.CurrentLevel.ToString());
            */

            // launch link
            System.Diagnostics.Process.Start(link);
        }

        private void ButtonForecastDate_Click(object sender, RoutedEventArgs e)
        {
            string aggregateValueOption = (string)aggregationGallery.SelectedValue;

            int monteCarloCycles = 100;
            string s = (string)monteCarloGallery.SelectedValue;
            if (s != null)
                monteCarloCycles = int.Parse(s);


            if (this.project.SimulationType == Contract.SimulationTypeEnum.Kanban)
            {
                ForecastDate dateWindow = new ForecastDate(
                    project,
                    monteCarloCycles,
                    aggregateValueOption);

                dateWindow.ShowDialog();
            }
            else
            {
                ForecastDateScrum dateWindow = new ForecastDateScrum(
                    project,
                    monteCarloCycles,
                    aggregateValueOption);

                dateWindow.ShowDialog();
            }

        }

        private void ButtonSensitivity_Click(object sender, RoutedEventArgs e)
        {
            string aggregateValueOption = (string)aggregationGallery.SelectedValue;

            int monteCarloCycles = 100;
            string s = (string)monteCarloGallery.SelectedValue;
            if (s != null)
                monteCarloCycles = int.Parse(s);

            if (this.project.SimulationType == Contract.SimulationTypeEnum.Kanban)
            {
                Sensitivity sensitivityWindow = new Sensitivity(
                    project,
                    monteCarloCycles,
                    aggregateValueOption);

                sensitivityWindow.ShowDialog();
            }
            else
            {
                SensitivityScrum sensitivityWindow = new SensitivityScrum(
                    project,
                    monteCarloCycles,
                    aggregateValueOption);

                sensitivityWindow.ShowDialog();
            }
        }

        private void ButtonStaff_Click(object sender, RoutedEventArgs e)
        {
            string aggregateValueOption = (string)aggregationGallery.SelectedValue;

            int monteCarloCycles = 100;
            string s = (string)monteCarloGallery.SelectedValue;
            if (s != null)
                monteCarloCycles = int.Parse(s);


            AddStaff addStaffWindow = new AddStaff(
                project,
                monteCarloCycles,
                aggregateValueOption);

            addStaffWindow.ShowDialog();
        }

        private void ButtonStats_Click(object sender, RoutedEventArgs e)
        {

            StatsWindow statsWindow = new StatsWindow();

            statsWindow.ShowDialog();
        }

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                if (System.IO.File.Exists(args[1]))
                {
                    if (project.LoadExampleFile(args[1]))
                        UpdateData(true);
                }
            }

            this.Title = string.Format("{0} v{1}.{2}.{3} - Focused Objective",
                this.Title,
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString(),
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString(),
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build.ToString());
        }

        private void ButtonInteractive_Click(object sender, RoutedEventArgs e)
        {
            string aggregateValueOption = (string)aggregationGallery.SelectedValue;

            int monteCarloCycles = 100;
            string s = (string)monteCarloGallery.SelectedValue;
            if (s != null)
                monteCarloCycles = int.Parse(s);

            InteractiveForecast interactiveWindow = new InteractiveForecast();
            interactiveWindow.SetupFromModel(
                project,
                monteCarloCycles,
                aggregateValueOption);

            interactiveWindow.ShowDialog();
        }

    }

    public class RefreshCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            MainWindow window = parameter as MainWindow;

            if (window != null)
                window.UpdateData(true);
        }
    }
    

}
