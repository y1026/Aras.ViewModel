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
    [Attributes.ClientControl("Aras.View.ListValue")]
    public class ListValue : Control
    {
        private System.String _value;
        [Attributes.Property("Value", Attributes.PropertyTypes.String, true)]
        public System.String Value 
        { 
            get
            {
                return this._value;
            }
            set 
            { 
                if (System.String.Compare(this._value, value) != 0)
                {
                    this._value = value;
                    this.OnPropertyChanged("Value");
                }
            }
        }

        private System.String _label;
        [Attributes.Property("Label", Attributes.PropertyTypes.String, true)]
        public System.String Label
        {
            get
            {
                return this._label;
            }
            set
            {
                if (System.String.Compare(this._label, value) != 0)
                {
                    this._label = value;
                    this.OnPropertyChanged("Label");
                }
            }
        }

        protected override void CheckBinding(object Binding)
        {
            base.CheckBinding(Binding);

            if (Binding != null)
            {
                if (!(Binding is Model.Relationships.Value))
                {
                    throw new Model.Exceptions.ArgumentException("Binding must be of type Aras.Model.ListValue");
                }
            }
        }

        protected override void AfterBindingChanged()
        {
            base.AfterBindingChanged();

            if (this.Binding != null)
            {
                this.Value = (System.String)((Model.Relationships.Value)this.Binding).Property("value").Value;
                this.Label = (System.String)((Model.Relationships.Value)this.Binding).Property("label").Value;
            }
            else
            {
                this.Value = null;
                this.Label = null;
            }
        }

        protected override void BeforeBindingChanged()
        {
            base.BeforeBindingChanged();

            if (this.Binding != null)
            {
                this.Value = null;
                this.Label = null;
            }
        }

        public override string ToString()
        {
            return this.Value;
        }

        public ListValue(Manager.Session Session)
            :base(Session)
        {

        }
    }
}
