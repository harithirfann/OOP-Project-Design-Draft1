using System;
using System.Collections.Generic;
using System.Linq;

namespace LaundryLib
{
    public class LaundryRoom
    {
        public string Name { get; set; }
        public List<Machine> Machines { get; } = new List<Machine>();
        public List<Booking> Bookings { get; } = new List<Booking>();
        public QueueManager Queue { get; } = new QueueManager();
        public INotifier Notifier { get; set; }
        public int NextBookingNo { get; private set; } = 1;

        //remembers which machine type each queued user is waiting for
        private readonly Dictionary<User, string> waitingFor = new Dictionary<User, string>();

        public LaundryRoom(string name, INotifier notifier)
        {
            Name = name;
            Notifier = notifier;
        }

        public void AddMachine(Machine m)
        {
            if (m == null) throw new ArgumentNullException(nameof(m));
            Machines.Add(m);
        }

        public Machine FindFreeMachine(string type)
        {
            return Machines.FirstOrDefault(m => m.IsFree() && IsType(m, type));
        }

        //returns null if the user had to join the queue instead
        public Booking Book(User user, string type, string payMethod)
        {
            if (!user.CanBook())
                throw new InvalidOperationException(
                    $"{user.Name} has reached the limit of {user.MaxBookings()} active bookings.");

            type = type.ToLowerInvariant();

            if (waitingFor.TryGetValue(user, out string current) && current != type)
                throw new InvalidOperationException(
                    $"{user.Name} is already waiting for a {current}.");

            //a user can only take a free machine if enough people ahead of them
            //in the queue (for the same type) are already covered by free machines
            List<User> ahead = WaitingFor(type);
            int rank = ahead.IndexOf(user);
            if (rank < 0) rank = ahead.Count;

            Machine machine = rank < FreeCount(type) ? FindFreeMachine(type) : null;

            if (machine == null)
            {
                Queue.Join(user);
                waitingFor[user] = type;
                Notifier.Send(user,
                    $"No {type} available for you yet. You are #{rank + 1} in the {type} queue.");
                return null;
            }

            var payment = new Payment(machine.Price, payMethod);
            if (!payment.Pay())
                throw new InvalidOperationException("Payment failed.");

            //they got a machine so they're no longer waiting
            Queue.Waiting.Remove(user);
            waitingFor.Remove(user);

            machine.Start();
            user.ActiveBookings++;

            var booking = new Booking(NextBookingNo++, user, machine, payment, DateTime.Now);
            Bookings.Add(booking);

            Notifier.Send(user,
                $"Booking #{booking.BookingNo} confirmed on {machine.Id}. Ready at {booking.EndTime():HH:mm}.");
            return booking;
        }

        public void FinishBooking(Booking b)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (b.Done) return;

            b.Complete();
            Notifier.Send(b.User, $"Booking #{b.BookingNo} finished. Please collect your laundry.");

            //with one more machine free, one more person in line gets their turn
            string type = b.Machine.GetType().Name.ToLowerInvariant();
            List<User> waiting = WaitingFor(type);
            int free = FreeCount(type);

            if (free > 0 && waiting.Count >= free)
                Notifier.Send(waiting[free - 1], $"A {type} is free now. It's your turn to book!");
        }

        public void ShowStatus()
        {
            Console.WriteLine($"=== {Name} ===");
            foreach (var m in Machines)
                Console.WriteLine($"{m.GetType().Name,-7} {m.Id,-5} {m.Status,-11} RM{m.Price:F2}");
            Console.WriteLine($"Waiting in queue: {Queue.Waiting.Count}");
        }

        private static bool IsType(Machine m, string type)
            => m.GetType().Name.Equals(type, StringComparison.OrdinalIgnoreCase);

        private int FreeCount(string type)
            => Machines.Count(m => m.IsFree() && IsType(m, type));

        //queued users for one machine type, in the order they joined
        private List<User> WaitingFor(string type)
            => Queue.Waiting.Where(u => waitingFor.TryGetValue(u, out string t) && t == type).ToList();
    }
}
