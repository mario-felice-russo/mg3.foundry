# FoundryServiceCli

The `FoundryServiceCli` acts as a wrapper around the `foundry` command-line interface. It is primarily used for administrative tasks and model lifecycle management (download, delete, list).

## Key Features

- **CLI-Centric**: Directly executes `foundry` commands using `ProcessStartInfo`.
- **Output Parsing**: Includes specialized logic to parse CLI table outputs into structured C# models.
- **Progress Reporting**: Supports standard `IProgress<double>` for long-running operations like model downloads.

## Main Methods

### `IsFoundryInstalledAsync()`
Checks if the `foundry` CLI is available in the system path by running `--version`.
- **Returns**: `Task<bool>`

### `GetAvailableModelsAsync()` / `GetCachedModelsAsync()`
Parses the output of `foundry model list` and `foundry cache list` respectively.
- **Returns**: `Task<List<CliModelInfo>>`

### `DownloadModelAsync(string modelName, IProgress<double> progress)`
Triggers a model download and reports percentage progress parsed from the CLI stream.
- **Returns**: `Task<bool>`

### `RunModelChatAsync(string modelName, string prompt)`
Executes `foundry model run` with a specific prompt, capturing the immediate CLI response.
- **Returns**: `Task<string>`

### `StartModelServerAsync(string modelName, int port)`
Helper to start the REST API server for a specific model via the CLI.
- **Returns**: `Task<ProcessResult>`

## Implementation Details
The service uses `StringBuilder` to capture standard output and error streams, and uses Regex to identify progress tokens (e.g., `85%`) during downloads.
