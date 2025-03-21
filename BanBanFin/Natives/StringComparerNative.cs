using System.Collections;
using System.Runtime.InteropServices;

namespace BanBanFin.Natives;

public class StringComparerNative : IComparer, IComparer<string>
{
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern int StrCmpLogical(string? x, string? y);

    private static int IComparer_Compare(object? x, object? y) => StrCmpLogical(x!.ToString(), y!.ToString());

    private static int IComparerOfString_Compare(string? x, string? y) => StrCmpLogical(x, y);

    int IComparer.        Compare(object? x, object? y) => IComparer_Compare(x, y);
    int IComparer<string>.Compare(string? x, string? y) => IComparerOfString_Compare(x, y);
}