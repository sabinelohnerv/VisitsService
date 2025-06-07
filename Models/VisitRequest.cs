using System;

namespace VisitService.API.Models
{
    public class VisitRequest
    {
        public Guid IdVisitRequest { get; set; }
        public Guid IdProperty { get; set; }
        public Guid IdInterestedUser { get; set; }
        public Guid IdOwnerUser { get; set; }
        public DateTime RequestedDateTime { get; set; }

        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string Status { get; set; } = "pendiente";  // 'pendiente','aceptada','rechazada','cancelada'

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
