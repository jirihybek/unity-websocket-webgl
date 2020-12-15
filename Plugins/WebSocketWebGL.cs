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
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// WebSocket class bound to JSLIB.
    /// </summary>
    public class WebSocketWebGL : IWebSocket
    {
        private bool _isAlive = false;

        public bool IsAlive
        {
            get
            {
                return _isAlive;
            }
        }

        /* WebSocket JSLIB functions */
        [DllImport ("__Internal")]
        public static extern int WebSocketConnect (int instanceId);

        [DllImport ("__Internal")]
        public static extern int WebSocketClose (int instanceId, int code, string reason);

        [DllImport ("__Internal")]
        public static extern int WebSocketSend (int instanceId, byte[] dataPtr, int dataLength);

        [DllImport ("__Internal")]
        public static extern int WebSocketSendStr (int instanceId, string str);

        [DllImport ("__Internal")]
        public static extern int WebSocketGetState (int instanceId);

        /// <summary>
        /// The instance identifier.
        /// </summary>
        protected int instanceId;

        /// <summary>
        /// Occurs when the connection is opened.
        /// </summary>
        public event WebSocketOpenEventHandler OnOpen;

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        public event WebSocketMessageEventHandler OnMessage;

        /// <summary>
        /// Occurs when an error was reported from WebSocket.
        /// </summary>
        public event WebSocketErrorEventHandler OnError;

        /// <summary>
        /// Occurs when the socked was closed.
        /// </summary>
        public event WebSocketCloseEventHandler OnClose;

        /// <summary>
        /// Constructor - receive JSLIB instance id of allocated socket
        /// </summary>
        /// <param name="instanceId">Instance identifier.</param>
        public WebSocketWebGL (int instanceId)
        {
            this.instanceId = instanceId;
        }

        /// <summary>
        /// Destructor - notifies WebSocketFactory about it to remove JSLIB references
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="T:HybridWebSocket.WebSocket"/> is reclaimed by garbage collection.
        /// </summary>
        ~WebSocketWebGL ()
        {
            WebSocketFactory.HandleInstanceDestroy (this.instanceId);
        }

        /// <summary>
        /// Return JSLIB instance ID
        /// </summary>
        /// <returns>The instance identifier.</returns>
        public int GetInstanceId ()
        {
            return this.instanceId;
        }

        /// <summary>
        /// Open WebSocket connection
        /// </summary>
        public void Connect ()
        {
            int ret = WebSocketConnect (this.instanceId);
            if (ret < 0) throw WebSocketHelpers.GetErrorMessageFromCode (ret, null);
        }

        /// <summary>
        /// Close WebSocket connection with optional status code and reason.
        /// </summary>
        /// <param name="code">Close status code.</param>
        /// <param name="reason">Reason string.</param>
        public void Close (WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {
            int ret = WebSocketClose (this.instanceId, (int) code, reason);
            if (ret < 0) throw WebSocketHelpers.GetErrorMessageFromCode (ret, null);
        }

        /// <summary>
        /// Send binary data over the socket.
        /// </summary>
        /// <param name="data">Payload data.</param>
        public void Send (byte[] data)
        {
            int ret = WebSocketSend (this.instanceId, data, data.Length);
            if (ret < 0) throw WebSocketHelpers.GetErrorMessageFromCode (ret, null);
        }

        public void Send (string str)
        {
            Debug.Log (str);
            int ret = WebSocketSendStr (this.instanceId, str);
            if (ret < 0) throw WebSocketHelpers.GetErrorMessageFromCode (ret, null);
        }

        /// <summary>
        /// Return WebSocket connection state.
        /// </summary>
        /// <returns>The state.</returns>
        public WebSocketState GetState ()
        {
            int state = WebSocketGetState (this.instanceId);
            if (state < 0) throw WebSocketHelpers.GetErrorMessageFromCode (state, null);
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

        /// <summary>
        /// Delegates onOpen event from JSLIB to native sharp event
        /// Is called by WebSocketFactory
        /// </summary>
        public void DelegateOnOpenEvent ()
        {
            Debug.Log ("OnOpen fired");
            _isAlive = true;
            this.OnOpen?.Invoke ();
        }

        /// <summary>
        /// Delegates onMessage event from JSLIB to native sharp event
        /// Is called by WebSocketFactory
        /// </summary>
        /// <param name="data">Binary data.</param>
        public void DelegateOnMessageEvent (byte[] data)
        {
            _isAlive = true;
            this.OnMessage?.Invoke (data);
        }

        /// <summary>
        /// Delegates onError event from JSLIB to native sharp event
        /// Is called by WebSocketFactory
        /// </summary>
        /// <param name="errorMsg">Error message.</param>
        public void DelegateOnErrorEvent (string errorMsg)
        {
            this.OnError?.Invoke (errorMsg);
        }

        /// <summary>
        /// Delegate onClose event from JSLIB to native sharp event
        /// Is called by WebSocketFactory
        /// </summary>
        /// <param name="closeCode">Close status code.</param>
        public void DelegateOnCloseEvent (int closeCode)
        {
            Debug.Log ("OnClose fired");
            _isAlive = false;
            this.OnClose?.Invoke (WebSocketHelpers.ParseCloseCodeEnum (closeCode));
        }
    }
}
