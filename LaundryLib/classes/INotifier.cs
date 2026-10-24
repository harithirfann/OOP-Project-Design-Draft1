using System;

namespace LaundryLib
{
    public interface INotifier
    {
        void Send(User user, string message);
    }
}
