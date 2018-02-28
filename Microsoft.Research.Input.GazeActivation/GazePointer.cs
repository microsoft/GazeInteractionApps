using Microsoft.Research.Input.Gaze;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Devices.Input.Preview;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Microsoft.Research.Input.Gaze
{
    public class GazePointer
    {
        // units in microseconds
        const int DEFAULT_FIXATION_DELAY = 400000;
        const int DEFAULT_DWELL_DELAY = 800000;
        const int DEFAULT_REPEAT_DELAY = int.MaxValue;
        const int DEFAULT_ENTER_EXIT_DELAY = 50000;
        const int DEFAULT_MAX_HISTORY_DURATION = 3000000;
        const int MAX_SINGLE_SAMPLE_DURATION = 100000;

        const int GAZE_IDLE_TIME = 2500000;

        readonly static DependencyProperty GazeTargetItemProperty = DependencyProperty.RegisterAttached("GazeTargetItem", typeof(GazeTargetItem), typeof(UIElement), new PropertyMetadata(null));

        internal GazePointer(UIElement root)
        {
            _rootElement = root;
            _coreDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            // Default to not filtering sample data
            Filter = new NullFilter();

            _gazeCursor = GazeCursor.Instance;

            // timer that gets called back if there gaze samples haven't been received in a while
            _eyesOffTimer = new DispatcherTimer();
            _eyesOffTimer.Tick += OnEyesOff;

            // provide a default of GAZE_IDLE_TIME microseconds to fire eyes off 
            EyesOffDelay = GAZE_IDLE_TIME;

            InitializeHistogram();
            InitializeGazeInputSource();
        }

        ~GazePointer()
        {
            if (_gazeInputSource != null)
            {
                _gazeInputSource.GazeMoved -= OnGazeMoved;
            }
        }

        GazeIsInvokableDelegate IsInvokableImpl
        {
            get
            {
                return _isInvokableImpl;
            }
            set
            {
                _isInvokableImpl = value;
            }
        }

        GazeInvokeTargetDelegate InvokeTargetImpl
        {
            get
            {
                return _invokeTargetImpl;
            }
            set
            {
                _invokeTargetImpl = value;
            }
        }

        void InvokeTarget(UIElement target)
        {
            if (target == _rootElement)
            {
                return;
            }

            var control = target as Control;
            if ((control == null) || (!control.IsEnabled))
            {
                return;
            }

            if (InvokeTargetImpl != null)
            {
                InvokeTargetImpl(target);
            }
            else
            {
                var button = control as Button;
                if (button != null)
                {
                    var peer = new ButtonAutomationPeer(button);
                    peer.Invoke();
                    return;
                }

                var toggleButton = control as ToggleButton;
                if (toggleButton != null)
                {
                    var peer = new ToggleButtonAutomationPeer(toggleButton);
                    peer.Toggle();
                    return;
                }

                var toggleSwitch = control as ToggleSwitch;
                if (toggleSwitch != null)
                {
                    var peer = new ToggleSwitchAutomationPeer(toggleSwitch);
                    peer.Toggle();
                    return;
                }

                var textBox = control as TextBox;
                if (textBox != null)
                {
                    var peer = new TextBoxAutomationPeer(textBox);
                    peer.SetFocus();
                    return;
                }

                var pivot = control as Pivot;
                if (pivot != null)
                {
                    var peer = new PivotAutomationPeer(pivot);
                    peer.SetFocus();
                    return;
                }
            }
        }

        void Reset()
        {
            _activeHitTargetTimes.Clear();
            _gazeHistory.Clear();
        }

        void SetElementStateDelay(UIElement element, GazePointerState relevantState, int stateDelay)
        {
            var property = GetProperty(relevantState);
            Object delay = TimeSpan.FromMinutes(stateDelay);
            element.SetValue(property, delay);

            // fix up _maxHistoryTime in case the new param exceeds the history length we are currently tracking
            int dwellTime = GetElementStateDelay(element, GazePointerState.Dwell);
            int repeatTime = GetElementStateDelay(element, GazePointerState.DwellRepeat);
            if (repeatTime != int.MaxValue)
            {
                _maxHistoryTime = Math.Max(2 * repeatTime, _maxHistoryTime);
            }
            else
            {
                _maxHistoryTime = Math.Max(2 * dwellTime, _maxHistoryTime);
            }
        }
        int GetElementStateDelay(UIElement element, GazePointerState pointerState)
        {
            TimeSpan delay;

            var property = GetProperty(pointerState);

            DependencyObject elementWalker = element;
            do
            {
                if (elementWalker == null)
                {
                    delay = GetDefaultPropertyValue(pointerState);
                }
                else
                {
                    var ob = element.GetValue(property);
                    delay = (TimeSpan)ob;
                    elementWalker = VisualTreeHelper.GetParent(elementWalker);
                }
            } while (delay == GazeApi.s_nonTimeSpan);

            return (int)(1000.0 * delay.TotalMilliseconds);
        }

        // Provide a configurable delay for when the EyesOffDelay event is fired
        // GOTCHA: this value requires that _eyesOffTimer is instantiated so that it
        // can update the timer interval 
        long EyesOffDelay
        {
            get { return _eyesOffDelay; }
            set
            {
                _eyesOffDelay = value;

                // convert GAZE_IDLE_TIME units (microseconds) to 100-nanosecond units used
                // by TimeSpan struct
                _eyesOffTimer.Interval = new TimeSpan(EyesOffDelay * 10);
            }
        }

        // Pluggable filter for eye tracking sample data. This defaults to being set to the
        // NullFilter which performs no filtering of input samples.
        IGazeFilter Filter { get; set; }

        internal bool IsCursorVisible
        {
            get { return _gazeCursor.IsCursorVisible; }
            set { _gazeCursor.IsCursorVisible = value; }
        }

        int CursorRadius
        {
            get { return _gazeCursor.CursorRadius; }
            set { _gazeCursor.CursorRadius = value; }
        }

        void InitializeHistogram()
        {
            _activeHitTargetTimes = new List<GazeTargetItem>();

            _offScreenElement = new UserControl();
            SetElementStateDelay(_offScreenElement, GazePointerState.Fixation, DEFAULT_FIXATION_DELAY);
            SetElementStateDelay(_offScreenElement, GazePointerState.Dwell, DEFAULT_DWELL_DELAY);

            _maxHistoryTime = DEFAULT_MAX_HISTORY_DURATION;    // maintain about 3 seconds of history (in microseconds)
            _gazeHistory = new List<GazeHistoryItem>();
        }

        void InitializeGazeInputSource()
        {
            _gazeInputSource = GazeInputSourcePreview.GetForCurrentView();
            if (_gazeInputSource != null)
            {
                _gazeInputSource.GazeMoved += OnGazeMoved;
            }
        }

        GazeTargetItem GetOrCreateGazeTargetItem(UIElement element)
        {
            var target = (GazeTargetItem)element.GetValue(GazeTargetItemProperty);
            if (target == null)
            {
                target = new GazeTargetItem(element);
                element.SetValue(GazeTargetItemProperty, target);
            }

            var index = _activeHitTargetTimes.IndexOf(target);
            if (index == -1)
            {
                _activeHitTargetTimes.Add(target);

                // calculate the time that the first DwellRepeat needs to be fired after. this will be updated every time a DwellRepeat is 
                // fired to keep track of when the next one is to be fired after that.
                int nextStateTime = GetElementStateDelay(element, GazePointerState.Enter);

                target.Reset(nextStateTime);
            }

            return target;
        }
        GazeTargetItem GetGazeTargetItem(UIElement element)
        {
            var target = (GazeTargetItem)element.GetValue(GazeTargetItemProperty);
            return target;
        }

        UIElement GetHitTarget(Point gazePoint)
        {
            var targets = VisualTreeHelper.FindElementsInHostCoordinates(gazePoint, _rootElement, false);
            foreach (var target in targets)
            {
                if (IsInvokable(target))
                {
                    return target;
                }
            }
            // TODO : Check if the location is offscreen
            return _rootElement;
        }

        UIElement ResolveHitTarget(Point gazePoint, long timestamp)
        {
            // create GazeHistoryItem to deal with this sample
            var historyItem = new GazeHistoryItem();
            historyItem.HitTarget = GetHitTarget(gazePoint);
            historyItem.Timestamp = timestamp;
            historyItem.Duration = 0;
            Debug.Assert(historyItem.HitTarget != null);

            // create new GazeTargetItem with a (default) total elapsed time of zero if one does not exist already.
            // this ensures that there will always be an entry for target elements in the code below.
            var target = GetOrCreateGazeTargetItem(historyItem.HitTarget);
            target.LastTimestamp = timestamp;

            // just append to the list and return if the list is empty
            if (_gazeHistory.Count == 0)
            {
                _gazeHistory.Add(historyItem);
                return historyItem.HitTarget;
            }

            // find elapsed time since we got the last hit target
            historyItem.Duration = (int)(timestamp - _gazeHistory[_gazeHistory.Count - 1].Timestamp);
            if (historyItem.Duration > MAX_SINGLE_SAMPLE_DURATION)
            {
                historyItem.Duration = MAX_SINGLE_SAMPLE_DURATION;
            }
            _gazeHistory.Add(historyItem);

            // update the time this particular hit target has accumulated
            target.ElapsedTime += historyItem.Duration;


            // drop the oldest samples from the list until we have samples only 
            // within the window we are monitoring
            //
            // historyItem is the last item we just appended a few lines above. 
            while (historyItem.Timestamp - _gazeHistory[0].Timestamp > _maxHistoryTime)
            {
                var evOldest = _gazeHistory[0];
                _gazeHistory.RemoveAt(0);

                Debug.Assert(GetGazeTargetItem(evOldest.HitTarget).ElapsedTime - evOldest.Duration >= 0);

                // subtract the duration obtained from the oldest sample in _gazeHistory
                var targetItem = GetGazeTargetItem(evOldest.HitTarget);
                targetItem.ElapsedTime -= evOldest.Duration;

                if (targetItem.ElementState == GazePointerState.Dwell)
                {
                    var dwellRepeat = GetElementStateDelay(targetItem.TargetElement, GazePointerState.DwellRepeat);
                    if (dwellRepeat != int.MaxValue)
                    {
                        targetItem.NextStateTime -= evOldest.Duration;
                    }
                }
            }

            // Return the most recent hit target 
            // Intuition would tell us that we should return NOT the most recent
            // hitTarget, but the one with the most accumulated time in 
            // in the maintained history. But the effect of that is that
            // the user will feel that they have clicked on the wrong thing
            // when they are looking at something else.
            // That is why we return the most recent hitTarget so that 
            // when its dwell time has elapsed, it will be invoked
            return historyItem.HitTarget;
        }

        bool IsInvokable(UIElement element)
        {
            if (IsInvokableImpl != null)
            {
                return IsInvokableImpl(element);
            }
            else
            {
                var button = element as Button;
                if (button != null)
                {
                    return true;
                }

                var toggleButton = element as ToggleButton;
                if (toggleButton != null)
                {
                    return true;
                }

                var toggleSwitch = element as ToggleSwitch;
                if (toggleSwitch != null)
                {
                    return true;
                }

                var textbox = element as TextBox;
                if (textbox != null)
                {
                    return true;
                }

                var pivot = element as Pivot;
                if (pivot != null)
                {
                    return true;
                }
            }

            return false;
        }

        void CheckIfExiting(long curTimestamp)
        {
            for (int index = 0; index < _activeHitTargetTimes.Count; index++)
            {
                var targetItem = _activeHitTargetTimes[index];
                var targetElement = targetItem.TargetElement;
                var exitDelay = GetElementStateDelay(targetElement, GazePointerState.Exit);

                long idleDuration = curTimestamp - targetItem.LastTimestamp;
                if (targetItem.ElementState != GazePointerState.PreEnter && idleDuration > exitDelay)
                {
                    GotoState(targetElement, GazePointerState.Exit);
                    RaiseGazePointerEvent(targetElement, GazePointerState.Exit, targetItem.ElapsedTime);

                    var index2 = _activeHitTargetTimes.IndexOf(targetItem);
                    if (index2 != -1)
                    {
                        _activeHitTargetTimes.RemoveAt(index2);
                    }
                    else
                    {
                        Debug.Assert(false);
                    }

                    // remove all history samples referring to deleted hit target
                    for (int i = 0; i < _gazeHistory.Count;)
                    {
                        var hitTarget = _gazeHistory[i].HitTarget;
                        if (hitTarget == targetElement)
                        {
                            _gazeHistory.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }

                    // return because only one element can be exited at a time and at this point
                    // we have done everything that we can do
                    return;
                }
            }
        }

        void GotoState(UIElement control, GazePointerState state)
        {
            string stateName;

            switch (state)
            {
                case GazePointerState.Enter:
                    return;
                case GazePointerState.Exit:
                    stateName = "Normal";
                    break;
                case GazePointerState.Fixation:
                    stateName = "Fixation";
                    break;
                case GazePointerState.DwellRepeat:
                case GazePointerState.Dwell:
                    stateName = "Dwell";
                    break;
                default:
                    Debug.Fail("Invalid state");
                    return;
            }

            Debug.Assert(stateName != null);
            // TODO: Implement proper support for visual states
            // VisualStateManager::GoToState(dynamic_cast<Control^>(control), stateName, true);
        }
        void RaiseGazePointerEvent(UIElement target, GazePointerState state, int elapsedTime)
        {
            //assert(target != _rootElement);
            var gpea = new GazePointerEventArgs(target, state, elapsedTime);
            //auto buttonObj = dynamic_cast<Button ^>(target);
            //if (buttonObj && buttonObj->Content)
            //{
            //    String^ buttonText = dynamic_cast<String^>(buttonObj->Content);
            //    Debug::WriteLine(L"GPE: %s -> %s, %d", buttonText, PointerStates[(int)state], elapsedTime);
            //}
            //else
            //{
            //    Debug::WriteLine(L"GPE: 0x%08x -> %s, %d", target != nullptr ? target->GetHashCode() : 0, PointerStates[(int)state], elapsedTime);
            //}

            var handled = false;

            if (target != null)
            {
                var element = GazeApi.GetGazeElement(target);
                if (element != null && state == GazePointerState.Dwell)
                {
                    var args = new GazeInvokedRoutedEventArgs();
                    element.RaiseInvoked(this, args);
                    handled = args.Handled;
                }
            }

            if (!handled)
            {
                if (state == GazePointerState.Dwell)
                {
                    InvokeTarget(target);
                }
                else
                {
                    var surrogate = (GazePage)_rootElement.GetValue(GazeApi.GazePageProperty);
                    if (surrogate != null)
                    {
                        surrogate.RaiseGazePointerEvent(this, gpea);
                    }
                }
            }
        }

        void OnGazeMoved(
            GazeInputSourcePreview provider,
            GazeMovedPreviewEventArgs args)
        {
            var intermediatePoints = args.GetIntermediatePoints();
            foreach (var point in intermediatePoints)
            {
                var position = point.EyeGazePosition;
                if (position != null)
                {
                    ProcessGazePoint((long)point.Timestamp, position.Value);
                }
            }
        }

        void ProcessGazePoint(long timestamp, Point position)
        {
            var ea = new GazeEventArgs(position, timestamp);

            var fa = Filter.Update(ea);
            _gazeCursor.Position = fa.Location;

            var hitTarget = ResolveHitTarget(fa.Location, fa.Timestamp);
            Debug.Assert(hitTarget != null);

            //Debug.WriteLine(L"ProcessGazePoint: %llu . [%d, %d], %llu", hitTarget.GetHashCode(), (int)fa.Location.X, (int)fa.Location.Y, fa.Timestamp);

            // check to see if any element in _hitTargetTimes needs an exit event fired.
            // this ensures that all exit events are fired before enter event
            CheckIfExiting(fa.Timestamp);

            var targetItem = GetGazeTargetItem(hitTarget);
            GazePointerState nextState = (GazePointerState)((int)(targetItem.ElementState) + 1);

            //Debug.WriteLine(L"%llu . State=%d, Elapsed=%d, NextStateTime=%d", targetItem.TargetElement, targetItem.ElementState, targetItem.ElapsedTime, targetItem.NextStateTime);

            if (targetItem.ElapsedTime > targetItem.NextStateTime)
            {
                // prevent targetItem from ever actually transitioning into the DwellRepeat state so as
                // to continuously emit the DwellRepeat event
                if (nextState != GazePointerState.DwellRepeat)
                {
                    targetItem.ElementState = nextState;
                    nextState = (GazePointerState)((int)(nextState) + 1);     // nextState++
                    targetItem.NextStateTime = GetElementStateDelay(targetItem.TargetElement, nextState);
                }
                else
                {
                    // move the NextStateTime by one dwell period, while continuing to stay in Dwell state
                    targetItem.NextStateTime += GetElementStateDelay(targetItem.TargetElement, GazePointerState.Dwell) -
                        GetElementStateDelay(targetItem.TargetElement, GazePointerState.Fixation);
                }

                GotoState(targetItem.TargetElement, targetItem.ElementState);
                RaiseGazePointerEvent(targetItem.TargetElement, targetItem.ElementState, targetItem.ElapsedTime);
            }

            _eyesOffTimer.Start();
            _lastTimestamp = fa.Timestamp;
        }

        void OnEyesOff(Object sender, Object ea)
        {
            _eyesOffTimer.Stop();

            CheckIfExiting(_lastTimestamp + EyesOffDelay);
            RaiseGazePointerEvent(null, GazePointerState.Enter, (int)EyesOffDelay);
        }


        UIElement _rootElement;

        long _eyesOffDelay;

        GazeCursor _gazeCursor;
        DispatcherTimer _eyesOffTimer;

        // _offScreenElement is a pseudo-element that represents the area outside
        // the screen so we can track how long the user has been looking outside
        // the screen and appropriately trigger the EyesOff event
        Control _offScreenElement;

        // The value is the total time that FrameworkElement has been gazed at
        List<GazeTargetItem> _activeHitTargetTimes;

        // A vector to track the history of observed gaze targets
        List<GazeHistoryItem> _gazeHistory;
        long _maxHistoryTime;

        // Used to determine if exit events need to be fired by adding GAZE_IDLE_TIME to the last 
        // saved timestamp
        long _lastTimestamp;

        GazeInputSourcePreview _gazeInputSource;
        CoreDispatcher _coreDispatcher;
        GazeIsInvokableDelegate _isInvokableImpl;
        GazeInvokeTargetDelegate _invokeTargetImpl;

        static DependencyProperty GetProperty(GazePointerState state)
        {
            switch (state)
            {
                case GazePointerState.Fixation: return GazeApi.FixationProperty;
                case GazePointerState.Dwell: return GazeApi.DwellProperty;
                case GazePointerState.DwellRepeat: return GazeApi.DwellRepeatProperty;
                case GazePointerState.Enter: return GazeApi.EnterProperty;
                case GazePointerState.Exit: return GazeApi.ExitProperty;
                default: return null;
            }
        }

        static TimeSpan GetDefaultPropertyValue(GazePointerState state)
        {
            switch (state)
            {
                case GazePointerState.Fixation: return TimeSpan.FromMilliseconds(DEFAULT_FIXATION_DELAY / 1000.0);
                case GazePointerState.Dwell: return TimeSpan.FromMilliseconds(DEFAULT_DWELL_DELAY / 1000.0);
                case GazePointerState.DwellRepeat: return TimeSpan.FromMilliseconds(DEFAULT_REPEAT_DELAY / 1000.0);
                case GazePointerState.Enter: return TimeSpan.FromMilliseconds(DEFAULT_ENTER_EXIT_DELAY / 1000.0);
                case GazePointerState.Exit: return TimeSpan.FromMilliseconds(DEFAULT_ENTER_EXIT_DELAY / 1000.0);
                default: return GazeApi.s_nonTimeSpan;
            }
        }
    }
}
