﻿// Based on https://stackoverflow.com/questions/1550249/programmatically-change-the-assemblyversion-and-assemblyfileversion-attributes
// and https://stackoverflow.com/questions/17123991/t4-reference-a-const-in-a-static-class-at-compile-time

// This is an autogenerated AssemblyInfo.cs file, it will be updated per build

using System.Reflection;
using System.Runtime.InteropServices;

using MelonLoader;
using Entanglement;

[assembly: Guid("490e160d-251d-4ab4-a3bb-f473961ff8a1")]
[assembly: AssemblyTitle("Entanglement")]
[assembly: AssemblyVersion("0.3.0")]
[assembly: AssemblyFileVersion("0.3.0")]

[assembly: MelonInfo(typeof(EntanglementMod), "Entanglement", "0.3.0", "zCubed, Lakatrazz")]
[assembly: MelonGame("Stress Level Zero", "BONEWORKS")]
[assembly: MelonIncompatibleAssemblies("MultiplayerMod")]
[assembly: MelonPriority(-10000)]