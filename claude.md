# Claude: Unity Multi-Modal Modes

Claude should dynamically switch between the following modes depending on the user's current goal in the Unity development cycle. Always wrap your thought process in `<thinking>` tags before outputting complex architectural code.

## 1. Unity AAA Architectural Mode
When the user is designing core systems, focus on robust design and deep logical problem-solving.
- **System Design:** Architect features (e.g., Inventory, AI State Machines, Network Sync) using Dependency Injection, ScriptableObjects, and strict SOLID principles.
- **DOTS & Multi-threading:** Plan out Entity-Component-System structures, C# Jobs, and Burst compiler optimizations. Define explicit `[ReadOnly]` and `[WriteOnly]` native array dependencies.
- **Deep Refactoring:** When analyzing monolithic `MonoBehaviours`, explicitly plan out how to split them into modular components and separate concerns before writing code. Explain the 'Big-O' complexity of changes.

## 2. Unity Prototyping Mode
When prototyping, Claude should prioritize speed of implementation and feature discovery over absolute optimization.
- **Rapid Iteration:** Use `GetComponent` liberally to avoid writing boilerplate manager systems immediately. Default to standard `MonoBehaviour` patterns.
- **Discovery & Learning:** When asked "how do I" or "does Unity have", suggest modern Unity packages (Input System, Cinemachine, Netcode for GameObjects) rather than manual implementations.
- **Asset Workflow:** Suggest simple primitive-based placeholders or Asset Store tools to get the "look and feel" right quickly.

## 3. Render Pipeline & Graphics Mode (SRP/URP/HDRP)
When the user asks about shaders, lighting, or post-processing:
- **Render Graph & SRP:** Consider the impact on the Render Graph, command buffers, and compute shaders. Ensure CPU/GPU synchronization.
- **HLSL & Compute:** Write optimized HLSL, providing specific pragmas and multi-compile variants to avoid shader variant explosion.

## 4. Test-Driven Development (TDD) Mode
When generating new systems or functions, Claude must integrate testing at the architectural level:
- **Automatic Test Generation:** Output the corresponding EditMode or PlayMode tests *before* writing the actual implementation code to enforce TDD.
- **Assembly Generation:** Always output the exact JSON structure for the `.asmdef` file needed for the `Tests/` directory, ensuring `"optionalUnityReferences": ["TestAssemblies"]` is set and referencing the main module.
- **Mocking:** Inject interfaces into constructors so that NSubstitute or Moq can be used seamlessly in the generated test scripts.
