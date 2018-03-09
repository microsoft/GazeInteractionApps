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
using Microsoft.Research.Input.Gaze;

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
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string PhraseConfigFile = "PhraseData.phr";
        PhraseNode _rootNode;
        PhraseNode _curNode;
        SolidColorBrush _backgroundBrush;
        SolidColorBrush _foregroundBrush;
        SpeechSynthesizer _speechSynthesizer;
        MediaElement _mediaElement;
        PageMode _pageMode;
        GazePointer _gazePointer;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            _gazePointer = new GazePointer(this);
            _gazePointer.OnGazePointerEvent += OnGazePointerEvent;

            _pageMode = PageMode.Run;
            _speechSynthesizer = new SpeechSynthesizer();
            _mediaElement = new MediaElement();
            _backgroundBrush = new SolidColorBrush(Colors.Gray);
            _foregroundBrush = new SolidColorBrush(Colors.Blue);
        }

        private void OnGazePointerEvent(GazePointer sender, GazePointerEventArgs ea)
        {
            if (ea.PointerState == GazePointerState.Dwell)
            {
                _gazePointer.InvokeTarget(ea.HitTarget);
            }
        }

        private async void CopyConfigFileMaybe()
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.TryGetItemAsync(PhraseConfigFile);
            if (file == null)
            {
                var configFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///" + PhraseConfigFile));
                await configFile.CopyAsync(folder);
            }
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            CopyConfigFileMaybe();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var navParams = e.Parameter as KeyboardPageNavigationParams;
            if (navParams == null)
            {
                LoadConfigFile(PhraseConfigFile);
                return;
            }

            _rootNode = navParams.RootNode;

            if (navParams.NeedsSaving)
            {
                SaveConfigFile(PhraseConfigFile);
            }
            GotoNode(navParams.CurrentNode);
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
            var isCategory = jsonObj.ContainsKey("isCategory") ? jsonObj.GetNamedBoolean("isCategory") : (items.Count > 0);

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
            return phraseNode;
        }

        private async void LoadConfigFile(string filename)
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.GetFileAsync(PhraseConfigFile);
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

        private void GotoNode(PhraseNode phraseNode)
        {
            int rows;
            int cols;

            _curNode = phraseNode;

            int size = phraseNode.Children.Count;
            GetNumRowColumns(size, out rows, out cols);

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

            int curNode = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    var button = new Button();
                    button.Content = phraseNode.Children[curNode].Caption;
                    button.Background = _backgroundBrush;
                    button.Foreground = _foregroundBrush;
                    button.VerticalAlignment = VerticalAlignment.Stretch;
                    button.HorizontalAlignment = HorizontalAlignment.Stretch;
                    button.Click += OnGridButtonClick;
                    button.Tag = phraseNode.Children[curNode];
                    button.Style = Resources["PhraseButton"] as Style;

                    Grid.SetColumn(button, col);
                    Grid.SetRow(button, row);
                    PhraseGrid.Children.Add(button);

                    curNode++;
                    if (curNode >= size)
                    {
                        return;
                    }
                }
            }
        }

        private void GetNumRowColumns(int size, out int rows, out int cols)
        {
            rows = (int)Math.Sqrt(size);
            cols = rows;
            while (rows * cols < size)
            {
                cols++;
            }
        }

        private void OnHomeClick(object sender, RoutedEventArgs e)
        {
            GotoNode(_rootNode);
        }

        private void OnUpClick(object sender, RoutedEventArgs e)
        {
            if (_curNode.Parent != null)
            {
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
            _pageMode = (_pageMode == PageMode.Run) ? PageMode.Edit : PageMode.Run;
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            _pageMode = (_pageMode == PageMode.Run) ? PageMode.Delete: PageMode.Run;
        }

        private async void OnGridButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var phraseNode = button.Tag as PhraseNode;
            var caption = phraseNode.Caption;

            switch (_pageMode)
            {
                case PageMode.Delete:
                    if ((phraseNode.IsCategory) && (phraseNode.Children.Count > 0))
                    {
                        // TODO: Add status info somewhere.
                        return;
                    }
                    _curNode.Children.Remove(phraseNode);
                    PhraseGrid.Children.Remove(button);
                    _pageMode = PageMode.Run;
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

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
