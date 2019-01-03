using System;
using System.Collections.Generic;
using System.IO;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Core.Preview;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GazeApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        List<Button> buttons = new List<Button>();
        Random rand = new Random();
        SolidColorBrush black = new SolidColorBrush(Windows.UI.Colors.Black);
        SolidColorBrush white = new SolidColorBrush(Windows.UI.Colors.White);
        SolidColorBrush blue = new SolidColorBrush(Windows.UI.Colors.Blue);
        Button currentButton = null;
        TranslateTransform translateTransform = new TranslateTransform();
        




        TransformGroup transformGroup = new TransformGroup();
        Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;
        private IRandomAccessStream stream;
        private IOutputStream outputStream;
        private DataWriter dataWriter;
        

        public MainPage()
        {
            createLogFileAsync();
            this.InitializeComponent();
            //SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += App_CloseRequested;
            // Ensure the current window is active
            Window.Current.Activate();
            var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            appView.Title = "ISO 9241 Pointing Task";
            buttons.Add(button1);
            buttons.Add(button2);
            buttons.Add(button3);
            buttons.Add(button4);
            buttons.Add(button5);
            buttons.Add(button6);
            buttons.Add(button7);
            buttons.Add(button8);
            foreach (Button b in buttons)
            {
                b.Visibility = Visibility.Collapsed;
                b.Margin = new Thickness(0, 0, 0, 0);
            }
            resetButtons();
            currentButton = button1 ;
            currentButton.Background = black;
            currentButton.Foreground = white;
            currentButton.Content = "X";



            centerButton.Margin = new Thickness(0, 0, 0, 0);
            translateTransform = new TranslateTransform();
            transformGroup = new TransformGroup();
            translateTransform.X = bounds.Width/2 -centerButton.Width/2;
            translateTransform.Y = bounds.Height/ 2 -centerButton.Height / 2;
            transformGroup.Children.Add(translateTransform);
            centerButton.RenderTransform = transformGroup;
            centerButton.Visibility = Visibility.Collapsed;

            StartButton.Margin = new Thickness(0, 0, 0, 0);
            translateTransform = new TranslateTransform();
            transformGroup = new TransformGroup();
            translateTransform.X = bounds.Width / 2 - centerButton.Width / 2;
            translateTransform.Y = bounds.Height / 2 - centerButton.Height / 2;
            transformGroup.Children.Add(translateTransform);
            StartButton.RenderTransform = transformGroup;
            
            
        }

        private async void createLogFileAsync()
        {
            Windows.Storage.StorageFolder storageFolder =
                Windows.Storage.ApplicationData.Current.LocalFolder;
            var messageDialog = new MessageDialog(storageFolder.ToString());

            Console.WriteLine(storageFolder);
            Windows.Storage.StorageFile sampleFile =
                await storageFolder.CreateFileAsync("sample.txt",
                    Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            stream = await sampleFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
             outputStream = stream.GetOutputStreamAt(0);
            dataWriter = new Windows.Storage.Streams.DataWriter(outputStream);




        }

        public static string GetNextAvailableFilename(string filename)
        {
            if (!System.IO.File.Exists(filename)) return filename;

            string alternateFilename;
            int fileNameIndex = 1;
            do
            {
                fileNameIndex += 1;
                alternateFilename = CreateNumberedFilename(filename, fileNameIndex);
            } while (System.IO.File.Exists(alternateFilename));

            return alternateFilename;
        }

        private static string CreateNumberedFilename(string filename, int number)
        {
            string plainName = System.IO.Path.GetFileNameWithoutExtension(filename);
            string extension = System.IO.Path.GetExtension(filename);
            return string.Format("{0}{1}{2}", plainName, number, extension);
        }


        private void positionTargetsAsync()
        {

            TranslateTransform translateTransform = new TranslateTransform();
          
            TransformGroup transformGroup = new TransformGroup();


            double radius = 260;

            int steps = buttons.Count;
            
           
            double left;
            double top;
            //var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            //var size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);

            for(var i=1;i<=buttons.Count;i++)
            {
                buttons[i-1].Visibility = Visibility.Visible;
                left = (bounds.Width / 2 + radius * Math.Cos(2 * Math.PI * i/ steps));
                top = (bounds.Height / 2 + radius * Math.Sin(2 * Math.PI * i / steps));
                translateTransform = new TranslateTransform();
                transformGroup = new TransformGroup();
                translateTransform.X = left;
                translateTransform.Y = top;
                //Console.WriteLine("left: " + left + ",top: "+top);

                transformGroup.Children.Add(translateTransform);

                buttons[i-1].RenderTransform = transformGroup;
                
            }
            
        }


        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //positionTargetsAsync();
            centerButton.Visibility = Visibility.Visible;
            Button b = sender as Button;
            b.Visibility = Visibility.Collapsed;
            writeDataAsync("StartButton");




        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            


            Button b = sender as Button;

            if(b==centerButton)
            {
                positionTargetsAsync();
                centerButton.Visibility = Visibility.Collapsed;
                writeDataAsync("CenterButton");
            }
            else
            if (b.Name.Equals(((Button)currentButton).Name))
            {
                resetButtons();
                var r = rand.Next(1, 8);
                Button s = buttons[r] as Button;
                s.Background = black;
                s.Foreground = white;
                s.Content = "X";
                s.FontSize = 50;
                currentButton = s;
                centerButton.Visibility = Visibility.Visible;
                writeDataAsync("CurrentButton" );
            }
            else
                writeDataAsync(b.Name);

        }
        private void resetButtons()
        {
            foreach(Button b in buttons)
            {
                
                b.Background = blue;
                b.Content = "";
                b.Visibility = Visibility.Collapsed;
            }

        }
        private async void writeDataAsync(String data)
        {
            dataWriter.WriteString(data + "," + DateTime.UtcNow.Ticks+Environment.NewLine);
            await dataWriter.StoreAsync();
            await dataWriter.FlushAsync();
        }
   
    }
}
