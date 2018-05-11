//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Data.Json;
using Windows.UI;
using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using Windows.Foundation.Collections;
using Windows.Foundation;
using Windows.UI.ViewManagement;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Phrasor
{
    public sealed class PhraseNode
    {
        public PhraseNode Parent;
        public string Caption;
        public bool IsCategory;
        public List<PhraseNode> Children;
    }

    public sealed class PhraseNodeComparer : IComparer<PhraseNode>
    {
        public int Compare(PhraseNode a, PhraseNode b)
        {
            if ((a.IsCategory) && (!b.IsCategory))
                return -1;
            if ((!a.IsCategory) && (b.IsCategory))
                return 1;
            return a.Caption.CompareTo(b.Caption); 
        }
    }

    public enum PageMode
    {
        Run,
        Edit,
        Delete
    }

    public sealed class KeyboardPageNavigationParams
    {
        public PhraseNode RootNode;
        public PhraseNode CurrentNode;
        public PhraseNode ChildNode;
        public bool IsCategory;
        public bool NeedsSaving;
        public bool SpeechMode;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string PhraseConfigFile = "PhraseData.phr";
        PhraseNode _rootNode;
        PhraseNode _curNode;
        PhraseNodeComparer _phraseNodeComparer;
        SolidColorBrush _backgroundBrush;
        SolidColorBrush _foregroundBrush;
        SpeechSynthesizer _speechSynthesizer;
        MediaElement _mediaElement;
        PageMode _pageMode;
        bool _interactionPaused;
        KeyboardPageNavigationParams _navParams;
        int _curPageIndex;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;

            //var sharedSettings = new ValueSet();
            //GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) => {
            //    var gazePointer = GazeInput.GetGazePointer(this);
            //    gazePointer.LoadSettings(sharedSettings);
            //});

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;

            SetPageMode(PageMode.Run);            
            _speechSynthesizer = new SpeechSynthesizer();
            _mediaElement = new MediaElement();
            _backgroundBrush = new SolidColorBrush(Colors.Gray);
            _foregroundBrush = new SolidColorBrush(Colors.Blue);
            _phraseNodeComparer = new PhraseNodeComparer();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_navParams != null)
            {
                _rootNode = _navParams.RootNode;
                if (_navParams.NeedsSaving)
                {
                    SaveConfigFile(PhraseConfigFile);
                }
                GotoNode(_navParams.CurrentNode);
                _navParams = null;
            }
            else
            {
                LoadConfigFile(PhraseConfigFile);
            }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var navParams = e.Parameter as KeyboardPageNavigationParams;
            _navParams = navParams;
        }

        private JsonObject SavePhraseNode(PhraseNode node)
        {
            var jsonObj = new JsonObject();
            jsonObj.Add("caption", JsonValue.CreateStringValue(node.Caption));
            jsonObj.Add("isCategory", JsonValue.CreateBooleanValue(node.IsCategory));

            var items = new JsonArray();
            jsonObj.Add("items", items);

            if (node.Children == null)
            {
                return jsonObj;
            }

            foreach (var childNode in node.Children)
            {
                items.Add(SavePhraseNode(childNode));
            }
            return jsonObj;
        }

        private PhraseNode LoadPhraseNode(IJsonValue jsonValue, PhraseNode parent)
        {
            var jsonObj = jsonValue.GetObject();
            var caption = jsonObj.GetNamedString("caption");
            var items = jsonObj.ContainsKey("items") ? jsonObj.GetNamedArray("items") : null;
            var isCategory = jsonObj.ContainsKey("isCategory") ? jsonObj.GetNamedBoolean("isCategory") : (items != null) && (items.Count > 0);

            var phraseNode = new PhraseNode
            {
                Caption = caption,
                Parent = parent,
                IsCategory = isCategory,
                Children = items != null ? new List<PhraseNode>(items.Count) : null
            };

            if (items == null)
            {
                return phraseNode;
            }

            foreach (var item in items)
            {
                var childNode = LoadPhraseNode(item, phraseNode);
                phraseNode.Children.Add(childNode);
            }
            phraseNode.Children.Sort(_phraseNodeComparer);
            return phraseNode;
        }

        private async void LoadConfigFile(string filename)
        {
            var folder = ApplicationData.Current.LocalFolder;

            var file = await folder.TryGetItemAsync(PhraseConfigFile) as StorageFile;

            if (file == null)
            {
                var configFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///" + PhraseConfigFile));
                file = await configFile.CopyAsync(folder);
            }

            var text = await FileIO.ReadTextAsync(file);
            var jsonRoot = JsonValue.Parse(text);
            _rootNode = LoadPhraseNode(jsonRoot, null);
            GotoNode(_rootNode);
        }

        private async void SaveConfigFile(string fileName)
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync(PhraseConfigFile, CreationCollisionOption.ReplaceExisting);
            var jsonObject = SavePhraseNode(_rootNode);
            await FileIO.WriteTextAsync(file, jsonObject.Stringify());
        }

        private Button AddButtonToPhraseGrid(Object content, Object tag, int row, int col, bool isCategory, bool navButtons)
        {
            var button = new Button();
            button.Content = isCategory ? content.ToString().ToUpper() : content;
            button.Background = _backgroundBrush;
            button.Foreground = _foregroundBrush;
            button.VerticalAlignment = VerticalAlignment.Stretch;
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.Click += OnGridButtonClick;
            button.Tag = tag;
            button.Style = navButtons ? Resources["ToolbarButton"] as Style : Resources["PhraseButton"] as Style;

            Grid.SetColumn(button, col);
            Grid.SetRow(button, row);
            PhraseGrid.Children.Add(button);
            return button;
        }

        private void RecreateGrid(int rows, int cols)
        {
            // clear the current grid of all buttons
            PhraseGrid.Children.Clear();
            PhraseGrid.RowDefinitions.Clear();
            PhraseGrid.ColumnDefinitions.Clear();
            for (int row = 0; row < rows; row++)
            {
                PhraseGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int col = 0; col < cols; col++)
            {
                PhraseGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

        }

        private void GotoNode(PhraseNode phraseNode)
        {
            int rows;
            int cols;

            _curNode = phraseNode;

            int numButtons = phraseNode.Children.Count;
            int buttonsPerPage = GetNumRowColumns(numButtons, out rows, out cols);

            RecreateGrid(rows, cols);

            int numPages = 1;
            bool addPrevPageButton = false;
            bool addNextPageButton = false;
            int curButtonIndex = 0;
            if (numButtons > buttonsPerPage)
            {
                // all pages have a prev and next button, except the first and last
                // pages, which only have either a next and prev button respectively
                numPages += (numButtons - 3) / (buttonsPerPage - 2);

                addPrevPageButton = _curPageIndex > 0;
                addNextPageButton = _curPageIndex != numPages - 1;

                curButtonIndex = _curPageIndex * (buttonsPerPage - 1);

                // if only the prev or next button are going to be added, remove one button, else remove two buttons
                buttonsPerPage = (addPrevPageButton ^ addNextPageButton) ? buttonsPerPage - 1 : buttonsPerPage - 2;
            }

            int row;
            int col;

            row = col = 0;
            if (addPrevPageButton)
            {
                AddButtonToPhraseGrid("\uE72B", "\uE72B", row, col++, false, true);
            }

            for (row = 0; row < rows; row++)
            {
                for (; col < cols; col++)
                {
                    var caption = phraseNode.Children[curButtonIndex].Caption;
                    var category = phraseNode.Children[curButtonIndex].IsCategory;
                    var tag = phraseNode.Children[curButtonIndex];

                    AddButtonToPhraseGrid(caption, tag, row, col, category, false);

                    curButtonIndex++;
                    if (curButtonIndex >= numButtons)
                    {
                        return;
                    }

                    buttonsPerPage--;
                    if (buttonsPerPage == 0)
                    {
                        if (addNextPageButton)
                        {
                            AddButtonToPhraseGrid("\uE72A", "\uE72A", row, col + 1, false, true);
                        }
                        return;
                    }
                }
                col = 0;
            }
        }

        private int GetNumRowColumns(int numButtons, out int rows, out int cols)
        {
            // TODO: Read these from settings
            var maxButtonsPerRow = 4;
            var maxButtonsPerCol = 4;

            var maxButtons = maxButtonsPerRow * maxButtonsPerCol;

            if (numButtons < (int)maxButtons)
            {
                rows = (int)Math.Sqrt(numButtons);
                cols = rows;
                while (rows * cols < numButtons)
                {
                    cols++;
                }
            }
            else
            {
                numButtons = (int)maxButtons;
                rows = (int)maxButtonsPerRow;
                cols = (int)maxButtonsPerCol;
            }

            return numButtons;
        }

        private void OnHomeClick(object sender, RoutedEventArgs e)
        {
            _curPageIndex = 0;
            GotoNode(_rootNode);
        }

        private void OnUpClick(object sender, RoutedEventArgs e)
        {
            if (_curNode.Parent != null)
            {
                _curPageIndex = 0; // TODO: Save current page index
                GotoNode(_curNode.Parent);
            }
        }

        private void AddEntry(bool isCategory)
        {
            var navParams = new KeyboardPageNavigationParams
            {
                RootNode = _rootNode,
                CurrentNode = _curNode,
                ChildNode = null,
                NeedsSaving = false,
                IsCategory = isCategory               
            };
            Frame.Navigate(typeof(KeyboardPage), navParams);
        }

        private void OnAddCategoryClick(object sender, RoutedEventArgs e)
        {
            AddEntry(true);
        }

        private void OnAddPhraseClick(object sender, RoutedEventArgs e)
        {
            AddEntry(false);
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            if (_pageMode != PageMode.Edit)
            {
                SetPageMode(PageMode.Edit);
            }
            else
            {
                SetPageMode(PageMode.Run);
            }
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (_pageMode != PageMode.Delete)
            {
                SetPageMode( PageMode.Delete);                
            }
            else
            {                
                SetPageMode(PageMode.Run);
            }            
        }

        private void SetPageMode(PageMode newPageMode)
        {
            DeleteButton.Background = Resources["ToolBarBackgroundBrush"] as SolidColorBrush;
            DeleteButton.Foreground = Resources["ToolBarForegroundBrush"] as SolidColorBrush;
            EditButton.Background = Resources["ToolBarBackgroundBrush"] as SolidColorBrush;
            EditButton.Foreground = Resources["ToolBarForegroundBrush"] as SolidColorBrush;

            if (newPageMode == PageMode.Delete)
            {
                DeleteButton.Background = Resources["ToolBarForegroundBrush"] as SolidColorBrush;
                DeleteButton.Foreground = Resources["ToolBarBackgroundBrush"] as SolidColorBrush;
                SetButtonDwellResponse(true);                
            }
            else if (newPageMode == PageMode.Edit)
            {
                EditButton.Background = Resources["ToolBarForegroundBrush"] as SolidColorBrush;
                EditButton.Foreground = Resources["ToolBarBackgroundBrush"] as SolidColorBrush;
                SetButtonDwellResponse(false);
            }
            else
            {
                SetButtonDwellResponse(false);
            }
            
            _pageMode = newPageMode;           
        }

        private void SetButtonDwellResponse(bool toDestructive)
        {
            TimeSpan destructiveDwellDuration = new TimeSpan(0, 0, 0, 0, 1500);
            TimeSpan normalDwellDuration = new TimeSpan(0, 0, 0, 0, 400);
            TimeSpan targetDwellDuration = normalDwellDuration;

            if (toDestructive)
            {
                targetDwellDuration = destructiveDwellDuration;
            }            
            foreach (Button childButton in PhraseGrid.Children)
            {
                childButton.SetValue(GazeInput.DwellDurationProperty, targetDwellDuration);
            }
        }

        private async void OnGridButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var phraseNode = button.Tag as PhraseNode;
            var caption = phraseNode != null ? phraseNode.Caption : button.Content.ToString();

            if (caption == "\uE72B")
            {
                _curPageIndex--;
                GotoNode(_curNode);
                return;
            }
            else if (caption == "\uE72A")
            {
                _curPageIndex++;
                GotoNode(_curNode);
                return;
            }

            switch (_pageMode)
            {
                case PageMode.Delete:
                    if ((phraseNode.IsCategory) && (phraseNode.Children.Count > 0))
                    {
                        // TODO: Add status info somewhere.
                        return;
                    }
                    SetButtonDwellResponse(false);
                    _curNode.Children.Remove(phraseNode);
                    PhraseGrid.Children.Remove(button);
                    SetPageMode(PageMode.Run);
                    GotoNode(_curNode);
                    break;

                case PageMode.Edit:
                    var navParams = new KeyboardPageNavigationParams
                    {
                        RootNode = _rootNode,
                        CurrentNode = _curNode,
                        ChildNode = phraseNode,
                        NeedsSaving = false
                    };                    
                    Frame.Navigate(typeof(KeyboardPage), navParams);
                    break;

                default:
                    if (phraseNode.IsCategory)
                    {
                        GotoNode(phraseNode);
                    }
                    else
                    {
                        var stream = await _speechSynthesizer.SynthesizeTextToStreamAsync(caption);
                        _mediaElement.SetSource(stream, stream.ContentType);
                        _mediaElement.AutoPlay = true;
                        _mediaElement.Play();
                    }
                    break;
            }

        }

        private void OnPauseClick(object sender, RoutedEventArgs e)
        {
            if (_interactionPaused)
            {
                this.SetValue(GazeInput.InteractionProperty, Interaction.Enabled);
                _interactionPaused = false;
                (sender as Button).Content = "\uE769";
                PauseIndicator1.Visibility = Visibility.Collapsed;
                PauseIndicator2.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.SetValue(GazeInput.InteractionProperty, Interaction.Disabled);
                _interactionPaused = true;
                (sender as Button).Content = "\uE768";
                PauseIndicator1.Visibility = Visibility.Visible;
                PauseIndicator2.Visibility = Visibility.Visible;
            }

        }

        private void OnSpeechClick(object sender, RoutedEventArgs e)
        {
            var navParams = new KeyboardPageNavigationParams
            {
                RootNode = _rootNode,
                CurrentNode = _curNode,
                ChildNode = null,
                NeedsSaving = false,
                SpeechMode = true
            };
            Frame.Navigate(typeof(KeyboardPage), navParams);
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
