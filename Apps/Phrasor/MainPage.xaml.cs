//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Data.Json;
using Windows.UI;
using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Phrasor
{

    public enum PageMode
    {
        Run,
        Edit,
        Delete
    }


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string PhraseConfigFile = "PhraseData.phr";
        SolidColorBrush _backgroundBrush;
        SolidColorBrush _foregroundBrush;
        SolidColorBrush _transparentBrush;
        SolidColorBrush _dwellProgressBrush;
        TimeSpan _FixationDefault;
        SpeechSynthesizer _speechSynthesizer;
        MediaElement _mediaElement;
        PageMode _pageMode;
        bool _interactionPaused;
        bool _toolBarToggled = false;
        KeyboardPageNavigationParams _navParams;

        ScrollViewer _tileViewer;

        ViewModel MasterViewModel;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;

            MasterViewModel = new ViewModel();

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
            _transparentBrush = new SolidColorBrush(Colors.Transparent);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _dwellProgressBrush = (SolidColorBrush)GazeInput.DwellFeedbackProgressBrush;
            _FixationDefault = GazeInput.GetFixationDuration(this);

            GazeInput.DwellFeedbackCompleteBrush = new SolidColorBrush(Colors.DimGray);
            GazeInput.DwellFeedbackEnterBrush = new SolidColorBrush(Colors.DimGray);

            GazeInput.SetIsCursorVisible(this, false);

            if (_navParams != null)
            {
                if (_navParams.NeedsSaving)
                {
                    SaveConfigFile(PhraseConfigFile);
                }
                MasterViewModel.GoToNode(_navParams.CurrentNode);
                ShowHideTileNavigation();
                _navParams = null;
            }
            else
            {
                LoadConfigFile(PhraseConfigFile);

                _tileViewer = UIHelper.FindChildControl<ScrollViewer>(grdvwPhrases, "ScrollViewer") as ScrollViewer;
                _tileViewer.ViewChanged += Viewer_ViewChanged;

                this.DataContext = MasterViewModel;
                groupTilesCVS.Source = MasterViewModel.Tiles;
            }            
            if (MasterViewModel.Settings.GazePlusClickMode)
            {                
                GazeInput.SetFixationDuration(this, TimeSpan.FromDays(1));
            }
            else
            {               
                GazeInput.SetFixationDuration(this, _FixationDefault);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var navParams = e.Parameter as KeyboardPageNavigationParams;
            _navParams = navParams;
            SetPageMode(PageMode.Run);
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
            MasterViewModel.RootNode = LoadPhraseNode(jsonRoot, null);
            MasterViewModel.GoToNode(MasterViewModel.RootNode);
        }

        private async void SaveConfigFile(string fileName)
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync(PhraseConfigFile, CreationCollisionOption.ReplaceExisting);
            var jsonObject = SavePhraseNode(MasterViewModel.RootNode);
            await FileIO.WriteTextAsync(file, jsonObject.Stringify());
        }

        private void OnHomeClick(object sender, RoutedEventArgs e)
        {
            MasterViewModel.GoToNode(MasterViewModel.RootNode);
            ShowHideTileNavigation();
        }

        private void OnUpClick(object sender, RoutedEventArgs e)
        {
            if (MasterViewModel.CurrentNode.Parent != null)
            {
                MasterViewModel.GoToNode(MasterViewModel.CurrentNode.Parent);
                ShowHideTileNavigation();
            }
        }

        private void AddEntry(bool isCategory)
        {
            var navParams = new KeyboardPageNavigationParams
            {
                RootNode = MasterViewModel.RootNode,
                CurrentNode = MasterViewModel.CurrentNode,
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
                SetPageMode(PageMode.Delete);
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
                SetToolsToolbarVisible(false);
            }

            _pageMode = newPageMode;
        }

        private void SetButtonDwellResponse(bool toDestructive)
        {
            TimeSpan targetDwellDuration = MasterViewModel.Settings.NormalDwellDuration;

            if (toDestructive)
            {
                targetDwellDuration = MasterViewModel.Settings.DestructiveDwellDuration;
            }

            grdvwPhrases.SetValue(GazeInput.DwellDurationProperty, targetDwellDuration);
        }

        private async void OnGridButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var phraseNode = button.DataContext as PhraseNode;
            var caption = phraseNode != null ? phraseNode.Caption : button.Content.ToString();

            if (caption == "\uE72B")
            {
                MasterViewModel.GoToNode(MasterViewModel.CurrentNode);
                ShowHideTileNavigation();
                return;
            }
            else if (caption == "\uE72A")
            {
                MasterViewModel.GoToNode(MasterViewModel.CurrentNode);
                ShowHideTileNavigation();
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
                    MasterViewModel.CurrentNode.Children.Remove(phraseNode);
                    SetPageMode(PageMode.Run);
                    MasterViewModel.GoToNode(MasterViewModel.CurrentNode);
                    ShowHideTileNavigation();
                    break;

                case PageMode.Edit:
                    var navParams = new KeyboardPageNavigationParams
                    {
                        RootNode = MasterViewModel.RootNode,
                        CurrentNode = MasterViewModel.CurrentNode,
                        ChildNode = phraseNode,
                        NeedsSaving = false
                    };
                    Frame.Navigate(typeof(KeyboardPage), navParams);
                    break;

                default:
                    if (phraseNode.IsCategory)
                    {
                        MasterViewModel.GoToNode(phraseNode);
                        ShowHideTileNavigation();
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
                RootNode = MasterViewModel.RootNode,
                CurrentNode = MasterViewModel.CurrentNode,
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

        private void grdvwPhrases_LayoutUpdated(object sender, object e)
        {
            ShowHideTileNavigation();
        }

        private void OnToolsClick(object sender, RoutedEventArgs e)
        {
            if (_toolBarToggled)
            {
                SetToolsToolbarVisible(false);
            }
            else
            {
                SetToolsToolbarVisible(true);
            }
        }

        private void SetToolsToolbarVisible(bool show)
        {
            if (show)
            {
                grdTools.Visibility = Visibility.Visible;
                _toolBarToggled = true;
                ToolsButton.Background = Resources["ToolBarForegroundBrush"] as SolidColorBrush;
                ToolsButton.Foreground = Resources["ToolBarBackgroundBrush"] as SolidColorBrush;
            }
            else
            {
                grdTools.Visibility = Visibility.Collapsed;
                _toolBarToggled = false;
                ToolsButton.Background = Resources["ToolBarBackgroundBrush"] as SolidColorBrush;
                ToolsButton.Foreground = Resources["ToolBarForegroundBrush"] as SolidColorBrush;
            }
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Visibility = Visibility.Visible;
            GazeInput.SetInteraction(MenuGrid, Interaction.Disabled);
            GazeInput.SetInteraction(PhraseGrid, Interaction.Disabled);
            GazeInput.SetInteraction(PauseButton, Interaction.Disabled);
        }

        private void OnExitSettings(object sender, RoutedEventArgs e)
        {
            MasterViewModel.Settings.Save();
            grdvwPhrases.FontSize = MasterViewModel.Settings.FontSize;
            SettingsGrid.Visibility = Visibility.Collapsed;
            GazeInput.SetInteraction(MenuGrid, Interaction.Enabled);
            GazeInput.SetInteraction(PhraseGrid, Interaction.Enabled);
            GazeInput.SetInteraction(PauseButton, Interaction.Enabled);
            AdjustTileSize();
            SetToolsToolbarVisible(false);
            if (MasterViewModel.Settings.GazePlusClickMode)
            {                
                GazeInput.SetFixationDuration(this, TimeSpan.FromDays(1));
            }
            else
            {                
                GazeInput.SetFixationDuration(this, _FixationDefault);
            }
        }

        private void grdvwPhrases_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double targetMenuHeight = this.ActualHeight / (MasterViewModel.Settings.Rows + 1);            
            MenuGrid.Height = targetMenuHeight;

            //Reposition scroll viewer to top to avoid misalignment of back/next buttons with rows 
            //that can get partially displayed when resize occurs if scrollviewer is in a nonzero position
            var viewer = UIHelper.FindChildControl<ScrollViewer>(grdvwPhrases, "ScrollViewer") as ScrollViewer;
            viewer.ChangeView(null, 0, null, false);

            MasterViewModel.TileHeight = (int)(e.NewSize.Height / MasterViewModel.Settings.Rows);
            MasterViewModel.TileWidth = (int)(e.NewSize.Width / MasterViewModel.Settings.Cols);
        }

        private void AdjustTileSize()
        {
            double targetMenuHeight = this.ActualHeight / (MasterViewModel.Settings.Rows + 1);            
            MenuGrid.Height = targetMenuHeight;

            MasterViewModel.TileHeight = (int)(grdvwPhrases.ActualHeight / MasterViewModel.Settings.Rows);
            MasterViewModel.TileWidth = (int)(grdvwPhrases.ActualWidth / MasterViewModel.Settings.Cols);
        }

        private void butBack_Click(object sender, RoutedEventArgs e)
        {
            var viewer = UIHelper.FindChildControl<ScrollViewer>(grdvwPhrases, "ScrollViewer") as ScrollViewer;
            var current = viewer.VerticalOffset;

            viewer.ChangeView(null, current - (MasterViewModel.TileHeight), null, false);
        }

        private void butNext_Click(object sender, RoutedEventArgs e)
        {
            var viewer = UIHelper.FindChildControl<ScrollViewer>(grdvwPhrases, "ScrollViewer") as ScrollViewer;
            var current = viewer.VerticalOffset;

            viewer.ChangeView(null, (MasterViewModel.TileHeight) + current, null, false);
        }

        private void Viewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ShowHideTileNavigation();
        }

        private void ShowHideTileNavigation()
        {
            if (_tileViewer == null) return;

            if (_tileViewer.VerticalOffset >= _tileViewer.ExtentHeight - _tileViewer.ActualHeight)
            {
                grdNext.Visibility = Visibility.Collapsed;
            }
            else
            {
                grdNext.Visibility = Visibility.Visible;
            }

            if (_tileViewer.VerticalOffset <= 0)
            {
                grdBack.Visibility = Visibility.Collapsed;
            }
            else
            {
                grdBack.Visibility = Visibility.Visible;
            }
        }

        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            MasterViewModel.Settings.FontColor = args.NewColor;
        }

        private void GazeElement_Invoked(object sender, DwellInvokedRoutedEventArgs e)
        {
            if (MasterViewModel.Settings.GazePlusClickMode)
            {
                e.Handled = true;
            }
        }

        private void GazeElement_DwellProgressFeedback(object sender, DwellProgressEventArgs e)
        {
            if (MasterViewModel.Settings.GazePlusClickMode)
            {

                switch (e.State)
                {
                    case DwellProgressState.Fixating:

                        TargetButton = sender as Button;
                        break;

                    case DwellProgressState.Idle:

                        TargetButton = null;
                        break;
                }
            }
        }

        Button TargetButton;

        private void Page_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (MasterViewModel.Settings.GazePlusClickMode)
            {
                if (TargetButton != null)
                {
                    var peer = FrameworkElementAutomationPeer.CreatePeerForElement(TargetButton);
                    var provider = (IInvokeProvider)peer;
                    provider.Invoke();
                }
            }
        }
    }
}
