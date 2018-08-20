//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
//See LICENSE in the project root for license information. 

using System;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Devices.Input.Preview;

namespace GazeHidParsers
{
    public struct Long3
    {
        public long X;
        public long Y;
        public long Z;
    }

    public sealed class GazeHidPositionParser
    {
        private readonly HidNumericControlDescription _X = null;
        private readonly HidNumericControlDescription _Y = null;
        private readonly HidNumericControlDescription _Z = null;

        private readonly UInt16 _usage = 0x0000;

        public GazeHidPositionParser(GazeDevicePreview gazeDevice, UInt16 usage)
        {
            _usage = usage;

            // Find all the head rotation usage from the device's
            // descriptor and store them for easy access
            _X = GazeHidParsers.GetGazeUsageFromCollectionId(gazeDevice, GazeHidUsages.Usage_PositionX, _usage);
            _Y = GazeHidParsers.GetGazeUsageFromCollectionId(gazeDevice, GazeHidUsages.Usage_PositionY, _usage);
            _Z = GazeHidParsers.GetGazeUsageFromCollectionId(gazeDevice, GazeHidUsages.Usage_PositionZ, _usage);
        }

        public Long3? GetPosition(HidInputReport report)
        {
            Long3? result = null;

            if (_X != null &&
                _Y != null &&
                _Z != null &&
                _usage != 0x0000)
            {
                var descX = report.GetNumericControlByDescription(_X);
                var descY = report.GetNumericControlByDescription(_Y);
                var descZ = report.GetNumericControlByDescription(_Z);

                var controlDescX = descX.ControlDescription;
                var controlDescY = descY.ControlDescription;
                var controlDescZ = descZ.ControlDescription;

                if ((controlDescX.LogicalMaximum < descX.Value || controlDescX.LogicalMinimum > descX.Value) ||
                    (controlDescY.LogicalMaximum < descY.Value || controlDescY.LogicalMinimum > descY.Value) ||
                    (controlDescZ.LogicalMaximum < descZ.Value || controlDescZ.LogicalMinimum > descZ.Value))
                {
                    // One of the values is outside of the valid range.
                }
                else
                {
                    result = new Long3
                    {
                        X = descX.Value,
                        Y = descY.Value,
                        Z = descZ.Value
                    };
                }
            }

            return result;
        }
    }

    public sealed class GazeHidRotationParser
    {
        private readonly HidNumericControlDescription _X = null;
        private readonly HidNumericControlDescription _Y = null;
        private readonly HidNumericControlDescription _Z = null;

        private readonly UInt16 _usage = 0x0000;

        public GazeHidRotationParser(GazeDevicePreview gazeDevice, UInt16 usage)
        {
            _usage = usage;

            // Find all the head rotation usage from the device's
            // descriptor and store them for easy access
            _X = GazeHidParsers.GetGazeUsageFromCollectionId(gazeDevice, GazeHidUsages.Usage_RotationX, _usage);
            _Y = GazeHidParsers.GetGazeUsageFromCollectionId(gazeDevice, GazeHidUsages.Usage_RotationY, _usage);
            _Z = GazeHidParsers.GetGazeUsageFromCollectionId(gazeDevice, GazeHidUsages.Usage_RotationZ, _usage);
        }

        public Long3? GetRotation(HidInputReport report)
        {
            Long3? result = null;

            if (_X != null &&
                _Y != null &&
                _Z != null &&
                _usage != 0x0000)
            {
                var descX = report.GetNumericControlByDescription(_X);
                var descY = report.GetNumericControlByDescription(_Y);
                var descZ = report.GetNumericControlByDescription(_Z);

                var controlDescX = descX.ControlDescription;
                var controlDescY = descY.ControlDescription;
                var controlDescZ = descZ.ControlDescription;

                if ((controlDescX.LogicalMaximum < descX.Value || controlDescX.LogicalMinimum > descX.Value) ||
                    (controlDescY.LogicalMaximum < descY.Value || controlDescY.LogicalMinimum > descY.Value) ||
                    (controlDescZ.LogicalMaximum < descZ.Value || controlDescZ.LogicalMinimum > descZ.Value))
                {
                    // One of the values is outside of the valid range.
                }
                else
                {
                    result = new Long3
                    {
                        X = descX.Value,
                        Y = descY.Value,
                        Z = descZ.Value
                    };
                }
            }

            return result;
        }
    }

    public class GazeHidParsers
    {
        public static HidNumericControlDescription GetGazeUsageFromCollectionId(GazeDevicePreview gazeDevice, UInt16 childUsageId, UInt16 parentUsageId)
        {
            var numericControls = gazeDevice.GetNumericControlDescriptions(
                GazeHidUsages.UsagePage_EyeHeadTracker, childUsageId);

            for (int i = 0; i < numericControls.Count; i++)
            {
                var parentCollections = numericControls[i].ParentCollections;
                if (parentCollections.Count > 0 &&
                    parentCollections[0].UsagePage == GazeHidUsages.UsagePage_EyeHeadTracker &&
                    parentCollections[0].UsageId == parentUsageId)
                {
                    return numericControls[i];
                }
            }
            return null;
        }
    }

    public sealed class LeftEyePositionParser
    {
        public LeftEyePositionParser(GazeDevicePreview gazeDevice)
        {
            gazeHidPositionParser = new GazeHidPositionParser(gazeDevice, GazeHidUsages.Usage_LeftEyePosition);
        }

        public Long3? GetPosition(HidInputReport report)
        {
            return gazeHidPositionParser.GetPosition(report);
        }

        private GazeHidPositionParser gazeHidPositionParser;
    }

    public sealed class RightEyePositionParser
    {
        public RightEyePositionParser(GazeDevicePreview gazeDevice)
        {
            gazeHidPositionParser = new GazeHidPositionParser(gazeDevice, GazeHidUsages.Usage_RightEyePosition);
        }

        public Long3? GetPosition(HidInputReport report)
        {
            return gazeHidPositionParser.GetPosition(report);
        }

        private GazeHidPositionParser gazeHidPositionParser;
    }

    public sealed class HeadPositionParser
    {
        public HeadPositionParser(GazeDevicePreview gazeDevice)
        {
            gazeHidPositionParser = new GazeHidPositionParser(gazeDevice, GazeHidUsages.Usage_HeadPosition);
        }

        public Long3? GetPosition(HidInputReport report)
        {
            return gazeHidPositionParser.GetPosition(report);
        }

        private GazeHidPositionParser gazeHidPositionParser;
    }

    public sealed class HeadRotationParser
    {
        public HeadRotationParser(GazeDevicePreview gazeDevice)
        {
            gazeHidRotationParser = new GazeHidRotationParser(gazeDevice, GazeHidUsages.Usage_HeadDirectionPoint);
        }

        public Long3? GetPosition(HidInputReport report)
        {
            return gazeHidRotationParser.GetRotation(report);
        }

        private GazeHidRotationParser gazeHidRotationParser;
    }
}
