using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.Toolkit.UWP.Input.Gaze;
using Windows.Foundation.Collections;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeGazeUserControls
{
    public sealed partial class GazeKeyboard : UserControl
    {
        StringBuilder _theText;
        GazePointer _gazePointer;

        public ButtonBase EnterButton
        {
            get { return enterButton; }
        }

        public ButtonBase CloseButton
        {
            get { return closeButton; }
        }

        public ButtonBase SettingsButton
        {
            get { return settingsButton; }
        }

        public TextBox TextControl
        {
            get { return textControl; }
        }

        public GazeKeyboard()
        {
            this.InitializeComponent();
            _theText = new StringBuilder();

            var sharedSettings = new ValueSet();
            GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new Windows.Foundation.AsyncActionCompletedHandler((asyncInfo, asyncStatus) => {
                _gazePointer = new GazePointer(this);
                _gazePointer.LoadSettings(sharedSettings);
                //_gazePointer.OnGazePointerEvent += OnGazePointerEvent;
            });
        }

        private void OnGazePointerEvent(GazePointer sender, GazePointerEventArgs ea)
        {
            if (ea.PointerState == GazePointerState.Dwell)
            {
                _gazePointer.InvokeTarget(ea.HitTarget);
            }
        }

        private void OnChar(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tag = button.Tag.ToString();
            if (tag == "0x22")
            {
                _theText.Append(' ');
            }
            else if (tag == "0x0E")
            {
                _theText.Remove(_theText.Length - 1, 1);
            }
            else
            {
                var content = button.Content.ToString();
                _theText.Append(content);
            }
            textControl.Text = _theText.ToString();
        }

        private void OnWordDelete(object sender, RoutedEventArgs e)
        {
            var text = _theText.ToString();
            int lastSpace = text.LastIndexOf(' ');
            if (lastSpace > 0)
            {
                _theText.Remove(lastSpace, _theText.Length - lastSpace);
            }
            else
            {
                // no space found, so empty the textbox
                _theText.Clear();
            }
            textControl.Text = _theText.ToString();
        }
    }
}
