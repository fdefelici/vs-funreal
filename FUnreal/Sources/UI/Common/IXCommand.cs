namespace FUnreal
{
    public interface IXActionCmd
    {
        int ID { get; }
        bool Enabled { get; set; }
        bool Visible { get; set; }
        AXActionController Controller { get;  set; }
        string Label { get; set; }
    }
}