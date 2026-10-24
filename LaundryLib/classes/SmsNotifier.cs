using System;

namespace LaundryLib
{
    public class SmsNotifier : INotifier
    {
        public void Send(User user, string message)
            => Console.WriteLine($"[SMS to {user.Phone}] {message}");
    }
}
