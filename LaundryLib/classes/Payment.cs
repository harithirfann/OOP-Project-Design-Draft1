using System;

namespace LaundryLib
{
    public class Payment
    {
        public double Amount { get; set; }
        public string Method { get; set; }
        public bool Paid { get; private set; }

        public Payment(double amount, string method)
        {
            Amount = amount;
            Method = method;
            Paid = false;
        }

        public bool Pay()
        {
            if (Paid) return true;
            if (Amount < 0 || string.IsNullOrWhiteSpace(Method)) return false;

            //payment gateway would be called here
            Paid = true;
            return Paid;
        }
    }
}
