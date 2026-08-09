$auth = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("neo4j:adminpassword"))
$headers = @{ 
    "Authorization" = "Basic $auth"; 
    "Content-Type"  = "application/json" 
}

$cypher1 = "MATCH (u:User)-[r]->(m:Movie)-[:BELONGS_TO]->(g:Genre) RETURN u.userId AS user, type(r) AS rel, m.movieId AS movieId, m.title AS title, g.genreName AS genre;"
$body1 = @{ statements = @( @{ statement = $cypher1 } ) } | ConvertTo-Json -Depth 5

try {
    $res = Invoke-RestMethod -Uri "http://localhost:7474/db/neo4j/tx/commit" -Method Post -Headers $headers -Body $body1
    Write-Host "Neo4j User Interactions:"
    $res.results[0].data | ForEach-Object { Write-Host ($_.row -join " | ") }
} catch {
    Write-Host "Error: $_"
}
