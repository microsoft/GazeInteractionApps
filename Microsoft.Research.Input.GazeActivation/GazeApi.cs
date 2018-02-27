using Microsoft.Research.Input.Gaze;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Research.Input.Gaze
{
    public sealed class GazeApi
    {
        internal static TimeSpan s_nonTimeSpan = new TimeSpan(-123456);

        static DependencyProperty s_gazePointerProperty = DependencyProperty.RegisterAttached("_GazePointer", typeof(GazePointer), typeof(Page), new PropertyMetadata(null));

        static DependencyProperty IsGazeEnabledProperty = DependencyProperty.RegisterAttached("IsGazeEnabled", typeof(bool), typeof(Page),
             new PropertyMetadata(false, OnIsGazeEnabledChanged));

        private static void OnIsGazeEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var isGazeEnabled = (bool)e.NewValue;
            if (isGazeEnabled)
            {
                var page = (Page)d;

                var gazePointer = (GazePointer)d.GetValue(s_gazePointerProperty);
                if (gazePointer == null)
                {
                    gazePointer = new GazePointer(page);
                    d.SetValue(s_gazePointerProperty, gazePointer);

                    gazePointer.IsCursorVisible = (bool)d.GetValue(GazeApi.IsGazeCursorVisibleProperty);
                }
            }
            else
            {
                // TODO: Turn off GazePointer
            }
        }

        static DependencyProperty IsGazeCursorVisibleProperty = DependencyProperty.RegisterAttached("IsGazeCursorVisible", typeof(bool), typeof(Page),
            new PropertyMetadata(true, OnIsGazeCursorVisibleChanged));

        private static void OnIsGazeCursorVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var gazePointer = (GazePointer)d.GetValue(s_gazePointerProperty);
            if (gazePointer != null)
            {
                gazePointer.IsCursorVisible = (bool)e.NewValue;
            }
        }

        static readonly DependencyProperty s_gazePageProperty = DependencyProperty.RegisterAttached("GazePage", typeof(Page), typeof(Page), new PropertyMetadata(null));
        static readonly DependencyProperty s_gazeElementProperty = DependencyProperty.RegisterAttached("GazeElement", typeof(GazeElement), typeof(UIElement), new PropertyMetadata(null));
        static readonly DependencyProperty s_fixationProperty = DependencyProperty.RegisterAttached("Fixation", typeof(TimeSpan), typeof(UIElement), new PropertyMetadata(s_nonTimeSpan));
        static readonly DependencyProperty s_dwellProperty = DependencyProperty.RegisterAttached("Dwell", typeof(TimeSpan), typeof(UIElement), new PropertyMetadata(s_nonTimeSpan));
        static readonly DependencyProperty s_dwellRepeatProperty = DependencyProperty.RegisterAttached("DwellRepeat", typeof(TimeSpan), typeof(UIElement), new PropertyMetadata(s_nonTimeSpan));
        static readonly DependencyProperty s_enterProperty = DependencyProperty.RegisterAttached("Enter", typeof(TimeSpan), typeof(UIElement), new PropertyMetadata(s_nonTimeSpan));
        static readonly DependencyProperty s_exitProperty = DependencyProperty.RegisterAttached("Exit", typeof(TimeSpan), typeof(UIElement), new PropertyMetadata(s_nonTimeSpan));

        public static DependencyProperty GazePageProperty { get { return s_gazePageProperty; } }
        public static DependencyProperty GazeElementProperty { get { return s_gazeElementProperty; } }
        public static DependencyProperty FixationProperty { get { return s_fixationProperty; } }
        public static DependencyProperty DwellProperty { get { return s_dwellProperty; } }
        public static DependencyProperty DwellRepeatProperty { get { return s_dwellRepeatProperty; } }
        public static DependencyProperty EnterProperty { get { return s_enterProperty; } }
        public static DependencyProperty ExitProperty { get { return s_exitProperty; } }

        public static bool GetIsGazeEnabled(Page page) { return (bool)page.GetValue(IsGazeEnabledProperty); }
        public static bool GetIsGazeCursorVisible(Page page) { return (bool)page.GetValue(IsGazeCursorVisibleProperty); }
        public static GazePage GetGazePage(Page page) { return (GazePage)page.GetValue(GazePageProperty); }
        public static GazeElement GetGazeElement(UIElement element) { return (GazeElement)element.GetValue(GazeElementProperty); }
        public static TimeSpan GetFixation(UIElement element) { return (TimeSpan)element.GetValue(FixationProperty); }
        public static TimeSpan GetDwell(UIElement element) { return (TimeSpan)element.GetValue(DwellProperty); }
        public static TimeSpan GetDwellRepeat(UIElement element) { return (TimeSpan)element.GetValue(DwellRepeatProperty); }
        public static TimeSpan GetEnter(UIElement element) { return (TimeSpan)element.GetValue(EnterProperty); }
        public static TimeSpan GetExit(UIElement element) { return (TimeSpan)element.GetValue(ExitProperty); }

        public static void SetIsGazeEnabled(Page page, bool value) { page.SetValue(IsGazeEnabledProperty, value); }
        public static void SetIsGazeCursorVisible(Page page, bool value) { page.SetValue(IsGazeCursorVisibleProperty, value); }
        public static void SetGazePage(Page page, GazePage value) { page.SetValue(GazePageProperty, value); }
        public static void SetGazeElement(UIElement element, GazeElement value) { element.SetValue(GazeElementProperty, value); }
        public static void SetFixation(UIElement element, TimeSpan span) { element.SetValue(FixationProperty, span); }
        public static void SetDwell(UIElement element, TimeSpan span) { element.SetValue(DwellProperty, span); }
        public static void SetDwellRepeat(UIElement element, TimeSpan span) { element.SetValue(DwellRepeatProperty, span); }
        public static void SetEnter(UIElement element, TimeSpan span) { element.SetValue(EnterProperty, span); }
        public static void SetExit(UIElement element, TimeSpan span) { element.SetValue(ExitProperty, span); }
    }
}
