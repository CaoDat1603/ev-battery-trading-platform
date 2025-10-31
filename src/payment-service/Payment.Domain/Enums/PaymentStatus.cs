﻿namespace Payment.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 0,
        Success = 1,
        Failed = 2,
        Refunded = 3,
        Cancelled = 4
    }
}
