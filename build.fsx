#I "Src/packages/FAKE/tools"
#r "FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.EnvironmentHelper
open Fake.MSBuildHelper
open Fake.NuGetHelper
open Fake.Testing.XUnit2

// properties
let semanticVersion = "5.0.4.2-alpha"
let version = (>=>) @"(?<major>\d*)\.(?<minor>\d*)\.(?<build>\d*).(?<fork>\d*)" "${major}.${minor}.${build}.${fork}" semanticVersion
let configuration = getBuildParamOrDefault "configuration" "Release"
// can be set by passing: -ev deployToNuGet true
let deployToNuGet = getBuildParamOrDefault "deployToNuGet" "false"
let genDir = "Gen/"
let docDir = "Doc/"
let srcDir = "Src/"
let testDir = genDir @@ "Test"
let nugetDir = genDir @@ "NuGet"
let nugetSource = "https://www.nuget.org/api/v2/package"
let projectName = "PCLMock"
let solutionPath = srcDir @@ projectName + ".sln"

Target "Restore" (fun _ ->
    DotNetCli.Restore (fun p ->
        { p with
            Project = solutionPath
        })
)

Target "Build" (fun _ ->
    // generate the shared assembly info
    CreateCSharpAssemblyInfoWithConfig (srcDir @@ "AssemblyInfoCommon.cs")
        [
            Attribute.Version version
            Attribute.FileVersion version
            Attribute.Configuration configuration
            Attribute.Company "Kent Boogaart"
            Attribute.Product "PCLMock"
            Attribute.Copyright "� Copyright. Kent Boogaart."
            Attribute.Trademark ""
            Attribute.Culture ""
            Attribute.StringAttribute("NeutralResourcesLanguage", "en-US", "System.Resources")
            Attribute.StringAttribute("AssemblyInformationalVersion", semanticVersion, "System.Reflection")
        ]
        (AssemblyInfoFileConfig(false))
      
    DotNetCli.Build (fun p ->
    { p with
        Project = solutionPath
        Configuration = configuration
        AdditionalArgs =
        [
            "/property:Version=" + semanticVersion
        ]
    })
)

Target "Test" (fun _ ->
    let unitTestProjectDir = srcDir @@ projectName + ".UnitTests"
    let result = Shell.Exec("dotnet", "xunit -configuration " + configuration, unitTestProjectDir)

    if result <> 0 then failwithf "Tests failed with exit code %d" result
)

Target "Push" (fun _ ->
    let packagePath = srcDir @@ projectName @@ "bin" @@ configuration @@ projectName + "." + semanticVersion + ".nupkg"
    let result = Shell.Exec("dotnet", "nuget push " + packagePath + " --source " + nugetSource)

    if result <> 0 then failwithf "Push failed with exit code %d" result
)

Target "All"
    DoNothing

// build order. Pass "-ev push true" to build script to push to NuGet
"Restore"
    ==> "Build"
    ==> "Test"
    =?> ("Push", getEnvironmentVarAsBool "push")
    ==> "All"

RunTargetOrDefault "All"
