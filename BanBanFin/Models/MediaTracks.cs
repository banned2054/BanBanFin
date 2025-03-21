namespace BanBanFin.Models;

public class MediaTracks
{
    public int    Id       { get; set; }
    public bool   External { get; set; }
    public string Text     { get; set; } = string.Empty;
    public string Type     { get; set; } = string.Empty;
}