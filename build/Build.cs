using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MinVer;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

class Build : NukeBuild
{
    public AbsolutePath SourceDirectory { get; } = RootDirectory / "src";
    public AbsolutePath TestDirectory { get; } = RootDirectory / "tests";
    public AbsolutePath PublishDirectory { get; } = RootDirectory / "publish";
    public AbsolutePath PackageDirectory { get; } = RootDirectory / "package";
    [Solution]
    private readonly Solution Solution;
    [MinVer]
    readonly MinVer MinVer;
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory
                .GlobDirectories("**/{obj,bin}")
                .DeleteDirectories();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            Log.Debug($"RootDirectory: {RootDirectory}");
            Log.Debug($"Solution: {Solution}");
            DotNetTasks.DotNet($"restore {Solution}");
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNet($"build {Solution}");
        });

    Target Publish => _ => _
        .After(Compile)
        .Executes(() =>
        {
            SourceDirectory.GlobFiles("*.csproj")
            .ForEach(proj =>
            {
                DotNetTasks.DotNet($"publish {proj} -o {PublishDirectory / MinVer.Version} --no-restore");
            });
        });

    Target Pack => _ => _
        .After(Compile)
        .Executes(() =>
        {
            DotNetTasks.DotNetPack(_ => _
                .SetProject(Solution.GetProject("SpectreHelpers"))
                .SetConfiguration(Configuration.Debug)
                .EnableNoBuild()
                .EnableNoRestore()
                .SetDescription("Small library of extension methods for Spectre.Console")
                .SetPackageTags("spectre.console")
                .SetOutputDirectory(RootDirectory / "packages")
            );
        });

    Target Print => _ => _
        .Executes(() =>
        {
            Log.Information("MinVer.Version = {Value}", MinVer.Version);
            Log.Information("MinVer.MinVerPreRelease = {Value}", MinVer.MinVerPreRelease);
            Log.Information("MinVer.MinVerPatch = {Value}", MinVer.MinVerPatch);
            Log.Information("MinVer.AssemblyVersion = {Value}", MinVer.AssemblyVersion);
            Log.Information("MinVer.Version = {Value}", MinVer.Version);
            Log.Information("MinVer.Version = {Value}", MinVer.MinVerVersion);
        });



}
