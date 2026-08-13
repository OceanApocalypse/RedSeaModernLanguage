#
<div align="center">
	<img src="assets/logo.png" alt="RSML Logo" align="center" width="150">
	<h1>Red Sea Modern Language</h1>
</div>
<div align="center">
	<details open>
	<summary style="font-size: small;">Show/hide badges</summary>
		<a href="https://www.nuget.org/packages/OceanApocalypse.RSML" target="_blank"><img src="https://img.shields.io/nuget/v/OceanApocalypseStudios.RSML?style=for-the-badge&logo=nuget&logoColor=white&logoSize=auto&label=latest%20on%20NuGet%20and%20github&labelColor=%231265fb&color=%2308317b"/></a>
		<img src="https://img.shields.io/nuget/dt/OceanApocalypseStudios.RSML?style=for-the-badge&logo=nuget&logoColor=white&logoSize=auto&label=downloads%20(modern)&labelColor=%234929ca&color=%2327156f" />
		<a href="LICENSE.txt"><img alt="GitHub License" src="https://img.shields.io/github/license/OceanApocalypse/RedSeaModernLanguage?style=for-the-badge&logo=opensourceinitiative&logoColor=white&logoSize=auto&labelColor=%23496300&color=%232b3c00"></a>
		<br/>
		<img alt="GitHub Actions Test Status" src="https://img.shields.io/github/actions/workflow/status/OceanApocalypse/RedSeaModernLanguage/ci.yml?style=for-the-badge&label=tests">
		<img alt="GitHub Actions Release Status" src="https://img.shields.io/github/actions/workflow/status/OceanApocalypse/RedSeaModernLanguage/release.yml?style=for-the-badge&label=release">
		<a href="https://OceanApocalypse.org/rsml-docs/"><img alt="GitHub Actions Documentation Status" src="https://img.shields.io/github/actions/workflow/status/OceanApocalypse/rsml-docs/docs.yml?style=for-the-badge&label=docs"></a>
		<br/>
		<img alt="Sonar Quality Gate" src="https://img.shields.io/sonar/quality_gate/oas_RedSeaModernLanguage?server=https%3A%2F%2Fsonarcloud.io&style=for-the-badge&logo=sonar&logoColor=white&labelColor=%2329293c&label=quality%20gate%20(prod)">
		<img alt="Sonar Coverage" src="https://img.shields.io/sonar/coverage/oas_RedSeaModernLanguage?server=https%3A%2F%2Fsonarcloud.io&style=for-the-badge&logo=sonar&logoColor=white&label=coverage%20(prod)&labelColor=%2329293c">
			<details>
				<summary style="font-size: small;">See more badges</summary>
				<img alt="Sonar Quality Gate (dev branch)" src="https://img.shields.io/sonar/quality_gate/oas_RedSeaModernLanguage/dev?server=https%3A%2F%2Fsonarcloud.io&style=for-the-badge&logo=sonar&logoColor=white&label=quality%20gate%20(dev)&labelColor=%2329293c">
				<img alt="Sonar Coverage (dev branch)" src="https://img.shields.io/sonar/coverage/oas_RedSeaModernLanguage/dev?server=https%3A%2F%2Fsonarcloud.io&style=for-the-badge&logo=sonar&logoColor=white&label=coverage%20(dev)&labelColor=%2329293c">
				<img alt="GitHub commits since latest release (branch)" src="https://img.shields.io/github/commits-since/OceanApocalypse/RedSeaModernLanguage/latest/dev?sort=semver&style=for-the-badge&logo=github&logoSize=auto&labelColor=%231c1c1c&color=%23101010">
			</details>
	</details>
</div>
<hr/>
<div align="center">
<strong style="font-size: small">
An OceanApocalypse project,
</strong>
<p>
the modern language designed to dynamically interpret different logic paths based on an host's OS and CPU architecture.
</p>
</div>

---

> [!NOTE]
> For releases other than the latest, you will need to visit the [GitHub mirror](https://github.com/OceanApocalypse/RedSeaModernLanguage) instead. In the Codeberg repository, we only maintain the latest release assets to avoid wasting resources.

> [!NOTE]
> If you're visiting the GitHub repository, please keep in mind it is a mirror: for the repository accepting pull requests, please visit https://codeberg.org/oas/RedSeaModernLanguage.

## Contents
- [Red Sea Modern Language (RSML)](#section)
	- [Why RSML?](#why-rsml)
	- [How to build RSML?](#how-to-build-rsml)
	- [Performance Analysis](#performance-analysis)
	- [Supported Editors](#supported-editors)
	- [Supported Programming Languages](#supported-programming-languages)

<details open>
<summary><strong>Where's the "How to use" section?</strong></summary>

This file does not attempt to be a full documentation on how to use RSML.
Instead, you can find full documentation [here](https://OceanApocalypse.org/rsml-docs/).

</details>

<details open>
<summary><strong>How can I see what is being worked on in RSML?</strong></summary>

You can find the official bug tracker and
roadmap [here](https://github.com/orgs/OceanApocalypse/projects/6/views/2).

</details>

---

## Why RSML?
**Red Sea Modern Language** is **the** powerful and robust fork of [MF's CrossRoad Solution](https://mf366-dev.github.io/documentation/mfroad/mfroad_1.0.html), a language designed to **dynamically interpret different logic paths based on the local host OS and CPU architecture**.

RSML, which is short for Red Sea Modern Language, is still in development, but the finished v3.0.0 will observe the following core features:

- A complete toolchain featuring buffers, readers, lexers, parsers and evaluators.
- The core library available in C#, Visual Basic, Python and many other languages.
- SDK for convenience and a more idiomatic usage of RSML in C#.
- A native shared library for those using RSML in C, C++ or languages that don't have official bindings.
- A CLI for interacting with the RSML toolchain easily.
- A static website for evaluating RSML on the go.
- First-class support for Visual Studio.
- Syntax highlighting for Visual Studio Code, JetBrains IDEs, Notepad++ and more.

RSML solves the issue of resolving logic paths dynamically based on a given host's characteristics.
When assigned an host, which could be the local host, RSML solves the logic paths and returns the first match's
associated value.
The important part here is to note RSML does this dynamically: if you were to switch hosts or pass different data, RSML
would adapt accordingly.

**Still unsure about RSML?** You can find some usage examples [here](#examples).

---

## How to build RSML?
<details>
	<summary><strong>Debug build</strong></summary>

1. Install the [.NET SDK 10.0](https://dotnet.microsoft.com/download).
2. Clone [this repository](https://github.com/OceanApocalypse/RedSeaModernLanguage).
3. Open the terminal in the root RSML directory (the one with the solution file).

```
dotnet build -c Debug RedSeaModernLanguage.slnx
./src/RSML.CLI/bin/Debug/net10.0/RSML.CLI.exe
```

</details>

<details>
	<summary><strong>Optimized build</strong></summary>

1. Install the [.NET SDK 10.0](https://dotnet.microsoft.com/download).
2. Clone [this repository](https://github.com/OceanApocalypse/RedSeaModernLanguage).
3. Open the terminal in the root RSML directory (the one with the solution file).

```
dotnet build -c Release RedSeaModernLanguage.slnx
./src/RSML.CLI/bin/Release/net10.0/RSML.CLI.exe
```

</details>
<details open>
	<summary><strong>CLI Framework-dependent Build</strong> <em>(good if you want a lighter CLI that uses your installed .NET SDK)</em></summary>

1. Install the [.NET SDK 10.0](https://dotnet.microsoft.com/download).
2. Clone [this repository](https://github.com/OceanApocalypse/RedSeaModernLanguage).
3. Open the terminal in the root RSML directory (the one with the solution file).

```
dotnet publish -c Release -r <rid> src/RSML.CLI/RSML.CLI.csproj --no-self-contained true
```
</details>
<details open>
	<summary><strong>Native build</strong> <em>(compiles into a shared library)</em></summary>

1. Install the [.NET SDK 10.0](https://dotnet.microsoft.com/download).
2. Clone [this repository](https://github.com/OceanApocalypse/RedSeaModernLanguage).
3. Open the terminal in the root RSML directory (the one with the solution file).

```
dotnet publish -c Release -r <rid> src/RSML.Native/RSML.Native.csproj
```

Once that is complete, the DLL will be located at
`src/RSML.Native/bin/<arch>/Release/net10.0/<rid>/publish/RSML.Native.dll`. The XML files in the same directory are
purely for documentation purposes and the `.pdb` file only matters for debugging.

</details>

---

## Performance Analysis
We analyse performance to see what can be improved for general user experience. We use [BenchmarkDotNet](https://benchmarkdotnet.org), a reputable benchmarking framework for .NET code.

More data on performance coming soon.

---

## Supported Editors
RSML is supported in **Visual Studio** and **Visual Studio Code** via extensions.
Support for more code editors and IDEs is also part of our RSML roadmap!

For more information, see: <img src="https://raw.githubusercontent.com/vscode-icons/vscode-icons/refs/heads/master/icons/file_type_sln.svg" alt="Visual Studio Logo" height="20" width="20" style="vertical-align: middle;" /> <a href="https://oceanapocalypse.org/rsml-docs/docs/tools/editor-support/">Editor Support</a>.

---

## Supported Programming Languages
As of now, you can only use RSML's API in **.NET** languages, such as **C#** (recommended for maximum support), **Visual Basic** and **F#**.
Support for more programming languages, such as Python, is also part of our RSML roadmap!

For more information, see: <img src="https://raw.githubusercontent.com/vscode-icons/vscode-icons/refs/heads/master/icons/file_type_csharp.svg" alt="C# Logo" height="20" width="20" style="vertical-align: middle;" /> <a href="https://oceanapocalypse.org/rsml-docs/docs/bindings/list">Programming Language Support</a>.

> **Copyright 2025-2026 OceanApocalypse**
>
> We :heart: open-source!
