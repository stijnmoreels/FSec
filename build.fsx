#load "./.fake/build.fsx/intellisense.fsx"

open Fake
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.DotNet.PaketTemplate
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators

type Project =
    { Name : string
      Summary : string
      Description : string list
      GitHubUrl : string
      NuGetUrl : string
      Authors : string list
      Guid : string }

let projectBase = 
  { Name = "FSecurity" 
    Summary = "FSecuriy is a tool for automatically running security tests for .NET programs"
    Description = 
      [ "FSec is a tool for automatically running security tests for .NET programs. "
        "The tool provides some basic security testing functionality to discover vulnerabilities." ]
    GitHubUrl = "https://github.com/stijnmoreels/FSecurity"
    NuGetUrl = "http://nuget.org/packages/FSecurity"
    Authors = [ "Stijn Moreels" ]
    Guid = "871111ca-f7e3-48c5-95b1-6eec4c289948" }

let projectApi = 
  { Name = "FSecurity.Api" 
    Summary = "FSecuriy is a tool for automatically running API security tests for .NET programs"
    Description = 
      [ "FSec is a tool for automatically running API security tests for .NET programs. "
        "The tool provides some basic security testing functionality to discover vulnerabilities." ]
    GitHubUrl = "https://github.com/stijnmoreels/FSecurity"
    NuGetUrl = "http://nuget.org/packages/FSecurity.Api"
    Authors = [ "Stijn Moreels" ]
    Guid = "871111ca-f7e3-48c5-95b1-6eec4c289949" }

let projectFsCheck = 
  { Name = "FSecurity.FsCheck" 
    Summary = "FSecuriy is a tool for automatically running FsCheck security tests for .NET programs"
    Description = 
      [ "FSec is a tool for automatically running FsCheck security tests for .NET programs. "
        "The tool provides some basic security testing functionality to discover vulnerabilities." ]
    GitHubUrl = "https://github.com/stijnmoreels/FSecurity"
    NuGetUrl = "http://nuget.org/packages/FSecurity.FsCheck"
    Authors = [ "Stijn Moreels" ]
    Guid = "871111ca-f7e3-48c5-95b1-6eec4c289950" }

let projects = [ projectBase; projectApi; projectFsCheck ]
let solution = projectBase.Name + ".sln"
let releaseNotes = ReleaseNotes.load "RELEASE_NOTES.md"
let dotnetExePath = "dotnet"

Target.create "Clean" <| fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj"
    ++ "tests/**/bin"
    ++ "tests/**/obj"
    ++ "docs"
    ++ "bin"
    |> Shell.cleanDirs

Target.create "Build" <| fun _ ->
  for project in projects do
    AssemblyInfoFile.createFSharp ("src/" + project.Name +  "/AssemblyInfo.fs")
      [ AssemblyInfo.Title project.Name
        AssemblyInfo.Description  projectBase.Summary
        AssemblyInfo.Guid project.Guid
        AssemblyInfo.Product project.Name
        AssemblyInfo.Version releaseNotes.NugetVersion
        AssemblyInfo.FileVersion releaseNotes.NugetVersion ]

  solution 
  |> DotNet.restore id

  !! "src/**/*.fsproj"
  ++ "tests/**/*.fsproj"
  |> Seq.iter (DotNet.build (fun defaults ->
      { defaults with 
          Configuration = DotNet.BuildConfiguration.Release }))

Target.create "Tests" <| fun _ ->
    let runTest project =
        DotNet.exec (DotNet.Options.withDotNetCliPath dotnetExePath)
             ("run --project " + project)
             "--summary"
        |> fun r -> if not r.OK then failwith (sprintf "%s failed: %A" project r.Errors)
    
    runTest (sprintf "tests/%s.Tests/%s.Tests.fsproj" projectBase.Name projectBase.Name)

Target.create "Paket" <| fun _ ->
    for project in projects do
      let templateFile = "src" @@ project.Name @@ "paket.template"
      let referencesFile = "src" @@ project.Name @@ "paket.references"
      PaketTemplate.create (fun defaults ->
        { defaults with 
            TemplateFilePath = Some templateFile
            TemplateType = PaketTemplate.PaketTemplateType.File
            Id = Some project.Name
            Version = Some releaseNotes.NugetVersion
            Description = project.Description
            Title = Some project.Name
            Authors = project.Authors
            Owners = project.Authors
            ReleaseNotes = releaseNotes.Notes
            Summary = [ project.Summary ]
            ProjectUrl = Some project.GitHubUrl
            LicenseUrl = Some (project.GitHubUrl + "/blob/master/LICENSE.txt")
            IconUrl = Some "https://raw.githubusercontent.com/stijnmoreels/FSecurity/master/docs/img/logo.png"
            Copyright = Some "Copyright 2020"
            Tags = [ "fsharp"; "security"; "testing"; "tool"; "automation"; "tests"; "ci"; "owasp"; "api"; "web" ]
            Files = 
                [ PaketFileInfo.Include ("bin/Release/netstandard2.0/*.dll", "lib/netstandard2.0")
                  PaketFileInfo.Include ("bin/Release/netstandard2.0/*.xml", "lib/netstandard2.0") ]
            Dependencies = 
                Paket.getDependenciesForReferencesFile referencesFile
                |> Array.map (fun (package, version) -> PaketDependency (package, GreaterOrEqual (Version version)))
                |> List.ofArray })

      Paket.pack (fun defaults ->
        { defaults with
            OutputPath = "bin"
            WorkingDir = "."
            TemplateFile = templateFile })


Target.create "Docs" <| fun _ ->
  let content    = __SOURCE_DIRECTORY__ @@ "docsrc/content"
  let docsOutput = __SOURCE_DIRECTORY__ @@ "docs"
  let files      = __SOURCE_DIRECTORY__ @@ "docsrc/files"
  let templates  = __SOURCE_DIRECTORY__ @@ "docsrc/tools/templates"
  let formatting = __SOURCE_DIRECTORY__ @@ "packages/formatting/FSharp.Formatting"
  let docTemplate = formatting @@ "templates/docpage.cshtml"
  let root = "/" + projectBase.Name
  let info =
    [ "project-name", projectBase.Name
      "project-author", projectBase.Authors |> List.head
      "project-summary", projectBase.Summary
      "project-github", projectBase.GitHubUrl
      "project-nuget", projectBase.NuGetUrl ]
          
  let layoutRootsAll = System.Collections.Generic.Dictionary<string, string list>()
  layoutRootsAll.Add("en", [ templates
                             formatting @@ "templates"
                             formatting @@ "templates/reference" ])

  File.delete "docsrc/content/release-notes.md"
  Shell.copyFile "docsrc/content/" "RELEASE_NOTES.md"
  Shell.rename "docsrc/content/release-notes.md" "docsrc/content/RELEASE_NOTES.md"

  File.delete "docsrc/content/license.md"
  Shell.copyFile "docsrc/content/" "LICENSE.txt"
  Shell.rename "docsrc/content/license.md" "docsrc/content/LICENSE.txt"

  DirectoryInfo.getSubDirectories (DirectoryInfo.ofPath templates)
  |> Seq.iter (fun d ->
      let name = d.Name
      if name.Length = 2 || name.Length = 3 
      then layoutRootsAll.Add(name, [ templates @@ name
                                      formatting @@ "templates"
                                      formatting @@ "templates/reference" ]))
  Shell.copyRecursive files docsOutput true
  |> Trace.logItems "Copying file: "
  Directory.ensure (docsOutput @@ "content")
  Shell.copyRecursive (formatting @@ "styles") (docsOutput @@ "content") true
  |> Trace.logItems "Copying styles and scripts: "

  let langSpecificPath (lang, path:string) =
    path.Split([|'/'; '\\'|], System.StringSplitOptions.RemoveEmptyEntries)
    |> Array.exists(fun i -> i = lang)
  
  let layoutRoots =
    let key = layoutRootsAll.Keys |> Seq.tryFind (fun i -> langSpecificPath(i, content))
    match key with
    | Some lang -> layoutRootsAll.[lang]
    | None -> layoutRootsAll.["en"]

  Directory.ensure (docsOutput @@ "reference")
  !! ("src/**/bin/Release/**/*.dll") 
  |> FSFormatting.createDocsForDlls (fun args ->
      { args with
          OutputDirectory = docsOutput @@ "reference"
          LayoutRoots =  layoutRootsAll.["en"]
          ProjectParameters =  ("root", root) :: info
          SourceRepository = projectBase.GitHubUrl @@ "tree/master" })

  FSFormatting.createDocs (fun args ->
    { args with
        Source = content
        OutputDirectory = docsOutput
        LayoutRoots = layoutRoots
        ProjectParameters  = ("root", root) :: info
        Template = docTemplate })


Target.create "All" ignore

"Clean"
==> "Build"
==> "Tests"
==> "Docs"
==> "Paket"
==> "All"

Target.runOrDefault "All"
