using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaBooking.Domain.Enums
{
    public enum BookingStatus
    {
        Pending = 1,
        Paid = 2,
        Cancelled = 3,
        Expired = 4,
        Failed = 5,
        Refunded = 6,
        RefundCompleted = 7
    }
}
