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

namespace Aras.ViewModel.Cells
{
    public class Decimal : Cell
    {
        const System.Decimal DefaultMinValue = System.Decimal.MinValue;
        const System.Decimal DefaultMaxValue = System.Decimal.MaxValue;

        private System.Decimal _minValue;
        [Attributes.Property("MinValue", Attributes.PropertyTypes.Decimal, true)]
        public System.Decimal MinValue
        {
            get
            {
                return this._minValue;
            }
            set
            {
                this._minValue = value;
                this.OnPropertyChanged("MinValue");
            }
        }

        private System.Decimal _maxValue;
        [Attributes.Property("MaxValue", Attributes.PropertyTypes.Decimal, true)]
        public System.Decimal MaxValue
        {
            get
            {
                return this._maxValue;
            }
            set
            {
                this._maxValue = value;
                this.OnPropertyChanged("MaxValue");
            }
        }

        public override void SetValue(object Value)
        {
            base.SetValue(Value);

            if (Value == null)
            {
                this.Value = null;
            }
            else
            {
                if (Value is System.Decimal)
                {
                    this.Value = ((System.Decimal)Value).ToString();
                }
                else
                {
                    throw new Model.Exceptions.ArgumentException("Value must be System.Decimal");
                }
            }
        }

        protected override void ProcessUpdateValue(string Value)
        {
            if (Value == null)
            {
                this.SetValue(null);
            }
            else
            {
                System.Decimal thisval = 0;

                if (System.Decimal.TryParse(Value, out thisval))
                {
                    this.SetValue(thisval);
                }
            }
        }

        protected override void CheckBinding(object Binding)
        {
            base.CheckBinding(Binding);

            if (!(Binding is Model.Properties.Decimal))
            {
                throw new Model.Exceptions.ArgumentException("Binding must be Aras.Model.Properties.Decimal");
            }
        }

        internal Decimal(Column Column, Row Row)
            :base(Column, Row)
        {
            this._minValue = DefaultMinValue;
            this._maxValue = DefaultMaxValue;
        }
    }
}
