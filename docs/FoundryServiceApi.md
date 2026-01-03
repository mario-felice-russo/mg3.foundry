# FoundryServiceApi

The `FoundryServiceApi` is the primary service used for interacting with the Microsoft Foundry Local REST API. It is designed to be highly adaptive, handling the dynamic nature of the Foundry service on Windows.

## Key Features

- **Dynamic Discovery**: Automatically resolves the active API endpoint by executing `foundry service status` and parsing the output.
- **OpenAI Compatibility**: Leverages the `Azure.AI.OpenAI` SDK for standardized chat completions and streaming.
- **Lazy Initialization**: Discovery and initialization occur on the first service call to ensure the endpoint is active when needed.

## Main Methods

### `GetCatalogAsync()`
Retrieves the full catalog of models available for download from the Azure Foundry.
- **Returns**: `Task<List<FoundryModelInfo>>`

### `GetActiveModelsAsync()`
Lists all models currently loaded or available in the local inference engine.
- **Returns**: `Task<List<V1ModelInfo>>`

### `DownloadModelAsync(FoundryModelInfo model, IProgress<double> progress = null)`
Downloads a model to the local cache using a streaming request. 
- **Parameters**: 
  - `model`: `FoundryModelInfo` object from the catalog.
  - `progress`: Optional `IProgress<double>` for real-time percentage updates.
- **Features**: 
  - **Payload Sanitization**: Automatically corrects provider types and nullifies complex objects to avoid server-side conversion errors.
  - **Streaming**: Uses `ResponseHeadersRead` to maintain a persistent connection, allowing real-time progress parsing from the JSON stream.
- **Returns**: `Task<bool>`

### `DeleteModelAsync(string modelId)`
Deletes a model from the local cache using the OpenAI standard endpoint `v1/models/{modelId}`.
- **Parameters**: `modelId` (e.g., `phi-3:1`).
- **Returns**: `Task<bool>`

### `CreateChatCompletionAsync(ChatCompletionRequest request)`
Sends a chat completion request to the active model using the OpenAI SDK.
- **Parameters**: `ChatCompletionRequest` (contains model ID and message list).
- **Returns**: `Task<ChatCompletionResponse?>`

### `CreateChatCompletionStreamAsync(ChatCompletionRequest request)`
Initiates a streaming chat completion.
- **Parameters**: `ChatCompletionRequest`.
- **Returns**: `IAsyncEnumerable<ChatCompletionChunk>`

### `GetStatusAsync()`
Checks the current health and status of the Foundry local service via the `openai/status` endpoint.
- **Returns**: `Task<FoundryStatus?>`
- **Output Sample**: Includes `endpoints`, `modelDirPath`, and registration status.

## Data Models

### `FoundryModelInfo`
Represents model metadata from the Azure Foundry catalog.
- **Properties**: `Name`, `DisplayName`, `ProviderType`, `Uri`, `Version`, `ModelType`, `Architecture`, `FileSize`, `Publisher`, `Runtime`, `Task`, `ParameterSize`, `PromptTemplate`.

### `DownloadModelRequest`
Envelope used for model download requests to the `openai/download` endpoint.
- **Fields**: `model` (FoundryModelInfo), `ignorePipeReport` (defaults to true), `bufferSize`, `customDirPath`, `token`.

## Configuration
The service defaults to `http://localhost:55267` if discovery fails, but it is primarily designed to find the port dynamically.
