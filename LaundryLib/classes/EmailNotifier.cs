using System;

namespace LaundryLib
{
    public class EmailNotifier : INotifier
    {
        public void Send(User user, string message)
            => Console.WriteLine($"[EMAIL to {user.Email}] {message}");
    }
}
