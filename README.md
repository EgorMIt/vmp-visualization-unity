# VMP - algorithms visualization

Дипломная работа по направлению "Программная инженерия", 2023 год  

*Developped by EgorMit*

## vmp-visualization-unity

App can be tested on: [https://egormit.itch.io/vmp-cloud-visualizer](https://egormit.itch.io/vmp-cloud-visualizer)

## How to build an application
1. Clone the repository and open it as a Unity project (Using version 2021.3.11f1 of Unity Editor)
2. Go to the "file" -> "build settings"
3. Select the target platform *WebGL* and click "Switch platforms"
4. Click "build" by selecting the target folder "builds"
5. The process takes several minutes. 
You will get the necessary files to import the application using HTML 5 and WebGL 2
6. You need to use 4 files in "build" folder:
   *  The **LoaderUrl**, this is a JavaScript file which contains the Unity Engine bootstrapping code. This file is required to load the Unity Engine and start the initialization process.
   *  The **FrameworkUrl**, this is a JavaScript file which contains the Runtime and Plugin code. This file is responsible for running the actual Unity Application.
   *  The **DataUrl**, this is a JSON file which contains the initial Unity Application state including your Assets and Scenes. This file can get big really fast so try to optimize your game's assets as much as possible. Try using both building and runtime compression techniques and usefull packages such as sprite atlasses.
   *  The **CodeUrl**, this is a Web Assembly binary file containg native code.

This is an example of single-page react app with WebGl:
```javascript
import React from "react";
import {Unity, useUnityContext} from "react-unity-webgl";

function App() {
    const {unityProvider} = useUnityContext({
        loaderUrl: "/build/Builds.loader.js",
        dataUrl: "/build/Builds.data.unityweb",
        frameworkUrl: "/build/Builds.framework.js.unityweb",
        codeUrl: "/build/Builds.wasm.unityweb",
    });

    return (
        <div>
            <Unity
                style={{
                    width: "80%",
                    justifySelf: "center",
                    alignSelf: "center",
                }}
                unityProvider={unityProvider}
            />
        </div>
    );
}

export default App;
```
