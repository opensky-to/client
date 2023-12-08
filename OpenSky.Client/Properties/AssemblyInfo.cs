// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
#if DEBUG
using System.Windows.Markup;
#endif

[assembly: AssemblyTitle("OpenSky.Client")]
[assembly: AssemblyDescription("OpenSky airline management game client")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("OpenSky")]
[assembly: AssemblyProduct("OpenSky")]
[assembly: AssemblyCopyright("OpenSky project 2021-2023")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: AssemblyVersion("0.5.5")]
[assembly: AssemblyFileVersion("0.5.5")]

// This allows us to detect debug mode in XAML
#if DEBUG
[assembly: XmlnsDefinition("debug-mode", "Namespace")]
#endif