//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
//See LICENSE in the project root for license information. 

using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace EyeGazeUserControls
{
    public sealed partial class GazeKeyboard : UserControl
    {
        readonly StringBuilder _theText= new StringBuilder();

        InputInjector _injector;

        ButtonBase _enterButton;
        ButtonBase _closeButton;
        ButtonBase _settingsButton;
        TextBox    _textControl;
        String     _layoutFile;

        List<ButtonBase> _keyboardButtons;

        public ButtonBase EnterButton
        {
            get { return _enterButton; }
        }

        public ButtonBase CloseButton
        {
            get { return _closeButton; }
        }

        public ButtonBase SettingsButton
        {
            get { return _settingsButton; }
        }

        public TextBox TextControl
        {
            get { return _textControl; }
        }

        public string LayoutFile
        {
            get { return _layoutFile; }
        }

        private bool _gazePlusClickMode = false;
        public bool GazePlusClickMode
        {
            get
            {
                return _gazePlusClickMode;
            }
            set
            {              
                _gazePlusClickMode = value;
            }
        }

        public GazeKeyboard()
        {
            InitializeComponent();
            _injector = InputInjector.TryCreate();
        }

        internal static void FindChildren<T>(List<T> results, DependencyObject startNode)
          where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(startNode);
            for (int i = 0; i < count; i++)
            {
                DependencyObject current = VisualTreeHelper.GetChild(startNode, i);
                if ((current.GetType()).Equals(typeof(T)) || (current.GetType().GetTypeInfo().IsSubclassOf(typeof(T))))
                {
                    T asType = (T)current;
                    results.Add(asType);
                }
                FindChildren<T>(results, current);
            }
        }

        public async Task LoadLayout(string layoutFile)
        {
            try
            {
                var uri = new Uri($"ms-appx:///EyeGazeUserControls/Layouts/{layoutFile}");
                var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
                var xaml = await FileIO.ReadTextAsync(storageFile);
                var xamlNode = XamlReader.Load(xaml) as FrameworkElement;
                //Load known variables
                _enterButton = xamlNode.FindName("enterButton") as Button;
                _settingsButton = xamlNode.FindName("settingsButton") as Button;
                _closeButton = xamlNode.FindName("closeButton") as Button;
                _textControl = xamlNode.FindName("textControl") as TextBox;

                _keyboardButtons = new List<ButtonBase>();
                FindChildren<ButtonBase>(_keyboardButtons, xamlNode);

                foreach (var button in _keyboardButtons)
                {
                    button.Click += OnKeyboardButtonClick;
                }

                LayoutRoot.Children.Add(xamlNode);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            _layoutFile = layoutFile;
        }

        private void HandleVirtualKey(int vk)
        {
            if (vk < 0)
            {
                return;
            }
            var key = new InjectedInputKeyboardInfo();
            key.VirtualKey = (ushort)vk;
            _injector.InjectKeyboardInput(new[] { key });
        }

        private void HandleVirtualKeyList(List<int> vkList)
        {
            var state = new Dictionary<int, bool>();
            var keys = new List<InjectedInputKeyboardInfo>();
            foreach (var vk in vkList)
            {
                var key = new InjectedInputKeyboardInfo();
                key.VirtualKey = (ushort)vk;
                if (state.ContainsKey(vk))
                {
                    key.KeyOptions = InjectedInputKeyOptions.KeyUp;
                    state.Remove(vk);
                }
                else
                {
                    state.Add(vk, true);
                }
                keys.Add(key);
            }
            _injector.InjectKeyboardInput(keys);
        }

        private async void OnKeyboardButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as ButtonBase;
            _textControl.Focus(FocusState.Programmatic);
            await Task.Delay(1);

            var vk = Keyboard.GetVK(button);
            if (vk != 0)
            {
                HandleVirtualKey(vk);
                return;
            }

            var vkList = Keyboard.GetVKList(button);
            if ((vkList != null) && (vkList.Count > 0)) 
            {
                HandleVirtualKeyList(vkList);
                return;
            }

            // default case
            var key = new InjectedInputKeyboardInfo();
            key.ScanCode = (ushort)button.Content.ToString()[0];
            key.KeyOptions = InjectedInputKeyOptions.Unicode;
            _injector.InjectKeyboardInput(new[] { key });
        }
    }
}
