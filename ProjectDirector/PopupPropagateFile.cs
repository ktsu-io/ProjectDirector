namespace ktsu.io.ProjectDirector;

using DiffPlex.Model;
using ImGuiNET;
using ktsu.io.Extensions;
using ktsu.io.ImGuiWidgets;
using ktsu.io.StrongPaths;

internal class PopupPropagateFile : PopupModal
{
	private ProjectDirectorOptions Options { get; set; } = new();
	private Dictionary<FullyQualifiedGitHubRepoName, bool> Propagation { get; } = new();
	private PopupPrompt Prompt { get; } = new();
	private bool ShouldClose { get; set; }

	public void Open(ProjectDirectorOptions options)
	{
		ShouldClose = false;
		Options = options;
		Propagation.Clear();
		base.Open("Propagate File");
	}

	public override void Open(string title) => throw new InvalidOperationException("Use Open(ProjectDirectorOptions) instead");

	protected override void ShowContent()
	{
		string normalizePath(string path) => path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		bool hasSimilarFile(KeyValuePair<FullyQualifiedGitHubRepoName, Dictionary<RelativeFilePath, DiffResult>> kvp) => kvp.Value.Any(x => normalizePath(x.Key) == normalizePath(Options.PropagatePath));

		var repo = Options.Repos[Options.BaseRepo];

		var sortedRepos = repo.SimilarRepoDiffs
			.ToDictionary(kvp => kvp.Key, hasSimilarFile)
			.OrderByDescending(kvp => kvp.Value);

		ImGui.TextUnformatted("Propagate file to other repos");
		ImGui.Separator();
		ImGui.TextUnformatted($"From: {Options.BaseRepo}");
		ImGui.TextUnformatted($"Path: {Options.PropagatePath}");
		ImGui.Separator();
		ImGui.TextUnformatted("Repos to propagate to:");
		foreach (var (name, similar) in sortedRepos)
		{
			bool shouldPropagate = Propagation.GetOrCreate(name, similar);
			ImGui.Checkbox($"{name}{(similar ? "*" : string.Empty)}", ref shouldPropagate);
			Propagation[name] = shouldPropagate;
		}
		ImGui.Separator();
		if (ImGui.Button("Propagate"))
		{
			int propagationCount = Propagation.Count(kvp => kvp.Value);
			Prompt.Open("Propagation", $"Are you sure you want to propagate {Options.PropagatePath} to {propagationCount} repos?", new()
			{
				{ "Yes", Propagate },
				{ "NO", null }
			});
		}

		if (ShouldClose)
		{
			ImGui.CloseCurrentPopup();
		}

		Prompt.ShowIfOpen();
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:Use expression body for method", Justification = "<Pending>")]
	private void Propagate()
	{
		//var repo = Options.Repos[Options.BaseRepo];
		//string from = Path.Combine(repo.LocalPath, Options.PropagatePath);
		//foreach (var (name, shouldPropagate) in Propagation)
		//{
		//	if (shouldPropagate)
		//	{
		//		var otherRepo = Options.Repos[name];
		//		string to = Path.Combine(otherRepo.LocalPath, Options.PropagatePath);
		//		string? directory = Path.GetDirectoryName(to);
		//		if (!string.IsNullOrEmpty(directory))
		//		{
		//			Directory.CreateDirectory(directory);
		//			File.Copy(from, to, overwrite: true);
		//		}
		//	}
		//}

		ShouldClose = true;
	}
}