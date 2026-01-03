namespace mg3.foundry.Features.FoundryCore.Models;

public class CliModelInfo
{
    public string Alias { get; set; } = string.Empty;
    public string Device { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string ModelID { get; set; } = string.Empty;
    public bool IsCached { get; set; }
}
