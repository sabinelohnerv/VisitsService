namespace VisitService.API.Dtos
{
    public class UpdateStatusDto
    {
        public string Status { get; set; } = "pendiente"; // nueva opción: aceptada, rechazada, cancelada
    }
}
