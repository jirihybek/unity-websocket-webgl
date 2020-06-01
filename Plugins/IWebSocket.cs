namespace HybridWebSocket
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

{
  /// <summary>
  /// WebSocket class interface shared by both native and JSLIB implementation.
  /// </summary>
  public interface IWebSocket
  {
    bool IsAlive { get; }

    /// <summary>
    /// Open WebSocket connection
    /// </summary>
    void Connect ();

    /// <summary>
    /// Close WebSocket connection with optional status code and reason.
    /// </summary>
    /// <param name="code">Close status code.</param>
    /// <param name="reason">Reason string.</param>
    void Close (WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null);

    /// <summary>
    /// Send binary data over the socket.
    /// </summary>
    /// <param name="data">Payload data.</param>
    void Send (byte[] data);

    /// <summary>
    /// Send binary data over the socket.
    /// </summary>
    /// <param name="str">Payload data.</param>
    void Send (string str);

    /// <summary>
    /// Return WebSocket connection state.
    /// </summary>
    /// <returns>The state.</returns>
    WebSocketState GetState ();

    /// <summary>
    /// Occurs when the connection is opened.
    /// </summary>
    event WebSocketOpenEventHandler OnOpen;

    /// <summary>
    /// Occurs when a message is received.
    /// </summary>
    event WebSocketMessageEventHandler OnMessage;

    /// <summary>
    /// Occurs when an error was reported from WebSocket.
    /// </summary>
    event WebSocketErrorEventHandler OnError;

    /// <summary>
    /// Occurs when the socked was closed.
    /// </summary>
    event WebSocketCloseEventHandler OnClose;
  }
}
