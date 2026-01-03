$body = "Phi-3.5-mini-instruct-onnx-cpu:1"
try {
    $response = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:55267/openai/download" -ContentType "application/json" -Body (ConvertTo-Json $body) -Verbose
    $response | ConvertTo-Json
} catch {
    Write-Host "Message: $($_.Exception.Message)"
    if ($_.ErrorDetails) { Write-Host "Details: $($_.ErrorDetails.Message)" }
    $_.ErrorDetails | ConvertTo-Json
}
