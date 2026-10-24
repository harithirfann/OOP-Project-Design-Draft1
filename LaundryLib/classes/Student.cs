using System;

namespace LaundryLib
{
    public class Student : User
    {
        public string MatricNo { get; set; }
        public string Hostel { get; set; }

        public Student(string id, string name, string email, string phone,
                       string matricNo, string hostel)
            : base(id, name, email, phone)
        {
            MatricNo = matricNo;
            Hostel = hostel;
        }

        public override int MaxBookings() => 2;
    }
}
