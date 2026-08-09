import re
import json
import urllib.request
import base64
import sys

sys.stdout.reconfigure(encoding='utf-8')

auth_str = base64.b64encode(b"neo4j:adminpassword").decode("utf-8")
headers = {
    "Authorization": f"Basic {auth_str}",
    "Content-Type": "application/json"
}

def exec_cypher(statements):
    payload = json.dumps({"statements": [{"statement": s} for s in statements]}).encode("utf-8")
    req = urllib.request.Request("http://localhost:7474/db/neo4j/tx/commit", data=payload, headers=headers)
    try:
        with urllib.request.urlopen(req) as resp:
            res = json.loads(resp.read().decode("utf-8"))
            if res.get("errors"):
                print("Neo4j Errors:", res["errors"])
            return res
    except Exception as e:
        print("HTTP Error:", e)
        return None

# 1. Clear database
exec_cypher(["MATCH (n) DETACH DELETE n;"])
print("Cleared Neo4j Database.")

# 2. Seed Genres
genres = [
    (1, "Hành động"), (2, "Viễn tưởng"), (3, "Tâm lý"), (4, "Hoạt hình"),
    (5, "Kinh dị"), (6, "Bí ẩn"), (7, "Tình cảm"), (8, "Trinh thám"),
    (9, "Gia đình"), (10, "Hài hước"), (11, "Lịch sử"), (12, "Phiêu lưu")
]
genre_stmts = [f"MERGE (g:Genre {{ genreId: {gid} }}) SET g.genreName = '{name}';" for gid, name in genres]
exec_cypher(genre_stmts)
print("Seeded 12 Genres.")

# 3. Read SQL file
with open(r"C:\Users\Le Ngoc Anh\Desktop\09_WebDatVeXemPhim\09_WebDatVeXemPhim\doan3\DatVeXemPhim.sql", "r", encoding="utf-8") as f:
    sql_text = f.read()

pattern = r"\(N'([^']+)',\s*N'([^']*)',\s*'([^']*)',\s*(\d+),\s*N'([^']*)',\s*N'([^']*)',\s*'([^']*)',\s*N?'Dang Chieu',\s*(\d+)\)"
matches = re.findall(pattern, sql_text)

movie_stmts = []
for i, m in enumerate(matches, start=1):
    title = m[0].replace("'", "\\'")
    poster = m[6].replace("'", "\\'")
    duration = int(m[3])
    genre_id = int(m[7])
    
    stmt = (
        f"MERGE (m:Movie {{ movieId: {i} }}) "
        f"SET m.title = '{title}', m.poster = '{poster}', m.duration = {duration} "
        f"WITH m MATCH (g:Genre {{ genreId: {genre_id} }}) "
        f"MERGE (m)-[:BELONGS_TO]->(g);"
    )
    movie_stmts.append(stmt)

exec_cypher(movie_stmts)
print(f"Successfully synced {len(movie_stmts)} Movies to Neo4j Graph Database!")

# 4. Check Genre 11
res = exec_cypher(["MATCH (m:Movie)-[:BELONGS_TO]->(g:Genre {genreId: 11}) RETURN m.movieId AS id, m.title AS title;"])
if res and res.get("results"):
    rows = res["results"][0]["data"]
    print("\n--- Movies in Genre 11 (Lịch sử) in Neo4j ---")
    for r in rows:
        print(f"Movie ID {r['row'][0]}: {r['row'][1]}")
