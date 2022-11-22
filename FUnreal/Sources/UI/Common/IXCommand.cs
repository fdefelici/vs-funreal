namespace FUnreal
{
    public interface IXActionCmd
    {
        int ID { get; }
        bool Enabled { get; set; }
        IXActionController Controller { get;  set; }
        string Label { get; set; }
    }
}