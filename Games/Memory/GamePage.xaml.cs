//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using Microsoft.Services.Store.Engagement;
using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace Memory
{
    public sealed partial class GamePage : Page
    {
        //const byte MIN_CHAR = 0x21;        
        //const byte MAX_CHAR = 0xff;
       
        enum BoardSpeed
        {
            Fast,
            Slow
        }

        BoardSpeed _boardSpeed = BoardSpeed.Slow;
        
        string[] VintageCollection = {"\u0021", "\u0022", "\u0023", "\u0024", "\u0025", "\u0026", "\u0027", "\u0028", "\u0029", "\u002A", "\u002B", "\u002C", "\u002D", "\u002E", "\u002F", "\u0030", "\u0031", "\u0032", "\u0033", "\u0034", "\u0035", "\u0036", "\u0037", "\u0038", "\u0039", "\u003A", "\u003B", "\u003C", "\u003D", "\u003E", "\u003F", "\u0040", "\u0041", "\u0042", "\u0043", "\u0044", "\u0045", "\u0046", "\u0047", "\u0048", "\u0049", "\u004A", "\u004B", "\u004C", "\u004D", "\u004E", "\u004F", "\u0050", "\u0051", "\u0052", "\u0053", "\u0054", "\u0055", "\u0056", "\u0057", "\u0058", "\u0059", "\u005A", "\u005B", "\u005C", "\u005D", "\u005E", "\u005F", "\u0060", "\u0061", "\u0062", "\u0063", "\u0064", "\u0065", "\u0066", "\u0067", "\u0068", "\u0069", "\u006A", "\u006B", "\u006C", "\u006D", "\u006E", "\u006F", "\u0070", "\u0071", "\u0072", "\u0073", "\u0074", "\u0075", "\u0076", "\u0077", "\u0078", "\u0079", "\u007A", "\u007B", "\u007C", "\u007D", "\u007E", "\u007F", "\u0080", "\u0081", "\u0082", "\u0083", "\u0084", "\u0085", "\u0086", "\u0087", "\u0088", "\u0089", "\u008A", "\u008B", "\u008C", "\u008D", "\u008E", "\u008F", "\u0090", "\u0091", "\u0092", "\u0093", "\u0094", "\u0095", "\u0096", "\u0097", "\u0098", "\u0099", "\u009A", "\u009B", "\u009C", "\u009D", "\u009E", "\u009F", "\u00A0", "\u00A1", "\u00A2", "\u00A3", "\u00A4", "\u00A5", "\u00A6", "\u00A7", "\u00A8", "\u00A9", "\u00AA", "\u00AB", "\u00AC", "\u00AD", "\u00AE", "\u00AF", "\u00B0", "\u00B1", "\u00B2", "\u00B3", "\u00B4", "\u00B5", "\u00B6", "\u00B7", "\u00B8", "\u00B9", "\u00BA", "\u00BB", "\u00BC", "\u00BD", "\u00BE", "\u00BF", "\u00C0", "\u00C1", "\u00C2", "\u00C3", "\u00C4", "\u00C5", "\u00C6", "\u00C7", "\u00C8", "\u00C9", "\u00CA", "\u00CB", "\u00CC", "\u00CD", "\u00CE", "\u00CF", "\u00D0", "\u00D1", "\u00D2", "\u00D3", "\u00D4", "\u00D5", "\u00D6", "\u00D7", "\u00D8", "\u00D9", "\u00DA", "\u00DB", "\u00DC", "\u00DD", "\u00DE", "\u00DF", "\u00E0", "\u00E1", "\u00E2", "\u00E3", "\u00E4", "\u00E5", "\u00E6", "\u00E7", "\u00E8", "\u00E9", "\u00EA", "\u00EB", "\u00EC", "\u00ED", "\u00EE", "\u00EF", "\u00F0", "\u00F1", "\u00F2", "\u00F3", "\u00F4", "\u00F5", "\u00F6", "\u00F7", "\u00F8", "\u00F9", "\u00FA", "\u00FB", "\u00FC", "\u00FD", "\u00FE", "\u00FF" };        
        string[] EmojiCollection1;

        string[] _symbolCollection;
        FontFamily _symbolFontfamily;

        enum SymbolSets
        {
            VintageCollection,
            EmojiCollection1
        }
     
        Random _rnd;
        Button _firstButton;
        Button _secondButton;
        DispatcherTimer _flashTimer;
        int _remaining;
        int _numMoves;
        bool _usePictures;

        bool _interactionPaused = false;

        bool _animationActive = false;
        bool _reverseAnimationActive = false;

        bool _gazePlusSwitch;

        bool _gameOver = false;

        SolidColorBrush _solidTileBrush;        
        SolidColorBrush _toolButtonBrush;
        SolidColorBrush _pausedButtonBrush = new SolidColorBrush(Colors.Black);
        SolidColorBrush _whiteBrush = new SolidColorBrush(Colors.White);
        SolidColorBrush _transparentBrush = new SolidColorBrush(Colors.Transparent);

        int _boardRows = 6;
        int _boardColumns = 11;

        CompositionScopedBatch _reverseFlipBatchAnimation;
        CompositionScopedBatch _flipBatchAnimation;
        CompositionScopedBatch _resetBatchAnimation;

        private void SetSymbolCollection(SymbolSets collection)
        {
            _symbolCollection = null;
            switch (collection)
            {
                case SymbolSets.VintageCollection:
                    _symbolCollection = VintageCollection;
                    _symbolFontfamily = new FontFamily("Webdings");
                    break;

                case SymbolSets.EmojiCollection1:
                    _symbolCollection = EmojiCollection1;
                    _symbolFontfamily = new FontFamily("Segoe Color Emoji");
                    break;
            }
        }

        private void SetRandomSymbolSet()
        {
            string[] symbolsets = Enum.GetNames(typeof(SymbolSets));            
            int randomIndex = _rnd.Next(symbolsets.Length);
            SetSymbolCollection((SymbolSets)Enum.Parse(typeof(SymbolSets), symbolsets[randomIndex]));
        }

        public GamePage()
        {
            InitializeComponent();

            LoadEmojiListForCurrentOS();

            _solidTileBrush = (SolidColorBrush)this.Resources["TileBackground"];
            _toolButtonBrush = (SolidColorBrush)this.Resources["ToolBarButtonBackground"];

            GazeInput.DwellFeedbackCompleteBrush = new SolidColorBrush(Colors.Transparent);

            _rnd = new Random();
            _flashTimer = new DispatcherTimer();
            _flashTimer.Interval = new TimeSpan(0, 0, 1);
            _flashTimer.Tick += OnFlashTimerTick;
            _usePictures = false;

            SetSymbolCollection(SymbolSets.EmojiCollection1);            

            CoreWindow.GetForCurrentThread().KeyDown += CoredWindow_KeyDown;

            Loaded += MainPage_Loaded;

            //var sharedSettings = new ValueSet();
            //GazeSettingsHelper.RetrieveSharedSettings(sharedSettings).Completed = new AsyncActionCompletedHandler((asyncInfo, asyncStatus) => {
            //    var gazePointer = GazeInput.GetGazePointer(this);
            //    gazePointer.LoadSettings(sharedSettings);
            //});

            LoadLocalSettings();            
        }

        private void LoadEmojiListForCurrentOS()
        {
            if (Environment.OSVersion.Version.Build >= 18348)
            {
                EmojiCollection1 = new string[]{ "\U0001F632", "\U0001F621", "\U0001F974", "\U0001F624", "\U0001F9D0", "\U0001F92F", "\U0001F613", "\U0001F615", "\U0001F610", "\U0001F62C", "\U0001F928", "\U0001F914", "\U0001F644", "\U0001F60F", "\U0001F92D", "\U0001F61C", "\U0001F92A", "\U0001F917", "\U0001F643", "\U0001F642", "\u263A\uFE0F", "\U0001F618", "\U0001F929", "\U0001F60D", "\U0001F970", "\U0001F60E", "\U0001F609", "\U0001F923", "\U0001F605", "\U0001F606", "\U0001F603", "\U0001F4A9", "\U0001F916", "\U0001F47E", "\U0001F47D", "\U0001F47B", "\u2620\uFE0F", "\U0001F4A1", "\U0001F4A5", "\U0001F4A6", "\U0001F525", "\U0001F4A7", "\U0001F4AB", "\U0001F4AC", "\U0001F4AD", "\U0001F5EA", "\U0001F5F1", "\U0001F463", "\U0001F441\uFE0F\u200D\U0001F5E8\uFE0F", "\U0001F9B7", "\U0001F9B4", "\U0001F97D", "\U0001F9F5", "\U0001F9F6", "\U0001F9F7", "\U0001F9E6", "\U0001F45B", "\U0001F45C", "\U0001F45D", "\U0001F6CD\uFE0F", "\U0001F392", "\U0001F45F", "\U0001F97E", "\U0001F97F", "\U0001F460", "\U0001F462", "\U0001F451", "\U0001F452", "\U0001F393", "\u26D1\uFE0F", "\U0001F48E", "\U0001F596\U0001F3FB", "\U0001F48B", "\U0001F497", "\U0001F496", "\U0001F493", "\U0001F49E", "\U0001F48C", "\U0001F394", "\U0001F383", "\U0001F386", "\u2728", "\U0001F389", "\U0001F38A", "\U0001F382", "\U0001F380", "\U0001F381", "\U0001F397\uFE0F", "\U0001F3EE", "\U0001F396\uFE0F", "\U0001F3A0", "\U0001F3A1", "\U0001F3A2", "\U0001F3A6", "\U0001F39E\uFE0F", "\U0001F39F\uFE0F", "\U0001F3A4", "\U0001F3A7", "\U0001F3AD", "\U0001F3A8", "\U0001F3B3", "\u26F8\uFE0F", "\U0001F3AF", "\U0001F3AE", "\U0001F9E9", "\U0001F9F8", "\u26BD", "\U0001F945", "\u26BE", "\U0001F3C0", "\U0001F3C9", "\U0001F3C8", "\U0001F3BE", "\U0001F94F", "\U0001F3CF", "\U0001F3D2", "\U0001F94D", "\U0001F3D3", "\U0001F94C", "\u26F3", "\U0001F3BF", "\U0001F6F7", "\U0001F3C6", "\U0001F3C5", "\U0001F3C2\U0001F3FB", "\U0001F3C4", "\U0001F6A3\U0001F3FB", "\U0001F6B4\U0001F3FB", "\U0001F3CE\uFE0F", "\U0001F938\U0001F3FB", "\u267f", "\U0001F399\uFE0F", "\U0001F39B\uFE0F", "\U0001F4FB", "\U0001F3B7", "\U0001F3B8", "\U0001F3B9", "\U0001F398", "\U0001F3BA", "\U0001F3BB", "\U0001F941", "\U0001F514", "\U0001F3BC", "\U0001F3B6", "\U0001F3B5", "\U0001F436", "\U0001F415", "\U0001F429", "\U0001F43A", "\U0001F99D", "\U0001F408", "\U0001F42F", "\U0001F40E", "\U0001F993", "\U0001F98C", "\U0001F984", "\U0001F42E", "\U0001F403", "\U0001F437", "\U0001F411", "\U0001F410", "\U0001F42A", "\U0001F992", "\U0001F401", "\U0001F439", "\U0001F407", "\U0001F43F\uFE0F", "\U0001F994", "\U0001F987", "\U0001F43B", "\U0001F43C", "\U0001F998", "\U0001F43E", "\U0001F983", "\U0001F414", "\U0001F413", "\U0001F423", "\U0001F425", "\U0001F426", "\U0001F427", "\U0001F54A\uFE0F", "\U0001F985", "\U0001F9A2", "\U0001F989", "\U0001F99A", "\U0001F99C", "\U0001F438", "\U0001F40A", "\U0001F422", "\U0001F98E", "\U0001F40D", "\U0001F409", "\U0001F995", "\U0001F996", "\U0001F433", "\U0001F42C", "\U0001F41F", "\U0001F420", "\U0001F988", "\U0001F419", "\U0001F41A", "\U0001F980", "\U0001F99E", "\U0001F990", "\U0001F991", "\U0001F40C", "\U0001F98B", "\U0001F41B", "\U0001F41C", "\U0001F41D", "\U0001F41E", "\U0001F997", "\U0001F577\uFE0F", "\U0001F338", "\U0001F339", "\U0001F940", "\U0001F33A", "\U0001F33B", "\U0001F337", "\U0001F331", "\U0001F332", "\U0001F333", "\U0001F334", "\U0001F335", "\U0001F33E", "\U0001F33F", "\u2618\uFE0F", "\U0001F341", "\U0001F342", "\U0001F343", "\U0001F30D", "\U0001F9ED", "\U0001F303", "\U0001F304", "\U0001F305", "\U0001F309", "\U0001F319", "\u2600\uFE0F", "\u2B50", "\U0001F31F", "\U0001F320", "\u2604\uFE0F", "\U0001F30C", "\u26C5", "\u26C8\uFE0F", "\U0001F324\uFE0F", "\u26C6", "\U0001F32A\uFE0F", "\U0001F300", "\U0001F308", "\U0001F302", "\u2614", "\u26F1\uFE0F", "\u26A1", "\u2744\uFE0F", "\u26C4", "\U0001F30A", "\U0001F30B", "\u231B", "\u23F0", "\u23F1\uFE0F", "\U0001F570\uFE0F", "\U0001F950", "\U0001F968", "\U0001F32E", "\U0001F958", "\U0001F963", "\U0001F35C", "\U0001F96E", "\U0001F361", "\U0001F95F", "\U0001F960", "\U0001F961", "\U0001F366", "\U0001F367", "\U0001F368", "\U0001F369", "\U0001F36A", "\U0001F382", "\U0001F370", "\U0001F9C1", "\U0001F967", "\U0001F36B", "\U0001F36C", "\U0001F36D", "\U0001F36F", "\U0001F347", "\U0001F348", "\U0001F349", "\U0001F34C", "\U0001F34D", "\U0001F34F", "\U0001F350", "\U0001F351", "\U0001F352", "\U0001F353", "\U0001F95D", "\U0001F345", "\U0001F965", "\U0001F955", "\U0001F33D", "\U0001F336\uFE0F", "\U0001F96C", "\U0001F966", "\U0001F344", "\U0001F95C", "\U0001F330", "\U0001F37C", "\U0001F95B", "\u2615", "\U0001F375", "\U0001F376", "\U0001F37E", "\U0001F377", "\U0001F378", "\U0001F379", "\U0001F37B", "\U0001F942", "\U0001F3FA", "\U0001F484", "\U0001F9F4", "\U0001F9EF", "\U0001F52E", "\U0001F6D2", "\U0001F6CE\uFE0F", "\U0001F9F3", "\U0001F6AA", "\U0001F6CB\uFE0F", "\U0001F6C1", "\U0001F9F7", "\U0001F9F9", "\u26F2", "\U0001F3D5\uFE0F", "\U0001F3DC\uFE0F", "\U0001F692", "\U0001F691", "\U0001F69A", "\U0001F69C", "\U0001F6B2", "\U0001F6F4", "\U0001F6F9", "\U0001F6F5", "\u26F5", "\U0001F6F6", "\u2708\uFE0F", "\U0001F681", "\U0001F6A0", "\U0001F6F0\uFE0F", "\U0001F680", "\U0001F6F8", "\u2693", "\u267B\uFE0F", "\U0001F4BC", "\U0001F5D2\uFE0F", "\U0001F4CB", "\u270F\uFE0F", "\U0001F58B\uFE0F", "\U0001F58C\uFE0F", "\U0001F4DD", "\U0001F4CC", "\U0001F4D0", "\U0001F4F1", "\U0001F56F\uFE0F", "\U0001F4A1", "\U0001F526", "\U0001F9EF", "\U0001F9F0", "\U0001F9F2", "\u2699\uFE0F", "\U0001F528", "\u26CF\uFE0F", "\u2692\uFE0F", "\U0001F6E0\uFE0F", "\U0001F3F9", "\U0001F6E1\uFE0F", "\u2697\uFE0F", "\U0001F9EA", "\U0001F9EB", "\U0001F9EC", "\U0001F52C", "\U0001F52D", "\U0001F4E1", "\U0001F5DD\uFE0F", "\U0001F4F5", "\U0001F4F8", "\U0001F4FA", "\U0001F4FC", "\U0001F4FD\uFE0F", "\U0001F4FE", "\U0001F5A8\uFE0F", "\U0001F50C", "\U0001F90E", "\U0001F90D", "\U0001F9BE", "\U0001F9BF", "\U0001F9BB", "\U0001F9CF", "\U0001F9CD", "\U0001F9CE", "\U0001F469\u200D\U0001F9AF", "\U0001F468\u200D\U0001F9BC", "\U0001F469\u200D\U0001F9BD", "\U0001F9A7", "\U0001F9AE", "\U0001F415\u200D\U0001F9BA", "\U0001F9A5", "\U0001F9A6", "\U0001F9A8", "\U0001F9A9", "\U0001F9C4", "\U0001F9C5", "\U0001F9C7", "\U0001F9C6", "\U0001F9C8", "\U0001F9C3", "\U0001F9C9", "\U0001F9CA", "\U0001F9BD", "\U0001F9BC", "\U0001F6FA", "\U0001FA82", "\U0001FA90", "\U0001F93F", "\U0001FA80", "\U0001FA81", "\U0001FA70", "\U0001FA95", "\U0001FA94", "\U0001FA93", "\U0001F9AF", "\U0001FA79", "\U0001FA7A", "\U0001FA91" };
            }
            else
            {
                EmojiCollection1 = new string[] { "\U0001F632","\U0001F621","\U0001F974","\U0001F624","\U0001F9D0","\U0001F92F","\U0001F613","\U0001F615","\U0001F610","\U0001F62C","\U0001F928","\U0001F914","\U0001F644","\U0001F60F","\U0001F92D","\U0001F61C","\U0001F92A","\U0001F917","\U0001F643","\U0001F642","\u263A\uFE0F","\U0001F618","\U0001F929","\U0001F60D","\U0001F970","\U0001F60E","\U0001F609","\U0001F923","\U0001F605","\U0001F606","\U0001F603","\U0001F4A9","\U0001F916","\U0001F47E","\U0001F47D","\U0001F47B","\u2620\uFE0F","\U0001F4A1","\U0001F4A5","\U0001F4A6","\U0001F525","\U0001F4A7","\U0001F4AB","\U0001F4AC","\U0001F4AD","\U0001F5EA","\U0001F5F1","\U0001F463","\U0001F441\uFE0F\u200D\U0001F5E8\uFE0F","\U0001F9B7","\U0001F9B4","\U0001F97D","\U0001F9F5","\U0001F9F6","\U0001F9F7","\U0001F9E6","\U0001F45B","\U0001F45C","\U0001F45D","\U0001F6CD\uFE0F","\U0001F392","\U0001F45F","\U0001F97E","\U0001F97F","\U0001F460","\U0001F462","\U0001F451","\U0001F452","\U0001F393","\u26D1\uFE0F","\U0001F48E","\U0001F596\U0001F3FB","\U0001F48B","\U0001F497","\U0001F496","\U0001F493","\U0001F49E","\U0001F48C","\U0001F394","\U0001F383","\U0001F386","\u2728","\U0001F389","\U0001F38A","\U0001F382","\U0001F380","\U0001F381","\U0001F397\uFE0F","\U0001F3EE","\U0001F396\uFE0F","\U0001F3A0","\U0001F3A1","\U0001F3A2","\U0001F3A6","\U0001F39E\uFE0F","\U0001F39F\uFE0F","\U0001F3A4","\U0001F3A7","\U0001F3AD","\U0001F3A8","\U0001F3B3","\u26F8\uFE0F","\U0001F3AF","\U0001F3AE","\U0001F9E9","\U0001F9F8","\u26BD","\U0001F945","\u26BE","\U0001F3C0","\U0001F3C9","\U0001F3C8","\U0001F3BE","\U0001F94F","\U0001F3CF","\U0001F3D2","\U0001F94D","\U0001F3D3","\U0001F94C","\u26F3","\U0001F3BF","\U0001F6F7","\U0001F3C6","\U0001F3C5","\U0001F3C2\U0001F3FB","\U0001F3C4","\U0001F6A3\U0001F3FB","\U0001F6B4\U0001F3FB","\U0001F3CE\uFE0F","\U0001F938\U0001F3FB","\u267f","\U0001F399\uFE0F","\U0001F39B\uFE0F","\U0001F4FB","\U0001F3B7","\U0001F3B8","\U0001F3B9","\U0001F398","\U0001F3BA","\U0001F3BB","\U0001F941","\U0001F514","\U0001F3BC","\U0001F3B6","\U0001F3B5","\U0001F436","\U0001F415","\U0001F429","\U0001F43A","\U0001F99D","\U0001F408","\U0001F42F","\U0001F40E","\U0001F993","\U0001F98C","\U0001F984","\U0001F42E","\U0001F403","\U0001F437","\U0001F411","\U0001F410","\U0001F42A","\U0001F992","\U0001F401","\U0001F439","\U0001F407","\U0001F43F\uFE0F","\U0001F994","\U0001F987","\U0001F43B","\U0001F43C","\U0001F998","\U0001F43E","\U0001F983","\U0001F414","\U0001F413","\U0001F423","\U0001F425","\U0001F426","\U0001F427","\U0001F54A\uFE0F","\U0001F985","\U0001F9A2","\U0001F989","\U0001F99A","\U0001F99C","\U0001F438","\U0001F40A","\U0001F422","\U0001F98E","\U0001F40D","\U0001F409","\U0001F995","\U0001F996","\U0001F433","\U0001F42C","\U0001F41F","\U0001F420","\U0001F988","\U0001F419","\U0001F41A","\U0001F980","\U0001F99E","\U0001F990","\U0001F991","\U0001F40C","\U0001F98B","\U0001F41B","\U0001F41C","\U0001F41D","\U0001F41E","\U0001F997","\U0001F577\uFE0F","\U0001F338","\U0001F339","\U0001F940","\U0001F33A","\U0001F33B","\U0001F337","\U0001F331","\U0001F332","\U0001F333","\U0001F334","\U0001F335","\U0001F33E","\U0001F33F","\u2618\uFE0F","\U0001F341","\U0001F342","\U0001F343","\U0001F30D","\U0001F9ED","\U0001F303","\U0001F304","\U0001F305","\U0001F309","\U0001F319","\u2600\uFE0F","\u2B50","\U0001F31F","\U0001F320","\u2604\uFE0F","\U0001F30C","\u26C5","\u26C8\uFE0F","\U0001F324\uFE0F","\u26C6","\U0001F32A\uFE0F","\U0001F300","\U0001F308","\U0001F302","\u2614","\u26F1\uFE0F","\u26A1","\u2744\uFE0F","\u26C4","\U0001F30A","\U0001F30B","\u231B","\u23F0","\u23F1\uFE0F","\U0001F570\uFE0F","\U0001F950","\U0001F968","\U0001F32E","\U0001F958","\U0001F963","\U0001F35C","\U0001F96E","\U0001F361","\U0001F95F","\U0001F960","\U0001F961","\U0001F366","\U0001F367","\U0001F368","\U0001F369","\U0001F36A","\U0001F382","\U0001F370","\U0001F9C1","\U0001F967","\U0001F36B","\U0001F36C","\U0001F36D","\U0001F36F","\U0001F347","\U0001F348","\U0001F349","\U0001F34C","\U0001F34D","\U0001F34F","\U0001F350","\U0001F351","\U0001F352","\U0001F353","\U0001F95D","\U0001F345","\U0001F965","\U0001F955","\U0001F33D","\U0001F336\uFE0F","\U0001F96C","\U0001F966","\U0001F344","\U0001F95C","\U0001F330","\U0001F37C","\U0001F95B","\u2615","\U0001F375","\U0001F376","\U0001F37E","\U0001F377","\U0001F378","\U0001F379","\U0001F37B","\U0001F942","\U0001F3FA","\U0001F484","\U0001F9F4","\U0001F9EF","\U0001F52E","\U0001F6D2","\U0001F6CE\uFE0F","\U0001F9F3","\U0001F6AA","\U0001F6CB\uFE0F","\U0001F6C1","\U0001F9F7","\U0001F9F9","\u26F2","\U0001F3D5\uFE0F","\U0001F3DC\uFE0F","\U0001F692","\U0001F691","\U0001F69A","\U0001F69C","\U0001F6B2","\U0001F6F4","\U0001F6F9","\U0001F6F5","\u26F5","\U0001F6F6","\u2708\uFE0F","\U0001F681","\U0001F6A0","\U0001F6F0\uFE0F","\U0001F680","\U0001F6F8","\u2693","\u267B\uFE0F", "\U0001F4BC","\U0001F5D2\uFE0F","\U0001F4CB","\u270F\uFE0F","\U0001F58B\uFE0F","\U0001F58C\uFE0F","\U0001F4DD","\U0001F4CC","\U0001F4D0","\U0001F4F1","\U0001F56F\uFE0F","\U0001F4A1","\U0001F526","\U0001F9EF","\U0001F9F0","\U0001F9F2","\u2699\uFE0F","\U0001F528","\u26CF\uFE0F","\u2692\uFE0F","\U0001F6E0\uFE0F","\U0001F3F9","\U0001F6E1\uFE0F","\u2697\uFE0F","\U0001F9EA","\U0001F9EB","\U0001F9EC","\U0001F52C","\U0001F52D","\U0001F4E1","\U0001F5DD\uFE0F","\U0001F4F5","\U0001F4F8","\U0001F4FA","\U0001F4FC","\U0001F4FD\uFE0F","\U0001F4FE","\U0001F5A8\uFE0F","\U0001F50C"};
            }
        }

        private void LoadLocalSettings()
        {
            var appSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            bool? storedGazePlusSwith = appSettings.Values[nameof(_gazePlusSwitch)] as bool?;
            if (storedGazePlusSwith != null)
            {
                _gazePlusSwitch = (bool)storedGazePlusSwith;

            }
            else
            {
                _gazePlusSwitch = false;
            }
            GazeInput.SetIsSwitchEnabled(this, _gazePlusSwitch);

            string symbolset = appSettings.Values[nameof(_symbolCollection)] as string ;

            SetSymbolCollection(SymbolSets.EmojiCollection1);
            string[] symbolsets = Enum.GetNames(typeof(SymbolSets));
            for (int i =0; i< symbolsets.Length; i++)
            {
                if (symbolsets[i] == symbolset)
                {
                    SetSymbolCollection((SymbolSets)Enum.Parse(typeof(SymbolSets), symbolsets[i]));
                }
            }            
        }

        private void CoredWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            bool wasFunctionKey = false;
            switch (args.VirtualKey)
            {
                case VirtualKey.F1:
                    _boardSpeed = BoardSpeed.Slow;
                    _flashTimer.Interval = new TimeSpan(0, 0, 1);
                    wasFunctionKey = true;
                    break;

                case VirtualKey.F2:
                    _boardSpeed = BoardSpeed.Fast;
                    _flashTimer.Interval = new TimeSpan(0, 0, 0, 0, 600);
                    wasFunctionKey = true;
                    break;

                case VirtualKey.F3:
                    debugPage = -1;
                    DebugLoadBoardWithNextPageOfSymbols();
                    wasFunctionKey = true;
                    break;

                case VirtualKey.F4:
                    DebugLoadBoardWithNextPageOfSymbols();
                    wasFunctionKey = true;
                    break;

                case VirtualKey.F5:
                    RevealBoard();
                    wasFunctionKey = true;
                    break;                
            }

            if (!args.KeyStatus.WasKeyDown && !wasFunctionKey)
            {
                GazeInput.GetGazePointer(this).Click();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);            

            switch (e.Parameter.ToString())
            {
                case "16":                    
                    _boardRows = 4;
                    _boardColumns = 4;
                    break;

                case "24":
                    _boardRows = 4;
                    _boardColumns = 6;
                    break;

                case "36":
                    _boardRows = 6;
                    _boardColumns = 6;
                    break;

                case "66":                  
                    _boardRows = 6;
                    _boardColumns = 11;
                    break;

                default:
                    _boardRows = 4;
                    _boardColumns = 4;
                    break;
            }
        }

        private ScrollViewer getRootScrollViewer()
        {
            DependencyObject el = this;
            while (el != null && !(el is ScrollViewer))
            {
                el = VisualTreeHelper.GetParent(el);
            }

            return (ScrollViewer)el;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            getRootScrollViewer().Focus(FocusState.Programmatic);

            ArrangeBoardLayout();
            ResetBoard();
        }

        private void ArrangeBoardLayout()
        {
            int pageHeaderButtonCount = 3;
            buttonMatrix.Children.Clear();
            buttonMatrix.RowDefinitions.Clear();
            buttonMatrix.ColumnDefinitions.Clear();

            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log($"NewBoard-ETD:{GazeInput.IsDeviceAvailable}-{_boardRows},{_boardColumns}");

            for (int row = 0; row < _boardRows; row++)
            {
                buttonMatrix.RowDefinitions.Add(new RowDefinition());                
            }

            for (int column = 0; column < _boardColumns; column++)
            {
                buttonMatrix.ColumnDefinitions.Add(new ColumnDefinition());

            }

            for (int row = 0; row < _boardRows; row++)
            {
                for (int col = 0; col < _boardColumns; col++)
                {
                    var button = new Button();
                    button.Name = $"button_{row}_{col}";
                    button.SetValue(AutomationProperties.NameProperty, $"Card {row} {col}");
                    button.TabIndex = (pageHeaderButtonCount - 1) + (int)((row * _boardColumns) + col);                    
                    button.Click += OnButtonClick;
                    button.Style = Resources["ButtonStyle"] as Style;
                    if (_boardColumns > 20)
                    {
                        button.Margin = new Thickness(.5);                       
                    }
                    Grid.SetRow(button, row);
                    Grid.SetColumn(button, col);
                    buttonMatrix.Children.Add(button);
                    button.Content = CreateTargetEllipse();                    
                }
            }

            buttonMatrix.UpdateLayout();
        }

        private Ellipse CreateTargetEllipse()
        {
            var target = new Ellipse();
            target.Width = 10;
            target.Height = 10;
            target.HorizontalAlignment = HorizontalAlignment.Center;
            target.VerticalAlignment = VerticalAlignment.Center;
            target.Fill = _toolButtonBrush;

            return target;
        }

        private void AdjustFontSizes()
        {            
            var buttons = GetButtonList();
            var shortEdge = buttons[0].ActualHeight;
            if (shortEdge > buttons[0].ActualWidth)
            {
                shortEdge = buttons[0].ActualWidth;
            }

            var tb = new TextBlock();
            tb.FontFamily = buttons[0].FontFamily;
            tb.FontSize = shortEdge * 0.7;           

            if (_symbolFontfamily != null && _symbolFontfamily.Source=="Segoe Color Emoji")
            {
                tb.Text = "\u0067";
            }
            else
            {
                tb.Text = "\u2B1C";
            }

            tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            var height = tb.DesiredSize.Height;

            do
            {
                tb.FontSize -= 1;
                tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                height = tb.DesiredSize.Height;
            }
            while (height > shortEdge * 0.7);

            foreach (Button button in buttons)
            {
                button.FontSize = height;
            }
        }

        private void RepositionButtons()
        {
            var buttons = GetButtonList();
            foreach (Button button in buttons)
            {
                var btn1Visual = ElementCompositionPreview.GetElementVisual(button);                
            }
        }

        private void OnFlashTimerTick(object sender, object e)
        {
            _reverseAnimationActive = true;
           
            //Flip button visual
            var btn1Visual = ElementCompositionPreview.GetElementVisual(_firstButton);
            var btn2Visual = ElementCompositionPreview.GetElementVisual(_secondButton);
            var compositor = btn1Visual.Compositor;

            //Get a visual for the content
            var btn1Content = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(_firstButton, 0), 0);
            var btn1ContentVisual = ElementCompositionPreview.GetElementVisual(btn1Content as FrameworkElement);
            var btn2Content = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(_secondButton, 0), 0);
            var btn2ContentVisual = ElementCompositionPreview.GetElementVisual(btn2Content as FrameworkElement);

            var easing = compositor.CreateLinearEasingFunction();
            
            if (_reverseFlipBatchAnimation != null)
            {
                _reverseFlipBatchAnimation.Completed -= ReverseFlipBatchAnimation_Completed;
                _reverseFlipBatchAnimation.Dispose();
            }

            _reverseFlipBatchAnimation = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _reverseFlipBatchAnimation.Completed += ReverseFlipBatchAnimation_Completed;

            ScalarKeyFrameAnimation flipAnimation = compositor.CreateScalarKeyFrameAnimation();
            flipAnimation.InsertKeyFrame(0.000001f, 0);
            flipAnimation.InsertKeyFrame(0.999999f, -180, easing);
            flipAnimation.InsertKeyFrame(1f, 0);
            if (_boardSpeed == BoardSpeed.Slow)
            {
                flipAnimation.Duration = TimeSpan.FromMilliseconds(400);
            }
            else
            {
                flipAnimation.Duration = TimeSpan.FromMilliseconds(1);
            }
            
            flipAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            flipAnimation.IterationCount = 1;            
            btn1Visual.CenterPoint = new Vector3((float)(0.5 * _firstButton.ActualWidth), (float)(0.5f * _firstButton.ActualHeight), 0f);
            btn1Visual.RotationAxis = new Vector3(0.0f, 1f, 0f);
            btn2Visual.CenterPoint = new Vector3((float)(0.5 * _secondButton.ActualWidth), (float)(0.5f * _secondButton.ActualHeight), 0f);
            btn2Visual.RotationAxis = new Vector3(0.0f, 1f, 0f);

            ScalarKeyFrameAnimation appearAnimation = compositor.CreateScalarKeyFrameAnimation();
            appearAnimation.InsertKeyFrame(0.0f, 1);
            appearAnimation.InsertKeyFrame(0.399999f, 1);
            appearAnimation.InsertKeyFrame(0.4f, 0);
            appearAnimation.InsertKeyFrame(1f, 0);
            if (_boardSpeed == BoardSpeed.Slow)
            {
                appearAnimation.Duration = TimeSpan.FromMilliseconds(400);
            }
            else
            {
                appearAnimation.Duration = TimeSpan.FromMilliseconds(1);
            }

            appearAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            appearAnimation.IterationCount = 1;

            btn1Visual.StartAnimation(nameof(btn1Visual.RotationAngleInDegrees), flipAnimation);
            btn2Visual.StartAnimation(nameof(btn2Visual.RotationAngleInDegrees), flipAnimation);
            btn1ContentVisual.StartAnimation(nameof(btn1ContentVisual.Opacity), appearAnimation);
            btn2ContentVisual.StartAnimation(nameof(btn2ContentVisual.Opacity), appearAnimation);
            _reverseFlipBatchAnimation.End();
           
            _flashTimer.Stop();
            GazeInput.SetInteraction(buttonMatrix, Interaction.Enabled);
        }

        void MakeButtonContentOpaque(Button thisButton)
        {
            var thisVisual = ElementCompositionPreview.GetElementVisual(thisButton);            
            var compositor = thisVisual.Compositor;

            //Get a visual for the content
            var thisContent = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(thisButton, 0), 0);
            var thisContentVisual = ElementCompositionPreview.GetElementVisual(thisContent as FrameworkElement);
          
            ScalarKeyFrameAnimation appearAnimation = compositor.CreateScalarKeyFrameAnimation();
            appearAnimation.InsertKeyFrame(1f, 1);
            
            if (_boardSpeed == BoardSpeed.Slow)
            {
                appearAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            }
            else
            {
                appearAnimation.Duration = TimeSpan.FromMilliseconds(1);
            }

            appearAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            appearAnimation.IterationCount = 1;

            thisContentVisual.StartAnimation(nameof(thisContentVisual.Opacity), appearAnimation);            
        }

        private void ReverseFlipBatchAnimation_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {             
            _firstButton.Content = CreateTargetEllipse();
            _secondButton.Content = CreateTargetEllipse();
            MakeButtonContentOpaque(_firstButton);
            MakeButtonContentOpaque(_secondButton);
            _firstButton = null;
            _secondButton = null;
            _reverseAnimationActive = false;
        }

        private async void FlipCardFaceUp(Button btn)
        {
            //Flip button visual
            var btnVisual = ElementCompositionPreview.GetElementVisual(btn);
            var compositor = btnVisual.Compositor;

            GazeInput.DwellFeedbackProgressBrush = _transparentBrush;
            
            //Get a visual for the content
            var btnContent = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(btn, 0), 0);
            var btnContentVisual = ElementCompositionPreview.GetElementVisual(btnContent as FrameworkElement);

            var easing = compositor.CreateLinearEasingFunction();

            if (_flipBatchAnimation != null)
            {
                _flipBatchAnimation.Completed -= FlipBatchAnimation_Completed;
                _flipBatchAnimation.Dispose();
            }

            _flipBatchAnimation = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _flipBatchAnimation.Completed += FlipBatchAnimation_Completed;

            ScalarKeyFrameAnimation flipAnimation = compositor.CreateScalarKeyFrameAnimation();
            flipAnimation.InsertKeyFrame(0.000001f, 180);
            flipAnimation.InsertKeyFrame(1f, 0, easing);
            
            if (_boardSpeed == BoardSpeed.Slow)
            {
                flipAnimation.Duration = TimeSpan.FromMilliseconds(800);
            }
            else
            {
                flipAnimation.Duration = TimeSpan.FromMilliseconds(1);
            }

            flipAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            flipAnimation.IterationCount = 1;
            btnVisual.CenterPoint = new Vector3((float)(0.5 * btn.ActualWidth), (float)(0.5f * btn.ActualHeight), 0f);
            btnVisual.RotationAxis = new Vector3(0.0f, 1f, 0f);

            ScalarKeyFrameAnimation appearAnimation = compositor.CreateScalarKeyFrameAnimation();
            appearAnimation.InsertKeyFrame(0.0f, 0);
            appearAnimation.InsertKeyFrame(0.599999f, 0);
            appearAnimation.InsertKeyFrame(0.6f, 0.5f);
            appearAnimation.InsertKeyFrame(1f, 1);
            
            if (_boardSpeed == BoardSpeed.Slow)
            {
                appearAnimation.Duration = TimeSpan.FromMilliseconds(800);
            }
            else
            {
                appearAnimation.Duration = TimeSpan.FromMilliseconds(1);
            }

            appearAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            appearAnimation.IterationCount = 1;

            btnVisual.StartAnimation(nameof(btnVisual.RotationAngleInDegrees), flipAnimation);
            btnContentVisual.StartAnimation(nameof(btnContentVisual.Opacity), appearAnimation);
            _flipBatchAnimation.End();

            if (_usePictures)
            {
                var file = await StorageFile.GetFileFromPathAsync(btn.Tag.ToString());
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var image = new Image();
                    var bmp = new BitmapImage();
                    await bmp.SetSourceAsync(stream);
                    image.Source = bmp;
                    btn.Content = image;
                }
            }
            else
            {
                TextBlock textBlock = new TextBlock();
                textBlock.FontFamily = _symbolFontfamily;
                textBlock.IsColorFontEnabled = false;
                textBlock.Padding = new Thickness(0);
                textBlock.Margin = new Thickness(0,10,0,0);
               
                textBlock.Text = btn.Tag.ToString();
                btn.Content = textBlock;                
            }
        }

        private void FlipCardFaceDown(Button card)
        {
            if (card.Content == null  || card.Content is Ellipse) return;            

            //Flip button visual
            var btn1Visual = ElementCompositionPreview.GetElementVisual(card);            
            var compositor = btn1Visual.Compositor;

            //Get a visual for the content
            var btn1Content = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(card, 0), 0);
            var btn1ContentVisual = ElementCompositionPreview.GetElementVisual(btn1Content as FrameworkElement);

            var easing = compositor.CreateLinearEasingFunction();

            TimeSpan rndDelay = TimeSpan.FromMilliseconds(_rnd.NextDouble() * 200);

            ScalarKeyFrameAnimation flipAnimation = compositor.CreateScalarKeyFrameAnimation();
            flipAnimation.InsertKeyFrame(0.000001f, 0);
            flipAnimation.InsertKeyFrame(0.999999f, 180, easing);
            flipAnimation.InsertKeyFrame(1f, 0);
            
            if (_boardSpeed == BoardSpeed.Slow)
            {
                flipAnimation.Duration = TimeSpan.FromMilliseconds(400);
            }
            else
            {
                flipAnimation.Duration = TimeSpan.FromMilliseconds(1);
            }

            flipAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            flipAnimation.IterationCount = 1;            
            btn1Visual.CenterPoint = new Vector3((float)(0.5 * card.ActualWidth), (float)(0.5f * card.ActualHeight), 0.0f);
            btn1Visual.RotationAxis = new Vector3(0.0f, 1f, 0f);
            flipAnimation.DelayTime = rndDelay;

            ScalarKeyFrameAnimation appearAnimation = compositor.CreateScalarKeyFrameAnimation();
            appearAnimation.InsertKeyFrame(0.0f, 1);
            appearAnimation.InsertKeyFrame(0.399999f, 1);
            appearAnimation.InsertKeyFrame(0.4f, 0);
            appearAnimation.InsertKeyFrame(1f, 0);
            
            if (_boardSpeed == BoardSpeed.Slow)
            {
                appearAnimation.Duration = TimeSpan.FromMilliseconds(400);
            }
            else
            {
                appearAnimation.Duration = TimeSpan.FromMilliseconds(1);
            }

            appearAnimation.IterationBehavior = AnimationIterationBehavior.Count;
            appearAnimation.IterationCount = 1;
            appearAnimation.DelayTime = rndDelay;

            btn1Visual.StartAnimation(nameof(btn1Visual.RotationAngleInDegrees), flipAnimation);            
            btn1ContentVisual.StartAnimation(nameof(btn1ContentVisual.Opacity), appearAnimation);                        
        }

        List<Button> ShuffleList(List<Button> list)
        {
            var len = list.Count;
            var repeatCount = _rnd.Next(1, 5);
            for (var repeat = 0; repeat < repeatCount; repeat++)
            {
                for (var i = 0; i < len; i++)
                {
                    var j = _rnd.Next(0, len);
                    var k = list[i];
                    list[i] = list[j];
                    list[j] = k;
                }
            }
            return list;
        }

        async Task<List<string>> GetPicturesContent(int len)
        {
            StorageFolder picturesFolder = KnownFolders.PicturesLibrary;            
            IReadOnlyList<StorageFile> pictures = await picturesFolder.GetFilesAsync();
            List<string> list = new List<string>();

            for (int i = 0; i < len; i++)
            {
                string filename;
                do
                {
                    filename = pictures[_rnd.Next(pictures.Count)].Path;
                }
                while (list.Contains(filename));

                list.Add(filename.ToString());
            }
            return list;
        }                

        List<string> GetSymbolContent(int len)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < len; i++)
            {
                string ch;
                do
                {                                                
                    ch = _symbolCollection[_rnd.Next(_symbolCollection.Length)];
                }
                while (list.Contains(ch.ToString()));

                list.Add(ch.ToString());
            }
            return list;
        }

        List<string> DebugGetNextPageOfSymbolContent(int len, int fromIndex)
        {
            int symbolIndex;

            List<string> list = new List<string>();
            for (int i = 0; i < len; i++)
            {
                symbolIndex = fromIndex + i;
                string ch;
                if (symbolIndex < _symbolCollection.Length)
                {                                                                               
                    ch = _symbolCollection[symbolIndex];                   
                }
                else
                {
                    ch = "";
                }
                list.Add(ch.ToString());
            }
            return list;
        }

        List<Button> GetButtonList()
        {
            List<Button> list = new List<Button>();            
            foreach (UIElement button in buttonMatrix.Children)
            {
                if (button is Button)
                {
                    list.Add(button as Button);
                }                
            }
            return list;            
        }
      
        async void ResetBoard()
        {
            PlayAgainText.Visibility = Visibility.Collapsed;
            _gameOver = false;
            _firstButton = null;
            _secondButton = null;
            _numMoves = 0;
            MoveCountTextBlock.Text = _numMoves.ToString();            
            _remaining = _boardRows * _boardColumns;
            var pairs = (_boardRows * _boardColumns) / 2;

            List<string> listContent;
            if (_usePictures)
            {
                try
                {                
                    listContent = await GetPicturesContent(pairs);
                }
                catch
                {
                    listContent = GetSymbolContent(pairs);
                    _usePictures = false;
                }
            }
            else
            {                
                listContent = GetSymbolContent(pairs);
            }

            List<Button> listButtons = ShuffleList(GetButtonList());

            var compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            if (_resetBatchAnimation != null)
            {
                _resetBatchAnimation.Completed -= ResetBatchAnimation_Completed;
                _resetBatchAnimation.Dispose();
            }

            _resetBatchAnimation = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _resetBatchAnimation.Completed += ResetBatchAnimation_Completed; ;

            foreach (Button button in listButtons)
            {
                FlipCardFaceDown(button);
                GazeInput.SetInteraction(button, Interaction.Inherited);                
            }
            _resetBatchAnimation.End();

            for (int i = 0; i < _boardRows * _boardColumns; i += 2)
            {
              
                listButtons[i].Tag = listContent[i / 2];
                listButtons[i + 1].Tag = listContent[i / 2];
            }
        }

        static int debugPage=-1;
        void DebugLoadBoardWithNextPageOfSymbols()
        {
            debugPage += 1;

            PlayAgainText.Visibility = Visibility.Collapsed;
            _gameOver = false;
            _firstButton = null;
            _secondButton = null;
            _numMoves = 0;
            MoveCountTextBlock.Text = _numMoves.ToString();            
            var cards = (_boardRows * _boardColumns);

            List<string> listContent;
                      
            listContent = DebugGetNextPageOfSymbolContent(cards,debugPage * cards);
           
            List<Button> listButtons = GetButtonList();

            foreach (Button button in listButtons)
            {
                FlipCardFaceDown(button);
                GazeInput.SetInteraction(button, Interaction.Inherited);
            }

            for (int i = 0; i < _boardRows * _boardColumns; i += 1)
            {
                listButtons[i].Tag = listContent[i ];                
            }

            RevealBoard();

            //Start over if last page            
            foreach (string s in listContent)
            {
                if (s == "")
                {
                    debugPage = -1;
                    break;
                } 
            }            
        }

        private void ResetBatchAnimation_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            List<Button> listButtons = GetButtonList();

            for (int i = 0; i < _boardRows * _boardColumns; i += 2)
            {                              
                listButtons[i].Content = CreateTargetEllipse();
                listButtons[i + 1].Content = CreateTargetEllipse();
                MakeButtonContentOpaque(listButtons[i]);
                MakeButtonContentOpaque(listButtons[i + 1]);
            }
            ApplyPerspective();
            AdjustFontSizes();
        }

        private void ApplyPerspective()
        {            
            //Apply the perspective to the environment

            var pageVisual = ElementCompositionPreview.GetElementVisual(this);            
            Vector2 pageSize = new Vector2((float)this.ActualWidth, (float)this.ActualHeight);

            Matrix4x4 perspective = new Matrix4x4(

                                    1.0f, 0f, 0f, 0,
                                      0f, 1f, 0f, 0,
                                      0f, 0f, 1f, 1/pageSize.X,
                                      0f, 0f, 0f, 1f);

            pageVisual.TransformMatrix =
                            Matrix4x4.CreateTranslation(-pageSize.X / 2, -pageSize.Y / 2, 0f) *  // Translate to origin
                            perspective *                                                        // Apply perspective at origin
                            Matrix4x4.CreateTranslation(pageSize.X / 2, pageSize.Y / 2, 0f);     // Translate back to original position
        }

        private void ClearBoard()
        {
            List<Button> listButtons = GetButtonList();
            foreach (Button button in listButtons)
            {                
                FlipCardFaceDown(button);
            }
        }

        private void RevealBoard()  //For debuging purposes
        {
            List<Button> listButtons = GetButtonList();
            foreach (Button button in listButtons)
            {
                FlipCardFaceUp(button);
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (_animationActive || _reverseAnimationActive || _interactionPaused) return;

            if (_gameOver)
            {                
                return;
            }

            var btn = sender as Button;
            if (!(btn.Content is Ellipse) && btn.Content != null)
            {             
                return;
            }          

            if (_flashTimer.IsEnabled)
            {
                return;
                //OnFlashTimerTick(null, null);  //Unclear about the original idea of this
            }

            _animationActive = true;
            _numMoves++;
            MoveCountTextBlock.Text = _numMoves.ToString();

            if (_firstButton == null)
            {
                _firstButton = btn;
            }
            else
            {
                _secondButton = btn;
            }

            FlipCardFaceUp(btn);

        }

        private void FlipBatchAnimation_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            GazeInput.DwellFeedbackProgressBrush = _whiteBrush;

            _animationActive = false;

            if (_secondButton == null)
            {
                return;
            }

            if (_secondButton.Tag.ToString() != _firstButton.Tag.ToString())
            {
                GazeInput.SetInteraction(buttonMatrix, Interaction.Disabled);
                _flashTimer.Start();            
            }
            else            
            {
                //Do Match confirmation animation

                PulseButton(_firstButton);
                PulseButton(_secondButton);

           
                _firstButton = null;
                _secondButton = null;
                _remaining -= 2;

                CheckGameCompletion();
            }
        }

        void PulseButton(Button button)
        {
            var btnVisual = ElementCompositionPreview.GetElementVisual(button);
            var compositor = btnVisual.Compositor;
            var springSpeed = 50;

            btnVisual.CenterPoint = new System.Numerics.Vector3((float)button.ActualWidth / 2, (float)button.ActualHeight / 2, 0f);

            var scaleAnimation = compositor.CreateSpringVector3Animation();
            scaleAnimation.InitialValue = new System.Numerics.Vector3(0.9f, 0.9f, 0f);
            scaleAnimation.FinalValue = new System.Numerics.Vector3(1.0f, 1.0f, 0f);
            scaleAnimation.DampingRatio = 0.4f;
            scaleAnimation.Period = TimeSpan.FromMilliseconds(springSpeed);

            btnVisual.StartAnimation(nameof(btnVisual.Scale), scaleAnimation);
        }

        async void  CheckGameCompletion()
        {
            if (_remaining > 0)
            {
                return;
            }
            _gameOver = true;

            //Pulse entire board
            List<Button> listButtons = GetButtonList();
            foreach (Button button in listButtons)
            {
                PulseButton(button);
            }

            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log($"AllCards{_boardRows * _boardColumns}-ETD:{GazeInput.IsDeviceAvailable}-m#{_numMoves}");

            await Task.Delay(1000);

            //string message = $"You matched the {_boardRows * _boardColumns} cards in {_numMoves} moves!";
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();            
            string message = String.Format(resourceLoader.GetString("CongratsDetailMessage"), _boardRows* _boardColumns,  _numMoves);

            DialogText.Text = message;
            GazeInput.DwellFeedbackProgressBrush = _solidTileBrush;
            DialogGrid.Visibility = Visibility.Visible;
            SetTabsForDialogView();
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = _whiteBrush;
            DialogGrid.Visibility = Visibility.Collapsed;
            ResetBoard();
            SetTabsForPageView();
        }

        private async void DialogButton2_Click(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = _whiteBrush;
            DialogGrid.Visibility = Visibility.Collapsed;
            ClearBoard();
            await Task.Delay(400);
            Frame.Navigate(typeof(MainPage));
        }


        private void DismissButton(object sender, RoutedEventArgs e)
        {
            GazeInput.DwellFeedbackProgressBrush = _whiteBrush;
            DialogGrid.Visibility = Visibility.Collapsed;

            //RootGrid.Children.Remove(PlayAgainButton);
            //Grid.SetColumn(PlayAgainButton, Grid.GetColumn(_buttons[_boardSize - 1, _boardSize - 1]));
            //Grid.SetRow(PlayAgainButton, Grid.GetRow(_buttons[_boardSize - 1, _boardSize - 1]));
            //PlayAgainButton.MaxWidth = _buttons[_boardSize - 1, _boardSize - 1].ActualWidth;
            //PlayAgainButton.MaxHeight = _buttons[_boardSize - 1, _boardSize - 1].ActualHeight;
            //GameGrid.Children.Add(PlayAgainButton);
            //PlayAgainButton.Visibility = Visibility.Visible;

            PlayAgainText.Visibility = Visibility.Visible;
            OnPause(PauseButton, null);
            SetTabsForPageView();
            PauseButton.Focus(FocusState.Pointer);
        }


        private void OnPause(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (_interactionPaused)
            {
                PauseButtonText.Text = "\uE769";
                PauseButtonBorder.Background = _toolButtonBrush;
                GazeInput.SetInteraction(buttonMatrix, Interaction.Enabled);
                _interactionPaused = false;
                if (_gameOver)
                {
                    ResetBoard();
                }
            }
            else
            {
                PauseButtonText.Text = "\uE768";
                PauseButtonBorder.Background = _pausedButtonBrush;
                GazeInput.SetInteraction(buttonMatrix, Interaction.Disabled);

                if (e != null)
                {
                    //User initiated pause, not code
                    StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
                    logger.Log($"Paused-ETD:{GazeInput.IsDeviceAvailable}");
                }
                _interactionPaused = true;
            }
        }

        private async void OnExit(object sender, RoutedEventArgs e)
        {
            if (((App)Application.Current).KioskActivation)
            {
                var uri = new Uri("eyes-first-app:");
                var ret = await Launcher.LaunchUriAsync(uri);
            }

            Application.Current.Exit();
        }

        private async void OnBack(object sender, RoutedEventArgs e)
        {
            ClearBoard();
            await Task.Delay(400);
            Frame.Navigate(typeof(MainPage));
        }
        
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustFontSizes();
            RepositionButtons();
        }

        private void DialogGrid_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                DismissButton(null, null);
            }
        }

        private void SetTabsForDialogView()
        {
            BackButton.IsTabStop = false;
            PauseButton.IsTabStop = false;            
            ExitButton.IsTabStop = false;

            for (int row = 0; row < _boardRows; row++)
            {
                for (int col = 0; col < _boardColumns; col++)
                {
                    string buttonName = $"button_{row}_{col}";
                    Button button = FindName(buttonName) as Button;
                    button.IsTabStop = false;
                }
            }
        }

        private void SetTabsForPageView()
        {
            BackButton.IsTabStop = true;
            PauseButton.IsTabStop = true;            
            ExitButton.IsTabStop = true;
            for (int row = 0; row < _boardRows; row++)
            {
                for (int col = 0; col < _boardColumns; col++)
                {
                    string buttonName = $"button_{row}_{col}";
                    Button button = FindName(buttonName) as Button;
                    button.IsTabStop = true;
                }
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            CoreWindow.GetForCurrentThread().KeyDown -= CoredWindow_KeyDown;
        }       
    }
}

