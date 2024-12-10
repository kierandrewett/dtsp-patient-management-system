using Azure;
using Microsoft.Identity.Client;
using PMS.Components;
using PMS.Context;
using PMS.Models;
using PMS.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PMS.Dialogs
{
    /// <summary>
    /// Interaction logic for PMSPatientNoteManagerWindow.xaml
    /// </summary>
    public partial class PMSPatientNoteManagerWindow : PMSLockableWindow
    {
        public string PatientID { get; set; }

        public Patient? Patient
        {
            get => Patient.GetPatientByID(PatientID);
        }

        public NotesDataContext Notes { get; set; }

        public override void InvalidateDataContext()
        {
            Notes = null;
            Notes = new NotesDataContext(PatientID);

            DidUpdateProperty("Notes");
        }

        public PMSPatientNoteManagerWindow(string patientID) : base()
        {
            InitializeComponent();

            this.PatientID = patientID;

            InvalidateDataContext();
            DataContext = this;

            AuxiliaryTitle = $"Patient Note Manager - {Patient?.FormatFullName()}";
            Title = ComputedTitle;
        }
    }
}
