public class UpdateVisitStatusDto
{
    public Guid IdVisitRequest { get; set; }
    public string NewStatus { get; set; } = string.Empty;
}
