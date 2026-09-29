using System;
using System.Globalization;
using System.Threading.Tasks;
using HotelManagementSystem.Core;
using HotelManagementSystem.Domain.Exceptions;

namespace HotelManagementSystem
{
    public class Program
    {
        private static readonly IHotelManager HotelEngine = new HotelManager();

        public static async Task Main(string[] args)
        {
            bool interfaceActive = true;
            while (interfaceActive)
            {
                Console.Clear();
                Console.WriteLine("=====================================================================");
                Console.WriteLine("    ENTERPRISE IN-MEMORY MANAGEMENT SOLUTION - HIGH LEVEL v4.0       ");
                Console.WriteLine("=====================================================================");
                Console.WriteLine("1. RENDER CORE OPERATIONS ROOM MATRIX DIRECTORY");
                Console.WriteLine("2. ALLOCATE NEW TRANSACTION ACCOMMODATION RECORD (CHECK-IN)");
                Console.WriteLine("3. EVALUATE ACCOUNTS & SETTLE OUTSTANDING LEDGER (CHECK-OUT)");
                Console.WriteLine("4. VIEW SYSTEM-WIDE TRANSACTION AUDIT TRACKS");
                Console.WriteLine("5. DISPLAY ANALYTICAL LIVE PERFORMANCE METRICS (LINQ METRICS)");
                Console.WriteLine("6. TERMINATE ENTERPRISE OPERATION");
                Console.WriteLine("=====================================================================");
                Console.Write("EXECUTE DIRECTION INDEX: ");

                switch (Console.ReadLine())
                {
                    case "1": await CommandRenderMatrix(); break;
                    case "2": await CommandCheckIn(); break;
                    case "3": await CommandCheckOut(); break;
                    case "4": await CommandViewAudit(); break;
                    case "5": await CommandViewMetrics(); break;
                    case "6": interfaceActive = false; break;
                }
            }
        }

        private static async Task CommandRenderMatrix()
        {
            Console.Clear();
            Console.WriteLine(string.Format("{0,-8} {1,-15} {2,-12} {3,-12} {4,-15} {5}", "Room", "Classification", "Base Night", "State", "Guest Client", "Check-In Stamp"));
            Console.WriteLine(new string('=', 85));

            var collection = await HotelEngine.GetAllRoomsAsync();
            foreach (var room in collection)
            {
                string stateText = room.IsOccupied ? "Occupied" : "Available";
                string guest = room.IsOccupied ? room.GuestName : "-";
                string dateStr = room.IsOccupied ? room.CheckInTimestamp?.ToString("dd/MM/yyyy") : "-";
                Console.WriteLine(string.Format("{0,-8} {1,-15} £{2,-11:F2} {3,-12} {4,-15} {5}", room.RoomNumber, room.RoomType, room.BasePricePerNight, stateText, guest, dateStr));
            }
            Console.ReadKey();
        }

        private static async Task CommandCheckIn()
        {
            Console.Clear();
            try
            {
                Console.Write("Target Asset Room Identifier: ");
                if (!int.TryParse(Console.ReadLine(), out int roomNum)) throw new FormatException("Room ID parameter input syntax must evaluate to numerical integer strings.");

                Console.Write("Primary Client Legal Identity: ");
                string name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name)) throw new FormatException("Client allocation tags cannot parse blank characters.");

                Console.Write("Arrival Accounting Date (dd/MM/yyyy): ");
                if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime checkInDate))
                    throw new FormatException("Target parameters failed date matching rules (dd/MM/yyyy).");

                await HotelEngine.CheckInAsync(roomNum, name, checkInDate);
                Console.WriteLine($"\nSystem Status: Asset {roomNum} successfully bound to '{name}'.");
            }
            catch (Exception ex) when (ex is HotelDomainException or FormatException)
            {
                Console.WriteLine($"\nTransaction Terminated: {ex.Message}");
            }
            Console.ReadKey();
        }

        private static async Task CommandCheckOut()
        {
            Console.Clear();
            try
            {
                Console.Write("Target Asset Room Identifier: ");
                if (!int.TryParse(Console.ReadLine(), out int roomNum)) throw new FormatException("Room ID parameter input syntax must evaluate to numerical integer strings.");

                Console.Write("Departure Accounting Date (dd/MM/yyyy): ");
                if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime checkOutDate))
                    throw new FormatException("Target parameters failed date matching rules (dd/MM/yyyy).");

                var (bill, nights, guest) = await HotelEngine.CheckOutAsync(roomNum, checkOutDate);

                Console.WriteLine($"\n--- SETTLED ACCOUNT STATEMENT INVOICE ---");
                Console.WriteLine($"Client Account:       {guest}");
                Console.WriteLine($"Calculated Duration:  {nights} Accounting Night(s)");
                Console.WriteLine($"Aggregate Invoice:    £{bill:F2} (Tax/Luxury Additions Compiled Polymorphically)");
            }
            catch (Exception ex) when (ex is HotelDomainException or FormatException)
            {
                Console.WriteLine($"\nTransaction Terminated: {ex.Message}");
            }
            Console.ReadKey();
        }

        private static async Task CommandViewAudit()
        {
            Console.Clear();
            Console.WriteLine("--- CORE APPLICATION TRANSACTION SECURITY LEDGER ---");
            var ledger = await HotelEngine.GetAuditLedgerAsync();
            if (ledger.Count == 0) Console.WriteLine("Zero ledger items recorded within the context heap context session.");
            else
            {
                foreach (var log in ledger)
                {
                    Console.WriteLine($"TX_REF: [{log.TransactionId}] | Room: {log.RoomNumber} ({log.RoomType})");
                    Console.WriteLine($"Client: {log.GuestName} | Duration: {log.TotalNights} Nights ({log.Arrived:dd/MM/yyyy} -> {log.Departed:dd/MM/yyyy})");
                    Console.WriteLine($"Invoice Aggregate: £{log.SettledInvoice:F2}\n");
                }
            }
            Console.ReadKey();
        }

        private static async Task CommandViewMetrics()
        {
            Console.Clear();
            Console.WriteLine("--- REAL-TIME BUSINESS LINQ ANALYTICS METRICS ---");
            var metricsData = await HotelEngine.GeneratePerformanceMetricsAsync();
            if (metricsData == null) Console.WriteLine("Insufficient history data constraints available to compute vector analysis matrix.");
            else
            {
                dynamic dyn = metricsData;
                Console.WriteLine($"Session Total Accumulated Revenue:  £{dyn.TotalGrossRevenue:F2}");
                Console.WriteLine($"Session Arithmetic Mean Duration:  {dyn.AverageStayDuration:F1} Night(s)");
                Console.WriteLine($"Highest Operational Yield Category: {dyn.TopYieldingCategory}");
            }
            Console.ReadKey();
        }
    }
}