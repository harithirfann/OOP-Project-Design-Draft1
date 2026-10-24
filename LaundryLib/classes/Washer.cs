using System;

namespace LaundryLib
{
    public class Washer : Machine
    {
        public double CapacityKg { get; set; }

        public Washer(string id, double price, double capacityKg)
            : base(id, price)
        {
            CapacityKg = capacityKg;
        }

        public override int CycleMinutes() => 30;
    }
}
