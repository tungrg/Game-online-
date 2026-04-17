# AAA Unity Game Development Rules

These rules represent the absolute highest standard for AAA 3D Unity development. All AI assistants must adhere to these guidelines to ensure code is production-ready, highly performant, and scalable across a massive codebase.

## 1. Core Architecture & Design Patterns
- **Dependency Injection:** Favor DI frameworks (e.g., VContainer, Zenject) over Singletons to ensure modularity and testability.
- **Event-Driven & Decoupled:** Use `Action`, `Func`, or robust event buses to communicate between systems. Avoid tight coupling between UI and Gameplay logic (use MVP/MVVM patterns).
- **Data-Driven Design:** Utilize `ScriptableObjects` extensively for configuration data, game balance tweaking, and inventory definitions to empower game designers.

## 2. Extreme Performance & Memory Management (Zero GC)
- **Zero Allocations in Hot Paths:** Absolutely NO allocations (`new`, boxing, string formatting, `Instantiate`, `Destroy`) in `Update`, `FixedUpdate`, `LateUpdate`, or inner loops.
- **Advanced Object Pooling:** Pre-warm object pools during loading screens. Pool not just GameObjects, but also pure C# classes, lists, and dictionaries.
- **Data-Oriented Technology Stack (DOTS):** For mass entity systems (crowds, particles, complex AI), default to Unity ECS, the Burst Compiler, and the C# Job System.
- **Native Collections:** Use `NativeArray`, `NativeList`, and `NativeHashMap` allocated with `Allocator.Temp` or `Allocator.TempJob` for temporary high-performance operations.

## 3. Math, Physics, and Logic
- **Physics NonAlloc:** Strictly use non-allocating physics APIs (`Physics.RaycastNonAlloc`, `Physics.OverlapSphereNonAlloc`). Utilize layer masks and minimal sphere/box casts.
- **Optimized Math:** Use `Unity.Mathematics` for Burst-compatible, SIMD-optimized math. Prefer `sqrMagnitude` over `Distance` to avoid expensive square root calculations.

## 4. Asset Management & Loading
- **Addressable Asset System:** Never use `Resources`. Load all dynamic assets via Addressables to control memory footprint and enable seamless DLC/patching.
- **Asynchronous Operations:** All loading, heavy computation, and I/O must be asynchronous (use `async/await` with `UniTask` or Unity's `AsyncOperation`).

## 5. UI and Tooling
- **UI Toolkit over uGUI:** Prefer UI Toolkit for Editor tools and modern game UI to reduce draw calls and memory overhead.
- **Custom Tooling:** Always build custom PropertyDrawers, EditorWindows, and Gizmos to streamline level design and debugging for the team.

## 6. Testing & Quality Assurance (TDD & UTF)
- **Test-Driven Architecture:** All systems must be designed for testability. Rely heavily on Interfaces (`IDamageable`, `IInputProvider`) so dependencies can be mocked (e.g., NSubstitute, Moq) during EditMode tests.
- **Strict Folder & Assembly Structure:** Whenever a new feature module (e.g., `Scripts/Inventory/`) is created, you MUST automatically specify the creation of:
  - `Tests/EditMode/` for pure logic tests.
  - `Tests/PlayMode/` for Unity lifecycle/physics tests.
  - An `.asmdef` for each test folder. These must have the `TestAssemblies` flag checked, and reference `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, and the target module's `.asmdef`.
- **The TDD Loop:** 
  1. **Write the Test First:** Before generating the function body, write a failing `[Test]` or `[UnityTest]`.
  2. **Arrange-Act-Assert (AAA):** Structure every test explicitly with `// Arrange`, `// Act`, and `// Assert` comments.
  3. **Implement:** Write the minimum C# code to pass the test.
- **PlayMode vs EditMode:** Use EditMode (`[Test]`) for instant math, state machines, and pure C# logic. Use PlayMode (`[UnityTest]`) with `yield return null;` or `yield return new WaitForFixedUpdate();` *only* for physics, monobehaviour lifecycles (`Start`/`Update`), and coroutine validations.
