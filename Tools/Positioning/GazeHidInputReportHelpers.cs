//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
//See LICENSE in the project root for license information. 

using System;
using System.Numerics;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Devices.Input.Preview;

namespace Positioning
{
    class GazeHidInputReportHelpers
    {
        public sealed class GazePositionHidParser
        {
            private readonly HidNumericControlDescription _X = null;
            private readonly HidNumericControlDescription _Y = null;
            private readonly HidNumericControlDescription _Z = null;

            private readonly UInt16 _usage = 0x0000;

            public GazePositionHidParser(GazeDevicePreview gazeDevice, UInt16 usage)
            {
                _usage = usage;

                // Find all the head rotation usage from the device's
                // descriptor and store them for easy access
                _X = GetGazeUsageFromCollectionId(gazeDevice, GazeExtendedUsages.Usage_PositionX, _usage);
                _Y = GetGazeUsageFromCollectionId(gazeDevice, GazeExtendedUsages.Usage_PositionY, _usage);
                _Z = GetGazeUsageFromCollectionId(gazeDevice, GazeExtendedUsages.Usage_PositionZ, _usage);
            }

            public Vector3? GetPosition(HidInputReport report)
            {
                Vector3? result = null;

                if (_X != null &&
                    _Y != null &&
                    _Z != null &&
                    _usage != 0x0000)
                {
                    var descX = report.GetNumericControlByDescription(_X);
                    var descY = report.GetNumericControlByDescription(_Y);
                    var descZ = report.GetNumericControlByDescription(_Z);

                    var controlDescX = descX.ControlDescription;
                    var controlDescY = descX.ControlDescription;
                    var controlDescZ = descX.ControlDescription;

                    if ((controlDescX.LogicalMaximum < descX.Value || controlDescX.LogicalMinimum > descX.Value) ||
                        (controlDescY.LogicalMaximum < descY.Value || controlDescY.LogicalMinimum > descY.Value) ||
                        (controlDescZ.LogicalMaximum < descZ.Value || controlDescZ.LogicalMinimum > descZ.Value))
                    {
                        // One of the values is outside of the valid range.
                    }
                    else
                    {
                        result = new Vector3
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

        public sealed class GazeRotationHidParser
        {
            private readonly HidNumericControlDescription _X = null;
            private readonly HidNumericControlDescription _Y = null;
            private readonly HidNumericControlDescription _Z = null;

            private readonly UInt16 _usage = 0x0000;

            public GazeRotationHidParser(GazeDevicePreview gazeDevice, UInt16 usage)
            {
                _usage = usage;

                // Find all the head rotation usage from the device's
                // descriptor and store them for easy access
                _X = GetGazeUsageFromCollectionId(gazeDevice, GazeExtendedUsages.Usage_RotationX, _usage);
                _Y = GetGazeUsageFromCollectionId(gazeDevice, GazeExtendedUsages.Usage_RotationY, _usage);
                _Z = GetGazeUsageFromCollectionId(gazeDevice, GazeExtendedUsages.Usage_RotationZ, _usage);
            }

            public Vector3? GetRotation(HidInputReport report)
            {
                Vector3? result = null;

                if (_X != null &&
                    _Y != null &&
                    _Z != null &&
                    _usage != 0x0000)
                {
                    var descX = report.GetNumericControlByDescription(_X);
                    var descY = report.GetNumericControlByDescription(_Y);
                    var descZ = report.GetNumericControlByDescription(_Z);

                    var controlDescX = descX.ControlDescription;
                    var controlDescY = descX.ControlDescription;
                    var controlDescZ = descX.ControlDescription;

                    if ((controlDescX.LogicalMaximum < descX.Value || controlDescX.LogicalMinimum > descX.Value) ||
                        (controlDescY.LogicalMaximum < descY.Value || controlDescY.LogicalMinimum > descY.Value) ||
                        (controlDescZ.LogicalMaximum < descZ.Value || controlDescZ.LogicalMinimum > descZ.Value))
                    {
                        // One of the values is outside of the valid range.
                    }
                    else
                    {
                        result = new Vector3
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

        private static HidNumericControlDescription GetGazeUsageFromCollectionId(GazeDevicePreview gazeDevice, UInt16 childUsageId, UInt16 parentUsageId)
        {
            var numericControls = gazeDevice.GetNumericControlDescriptions(
                GazeExtendedUsages.UsagePage_EyeHeadTracker, childUsageId);

            for (int i = 0; i < numericControls.Count; i++)
            {
                var parentCollections = numericControls[i].ParentCollections;
                if (parentCollections.Count > 0 &&
                    parentCollections[0].UsagePage == GazeExtendedUsages.UsagePage_EyeHeadTracker &&
                    parentCollections[0].UsageId == parentUsageId)
                {
                    return numericControls[i];
                }
            }
            return null;
        }
    }
}
