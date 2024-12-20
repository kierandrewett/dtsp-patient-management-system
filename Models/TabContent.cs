﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PMS.Models
{
    public class TabContent<T>
    {
        public T Value { get; set; }
        public string Name { get; set; }
        
        public Func<object?, FrameworkElement> RenderContent { get; set; }

        private Func<T, bool> _VisibleFn;

        public bool IsVisible
        {
            get { return this._VisibleFn(Value); }
        }

        public TabContent(T Value, string Name, Func<object?, FrameworkElement> RenderContent, Func<T, bool> VisibleFn)
        {
            this.Value = Value;
            this.Name = Name;
            this.RenderContent = RenderContent;
            this._VisibleFn = VisibleFn;
        }
    }
}
