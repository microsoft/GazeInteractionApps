//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Research.Input.Gaze;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;

namespace EyeControlToolkitSettings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private GazePointer _gazePointer;
        public GazeSettings GazeSettings = new GazeSettings();

        public MainPage()
        {
            InitializeComponent();

            var localSettings = new ValueSet();
            GazeSettings.ValueSetFromLocalSettings(localSettings);

            _gazePointer = new GazePointer(this);
            _gazePointer.LoadSettings(localSettings);
            _gazePointer.OnGazePointerEvent += OnGazePointerEvent;

            GazeSettings.GazePointer = _gazePointer;
        }

        private void OnGazePointerEvent(GazePointer sender, GazePointerEventArgs ea)
        {
            if (ea.PointerState == GazePointerState.Dwell)
            {
                _gazePointer.InvokeTarget(ea.HitTarget);
            }
        }
    }
}
