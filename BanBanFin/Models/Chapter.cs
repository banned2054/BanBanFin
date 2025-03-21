using BanBanFin.Utils.ExtensionMethod;

namespace BanBanFin.Models;

public class Chapter
{
    public string Title { get; set; } = "";
    public double Time  { get; set; }

    private string? _timeDisplay;

    public string TimeDisplay
    {
        get
        {
            if (_timeDisplay != null) return _timeDisplay;
            _timeDisplay = TimeSpan.FromSeconds(Time).ToString();

            if (_timeDisplay.ContainsEx("."))
                _timeDisplay = _timeDisplay[.._timeDisplay.LastIndexOf(".", StringComparison.Ordinal)];

            return _timeDisplay;
        }
    }
}