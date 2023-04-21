﻿using Avalonia.Controls;
using Avalonia.PropertyGrid.Model.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.PropertyGrid.Model.Extensions;

namespace Avalonia.PropertyGrid.Controls.Factories.Builtins
{
    internal class PropertyGridCheckedListFactory : AbstractPropertyGridFactory
    {
        public override int ImportPriority => base.ImportPriority - 100000;

        /// <summary>
        /// Handles the new property.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="propertyDescriptor">The property descriptor.</param>
        /// <returns>Control.</returns>
        public override Control HandleNewProperty(object target, PropertyDescriptor propertyDescriptor)
        {
            if (!propertyDescriptor.PropertyType.IsImplementFrom<ICheckedList>())
            {
                return null;
            }

            ICheckedList list = propertyDescriptor.GetValue(target) as ICheckedList;

            if (list == null)
            {
                return null;
            }

            var control = new CheckedListEdit();
            control.Items = list.SourceItems;

            control.SelectedItemsChanged += (s, e) =>
            {
                var items = control.SelectedItems;

                list.Clear();
                foreach (var item in items)
                {
                    list.SetChecked(item, true);
                }

                SetAndRaise(control, propertyDescriptor, target, list);
            };

            return control;
        }

        protected override void SetAndRaise(Control sourceControl, PropertyDescriptor propertyDescriptor, object component, object value)
        {
            base.SetAndRaise(sourceControl, propertyDescriptor, component, value);

            // check list is special case, so we force raise events
            propertyDescriptor.RaiseEvent(component);
        }

        public override bool HandlePropertyChanged(object target, PropertyDescriptor propertyDescriptor, Control control)
        {
            if (!propertyDescriptor.PropertyType.IsImplementFrom<ICheckedList>())
            {
                return false;
            }

            ValidateProperty(control, propertyDescriptor, target);

            if (control is CheckedListEdit c)
            {
                ICheckedList list = propertyDescriptor.GetValue(target) as ICheckedList;

                if (list != null)
                {
                    var old = c.EnableRaiseSelectedItemsChangedEvent;
                    c.EnableRaiseSelectedItemsChangedEvent = false;

                    try
                    {
                        c.SelectedItems = new object[] { };
                        c.Items = list.SourceItems;
                        c.SelectedItems = list.Items;
                    }
                    finally
                    {
                        c.EnableRaiseSelectedItemsChangedEvent = old;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
