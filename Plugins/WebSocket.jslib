/*
 * unity-websocket-webgl
 * 
 * @author Jiri Hybek <jiri@hybek.cz>
 * @copyright 2018 Jiri Hybek <jiri@hybek.cz>
 * @license Apache 2.0 - See LICENSE file distributed with this source code.
 */

var LibraryWebSocket = {
	$webSocketState: {
		/*
		 * Map of instances
		 * 
		 * Instance structure:
		 * {
		 * 	url: string,
		 * 	ws: WebSocket
		 * }
		 */
		instances: {},

		/* Last instance ID */
		lastId: 0,

		/* Event listeners */
		onOpen: null,
		onMesssage: null,
		onError: null,
		onClose: null,

		/* Debug mode */
		debug: true
	},

	/**
	 * Set onOpen callback
	 * 
	 * @param callback Reference to C# static function
	 */
	WebSocketSetOnOpen: function(callback) {

		webSocketState.onOpen = callback;

	},

	/**
	 * Set onMessage callback
	 * 
	 * @param callback Reference to C# static function
	 */
	WebSocketSetOnMessage: function(callback) {

		webSocketState.onMessage = callback;

	},

	/**
	 * Set onError callback
	 * 
	 * @param callback Reference to C# static function
	 */
	WebSocketSetOnError: function(callback) {

		webSocketState.onError = callback;

	},

	/**
	 * Set onClose callback
	 * 
	 * @param callback Reference to C# static function
	 */
	WebSocketSetOnClose: function(callback) {

		webSocketState.onClose = callback;

	},

	/**
	 * Allocate new WebSocket instance struct
	 * 
	 * @param url Server URL
	 */
	WebSocketAllocate: function(url) {

		var urlStr = UTF8ToString(url);
		var id = webSocketState.lastId++;

		webSocketState.instances[id] = {
			url: urlStr,
			ws: null
		};

		return id;

	},

	/**
	 * Remove reference to WebSocket instance
	 * 
	 * If socket is not closed function will close it but onClose event will not be emitted because
	 * this function should be invoked by C# WebSocket destructor.
	 * 
	 * @param instanceId Instance ID
	 */
	WebSocketFree: function(instanceId) {

		var instance = webSocketState.instances[instanceId];

		if (!instance) return 0;

		// Close if not closed
		if (instance.ws !== null && instance.ws.readyState < 2)
			instance.ws.close();

		// Remove reference
		delete webSocketState.instances[instanceId];

		return 0;

	},

	/**
	 * Connect WebSocket to the server
	 * 
	 * @param instanceId Instance ID
	 */
	WebSocketConnect: function(instanceId) {

		// fix for unity 2021 because unity bug in .jslib
		if (typeof Runtime === "undefined") {
			// if unity doesn't create Runtime, then make it here
			// dont ask why this works, just be happy that it does
			Runtime = {
				dynCall: dynCall
			}
		}

		var instance = webSocketState.instances[instanceId];
		if (!instance) return -1;

		if (instance.ws !== null)
			return -2;

		instance.ws = new WebSocket(instance.url);

		instance.ws.binaryType = 'arraybuffer';

		instance.ws.onopen = function() {

			if (webSocketState.debug)
				console.log("[JSLIB WebSocket] Connected.");

			if (webSocketState.onOpen)
				Runtime.dynCall('vi', webSocketState.onOpen, [ instanceId ]);
				//Module['dynCall_vi'](webSocketState.onOpen, [ instanceId ]);

		};

		instance.ws.onmessage = function(ev) {

			if (webSocketState.debug)
				console.log("[JSLIB WebSocket] Received message:", ev.data);

			if (webSocketState.onMessage === null)
				return;

			if (ev.data instanceof ArrayBuffer) {
				console.log("[JSLIB WebSocket] Received ArrayBuffer");
				var dataBuffer = new Uint8Array(ev.data);
				
				var buffer = _malloc(dataBuffer.length);
				HEAPU8.set(dataBuffer, buffer);

				try {
					Runtime.dynCall('viii', webSocketState.onMessage, [ instanceId, buffer, dataBuffer.length ]);
					//Module['dynCall_viii']( webSocketState.onMessage, [ instanceId, buffer, dataBuffer.length ]);
				} finally {
					_free(buffer);
				}

			} else if (typeof ev.data == 'string') {
				var arrBuffer = new ArrayBuffer(ev.data.length)
				var dataBuffer = new Uint8Array(arrBuffer)
				for (var i = 0, len = ev.data.length; i < len; i++) {
				  dataBuffer[i] = ev.data.charCodeAt(i)
				}
				var buffer = _malloc(dataBuffer.length);
				HEAPU8.set(dataBuffer, buffer);
				try {
					Runtime.dynCall('viii', webSocketState.onMessage, [ instanceId, buffer, dataBuffer.length ]);
					//Module['dynCall_viii']( webSocketState.onMessage,[ instanceId, buffer, dataBuffer.length ]);
				} finally {
					_free(buffer);
				}
			}

		};

		instance.ws.onerror = function(ev) {
			
			if (webSocketState.debug)
				console.log("[JSLIB WebSocket] Error occured.");

			if (webSocketState.onError) {
				
				var msg = "WebSocket error.";
				var msgBytes = lengthBytesUTF8(msg);
				var msgBuffer = _malloc(msgBytes + 1);
				stringToUTF8(msg, msgBuffer, msgBytes);

				try {
					Runtime.dynCall('vii', webSocketState.onError, [ instanceId, msgBuffer ]);
					//Module['dynCall_vii']( webSocketState.onError, [ instanceId, msgBuffer ]);
				} finally {
					_free(msgBuffer);
				}

			}

		};

		instance.ws.onclose = function(ev) {

			if (webSocketState.debug)
				console.log("[JSLIB WebSocket] Closed." + ev.code);

			if (webSocketState.onClose)
				Runtime.dynCall('vii', webSocketState.onClose, [ instanceId, ev.code ]);
				//Module['dynCall_vii']( webSocketState.onClose, [ instanceId, ev.code ]);

			delete instance.ws;

		};

		return 0;

	},

	/**
	 * Close WebSocket connection
	 * 
	 * @param instanceId Instance ID
	 * @param code Close status code
	 * @param reasonPtr Pointer to reason string
	 */
	WebSocketClose: function(instanceId, code, reasonPtr) {

		var instance = webSocketState.instances[instanceId];
		if (!instance) return -1;

		if (instance.ws === null)
			return -3;

		if (instance.ws.readyState === 2)
			return -4;

		if (instance.ws.readyState === 3)
			return -5;

		var reason = ( reasonPtr ? UTF8ToString(reasonPtr) : undefined );
		
		try {
			instance.ws.close(code, reason);
		} catch(err) {
			return -7;
		}

		return 0;

	},

	/**
	 * Send message over WebSocket
	 * 
	 * @param instanceId Instance ID
	 * @param bufferPtr Pointer to the message buffer
	 * @param length Length of the message in the buffer
	 */
	WebSocketSend: function(instanceId, bufferPtr, length) {
		var instance = webSocketState.instances[instanceId];
		if (!instance) return -1;
		
		if (instance.ws === null)
			return -3;

		if (instance.ws.readyState !== 1)
			return -6;

		instance.ws.send(HEAPU8.buffer.slice(bufferPtr, bufferPtr + length));

		return 0;

	},

	/**
	 * Return WebSocket readyState
	 * 
	 * @param instanceId Instance ID
	 */
	WebSocketGetState: function(instanceId) {

		var instance = webSocketState.instances[instanceId];
		if (!instance) return -1;

		if (instance.ws)
			return instance.ws.readyState;
		else
			return 3;

	}

};

autoAddDeps(LibraryWebSocket, '$webSocketState');
mergeInto(LibraryManager.library, LibraryWebSocket);
