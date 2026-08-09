$auth = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("neo4j:adminpassword"))
$headers = @{ 
    "Authorization" = "Basic $auth"; 
    "Content-Type"  = "application/json" 
}

# Clear old data
$clearBody = @{ statements = @( @{ statement = "MATCH (n) DETACH DELETE n;" } ) } | ConvertTo-Json -Depth 5
Invoke-RestMethod -Uri "http://localhost:7474/db/neo4j/tx/commit" -Method Post -Headers $headers -Body $clearBody | Out-Null

Write-Host "Cleared Neo4j Database."
