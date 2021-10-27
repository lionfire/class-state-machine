
## Class State Machine (LionFire.StateMachines.Class) 

A convention-oriented state machine to ease the burden for typical C# classes.  Roslyn is used to generate code at design-time.

--> Docs & walkthrough: [lionfire.readthedocs.io/en/latest/class-state-machine/](https://lionfire.readthedocs.io/en/latest/class-state-machine/index.html) <--

## Status

Unit tests in this project work, but I'm having issues getting it working in a NETStandard2.0 environment. YMMV.  PR's welcome.  There are potentially related issues in the upstream with netstandard2.0.  See [https://github.com/AArnott/CodeGeneration.Roslyn/issues/48](https://github.com/AArnott/CodeGeneration.Roslyn/issues/48)

### ROSLYN MIGRATION STATUS

The CodeGeneration.Roslyn project itself has been discontinued in favor of the new Roslyn Source Generators.  I ported it in the source-generator branch, and the generation works (generator project can't be newer than netstandard2.0), however, Visual Studio 2019 and 2022 claims the generator generated no files, and Intellisense is not picking up which is a dealbreaker for me.  I saw others have this problem on a github issue thread but nobody had any answers.  So I have to conclude the support for this doesn't seem to be really complete yet.

## Info

Package | Version | Pre-release | Downloads
------- | ------- | ----------- | ---------
LionFire.StateMachines.Class.Abstractions | [![NuGet](https://img.shields.io/nuget/v/LionFire.StateMachines.Class.Abstractions.svg)]() | [![NuGet Pre Release](https://img.shields.io/nuget/vpre/LionFire.StateMachines.Class.Abstractions.svg)]() | [![NuGet](https://img.shields.io/nuget/dt/LionFire.StateMachines.Class.Abstractions.svg)]() 
LionFire.StateMachines.Class | [![NuGet](https://img.shields.io/nuget/v/LionFire.StateMachines.Class.svg)]() | [![NuGet Pre Release](https://img.shields.io/nuget/vpre/LionFire.StateMachines.Class.svg)]() | [![NuGet](https://img.shields.io/nuget/dt/LionFire.StateMachines.Class.svg)]() 
LionFire.StateMachines.Class.Generation | [![NuGet](https://img.shields.io/nuget/v/LionFire.StateMachines.Class.Generation.svg)]() | [![NuGet Pre Release](https://img.shields.io/nuget/vpre/LionFire.StateMachines.Class.Generation.svg)]() | [![NuGet](https://img.shields.io/nuget/dt/LionFire.StateMachines.Class.Generation.svg)]() 

Target    | SDK Version | OS           | Status
--------- | ----------- | ---          | ---
.NET Core | 2.0.0       | Ubuntu 14.04 | [![Build Status](https://travis-ci.org/lionfire/class-state-machine.svg?branch=master)](https://travis-ci.org/lionfire/class-state-machine)


