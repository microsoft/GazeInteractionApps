//Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
//See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Phrasor
{
    class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<PhraseNode> Tiles { get; private set; }

        public PhraseNode RootNode;
        public AppSettings Settings;
       
        public ViewModel()
        {
            this.Tiles = new ObservableCollection<PhraseNode>();

            this.RootNode = new PhraseNode();

            this.TileWidth = 200;
            this.TileHeight = 200;           

            Settings = new AppSettings();
            Settings.Load();          
        }

        public void GoToNode(PhraseNode node)
        {
            Tiles.Clear();

            var sortedChildren =
            from t in node.Children
            orderby t.Caption
            orderby t.IsCategory descending
            select t;

            foreach (PhraseNode chlldNode in sortedChildren)
            {
                Tiles.Add(chlldNode);
            }
            CurrentNode = node;

            AtRootNode = false;
            if (node == RootNode)
            {
                AtRootNode = true;
            }
        }

        private bool _AtRootNode;
        public bool AtRootNode
        {
            get { return _AtRootNode; }
            set
            {
                _AtRootNode = value;
                OnPropertyChanged();
            }
        }

        private PhraseNode _CurrentNode;
        public PhraseNode CurrentNode
        {
            get { return _CurrentNode; }
            set
            {
                _CurrentNode = value;
                OnPropertyChanged();
            }
        } 

        private double _TileWidth;
        public double TileWidth
        {
            get { return _TileWidth; }
            set
            {
                _TileWidth = value;
                OnPropertyChanged();
            }
        }

        private double _TileHeight;
        public double TileHeight
        {
            get { return _TileHeight; }
            set
            {
                _TileHeight = value;
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
