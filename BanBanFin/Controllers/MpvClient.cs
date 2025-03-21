using BanBanFin.Models.Enums.Mpv;
using BanBanFin.Natives;
using System.Runtime.InteropServices;

namespace BanBanFin.Controllers;

public class MpvClient
{
    public event Action<string[]>?            ClientMessage;    // client-message      MPV_EVENT_CLIENT_MESSAGE
    public event Action<MpvLogLevel, string>? LogMessage;       // log-message         MPV_EVENT_LOG_MESSAGE
    public event Action<MpvEndFileReason>?    EndFile;          // end-file            MPV_EVENT_END_FILE
    public event Action?                      Shutdown;         // shutdown            MPV_EVENT_SHUTDOWN
    public event Action?                      GetPropertyReply; // get-property-reply  MPV_EVENT_GET_PROPERTY_REPLY
    public event Action?                      SetPropertyReply; // set-property-reply  MPV_EVENT_SET_PROPERTY_REPLY
    public event Action?                      CommandReply;     // command-reply       MPV_EVENT_COMMAND_REPLY
    public event Action?                      StartFile;        // start-file          MPV_EVENT_START_FILE
    public event Action?                      FileLoaded;       // file-loaded         MPV_EVENT_FILE_LOADED
    public event Action?                      VideoReconfig;    // video-reconfig      MPV_EVENT_VIDEO_RECONFIG
    public event Action?                      AudioReconfig;    // audio-reconfig      MPV_EVENT_AUDIO_RECONFIG
    public event Action?                      Seek;             // seek                MPV_EVENT_SEEK
    public event Action?                      PlaybackRestart;  // playback-restart    MPV_EVENT_PLAYBACK_RESTART

    public Dictionary<string, List<Action>> PropChangeActions { get; set; } = new();

    public Dictionary<string, List<Action<int>>> IntPropChangeActions { get; set; } = new();

    public Dictionary<string, List<Action<bool>>> BoolPropChangeActions { get; set; } = new();

    public Dictionary<string, List<Action<double>>> DoublePropChangeActions { get; set; } = new();

    public Dictionary<string, List<Action<string>>> StringPropChangeActions { get; set; } = new();

    public nint Handle { get; set; }

    public void EventLoop()
    {
        while (true)
        {
            var ptr = MpvNative.WaitEvent(Handle, -1);
            var evt = (MpvNative.MpvEvent)Marshal.PtrToStructure(ptr, typeof(MpvNative.MpvEvent))!;

            try
            {
                switch (evt.eventId)
                {
                    case MpvEventId.Shutdown :
                        OnShutdown();
                        return;
                    case MpvEventId.LogMessage :
                    {
                        var data =
                            (MpvNative.MpvEventLogMessage)
                            Marshal.PtrToStructure(evt.data, typeof(MpvNative.MpvEventLogMessage))!;
                        OnLogMessage(data);
                    }
                        break;
                    case MpvEventId.ClientMessage :
                    {
                        var data =
                            (MpvNative.MpvEventClientMessage)
                            Marshal.PtrToStructure(evt.data, typeof(MpvNative.MpvEventClientMessage))!;
                        OnClientMessage(data);
                    }
                        break;
                    case MpvEventId.VideoReconfig :
                        OnVideoReconfig();
                        break;
                    case MpvEventId.EndFile :
                    {
                        var data =
                            (MpvNative.MpvEventEndFile)
                            Marshal.PtrToStructure(evt.data, typeof(MpvNative.MpvEventEndFile))!;
                        OnEndFile(data);
                    }
                        break;
                    case MpvEventId.FileLoaded : // triggered after MPV_EVENT_START_FILE
                        OnFileLoaded();
                        break;
                    case MpvEventId.PropertyChange :
                    {
                        var data =
                            (MpvNative.MpvEventProperty)
                            Marshal.PtrToStructure(evt.data, typeof(MpvNative.MpvEventProperty))!;
                        OnPropertyChange(data);
                    }
                        break;
                    case MpvEventId.GetPropertyReply :
                        OnGetPropertyReply();
                        break;
                    case MpvEventId.SetPropertyReply :
                        OnSetPropertyReply();
                        break;
                    case MpvEventId.CommandReply :
                        OnCommandReply();
                        break;
                    case MpvEventId.StartFile : // triggered before MPV_EVENT_FILE_LOADED
                        OnStartFile();
                        break;
                    case MpvEventId.AudioReconfig :
                        OnAudioReconfig();
                        break;
                    case MpvEventId.Seek :
                        OnSeek();
                        break;
                    case MpvEventId.PlaybackRestart :
                        OnPlaybackRestart();
                        break;
                    case MpvEventId.None :
                        break;
                    case MpvEventId.ScriptInputDispatch :
                        break;
                    case MpvEventId.QueueOverflow :
                        break;
                    case MpvEventId.Hook :
                        break;
                    default :
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    protected virtual void OnClientMessage(MpvNative.MpvEventClientMessage data) =>
        ClientMessage?.Invoke(MpvNative.ConvertFromUtf8Strings(data.args, data.numArgs));

    protected virtual void OnLogMessage(MpvNative.MpvEventLogMessage data)
    {
        if (LogMessage == null) return;
        var msg = $"[{MpvNative.ConvertFromUtf8(data.prefix)}] {MpvNative.ConvertFromUtf8(data.text)}";
        LogMessage.Invoke(data.logLevel, msg);
    }

    protected virtual void OnPropertyChange(MpvNative.MpvEventProperty data)
    {
        switch (data.format)
        {
            case MpvFormat.Flag :
            {
                lock (BoolPropChangeActions)
                    foreach (var pair in BoolPropChangeActions)
                        if (pair.Key == data.name)
                        {
                            var value = Marshal.PtrToStructure<int>(data.data) == 1;

                            foreach (var action in pair.Value)
                                action.Invoke(value);
                        }

                break;
            }
            case MpvFormat.String :
            {
                lock (StringPropChangeActions)
                    foreach (var pair in StringPropChangeActions)
                        if (pair.Key == data.name)
                        {
                            var value = MpvNative.ConvertFromUtf8(Marshal.PtrToStructure<IntPtr>(data.data));

                            foreach (var action in pair.Value)
                                action.Invoke(value);
                        }

                break;
            }
            case MpvFormat.Int64 :
            {
                lock (IntPropChangeActions)
                    foreach (var pair in IntPropChangeActions)
                        if (pair.Key == data.name)
                        {
                            var value = Marshal.PtrToStructure<int>(data.data);

                            foreach (var action in pair.Value)
                                action.Invoke(value);
                        }

                break;
            }
            case MpvFormat.None :
            {
                lock (PropChangeActions)
                    foreach (var action in PropChangeActions.Where(pair => pair.Key == data.name)
                                                            .SelectMany(pair => pair.Value))
                        action.Invoke();
                break;
            }
            case MpvFormat.Double :
            {
                lock (DoublePropChangeActions)
                    foreach (var pair in DoublePropChangeActions)
                        if (pair.Key == data.name)
                        {
                            var value = Marshal.PtrToStructure<double>(data.data);

                            foreach (var action in pair.Value)
                                action.Invoke(value);
                        }

                break;
            }
            case MpvFormat.OsdString :
                break;
            case MpvFormat.Node :
                break;
            case MpvFormat.NodeArray :
                break;
            case MpvFormat.NodeMap :
                break;
            case MpvFormat.ByteArray :
                break;
            default :
                throw new ArgumentOutOfRangeException();
        }
    }

    protected virtual void OnEndFile(MpvNative.MpvEventEndFile data) =>
        EndFile?.Invoke((MpvEndFileReason)data.reason);

    protected virtual void OnFileLoaded()       => FileLoaded?.Invoke();
    protected virtual void OnShutdown()         => Shutdown?.Invoke();
    protected virtual void OnGetPropertyReply() => GetPropertyReply?.Invoke();
    protected virtual void OnSetPropertyReply() => SetPropertyReply?.Invoke();
    protected virtual void OnCommandReply()     => CommandReply?.Invoke();
    protected virtual void OnStartFile()        => StartFile?.Invoke();
    protected virtual void OnVideoReconfig()    => VideoReconfig?.Invoke();
    protected virtual void OnAudioReconfig()    => AudioReconfig?.Invoke();
    protected virtual void OnSeek()             => Seek?.Invoke();
    protected virtual void OnPlaybackRestart()  => PlaybackRestart?.Invoke();

    public void Command(string command)
    {
        var err = MpvNative.CommandString(Handle, command);

        if (err < 0)
            HandleError(err, "error executing command: " + command);
    }

    public void CommandV(params string[] args)
    {
        var count    = args.Length + 1;
        var pointers = new IntPtr[count];
        var rootPtr  = Marshal.AllocHGlobal(IntPtr.Size * count);

        for (var index = 0; index < args.Length; index++)
        {
            var bytes = MpvNative.GetUtf8Bytes(args[index]);
            var ptr   = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            pointers[index] = ptr;
        }

        Marshal.Copy(pointers, 0, rootPtr, count);
        var err = MpvNative.Command(Handle, rootPtr);

        foreach (var ptr in pointers)
            Marshal.FreeHGlobal(ptr);

        Marshal.FreeHGlobal(rootPtr);

        if (err < 0)
            HandleError(err, "error executing command: " + string.Join("\n", args));
    }

    public string Expand(string? value)
    {
        if (value == null)
            return "";

        if (!value.Contains("${"))
            return value;

        string[] args     = { "expand-text", value };
        var      count    = args.Length + 1;
        var      pointers = new IntPtr[count];
        var      rootPtr  = Marshal.AllocHGlobal(IntPtr.Size * count);

        for (var index = 0; index < args.Length; index++)
        {
            var bytes = MpvNative.GetUtf8Bytes(args[index]);
            var ptr   = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            pointers[index] = ptr;
        }

        Marshal.Copy(pointers, 0, rootPtr, count);
        var resultNodePtr = Marshal.AllocHGlobal(16);
        var err           = MpvNative.CommandRet(Handle, rootPtr, resultNodePtr);

        foreach (var ptr in pointers)
            Marshal.FreeHGlobal(ptr);

        Marshal.FreeHGlobal(rootPtr);

        if (err < 0)
        {
            HandleError(err, "error executing command: " + string.Join("\n", args));
            Marshal.FreeHGlobal(resultNodePtr);
            return "property expansion error";
        }

        var resultNode = Marshal.PtrToStructure<MpvNative.MpvNode>(resultNodePtr);
        var ret        = MpvNative.ConvertFromUtf8(resultNode.str);
        MpvNative.FreeNodeContents(resultNodePtr);
        Marshal.FreeHGlobal(resultNodePtr);
        return ret;
    }

    public bool GetPropertyBool(string name)
    {
        var err = MpvNative.GetProperty(Handle, MpvNative.GetUtf8Bytes(name), MpvFormat.Flag, out IntPtr lpBuffer);

        if (err < 0)
            HandleError(err, "error getting property: " + name);

        return lpBuffer.ToInt32() != 0;
    }

    public void SetPropertyBool(string name, bool value)
    {
        long val = value ? 1 : 0;
        var err =
            MpvNative.SetProperty(Handle, MpvNative.GetUtf8Bytes(name), MpvFormat.Flag,
                                  ref val);

        if (err < 0)
            HandleError(err, $"error setting property: {name} = {value}");
    }

    public int GetPropertyInt(string name)
    {
        MpvNative.GetProperty(Handle, MpvNative.GetUtf8Bytes(name), MpvFormat.Int64, out IntPtr lpBuffer);


        return lpBuffer.ToInt32();
    }

    public void SetPropertyInt(string name, int value)
    {
        long val = value;
        var err =
            MpvNative.SetProperty(Handle, MpvNative.GetUtf8Bytes(name), MpvFormat.Int64,
                                  ref val);

        if (err < 0)
            HandleError(err, $"error setting property: {name} = {value}");
    }

    public void SetPropertyLong(string name, long value)
    {
        var err =
            MpvNative.SetProperty(Handle, MpvNative.GetUtf8Bytes(name), MpvFormat.Int64,
                                  ref value);

        if (err < 0)
            HandleError(err, $"error setting property: {name} = {value}");
    }

    public long GetPropertyLong(string name)
    {
        var err = MpvNative.GetProperty(Handle, MpvNative.GetUtf8Bytes(name),
                                        MpvFormat.Int64, out IntPtr lpBuffer);

        if (err < 0)
            HandleError(err, "error getting property: " + name);

        return lpBuffer.ToInt64();
    }

    public double GetPropertyDouble(string name, bool handleError = true)
    {
        MpvNative.GetProperty(Handle, MpvNative.GetUtf8Bytes(name),
                              MpvFormat.Double, out double value);
        return value;
    }

    public void SetPropertyDouble(string name, double value)
    {
        var val = value;
        var err =
            MpvNative.SetProperty(Handle, MpvNative.GetUtf8Bytes(name), MpvFormat.Double,
                                  ref val);

        if (err < 0)
            HandleError(err, $"error setting property: {name} = {value}");
    }

    public string GetPropertyString(string name)
    {
        var err = MpvNative.GetProperty(Handle, MpvNative.GetUtf8Bytes(name),
                                        MpvFormat.String, out IntPtr lpBuffer);

        if (err != 0) return "";
        var ret = MpvNative.ConvertFromUtf8(lpBuffer);
        MpvNative.Free(lpBuffer);
        return ret;
    }

    public void SetPropertyString(string name, string value)
    {
        var bytes = MpvNative.GetUtf8Bytes(value);
        var err =
            MpvNative.SetProperty(Handle, MpvNative.GetUtf8Bytes(name), MpvFormat.String,
                                  ref bytes);

        if (err < 0)
            HandleError(err, $"error setting property: {name} = {value}");
    }

    public string GetPropertyOsdString(string name)
    {
        var err = MpvNative.GetProperty(Handle, MpvNative.GetUtf8Bytes(name),
                                        MpvFormat.OsdString, out IntPtr lpBuffer);

        switch (err)
        {
            case 0 :
            {
                var ret = MpvNative.ConvertFromUtf8(lpBuffer);
                MpvNative.Free(lpBuffer);
                return ret;
            }
            case < 0 :
                HandleError(err, "error getting property: " + name);
                break;
        }

        return "";
    }

    public void ObservePropertyInt(string name, Action<int> action)
    {
        lock (IntPropChangeActions)
        {
            if (!IntPropChangeActions.ContainsKey(name))
            {
                var err = MpvNative.ObserveProperty(Handle, 0, name, MpvFormat.Int64);

                if (err < 0)
                    HandleError(err, "error observing property: " + name);
                else
                    IntPropChangeActions[name] = new List<Action<int>>();
            }

            if (IntPropChangeActions.ContainsKey(name))
                IntPropChangeActions[name].Add(action);
        }
    }

    public void ObservePropertyDouble(string name, Action<double> action)
    {
        lock (DoublePropChangeActions)
        {
            if (!DoublePropChangeActions.ContainsKey(name))
            {
                var err =
                    MpvNative.ObserveProperty(Handle, 0, name, MpvFormat.Double);

                if (err < 0)
                    HandleError(err, "error observing property: " + name);
                else
                    DoublePropChangeActions[name] = new List<Action<double>>();
            }

            if (DoublePropChangeActions.ContainsKey(name))
                DoublePropChangeActions[name].Add(action);
        }
    }

    public void ObservePropertyBool(string name, Action<bool> action)
    {
        lock (BoolPropChangeActions)
        {
            if (!BoolPropChangeActions.ContainsKey(name))
            {
                var err = MpvNative.ObserveProperty(Handle, 0, name, MpvFormat.Flag);

                if (err < 0)
                    HandleError(err, "error observing property: " + name);
                else
                    BoolPropChangeActions[name] = new List<Action<bool>>();
            }

            if (BoolPropChangeActions.ContainsKey(name))
                BoolPropChangeActions[name].Add(action);
        }
    }

    public void ObservePropertyString(string name, Action<string> action)
    {
        lock (StringPropChangeActions)
        {
            if (!StringPropChangeActions.ContainsKey(name))
            {
                var err =
                    MpvNative.ObserveProperty(Handle, 0, name, MpvFormat.String);

                if (err < 0)
                    HandleError(err, "error observing property: " + name);
                else
                    StringPropChangeActions[name] = new List<Action<string>>();
            }

            if (StringPropChangeActions.ContainsKey(name))
                StringPropChangeActions[name].Add(action);
        }
    }

    public void ObserveProperty(string name, Action action)
    {
        lock (PropChangeActions)
        {
            if (!PropChangeActions.ContainsKey(name))
            {
                var err = MpvNative.ObserveProperty(Handle, 0, name, MpvFormat.None);

                if (err < 0)
                    HandleError(err, "error observing property: " + name);
                else
                    PropChangeActions[name] = new List<Action>();
            }

            if (PropChangeActions.ContainsKey(name))
                PropChangeActions[name].Add(action);
        }
    }

    private static void HandleError(MpvError err, string msg)
    {
    }
}