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

using System;

namespace HybridWebSocket
{
    public class WebSocket : IWebSocket
    {
        public bool IsAlive
        {
            get
            {
                return this.ws.IsAlive;
            }
        }

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
        /// The WebSocketSharp instance.
        /// </summary>
        protected WebSocketSharp.WebSocket ws;

        /// <summary>
        /// WebSocket constructor.
        /// </summary>
        /// <param name="url">Valid WebSocket URL.</param>
        public WebSocket (string url)
        {

            try
            {
                // Create WebSocket instance
                this.ws = new WebSocketSharp.WebSocket (url);

                // Bind OnOpen event
                this.ws.OnOpen += (sender, ev) =>
                {
                    this.OnOpen?.Invoke ();
                };

                // Bind OnMessage event
                this.ws.OnMessage += (sender, ev) =>
                {
                    if (ev.Data != null)
                    {
                        this.OnMessage?.Invoke (System.Text.Encoding.UTF8.GetBytes (ev.Data));
                    }
                };

                // Bind OnError event
                this.ws.OnError += (sender, ev) =>
                {
                    this.OnError?.Invoke (ev.Message);
                };

                // Bind OnClose event
                this.ws.OnClose += (sender, ev) =>
                {
                    this.OnClose?.Invoke (
                        WebSocketHelpers.ParseCloseCodeEnum ((int) ev.Code)
                    );
                };
            }
            catch (Exception e)
            {

                throw new WebSocketUnexpectedException ("Failed to create WebSocket Client.", e);
            }
        }

        /// <summary>
        /// Open WebSocket connection
        /// </summary>
        public void Connect ()
        {
            // Check state
            if (this.ws.ReadyState == WebSocketSharp.WebSocketState.Open ||
                this.ws.ReadyState == WebSocketSharp.WebSocketState.Closing)
            {
                throw new WebSocketInvalidStateException ("WebSocket is already connected or is closing.");
            }
            try
            {
                this.ws.Connect ();
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException ("Failed to connect.", e);
            }
        }

        /// <summary>
        /// Close WebSocket connection with optional status code and reason.
        /// </summary>
        /// <param name="code">Close status code.</param>
        /// <param name="reason">Reason string.</param>
        public void Close (WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {
            // Check state
            if (this.ws.ReadyState == WebSocketSharp.WebSocketState.Closing)
                throw new WebSocketInvalidStateException ("WebSocket is already closing.");

            if (this.ws.ReadyState == WebSocketSharp.WebSocketState.Closed)
                throw new WebSocketInvalidStateException ("WebSocket is already closed.");
            try
            {
                this.ws.CloseAsync ((ushort) code, reason);
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException ("Failed to close the connection.", e);
            }
        }

        /// <summary>
        /// Send binary data over the socket.
        /// </summary>
        /// <param name="data">Payload data.</param>
        public void Send (byte[] data)
        {
            // Check state
            if (this.ws.ReadyState != WebSocketSharp.WebSocketState.Open)
                throw new WebSocketInvalidStateException ("WebSocket is not in open state.");
            try
            {
                this.ws.Send (data);
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException ("Failed to send message.", e);
            }
        }

        public void Send (string str)
        {
            // Check state
            if (this.ws.ReadyState != WebSocketSharp.WebSocketState.Open)
                throw new WebSocketInvalidStateException ("WebSocket is not in open state.");
            try
            {
                this.ws.Send (str);
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException ("Failed to send message.", e);
            }
        }

        /// <summary>
        /// Return WebSocket connection state.
        /// </summary>
        /// <returns>The state.</returns>
        public WebSocketState GetState ()
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
}
