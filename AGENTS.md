# AGENTS.md

Shared instructions for every AI agent (Claude Code, Codex, Cursor, Copilot, ...) working on this project.

## Project overview

- **Name:** Tutor
- **Purpose:** Unity 3D physics simulation: **simple pendulum experiment** (subject: Physics). The player walks around a virtual lab in first person and performs the experiment.
- **Perspective:** First-person (player uses a `CharacterController` with a child camera)

## Experiment specification

### 1. Equipment (must exist in the scene)
- A sturdy stand to hang the pendulum from
- A light, inextensible string
- A bob (the weight)
- A ruler to measure string length
- A stopwatch
- A whiteboard (the pendulum hangs in front of it)

### 2. Procedure
1. Set up the pendulum hanging in front of the whiteboard.
2. Pull the bob away from equilibrium by a **small angle**.
3. Release it gently.
   - Expected: the bob swings left ↔ right along a circular arc and gradually slows down (damping).

### 3. Expected results (the simulation must demonstrate these)
- The motion is periodic.
- The period depends on the string length: `T = 2π * sqrt(L / g)`.
- The period does **not** depend on the bob's mass (for small oscillations).

### Physics rules for the code
- Pendulum speed/period must be **computed** from string length `L` and gravity `g` (use `Physics.gravity.magnitude`), not set as an arbitrary value.
- Mass must not affect the period.
- Swing angle and damping must be configurable; keep realistic defaults (initial angle ≤ ~10–15° for the small-angle case).
- Values shown to the user (length, period, time) should use real units (m, s).

### Accuracy and determinism (mandatory)

Unity's built-in physics (PhysX) is a **game** physics engine: it trades exact accuracy for performance. Even inside `FixedUpdate`, running the same experiment twice can give different results (e.g. the bob's position after 5 s differs between runs, and the error grows over time). This project is a scientific simulation, so:

- **Requirement:** the same initial conditions must produce the **same result on every run** (same position at the same simulated time), and the result must match theory (`T = 2π * sqrt(L / g)`).
- **Never** advance the simulation with `Time.deltaTime` or from `Update`. Simulation time advances only in fixed steps.
- Keep simulated time as a step counter (`stepCount * dt`), not by summing floats (`timer += dt` accumulates rounding error).
- Separate **simulation state** (angle, angular velocity, time) from **visuals** (the `Transform`). The `Transform` only displays the current state.

Unity physics is **not required**, but it must still be studied and applied. Implement **both** approaches below, so they can be compared side by side (same `L`, angle and damping) and the differences in accuracy and determinism can be shown. The custom solver is the reference ("ground truth") version.

1. **Custom solver (reference implementation):** integrate the pendulum equation ourselves instead of using `Rigidbody` / `HingeJoint`:
   `θ'' = -(g / L) * sin(θ) - b * θ'` (b = damping coefficient).
   Use `double`, a fixed small step (e.g. 0.001 s) with several substeps per `FixedUpdate`, and a stable integrator (RK4 or semi-implicit Euler). Fully deterministic and verifiable against theory.
2. **PhysX version (`Rigidbody` + `HingeJoint`), applying the determinism fixes:**
   - Enable **Enhanced Determinism** (Project Settings > Physics).
   - Set `Physics.simulationMode = SimulationMode.Script` and step manually with `Physics.Simulate(fixedDt)` a fixed number of times.
   - Use a small fixed timestep, raise solver iterations (`Rigidbody.solverIterations`), disable sleeping on the bob.
   - Create/spawn physics objects in the same order on every run; apply inputs only at fixed steps.
   - Rigidbody interpolation is visual only and must not be used for measurements.

Verification: provide a way to log state (time, angle, measured period) so two runs can be compared and the measured period can be checked against `2π * sqrt(L / g)`.

## Tech stack

- **Unity:** 6000.3.15f1 (Unity 6)
- **Render pipeline:** URP 17.3.0, with Shader Graph (`Assets/Materials/Shader_graph`)
- **Input:** new Input System 1.19.0. Read input via `InputSystem.actions.FindAction("...")` (project-wide actions in `Assets/InputSystem_Actions.inputactions`).
  - Available actions include `Move`, `Look`, `Jump`, `Sprint`, `Attack`.
- **Physics:** 3D (`Rigidbody`, `CharacterController`, Joints)
- **IDE:** VS Code (with the Claude Code extension)

## Folder structure

| Folder | Contents |
|---|---|
| `Assets/Scripts/` | All gameplay C# scripts |
| `Assets/Scenes/` | Main scene: `SampleScene.unity` |
| `Assets/Materials/` | Materials, `Shader_graph/`, `Texture/` |
| `Assets/Prefab/` | Prefabs and models (`.fbx`) |
| `Assets/Coliider/` | Physic Materials (folder name is misspelled; keep it to avoid breaking references) |
| `Assets/SFX/` | Audio |

Do not touch: `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `*.csproj`, `*.slnx` (generated by Unity).

## Existing systems

- `PlayerController` (`Player.cs`): walk, sprint, jump, mouse look. Note: class name differs from file name.
- `RaycastSystem`: casts a ray from the camera center using `Physics.RaycastNonAlloc` and calls the `IRaycastTarget` interface (`OnRaycastEnter` / `OnRaycastExit` / `OnRaycastInteract`).
- `RaycastHighlight`: example target; changes color when looked at, gets pushed when clicked.
- `PendulumScript`: damped pendulum with a `Push()` method.

## OOP principles (mandatory)

All code in this project **must** follow the four pillars of OOP. When writing or reviewing code, check each one. If existing code violates them, point it out and propose a fix.

### 1. Encapsulation
- No `public` fields. Use `[SerializeField] private` for Inspector values.
- Expose data through read-only properties (`public float Speed => speed;`) or methods that validate input.
- Validate values that can break behavior (`[Min]`, `[Range]`, `Mathf.Clamp`).
- Keep designer configuration (set in the Inspector) separate from runtime state; methods must not overwrite configuration values.
- Other classes must never change an object's internal state directly.

### 2. Abstraction
- Expose only what callers need; keep helpers `private`.
- Define interactions through interfaces (e.g. `IRaycastTarget`) so callers depend on *what* an object can do, not *how* it does it.
- Each class has one clear responsibility. Split a class when it starts doing unrelated jobs.

### 3. Inheritance
- Use a base class (often `abstract`) when several classes share real state and behavior (e.g. an `Interactable` base with shared highlight logic).
- Use `protected` for members subclasses need; mark overridable methods `virtual` / `abstract`.
- Prefer composition (separate components) over deep inheritance chains; keep hierarchies shallow (max 2–3 levels below `MonoBehaviour`).
- Subclasses must be usable anywhere the base class is expected without breaking behavior.

### 4. Polymorphism
- Callers work with the interface or base type and let each object decide its own behavior. Avoid type checks such as `if (TryGetComponent(out SomeConcreteClass x))` chains or `switch` on type.
- Adding a new kind of object should mean adding a new class, not editing existing callers (Open/Closed principle).

### C# language rules for OOP
- Always write access modifiers explicitly (`private`, `protected`, `public`); never rely on defaults.
- Use C# properties (`{ get; private set; }`, `=> field`) instead of public fields or `GetX()`/`SetX()` pairs.
- Use `readonly` / `const` for values that must not change after construction.
- Use `interface` for capabilities (e.g. `IRaycastTarget`), `abstract class` for shared base behavior.
- Override with `virtual` / `abstract` + `override`; never hide members with `new`.
- Mark classes that are not designed for inheritance as `sealed`.
- In Unity, override `MonoBehaviour` lifecycle methods in subclasses by making the base one `protected virtual` (e.g. `protected virtual void Awake()`) and calling `base.Awake()` in the override.

### Known violations to fix
- `PendulumScript`: public fields (`speed`, `initialAngle`, `damping`, `randomStart`) break encapsulation; `Push()` overwrites the `initialAngle` configuration. It also uses an arbitrary `speed` instead of computing the period from string length (see Physics rules above).
- `RaycastHighlight`: checks for the concrete `PendulumScript` type and reads its public field instead of relying on `IRaycastTarget`.

## Code conventions

- All code, identifiers, comments and commit messages are written in **English**.
- `.cs` file name **must match** the `MonoBehaviour` class name.
- Naming: `PascalCase` for classes, methods, properties; `camelCase` for fields and locals.
- Cache components in `Awake` / `Start`; avoid allocations in `Update` (reuse buffers, use `NonAlloc` APIs).
- Do not add packages to `Packages/manifest.json` without asking.

## Working with Unity

- Agents cannot run the Unity Editor. After changing code, state clearly that it has not been tested in Play mode and explain how to test it.
- Do not hand-edit `.unity`, `.prefab`, `.mat` or `.meta` files unless asked. For scene changes, give the user step-by-step Editor instructions.
- When creating, deleting or renaming files in `Assets/`, handle the matching `.meta` file (rename it together; never generate a new GUID for an existing asset).

## Git

- Main branch: `main`
- Commit messages: English, short, imperative (e.g. `add raycast system`).
- Only commit when the user asks.

## Communication

- Reply to the user in **Vietnamese**. Code, comments and identifiers stay in English.
