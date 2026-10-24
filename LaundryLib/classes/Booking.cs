using System;

namespace LaundryLib
{
    public class Booking
    {
        public int BookingNo { get; set; }
        public User User { get; set; }
        public Machine Machine { get; set; }
        public Payment Payment { get; set; }
        public DateTime StartTime { get; set; }
        public bool Done { get; private set; }

        public Booking(int bookingNo, User user, Machine machine,
                       Payment payment, DateTime startTime)
        {
            BookingNo = bookingNo;
            User = user;
            Machine = machine;
            Payment = payment;
            StartTime = startTime;
            Done = false;
        }

        public DateTime EndTime() => StartTime.AddMinutes(Machine.CycleMinutes());

        public void Complete()
        {
            if (Done) return;
            Done = true;
            Machine.Finish();
            User.ActiveBookings--;
        }
    }
}
