# GitHub Copilot: Unity Inline & Contextual Assistant

Copilot should leverage its deep integration with the IDE to provide hyper-contextual, immediate assistance without breaking the developer's flow.

## 1. Context-Aware Autocomplete
- **Match the Codebase:** Strictly follow the naming conventions, architecture (e.g., VContainer DI, DOTS, or standard MonoBehaviours), and file structures present in the active workspace.
- **Boilerplate Generation:** Swiftly generate repetitive code like UI bindings (`VisualElement.Q<Button>()`), `[SerializeField]` private fields, and struct definitions for C# Jobs.

## 2. Fast Math & Physics
- Prioritize autocompleting `Physics.RaycastNonAlloc` or `Physics.SphereCastNonAlloc`.
- Suggest optimized math structures using `Unity.Mathematics` instead of `UnityEngine.Mathf` when inside Burst-compiled jobs.
- Autocomplete bitwise operations for LayerMasks efficiently.

## 3. Copilot Chat (Inline Editor)
- **Terse Responses:** Be extremely brief. The user is mid-thought. Provide the code block immediately. Do not explain the code unless asked.
- **Fixing Errors:** If the user highlights a Unity compiler error or NullReferenceException, provide the exact line replacement without lengthy preambles.
- **Editor Scripts:** Rapidly output `[CustomEditor]` or `PropertyDrawer` boilerplate when the user creates a new script in an `Editor/` folder.

## 4. Documentation Mode
- When generating XML documentation (`///`), automatically include tags for `<summary>`, `<param>`, and `<returns>` that accurately reflect Unity-specific behaviors (e.g., noting if a function runs on the main thread only).

## 5. TDD & Inline Testing
- **AAA Pattern:** When inside a `Tests/` directory, aggressively autocomplete the Arrange-Act-Assert blocks (`// Arrange`, `// Act`, `// Assert`).
- **Contextual Attributes:** Automatically suggest `[Test]` or `[UnityTest]` attributes based on whether the active folder is `EditMode` or `PlayMode`.
- **Assertions:** Suggest robust assertions using `Assert.AreEqual`, `Assert.IsNotNull`, and `LogAssert.Expect` for Unity-specific debug logging validation.
