# Phase 1 source checks (no Unity editor launch)

Requires .NET SDK 10 and local Unity reference assemblies. The default reference path is the installed 6000.6.0f1 editor's Data directory. Override with `-p:UnityReferenceEditor="path/to/Editor/Data"` when building. This does **not** change ProjectVersion.txt or open/import/save the Unity project.

From this directory:

```
dotnet build Domain/Domain.csproj --configfile NuGet.Config
dotnet Domain/bin/Debug/net10.0/Domain.dll
dotnet build Compile/Compile.csproj --configfile NuGet.Config
```

The small standalone runner invokes the actual NUnit test methods for BoardGrid, HelpPositionPlanner, BallStateMachine and input providers/range/lifecycle. Input tests use managed Unity vector types and mocked wrappers; they do not access device/native Unity services. This is not a Unity Test Runner result. No package feeds are used.

Compile checks runtime/editor/test C# against local reference assemblies. Using 6.6 references is a limited source/API check, not certification against the project's recorded 6000.3.11f1 editor. Exact-editor closeout on 20 September passed 79 EditMode and 14 PlayMode tests; see `Docs/Phase1/Verification`. Run both suites in the recorded editor: SandboxPlayTest now belongs to a separate editor-hosted PlayMode test assembly. Android hardware verification remains separate.
