/*
COPYRIGHT 2017 - PROPERTY OF TOBII AB
-------------------------------------
2017 TOBII AB - KARLSROVAGEN 2D, DANDERYD 182 53, SWEDEN - All Rights Reserved.

NOTICE:  All information contained herein is, and remains, the property of Tobii AB and its suppliers, if any.
The intellectual and technical concepts contained herein are proprietary to Tobii AB and its suppliers and may be
covered by U.S.and Foreign Patents, patent applications, and are protected by trade secret or copyright law.
Dissemination of this information or reproduction of this material is strictly forbidden unless prior written
permission is obtained from Tobii AB.
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Tobii.StreamEngine
{
    public static class Interop
    {
        public const string stream_engine_dll = "tobii_stream_engine";

        public static string tobii_error_message(tobii_error_t result_code)
        {
            var pStr = tobii_error_message_internal(result_code);
            return Marshal.PtrToStringAnsi(pStr);
        }

        [DllImport(stream_engine_dll, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "tobii_error_message")]
        private static extern IntPtr tobii_error_message_internal(tobii_error_t result_code);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "tobii_get_api_version")]
        public static extern tobii_error_t tobii_get_api_version(out tobii_version_t version);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void tobii_log_func_t(IntPtr log_context, tobii_log_level_t level, string text);

        public static tobii_error_t tobii_api_create(out IntPtr api, tobii_custom_log_t custom_log)
        {
            return tobii_api_create(out api, IntPtr.Zero, custom_log);
        }

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_api_create")]
        private static extern tobii_error_t tobii_api_create(out IntPtr api, IntPtr customAlloc, tobii_custom_log_t custom_log); // Custom alloc doesn't make sense in .NET

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_api_destroy")]
        public static extern tobii_error_t tobii_api_destroy(IntPtr api);

        public static tobii_error_t tobii_enumerate_local_device_urls(IntPtr api, out List<string> device_urls)
        {
            var urls = new List<string>();
            tobii_device_url_receiver_t handler = (url, data) => { urls.Add(url); };
            var result = tobii_enumerate_local_device_urls_internal(api, handler, IntPtr.Zero);

            device_urls = urls;

            return result;
        }

        public enum tobii_device_generations_t
        {
            G5 = 0x00000002,
            IS3 = 0x00000004,
            IS4 = 0x00000008,
        }

        public static tobii_error_t tobii_enumerate_local_device_urls_ex(IntPtr api, out List<string> device_urls, uint device_generations)
        {
            var urls = new List<string>();
            tobii_device_url_receiver_t handler = (url, data) => { urls.Add(url); };
            var result = tobii_enumerate_local_device_urls_ex_internal(api, handler, IntPtr.Zero, device_generations);

            device_urls = urls;

            return result;
        }

        public enum tobii_field_of_use_t
        {
            TOBII_FIELD_OF_USE_STORE_OR_TRANSFER_FALSE = 1,
            TOBII_FIELD_OF_USE_STORE_OR_TRANSFER_TRUE = 2,
        }

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "tobii_enumerate_local_device_urls")]
        public static extern tobii_error_t tobii_enumerate_local_device_urls_internal(IntPtr api, tobii_device_url_receiver_t receiverFunction, IntPtr userData);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "tobii_enumerate_local_device_urls_ex")]
        public static extern tobii_error_t tobii_enumerate_local_device_urls_ex_internal(IntPtr api, tobii_device_url_receiver_t receiverFunction, IntPtr userData, uint deviceGenerations);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "tobii_device_create")]
        public static extern tobii_error_t tobii_device_create(IntPtr api, string url, tobii_field_of_use_t field_of_use, out IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_device_create_ex")]
        private static extern tobii_error_t tobii_device_create_ex_internal(IntPtr api, string url, tobii_field_of_use_t field_of_use, tobii_license_key_t[] license_keys, int license_count, [MarshalAs(UnmanagedType.LPArray)] tobii_license_validation_result_t[] licenseResults, out IntPtr device);

        public static tobii_error_t tobii_device_create_ex(IntPtr api, string url, tobii_field_of_use_t field_of_use, string[] license_keys, List<tobii_license_validation_result_t> license_results, out IntPtr device)
        {
            var keys = new List<tobii_license_key_t>();

            foreach (var key in license_keys)
            {
                keys.Add(new tobii_license_key_t { license_key = key, size_in_bytes = new IntPtr(key.Length * 2) });
            }

            var license_results_array = new tobii_license_validation_result_t[license_keys.Length];
            var tobii_error = tobii_device_create_ex_internal(api, url, field_of_use, keys.ToArray(), keys.Count, license_results_array, out device);

            if (license_results != null)
            {
                license_results.InsertRange(0, license_results_array);
            }

            return tobii_error;
        }

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "tobii_get_feature_group")]
        public static extern tobii_error_t tobii_get_feature_group(IntPtr device, out tobii_feature_group_t feature_group);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "tobii_device_destroy")]
        public static extern tobii_error_t tobii_device_destroy(IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "tobii_wait_for_callbacks")]
        private static extern tobii_error_t tobii_wait_for_callbacks_internal(int device_count, IntPtr[] devices);

        public static tobii_error_t tobii_wait_for_callbacks(IntPtr[] devices)
        {
            var length = (devices != null) ? devices.Length : 0;
            var tobii_error = tobii_wait_for_callbacks_internal(length, devices);
            return tobii_error;
        }

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "tobii_device_process_callbacks")]
        public static extern tobii_error_t tobii_device_process_callbacks(IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "tobii_device_clear_callback_buffers")]
        public static extern tobii_error_t tobii_device_clear_callback_buffers(IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "tobii_device_reconnect")]
        public static extern tobii_error_t tobii_device_reconnect(IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "tobii_system_clock")]
        public static extern tobii_error_t tobii_system_clock(IntPtr api, out long timestamp_us);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "tobii_get_device_info")]
        public static extern tobii_error_t tobii_get_device_info(IntPtr device, out tobii_device_info_t device_info);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl,
           EntryPoint = "tobii_license_key_store")]
        public static extern tobii_error_t tobii_license_key_store(IntPtr device, IntPtr data, IntPtr size);

        public static tobii_error_t tobii_license_key_store(IntPtr device, byte[] license_key)
        {
            var ptr = Marshal.AllocHGlobal(license_key.Length);
            tobii_error_t result;

            try
            {
                Marshal.Copy(license_key, 0, ptr, license_key.Length);
                result = tobii_license_key_store(device, ptr, new IntPtr(license_key.Length));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return result;
        }

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "tobii_license_key_retrieve")]
        public static extern tobii_error_t tobii_license_key_retrieve(IntPtr device, tobii_data_receiver_t receiver);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_get_state_bool")]
        public static extern tobii_error_t tobii_get_state_bool(IntPtr device, tobii_state_t state, out tobii_state_bool_t value);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_get_state_uint32")]
        public static extern tobii_error_t tobii_get_state_uint32(IntPtr device, tobii_state_t state, out uint value);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_get_state_string")]
        private static extern tobii_error_t tobii_get_state_string(IntPtr device, tobii_state_t state, StringBuilder value);

        public static tobii_error_t tobii_get_state_string(IntPtr device, tobii_state_t state, out string value)
        {
            var val = new StringBuilder(512);
            var result = tobii_get_state_string(device, state, val);
            value = val.ToString();
            return result;
        }

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_notifications_subscribe")]
        public static extern tobii_error_t tobii_notifications_subscribe(IntPtr device, tobii_notifications_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_notifications_unsubscribe")]
        public static extern tobii_error_t tobii_notifications_unsubscribe(IntPtr device);


        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_capability_supported")]
        public static extern tobii_error_t tobii_capability_supported(IntPtr device, tobii_capability_t capability, out bool supported);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_stream_supported")]
        public static extern tobii_error_t tobii_stream_supported(IntPtr device, tobii_stream_t stream, out bool supported);

        #region Streams

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_gaze_point_subscribe")]
        public static extern tobii_error_t tobii_gaze_point_subscribe(IntPtr device, tobii_gaze_point_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_gaze_point_unsubscribe")]
        public static extern tobii_error_t tobii_gaze_point_unsubscribe(IntPtr device);


        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_gaze_origin_subscribe")]
        public static extern tobii_error_t tobii_gaze_origin_subscribe(IntPtr device, tobii_gaze_origin_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_gaze_origin_unsubscribe")]
        public static extern tobii_error_t tobii_gaze_origin_unsubscribe(IntPtr device);


        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_eye_position_normalized_subscribe")]
        public static extern tobii_error_t tobii_eye_position_normalized_subscribe(IntPtr device, tobii_eye_position_normalized_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_eye_position_normalized_unsubscribe")]
        public static extern tobii_error_t tobii_eye_position_normalized_unsubscribe(IntPtr device);


        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_user_presence_subscribe")]
        public static extern tobii_error_t tobii_user_presence_subscribe(IntPtr device, tobii_user_presence_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_user_presence_unsubscribe")]
        public static extern tobii_error_t tobii_user_presence_unsubscribe(IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_head_pose_subscribe")]
        public static extern tobii_error_t tobii_head_pose_subscribe(IntPtr device, tobii_head_pose_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_head_pose_unsubscribe")]
        public static extern tobii_error_t tobii_head_pose_unsubscribe(IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_get_track_box")]
        public static extern tobii_error_t tobii_get_track_box(IntPtr device, out tobii_track_box_t track_box);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_update_timesync")]
        public static extern tobii_error_t tobii_update_timesync(IntPtr device);


        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_user_position_guide_subscribe")]
        public static extern tobii_error_t tobii_user_position_guide_subscribe(IntPtr device, tobii_user_position_guide_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_user_position_guide_unsubscribe")]
        public static extern tobii_error_t tobii_user_position_guide_unsubscribe(IntPtr device);


        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_responsive_gaze_point_subscribe")]
        public static extern tobii_error_t tobii_responsive_gaze_point_subscribe(IntPtr device, tobii_responsive_gaze_point_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_responsive_gaze_point_unsubscribe")]
        public static extern tobii_error_t tobii_responsive_gaze_point_unsubscribe(IntPtr device);

        #endregion


        #region Wearable

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "tobii_wearable_consumer_data_subscribe")]
        public static extern tobii_error_t tobii_wearable_consumer_data_subscribe(IntPtr device, tobii_wearable_consumer_data_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "tobii_wearable_consumer_data_unsubscribe")]
        public static extern tobii_error_t tobii_wearable_consumer_data_unsubscribe(IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "tobii_wearable_advanced_data_subscribe")]
        public static extern tobii_error_t tobii_wearable_advanced_data_subscribe(IntPtr device, tobii_wearable_advanced_data_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "tobii_wearable_advanced_data_unsubscribe")]
        public static extern tobii_error_t tobii_wearable_advanced_data_unsubscribe(IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_get_lens_configuration")]
        public static extern tobii_error_t tobii_get_lens_configuration(IntPtr device, out tobii_lens_configuration_t configuration);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_set_lens_configuration")]
        public static extern tobii_error_t tobii_set_lens_configuration(IntPtr device, ref tobii_lens_configuration_t configuration);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_lens_configuration_writable")]
        public static extern tobii_error_t tobii_lens_configuration_writable(IntPtr device, out tobii_lens_configuration_writable_t writable);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "tobii_wearable_foveated_gaze_subscribe")]
        public static extern tobii_error_t tobii_wearable_foveated_gaze_subscribe(IntPtr device, tobii_wearable_foveated_gaze_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "tobii_wearable_foveated_gaze_unsubscribe")]
        public static extern tobii_error_t tobii_wearable_foveated_gaze_unsubscribe(IntPtr device);

        #endregion

        #region Config

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_get_enabled_eye")]
        public static extern tobii_error_t tobii_get_enabled_eye(IntPtr device, out tobii_enabled_eye_t enabled_eye);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_set_enabled_eye")]
        public static extern tobii_error_t tobii_set_enabled_eye(IntPtr device, tobii_enabled_eye_t enabled_eye);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_start")]
        public static extern tobii_error_t tobii_calibration_start(IntPtr device, tobii_enabled_eye_t enabled_eye);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_stop")]
        public static extern tobii_error_t tobii_calibration_stop(IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_clear")]
        public static extern tobii_error_t tobii_calibration_clear(IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_collect_data_2d")]
        public static extern tobii_error_t tobii_calibration_collect_data_2d(IntPtr device, float x, float y);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_collect_data_3d")]
        public static extern tobii_error_t tobii_calibration_collect_data_3d(IntPtr device, float x, float y, float z);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_collect_data_per_eye_2d")]
        public static extern tobii_error_t tobii_calibration_collect_data_per_eye_2d(IntPtr device, float x, float y, tobii_enabled_eye_t requested_eyes, out tobii_enabled_eye_t collected_eyes);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_discard_data_2d")]
        public static extern tobii_error_t tobii_calibration_discard_data_2d(IntPtr device, float x, float y);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_discard_data_3d")]
        public static extern tobii_error_t tobii_calibration_discard_data_3d(IntPtr device, float x, float y, float z);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_discard_data_per_eye_2d")]
        public static extern tobii_error_t tobii_calibration_discard_data_per_eye_2d(IntPtr device, float x, float y, tobii_enabled_eye_t calibrated_eyes);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_compute_and_apply")]
        public static extern tobii_error_t tobii_calibration_compute_and_apply(IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_compute_and_apply_per_eye")]
        public static extern tobii_error_t tobii_calibration_compute_and_apply_per_eye(IntPtr device, out tobii_enabled_eye_t collected_eyes);

        public static tobii_error_t tobii_calibration_retrieve(IntPtr device, out byte[] calibration)
        {
            byte[] buffer = null;

            var result = tobii_calibration_retrieve(device, (data, size, userData) =>
            {
                buffer = new byte[size.ToInt32()];
                if (size.ToInt32() > 0)
                {
                    Marshal.Copy(data, buffer, 0, size.ToInt32());
                }
            });

            calibration = buffer;

            return result;
        }

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_retrieve")]
        public static extern tobii_error_t tobii_calibration_retrieve(IntPtr device, tobii_data_receiver_t callback, IntPtr user_data = default(IntPtr));

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void tobii_calibration_point_data_receiver_t(ref tobii_calibration_point_data_t point_data, IntPtr user_data);

        public static tobii_error_t tobii_calibration_parse(IntPtr api, byte[] calibration, out List<tobii_calibration_point_data_t> point_data_items)
        {
            var points = new List<tobii_calibration_point_data_t>();
            var p = Marshal.AllocHGlobal(calibration.Length);
            Marshal.Copy(calibration, 0, p, calibration.Length);
            var result = tobii_calibration_parse(api, p, new IntPtr(calibration.Length), (ref tobii_calibration_point_data_t point_data, IntPtr user_data) =>
            {
                tobii_calibration_point_data_t point;
                point.point_xy = point_data.point_xy;
                point.left_status = point_data.left_status;
                point.left_mapping_xy = point_data.left_mapping_xy;
                point.right_status = point_data.right_status;
                point.right_mapping_xy = point_data.right_mapping_xy;

                points.Add(point);
            }, IntPtr.Zero);

            Marshal.FreeHGlobal(p);
            point_data_items = points;

            return result;
        }

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_parse")]
        public static extern tobii_error_t tobii_calibration_parse(IntPtr api, IntPtr calibration, IntPtr calibration_size, tobii_calibration_point_data_receiver_t callback, IntPtr user_data);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_apply")]
        public static extern tobii_error_t tobii_calibration_apply(IntPtr device, IntPtr data, IntPtr size);

        public static tobii_error_t tobii_calibration_apply(IntPtr device, byte[] calibration)
        {
            var ptr = Marshal.AllocHGlobal(calibration.Length);
            tobii_error_t result;

            try
            {
                Marshal.Copy(calibration, 0, ptr, calibration.Length);
                result = tobii_calibration_apply(device, ptr, new IntPtr(calibration.Length));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return result;
        }

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calculate_display_area_basic")]
        public static extern tobii_error_t tobii_calculate_display_area_basic(IntPtr api, float width_mm, float height_mm, float offset_x_mm, ref tobii_geometry_mounting_t geometry_mounting, out tobii_display_area_t display_area);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_get_display_area")]
        public static extern tobii_error_t tobii_get_display_area(IntPtr device, out tobii_display_area_t display_area);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_set_display_area")]
        public static extern tobii_error_t tobii_set_display_area(IntPtr device, ref tobii_display_area_t display_area);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_get_geometry_mounting")]
        public static extern tobii_error_t tobii_get_geometry_mounting(IntPtr device, out tobii_geometry_mounting_t geometry);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "tobii_get_device_name")]
        private static extern tobii_error_t tobii_get_device_name(IntPtr device, StringBuilder device_name);

        public static tobii_error_t tobii_get_device_name(IntPtr device, out string device_name)
        {
            var dn = new StringBuilder(64);
            var result = tobii_get_device_name(device, dn);
            device_name = dn.ToString();
            return result;
        }

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "tobii_set_device_name")]
        public static extern tobii_error_t tobii_set_device_name(IntPtr device, string device_name);


        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_enumerate_output_frequencies")]
        public static extern tobii_error_t tobii_enumerate_output_frequencies(IntPtr device, tobii_output_frequency_receiver_t receiver);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_set_output_frequency")]
        public static extern tobii_error_t tobii_set_output_frequency(IntPtr device, float output_frequency);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_get_output_frequency")]
        public static extern tobii_error_t tobii_get_output_frequency(IntPtr device, out float output_frequency);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_output_frequency_subscribe")]
        public static extern tobii_error_t tobii_output_frequency_subscribe(IntPtr device, tobii_output_frequency_receiver_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_output_frequency_unsubscribe")]
        public static extern tobii_error_t tobii_output_frequency_unsubscribe(IntPtr device);


        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "tobii_get_display_id")]
        private static extern tobii_error_t tobii_get_display_id(IntPtr device, StringBuilder display_id);

        public static tobii_error_t tobii_get_display_id(IntPtr device, out string display_id)
        {
            var di = new StringBuilder(256);
            var result = tobii_get_display_id(device, di);
            display_id = di.ToString();
            return result;
        }

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "tobii_set_display_id")]
        public static extern tobii_error_t tobii_set_display_id(IntPtr device, string display_id);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void tobii_display_id_callback_t(string display_id, IntPtr user_data);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_display_id_subscribe")]
        public static extern tobii_error_t tobii_display_id_subscribe(IntPtr device, tobii_display_id_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_display_id_unsubscribe")]
        public static extern tobii_error_t tobii_display_id_unsubscribe(IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_calibration_stimulus_points_get")]
        public static extern tobii_error_t tobii_calibration_stimulus_points_get(IntPtr device, out tobii_calibration_stimulus_points_t stimulus_points);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "tobii_get_faults")]
        private static extern tobii_error_t tobii_get_faults(IntPtr device, StringBuilder faults);

        public static tobii_error_t tobii_get_faults(IntPtr device, out string faults)
        {
            var fault = new StringBuilder(512);
            var result = tobii_get_faults(device, fault);
            faults = fault.ToString();
            return result;
        }
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void tobii_faults_callback_t(string faults, IntPtr user_data);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_faults_subscribe")]
        public static extern tobii_error_t tobii_faults_subscribe(IntPtr device, tobii_faults_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_faults_unsubscribe")]
        public static extern tobii_error_t tobii_faults_unsubscribe(IntPtr device);
        
        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "tobii_get_warnings")]
        private static extern tobii_error_t tobii_get_warnings(IntPtr device, StringBuilder warnings);

        public static tobii_error_t tobii_get_warnings(IntPtr device, out string warnings)
        {
            var warning = new StringBuilder(512);
            var result = tobii_get_warnings(device, warning);
            warnings = warning.ToString();
            return result;
        }
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void tobii_warnings_callback_t(string warnings, IntPtr user_data);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_warnings_subscribe")]
        public static extern tobii_error_t tobii_warnings_subscribe(IntPtr device, tobii_warnings_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_warnings_unsubscribe")]
        public static extern tobii_error_t tobii_warnings_unsubscribe(IntPtr device);
  
        #endregion

        #region Advanced

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_gaze_data_subscribe")]
        public static extern tobii_error_t tobii_gaze_data_subscribe(IntPtr device, tobii_gaze_data_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_gaze_data_unsubscribe")]
        public static extern tobii_error_t tobii_gaze_data_unsubscribe(IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_enumerate_illumination_modes")]
        public static extern tobii_error_t tobii_enumerate_illumination_modes(IntPtr device, tobii_illumination_mode_receiver_t receiver);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_get_illumination_mode")]
        private static extern tobii_error_t tobii_get_illumination_mode(IntPtr device, StringBuilder illumination_mode);

        public static tobii_error_t tobii_get_illumination_mode(IntPtr device, out string illumination_mode)
        {
            var im = new StringBuilder(64);
            var result = tobii_get_illumination_mode(device, im);
            illumination_mode = im.ToString();
            return result;
        }

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_set_illumination_mode")]
        public static extern tobii_error_t tobii_set_illumination_mode(IntPtr device, string illumination_mode);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_timesync")]
        public static extern tobii_error_t tobii_timesync(IntPtr device, out tobii_timesync_data_t timesync);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_digital_syncport_subscribe")]
        public static extern tobii_error_t tobii_digital_syncport_subscribe(IntPtr device, tobii_digital_syncport_callback_t callback, IntPtr user_data = default(IntPtr));

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_digital_syncport_unsubscribe")]
        public static extern tobii_error_t tobii_digital_syncport_unsubscribe(IntPtr device);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_enumerate_face_types")]
        private static extern tobii_error_t tobii_enumerate_face_types_internal(IntPtr device, tobii_face_type_receiver_t receiver, IntPtr user_data);

        public static tobii_error_t tobii_enumerate_face_types(IntPtr device, out List<string> face_types)
        {
            var types = new List<string>();
            tobii_face_type_receiver_t handler = (face_type, data) => { types.Add(face_type); };
            var result = tobii_enumerate_face_types_internal(device, handler, IntPtr.Zero);

            face_types = types;

            return result;
        }

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_set_face_type")]
        public static extern tobii_error_t tobii_set_face_type(IntPtr device, string face_type);

        [DllImport(stream_engine_dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tobii_get_face_type")]
        private static extern tobii_error_t tobii_get_face_type(IntPtr device, StringBuilder face_type);

        public static tobii_error_t tobii_get_face_type(IntPtr device, out string face_type)
        {
            var val = new StringBuilder(64);
            var result = tobii_get_face_type(device, val);
            face_type = val.ToString();
            return result;
        }

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_version_t
    {
        public int major;
        public int minor;
        public int revision;
        public int build;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct tobii_device_info_t
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string serial_number;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string model;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string generation;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string firmware_version;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string integration_id;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string hw_calibration_version;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string hw_calibration_date;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string lot_id;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string integration_type;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string runtime_build_version;

    }

    [StructLayout(LayoutKind.Sequential)]
    public class tobii_custom_log_t
    {
        public IntPtr log_context;
        public Interop.tobii_log_func_t log_func;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_data_receiver_t(IntPtr data, IntPtr size, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_device_url_receiver_t(string url, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_face_type_receiver_t(string face_type, IntPtr user_data);

    public enum tobii_feature_group_t
    {
        TOBII_FEATURE_GROUP_BLOCKED,
        TOBII_FEATURE_GROUP_CONSUMER,
        TOBII_FEATURE_GROUP_CONFIG,
        TOBII_FEATURE_GROUP_PROFESSIONAL,
        TOBII_FEATURE_GROUP_INTERNAL,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_license_key_t
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string license_key;

        public IntPtr size_in_bytes;
    }

    public enum tobii_license_validation_result_t
    {
        TOBII_LICENSE_VALIDATION_RESULT_OK,
        TOBII_LICENSE_VALIDATION_RESULT_TAMPERED,
        TOBII_LICENSE_VALIDATION_RESULT_INVALID_APPLICATION_SIGNATURE,
        TOBII_LICENSE_VALIDATION_RESULT_NONSIGNED_APPLICATION,
        TOBII_LICENSE_VALIDATION_RESULT_EXPIRED,
        TOBII_LICENSE_VALIDATION_RESULT_PREMATURE,
        TOBII_LICENSE_VALIDATION_RESULT_INVALID_PROCESS_NAME,
        TOBII_LICENSE_VALIDATION_RESULT_INVALID_SERIAL_NUMBER,
        TOBII_LICENSE_VALIDATION_RESULT_INVALID_MODEL,
        TOBII_LICENSE_VALIDATION_RESULT_INVALID_PLATFORM_TYPE,
        TOBII_LICENSE_VALIDATION_RESULT_REVOKED,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TobiiVector2
    {
        public float x;
        public float y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TobiiVector3
    {
        public float x;
        public float y;
        public float z;
    }

    public enum tobii_validity_t
    {
        TOBII_VALIDITY_INVALID = 0,
        TOBII_VALIDITY_VALID = 1
    }

    public enum tobii_error_t
    {
        TOBII_ERROR_NO_ERROR,
        TOBII_ERROR_INTERNAL,
        TOBII_ERROR_INSUFFICIENT_LICENSE,
        TOBII_ERROR_NOT_SUPPORTED,
        TOBII_ERROR_NOT_AVAILABLE,
        TOBII_ERROR_CONNECTION_FAILED,
        TOBII_ERROR_TIMED_OUT,
        TOBII_ERROR_ALLOCATION_FAILED,
        TOBII_ERROR_INVALID_PARAMETER,
        TOBII_ERROR_CALIBRATION_ALREADY_STARTED,
        TOBII_ERROR_CALIBRATION_NOT_STARTED,
        TOBII_ERROR_ALREADY_SUBSCRIBED,
        TOBII_ERROR_NOT_SUBSCRIBED,
        TOBII_ERROR_OPERATION_FAILED,
        TOBII_ERROR_CONFLICTING_API_INSTANCES,
        TOBII_ERROR_CALIBRATION_BUSY,
        TOBII_ERROR_CALLBACK_IN_PROGRESS,
        TOBII_ERROR_TOO_MANY_SUBSCRIBERS,
        TOBII_ERROR_CONNECTION_FAILED_DRIVER,
        TOBII_ERROR_UNAUTHORIZED,
        TOBII_ERROR_FIRMWARE_UPGRADE_IN_PROGRESS
    }

    public enum tobii_log_level_t
    {
        TOBII_LOG_LEVEL_ERROR,
        TOBII_LOG_LEVEL_WARN,
        TOBII_LOG_LEVEL_INFO,
        TOBII_LOG_LEVEL_DEBUG,
        TOBII_LOG_LEVEL_TRACE,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_notifications_callback_t(ref tobii_notification_t notification, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_gaze_point_callback_t(ref tobii_gaze_point_t gaze_point, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_gaze_origin_callback_t(ref tobii_gaze_origin_t gaze_origin, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_eye_position_normalized_callback_t(ref tobii_eye_position_normalized_t eye_position, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_user_presence_callback_t(tobii_user_presence_status_t status, long timestamp_us, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_responsive_gaze_point_callback_t(ref tobii_responsive_gaze_point_t gaze_point, IntPtr user_data);

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_gaze_point_t
    {
        public long timestamp_us;
        public tobii_validity_t validity;
        public TobiiVector2 position;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_track_box_t
    {
        public TobiiVector3 front_upper_right_xyz;
        public TobiiVector3 front_upper_left_xyz;
        public TobiiVector3 front_lower_left_xyz;
        public TobiiVector3 front_lower_right_xyz;
        public TobiiVector3 back_upper_right_xyz;
        public TobiiVector3 back_upper_left_xyz;
        public TobiiVector3 back_lower_left_xyz;
        public TobiiVector3 back_lower_right_xyz;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_gaze_origin_t
    {
        public long timestamp_us;
        public tobii_validity_t left_validity;
        public TobiiVector3 left;
        public tobii_validity_t right_validity;
        public TobiiVector3 right;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_eye_position_normalized_t
    {
        public long timestamp_us;
        public tobii_validity_t left_validity;
        public TobiiVector3 left;
        public tobii_validity_t right_validity;
        public TobiiVector3 right;
    }

    public enum tobii_user_presence_status_t
    {
        TOBII_USER_PRESENCE_STATUS_UNKNOWN,
        TOBII_USER_PRESENCE_STATUS_AWAY,
        TOBII_USER_PRESENCE_STATUS_PRESENT,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_responsive_gaze_point_t
    {
        public long timestamp_us;
        public tobii_validity_t validity;
        public TobiiVector2 position;
    }

    public enum tobii_notification_type_t
    {
        TOBII_NOTIFICATION_TYPE_CALIBRATION_STATE_CHANGED,
        TOBII_NOTIFICATION_TYPE_EXCLUSIVE_MODE_STATE_CHANGED,
        TOBII_NOTIFICATION_TYPE_TRACK_BOX_CHANGED,
        TOBII_NOTIFICATION_TYPE_DISPLAY_AREA_CHANGED,
        TOBII_NOTIFICATION_TYPE_FRAMERATE_CHANGED,
        TOBII_NOTIFICATION_TYPE_POWER_SAVE_STATE_CHANGED,
        TOBII_NOTIFICATION_TYPE_DEVICE_PAUSED_STATE_CHANGED,
        TOBII_NOTIFICATION_TYPE_CALIBRATION_ENABLED_EYE_CHANGED,
        TOBII_NOTIFICATION_TYPE_CALIBRATION_ID_CHANGED,
        TOBII_NOTIFICATION_TYPE_COMBINED_GAZE_FACTOR_CHANGED,
        TOBII_NOTIFICATION_TYPE_FAULTS_CHANGED,
        TOBII_NOTIFICATION_TYPE_WARNINGS_CHANGED,
        TOBII_NOTIFICATION_TYPE_FACE_TYPE_CHANGED,
        TOBII_NOTIFICATION_TYPE_CALIBRATION_ACTIVE_CHANGED,
    }

    public enum tobii_notification_value_type_t
    {
        TOBII_NOTIFICATION_VALUE_TYPE_NONE,
        TOBII_NOTIFICATION_VALUE_TYPE_FLOAT,
        TOBII_NOTIFICATION_VALUE_TYPE_STATE,
        TOBII_NOTIFICATION_VALUE_TYPE_DISPLAY_AREA,
        TOBII_NOTIFICATION_VALUE_TYPE_UINT,
        TOBII_NOTIFICATION_VALUE_TYPE_ENABLED_EYE,
        TOBII_NOTIFICATION_VALUE_TYPE_STRING,
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct tobii_notification_value_t
    {
        [FieldOffset(0)]
        public float float_;
        [FieldOffset(0)]
        public tobii_state_bool_t state;
        [FieldOffset(0)]
        public tobii_display_area_t display_area;
        [FieldOffset(0)]
        public uint uint_;
        [FieldOffset(0)]
        public tobii_enabled_eye_t enabled_eye;
        // TODO: Overlapping an object field with a non-object field i.e string and integer,
        // will generate a runtime error. A re-design of this union is probably needed in order
        // for it to work in the .NET bindings.
        // WORK AROUND: When a notification containing a string is received, it needs to be
        // queried through the tobii_get_state_string function.
        //[FieldOffset(0), MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        //public string string_;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_notification_t
    {
        public tobii_notification_type_t type;
        public tobii_notification_value_type_t value_type;
        public tobii_notification_value_t value;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_wearable_consumer_data_callback_t(ref tobii_wearable_consumer_data_t data, IntPtr user_data);

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_wearable_consumer_eye_t
    {
        public tobii_validity_t pupil_position_in_sensor_area_validity;
        public TobiiVector2 pupil_position_in_sensor_area_xy;

        public tobii_validity_t position_guide_validity;
        public TobiiVector2 position_guide_xy;

        public tobii_validity_t blink_validity;
        public tobii_state_bool_t blink;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_wearable_consumer_data_t
    {
        public long timestamp_us;
        public tobii_wearable_consumer_eye_t left;
        public tobii_wearable_consumer_eye_t right;

        public tobii_validity_t gaze_origin_combined_validity;
        public TobiiVector3 gaze_origin_combined_mm_xyz;
        public tobii_validity_t gaze_direction_combined_validity;
        public TobiiVector3 gaze_direction_combined_normalized_xyz;
        public tobii_validity_t convergence_distance_validity;
        public float convergence_distance_mm;
        public tobii_state_bool_t improve_user_position_hmd;
        public tobii_state_bool_t increase_eye_relief;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_wearable_advanced_data_callback_t(ref tobii_wearable_advanced_data_t data, IntPtr user_data);

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_wearable_advanced_eye_t
    {
        public tobii_validity_t gaze_origin_validity;
        public TobiiVector3 gaze_origin_mm_xyz;

        public tobii_validity_t gaze_direction_validity;
        public TobiiVector3 gaze_direction_normalized_xyz;

        public tobii_validity_t pupil_diameter_validity;
        public float pupil_diameter_mm;

        public tobii_validity_t pupil_position_in_sensor_area_validity;
        public TobiiVector2 pupil_position_in_sensor_area_xy;

        public tobii_validity_t position_guide_validity;
        public TobiiVector2 position_guide_xy;

        public tobii_validity_t blink_validity;
        public tobii_state_bool_t blink;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_wearable_advanced_data_t
    {
        public long timestamp_tracker_us;
        public long timestamp_system_us;

        public tobii_wearable_advanced_eye_t left;
        public tobii_wearable_advanced_eye_t right;

        public tobii_validity_t gaze_origin_combined_validity;
        public TobiiVector3 gaze_origin_combined_mm_xyz;
        public tobii_validity_t gaze_direction_combined_validity;
        public TobiiVector3 gaze_direction_combined_normalized_xyz;
        public tobii_validity_t convergence_distance_validity;
        public float convergence_distance_mm;
        public tobii_state_bool_t improve_user_position_hmd;
        public tobii_state_bool_t increase_eye_relief;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_lens_configuration_t
    {
        public TobiiVector3 left_xyz;
        public TobiiVector3 right_xyz;
    }

    public enum tobii_lens_configuration_writable_t
    {
        TOBII_LENS_CONFIGURATION_NOT_WRITABLE,
        TOBII_LENS_CONFIGURATION_WRITABLE,
    }

    public enum tobii_wearable_foveated_tracking_state_t
    {
        TOBII_WEARABLE_FOVEATED_TRACKING_STATE_TRACKING = 0,
        TOBII_WEARABLE_FOVEATED_TRACKING_STATE_EXTRAPOLATED = 1,
        TOBII_WEARABLE_FOVEATED_TRACKING_STATE_LAST_KNOWN = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_wearable_foveated_gaze_t
    {
        public long timestamp_us;
        public tobii_wearable_foveated_tracking_state_t tracking_state;
        public TobiiVector3 gaze_direction_combined_normalized_xyz;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_wearable_foveated_gaze_callback_t(ref tobii_wearable_foveated_gaze_t data, IntPtr user_data);

    public enum tobii_capability_t
    {
        TOBII_CAPABILITY_DISPLAY_AREA_WRITABLE,
        TOBII_CAPABILITY_CALIBRATION_2D,
        TOBII_CAPABILITY_CALIBRATION_3D,
        TOBII_CAPABILITY_PERSISTENT_STORAGE,
        TOBII_CAPABILITY_CALIBRATION_PER_EYE,
        TOBII_CAPABILITY_COMPOUND_STREAM_WEARABLE_3D_GAZE_COMBINED,
        TOBII_CAPABILITY_FACE_TYPE,
        TOBII_CAPABILITY_COMPOUND_STREAM_USER_POSITION_GUIDE_XY,
        TOBII_CAPABILITY_COMPOUND_STREAM_USER_POSITION_GUIDE_Z,
        TOBII_CAPABILITY_COMPOUND_STREAM_WEARABLE_LIMITED_IMAGE,
        TOBII_CAPABILITY_COMPOUND_STREAM_WEARABLE_PUPIL_DIAMETER,
        TOBII_CAPABILITY_COMPOUND_STREAM_WEARABLE_PUPIL_POSITION,
        TOBII_CAPABILITY_COMPOUND_STREAM_WEARABLE_EYE_OPENNESS,
        TOBII_CAPABILITY_COMPOUND_STREAM_WEARABLE_3D_GAZE_PER_EYE,
        TOBII_CAPABILITY_COMPOUND_STREAM_WEARABLE_USER_POSITION_GUIDE_XY,
        TOBII_CAPABILITY_COMPOUND_STREAM_WEARABLE_TRACKING_IMPROVEMENTS,
        TOBII_CAPABILITY_COMPOUND_STREAM_WEARABLE_CONVERGENCE_DISTANCE,
        TOBII_CAPABILITY_COMPOUND_STREAM_WEARABLE_IMPROVE_USER_POSITION_HMD,
        TOBII_CAPABILITY_COMPOUND_STREAM_WEARABLE_INCREASE_EYE_RELIEF,
    }

    public enum tobii_stream_t
    {
        TOBII_STREAM_GAZE_POINT,
        TOBII_STREAM_GAZE_ORIGIN,
        TOBII_STREAM_EYE_POSITION_NORMALIZED,
        TOBII_STREAM_USER_PRESENCE,
        TOBII_STREAM_HEAD_POSE,
        TOBII_STREAM_GAZE_DATA,
        TOBII_STREAM_DIGITAL_SYNCPORT,
        TOBII_STREAM_DIAGNOSTICS_IMAGE,
        TOBII_STREAM_USER_POSITION_GUIDE,
        TOBII_STREAM_WEARABLE_CONSUMER,
        TOBII_STREAM_WEARABLE_ADVANCED,
        TOBII_STREAM_WEARABLE_FOVEATED_GAZE,
        TOBII_STREAM_RESPONSIVE_GAZE_POINT,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_output_frequency_receiver_t(float output_frequency, IntPtr user_data);

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_display_area_t
    {
        public TobiiVector3 top_left_mm_xyz;
        public TobiiVector3 top_right_mm_xyz;
        public TobiiVector3 bottom_left_mm_xyz;
    }

    public enum tobii_enabled_eye_t
    {
        TOBII_ENABLED_EYE_LEFT,
        TOBII_ENABLED_EYE_RIGHT,
        TOBII_ENABLED_EYE_BOTH,
    }

    public enum tobii_calibration_point_status_t
    {
        TOBII_CALIBRATION_POINT_STATUS_FAILED_OR_INVALID,
        TOBII_CALIBRATION_POINT_STATUS_VALID_BUT_NOT_USED_IN_CALIBRATION,
        TOBII_CALIBRATION_POINT_STATUS_VALID_AND_USED_IN_CALIBRATION,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_calibration_point_data_t
    {
        public TobiiVector2 point_xy;

        public tobii_calibration_point_status_t left_status;
        public TobiiVector2 left_mapping_xy;

        public tobii_calibration_point_status_t right_status;
        public TobiiVector2 right_mapping_xy;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_geometry_mounting_t
    {
        public int guides;
        public float width_mm;
        public float angle_deg;

        public TobiiVector3 external_offset_mm_xyz;
        public TobiiVector3 internal_offset_mm_xyz;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_gaze_data_eye_t
    {
        public tobii_validity_t gaze_origin_validity;
        public TobiiVector3 gaze_origin_from_eye_tracker_mm_xyz;

        public tobii_validity_t eye_position_validity;
        public TobiiVector3 eye_position_in_track_box_normalized_xyz;

        public tobii_validity_t gaze_point_validity;
        public TobiiVector3 gaze_point_from_eye_tracker_mm_xyz;
        public TobiiVector2 gaze_point_on_display_normalized_xy;

        public tobii_validity_t eyeball_center_validity;
        public TobiiVector3 eyeball_center_from_eye_tracker_mm_xyz;

        public tobii_validity_t pupil_validity;
        public float pupil_diameter_mm;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_gaze_data_t
    {
        public long timestamp_tracker_us;
        public long timestamp_system_us;
        public tobii_gaze_data_eye_t left;
        public tobii_gaze_data_eye_t right;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_gaze_data_callback_t(ref tobii_gaze_data_t gaze_data, IntPtr user_data);

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_timesync_data_t
    {
        public long system_start_us;
        public long system_end_us;
        public long tracker_us;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_illumination_mode_receiver_t(string illumination_mode);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_digital_syncport_callback_t(uint signal, long timestamp_tracker_us, long timestamp_system_us, IntPtr user_data);

    public enum tobii_state_t
    {
        TOBII_STATE_POWER_SAVE_ACTIVE,
        TOBII_STATE_REMOTE_WAKE_ACTIVE,
        TOBII_STATE_DEVICE_PAUSED,
        TOBII_STATE_EXCLUSIVE_MODE,
        TOBII_STATE_FAULT,
        TOBII_STATE_WARNING,
        TOBII_STATE_CALIBRATION_ID,
        TOBII_STATE_CALIBRATION_ACTIVE,
    }

    public enum tobii_state_bool_t
    {
        TOBII_STATE_BOOL_FALSE,
        TOBII_STATE_BOOL_TRUE,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_head_pose_callback_t(ref tobii_head_pose_t head_pose, IntPtr user_data);

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_head_pose_t
    {
        public long timestamp_us;
        public tobii_validity_t position_validity;
        public TobiiVector3 position_xyz;

        public tobii_validity_t rotation_x_validity;
        public tobii_validity_t rotation_y_validity;
        public tobii_validity_t rotation_z_validity;
        public TobiiVector3 rotation_xyz;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_device_list_change_callback_t(string url, tobii_device_list_change_type_t type,
        tobii_device_readiness_t readiness, long timestamp_us);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_enumerated_device_receiver_t(ref tobii_enumerated_device_t enumerated_device, IntPtr user_data);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct tobii_enumerated_device_t
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string url;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string serial_number;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string model;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string generation;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string firmware_version;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 120)] public string integration;
        public tobii_device_readiness_t readiness;
    }

    public enum tobii_device_readiness_t
    {
        TOBII_DEVICE_READINESS_WAITING_FOR_FIRMWARE_UPGRADE,
        TOBII_DEVICE_READINESS_UPGRADING_FIRMWARE,
        TOBII_DEVICE_READINESS_WAITING_FOR_DISPLAY_AREA,
        TOBII_DEVICE_READINESS_WAITING_FOR_CALIBRATION,
        TOBII_DEVICE_READINESS_CALIBRATING,
        TOBII_DEVICE_READINESS_READY,
        TOBII_DEVICE_READINESS_PAUSED,
        TOBII_DEVICE_READINESS_MALFUNCTIONING
    }

    public enum tobii_device_list_change_type_t
    {
        TOBII_DEVICE_LIST_CHANGE_TYPE_ADDED,
        TOBII_DEVICE_LIST_CHANGE_TYPE_REMOVED,
        TOBII_DEVICE_LIST_CHANGE_TYPE_CHANGED
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_user_position_guide_t
    {
        public long timestamp_us;
        public tobii_validity_t left_position_validity;
        public TobiiVector3 left_position_normalized_xyz;
        public tobii_validity_t right_position_validity;
        public TobiiVector3 right_position_normalized_xyz;
    }

    public enum tobii_calibration_stimulus_point_status_t
    {
        TOBII_CALIBRATION_STIMULUS_POINT_STATUS_FAILED_OR_INVALID,
        TOBII_CALIBRATION_STIMULUS_POINT_STATUS_VALID_NOT_USED,
        TOBII_CALIBRATION_STIMULUS_POINT_STATUS_VALID_USED,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_calibration_stimulus_point_data_t
    {
        public TobiiVector3  point_xyz;
        public tobii_calibration_stimulus_point_status_t left_status;
        public float left_bias;
        public float left_precision;
        public tobii_calibration_stimulus_point_status_t right_status;
        public float right_bias;
        public float right_precision;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tobii_calibration_stimulus_points_t
    {
        public int stimulus_point_count;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public tobii_calibration_stimulus_point_data_t[] stimulus_points;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void tobii_user_position_guide_callback_t(ref tobii_user_position_guide_t data, IntPtr user_data);
}
