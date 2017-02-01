﻿/*  
  Aras.ViewModel provides a .NET library for building Aras Innovator Applications

  Copyright (C) 2015 Processwall Limited.

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU Affero General Public License as published
  by the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Affero General Public License for more details.

  You should have received a copy of the GNU Affero General Public License
  along with this program.  If not, see http://opensource.org/licenses/AGPL-3.0.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Aras.ViewModel
{
    public enum Regions { Top=1, Bottom=2, Right=3, Left=4, Center=5, Leading=6, Trailing=7 };

    public abstract class Control : IEquatable<Control>, INotifyPropertyChanged
    {
        public Guid ID { get; private set; }

        [Attributes.Property("Region", Attributes.PropertyTypes.Int32, true)]
        public Regions Region { get; set; }

        [Attributes.Property("Tooltip", Attributes.PropertyTypes.String, true)]
        public System.String Tooltip { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String Name)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(Name);
                this.PropertyChanged(this, args);
            }
        }

        [ViewModel.Attributes.Command("Refresh")]
        public RefreshCommand Refresh { get; private set; }

        [ViewModel.Attributes.Command("Close")]
        public CloseCommand Close { get; private set; }

        private Boolean _enabled;
        [Attributes.Property("Enabled", Attributes.PropertyTypes.Boolean, true)]
        public Boolean Enabled
        {
            get
            {
                return this._enabled;
            }
            set
            {
                if (this._enabled != value)
                {
                    this._enabled = value;
                    this.OnPropertyChanged("Enabled");
                }
            }
        }

        private Boolean _inError;
        [Attributes.Property("InError", Attributes.PropertyTypes.Boolean, true)]
        public Boolean InError
        {
            get
            {
                return this._inError;
            }
            private set
            {
                if (this._inError != value)
                {
                    this._inError = value;
                    this.OnPropertyChanged("InError");
                }
            }
        }

        private String _errorMessage;
        [Attributes.Property("ErrorMessage", Attributes.PropertyTypes.String, true)]
        public String ErrorMessage
        {
            get
            {
                return this._errorMessage;
            }
            private set
            {
                if (this._errorMessage != value)
                {
                    this._errorMessage = value;
                    this.OnPropertyChanged("ErrorMessage");
                }
            }
        }

        protected void ResetError()
        {
            this.InError = false;
            this.ErrorMessage = null;
        }

        protected void OnError(String ErrorMessage)
        {
            this.InError = true;
            this.ErrorMessage = ErrorMessage;
        }

        protected virtual void CheckBinding(Object Binding)
        {

        }

        private Object _binding;
        public Object Binding
        {
            get
            {
                return this._binding;
            }
            set
            {
                if (this._binding == null)
                {
                    if (value != null)
                    {
                        this.CheckBinding(value);
                        this.BeforeBindingChanged();
                        this._binding = value;
                        this.AfterBindingChanged();
                        this.OnPropertyChanged("Binding");
                    }
                }
                else
                {
                    if (!this._binding.Equals(value))
                    {
                        this.CheckBinding(value);
                        this.BeforeBindingChanged();
                        this._binding = value;
                        this.AfterBindingChanged();
                        this.OnPropertyChanged("Binding");
                    }
                }
            }
        }

        protected virtual void AfterBindingChanged()
        {

        }

        protected virtual void BeforeBindingChanged()
        {

        }

        private Dictionary<String, System.Reflection.PropertyInfo> _commandInfoCache;
        private Dictionary<String, System.Reflection.PropertyInfo> CommandInfoCache
        {
            get
            {
                if (this._commandInfoCache == null)
                {
                    this._commandInfoCache = new Dictionary<String, System.Reflection.PropertyInfo>();

                    foreach (System.Reflection.PropertyInfo propinfo in this.GetType().GetProperties())
                    {
                        object[] customattrs = propinfo.GetCustomAttributes(typeof(Attributes.Command), true);

                        foreach (object customattr in customattrs)
                        {

                            this._commandInfoCache[((Attributes.Command)customattr).Name] = propinfo;
                            break;

                        }
                    }
                }

                return this._commandInfoCache;
            }
        }

        public IEnumerable<String> Commands
        {
            get
            {
                return this.CommandInfoCache.Keys;
            }
        }

        public Command GetCommand(String Name)
        {
            return (Command)this.CommandInfoCache[Name].GetValue(this);
        }

        private Dictionary<String, PropertyDetails> _propertyInfoCache;
        private Dictionary<String, PropertyDetails> PropertyInfoCache
        {
            get
            {
                if (this._propertyInfoCache == null)
                {
                    this._propertyInfoCache = new Dictionary<String, PropertyDetails>();

                    foreach (System.Reflection.PropertyInfo propinfo in this.GetType().GetProperties())
                    {
                        object[] customattrs = propinfo.GetCustomAttributes(typeof(Attributes.Property), true);

                        foreach (object customattr in customattrs)
                        {
                            this._propertyInfoCache[((Attributes.Property)customattr).Name] = new PropertyDetails(propinfo, (Attributes.Property)customattr);
                            break;
                        }
                    }
                }

                return this._propertyInfoCache;
            }
        }

        public IEnumerable<String> Properties
        {
            get
            {
                return this.PropertyInfoCache.Keys;
            }
        }

        public Boolean HasProperty(String Name)
        {
            return this.PropertyInfoCache.ContainsKey(Name);
        }

        public object GetPropertyValue(String Name)
        {
            return this.PropertyInfoCache[Name].PropertyInfo.GetValue(this);
        }

        public void SetPropertyValue(String Name, object value)
        {
            this.PropertyInfoCache[Name].PropertyInfo.SetValue(this, value);
        }

        public Boolean GetPropertyReadOnly(String Name)
        {
            return (this.PropertyInfoCache[Name].Attribute.ReadOnly);
        }

        public Attributes.PropertyTypes GetPropertyType(String Name)
        {
            return this.PropertyInfoCache[Name].Attribute.Type;
        }

        public IEnumerable<Control> Controls
        {
            get
            {
                List<Control> controls = new List<Control>();

                foreach (String property in this.Properties)
                {
                    object propertyvalue = this.GetPropertyValue(property);

                    if (propertyvalue is Control)
                    {
                        Control thiscontrol = (Control)propertyvalue;

                        if (!controls.Contains(thiscontrol))
                        {
                            controls.Add(thiscontrol);
                        }
                    }
                    else if (propertyvalue is IEnumerable<Control>)
                    {
                        IEnumerable<Control> thiscontrols = (IEnumerable<Control>)propertyvalue;

                        foreach (Control thiscontrol in thiscontrols)
                        {
                            if (!controls.Contains(thiscontrol))
                            {
                                controls.Add(thiscontrol);
                            }
                        }
                    }
                }

                return controls;
            }
        }

        protected virtual void RefreshControl()
        {

        }

        protected virtual void CloseControl()
        {

        }

        public bool Equals(Control other)
        {
            if (other == null)
            {
                return false;
            }
            else
            {
                return this.ID.Equals(other.ID);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                if (obj is Control)
                {
                    return this.ID.Equals(((Control)obj).ID);
                }
                else
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public Control()
        {
            this.ID = Guid.NewGuid();
            this.Refresh = new RefreshCommand(this);
            this.Close = new CloseCommand(this);
            this.ErrorMessage = null;
            this.InError = false;
            this.Region = Regions.Center;
        }

        public class RefreshCommand : Aras.ViewModel.Command
        {
            public Control Control { get; private set; }

            protected override void Run(IEnumerable<Control> Parameters)
            {
                this.CanExecute = false;
                this.Control.RefreshControl();
                this.CanExecute = true;
            }

            internal RefreshCommand(Control Control)
            {
                this.Control = Control;
                this.CanExecute = true;
            }
        }

        public class CloseCommand : Aras.ViewModel.Command
        {
            public Control Control { get; private set; }

            protected override void Run(IEnumerable<Control> Parameters)
            {
                this.Control.CloseControl();
            }

            internal CloseCommand(Control Control)
            {
                this.Control = Control;
                this.CanExecute = true;
            }
        }
    }
}
