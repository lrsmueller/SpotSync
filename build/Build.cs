using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.Docker.DockerTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[GitHubActions(
    "docker-publish",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] { "main" },
    InvokedTargets = new[] { nameof(PublishDocker) }
)]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath OutputDirectory => RootDirectory / "bin";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Docker Image Name")]
    readonly string DockerImageName = "SpotSync";

    [Parameter("Docker Image Tag")]
    readonly string DockerImageTag = "latest";

    [Solution] readonly Solution Solution;

    GitHubActions GitHubActions => GitHubActions.Instance;

    Target Clean => _ => _
        .Executes(() =>
        {
            OutputDirectory.CreateOrCleanDirectory();
            
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });


    Target PublishDocker => _ => _
    .DependsOn(Compile)
    .Executes(() =>
    {
        DockerBuild(x => x
            .SetPath(".")
            .SetFile($"{DockerImageName}/Dockerfile")
            .SetTag($"{DockerImageName}:{DockerImageTag}"));

        DockerLogin(x => x
            .SetUsername(GitHubActions.Actor)
            .SetPassword(GitHubActions.Token)
            .SetServer("ghcr.io"));

        DockerPush(x => x
            .SetName($"ghcr.io/{DockerImageName}:{DockerImageTag}"));
    });

}
