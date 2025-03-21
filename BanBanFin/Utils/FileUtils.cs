using System.IO;

namespace BanBanFin.Utils;

internal class FileUtils
{
    public static void Delete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            // Terminal.WriteError("Failed to delete file:" + Br + path + Br + ex.Message);
        }
    }

    public static string ReadTextFile(string path) => File.Exists(path) ? File.ReadAllText(path) : "";
}