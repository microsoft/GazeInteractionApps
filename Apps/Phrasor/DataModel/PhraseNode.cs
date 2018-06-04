//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace Phrasor
{
    sealed class PhraseNode : INotifyPropertyChanged
    {
        public ObservableCollection<PhraseNode> Children { get; set; }

        public PhraseNode()
        {
            Children = new ObservableCollection<PhraseNode>();
        }

        private PhraseNode parent;
        public PhraseNode Parent
        {
            get { return parent; }
            set
            {
                parent = value;
                OnPropertyChanged();
            }
        }

        private string caption;
        public string Caption
        {
            get { return caption; }
            set
            {
                caption = value;
                OnPropertyChanged();
            }
        }

        private bool isCategory = false;
        public bool IsCategory
        {
            get { return isCategory; }
            set
            {
                isCategory = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string caller = "")
        {
            var holder = PropertyChanged;
            if (holder != null)
            {
                holder(this,
                    new PropertyChangedEventArgs(caller));
            }
        }
    }

}
