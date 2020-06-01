/*
 * unity-websocket-webgl
 * 
 * @author Jiri Hybek <jiri@hybek.cz>
 * @copyright 2018 Jiri Hybek <jiri@hybek.cz>
 * @license Apache 2.0 - See LICENSE file distributed with this source code.
 */

/*
 * @edited by Khiem (Kei) Nguyen.
 * Splitted to single files and support sending string type message.
 */

namespace HybridWebSocket
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System;
    using AOT;
    using UnityEngine;

    /// <summary>
    /// Class providing static access methods to work with JSLIB WebSocket or WebSocketSharp interface
    /// </summary>
    public static class WebSocketFactory
    {
        /* Map of websocket instances */
        private static Dictionary<Int32, WebSocketWebGL> instances = new Dictionary<Int32, WebSocketWebGL> ();

        /* Delegates */
        public delegate void OnOpenCallback (int instanceId);
        public delegate void OnMessageCallback (int instanceId, System.IntPtr msgPtr, int msgSize);
        public delegate void OnErrorCallback (int instanceId, System.IntPtr errorPtr);
        public delegate void OnCloseCallback (int instanceId, int closeCode);

        /* WebSocket JSLIB callback setters and other functions */
        [DllImport ("__Internal")]
        public static extern int WebSocketAllocate (string url);

        [DllImport ("__Internal")]
        public static extern void WebSocketFree (int instanceId);

        [DllImport ("__Internal")]
        public static extern void WebSocketSetOnOpen (OnOpenCallback callback);

        [DllImport ("__Internal")]
        public static extern void WebSocketSetOnMessage (OnMessageCallback callback);

        [DllImport ("__Internal")]
        public static extern void WebSocketSetOnError (OnErrorCallback callback);

        [DllImport ("__Internal")]
        public static extern void WebSocketSetOnClose (OnCloseCallback callback);

        /* If callbacks was initialized and set */
        private static bool isInitialized = false;

        /*
         * Initialize WebSocket callbacks to JSLIB
         */
        private static void Initialize ()
        {
            WebSocketSetOnOpen (DelegateOnOpenEvent);
            WebSocketSetOnMessage (DelegateOnMessageEvent);
            WebSocketSetOnError (DelegateOnErrorEvent);
            WebSocketSetOnClose (DelegateOnCloseEvent);

            isInitialized = true;
        }

        /// <summary>
        /// Called when instance is destroyed (by destructor)
        /// Method removes instance from map and free it in JSLIB implementation
        /// </summary>
        /// <param name="instanceId">Instance identifier.</param>
        public static void HandleInstanceDestroy (Int32 instanceId)
        {
            instances.Remove (instanceId);
            WebSocketFree (instanceId);
        }

        [MonoPInvokeCallback (typeof (OnOpenCallback))]
        public static void DelegateOnOpenEvent (Int32 instanceId)
        {
            WebSocketWebGL instanceRef;
            if (instances.TryGetValue (instanceId, out instanceRef))
            {
                instanceRef.DelegateOnOpenEvent ();
            }
        }

        [MonoPInvokeCallback (typeof (OnMessageCallback))]
        public static void DelegateOnMessageEvent (Int32 instanceId, System.IntPtr msgPtr, int msgSize)
        {
            WebSocketWebGL instanceRef;
            if (instances.TryGetValue (instanceId, out instanceRef))
            {
                var msg = new byte[msgSize];
                Marshal.Copy (msgPtr, msg, 0, msgSize);
                instanceRef.DelegateOnMessageEvent (msg);
            }
        }

        [MonoPInvokeCallback (typeof (OnErrorCallback))]
        public static void DelegateOnErrorEvent (int instanceId, System.IntPtr errorPtr)
        {
            WebSocketWebGL instanceRef;
            if (instances.TryGetValue (instanceId, out instanceRef))
            {
                var errorMsg = Marshal.PtrToStringAuto (errorPtr);
                instanceRef.DelegateOnErrorEvent (errorMsg);
            }
        }

        [MonoPInvokeCallback (typeof (OnCloseCallback))]
        public static void DelegateOnCloseEvent (int instanceId, int closeCode)
        {
            WebSocketWebGL instanceRef;
            if (instances.TryGetValue (instanceId, out instanceRef))
            {
                instanceRef.DelegateOnCloseEvent (closeCode);
            }
        }

        /// <summary>
        /// Create WebSocket client instance
        /// </summary>
        /// <returns>The WebSocket instance.</returns>
        /// <param name="url">WebSocket valid URL.</param>
        public static IWebSocket CreateInstance (string url)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!isInitialized)
            {
                Initialize ();
            }
            int instanceId = WebSocketAllocate (url);
            var wrapper = new WebSocketWebGL (instanceId);
            instances.Add (instanceId, wrapper);
            return wrapper;
#else
            return new WebSocket (url);
#endif
        }
    }
}
