using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.Domain.Enums
{
    public class OrderStatus
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";

        public static string Initial => Pending;

        private static readonly HashSet<string> ValidStatuses = new HashSet<string>
        {
            Pending,
            Processing,
            Shipped,
            Delivered,
            Cancelled
        };

        public static bool IsValidStatus(string status)
        {
            return !string.IsNullOrWhiteSpace(status) && ValidStatuses.Contains(status);
        }

        public static IReadOnlyCollection<string> GetAll()
        {
            return ValidStatuses.ToList().AsReadOnly();
        }
    }
}