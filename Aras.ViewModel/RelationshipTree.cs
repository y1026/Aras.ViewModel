﻿/*  
  Copyright 2017 Processwall Limited

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Web:     http://www.processwall.com
  Email:   support@processwall.com
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.ViewModel
{
    public class RelationshipTree : Tree
    {
        private IItemFormatter _itemFormatter;
        public IItemFormatter ItemFormatter
        {
            get
            {
                return this._itemFormatter;
            }
            set
            {
                if (this._itemFormatter != value)
                {
                    this._itemFormatter = value;
                    this.Refresh.Execute();
                }
            }
        }

        private IRelationshipFormatter _relationshipFormatter;
        public IRelationshipFormatter RelationshipFormatter 
        { 
            get
            {
                return this._relationshipFormatter;
            }
            set
            {
                if (this._relationshipFormatter != value)
                {
                    this._relationshipFormatter = value;
                    this.Refresh.Execute();
                }
            }
        }

        private Model.RelationshipType _relationshipType;
        public Model.RelationshipType RelationshipType
        {
            get
            {
                return this._relationshipType;
            }
            set
            {
                this._relationshipType = value;

                if (this._relationshipType != null)
                {
                    this.Refresh.Execute();
                }
            }
        }

        [ViewModel.Attributes.Command("Refresh")]
        public RefreshCommand Refresh { get; private set; }

        [ViewModel.Attributes.Command("Select")]
        public SelectCommand Select { get; private set; }

        [ViewModel.Attributes.Command("Add")]
        public AddCommand Add { get; private set; }

        [ViewModel.Attributes.Command("Cut")]
        public CutCommand Cut { get; private set; }

        [ViewModel.Attributes.Command("Copy")]
        public CopyCommand Copy { get; private set; }

        [ViewModel.Attributes.Command("Paste")]
        public PasteCommand Paste { get; private set; }

        [ViewModel.Attributes.Command("Delete")]
        public DeleteCommand Delete { get; private set; }

        [ViewModel.Attributes.Command("Save")]
        public SaveCommand Save { get; private set; }

        [ViewModel.Attributes.Command("Undo")]
        public UndoCommand Undo { get; private set; }

        [ViewModel.Attributes.Command("Indent")]
        public IndentCommand Indent { get; private set; }

        [ViewModel.Attributes.Command("Outdent")]
        public OutdentCommand Outdent { get; private set; }

        [ViewModel.Attributes.Command("SearchClosed")]
        public SearchClosedCommand SearchClosed { get; private set; }

        private RelationshipTreeNode _selected;
        public RelationshipTreeNode Selected
        {
            get
            {
                return this._selected;
            }
            private set
            {
                if (value != null)
                {
                    if (value.Tree.ID.Equals(this.ID) && !value.Equals(this._selected))
                    {
                        this._selected = value;
                        this.OnPropertyChanged("Selected");
                    }
                }
                else
                {
                    if (this._selected != null)
                    {
                        this._selected = null;
                        this.OnPropertyChanged("Selected");
                    }
                }
            }
        }

        private Boolean _showSearch;
        [ViewModel.Attributes.Property("ShowSearch", Aras.ViewModel.Attributes.PropertyTypes.Boolean, true)]
        public Boolean ShowSearch
        {
            get
            {
                return this._showSearch;
            }
            private set
            {
                if (this._showSearch != value)
                {
                    this._showSearch = value;
                    this.OnPropertyChanged("ShowSearch");
                }
            }
        }

        [ViewModel.Attributes.Property("Search", Aras.ViewModel.Attributes.PropertyTypes.Control, true)]
        public Grids.Search Search { get; private set; }
   
        private Dictionary<String, RelationshipTreeNode> NodeCache;

        internal RelationshipTreeNode GetNodeFromCache(String ID)
        {
            if (this.NodeCache.ContainsKey(ID))
            {
                return this.NodeCache[ID];
            }
            else
            {
                return null;
            }
        }

        internal void AddNodeToCache(RelationshipTreeNode Node)
        {
            if (Node.Relationship == null)
            {
                this.NodeCache[Node.Item.ID] = Node;
            }
            else
            {
                this.NodeCache[Node.Relationship.ID] = Node;
            }
        }

        private Model.Item CopyPasteBuffer { get; set; }

        private Model.Transaction _transaction;
        private Model.Transaction Transaction
        {
            get
            {
                if (this._transaction == null)
                {
                    if (this.Binding != null)
                    {
                        this._transaction = ((Model.Item)this.Binding).Store.Session.BeginTransaction();
                    }
                }

                return this._transaction;
            }
        }

        private void RefreshCommands()
        {
            this.Copy.UpdateCanExecute();
            this.Cut.UpdateCanExecute();
            this.Delete.UpdateCanExecute();
            this.Paste.UpdateCanExecute();
            this.Save.UpdateCanExecute();
            this.Indent.UpdateCanExecute();
            this.Outdent.UpdateCanExecute();
            this.Undo.UpdateCanExecute();
            this.Add.UpdateCanExecute();
        }

        protected override void AfterBindingChanged()
        {
            base.AfterBindingChanged();

            if (this.Binding != null)
            {
                if (this.Binding is Model.Item)
                {
                    // Create Root Node
                    this.Node = this.GetNodeFromCache(((Model.Item)this.Binding).ID);

                    if (this.Node == null)
                    {
                        this.Node = new RelationshipTreeNode(this, null);
                        this.Node.Binding = this.Binding;
                        this.AddNodeToCache((RelationshipTreeNode)this.Node);
                    }
                    else
                    {
                        this.Node.Binding = this.Binding;
                    }

                    // Set Binding for Search Control
                    //this.Search.Binding = new Model.Stores.Item<Model.Item>(((Model.Item)this.Binding).ItemType);
                }
                else
                {
                    throw new Model.Exceptions.ArgumentException("Binding must be of type Model.Item");
                }
            }
            else
            {
                this.Node = null;
            }
        }

        protected virtual void RefreshControl()
        {
            if (this.Node != null)
            {
                this.Node.Refresh.Execute();
            }
        }

        public RelationshipTree(Manager.Session Session, IRelationshipFormatter RelationshipFormatter, IItemFormatter ItemFormatter)
            :base(Session)
        {
            this.NodeCache = new Dictionary<String, RelationshipTreeNode>();
            this._relationshipFormatter = RelationshipFormatter;
            this._itemFormatter = ItemFormatter;
            this.Node = null;
            this._transaction = null;
            this.CopyPasteBuffer = null;
            this.Refresh = new RefreshCommand(this);
            this.Add = new AddCommand(this);
            this.Cut = new CutCommand(this);
            this.Copy = new CopyCommand(this);
            this.Delete = new DeleteCommand(this);
            this.Paste = new PasteCommand(this);
            this.Save = new SaveCommand(this);
            this.Undo = new UndoCommand(this);
            this.Select = new SelectCommand(this);
            this.Indent = new IndentCommand(this);
            this.Outdent = new OutdentCommand(this);
            this.SearchClosed = new SearchClosedCommand(this);
            this.Search = new Grids.Search(this.Session);

            // Watch for Selection on Search Control
            this.Search.Selected.ListChanged += Selected_ListChanged;
        }

        public RelationshipTree(Manager.Session Session)
            : this(Session, new RelationshipFormatters.Default(), new ItemFormatters.Default())
        {

        }

        public class RefreshCommand : Aras.ViewModel.Command
        {
            protected override void Run(IEnumerable<Control> Parameters)
            {
                ((RelationshipTree)this.Control).RefreshControl();
                this.CanExecute = true;
            }

            internal RefreshCommand(Control Control)
                : base(Control)
            {
                this.CanExecute = true;
            }
        }

        public class SelectCommand : Aras.ViewModel.Command
        {
            public RelationshipTree RelationshipTree
            {
                get
                {
                    return ((RelationshipTree)this.Control);
                }
            }

            protected override void Run(IEnumerable<Control> Parameters)
            {
                if (Parameters != null)
                {
                    foreach (Control parameter in Parameters)
                    {
                        if (parameter is RelationshipTreeNode)
                        {
                            this.RelationshipTree.Selected = (RelationshipTreeNode)parameter;
                        }
                    }

                    this.RelationshipTree.RefreshCommands();
                }
            }

            internal SelectCommand(RelationshipTree RelationshipTree)
                : base(RelationshipTree)
            {
                this.CanExecute = true;
            }
        }

        public class AddCommand : Aras.ViewModel.Command
        {
            public RelationshipTree RelationshipTree
            {
                get
                {
                    return ((RelationshipTree)this.RelationshipTree);
                }
            }

            internal void UpdateCanExecute()
            {
                if(this.RelationshipTree.Selected != null)
                {
                    if (this.RelationshipTree.Selected.Item.CanUpdate)
                    {
                        this.CanExecute = true;
                    }
                    else
                    {
                        this.CanExecute = false;
                    }
                }
                else
                {
                    this.CanExecute = false;
                }
            }

            protected override void Run(IEnumerable<Control> Parameters)
            {
                if (this.RelationshipTree.Selected != null)
                {
                    this.RelationshipTree.ShowSearch = true;
                }
                else
                {
                    this.RelationshipTree.ShowSearch = false;
                }
            }

            internal AddCommand(RelationshipTree RelationshipTree)
                : base(RelationshipTree)
            {
            }
        }

        public class CutCommand : Aras.ViewModel.Command
        {
            public RelationshipTree RelationshipTree
            {
                get
                {
                    return ((RelationshipTree)this.Control);
                }
            }

            internal void UpdateCanExecute()
            {
                if ((this.RelationshipTree.Selected != null) && (this.RelationshipTree.Selected.Parent != null))
                {
                    if (((RelationshipTreeNode)this.RelationshipTree.Selected.Parent).Item.CanUpdate)
                    {
                        this.CanExecute = true;
                    }
                    else
                    {
                        this.CanExecute = false;
                    }
                }
                else
                {
                    this.CanExecute = false;
                }
            }

            protected override void Run(IEnumerable<Control> Parameters)
            {
                if ((this.RelationshipTree.Selected != null) && (this.RelationshipTree.Selected.Parent != null))
                {
                    // Store Related Item in CopyPaste Buffer
                    this.RelationshipTree.CopyPasteBuffer = this.RelationshipTree.Selected.Item;

                    // Update Parent Item
                    ((RelationshipTreeNode)this.RelationshipTree.Selected.Parent).Item.Update(this.RelationshipTree.Transaction);

                    // Delete Relationship
                    this.RelationshipTree.Selected.Relationship.Delete(this.RelationshipTree.Transaction);

                    // Refesh Parent Node
                    this.RelationshipTree.Selected.Parent.Refresh.Execute();

                    // Select Parent
                    this.RelationshipTree.Selected = (RelationshipTreeNode)this.RelationshipTree.Selected.Parent;

                    // Refesh Parent
                    this.RelationshipTree.Selected.RefreshChildren();

                    // Refresh Commands
                    this.RelationshipTree.RefreshCommands();
                }
            }

            internal CutCommand(RelationshipTree RelationshipTree)
                : base(RelationshipTree)
            {
            }
        }

        void Selected_ListChanged(object sender, EventArgs e)
        {
            if ((this.Selected != null) && (this.Search.Selected != null))
            {
                // Seach Selection - Add Selected Search Item as a child of Selected Tree Node

                // Update Parent Item
                this.Selected.Item.Update(this.Transaction);

                // Create Relationship to Selected Search Item
                //((RelationshipTreeNode)this.Selected).Store.Create(this.Transaction);
                //this.Search.Selected.First()

                // Refresh Parent Node
                this.Selected.Refresh.Execute();

                // Refresh Commands
                this.RefreshCommands();
            }

            // Close Search Dialogue
            this.ShowSearch = false;
        }

        public class CopyCommand : Aras.ViewModel.Command
        {
            public RelationshipTree RelationshipTree
            {
                get
                {
                    return ((RelationshipTree)this.Control);
                }
            }

            internal void UpdateCanExecute()
            {
                if (this.RelationshipTree.Selected != null)
                {
                    this.CanExecute = true;
                }
                else
                {
                    this.CanExecute = false;
                }
            }

            protected override void Run(IEnumerable<Control> Parameters)
            {
                if (this.RelationshipTree.Selected != null)
                {
                    // Store Related Item in CopyPaste Buffer
                    this.RelationshipTree.CopyPasteBuffer = this.RelationshipTree.Selected.Item;

                    // Refresh Commands
                    this.RelationshipTree.RefreshCommands();
                }
            }

            internal CopyCommand(RelationshipTree RelationshipTree)
                : base(RelationshipTree)
            {
            }
        }

        public class PasteCommand : Aras.ViewModel.Command
        {
            public RelationshipTree RelationshipTree
            {
                get
                {
                    return ((RelationshipTree)this.Control);
                }
            }

            internal void UpdateCanExecute()
            {
                if ((this.RelationshipTree.Selected != null) && (this.RelationshipTree.Selected.Parent != null) && (this.RelationshipTree.CopyPasteBuffer != null))
                {
                    if (((RelationshipTreeNode)this.RelationshipTree.Selected.Parent).Item.CanUpdate)
                    {
                        this.CanExecute = true;
                    }
                    else
                    {
                        this.CanExecute = false;
                    }
                }
                else
                {
                    this.CanExecute = false;
                }
            }

            protected override void Run(IEnumerable<Control> Parameters)
            {
                if ((this.RelationshipTree.Selected != null) && (this.RelationshipTree.CopyPasteBuffer != null))
                {
                    // Update Parent Item
                    this.RelationshipTree.Selected.Item.Update(this.RelationshipTree.Transaction);

                    // Create Relationship
                    //this.RelationshipTree.Selected.Store.Create(this.RelationshipTree.CopyPasteBuffer, this.RelationshipTree.Transaction);

                    // Refresh Parent Node
                    this.RelationshipTree.Selected.Refresh.Execute();

                    // Refresh Commands
                    this.RelationshipTree.RefreshCommands();
                }
            }

            internal PasteCommand(RelationshipTree RelationshipTree)
                : base(RelationshipTree)
            {
            }
        }

        public class DeleteCommand : Aras.ViewModel.Command
        {
            public RelationshipTree RelationshipTree
            {
                get
                {
                    return ((RelationshipTree)this.Control);
                }
            }

            internal void UpdateCanExecute()
            {
                if ((this.RelationshipTree.Selected != null) && (this.RelationshipTree.Selected.Parent != null))
                {
                    if (((RelationshipTreeNode)this.RelationshipTree.Selected.Parent).Item.CanUpdate)
                    {
                        this.CanExecute = true;
                    }
                    else
                    {
                        this.CanExecute = false;
                    }
                }
                else
                {
                    this.CanExecute = false;
                }
            }

            protected override void Run(IEnumerable<Control> Parameters)
            {
                if ((this.RelationshipTree.Selected != null) && (this.RelationshipTree.Selected.Parent != null))
                {
                    // Get Parent Node
                    RelationshipTreeNode parentnode = (RelationshipTreeNode)this.RelationshipTree.Selected.Parent;

                    // Get Node to Delete
                    RelationshipTreeNode todeletenode = (RelationshipTreeNode)this.RelationshipTree.Selected;

                    // Update Parent Item
                    parentnode.Item.Update(this.RelationshipTree.Transaction);

                    // Delete Relationship
                    todeletenode.Relationship.Delete(this.RelationshipTree.Transaction);

                    // Set Selected to Parent
                    this.RelationshipTree.Selected = parentnode;

                    // Refesh Parent Node
                    parentnode.RefreshChildren();

                    // Refresh Commands
                    this.RelationshipTree.RefreshCommands();
                }
            }

            internal DeleteCommand(RelationshipTree RelationshipTree)
                : base(RelationshipTree)
            {
            }
        }

        public class SaveCommand : Aras.ViewModel.Command
        {
            public RelationshipTree RelationshipTree
            {
                get
                {
                    return ((RelationshipTree)this.Control);
                }
            }

            internal void UpdateCanExecute()
            {
                if (this.RelationshipTree._transaction != null)
                {
                    this.CanExecute = true;
                }
                else
                {
                    this.CanExecute = false;
                }
            }

            protected override void Run(IEnumerable<Control> Parameters)
            {
                if (this.RelationshipTree._transaction != null)
                {
                    this.RelationshipTree._transaction.Commit(false);
                    this.RelationshipTree._transaction = null;

                    // Refresh Commands
                    this.RelationshipTree.RefreshCommands();
                }
            }

            internal SaveCommand(RelationshipTree RelationshipTree)
                : base(RelationshipTree)
            {
            }
        }

        public class UndoCommand : Aras.ViewModel.Command
        {
            public RelationshipTree RelationshipTree
            {
                get
                {
                    return ((RelationshipTree)this.Control);
                }
            }

            internal void UpdateCanExecute()
            {
                if (this.RelationshipTree._transaction != null)
                {
                    this.CanExecute = true;
                }
                else
                {
                    this.CanExecute = false;
                }
            }

            protected override void Run(IEnumerable<Control> Parameters)
            {
                if (this.RelationshipTree._transaction != null)
                {
                    this.RelationshipTree._transaction.RollBack();
                    this.RelationshipTree._transaction = null;

                    // Refresh Tree
                    this.RelationshipTree.Refresh.Execute();

                    // Refresh Commands
                    this.RelationshipTree.RefreshCommands();
                }
            }

            internal UndoCommand(RelationshipTree RelationshipTree)
                : base(RelationshipTree)
            {
            }
        }

        public class OutdentCommand : Aras.ViewModel.Command
        {
            public RelationshipTree RelationshipTree
            {
                get
                {
                    return ((RelationshipTree)this.Control);
                }
            }

            internal void UpdateCanExecute()
            {
                if ((this.RelationshipTree.Selected != null) && (this.RelationshipTree.Selected.Parent != null) && (this.RelationshipTree.Selected.Parent.Parent != null))
                {
                    if (((RelationshipTreeNode)this.RelationshipTree.Selected.Parent).Item.CanUpdate && ((RelationshipTreeNode)this.RelationshipTree.Selected.Parent.Parent).Item.CanUpdate)
                    {
                        this.CanExecute = true;
                    }
                    else
                    {
                        this.CanExecute = false;
                    }
                }
                else
                {
                    this.CanExecute = false;
                }
            }

            protected override void Run(IEnumerable<Control> Parameters)
            {
                if ((this.RelationshipTree.Selected != null) && (this.RelationshipTree.Selected.Parent != null) && (this.RelationshipTree.Selected.Parent.Parent != null))
                {
                    // Get Current Child Node
                    RelationshipTreeNode childnode = (RelationshipTreeNode)this.RelationshipTree.Selected;

                    // Get Child Item
                    Model.Item childitem = childnode.Item;

                    // Get Current Parent Node
                    RelationshipTreeNode currentparentnode = (RelationshipTreeNode)this.RelationshipTree.Selected.Parent;

                    // Get New Parent Node
                    RelationshipTreeNode newparentnode = (RelationshipTreeNode)this.RelationshipTree.Selected.Parent.Parent;

                    // Update Current Parent Item
                    currentparentnode.Item.Update(this.RelationshipTree.Transaction);

                    // Delete Current Relationship
                    childnode.Relationship.Delete(this.RelationshipTree.Transaction);

                    // Refesh Current Parent Node
                    currentparentnode.Refresh.Execute();

                    // UpdateNew Parent Item
                    newparentnode.Item.Update(this.RelationshipTree.Transaction);

                    // Add New Relationship
                    //newparentnode.Store.Create(childitem, this.RelationshipTree.Transaction);

                    // Refresh new Parent
                    newparentnode.Refresh.Execute();

                    // Select New Parent
                    this.RelationshipTree.Selected = newparentnode;

                    // Refresh Commands
                    this.RelationshipTree.RefreshCommands();
                }
            }

            internal OutdentCommand(RelationshipTree RelationshipTree)
                : base(RelationshipTree)
            {
            }
        }

        public class IndentCommand : Aras.ViewModel.Command
        {
            public RelationshipTree RelationshipTree
            {
                get
                {
                    return ((RelationshipTree)this.Control);
                }
            }

            internal void UpdateCanExecute()
            {
                if ((this.RelationshipTree.Selected != null) && (this.RelationshipTree.Selected.Parent != null) && !this.RelationshipTree.Selected.Parent.Children.First().Equals(this.RelationshipTree.Selected))
                {
                    if (this.CurrentParentNode.Item.CanUpdate && this.NewParentNode.Item.CanUpdate)
                    {
                        this.CanExecute = true;
                    }
                    else
                    {
                        this.CanExecute = false;
                    }
                }
                else
                {
                    this.CanExecute = false;
                }
            }

            private RelationshipTreeNode ChildNode
            {
                get
                {
                    return this.RelationshipTree.Selected;
                }
            }

            private RelationshipTreeNode CurrentParentNode
            {
                get
                {
                    return (RelationshipTreeNode)this.ChildNode.Parent;
                }
            }

            private RelationshipTreeNode NewParentNode
            {
                get
                {
                    RelationshipTreeNode newparentnode = null;

                    foreach (RelationshipTreeNode child in this.CurrentParentNode.Children)
                    {
                        if (child.Equals(this.ChildNode))
                        {
                            break;
                        }
                        else
                        {
                            newparentnode = child;
                        }
                    }

                    return newparentnode;
                }
            }

            protected override void Run(IEnumerable<Control> Parameters)
            {
                if ((this.RelationshipTree.Selected != null) && (this.RelationshipTree.Selected.Parent != null) && !this.RelationshipTree.Selected.Parent.Children.First().Equals(this.RelationshipTree.Selected))
                {
                    // Get Child Node
                    RelationshipTreeNode childnode = this.ChildNode;

                    // Get Current Parent Node
                    RelationshipTreeNode currentparantnode = this.CurrentParentNode;

                    // Work out new Parent Node
                    RelationshipTreeNode newparentnode = this.NewParentNode;

                    // Update Current Parent Item
                    currentparantnode.Item.Update(this.RelationshipTree.Transaction);

                    // Delete Current Relationship
                    childnode.Relationship.Delete(this.RelationshipTree.Transaction);

                    // Refesh Current Parent Node
                    currentparantnode.Refresh.Execute();

                    // Update New Parent Item
                    newparentnode.Item.Update(this.RelationshipTree.Transaction);

                    // Create new Relationship
                    //newparentnode.Store.Create(childnode.Item, this.RelationshipTree.Transaction);

                    // Refresh New Parent Node
                    newparentnode.Refresh.Execute();

                    // Select New Parent Node
                    this.RelationshipTree.Selected = newparentnode;

                    // Refresh Commands
                    this.RelationshipTree.RefreshCommands();
                }
            }

            internal IndentCommand(RelationshipTree RelationshipTree)
                : base(RelationshipTree)
            {
            }
        }

        public class SearchClosedCommand : Aras.ViewModel.Command
        {
            protected override void Run(IEnumerable<Control> Parameters)
            {
                ((RelationshipTree)this.Control).ShowSearch = false;
            }

            internal SearchClosedCommand(RelationshipTree RelationshipTree)
                :base(RelationshipTree)
            {
                this.CanExecute = true;
            }
        }
    }
}
