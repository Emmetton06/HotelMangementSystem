using System;

namespace HotelManagementSystem.Domain.Exceptions
{
    public class HotelDomainException : Exception
    {
        public HotelDomainException(string message) : base(message) { }
    }

    public class RoomNotFoundException : HotelDomainException
    {
        public RoomNotFoundException(int roomNumber)
            : base($"Asset Directory Fault: Room assignment index {roomNumber} does not exist.") { }
    }

    public class RoomStateConflictException : HotelDomainException
    {
        public RoomStateConflictException(int roomNumber, string reason)
            : base($"State Collision on Room {roomNumber}: {reason}") { }
    }

    public class InvalidBookingDurationException : HotelDomainException
    {
        public InvalidBookingDurationException(string reason)
            : base($"Temporal Constraint Violation: {reason}") { }
    }
}