using PMS.Models;

namespace PMS.Context
{
    public class TimetableDataContext : SchedulingDataContext
    {
        public TimetableDataContext(int doctorID)
        {
            DataSource = Appointment.GetAllAppointmentsForDoctorID(doctorID) ?? Array.Empty<Appointment>();
        }
    }
}
