namespace CinemaBooking.Domain.Entities
{
    public class SystemSetting
    {
        public int Id { get; set; }
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public string? Label { get; set; }
    }
}
