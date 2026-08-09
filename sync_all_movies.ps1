$auth = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("neo4j:adminpassword"))
$headers = @{ 
    "Authorization" = "Basic $auth"; 
    "Content-Type"  = "application/json" 
}

# 1. Clear database
$clearCypher = "MATCH (n) DETACH DELETE n;"
Invoke-RestMethod -Uri "http://localhost:7474/db/neo4j/tx/commit" -Method Post -Headers $headers -Body (@{ statements = @( @{ statement = $clearCypher } ) } | ConvertTo-Json -Depth 5) | Out-Null

Write-Host "Cleared Neo4j Database."

# 2. Seed Genres
$genres = @(
    @{ id = 1; name = "Hành động" },
    @{ id = 2; name = "Viễn tưởng" },
    @{ id = 3; name = "Tâm lý" },
    @{ id = 4; name = "Hoạt hình" },
    @{ id = 5; name = "Kinh dị" },
    @{ id = 6; name = "Bí ẩn" },
    @{ id = 7; name = "Tình cảm" },
    @{ id = 8; name = "Trinh thám" },
    @{ id = 9; name = "Gia đình" },
    @{ id = 10; name = "Hài hước" },
    @{ id = 11; name = "Lịch sử" },
    @{ id = 12; name = "Phiêu lưu" }
)

$statements = @()
foreach ($g in $genres) {
    $statements += @{ statement = "MERGE (g:Genre { genreId: $($g.id) }) SET g.genreName = '$($g.name)';" }
}

Invoke-RestMethod -Uri "http://localhost:7474/db/neo4j/tx/commit" -Method Post -Headers $headers -Body (@{ statements = $statements } | ConvertTo-Json -Depth 5) | Out-Null
Write-Host "Seeded 12 Genres in Neo4j."

# 3. Read Movies from SQL file and sync to Neo4j
$sqlContent = Get-Content "C:\Users\Le Ngoc Anh\Desktop\09_WebDatVeXemPhim\09_WebDatVeXemPhim\doan3\DatVeXemPhim.sql" -Encoding UTF8
$movieLines = $sqlContent | Where-Object { $_ -match "^\s*\(N'" }

$movieId = 1
$movieStatements = @()

foreach ($line in $movieLines) {
    if ($line -match "^\s*\(N'([^']+)',.*'([^']+\.png)',\s*N?'Dang Chieu',\s*([0-9]+)\)") {
        $title = $Matches[1].Replace("'", "\'")
        $poster = $Matches[2]
        $genreId = [int]$Matches[3]
        
        $cypher = "MERGE (m:Movie { movieId: $movieId }) SET m.title = '$title', m.poster = '$poster', m.duration = 120 WITH m MATCH (g:Genre { genreId: $genreId }) MERGE (m)-[:BELONGS_TO]->(g);"
        $movieStatements += @{ statement = $cypher }
        $movieId++
    }
}

Invoke-RestMethod -Uri "http://localhost:7474/db/neo4j/tx/commit" -Method Post -Headers $headers -Body (@{ statements = $movieStatements } | ConvertTo-Json -Depth 5) | Out-Null
Write-Host "Successfully synced $($movieStatements.Count) Movies from SQL Server to Neo4j Graph!"
