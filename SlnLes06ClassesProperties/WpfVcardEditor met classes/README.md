# WpfVcardEditor with Classes

## Changes Made to Fix Compilation Errors and Warnings

1. **Fixed .NET Framework Reference Error**
   - Kept only `<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>` which is correct for .NET Framework projects
   - Added a reference to `Microsoft.NETFramework.ReferenceAssemblies` package
   - Added a reference to `NuGet.Build.Tasks.Pack` package for improved NuGet compatibility
   - Added a standard App.config file with proper .NET Framework configuration
   - Set `<RuntimeIdentifiers>win</RuntimeIdentifiers>` to specify the Windows platform target
   - Created a `Microsoft.NuGet.targets` file to override problematic NuGet validation
   - Added `<SkipRuntimeIdentifierCheck>true</SkipRuntimeIdentifierCheck>` to disable the strict runtime check
   - Created a `Directory.Build.props` file at the solution level to enforce these settings
   - Added a custom `NuGet.config` file to control package restoration behavior

2. **Fixed Missing Property Files**
   - Created necessary files in the Properties folder:
     - `AssemblyInfo.cs` - Contains assembly metadata and version information
     - `Resources.Designer.cs` - Auto-generated resource manager code
     - `Resources.resx` - Resource file for storing application resources (fixed invalid XML issue)
     - `Settings.Designer.cs` - Auto-generated settings code 
     - `Settings.settings` - Application settings file

3. **Fixed Code Style Warnings**
   - Fixed SA1115 (parameter formatting) - Parameters now begin on the line after previous parameter
   - Fixed SA1009 (closing parenthesis) - Removed space before closing parenthesis
   - Fixed SA1111 (closing parenthesis positioning) - Closing parenthesis now on the same line as the last parameter

4. **Fixed C# Language Version Compatibility**
   - Replaced C# 8.0+ syntax with C# 7.3 compatible code
   - Removed range operator (`..`) and replaced with `Substring()`
   - Removed nullable reference type annotations (`?`)
   - Expanded target-typed new expressions (`new ()` to `new TypeName()`)

5. **Fixed Invalid Resource File**
   - Recreated the Resources.resx file with proper XML formatting to resolve "Invalid Resx file. Root element is missing" error

## Building the Project

This project should be built using Visual Studio with .NET Framework 4.7.2 support. The command-line build using `dotnet build` may not work due to compatibility issues between .NET SDK tools and .NET Framework projects.

To properly build the project:
1. Open the solution in Visual Studio
2. Right-click on the solution and select "Restore NuGet Packages"
3. Build the solution using the Build menu

## Implementation Details

- Created a `VCard` class with properties for all form controls
- Added two constructors: a default constructor and a parameterized constructor
- Implemented a `GenerateVcfCode()` method that returns the vCard format (.vcf) representation
- Updated the MainWindow class to use the VCard class for data management 