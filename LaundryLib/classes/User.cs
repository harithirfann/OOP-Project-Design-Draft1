using System;

namespace LaundryLib
{
    public abstract class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int ActiveBookings { get; set; }

        protected User(string id, string name, string email, string phone)
        {
            Id = id;
            Name = name;
            Email = email;
            Phone = phone;
            ActiveBookings = 0;
        }

        public abstract int MaxBookings();

        public bool CanBook() => ActiveBookings < MaxBookings();
    }
}
