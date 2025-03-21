namespace BanBanFin.Utils.ExtensionMethod;

public static class ObjectExtension
{
    public static string ToStringEx(this object instance) => instance?.ToString() ?? "";
}