using PMS.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace PMS.Components
{
    public class PMSWindow : Window
    {
        public AccessibilityController AccessibilityController { get; set; }

        public PMSWindow()
        {
            AccessibilityController = new(this);
        }
    }
}
