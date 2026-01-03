$port = 53875
$body = @{
    model = "Phi-3.5-mini-instruct-onnx-cpu:1"
} | ConvertTo-Json
try {
    $response = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:$port/openai/download" -ContentType "application/json" -Body $body -Verbose
    $response | ConvertTo-Json
}
catch {
    Write-Host "Message: $($_.Exception.Message)"
    if ($_.ErrorDetails) { 
        $details = $_.ErrorDetails | ConvertFrom-Json
        $details | ConvertTo-Json
    }
}
