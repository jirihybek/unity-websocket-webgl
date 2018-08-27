/*
 * unity-websocket-webgl
 * 
 * @author Jiri Hybek <jiri@hybek.cz>
 * @copyright 2018 Jiri Hybek <jiri@hybek.cz>
 * @license Apache 2.0 - See LICENSE file distributed with this source code.
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace HybridWebSocket
{

    /* WebSocket event delegates */
    public delegate void WebSocketOnOpen();
    public delegate void WebSocketOnMessage(byte[] data);
    public delegate void WebSocketOnError(string errorMsg);
    public delegate void WebSocketOnClose(WebSocketCloseCode closeCode);

    /*
     * WebSocket state enum
     */
    public enum WebSocketState
    {
        Connecting,
        Open,
        Closing,
        Closed
    }

    /*
     * Enum of WebSocket close status codes
     */
    public enum WebSocketCloseCode
    {
        /* Do NOT use NotSet - it's only purpose is to indicate that the close code cannot be parsed. */
        NotSet = 0,
        Normal = 1000,
        Away = 1001,
        ProtocolError = 1002,
        UnsupportedData = 1003,
        Undefined = 1004,
        NoStatus = 1005,
        Abnormal = 1006,
        InvalidData = 1007,
        PolicyViolation = 1008,
        TooBig = 1009,
        MandatoryExtension = 1010,
        ServerError = 1011,
        TlsHandshakeFailure = 1015
    }

    /*
     * WebSocket generic interface
     */
    public interface IWebSocket
    {
        /* Open connection */
        void Connect();

        /* Close connection */
        void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null);

        /* Send binary data over the socket */
        void Send(byte[] data);

        /* Return WebSocket state number */
        WebSocketState GetState();

        /* Events */
        event WebSocketOnOpen OnOpen;
        event WebSocketOnMessage OnMessage;
        event WebSocketOnError OnError;
        event WebSocketOnClose OnClose;
    }

    /*
     * Various helpers to work mainly with enums
     */
    public static class WebSocketHelpers
    {

        /*
         * Safely parse close code enum from int value
         */
        public static WebSocketCloseCode ParseCloseCodeEnum(int closeCode)
        {

            if (WebSocketCloseCode.IsDefined(typeof(WebSocketCloseCode), closeCode))
            {
                return (WebSocketCloseCode)closeCode;
            }
            else
            {
                return WebSocketCloseCode.Undefined;
            }

        }

        /*
         * Return error message based on int code
         * 
         * Used for resolving JSLIB errors to meaninfull messages.
         */
        public static string GetErrorMessageFromCode(int errorCode)
        {

            switch(errorCode)
            {

                case -1: return "WebSocket instance not found.";
                case -2: return "WebSocket is already connected or in connecting state.";
                case -3: return "WebSocket is not connected.";
                case -4: return "WebSocket is already closing.";
                case -5: return "WebSocket is already closed.";
                case -6: return "WebSocket is not in open state.";
                case -7: return "Cannot close WebSocket. An invalid code was specified or reason is too long.";
                default: return "Unknown error.";

            }

        }

    }

    /*
     * Generic WebSocket exception class
     */
    public class WebSocketException : Exception
    {

        public WebSocketException()
        {
        }

        public WebSocketException(string message)
            : base(message)
        {
        }

        public WebSocketException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }

#if UNITY_WEBGL && !UNITY_EDITOR
/*
 * WebSocket class bound to JSLIB
 */
    public class WebSocket: IWebSocket
    {

        /* WebSocket JSLIB functions */
        [DllImport("__Internal")]
        public static extern int WebSocketConnect(int instanceId);

        [DllImport("__Internal")]
        public static extern int WebSocketClose(int instanceId, int code, string reason);

        [DllImport("__Internal")]
        public static extern int WebSocketSend(int instanceId, byte[] dataPtr, int dataLength);

        [DllImport("__Internal")]
        public static extern int WebSocketGetState(int instanceId);

        /* Socket server URL */
        protected int instanceId;

        /* Events */
        public event WebSocketOnOpen OnOpen;
        public event WebSocketOnMessage OnMessage;
        public event WebSocketOnError OnError;
        public event WebSocketOnClose OnClose;

        /*
         * Constructor - receive JSLIB instance id of allocated socket
         */
        public WebSocket(int instanceId)
        {

            this.instanceId = instanceId;

        }

        /*
         * Destructor - notifies WebSocketFactory about it to remove JSLIB references
         */
        ~WebSocket()
        {
            WebSocketFactory.HandleInstanceDestroy(this.instanceId);
        }

        /*
         * Return JSLIB instance ID
         */
        public int GetInstanceId()
        {

            return this.instanceId;

        }

        /*
         * Connect to the server
         */
        public void Connect()
        {

            int ret = WebSocketConnect(this.instanceId);

            if (ret < 0)
                throw new WebSocketException(
                    WebSocketHelpers.GetErrorMessageFromCode(ret)
                );

        }

        /*
         * Close connection
         */
        public void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {

            int ret = WebSocketClose(this.instanceId, (int)code, reason);

            if (ret < 0)
                throw new WebSocketException(
                    WebSocketHelpers.GetErrorMessageFromCode(ret)
                );

        }

        /*
         * Send data over the socket
         */
        public void Send(byte[] data)
        {

            int ret = WebSocketSend(this.instanceId, data, data.Length);

            if (ret < 0)
                throw new WebSocketException(
                    WebSocketHelpers.GetErrorMessageFromCode(ret)
                );

        }

        /*
         * Return WebSocket connection state
         */
        public WebSocketState GetState()
        {

            int state = WebSocketGetState(this.instanceId);

            if (state < 0)
                throw new WebSocketException(
                    WebSocketHelpers.GetErrorMessageFromCode(state)
                );

            switch (state)
            {
                case 0:
                    return WebSocketState.Connecting;

                case 1:
                    return WebSocketState.Open;

                case 2:
                    return WebSocketState.Closing;

                case 3:
                    return WebSocketState.Closed;

                default:
                    return WebSocketState.Closed;
            }

        }

        /*
         * Delegate onOpen event from JSLIB to native sharp event
         * Is called by WebSocketFactory
         */
        public void DelegateOnOpenEvent()
        {

            this.OnOpen?.Invoke();

        }

        /*
         * Delegate onMessage event from JSLIB to native sharp event
         * Is called by WebSocketFactory
         */
        public void DelegateOnMessageEvent(byte[] data)
        {

            this.OnMessage?.Invoke(data);

        }

        /*
         * Delegate onError event from JSLIB to native sharp event
         * Is called by WebSocketFactory
         */
        public void DelegateOnErrorEvent(string errorMsg)
        {

            this.OnError?.Invoke(errorMsg);

        }

        /*
         * Delegate onClose event from JSLIB to native sharp event
         * Is called by WebSocketFactory
         */
        public void DelegateOnCloseEvent(int closeCode)
        {

            this.OnClose?.Invoke(WebSocketHelpers.ParseCloseCodeEnum(closeCode));

        }

    }
#else
    public class WebSocket : IWebSocket
    {
        /* Events */
        public event WebSocketOnOpen OnOpen;
        public event WebSocketOnMessage OnMessage;
        public event WebSocketOnError OnError;
        public event WebSocketOnClose OnClose;

        /* WebSocket instance */
        protected WebSocketSharp.WebSocket ws;

        /*
         * WebSocket constructor
         */
        public WebSocket(string url)
        {

            try
            {
                   
                // Create WebSocket instance
                this.ws = new WebSocketSharp.WebSocket(url);

                // Bind OnOpen event
                this.ws.OnOpen += (sender, ev) =>
                {
                    this.OnOpen?.Invoke();
                };

                // Bind OnMessage event
                this.ws.OnMessage += (sender, ev) =>
                {
                    if (ev.RawData != null)
                        this.OnMessage?.Invoke(ev.RawData);
                };

                // Bind OnError event
                this.ws.OnError += (sender, ev) =>
                {
                    this.OnError?.Invoke(ev.Message);
                };

                // Bind OnClose event
                this.ws.OnClose += (sender, ev) =>
                {
                    this.OnClose?.Invoke(
                        WebSocketHelpers.ParseCloseCodeEnum( (int)ev.Code )
                    );
                };

            }
            catch (Exception e)
            {

                throw new WebSocketException("Failed to create WebSocket Client.", e);

            }

        }

        /*
         * Connect to the server
         */
        public void Connect()
        {

            // Check state
            if (this.ws.ReadyState == WebSocketSharp.WebSocketState.Open || this.ws.ReadyState == WebSocketSharp.WebSocketState.Closing)
                throw new WebSocketException("WebSocket is already connected or is closing.");

            try
            {
                this.ws.ConnectAsync();
            }
            catch (Exception e)
            {
                throw new WebSocketException("Failed to connect.", e);
            }

        }

        /*
         * Close connection
         */
        public void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {

            // Check state
            if (this.ws.ReadyState != WebSocketSharp.WebSocketState.Closing)
                throw new WebSocketException("WebSocket is already closing.");

            if (this.ws.ReadyState != WebSocketSharp.WebSocketState.Closed)
                throw new WebSocketException("WebSocket is already closed.");

            try
            {
                this.ws.CloseAsync((ushort)code, reason);
            }
            catch (Exception e)
            {
                throw new WebSocketException("Failed to close the connection.", e);
            }

        }

        /*
         * Send data over the socket
         */
        public void Send(byte[] data)
        {

            // Check state
            if (this.ws.ReadyState != WebSocketSharp.WebSocketState.Open)
                throw new WebSocketException("WebSocket is not in open state.");

            try
            {
                this.ws.Send(data);
            }
            catch (Exception e)
            {
                throw new WebSocketException("Failed to send message.", e);
            }

        }

        /*
         * Return WebSocket connection state
         */
        public WebSocketState GetState()
        {

            switch (this.ws.ReadyState)
            {
                case WebSocketSharp.WebSocketState.Connecting:
                    return WebSocketState.Connecting;

                case WebSocketSharp.WebSocketState.Open:
                    return WebSocketState.Open;

                case WebSocketSharp.WebSocketState.Closing:
                    return WebSocketState.Closing;

                case WebSocketSharp.WebSocketState.Closed:
                    return WebSocketState.Closed;

                default:
                    return WebSocketState.Closed;
            }

        }

    }
#endif

    /*
     * Class providing static access methods to work with JSLIB WebSocket interface
     */
    public static class WebSocketFactory
    {

#if UNITY_WEBGL && !UNITY_EDITOR
        /* Map of websocket instances */
        private static Dictionary<Int32, WebSocket> instances = new Dictionary<Int32, WebSocket>();

        /* Delegates */
        public delegate void OnOpenCallback(int instanceId);
        public delegate void OnMessageCallback(int instanceId, System.IntPtr msgPtr, int msgSize);
        public delegate void OnErrorCallback(int instanceId, System.IntPtr errorPtr);
        public delegate void OnCloseCallback(int instanceId, int closeCode);

        /* WebSocket JSLIB callback setters and other functions */
        [DllImport("__Internal")]
        public static extern int WebSocketAllocate(string url);

        [DllImport("__Internal")]
        public static extern void WebSocketFree(int instanceId);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnOpen(OnOpenCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnMessage(OnMessageCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnError(OnErrorCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnClose(OnCloseCallback callback);

        /* If callbacks was initialized and set */
        private static bool isInitialized = false;

        /*
         * Initialize WebSocket callbacks to JSLIB
         */
        private static void Initialize()
        {

            WebSocketSetOnOpen(DelegateOnOpenEvent);
            WebSocketSetOnMessage(DelegateOnMessageEvent);
            WebSocketSetOnError(DelegateOnErrorEvent);
            WebSocketSetOnClose(DelegateOnCloseEvent);

            isInitialized = true;

        }

        /*
         * Called when instance is destroyed (by destructor)
         * Function removes instance from map and free it in JSLIB implementation
         */
        public static void HandleInstanceDestroy(int instanceId)
        {

            instances.Remove(instanceId);
            WebSocketFree(instanceId);

        }

        [MonoPInvokeCallback(typeof(OnOpenCallback))]
        public static void DelegateOnOpenEvent(int instanceId)
        {

            WebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {
                instanceRef.DelegateOnOpenEvent();
            }

        }

        [MonoPInvokeCallback(typeof(OnMessageCallback))]
        public static void DelegateOnMessageEvent(int instanceId, System.IntPtr msgPtr, int msgSize)
        {

            WebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {
                byte[] msg = new byte[msgSize];
                Marshal.Copy(msgPtr, msg, 0, msgSize);

                instanceRef.DelegateOnMessageEvent(msg);
            }

        }

        [MonoPInvokeCallback(typeof(OnErrorCallback))]
        public static void DelegateOnErrorEvent(int instanceId, System.IntPtr errorPtr)
        {

            WebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {

                string errorMsg = Marshal.PtrToStringAuto(errorPtr);
                instanceRef.DelegateOnErrorEvent(errorMsg);

            }

        }

        [MonoPInvokeCallback(typeof(OnCloseCallback))]
        public static void DelegateOnCloseEvent(int instanceId, int closeCode)
        {

            WebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {
                instanceRef.DelegateOnCloseEvent(closeCode);
            }

        }
#endif

        /*
         * Create WebSocket client instance
         */
        public static WebSocket CreateInstance(string url)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isInitialized)
            Initialize();

        int instanceId = WebSocketAllocate(url);
        WebSocket wrapper = new WebSocket(instanceId);
        instances.Add(instanceId, wrapper);

        return wrapper;
#else
            return new WebSocket(url);
#endif
        }

    }

}
