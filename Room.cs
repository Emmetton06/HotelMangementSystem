using System;

namespace HotelManagementSystem.Domain.Models
{
    public abstract class Room
    {
        public int RoomNumber { get; init; }
        public string RoomType { get; init; }
        public double BasePricePerNight { get; init; }
        public bool IsOccupied { get; private set; }
        public string GuestName { get; private set; } = string.Empty;
        public DateTime? CheckInTimestamp { get; private set; }

        protected Room(int roomNumber, string roomType, double basePricePerNight)
        {
            RoomNumber = roomNumber;
            RoomType = roomType;
            BasePricePerNight = basePricePerNight;
        }

        public abstract double CalculateTotalBill(int nightsStayed);

        public void AssignGuest(string guestName, DateTime checkInDate)
        {
            IsOccupied = true;
            GuestName = guestName;
            CheckInTimestamp = checkInDate;
        }

        public void EvictAndClear()
        {
            IsOccupied = false;
            GuestName = string.Empty;
            CheckInTimestamp = null;
        }
    }

    public class StandardRoom(int roomNumber, double basePrice)
        : Room(roomNumber, "Standard", basePrice)
    {
        public override double CalculateTotalBill(int nightsStayed) => BasePricePerNight * nightsStayed;
    }

    public class DeluxeRoom(int roomNumber, double basePrice)
        : Room(roomNumber, "Deluxe", basePrice)
    {
        public override double CalculateTotalBill(int nightsStayed)
        {
            double baseCost = BasePricePerNight * nightsStayed;
            return baseCost + (baseCost * 0.12); // Dynamic 12% Local Tourism Tax Addition
        }
    }

    public class SuiteRoom(int roomNumber, double basePrice)
        : Room(roomNumber, "Luxury Suite", basePrice)
    {
        private const double LuxuryServiceFee = 45.00;
        public override double CalculateTotalBill(int nightsStayed) => (BasePricePerNight * nightsStayed) + LuxuryServiceFee;
    }
}
