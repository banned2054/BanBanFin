using BanBanFin.Models.Enums.MediaInfo;
using System.Runtime.InteropServices;

namespace BanBanFin.Natives;

public class MediaInfoNative : IDisposable
{
    private readonly IntPtr _handle;

    public MediaInfoNative(string file)
    {
        if ((_handle = MediaInfo_New()) == IntPtr.Zero)
            throw new Exception("Failed to call MediaInfo_New");

        if (MediaInfo_Open(_handle, file) == 0)
            throw new Exception("Error MediaInfo_Open");
    }

    public string GetInfo(MediaInfoStreamKind kind, string parameter)
    {
        return Marshal.PtrToStringUni(MediaInfo_Get(_handle, kind, 0,
                                                    parameter, MediaInfoKind.Text, MediaInfoKind.Name)) ?? "";
    }

    public int GetCount(MediaInfoStreamKind kind) => MediaInfo_Count_Get(_handle, kind, -1);

    public string GetGeneral(string parameter)
    {
        return Marshal.PtrToStringUni(MediaInfo_Get(_handle, MediaInfoStreamKind.General,
                                                    0, parameter, MediaInfoKind.Text, MediaInfoKind.Name)) ?? "";
    }

    public string GetVideo(int stream, string parameter)
    {
        return Marshal.PtrToStringUni(MediaInfo_Get(_handle, MediaInfoStreamKind.Video,
                                                    stream, parameter, MediaInfoKind.Text, MediaInfoKind.Name)) ?? "";
    }

    public string GetAudio(int stream, string parameter)
    {
        return Marshal.PtrToStringUni(MediaInfo_Get(_handle, MediaInfoStreamKind.Audio,
                                                    stream, parameter, MediaInfoKind.Text, MediaInfoKind.Name)) ?? "";
    }

    public string GetText(int stream, string parameter)
    {
        return Marshal.PtrToStringUni(MediaInfo_Get(_handle, MediaInfoStreamKind.Text,
                                                    stream, parameter, MediaInfoKind.Text, MediaInfoKind.Name)) ?? "";
    }

    public string GetSummary(bool complete, bool rawView)
    {
        MediaInfo_Option(_handle, "Language", rawView ? "raw" : "");
        MediaInfo_Option(_handle, "Complete", complete ? "1" : "0");
        return Marshal.PtrToStringUni(MediaInfo_Inform(_handle, 0)) ?? "";
    }

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        if (_handle != IntPtr.Zero)
        {
            MediaInfo_Close(_handle);
            MediaInfo_Delete(_handle);
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~MediaInfoNative()
    {
        Dispose();
    }

    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_New();

    [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode)]
    private static extern int MediaInfo_Open(IntPtr handle, string path);

    [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr MediaInfo_Option(IntPtr handle, string option, string value);

    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Inform(IntPtr handle, int reserved);

    [DllImport("MediaInfo.dll")]
    private static extern int MediaInfo_Close(IntPtr handle);

    [DllImport("MediaInfo.dll")]
    private static extern void MediaInfo_Delete(IntPtr handle);

    [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr MediaInfo_Get(IntPtr        handle, MediaInfoStreamKind kind,
                                               int           stream, string parameter, MediaInfoKind infoKind,
                                               MediaInfoKind searchKind);

    [DllImport("MediaInfo.dll", CharSet = CharSet.Unicode)]
    private static extern int MediaInfo_Count_Get(IntPtr handle, MediaInfoStreamKind streamKind, int stream);
}