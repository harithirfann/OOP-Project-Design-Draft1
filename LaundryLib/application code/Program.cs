using System;
using LaundryLib;

class Program
{
    static void Main()
    {
        //Set up the laundry room (SMS alerts) with two washers and one dryer
        LaundryRoom room = new LaundryRoom("Village 4 Laundry Room", new SmsNotifier());
        room.AddMachine(new Washer("W1", 4.00, 8));
        room.AddMachine(new Washer("W2", 4.00, 10));
        room.AddMachine(new Dryer("D1", 3.00, 60));

        //Create users
        Student ali  = new Student("U01", "Ali",  "ali@utp.edu.my",  "011-1111111", "22001234", "V4");
        Student amir = new Student("U02", "Amir", "amir@utp.edu.my", "012-2222222", "22003456", "V4");
        Student siti = new Student("U03", "Siti", "siti@utp.edu.my", "013-3333333", "22005678", "V5");
        Staff raju   = new Staff("S01", "Mr. Raju", "raju@utp.edu.my", "014-4444444", "Facilities");

        //Ali and Amir take both washers; Siti has to join the washer queue
        Booking b1 = room.Book(ali, "Washer", "E-Wallet");
        room.Book(amir, "Washer", "Cash");
        room.Book(siti, "Washer", "Card");

        //Raju uses the dryer
        Booking b3 = room.Book(raju, "Dryer", "Card");
        room.ShowStatus();

        //Ali's wash finishes -> Siti (first in queue) is told it's her turn
        room.FinishBooking(b1);
        room.Book(siti, "Washer", "Card");

        //Switch to e-mail alerts at run time (polymorphism through INotifier)
        room.Notifier = new EmailNotifier();
        room.FinishBooking(b3);
        room.Book(amir, "Dryer", "Cash");

        //Amir now has 2 active bookings (student limit) -> library rejects a third
        try
        {
            room.Book(amir, "Washer", "Cash");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

        room.ShowStatus();
    }
}
