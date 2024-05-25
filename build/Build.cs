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
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Polly;
using Serilog;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.Docker.DockerTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.GitHub.GitHubTasks;

[GitHubActions(
    "docker-publish",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = ["main"],
    InvokedTargets = [nameof(PublishDocker)],
    EnableGitHubToken = true,
    WritePermissions = [GitHubActionsPermissions.Packages,GitHubActionsPermissions.IdToken],
    ReadPermissions = [GitHubActionsPermissions.Contents]
)]
class Build : NukeBuild
{
    public Build()
    {
        DockerLogger = (_, message) => Log.Debug(message);
        GitLogger = (_, message) => Log.Debug(message);
    }

    public static int Main() => Execute<Build>(x => x.Compile);

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath OutputDirectory => RootDirectory / "bin";

    [GitRepository] readonly GitRepository Repository;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    //[Parameter("Docker Image Name")]
    string DockerImageName => Repository.GetGitHubName().ToLowerInvariant();

    readonly string DockerImageTag = "latest";
    
    string BaseImageName => $"{DockerImageName}:{DockerImageTag}";

    readonly string DockerRegistry = "ghcr.io";

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

        Policy
                .Handle<Exception>()
                .WaitAndRetry(5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, _, retryCount, _) =>
                    {
                        Log.Warning($"Docker login exited with code: '{ex}'");
                        Log.Information($"Attempting to login into GitHub Docker image registry. Try #{retryCount}");
                    })
                .Execute(() => DockerLogin(settings => settings
                    .SetServer(DockerRegistry)
                    .SetUsername(GitHubActions.Actor)
                    .SetPassword(GitHubActions.Token)
                    .DisableProcessLogOutput()));


        var DockerFile = SourceDirectory / "SpotSync/Dockerfile";



        DockerBuild(x => x
            .SetPath(".")
            .SetFile(DockerFile)
            .SetTag(BaseImageName)
            .DisableProcessLogOutput());

        var repositoryOwner = Repository.GetGitHubOwner().ToLowerInvariant();
        var repositoryName = Repository.GetGitHubName().ToLowerInvariant();
        var targetImageName = $"{DockerRegistry}/{repositoryOwner}/{repositoryName}/{DockerImageName}:{DockerImageTag}";

        DockerTag(settings => settings
                .SetSourceImage(BaseImageName)
                .SetTargetImage(targetImageName));

        DockerPush(x => x.SetName(targetImageName));
    });

}
