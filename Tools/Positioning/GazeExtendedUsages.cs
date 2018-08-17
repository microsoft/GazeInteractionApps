//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
//See LICENSE in the project root for license information. 

using System;

namespace GazeHidParsers
{
    /// <summary>
    /// Usage page and usages from the EyeHeadTracker HID specification
    /// </summary>
    /// <see cref="http://www.usb.org/developers/hidpage/HUTRR74_-_Usage_Page_for_Head_and_Eye_Trackers.pdf"/>
    public sealed class GazeHidUsages
    {
        public const UInt16 UsagePage_EyeHeadTracker            = 0x0012;   // TYPE
        public const UInt16 Usage_EyeTracker                    = 0x0001;   // CA
        public const UInt16 Usage_HeadTracker                   = 0x0002;   // CA
        // 0x0003-0x000F                                        RESERVED
        public const UInt16 Usage_TrackingData                  = 0x0010;   // CP
        public const UInt16 Usage_Capabilities                  = 0x0011;   // CL
        public const UInt16 Usage_Configuration                 = 0x0012;   // CL
        public const UInt16 Usage_Status                        = 0x0013;   // CL
        public const UInt16 Usage_Control                       = 0x0014;   // CL
        // 0x0015-0x001F                                        RESERVED
        public const UInt16 Usage_Timestamp                     = 0x0020;   // DV
        public const UInt16 Usage_PositionX                     = 0x0021;   // DV
        public const UInt16 Usage_PositionY                     = 0x0022;   // DV
        public const UInt16 Usage_PositionZ                     = 0x0023;   // DV
        public const UInt16 Usage_GazePoint                     = 0x0024;   // CP
        public const UInt16 Usage_LeftEyePosition               = 0x0025;   // CP
        public const UInt16 Usage_RightEyePosition              = 0x0026;   // CP
        public const UInt16 Usage_HeadPosition                  = 0x0027;   // CP
        public const UInt16 Usage_HeadDirectionPoint            = 0x0028;   // CP
        public const UInt16 Usage_RotationX                     = 0x0029;   // DV
        public const UInt16 Usage_RotationY                     = 0x002A;   // DV
        public const UInt16 Usage_RotationZ                     = 0x002B;   // DV
        // 0x002C-0x00FF                                        RESERVED
        public const UInt16 Usage_TrackerQuality                = 0x0100;   // SV
        public const UInt16 Usage_MinimumTrackingDistance       = 0x0101;   // SV
        public const UInt16 Usage_OptimumTrackingDistance       = 0x0102;   // SV
        public const UInt16 Usage_MaximumTrackingDistance       = 0x0103;   // SV
        public const UInt16 Usage_MaximumScreenPlaneWidth       = 0x0104;   // SV
        public const UInt16 Usage_MaximumScreenPlaneHeight      = 0x0105;   // SV
        // 0x0106-0x01FF                                        RESERVED
        public const UInt16 Usage_DisplayManufacturerId         = 0x0200;   // SV
        public const UInt16 Usage_DisplayProductId              = 0x0201;   // SV
        public const UInt16 Usage_DisplaySerialNumber           = 0x0202;   // SV
        public const UInt16 Usage_DisplayManufacturerDate       = 0x0203;   // SV
        public const UInt16 Usage_CalibratedScreenWidth         = 0x0204;   // SV
        public const UInt16 Usage_CalibratedScreenHeight        = 0x0205;   // SV
        // 0x0206-0x02FF                                        RESERVED
        public const UInt16 Usage_SamplingFrequency             = 0x0300;   // DV
        public const UInt16 Usage_ConfigurationStatus           = 0x0301;   // DV
        // 0x0302-0x03FF                                        RESERVED
        public const UInt16 Usage_DeviceModeRequest             = 0x0400;   // DV
        // 0x0401-0xFFFF                                        RESERVED
    }
}
