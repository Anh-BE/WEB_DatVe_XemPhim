$auth = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("neo4j:adminpassword"))
$headers = @{ 
    "Authorization" = "Basic $auth"; 
    "Content-Type"  = "application/json" 
}

$cypher = "MATCH (u:User) WHERE u.userId = 'leanh325' OR u.username = 'leanh325' DETACH DELETE u;"
$body = @{ statements = @( @{ statement = $cypher } ) } | ConvertTo-Json -Depth 5

try {
    $res = Invoke-RestMethod -Uri "http://localhost:7474/db/neo4j/tx/commit" -Method Post -Headers $headers -Body $body
    Write-Host "Success: $($res | ConvertTo-Json -Depth 5)"
} catch {
    Write-Host "Error: $_"
}
