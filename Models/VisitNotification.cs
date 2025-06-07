using System;

namespace VisitService.API.Models
{
    public class VisitNotification
    {
        public Guid IdNotification { get; set; }
        public Guid IdVisitRequest { get; set; }

        public string NotificationType { get; set; } = string.Empty; // 'solicitud','aceptada','rechazada','recordatorio'
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string Channel { get; set; } = "email"; // 'email','push'
        public string Status { get; set; } = "enviado"; // 'enviado','fallido'
    }
}
