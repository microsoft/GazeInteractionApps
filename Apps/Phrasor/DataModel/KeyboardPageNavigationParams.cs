//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phrasor
{
    sealed class KeyboardPageNavigationParams
    {
        public PhraseNode RootNode;
        public PhraseNode CurrentNode;
        public PhraseNode ChildNode;
        public bool IsCategory;
        public bool NeedsSaving;
        public bool SpeechMode;
        public bool GazePlusClickMode;
    }

}
