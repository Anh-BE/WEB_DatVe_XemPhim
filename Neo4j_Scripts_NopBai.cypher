// =========================================================================
// KỊCH BẢN (SCRIPTS) NEO4J GRAPH DATABASE - DỰ ÁN WEB ĐẶT VÉ XEM PHIM
// Môn học: Các hệ CSDL NoSQL (Đồ án Cuối kỳ)
// Yêu cầu: Tối thiểu 2 Node Types (:User, :Movie, :Genre) & 3 Relationship Types (:BOOKED, :FAVORITE, :BELONGS_TO)
// =========================================================================

// -------------------------------------------------------------------------
// BƯỚC 1: XÓA DỮ LIỆU CŨ & TẠO RÀNG BUỘC (CONSTRAINTS / INDEXES)
// -------------------------------------------------------------------------
// Xóa toàn bộ dữ liệu thử nghiệm cũ (dùng cẩn thận)
MATCH (n) DETACH DELETE n;

// Tạo chỉ mục độc nhất cho các Node
CREATE CONSTRAINT FOR (u:User) REQUIRE u.userId IS UNIQUE;
CREATE CONSTRAINT FOR (m:Movie) REQUIRE m.movieId IS UNIQUE;
CREATE CONSTRAINT FOR (g:Genre) REQUIRE g.genreId IS UNIQUE;

// -------------------------------------------------------------------------
// BƯỚC 2: KHỞI TẠO CÁC NODE THỂ LOẠI (GENRE NODES)
// -------------------------------------------------------------------------
CREATE (g1:Genre { genreId: 1, genreName: "Hành Động" });
CREATE (g2:Genre { genreId: 2, genreName: "Viễn Tưởng" });
CREATE (g3:Genre { genreId: 3, genreName: "Tình Cảm" });
CREATE (g4:Genre { genreId: 4, genreName: "Kinh Dị" });
CREATE (g5:Genre { genreId: 5, genreName: "Hoạt Hình" });

// -------------------------------------------------------------------------
// BƯỚC 3: KHỞI TẠO CÁC NODE BỘ PHIM (MOVIE NODES)
// -------------------------------------------------------------------------
CREATE (m1:Movie { movieId: 1, title: "Avatar 3: Fire and Ash", poster: "/Images/avatar3.jpg", duration: 180 });
CREATE (m2:Movie { movieId: 2, title: "Avengers: Secret Wars", poster: "/Images/avengers.jpg", duration: 165 });
CREATE (m3:Movie { movieId: 3, title: "Conan Movie 27", poster: "/Images/conan.jpg", duration: 110 });
CREATE (m4:Movie { movieId: 4, title: "Mai - Trấn Thành", poster: "/Images/mai.jpg", duration: 130 });
CREATE (m5:Movie { movieId: 5, title: "Quật Mộ Trùng Phùng (Exhuma)", poster: "/Images/exhuma.jpg", duration: 134 });
CREATE (m6:Movie { movieId: 6, title: "Dune: Part Two", poster: "/Images/dune2.jpg", duration: 166 });

// -------------------------------------------------------------------------
// BƯỚC 4: KHỞI TẠO CÁC NODE NGƯỜI DÙNG (USER NODES)
// -------------------------------------------------------------------------
CREATE (u1:User { userId: "anh874343@gmail.com", username: "Anh Le" });
CREATE (u2:User { userId: "user_minhanh", username: "Minh Anh" });
CREATE (u3:User { userId: "user_duybao", username: "Duy Bảo" });
CREATE (u4:User { userId: "user_thuha", username: "Thu Hà" });
CREATE (u5:User { userId: "user_tuangiam", username: "Tuấn Giảm" });

// -------------------------------------------------------------------------
// BƯỚC 5: TẠO QUAN HỆ PHIM THUỘC THỂ LOẠI (:BELONGS_TO) - Quan hệ 1
// -------------------------------------------------------------------------
MATCH (m1:Movie {movieId: 1}), (g2:Genre {genreId: 2}) CREATE (m1)-[:BELONGS_TO]->(g2);
MATCH (m2:Movie {movieId: 2}), (g1:Genre {genreId: 1}) CREATE (m2)-[:BELONGS_TO]->(g1);
MATCH (m2:Movie {movieId: 2}), (g2:Genre {genreId: 2}) CREATE (m2)-[:BELONGS_TO]->(g2);
MATCH (m3:Movie {movieId: 3}), (g5:Genre {genreId: 5}) CREATE (m3)-[:BELONGS_TO]->(g5);
MATCH (m4:Movie {movieId: 4}), (g3:Genre {genreId: 3}) CREATE (m4)-[:BELONGS_TO]->(g3);
MATCH (m5:Movie {movieId: 5}), (g4:Genre {genreId: 4}) CREATE (m5)-[:BELONGS_TO]->(g4);
MATCH (m6:Movie {movieId: 6}), (g2:Genre {genreId: 2}) CREATE (m6)-[:BELONGS_TO]->(g2);

// -------------------------------------------------------------------------
// BƯỚC 6: TẠO QUAN HỆ NGƯỜI DÙNG THẢ TIM / YÊU THÍCH (:FAVORITE) - Quan hệ 2
// -------------------------------------------------------------------------
MATCH (u1:User {userId: "anh874343@gmail.com"}), (m1:Movie {movieId: 1}) CREATE (u1)-[:FAVORITE { createdAt: "2026-08-01 10:00:00" }]->(m1);
MATCH (u1:User {userId: "anh874343@gmail.com"}), (m2:Movie {movieId: 2}) CREATE (u1)-[:FAVORITE { createdAt: "2026-08-01 11:30:00" }]->(m2);
MATCH (u2:User {userId: "user_minhanh"}), (m1:Movie {movieId: 1}) CREATE (u2)-[:FAVORITE { createdAt: "2026-08-02 08:15:00" }]->(m1);
MATCH (u2:User {userId: "user_minhanh"}), (m3:Movie {movieId: 3}) CREATE (u2)-[:FAVORITE { createdAt: "2026-08-02 09:20:00" }]->(m3);
MATCH (u3:User {userId: "user_duybao"}), (m2:Movie {movieId: 2}) CREATE (u3)-[:FAVORITE { createdAt: "2026-08-02 14:00:00" }]->(m2);
MATCH (u4:User {userId: "user_thuha"}), (m4:Movie {movieId: 4}) CREATE (u4)-[:FAVORITE { createdAt: "2026-08-02 15:45:00" }]->(m4);
MATCH (u5:User {userId: "user_tuangiam"}), (m1:Movie {movieId: 1}) CREATE (u5)-[:FAVORITE { createdAt: "2026-08-02 16:10:00" }]->(m1);

// -------------------------------------------------------------------------
// BƯỚC 7: TẠO QUAN HỆ NGƯỜI DÙNG ĐẶT VÉ XEM PHIM (:BOOKED) - Quan hệ 3
// -------------------------------------------------------------------------
MATCH (u1:User {userId: "anh874343@gmail.com"}), (m1:Movie {movieId: 1})
CREATE (u1)-[:BOOKED { bookingId: "BK1001", seatCount: 2, totalAmount: 180000, date: "2026-08-01" }]->(m1);

MATCH (u1:User {userId: "anh874343@gmail.com"}), (m2:Movie {movieId: 2})
CREATE (u1)-[:BOOKED { bookingId: "BK1002", seatCount: 3, totalAmount: 270000, date: "2026-08-02" }]->(m2);

MATCH (u2:User {userId: "user_minhanh"}), (m1:Movie {movieId: 1})
CREATE (u2)-[:BOOKED { bookingId: "BK1003", seatCount: 1, totalAmount: 90000, date: "2026-08-02" }]->(m1);

MATCH (u2:User {userId: "user_minhanh"}), (m6:Movie {movieId: 6})
CREATE (u2)-[:BOOKED { bookingId: "BK1004", seatCount: 4, totalAmount: 360000, date: "2026-08-02" }]->(m6);

MATCH (u3:User {userId: "user_duybao"}), (m1:Movie {movieId: 1})
CREATE (u3)-[:BOOKED { bookingId: "BK1005", seatCount: 2, totalAmount: 180000, date: "2026-08-02" }]->(m1);

MATCH (u3:User {userId: "user_duybao"}), (m2:Movie {movieId: 2})
CREATE (u3)-[:BOOKED { bookingId: "BK1006", seatCount: 2, totalAmount: 180000, date: "2026-08-02" }]->(m2);

MATCH (u4:User {userId: "user_thuha"}), (m3:Movie {movieId: 3})
CREATE (u4)-[:BOOKED { bookingId: "BK1007", seatCount: 2, totalAmount: 180000, date: "2026-08-02" }]->(m3);

MATCH (u5:User {userId: "user_tuangiam"}), (m5:Movie {movieId: 5})
CREATE (u5)-[:BOOKED { bookingId: "BK1008", seatCount: 1, totalAmount: 90000, date: "2026-08-02" }]->(m5);


// =========================================================================
// CÁC CÂU LỆNH TRUY VẤN DEMO NỘP BÀI (CYPHER QUERIES FOR GRADING)
// =========================================================================

// Truy vấn 1: TOP PHIM ĐƯỢC ĐẶT VÉ NHIỀU NHẤT
MATCH (u:User)-[r:BOOKED]->(m:Movie)
RETURN m.movieId AS MovieId, m.title AS Title, m.poster AS Poster, COUNT(r) AS BookingCount, SUM(r.seatCount) AS TotalSeats
ORDER BY BookingCount DESC, TotalSeats DESC
LIMIT 5;

// Truy vấn 2: TOP PHIM ĐƯỢC YÊU THÍCH (THẢ TIM) NHIỀU NHẤT
MATCH (u:User)-[r:FAVORITE]->(m:Movie)
RETURN m.movieId AS MovieId, m.title AS Title, m.poster AS Poster, COUNT(r) AS FavoriteCount
ORDER BY FavoriteCount DESC
LIMIT 5;

// Truy vấn 3: THỐNG KÊ TOP THỂ LOẠI PHIM HOT NHẤT DỰA TRÊN LƯỢT ĐẶT VÉ & THẢ TIM
MATCH (m:Movie)-[:BELONGS_TO]->(g:Genre)
OPTIONAL MATCH (u1:User)-[b:BOOKED]->(m)
OPTIONAL MATCH (u2:User)-[f:FAVORITE]->(m)
RETURN g.genreId AS GenreId, g.genreName AS GenreName, COUNT(DISTINCT b) AS TotalBookings, COUNT(DISTINCT f) AS TotalFavorites, (COUNT(DISTINCT b) + COUNT(DISTINCT f)) AS PopularityScore
ORDER BY PopularityScore DESC;

// Truy vấn 4: TRUY VẤN TƯƠNG QUAN ĐỒ THỊ - NHỮNG PHIM ĐƯỢC ĐẶT VÉ CÙNG NGUYÊN NÚT DỰA TRÊN NGƯỜI DÙNG CÙNG GU
MATCH (u:User)-[:BOOKED]->(m1:Movie {movieId: 1})
MATCH (u)-[:BOOKED]->(m2:Movie)
WHERE m1 <> m2
RETURN m2.movieId AS RelatedMovieId, m2.title AS RelatedMovieTitle, COUNT(u) AS CoBookedCount
ORDER BY CoBookedCount DESC;
