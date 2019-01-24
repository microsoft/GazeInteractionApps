using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Input.Preview;


namespace Pointing_Task
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

		int counter = 0;
		int trials = 0;




		TransformGroup transformGroup = new TransformGroup();
		Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;
		private IRandomAccessStream stream;
		private IOutputStream outputStream;
		private DataWriter dataWriter;


		public MainPage()
		{

			createLogFileAsync();//creating logFile in the app's localstate folder
			this.InitializeComponent();
			Window.Current.Activate();
			var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
			appView.Title = "Pointing Task";
			buttons.Add(button1);
			buttons.Add(button2);
			buttons.Add(button3);
			buttons.Add(button4);
			buttons.Add(button5);
			buttons.Add(button6);
			buttons.Add(button7);
			buttons.Add(button8);
			//initialising all button positions to 0,0
			foreach (Button b in buttons)
			{
				b.Visibility = Visibility.Collapsed;
				b.Margin = new Thickness(0, 0, 0, 0);
			}
			resetButtons();
			currentButton = button1;
			currentButton.Background = black;
			currentButton.Foreground = white;
			currentButton.Content = "X";



			centerButton.Margin = new Thickness(0, 0, 0, 0);
			translateTransform = new TranslateTransform();
			transformGroup = new TransformGroup();
			translateTransform.X = bounds.Width / 2 - centerButton.Width / 2;
			translateTransform.Y = bounds.Height / 2 - centerButton.Height / 2;
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


			Windows.Storage.StorageFile sampleFile =
			 await storageFolder.CreateFileAsync("log.txt",
			  Windows.Storage.CreationCollisionOption.GenerateUniqueName);
			stream = await sampleFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
			outputStream = stream.GetOutputStreamAt(0);
			dataWriter = new Windows.Storage.Streams.DataWriter(outputStream);




		}

		
		struct cStruct //structure for storing combination
		{
			public int radius;
			public int edge;
			public cStruct(int radius, int edge)
			{
				this.radius = radius;
				this.edge = edge;
			}
		};
		List<cStruct> combinationList = new List<cStruct>();

		private void resetList() //generating list for all combinations
		{
			
			int radius = 200;
			int edge = 135;
			int radius1 = 260;
			int edge1 = 100;

			combinationList = new List<cStruct>();
			combinationList.Add(new cStruct(radius, edge)); //200,135
			combinationList.Add(new cStruct(radius1, edge1)); //260,100
			combinationList.Add(new cStruct(radius1, edge)); //260,135
			combinationList.Add(new cStruct(radius, edge1)); //200,100
		}

		private void positionTargets() //positioning targets in a circle
		{

			TranslateTransform translateTransform = new TranslateTransform();

			TransformGroup transformGroup = new TransformGroup();
			if (counter % 4 == 0) resetList();

			int steps = buttons.Count;


			double left;
			double top;

			Random random = new Random();
			int a = random.Next(0, combinationList.Count - 1);
			if (counter == 3)
			{
				a = 0;
			}
			var cStruct = combinationList[a];

			for (var i = 1; i <= buttons.Count; i++)
			{
				buttons[i - 1].Visibility = Visibility.Visible;
				buttons[i - 1].Width = cStruct.edge;
				buttons[i - 1].Height = cStruct.edge;
				left = (bounds.Width / 2 + cStruct.radius * Math.Cos(2 * Math.PI * i / steps));
				top = (bounds.Height / 2 + cStruct.radius * Math.Sin(2 * Math.PI * i / steps));
				translateTransform = new TranslateTransform();
				transformGroup = new TransformGroup();
				translateTransform.X = left;
				translateTransform.Y = top;
				transformGroup.Children.Add(translateTransform);

				buttons[i - 1].RenderTransform = transformGroup;

			}


			counter++;
			combinationList.Remove(cStruct);
			trials++;
			if (trials == 20) Application.Current.Exit();

		}


		private void StartButton_Click(object sender, RoutedEventArgs e)
		{
			centerButton.Visibility = Visibility.Visible;
			Button b = sender as Button;
			b.Visibility = Visibility.Collapsed;
			var ttv = b.TransformToVisual(Window.Current.Content);
			Point screenCoords = ttv.TransformPoint(new Point(0, 0));
			writeDataAsync("StartButton," + screenCoords.X + "," + screenCoords.Y + "," + b.Width + "," + b.Height);

		}

		private void Button5_Click(object sender, RoutedEventArgs e)
		{

			Button b = sender as Button;

			if (b == centerButton)
			{
				positionTargets();
				centerButton.Visibility = Visibility.Collapsed;
				var ttv = b.TransformToVisual(Window.Current.Content);
				Point screenCoords = ttv.TransformPoint(new Point(0, 0));
				writeDataAsync("CenterButton," + bounds.Width / 2 + "," + bounds.Height / 2 + "," + b.Width + "," + b.Height);
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
				var ttv = b.TransformToVisual(Window.Current.Content);
				Point screenCoords = ttv.TransformPoint(new Point(0, 0));
				writeDataAsync("CurrentButton," + screenCoords.X + "," + screenCoords.Y + "," + b.Width + "," + b.Height);

			}
			else
			{
				var ttv = b.TransformToVisual(Window.Current.Content);
				Point screenCoords = ttv.TransformPoint(new Point(0, 0));
				writeDataAsync("Missed" + b.Name + "," + screenCoords.X + "," + screenCoords.Y + "," + b.Width + "," + b.Height);
			}

		}
		private void resetButtons()
		{
			foreach (Button b in buttons)
			{

				b.Background = blue;
				b.Content = "";
				b.Visibility = Visibility.Collapsed;
			}

		}
		private async void writeDataAsync(String data)
		{
			dataWriter.WriteString(data + "," + DateTime.UtcNow.Ticks + Environment.NewLine);
			await dataWriter.StoreAsync();
			await dataWriter.FlushAsync();
		}

	}
}