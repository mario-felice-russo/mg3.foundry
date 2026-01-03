$body = @'
{
  "name": "Phi-3.5-mini-instruct-onnx-cpu:1",
  "providerType": "AzureFoundry"
}
'@
try {
    $response = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:55267/openai/download" -ContentType "application/json" -Body $body -Verbose
    $response | ConvertTo-Json
} catch {
    $_.Exception.Message
    $_.ErrorDetails.Message
}
