// <copyright file=VolPluginInterface company=Volograms>
// Copyright (c) 2022 All Rights Reserved
// </copyright>
// <author>Patrick Geoghegan</author>
// <date>18/02/22</date>
// <summary>Interface bridging C# scripts and native C code</summary>

using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

public class VolPluginInterface
{
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    private const string DLL = "volplayer";
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private const string DLL = "vol_unity_lib_win";
#elif UNITY_IOS && !UNITY_EDITOR
    private const string DLL = "__Internal";
#elif UNITY_ANDROID && !UNITY_EDITOR
    private const string DLL = "volplayer";
#else
    private const string DLL = "UnityPlugin";
#endif

    public static VolEnums.LoggingLevel interfaceLoggingLevel = VolEnums.LoggingLevel.Info;
    public static VolEnums.LoggingLevel avLoggingLevel = VolEnums.LoggingLevel.Info;
    public static VolEnums.LoggingLevel geomLoggingLevel = VolEnums.LoggingLevel.Info;
    
    [StructLayout(LayoutKind.Sequential)]
    public struct VolGeometryData
    {
        public IntPtr blockDataPtr;
        public ulong blockDataSize;
        public ulong verticesOffset;
        public int verticesSize;
        public ulong normalOffset;
        public int normalSize;
        public ulong indicesOffset;
        public int indicesSize;
        public ulong uvOffset;
        public int uvSize;
        public ulong textureOffset;
        public int textureSize;

        public override string ToString()
        {
            return $"Bytes size: {blockDataSize}\n" +
                   $"Vertices: {verticesOffset} -> {verticesSize}\n" +
                   $"Normals: {normalOffset} -> {normalSize}\n" +
                   $"Indices: {indicesOffset} -> {indicesSize}\n" +
                   $"UVs: {uvOffset} -> {uvSize}\n" +
                   $"Texture: {textureOffset} -> {textureSize}\n";
        }
    }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
#else
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
#endif
    public delegate void DebugDelegate(int type, string debugString);

    [DllImport(DLL, EntryPoint = "register_debug_callback")]
    private static extern void RegisterDebugCallback(IntPtr funcPtr);

    [DllImport(DLL, EntryPoint = "register_geom_log_callback")]
    private static extern void RegisterGeomLogCallback(IntPtr funcPtr);

    [DllImport(DLL, EntryPoint = "register_av_log_callback")]
    private static extern void RegisterAvLogCallback(IntPtr funcPtr);

    [DllImport(DLL, EntryPoint = "clear_logging_functions")]
    public static extern void ClearLoggingFunctions();

    //Geometry file functions
    [DllImport(DLL, EntryPoint = "native_vol_open_geom_file")]
    public static extern bool VolGeomOpenFile(string headerFile, string sequenceFile, bool streamingMode);

    [DllImport(DLL, EntryPoint = "native_vol_free_geom_data")]
    public static extern bool VolFreeGeomData();

    [DllImport(DLL, EntryPoint = "native_vol_get_geom_frame_count")]
    public static extern int VolGeomGetFrameCount();
    
    [DllImport(DLL, EntryPoint = "native_vol_read_geom_frame")]
    public static extern bool VolGeomReadFrame(string sequenceFile, int frame);

    [DllImport(DLL, EntryPoint = "native_vol_geom_is_keyframe")]
    public static extern bool VolGeomIsKeyframe(int frame);

    [DllImport(DLL, EntryPoint = "native_vol_geom_find_previous_keyframe")]
    public static extern int VolGeomFindPreviousKeyframe(int frame);

    [DllImport(DLL, EntryPoint = "native_vol_get_geom_ptr_data")]
    public static extern VolGeometryData VolGeomGetPtrData();

    // Video file functions
    [DllImport(DLL, EntryPoint = "native_vol_open_video_file")]
    public static extern bool VolOpenFile(string filename);

    [DllImport(DLL, EntryPoint = "native_vol_close_video_file")]
    public static extern bool VolCloseFile();

    [DllImport(DLL, EntryPoint = "native_vol_get_video_width")]
    public static extern int VolGetVideoWidth();

    [DllImport(DLL, EntryPoint = "native_vol_get_video_height")]
    public static extern int VolGetVideoHeight();

    [DllImport(DLL, EntryPoint = "native_vol_get_video_frame_rate")]
    public static extern double VolGetFrameRate();

    [DllImport(DLL, EntryPoint = "native_vol_get_video_frame_count")]
    public static extern long VolGetNumFrames();

    [DllImport(DLL, EntryPoint = "native_vol_get_video_duration")]
    public static extern double VolGetDuration();

    [DllImport(DLL, EntryPoint = "native_vol_get_video_frame_size")]
    public static extern long VolGetFrameSize();

    [DllImport(DLL, EntryPoint = "native_vol_read_next_video_frame")]
    public static extern IntPtr VolReadNextVideoFrame(bool flipVertical);

    //[DllImport(DLL, EntryPoint = "get_texture_update_callback")]
    //private static extern System.IntPtr GetTextureUpdateCallback();

    //private static CommandBuffer _commandBuffer;
    
    [MonoPInvokeCallback(typeof(DebugDelegate))]
    private static void DebugCallbackFunction(int logType, string debugString)
    {
        switch (logType)
        {
            case 0: 
                if (interfaceLoggingLevel.HasFlag(VolEnums.LoggingLevel.Info))
                    Debug.Log($"VOL_LIB {debugString}");
                break;
            case 1:
                if (interfaceLoggingLevel.HasFlag(VolEnums.LoggingLevel.Debug))
                    Debug.Log($"VOL_LIB {debugString}");
                break;
            case 2:
                if (interfaceLoggingLevel.HasFlag(VolEnums.LoggingLevel.Warning))
                    Debug.LogWarning($"VOL_LIB {debugString}");
                break;
            case 3:
                if (interfaceLoggingLevel.HasFlag(VolEnums.LoggingLevel.Error))
                    Debug.LogError($"VOL_LIB {debugString}");
                break;
            default:
                Debug.Log(debugString);
                break;
        }
    }
    
    [MonoPInvokeCallback(typeof(DebugDelegate))]
    private static void DebugGeomCallbackFunction(int logType, string debugString)
    {
        switch (logType)
        {
            case 0: 
                if (geomLoggingLevel.HasFlag(VolEnums.LoggingLevel.Info))
                    Debug.Log($"VOL_GEOM {debugString}");
                break;
            case 1:
                if (geomLoggingLevel.HasFlag(VolEnums.LoggingLevel.Debug))
                    Debug.Log($"VOL_GEOM {debugString}");
                break;
            case 2:
                if (geomLoggingLevel.HasFlag(VolEnums.LoggingLevel.Warning))
                    Debug.LogWarning($"VOL_GEOM {debugString}");
                break;
            case 3:
                if (geomLoggingLevel.HasFlag(VolEnums.LoggingLevel.Error))
                    Debug.LogError($"VOL_GEOM {debugString}");
                break;
            default:
                Debug.Log(debugString);
                break;
        }
    }
    
    [MonoPInvokeCallback(typeof(DebugDelegate))]
    private static void DebugAvCallbackFunction(int logType, string debugString)
    {
        switch (logType)
        {
            case 0:
                if (avLoggingLevel.HasFlag(VolEnums.LoggingLevel.Info))
                    Debug.Log($"VOL_AV {debugString}");
                break;
            case 1:
                if (avLoggingLevel.HasFlag(VolEnums.LoggingLevel.Debug))
                    Debug.Log($"VOL_AV {debugString}");
                break;
            case 2:
                if (avLoggingLevel.HasFlag(VolEnums.LoggingLevel.Warning))
                    Debug.LogWarning($"VOL_AV {debugString}");
                break;
            case 3:
                if (avLoggingLevel.HasFlag(VolEnums.LoggingLevel.Error))
                    Debug.LogError($"VOL_AV {debugString}");
                break;
            default:
                Debug.Log(debugString);
                break;
        }
    }
    
    
    public static void EnableInterfaceLogging()
    {
        DebugDelegate debugDelegate = DebugCallbackFunction;
        IntPtr delegatePtr = Marshal.GetFunctionPointerForDelegate(debugDelegate);
        RegisterDebugCallback(delegatePtr);
    }

    public static void EnableGeomLogging()
    {
        DebugDelegate debugGeomDelegate = DebugGeomCallbackFunction;
        IntPtr delegateGeomPtr = Marshal.GetFunctionPointerForDelegate(debugGeomDelegate);
        RegisterGeomLogCallback(delegateGeomPtr);
    }

    public static void EnableAvLogging()
    {
        DebugDelegate debugAvDelegate = DebugAvCallbackFunction;
        IntPtr delegateAvPtr = Marshal.GetFunctionPointerForDelegate(debugAvDelegate);
        RegisterAvLogCallback(delegateAvPtr);
    }
    
    /* -- TODO: REIMPLEMENT 
    public static void InitCommandBuffer()
    {
        _commandBuffer = new CommandBuffer();
    }

    public static void UpdateTexture(Texture texture)
    {
        _commandBuffer.IssuePluginCustomTextureUpdateV2(GetTextureUpdateCallback(), texture, (uint)Time.time * 60);
        Graphics.ExecuteCommandBuffer(_commandBuffer);
        _commandBuffer.Clear();
    }*/
}
