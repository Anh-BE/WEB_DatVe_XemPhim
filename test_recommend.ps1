$auth = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("neo4j:adminpassword"))
$headers = @{ 
    "Authorization" = "Basic $auth"; 
    "Content-Type"  = "application/json" 
}

$cypher1 = "MATCH (u:User {userId: 'leanh325'})-[:FAVORITE|BOOKED]->(mFav:Movie)-[:BELONGS_TO]->(g:Genre)<-[:BELONGS_TO]-(rec:Movie) WHERE NOT (u)-[:BOOKED]->(rec) AND mFav <> rec RETURN rec.movieId AS movieId, rec.title AS title, g.genreName AS genreName;"
$body1 = @{ statements = @( @{ statement = $cypher1 } ) } | ConvertTo-Json -Depth 5

try {
    $res = Invoke-RestMethod -Uri "http://localhost:7474/db/neo4j/tx/commit" -Method Post -Headers $headers -Body $body1
    Write-Host "Same genre recommendation result:"
    $res.results[0].data | ForEach-Object { Write-Host ($_.row -join " | ") }
} catch {
    Write-Host "Error: $_"
}
