using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagementSystem.Domain.Exceptions;
using HotelManagementSystem.Domain.Models;

namespace HotelManagementSystem.Core
{
    // High performance read-only data record object
    public record BookingAuditLog(
        Guid TransactionId,
        int RoomNumber,
        string RoomType,
        string GuestName,
        DateTime Arrived,
        DateTime Departed,
        int TotalNights,
        double SettledInvoice
    );

    public interface IHotelManager
    {
        Task<IEnumerable<Room>> GetAllRoomsAsync();
        Task<Room> GetRoomAsync(int roomNumber);
        Task CheckInAsync(int roomNumber, string guestName, DateTime checkInDate);
        Task<(double FinalBill, int NightsCalculated, string Guest)> CheckOutAsync(int roomNumber, DateTime checkOutDate);
        Task<IReadOnlyCollection<BookingAuditLog>> GetAuditLedgerAsync();
        Task<object?> GeneratePerformanceMetricsAsync();
    }

    public class HotelManager : IHotelManager
    {
        private readonly ConcurrentDictionary<int, Room> _inventoryStore = new();
        private readonly List<BookingAuditLog> _auditLedger = new();
        private readonly object _ledgerLock = new();

        public HotelManager() => SeedInventory();

        private void SeedInventory()
        {
            // Fixed parameter mapping to match explicit primary constructor patterns
            _inventoryStore.TryAdd(101, new StandardRoom(101, 50.0));
            _inventoryStore.TryAdd(102, new StandardRoom(102, 50.0));
            _inventoryStore.TryAdd(201, new DeluxeRoom(201, 90.0));
            _inventoryStore.TryAdd(202, new DeluxeRoom(202, 90.0));
            _inventoryStore.TryAdd(301, new SuiteRoom(301, 175.0));
        }

        public Task<IEnumerable<Room>> GetAllRoomsAsync() => Task.FromResult<IEnumerable<Room>>(_inventoryStore.Values);

        public Task<Room> GetRoomAsync(int roomNumber)
        {
            if (!_inventoryStore.TryGetValue(roomNumber, out var room))
                throw new RoomNotFoundException(roomNumber);
            return Task.FromResult(room);
        }

        public async Task CheckInAsync(int roomNumber, string guestName, DateTime checkInDate)
        {
            var room = await GetRoomAsync(roomNumber);

            lock (room)
            {
                if (room.IsOccupied)
                    throw new RoomStateConflictException(roomNumber, "Asset is flagged as actively occupied by an existing guest.");

                room.AssignGuest(guestName, checkInDate);
            }
        }

        public async Task<(double FinalBill, int NightsCalculated, string Guest)> CheckOutAsync(int roomNumber, DateTime checkOutDate)
        {
            var room = await GetRoomAsync(roomNumber);

            double finalBill;
            int totalNights;
            string explicitGuest;

            lock (room)
            {
                if (!room.IsOccupied || !room.CheckInTimestamp.HasValue)
                    throw new RoomStateConflictException(roomNumber, "Asset registration lookup failure. Room is currently vacant.");

                if (checkOutDate.Date < room.CheckInTimestamp.Value.Date)
                    throw new InvalidBookingDurationException($"Departure timeline ({checkOutDate:dd/MM/yyyy}) cannot precede check-in point ({room.CheckInTimestamp.Value:dd/MM/yyyy}).");

                TimeSpan executionSpan = checkOutDate.Date - room.CheckInTimestamp.Value.Date;
                totalNights = executionSpan.Days == 0 ? 1 : executionSpan.Days;

                finalBill = room.CalculateTotalBill(totalNights);
                explicitGuest = room.GuestName;

                lock (_ledgerLock)
                {
                    _auditLedger.Add(new BookingAuditLog(
                        Guid.NewGuid(),
                        room.RoomNumber,
                        room.RoomType,
                        explicitGuest,
                        room.CheckInTimestamp.Value,
                        checkOutDate,
                        totalNights,
                        finalBill
                    ));
                }

                room.EvictAndClear();
            }

            return (finalBill, totalNights, explicitGuest);
        }

        public Task<IReadOnlyCollection<BookingAuditLog>> GetAuditLedgerAsync()
        {
            lock (_ledgerLock) return Task.FromResult<IReadOnlyCollection<BookingAuditLog>>(_auditLedger.AsReadOnly());
        }

        // Fixed compiler notice: converted method to leverage modern non-blocking synchronous returns cleanly
        public async Task<object?> GeneratePerformanceMetricsAsync()
        {
            var logContext = await GetAuditLedgerAsync();
            if (!logContext.Any()) return null;

            lock (_ledgerLock)
            {
                return new
                {
                    TotalGrossRevenue = logContext.Sum(l => l.SettledInvoice),
                    AverageStayDuration = logContext.Average(l => l.TotalNights),
                    TopYieldingCategory = logContext.GroupBy(l => l.RoomType)
                                                    .Select(g => new { Type = g.Key, Revenue = g.Sum(x => x.SettledInvoice) })
                                                    .OrderByDescending(res => res.Revenue)
                                                    .FirstOrDefault()?.Type ?? "None"
                };
            }
        }
    }
}