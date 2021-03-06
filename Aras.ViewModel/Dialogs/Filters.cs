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

namespace Aras.ViewModel.Dialogs
{
    public class Filters : Dialog
    {
        private const Int32 NoColumns = 3;
        private const Int32 RowHeight = 60;
        private const Int32 ToolbarHeight = 50;

        [ViewModel.Attributes.Command("ApplyFilter")]
        public FilterCommand ApplyFilter { get; private set; }

        [ViewModel.Attributes.Command("ClearFilter")]
        public ClearCommand ClearFilter { get; private set; }

        public IEnumerable<Model.PropertyType> PropertyTypes { get; private set; }

        public Containers.BorderContainer Layout { get; private set; }

        public Containers.Toolbar Toolbar { get; private set; }

        public Button ClearButton { get; private set; }

        public Button FilterButton { get; private set; }

        public Containers.TableContainer Table { get; private set; }

        public Model.Condition Condition()
        {
            Model.Condition ret = null;

            foreach (Control prop in this.Table.Children)
            {
                Model.Condition condition = null;

                switch (prop.GetType().Name)
                {
                     case "Boolean":

                        if (((Properties.Boolean)prop).Value != null)
                        {
                            if ((System.Boolean)((Properties.Boolean)prop).Value)
                            {
                                condition = Aras.Conditions.Eq(((Properties.Boolean)prop).PropertyType.Name, "1");
                            }
                            else
                            {
                                condition = Aras.Conditions.Eq(((Properties.Boolean)prop).PropertyType.Name, "0");
                            }
                        }

                        break;
                    case "Date":
                        break;
                    case "Decimal":

                        if (((Properties.Decimal)prop).Value != null)
                        {
                            condition = Aras.Conditions.Eq(((Properties.Decimal)prop).PropertyType.Name, ((Properties.Decimal)prop).Value);
                        }

                        break;
                    case "Float":

                        if (((Properties.Float)prop).Value != null)
                        {
                            condition = Aras.Conditions.Eq(((Properties.Float)prop).PropertyType.Name, ((Properties.Float)prop).Value);
                        }

                        break;
                    case "Integer":

                        if (((Properties.Integer)prop).Value != null)
                        {
                            condition = Aras.Conditions.Eq(((Properties.Integer)prop).PropertyType.Name, ((Properties.Integer)prop).Value);
                        }

                        break;
                    case "Item":

                        if (((Properties.Item)prop).PropetyItem != null)
                        {
                            condition = Aras.Conditions.Eq(((Properties.Item)prop).PropertyType.Name, ((Properties.Item)prop).PropetyItem.ID);
                        }

                        break;
                    case "List":

                        if (!String.IsNullOrEmpty(((Properties.List)prop).Value))
                        {
                            condition = Aras.Conditions.Eq(((Properties.List)prop).PropertyType.Name, ((Properties.List)prop).Value);
                        }

                        break;
                    case "Sequence":

                        if (!String.IsNullOrEmpty(((Properties.Sequence)prop).Value))
                        {
                            condition = Aras.Conditions.Like(((Properties.Sequence)prop).PropertyType.Name, ((Properties.Sequence)prop).Value);
                        }

                        break;
                    case "String":


                        if (!String.IsNullOrEmpty(((Properties.String)prop).Value))
                        {
                            condition = Aras.Conditions.Like(((Properties.String)prop).PropertyType.Name, ((Properties.String)prop).Value);
                        }

                        break;
                    case "Text":

                        if (!String.IsNullOrEmpty(((Properties.Text)prop).Value))
                        {
                            condition = Aras.Conditions.Like(((Properties.Text)prop).PropertyType.Name, ((Properties.Text)prop).Value);
                        }

                        break;
                    case "Federated":

                        if (!String.IsNullOrEmpty(((Properties.Federated)prop).Value))
                        {
                            condition = Aras.Conditions.Like(((Properties.Federated)prop).PropertyType.Name, ((Properties.Federated)prop).Value);
                        }

                        break;
                    default:
                        throw new Model.Exceptions.ArgumentException("Property Type not implemented: " + prop.GetType().Name);
                }

                if (condition != null)
                {
                    if (ret == null)
                    {
                        ret = condition;
                    }
                    else
                    {
                        if (ret is Model.Conditions.And)
                        {
                            ((Model.Conditions.And)ret).Add(condition);
                        }
                        else
                        {
                            ret = Aras.Conditions.And(ret, condition);
                        }
                    }
                }
            }

            if (ret == null)
            {
                ret = Aras.Conditions.All();
            }

            return ret;
        }

        public void Clear()
        {
            foreach(Control prop in this.Table.Children)
            {
                switch (prop.GetType().Name)
                {
                    case "Boolean":
                        ((Properties.Boolean)prop).Value = null;
                        break;
                    case "Date":
                        ((Properties.Date)prop).Value = null;
                        break;
                    case "Decimal":
                        ((Properties.Decimal)prop).Value = null;
                        break;
                    case "Federated":
                        ((Properties.Federated)prop).Value = null;
                        break;
                    case "Float":
                        ((Properties.Float)prop).Value = null;
                        break;
                    case "Integer":
                        ((Properties.Integer)prop).Value = null;
                        break;
                    case "Item":
                        ((Properties.Item)prop).PropetyItem = null;
                        break;
                    case "List":
                        ((Properties.List)prop).Value = null;
                        break;
                    case "Sequence":
                        ((Properties.Sequence)prop).Value = null;
                        break;
                    case "String":
                        ((Properties.String)prop).Value = null;
                        break;
                    case "Text":
                        ((Properties.Text)prop).Value = null;
                        break;
                    default:
                        throw new Model.Exceptions.ArgumentException("Property Type not implemented: " + prop.GetType().Name);
                }
            }
        }

        
        public Filters(Control Parent, IEnumerable<Model.PropertyType> PropertyTypes)
            :base(Parent)
        {
            // Store PropertyTypes
            this.PropertyTypes = PropertyTypes;

            // Create Commands
            this.ApplyFilter = new FilterCommand(this);
            this.ClearFilter = new ClearCommand(this);

            // Set Title
            this.Title = "Search Filters";

            // Create Layout
            this.Layout = new Containers.BorderContainer(this.Session);
            this.Content = this.Layout;

            this.Layout.Children.NotifyListChanged = false;

            // Build Toolbar
            this.Toolbar = new Containers.Toolbar(this.Session);
            this.Toolbar.Region = Regions.Top;
            this.Layout.Children.Add(this.Toolbar);

            this.Toolbar.Children.NotifyListChanged = false;

            // Add Filter Button
            this.FilterButton = new Button(this.Session);
            this.Toolbar.Children.Add(this.FilterButton);
            this.FilterButton.Icon = "Search";
            this.FilterButton.Tooltip = "Apply Filters";
            this.FilterButton.Command = this.ApplyFilter;

            // Add Clear Button
            this.ClearButton = new Button(this.Session);
            this.Toolbar.Children.Add(this.ClearButton);
            this.ClearButton.Icon = "ClearFilter";
            this.ClearButton.Tooltip = "Clear Filters";
            this.ClearButton.Command = this.ClearFilter;

            this.Toolbar.Children.NotifyListChanged = true;

            // Build Table
            this.Table = new Containers.TableContainer(this.Session);
            this.Table.Columns = NoColumns;
            this.Table.Children.NotifyListChanged = false;

            foreach (Model.PropertyType proptype in this.PropertyTypes)
            {
                ViewModel.Property viewmodelproperty = this.Session.CreateProperty(proptype, true);
                viewmodelproperty.IntermediateChanges = true;
                viewmodelproperty.Enabled = true;
                this.Table.Children.Add(viewmodelproperty);
            }

            this.Table.Children.NotifyListChanged = true;

            this.Table.Region = Regions.Center;
            this.Layout.Children.Add(this.Table);

            this.Layout.Children.NotifyListChanged = true;

            // Set Width and Height
            this.Width = 600;
            this.Height = (((this.PropertyTypes.Count() / NoColumns) + (this.PropertyTypes.Count() % NoColumns > 0 ? 1 : 0)) * RowHeight) + ToolbarHeight;                                                 
        }

        public class FilterCommand : Aras.ViewModel.Command
        {
            protected override void Run(IEnumerable<Control> Parameters)
            {
                // Close Dialogue
                ((Filters)this.Control).Open = false;
                
                this.CanExecute = true;
            }

            internal FilterCommand(Control Control)
                : base(Control)
            {
                this.CanExecute = true;
            }
        }

        public class ClearCommand : Aras.ViewModel.Command
        {
            protected override void Run(IEnumerable<Control> Parameters)
            {
                // Clear Filters
                ((Filters)this.Control).Clear();
                
                // Close Dialogue
                ((Filters)this.Control).Open = false;

                this.CanExecute = true;
            }

            internal ClearCommand(Control Control)
                : base(Control)
            {
                this.CanExecute = true;
            }
        }
    }
}
