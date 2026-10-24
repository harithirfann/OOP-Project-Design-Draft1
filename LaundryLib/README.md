# LaundryLib – Digital Laundry Queue Class Library (C#)

A C# class library for managing a shared hostel laundry room: washers and dryers, bookings, payments, a first-come-first-served waiting queue, and SMS/e-mail notifications.

## Folder structure
| Folder / file | Contents |
|---|---|
| `classes/` | The library – one `.cs` file per class (14 files) |
| `application code/` | `Program.cs` – console demo showing how the classes are used |
| `LaundryLib.csproj` | Project file that builds everything |

## How to run
Requires the .NET SDK (8 or newer): https://dotnet.microsoft.com/download

```
cd LaundryLib
dotnet run
```

## Classes
| Class | Kind | Purpose |
|---|---|---|
| User | abstract class | Base for anyone using the laundry room |
| Student, Staff | subclasses of User | Different booking limits (2 and 3) |
| Machine | abstract class | Base for laundry machines |
| Washer, Dryer | subclasses of Machine | Different cycle times (30 and 40 min) |
| MachineStatus | enum | Available / InUse / OutOfOrder |
| Booking | class | Links a user, a machine and a payment |
| Payment | class | Payment record for a booking |
| QueueManager | class | Waiting list when no machine is free |
| LaundryRoom | class | Main controller – owns machines and queue |
| INotifier | interface | Sends alerts to users |
| EmailNotifier, SmsNotifier | implement INotifier | E-mail / SMS alerts |
