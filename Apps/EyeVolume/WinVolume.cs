using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Media.Devices;

namespace EyeVolume
{
    //http://pastebin.com/cPhVCyWj

    enum HResult
    {
        S_OK = 0
    }

    [ComImport]
    [Guid("72A22D78-CDE4-431D-B8CC-843A71199B6D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IActivateAudioInterfaceAsyncOperation
    {
        void GetActivateResult([MarshalAs(UnmanagedType.Error)]out HResult activateResult, [MarshalAs(UnmanagedType.IUnknown)]out object activatedInterface);
    }

    [ComImport]
    [Guid("41D949AB-9862-444A-80F6-C261334DA5EB")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IActivateAudioInterfaceCompletionHandler
    {
        void ActivateCompleted(IActivateAudioInterfaceAsyncOperation activateOperation);
    }

    class ActivateAudioInterfaceCompletionHandler<T> : IActivateAudioInterfaceCompletionHandler
    {
        public ActivateAudioInterfaceCompletionHandler()
        {
            m_CompletionEvent = new AutoResetEvent(false);
        }

        public void ActivateCompleted(IActivateAudioInterfaceAsyncOperation operation)
        {
            HResult operationHR;
            object activatedInterface;
            operation.GetActivateResult(out operationHR, out activatedInterface);
            Debug.Assert(operationHR == HResult.S_OK);

            m_Result = (T)activatedInterface;

            var setResult = m_CompletionEvent.Set();
            Debug.Assert(setResult != false);
        }

        public T WaitForCompletion()
        {
            var waitResult = m_CompletionEvent.WaitOne();
            Debug.Assert(waitResult != false);

            return m_Result;
        }

        private AutoResetEvent m_CompletionEvent;
        private T m_Result;
    }

    [ComImport]
    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal unsafe interface IAudioEndpointVolume
    {
        void RegisterControlChangeNotify(object pNotify);
        void UnregisterControlChangeNotify(object pNotify);
        uint GetChannelCount();
        void SetMasterVolumeLevel(float fLevelDB, Guid* pguidEventContext);
        void SetMasterVolumeLevelScalar(float fLevel, Guid* pguidEventContext);
        float GetMasterVolumeLevel();
        float GetMasterVolumeLevelScalar();
        void SetChannelVolumeLevel(uint nChannel, float fLevelDB, Guid* pguidEventContext);
        void SetChannelVolumeLevelScalar(uint nChannel, float fLevel, Guid* pguidEventContext);
        float GetChannelVolumeLevel(uint nChannel);
        float GetChannelVolumeLevelScalar(uint nChannel);
        void SetMute(bool bMute, Guid* pguidEventContext);
        bool GetMute();
        void GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
        void VolumeStepUp(Guid* pguidEventContext);
        void VolumeStepDown(Guid* pguidEventContext);
        uint QueryHardwareSupport();
        void GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
    }

    class VolumeControl
    {
        [DllImport("Mmdevapi.dll")]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern HResult ActivateAudioInterfaceAsync(
            [MarshalAs(UnmanagedType.LPWStr)]string deviceInterfacePath,
            [MarshalAs(UnmanagedType.LPStruct)]Guid riid,
            IntPtr activationParams,
            IActivateAudioInterfaceCompletionHandler completionHandler,
            out IActivateAudioInterfaceAsyncOperation activationOperation);

        private IAudioEndpointVolume _audioEndpointVolume;

        public VolumeControl()
        {
            var defaultAudioRenderDevice = MediaDevice.GetDefaultAudioRenderId(AudioDeviceRole.Default);
            var activateAudioInterfaceCompletionHandler = new ActivateAudioInterfaceCompletionHandler<IAudioEndpointVolume>();

            IActivateAudioInterfaceAsyncOperation activateOperation;
            var hr = ActivateAudioInterfaceAsync(defaultAudioRenderDevice, typeof(IAudioEndpointVolume).GetTypeInfo().GUID, IntPtr.Zero, (IActivateAudioInterfaceCompletionHandler)activateAudioInterfaceCompletionHandler, out activateOperation);
            Debug.Assert(hr == HResult.S_OK);

            _audioEndpointVolume = activateAudioInterfaceCompletionHandler.WaitForCompletion();
            Debug.Assert(_audioEndpointVolume != null);
        }

        public unsafe bool Mute
        {
            get { return _audioEndpointVolume.GetMute(); }
            set
            {
                _audioEndpointVolume.SetMute(value, null);
            }
        }

        public unsafe float Volume
        {
            get { return _audioEndpointVolume.GetMasterVolumeLevelScalar(); }
            set
            {
                try
                {

                    if (value > 1)
                    {
                        value = 1;
                    }
                    else if (value < 0)
                    {
                        value = 0;
                    }

                    _audioEndpointVolume.SetMasterVolumeLevelScalar(value, null);
                }
                catch { }
            }
        }
    }
}