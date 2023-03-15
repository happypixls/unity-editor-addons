[![Unity 2020.3+](https://img.shields.io/badge/unity-2020.3%2B-blue.svg)](https://unity3d.com/get-unity/download) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=shields)](http://makeapullrequest.com) [![License MIT](https://img.shields.io/badge/license-MIT-blue.svg?style=shields)](https://github.com/happypixls/unity-editor-addons/blob/main/LICENSE)
## Unity editor addons

It is a small library that handles generating namespaces for you and provides couple handy extensions ;).

## Features
- Namespace generation based on folder location (similar to classical C# projects created in Visual studio for example).
- Added templates and menu items for creating C# interfaces, enums and PoC classes.
- Once you move folders and assembly definitions around, the namespaces will be updated automatically.
- Extension to _invoke_ or _invoke repeating_ using a delegate with specifying a delay time before the method calls.
- _Calculator_ For now has methods for normalizing and reverse normalizing numbers for any given range.
- More features to be added soon :)

![CreatingMonoBehaviour](https://user-images.githubusercontent.com/118393872/225415971-464324e4-6646-4463-a02d-cfeeaf3f67ce.gif)

![CreatingPoC](https://user-images.githubusercontent.com/118393872/225415987-eadb49a4-b81c-4c7e-813b-a8dd88c16b71.gif)

![MovingFilesAndFoldersAround](https://user-images.githubusercontent.com/118393872/225415994-4a10ed04-3788-4f1e-8e65-2719da1018b3.gif)

## Usage
Make sure to add a root namespace in your project settings, otherwise namespace generation won't work.
![Rootnamespace](https://user-images.githubusercontent.com/118393872/225414983-62529749-41a8-4f96-83e3-0a04ef5cdecd.png)

To use `Invoke` with delay, add `HappyPixels.EditorAddons.Extensions` assembly definition to your asmdef file in your project

```csharp
using UnityEngine;
using HappyPixels.EditorAddons.Extensions;

namespace MyAwesomeProject.Behaviours
{
    public class MyBehaviour : MonoBehaviour
    {
        private void Start()
        {
            this.Invoke(() => { Debug.Log("My method body"); }, 1.0f); // Run method after 1 second delay
            this.InvokeRepeating(() => {Debug.Log("Repeating method body"), 1.5f, 5}; // Invoke repeating with a delay of 1.5 seconds
        }
    }
}
```
