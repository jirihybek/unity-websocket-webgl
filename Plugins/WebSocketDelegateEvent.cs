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
    /// <summary>
    /// Handler for WebSocket Open event.
    /// </summary>
    public delegate void WebSocketOpenEventHandler ();

    /// <summary>
    /// Handler for message received from WebSocket.
    /// </summary>
    public delegate void WebSocketMessageEventHandler (byte[] data);

    /// <summary>
    /// Handler for an error event received from WebSocket.
    /// </summary>
    public delegate void WebSocketErrorEventHandler (string errorMsg);

    /// <summary>
    /// Handler for WebSocket Close event.
    /// </summary>
    public delegate void WebSocketCloseEventHandler (WebSocketCloseCode closeCode);
}
