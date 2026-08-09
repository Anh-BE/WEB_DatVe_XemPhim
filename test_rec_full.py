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

def exec_cypher(query, params=None):
    stmt = {"statement": query}
    if params:
        stmt["parameters"] = params
    payload = json.dumps({"statements": [stmt]}).encode("utf-8")
    req = urllib.request.Request("http://localhost:7474/db/neo4j/tx/commit", data=payload, headers=headers)
    with urllib.request.urlopen(req) as resp:
        return json.loads(resp.read().decode("utf-8"))

# Test adding booking for leanh325 on Mùi Cỏ Cháy (Movie ID 42)
add_stmt = """
MERGE (u:User {userId: 'leanh325'}) ON CREATE SET u.username = 'Lê Anh'
MERGE (m:Movie {movieId: 42})
MERGE (u)-[:FAVORITE]->(m)
MERGE (u)-[:BOOKED { bookingId: 'BK9999', seatCount: 2, totalAmount: 180000, date: '2026-08-08' }]->(m)
"""
exec_cypher(add_stmt)
print("Added FAVORITE and BOOKED for leanh325 on Movie 42 (Mùi Cỏ Cháy).")

# Test Recommendation Query
rec_query = """
MATCH (u:User {userId: 'leanh325'})-[:FAVORITE|BOOKED]->(mFav:Movie)-[:BELONGS_TO]->(g:Genre)<-[:BELONGS_TO]-(rec:Movie)
WHERE NOT (u)-[:BOOKED]->(rec) AND mFav <> rec
OPTIONAL MATCH (uAll:User)-[r:BOOKED]->(rec)
WITH DISTINCT rec, g, COUNT(DISTINCT r) AS relCount, COALESCE(SUM(r.seatCount), 0) AS totalSeats
RETURN rec.movieId AS movieId, rec.title AS title, g.genreName AS genreName
ORDER BY rec.movieId ASC
LIMIT 4
"""

res = exec_cypher(rec_query)
print("\n--- Recommendation Results for leanh325 ---")
if res and res.get("results"):
    rows = res["results"][0]["data"]
    for r in rows:
        print(f"Movie ID {r['row'][0]}: {r['row'][1]} ({r['row'][2]})")
