using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VisitService.API.DTOs
{
    public class CreateVisitRequestDto
    {
        public Guid IdProperty { get; set; }
        public Guid IdOwnerUser { get; set; }

        public DateTime RequestedDateTime { get; set; }

        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;

        [JsonIgnore]
        public Guid IdInterestedUser { get; set; }
    }
}
