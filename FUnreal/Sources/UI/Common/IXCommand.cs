namespace FUnreal
{
    public interface IXActionCmd
    {
        int ID { get; }
        bool Enabled { get; set; }
    }
}