using System;

namespace LaundryLib
{
    public class Dryer : Machine
    {
        public int Temperature { get; set; }

        public Dryer(string id, double price, int temperature)
            : base(id, price)
        {
            Temperature = temperature;
        }

        public override int CycleMinutes() => 40;
    }
}
