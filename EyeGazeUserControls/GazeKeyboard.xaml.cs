using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace EyeGazeUserControls
{
    public sealed partial class GazeKeyboard : UserControl
    {
        readonly StringBuilder _theText= new StringBuilder();

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
        }

        private void OnChar(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var content = button.Content.ToString();
            _theText.Append(content);
            textControl.Text = _theText.ToString();
        }

        private void OnSpace(object sender, RoutedEventArgs e)
        {
            _theText.Append(' ');
            textControl.Text = _theText.ToString();
        }

        private void OnBackspace(object sender, RoutedEventArgs e)
        {
            if (_theText.Length != 0)
            {
                _theText.Remove(_theText.Length - 1, 1);
                textControl.Text = _theText.ToString();
            }
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
