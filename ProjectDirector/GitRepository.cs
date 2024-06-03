namespace ktsu.io.ProjectDirector;

using System.Text.Json.Serialization;
using DiffPlex.Model;
using ktsu.io.StrongPaths;
using LibGit2Sharp;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[JsonDerivedType(typeof(GitHubRepository), nameof(GitHubRepository))]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "TypeName")]
public abstract class GitRepository
{
	public int MinFetchIntervalSeconds { get; set; } = 60;
	public DateTime LastFetchTime { get; set; } = DateTime.MinValue;

	public GitRemotePath RemotePath { get; set; } = new();
	public FullyQualifiedLocalRepoPath LocalPath { get; set; } = new();

	public bool IsCloned => Directory.Exists(LocalPath);

	[JsonIgnore]
	public Dictionary<FullyQualifiedGitHubRepoName, Dictionary<RelativeFilePath, DiffResult>> SimilarRepoDiffs { get; } = [];

	public static GitRepository? Create(GitRemotePath remotePath, FullyQualifiedLocalRepoPath localPath)
	{
		return GitHubRepository.IsRemotePathValid(remotePath)
			? (GitRepository)new GitHubRepository
			{
				RemotePath = remotePath,
				LocalPath = localPath
			}
			: null;
	}

	public bool IsDirty { get; private set; }
	public bool IsOutOfDate { get; private set; }

	internal void UpdateStatus()
	{
		IsDirty = false;
		IsOutOfDate = false;

		using var repo = new Repository(LocalPath);
		var status = repo.RetrieveStatus();
		IsDirty = status.IsDirty;
		// work out if the repository is behind the remote
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
