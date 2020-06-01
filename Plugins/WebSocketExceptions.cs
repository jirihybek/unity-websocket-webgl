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
  using System;

  /// <summary>
  /// Generic WebSocket exception class
  /// </summary>
  public class WebSocketException : Exception
    {
        public WebSocketException () { }
        public WebSocketException (string message) : base (message) { }
        public WebSocketException (string message, Exception inner) : base (message, inner) { }
    }

    /// <summary>
    /// Web socket exception raised when an error was not expected, probably due to corrupted internal state.
    /// </summary>
    public class WebSocketUnexpectedException : WebSocketException
    {
        public WebSocketUnexpectedException () { }
        public WebSocketUnexpectedException (string message) : base (message) { }
        public WebSocketUnexpectedException (string message, Exception inner) : base (message, inner) { }
    }

    /// <summary>
    /// Invalid argument exception raised when bad arguments are passed to a method.
    /// </summary>
    public class WebSocketInvalidArgumentException : WebSocketException
    {
        public WebSocketInvalidArgumentException () { }
        public WebSocketInvalidArgumentException (string message) : base (message) { }
        public WebSocketInvalidArgumentException (string message, Exception inner) : base (message, inner) { }
    }

    /// <summary>
    /// Invalid state exception raised when trying to invoke action which cannot be done due to different then required state.
    /// </summary>
    public class WebSocketInvalidStateException : WebSocketException
    {
        public WebSocketInvalidStateException () { }
        public WebSocketInvalidStateException (string message) : base (message) { }
        public WebSocketInvalidStateException (string message, Exception inner) : base (message, inner) { }
    }
}
