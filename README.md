# unity-websocket-webgl

## Maintainers Wanted

I've created this library as a Hobby project when I was experimenting with Unity. I haven't used Unity for over 2 years and I'm not able to maintain the repo. If you want to become a maintainer a keep this project running, send me an e-mail or create an issue. Thank you for understanding.

---

Hybrid event-driven WebSocket implementation for Unity 3D.

It automatically compiles browser or native implementation based on project's target platform. Native implementation is using [WebSocketSharp](https://github.com/sta/websocket-sharp) library (must be downloaded separately - see below). For the browser implementation the custom emscripten JSLIB is used.

**Warning:** WebSocket client is intended to support only binary messages. So if you want to send or receive string messages you must convert it to/from byte array in your code.

## Downloading WebSocketSharp

You can get `WebSocketSharp.dll` from NuGet package manager. See [NuGet Gallery: websocket-sharp](http://www.nuget.org/packages/WebSocketSharp).

Library can be installed manually. Just download the NuGet package. Then rename the file extension from `.nupkg` to `.zip` and extract it. Now you can copy `lib/websocket-sharp.dll` file to your Unity project into the `Assets/Plugins` directory.

For more info please visit official [WebSocketSharp GitHub repo](https://github.com/sta/websocket-sharp).

## Installing plugin

To install this plugin just copy the contents of `Plugins` directory to your Unity project's `Assets/Plugins` directory.

## Usage

For example usage see `Scripts/WebSocketDemo.cs` file. You can import it to your project and assign it to any GameObject. Then run your project and check the console.

```csharp
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Use plugin namespace
using HybridWebSocket;

public class WebSocketDemo : MonoBehaviour {

    // Use this for initialization
    void Start () {

        // Create WebSocket instance
        WebSocket ws = WebSocketFactory.CreateInstance("ws://echo.websocket.org");

        // Add OnOpen event listener
        ws.OnOpen += () =>
        {
            Debug.Log("WS connected!");
            Debug.Log("WS state: " + ws.GetState().ToString());

            ws.Send(Encoding.UTF8.GetBytes("Hello from Unity 3D!"));
        };

        // Add OnMessage event listener
        ws.OnMessage += (byte[] msg) =>
        {
            Debug.Log("WS received message: " + Encoding.UTF8.GetString(msg));

            ws.Close();
        };

        // Add OnError event listener
        ws.OnError += (string errMsg) =>
        {
            Debug.Log("WS error: " + errMsg);
        };

        // Add OnClose event listener
        ws.OnClose += (WebSocketCloseCode code) =>
        {
            Debug.Log("WS closed with code: " + code.ToString());
        };

        // Connect to the server
        ws.Connect();

    }
    
    // Update is called once per frame
    void Update () {
        
    }
}
```

## Error Handling

When any error occours during method call then the `WebSocketException` is thrown. Unified both for native and browser client so you can catch it in your C# code.

## License Apache 2.0

Copyright 2018 Jiri Hybek <jiri@hybek.cz>

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

```
http://www.apache.org/licenses/LICENSE-2.0
```

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
