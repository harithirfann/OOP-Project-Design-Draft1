#nullable disable
using System;
using LaundryLib;

namespace LaundryGui
{
    // Same job as SmsNotifier / EmailNotifier, but writes to the on-screen log
    // instead of the console (a WinForms app has no console).
    public class GuiNotifier : INotifier
    {
        private readonly Action<string> log;
        private readonly bool email;

        public GuiNotifier(Action<string> log, bool email)
        {
            this.log = log;
            this.email = email;
        }

        public void Send(User user, string message)
        {
            if (email)
                log($"[EMAIL to {user.Email}] {message}");
            else
                log($"[SMS to {user.Phone}] {message}");
        }
    }
}
