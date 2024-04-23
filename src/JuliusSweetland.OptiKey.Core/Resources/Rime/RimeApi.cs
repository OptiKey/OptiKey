using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace JuliusSweetland.OptiKey.Rime {
    public class RimeCommand {
        public const string Suggestion1 = "{Control+1}";
        public const string Suggestion2 = "{Control+2}";
        public const string Suggestion3 = "{Control+3}";
        public const string Suggestion4 = "{Control+4}";
        public const string Suggestion5 = "{Control+5}";
        public const string Suggestion6 = "{Control+6}"; 
        public const string PreviousSuggestions = "{Page_Up}"; 
        public const string NextSuggestions = "{Page_Down}"; 
    }
    public class MyRimeApi {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static UIntPtr SessionId = UIntPtr.Zero;
        public static bool IsAsciiMode = false;
        public static bool IsComposing = false;
        public static bool IsFirstPage = false;
        public static bool IsLastPage = false;

        public static RimeNotificationHandler NotificationHandler;

        public static UIntPtr GetSession() {
            if (SessionId != UIntPtr.Zero) {
                Log.Info("Session ID " + SessionId);
                return SessionId;
            }

            Log.Info("Getting RIME session");
            var rime = rime_get_api();

            // Register for notifications
            NotificationHandler = on_message; // keep static instance of delegate alive
            rime.set_notification_handler(NotificationHandler, System.IntPtr.Zero);

            // setup 
            var traits = new RimeTraits();
            RIME_STRUCT(ref traits);
            traits.app_name = "optikey";
            traits.shared_data_dir = Settings.Default.RimeLocation;
            traits.user_data_dir = Path.Combine(Settings.Default.RimeLocation, "user");
            rime.setup(ref traits);

            var traits_null = new RimeTraits();
            rime.initialize(ref traits_null);

            bool full_check = true;
            if (rime.start_maintenance(full_check)) {
                Log.Info("Running RIME maintenance check");
                rime.join_maintenance_thread(); 
            }
            Log.Info("Ready for RIME");            
            SessionId = rime.create_session();
            if (SessionId == UIntPtr.Zero) {
                throw new ApplicationException("Error creating RIME session for Chinese text input");                
            }
            return SessionId;
        }

        public static void on_message(System.IntPtr context_object, UIntPtr session_id, string message_type, string message_value)
        {
            if (message_type == "failure")
            {
                Log.ErrorFormat($"RIME session {session_id}, {message_type} message: {message_value}");
            }
            else
            {
                Log.InfoFormat($"RIME session {session_id}, {message_type} message: {message_value}");
            }
        }

        public static void SelectSchema() {
            Log.Info("RIME SelectSchema");
            var language = Settings.Default.KeyboardAndDictionaryLanguage;
            if (!language.ManagedByRime()) {
                Log.Warn($"Current language {language} not managed by RIME");
                return;
            }
            var rime = rime_get_api();
            rime.select_schema(GetSession(), language.GetRimeSchemaId());
            var option = language.GetRimeOption();
            if (!string.IsNullOrWhiteSpace(option)) {
                if (option.StartsWith("!")) {
                    rime.set_option(GetSession(), option.Substring(1), false);
                } else {
                    rime.set_option(GetSession(), option, true);
                }
            }
            Log.Info("Finished setting RIME schema");
        }
        public static List<RimeCandidate> GetCandidates(RimeMenu menu) {
            var result = new List<RimeCandidate>();
            if (menu.num_candidates == 0) return result;
            var itemSize = Marshal.SizeOf(typeof(RimeCandidate));
            for (int i = 0; i < menu.num_candidates; ++i) {
                IntPtr ins = new IntPtr(menu.candidates.ToInt64() + i * itemSize);
                result.Add(Marshal.PtrToStructure<RimeCandidate>(ins));
            }
            return result;
        }
        public static void Clear()
        {
            if (SessionId != UIntPtr.Zero)
            {
                var rime = rime_get_api();
                rime.clear_composition(SessionId);
            }
        }
        [DllImport("Resources/Rime/rime.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern RimeApi rime_get_api();
        public static void RIME_STRUCT<T>(ref T value) {
            dynamic temp = value;
            temp.data_size = 0;
            value = temp;
            RIME_STRUCT_INIT(ref value);
        }
        private static void RIME_STRUCT_INIT<T>(ref T value) {
            dynamic temp = value;
            temp.data_size = Marshal.SizeOf(typeof(T));
            value = temp;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeApi {
        public int data_size;
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeSetup(ref RimeTraits traits);
        public void setup(ref RimeTraits traits) {
            RimeSetup(ref traits);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeSetNotificationHandler(RimeNotificationHandler handler, System.IntPtr context_object);
        public void set_notification_handler(RimeNotificationHandler handler, System.IntPtr context_object) {
            RimeSetNotificationHandler(handler, context_object);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeInitialize(ref RimeTraits traits);
        public void initialize(ref RimeTraits traits) {
            RimeInitialize(ref traits);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeStartMaintenance(bool full_check);
        public bool start_maintenance(bool full_check) {
            return RimeStartMaintenance(full_check);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeJoinMaintenanceThread();
        public void join_maintenance_thread() {
            RimeJoinMaintenanceThread();
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern UIntPtr RimeCreateSession();
        public UIntPtr create_session() {
            return RimeCreateSession();
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeSimulateKeySequence(UIntPtr session_id, string key_sequence);
        public bool simulate_key_sequence(UIntPtr session_id, string key_sequence) {
            return RimeSimulateKeySequence(session_id, key_sequence);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeGetSchemaList(ref RimeSchemaList schemaList);
        public bool get_schema_list(ref RimeSchemaList schemaList) {
            return RimeGetSchemaList(ref schemaList);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeFreeSchemaList(ref RimeSchemaList schemaList);
        public void free_schema_list(ref RimeSchemaList schema) {
            RimeFreeSchemaList(ref schema);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeSelectCandidateOnCurrentPage(UIntPtr session_id, int index);
        public bool select_candidate_on_current_page(UIntPtr session_id, int index) {
            return RimeSelectCandidateOnCurrentPage(session_id, index);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeCandidateListBegin(UIntPtr session_id, [Out] RimeCandidateListIterator iterator);
        public bool candidate_list_begin(UIntPtr session_id, ref RimeCandidateListIterator iterator) {
            return RimeCandidateListBegin(session_id, iterator);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeCandidateListNext([Out] RimeCandidateListIterator iterator);
        public bool candidate_list_next(ref RimeCandidateListIterator iterator) {
            return RimeCandidateListNext(iterator);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeCandidateListEnd([Out] RimeCandidateListIterator iterator);
        public void candidate_list_end(ref RimeCandidateListIterator iterator) {
            RimeCandidateListEnd(iterator);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeGetCommit(UIntPtr session_id, ref RimeCommit commit);
        public bool get_commit(UIntPtr session_id, ref RimeCommit commit) {
            return RimeGetCommit(session_id, ref commit);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeGetCurrentSchema(UIntPtr session_id, [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 100)] char[] schema_id, int buffer_size);
        public bool get_current_schema(UIntPtr session_id, ref char[] schema_id, int buffer_size) {
            return RimeGetCurrentSchema(session_id, schema_id, buffer_size);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeFreeCommit(ref RimeCommit commit);
        public bool free_commit(ref RimeCommit commit) {
            return RimeFreeCommit(ref commit);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeSelectSchema(UIntPtr session_id, string schema_id);
        public bool select_schema(UIntPtr session_id, string schema_id) {
            return RimeSelectSchema(session_id, schema_id);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeGetStatus(UIntPtr session_id, ref RimeStatus status);
        public bool get_status(UIntPtr session_id, ref RimeStatus status) {
            return RimeGetStatus(session_id, ref status);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeFreeStatus(ref RimeStatus status);
        public bool free_status(ref RimeStatus status) {
            return RimeFreeStatus(ref status);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeGetContext(UIntPtr session_id, ref RimeContext context);
        public bool get_context(UIntPtr session_id, ref RimeContext context) {
            return RimeGetContext(session_id, ref context);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeFreeContext(ref RimeContext context);
        public bool free_context(ref RimeContext context) {
            return RimeFreeContext(ref context);
        }

        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeSetupLogging(ref string app_name);
        public void setup_logging(ref string app_name) {
            RimeSetupLogging(ref app_name);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeFinalize();
        public void finalize() {
            RimeFinalize();
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeDestroySession(UIntPtr session_id);
        public bool destroy_session(UIntPtr session_id) {
            return RimeDestroySession(session_id);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RimeClearComposition(UIntPtr session_id);
        public void clear_composition(UIntPtr session_id)
        {
            RimeClearComposition(session_id);
        }
        [DllImport("Resources/Rime/rime.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RimeSetOption(UIntPtr session_id, string option, bool value);
        public bool set_option(UIntPtr session_id, string option, bool value) {
            return RimeSetOption(session_id, option, value);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeTraits {
        public int data_size;
        [MarshalAs(UnmanagedType.LPStr)] public string shared_data_dir;
        [MarshalAs(UnmanagedType.LPStr)] public string user_data_dir;
        [MarshalAs(UnmanagedType.LPStr)] public string distribution_name;
        [MarshalAs(UnmanagedType.LPStr)] public string distribution_code_name;
        [MarshalAs(UnmanagedType.LPStr)] public string distribution_version;
        [MarshalAs(UnmanagedType.LPStr)] public string app_name;
        public IntPtr modules;
        public int min_log_level;
        [MarshalAs(UnmanagedType.LPStr)] public string log_dir;
        [MarshalAs(UnmanagedType.LPStr)] public string prebuild_data_dir;
        [MarshalAs(UnmanagedType.LPStr)] public string staging_dir;
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RimeNotificationHandler(System.IntPtr context_object, UIntPtr session_id, string message_type, string message_value);
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeComposition {
        public int length;
        public int cursor_pos;
        public int sel_start;
        public int sel_end;
        [MarshalAs(UnmanagedType.LPStr)] public string preedit;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeCandidate {
        [MarshalAs(UnmanagedType.LPStr)] public string text;
        [MarshalAs(UnmanagedType.LPStr)] public string comment;
        public IntPtr reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeMenu {
        public int page_size;
        public int page_no;
        public bool is_last_page;
        public int highlighted_candidate_index;
        public int num_candidates;
        public IntPtr candidates;
        [MarshalAs(UnmanagedType.LPStr)] public string select_keys;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeCommit {
        public int data_size;
        [MarshalAs(UnmanagedType.LPStr)] public string text;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeContext {
        public int data_size;
        public RimeComposition composition;
        public RimeMenu menu;
        [MarshalAs(UnmanagedType.LPStr)] public string commit_text_preview;
        //char** select_labels;
        public IntPtr select_labels;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeStatus {
        public int data_size;
        [MarshalAs(UnmanagedType.LPStr)] public string schema_id;
        [MarshalAs(UnmanagedType.LPStr)] public string schema_name;
        public bool is_disabled;
        public bool is_composing;
        public bool is_ascii_mode;
        public bool is_full_shape;
        public bool is_simplified;
        public bool is_traditional;
        public bool is_ascii_punct;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeCandidateListIterator {
        public IntPtr ptr;
        public int index;
        public RimeCandidate candidate;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeConfig {
        public IntPtr ptr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RimeConfigIterator {
        public IntPtr list;
        public IntPtr map;
        public int index;
        [MarshalAs(UnmanagedType.LPStr)] public string key;
        [MarshalAs(UnmanagedType.LPStr)] public string path;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct RimeSchemaListItem {
        [MarshalAs(UnmanagedType.LPStr)] public string schema_id;
        [MarshalAs(UnmanagedType.LPStr)] public string name;
        public IntPtr reserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RimeSchemaList {
        public uint size;
        public IntPtr list;
    }
}
