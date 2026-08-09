$auth = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("neo4j:adminpassword"))
$headers = @{ 
    "Authorization" = "Basic $auth"; 
    "Content-Type"  = "application/json" 
}

# 1. Clean up duplicate BELONGS_TO relationships where genreId = 1 was wrongly added to movies having other genres
$cypher1 = "MATCH (m:Movie)-[r:BELONGS_TO]->(g:Genre {genreId: 1}) WHERE EXISTS { MATCH (m)-[:BELONGS_TO]->(g2:Genre) WHERE g2.genreId <> 1 } DELETE r;"

# 2. Synchronize all movies' BELONGS_TO from SQL database
$body1 = @{ statements = @( @{ statement = $cypher1 } ) } | ConvertTo-Json -Depth 5
try {
    $res = Invoke-RestMethod -Uri "http://localhost:7474/db/neo4j/tx/commit" -Method Post -Headers $headers -Body $body1
    Write-Host "Cleanup Result: $($res | ConvertTo-Json -Depth 5)"
} catch {
    Write-Host "Error: $_"
}
