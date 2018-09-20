using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Media.Devices;

namespace EyeVolume
{
    //http://pastebin.com/cPhVCyWj

    internal enum HResult : uint
    {
        S_OK = 0
    }

    [ComImport]
    [Guid("72A22D78-CDE4-431D-B8CC-843A71199B6D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IActivateAudioInterfaceAsyncOperation
    {
        void GetActivateResult(
            [MarshalAs(UnmanagedType.Error)]out uint activateResult, 
            [MarshalAs(UnmanagedType.IUnknown)]out object activatedInterface);
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
           
            operation.GetActivateResult(out var operationHR, out var activatedInterface);

            Debug.Assert(operationHR == (uint)HResult.S_OK);

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

        [return: MarshalAs(UnmanagedType.U4)]
        uint GetChannelCount();

        void SetMasterVolumeLevel(
            [In] [MarshalAs(UnmanagedType.R4)] float fLevelDB,
            [In] [MarshalAs(UnmanagedType.LPStruct)] Guid pguidEventContext);

        void SetMasterVolumeLevelScalar(
            [In] [MarshalAs(UnmanagedType.R4)] float fLevel,
            [In] [MarshalAs(UnmanagedType.LPStruct)] Guid pguidEventContext);

        [return: MarshalAs(UnmanagedType.R4)]
        float GetMasterVolumeLevel();

        [return: MarshalAs(UnmanagedType.R4)]
        float GetMasterVolumeLevelScalar();

        void SetChannelVolumeLevel(
            [In] [MarshalAs(UnmanagedType.U4)] uint nChannel,
            [In] [MarshalAs(UnmanagedType.R4)] float fLevelDB,
            [In] [MarshalAs(UnmanagedType.LPStruct)] Guid pguidEventContext);

        void SetChannelVolumeLevelScalar(
            [In] [MarshalAs(UnmanagedType.U4)] uint nChannel,
            [In] [MarshalAs(UnmanagedType.R4)]float fLevel,
            [In] [MarshalAs(UnmanagedType.LPStruct)]Guid pguidEventContext);

        void GetChannelVolumeLevel(
             [In] [MarshalAs(UnmanagedType.U4)] uint nChannel,
             [Out] [MarshalAs(UnmanagedType.R4)] out float level);

        [return: MarshalAs(UnmanagedType.R4)]
        float GetChannelVolumeLevelScalar([In] [MarshalAs(UnmanagedType.U4)] uint nChannel);        

        void SetMute(
            [In] [MarshalAs(UnmanagedType.Bool)] bool bMute, 
            [In] [MarshalAs(UnmanagedType.LPStruct)] Guid pguidEventContext);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool GetMute();

        void GetVolumeStepInfo(
            [Out] [MarshalAs(UnmanagedType.U4)] out uint pnStep,
            [Out] [MarshalAs(UnmanagedType.U4)] out uint pnStepCount);

        void VolumeStepUp([In] [MarshalAs(UnmanagedType.LPStruct)] Guid pguidEventContext);

        void VolumeStepDown([In] [MarshalAs(UnmanagedType.LPStruct)] Guid pguidEventContext);

        [return: MarshalAs(UnmanagedType.U4)] // bit mask
        uint QueryHardwareSupport();

        void GetVolumeRange(
            [Out] [MarshalAs(UnmanagedType.R4)] out float pflVolumeMindB,
            [Out] [MarshalAs(UnmanagedType.R4)] out float pflVolumeMaxdB,
            [Out] [MarshalAs(UnmanagedType.R4)] out float pflVolumeIncrementdB);
    }

    class VolumeControl
    {
        [DllImport("Mmdevapi.dll")]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern HResult ActivateAudioInterfaceAsync(
            [In, MarshalAs(UnmanagedType.LPWStr)]string deviceInterfacePath,
            [In, MarshalAs(UnmanagedType.LPStruct)]Guid riid,
            [In] IntPtr activationParams,
            [In] IActivateAudioInterfaceCompletionHandler completionHandler,
            out IActivateAudioInterfaceAsyncOperation activationOperation);

        private IAudioEndpointVolume _audioEndpointVolume;

        private static IAudioEndpointVolume GetAudioEndpointVolumeInterface()

        {
            var speakerId = MediaDevice.GetDefaultAudioRenderId(AudioDeviceRole.Default);
            var completionHandler = new ActivateAudioInterfaceCompletionHandler<IAudioEndpointVolume>();

            var hr = ActivateAudioInterfaceAsync(
                speakerId,
                typeof(IAudioEndpointVolume).GetTypeInfo().GUID,
                IntPtr.Zero,
                completionHandler,
                out var activateOperation);

            Debug.Assert(hr == (uint)HResult.S_OK);

            return completionHandler.WaitForCompletion();
        }

        public VolumeControl()
        {
            var defaultAudioRenderDevice = MediaDevice.GetDefaultAudioRenderId(AudioDeviceRole.Default);
            var activateAudioInterfaceCompletionHandler = new ActivateAudioInterfaceCompletionHandler<IAudioEndpointVolume>();

            IActivateAudioInterfaceAsyncOperation activateOperation;
            var hr = ActivateAudioInterfaceAsync(defaultAudioRenderDevice, typeof(IAudioEndpointVolume).GetTypeInfo().GUID, IntPtr.Zero, (IActivateAudioInterfaceCompletionHandler)activateAudioInterfaceCompletionHandler, out activateOperation);
            Debug.Assert(hr == HResult.S_OK);
                       
            _audioEndpointVolume = GetAudioEndpointVolumeInterface();

            Debug.Assert(_audioEndpointVolume != null);
        }

        public unsafe bool Mute
        {
            get { return _audioEndpointVolume.GetMute(); }
            set
            {
                try
                {
                    _audioEndpointVolume.SetMute(value, Guid.Empty);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Set Mute to: " + value + " failed: " + e.Message);
                }
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

                    _audioEndpointVolume.SetMasterVolumeLevelScalar(value, Guid.Empty);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Set Master Volume to: " + value + " failed: " + e.Message);
                }
            }
        }
    }
}