using BanBanFin.Models.Enums.Mpv;

namespace BanBanFin.Natives;

using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable IDE1006 // type name starts with underscore
#pragma warning disable CA1401  // P/Invokes should not be visible
#pragma warning disable CA2101  // Specify marshaling for P/Invoke string arguments
public static class MpvNative
{
    private const string DllPath = "libmpv-2.dll";

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_create")]
    public static extern nint Create();

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_create_client")]
    public static extern nint CreateClient(nint mpvHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string command);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_initialize")]
    public static extern MpvError Initialize(nint mpvHandle);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_destroy")]
    public static extern void Destroy(nint mpvHandle);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_command")]
    public static extern MpvError Command(nint mpvHandle, nint strings);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_command_string")]
    public static extern MpvError CommandString(nint mpvHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string command);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_command_ret")]
    public static extern MpvError CommandRet(nint mpvHandle, nint strings, nint node);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_free_node_contents")]
    public static extern void FreeNodeContents(nint node);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_error_string")]
    public static extern nint ErrorString(MpvError error);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_request_log_messages")]
    public static extern MpvError RequestLogMessages(nint                                        mpvHandle,
                                                     [MarshalAs(UnmanagedType.LPUTF8Str)] string minLevel);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_set_option")]
    public static extern int SetOption(nint mpvHandle, byte[] name, MpvFormat format, ref long data);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_set_option_string")]
    public static extern int SetOptionString(nint mpvHandle, byte[] name, byte[] value);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_get_property")]
    public static extern MpvError GetProperty(nint mpvHandle, byte[] name, MpvFormat format, out nint data);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_get_property")]
    public static extern MpvError GetProperty(nint mpvHandle, byte[] name, MpvFormat format, out double data);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_set_property")]
    public static extern MpvError SetProperty(nint mpvHandle, byte[] name, MpvFormat format, ref byte[] data);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_set_property")]
    public static extern MpvError SetProperty(nint mpvHandle, byte[] name, MpvFormat format, ref long data);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_set_property")]
    public static extern MpvError SetProperty(nint mpvHandle, byte[] name, MpvFormat format, ref double data);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_observe_property")]
    public static extern MpvError ObserveProperty(nint mpvHandle, ulong replyUserData,
                                                  [MarshalAs(UnmanagedType.LPUTF8Str)] string name, MpvFormat format);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_unobserve_property")]
    public static extern int UnobserveProperty(nint mpvHandle, ulong registeredReplyUserData);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_free")]
    public static extern void Free(nint data);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_wait_event")]
    public static extern nint WaitEvent(nint mpvHandle, double timeout);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, EntryPoint = "mpv_request_event")]
    public static extern MpvError RequestEvent(nint mpvHandle, MpvEventId id, int enable);

    [StructLayout(LayoutKind.Sequential)]
    public struct MpvEventLogMessage
    {
        public nint        prefix;
        public nint        level;
        public nint        text;
        public MpvLogLevel logLevel;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MpvEvent
    {
        public MpvEventId eventId;
        public int        error;
        public ulong      replyUserData;
        public nint       data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MpvEventClientMessage
    {
        public int  numArgs;
        public nint args;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MpvEventProperty
    {
        public string    name;
        public MpvFormat format;
        public nint      data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MpvEventEndFile
    {
        public int reason;
        public int error;
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct MpvNode
    {
        [FieldOffset(0)]
        public nint str;

        [FieldOffset(0)]
        public int flag;

        [FieldOffset(0)]
        public long int64;

        [FieldOffset(0)]
        public double dbl;

        [FieldOffset(0)]
        public nint list;

        [FieldOffset(0)]
        public nint ba;

        [FieldOffset(8)]
        public MpvFormat format;
    }

    public static string[] ConvertFromUtf8Strings(nint utf8StringArray, int stringCount)
    {
        var intPtrArray = new nint[stringCount];
        var stringArray = new string[stringCount];
        Marshal.Copy(utf8StringArray, intPtrArray, 0, stringCount);

        for (var i = 0; i < stringCount; i++)
            stringArray[i] = ConvertFromUtf8(intPtrArray[i]);

        return stringArray;
    }

    public static string ConvertFromUtf8(nint nativeUtf8)
    {
        var len = 0;

        while (Marshal.ReadByte(nativeUtf8, len) != 0)
            ++len;

        var buffer = new byte[len];
        Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer);
    }

    public static string GetError(MpvError err) => ConvertFromUtf8(ErrorString(err));

    public static byte[] GetUtf8Bytes(string s) => Encoding.UTF8.GetBytes(s + "\0");
}