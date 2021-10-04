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
    /// Various helpers to work mainly with enums and exceptions.
    /// </summary>
    public static class WebSocketHelpers
    {

        /// <summary>
        /// Safely parse close code enum from int value.
        /// </summary>
        /// <returns>The close code enum.</returns>
        /// <param name="closeCode">Close code as int.</param>
        public static WebSocketCloseCode ParseCloseCodeEnum (int closeCode)
        {
            if (WebSocketCloseCode.IsDefined (typeof (WebSocketCloseCode), closeCode))
            {
                return (WebSocketCloseCode) closeCode;
            }
            else
            {
                return WebSocketCloseCode.Undefined;
            }
        }

        /*
         * Return error message based on int code
         * 

         */
        /// <summary>
        /// Return an exception instance based on int code.
        /// 
        /// Used for resolving JSLIB errors to meaninfull messages.
        /// </summary>
        /// <returns>Instance of an exception.</returns>
        /// <param name="errorCode">Error code.</param>
        /// <param name="inner">Inner exception</param>
        public static WebSocketException GetErrorMessageFromCode (int errorCode, Exception inner)
        {

            switch (errorCode)
            {
                case -1:
                    return new WebSocketUnexpectedException ("WebSocket instance not found.", inner);
                case -2:
                    return new WebSocketInvalidStateException ("WebSocket is already connected or in connecting state.", inner);
                case -3:
                    return new WebSocketInvalidStateException ("WebSocket is not connected.", inner);
                case -4:
                    return new WebSocketInvalidStateException ("WebSocket is already closing.", inner);
                case -5:
                    return new WebSocketInvalidStateException ("WebSocket is already closed.", inner);
                case -6:
                    return new WebSocketInvalidStateException ("WebSocket is not in open state.", inner);
                case -7:
                    return new WebSocketInvalidArgumentException ("Cannot close WebSocket. An invalid code was specified or reason is too long.", inner);
                default:
                    return new WebSocketUnexpectedException ("Unknown error.", inner);

            }
        }
    }
}
