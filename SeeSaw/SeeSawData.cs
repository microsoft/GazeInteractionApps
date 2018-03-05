//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Input.Gaze;
namespace SeeSaw
{
    public class SeeSawData
    {
        private static SeeSawData _instance;

        public Dictionary<string, List<GazeEventArgs>> GazeData;

        public SeeSawData()
        {
            GazeData = new Dictionary<string, List<GazeEventArgs>>();
        }

        public static SeeSawData Instance
        {
            get
            {
                if (_instance == null)
                {
                     _instance = new SeeSawData();
                }
                return _instance;
            }
        }
    };
}
