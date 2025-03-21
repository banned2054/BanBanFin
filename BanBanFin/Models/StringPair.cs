namespace BanBanFin.Models;

internal class StringPair
{
    public string Name  { get; set; }
    public string Value { get; set; }

    public StringPair(string name, string value)
    {
        Name  = name;
        Value = value;
    }
}