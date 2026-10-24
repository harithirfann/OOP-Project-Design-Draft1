using System;

namespace LaundryLib
{
    public abstract class Machine
    {
        public string Id { get; set; }
        public double Price { get; set; }
        public MachineStatus Status { get; set; }

        protected Machine(string id, double price)
        {
            Id = id;
            Price = price;
            Status = MachineStatus.Available;
        }

        public abstract int CycleMinutes();

        public bool IsFree() => Status == MachineStatus.Available;

        public void Start()
        {
            if (!IsFree())
                throw new InvalidOperationException($"Machine {Id} is not available.");
            Status = MachineStatus.InUse;
        }

        public void Finish()
        {
            //don't flip a broken machine back to available
            if (Status == MachineStatus.InUse)
                Status = MachineStatus.Available;
        }
    }
}
