using System;

namespace LaundryLib
{
    public class Staff : User
    {
        public string Department { get; set; }

        public Staff(string id, string name, string email, string phone,
                     string department)
            : base(id, name, email, phone)
        {
            Department = department;
        }

        public override int MaxBookings() => 3;
    }
}
