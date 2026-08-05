using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;

namespace doan3.Models
{
    public class Neo4jMovieViewModel
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string Poster { get; set; }
        public int Duration { get; set; }
        public int BookingCount { get; set; }
        public int FavoriteCount { get; set; }
        public string GenreName { get; set; }
        public bool IsFavorite { get; set; }
    }

    public class Neo4jGenreViewModel
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; }
        public int TotalBookings { get; set; }
        public int TotalFavorites { get; set; }
        public int PopularityScore { get; set; }
    }

    public class Neo4jService
    {
        private static readonly string Neo4jUri = ConfigurationManager.AppSettings["Neo4jUri"] ?? "http://localhost:7474";
        private static readonly string Neo4jUser = ConfigurationManager.AppSettings["Neo4jUser"] ?? "neo4j";
        private static readonly string Neo4jPassword = ConfigurationManager.AppSettings["Neo4jPassword"] ?? "adminpassword";

        private static string _cachedEndpoint = null;

        /// <summary>
        /// Gửi truy vấn Cypher tới Neo4j qua HTTP REST API (Hỗ trợ cả Neo4j 3.x, 4.x và 5.x)
        /// </summary>
        public JObject ExecuteCypher(string cypherQuery, Dictionary<string, object> parameters = null)
        {
            string[] endpoints = !string.IsNullOrEmpty(_cachedEndpoint)
                ? new[] { _cachedEndpoint }
                : new[]
                {
                    Neo4jUri.TrimEnd('/') + "/db/data/transaction/commit",
                    Neo4jUri.TrimEnd('/') + "/db/neo4j/tx/commit"
                };

            foreach (var endpoint in endpoints)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(endpoint);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.Timeout = 1500; // 1.5s timeout

                    // Header Basic Authentication
                    string authInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Neo4jUser}:{Neo4jPassword}"));
                    request.Headers["Authorization"] = "Basic " + authInfo;

                    // Build Payload JSON
                    var statementObj = new Dictionary<string, object>
                    {
                        { "statement", cypherQuery }
                    };

                    if (parameters != null && parameters.Count > 0)
                    {
                        statementObj["parameters"] = parameters;
                    }

                    var payload = new
                    {
                        statements = new[] { statementObj }
                    };

                    string jsonPayload = new JavaScriptSerializer().Serialize(payload);
                    byte[] byteArray = Encoding.UTF8.GetBytes(jsonPayload);
                    request.ContentLength = byteArray.Length;

                    using (Stream dataStream = request.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                    }

                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        string responseText = reader.ReadToEnd();
                        _cachedEndpoint = endpoint;
                        return JObject.Parse(responseText);
                    }
                }
                catch (WebException webEx)
                {
                    // Nếu lỗi 404 (endpoint cũ), thử tiếp endpoint 5.x
                    var httpStatus = (webEx.Response as HttpWebResponse)?.StatusCode;
                    if (httpStatus == HttpStatusCode.NotFound)
                    {
                        continue;
                    }
                    System.Diagnostics.Debug.WriteLine($"[Neo4j WebException] {webEx.Message}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Neo4j Error] {ex.Message}");
                }
            }

            return null;
        }

        /// <summary>
        /// Khởi tạo dữ liệu mẫu ban đầu nếu Neo4j đang trống
        /// </summary>
        private static bool _isSeeded = false;

        public bool SeedInitialData(LTW_DatVeXemPhimEntities db = null, bool force = false)
        {
            if (_isSeeded && !force) return true;

            if (db == null)
            {
                db = new LTW_DatVeXemPhimEntities();
            }

            try
            {
                _isSeeded = true;
                // 1. Xóa toàn bộ dữ liệu mẫu cũ trong Neo4j
                string clearCypher = "MATCH (n) DETACH DELETE n;";
                ExecuteCypher(clearCypher);

                // 2. Đồng bộ Thể Loại từ SQL Server
                var listTheLoai = db.TheLoais.ToList();
                foreach (var g in listTheLoai)
                {
                    string name = g.TenTheLoai?.Replace("'", "\\'") ?? "Thể loại";
                    string cypherG = $"MERGE (g:Genre {{ genreId: {g.MaTheLoai} }}) SET g.genreName = '{name}'";
                    ExecuteCypher(cypherG);
                }

                // 3. Đồng bộ Phim từ SQL Server
                var listPhim = db.Phims.ToList();
                foreach (var p in listPhim)
                {
                    string title = p.TenPhim?.Replace("'", "\\'") ?? "";
                    string poster = p.Poster?.Replace("'", "\\'") ?? "";
                    int duration = p.ThoiLuong ?? 120;
                    int genreId = p.MaTheLoai ?? 1;

                    string cypherP = $@"
                        MERGE (m:Movie {{ movieId: {p.PhimID} }}) 
                        SET m.title = '{title}', m.poster = '{poster}', m.duration = {duration}
                        WITH m
                        MATCH (g:Genre {{ genreId: {genreId} }})
                        MERGE (m)-[:BELONGS_TO]->(g)
                    ";
                    ExecuteCypher(cypherP);
                }

                // 4. Đồng bộ Người Dùng từ SQL Server
                var listNguoiDung = db.NguoiDungs.ToList();
                foreach (var u in listNguoiDung)
                {
                    string userId = u.UserName?.Replace("'", "\\'") ?? u.UserID.ToString();
                    string username = u.Name?.Replace("'", "\\'") ?? u.UserName ?? "User";
                    string cypherU = $"MERGE (u:User {{ userId: '{userId}' }}) SET u.username = '{username}'";
                    ExecuteCypher(cypherU);
                }

                // 5. Đồng bộ Lịch Sử Đặt Vé thực tế từ SQL Server
                var listDonDatVe = db.Don_Dat_Ve.ToList();
                foreach (var don in listDonDatVe)
                {
                    var chiTiet = db.Chi_Tiet_Ve.Where(c => c.DonDatVeID == don.DonDatVeID).ToList();
                    if (chiTiet.Count > 0)
                    {
                        var firstVe = chiTiet.FirstOrDefault();
                        var lichChieu = firstVe != null ? db.Lich_Chieu.FirstOrDefault(l => l.LichChieuID == firstVe.LichChieuID) : null;
                        if (lichChieu != null && lichChieu.PhimID.HasValue)
                        {
                            var khachHang = db.Khach_Hang.FirstOrDefault(k => k.KhachHangID == don.KhachHangID);
                            string userId = khachHang != null ? (khachHang.Email ?? khachHang.TenDayDu ?? khachHang.KhachHangID.ToString()) : "user_guest";
                            string dateStr = don.ThoiGianDat.HasValue ? don.ThoiGianDat.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");
                            string cypherBooking = $@"
                                MATCH (u:User {{ userId: '{userId.Replace("'", "\\'")}' }}), (m:Movie {{ movieId: {lichChieu.PhimID.Value} }})
                                MERGE (u)-[r:BOOKED {{ bookingId: '{don.MaDatVe}' }}]->(m)
                                SET r.seatCount = {chiTiet.Count}, r.date = '{dateStr}'
                            ";
                            ExecuteCypher(cypherBooking);
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Truy vấn Top Phim Đặt Vé Nhiều Nhất từ Neo4j Graph
        /// </summary>
        public List<Neo4jMovieViewModel> GetTopBookedMovies(int limit = 4, string currentUsername = "")
        {
            string query = @"
                MATCH (m:Movie)
                OPTIONAL MATCH (u:User)-[r:BOOKED]->(m)
                OPTIONAL MATCH (m)-[:BELONGS_TO]->(g:Genre)
                WITH m, g, COUNT(r) AS bookingCount, COALESCE(SUM(r.seatCount), 0) AS totalSeats
                OPTIONAL MATCH (uFav:User {userId: $username})-[f:FAVORITE]->(m)
                RETURN m.movieId AS movieId, m.title AS title, m.poster AS poster, m.duration AS duration, 
                       g.genreName AS genreName, bookingCount, (f IS NOT NULL) AS isFav
                ORDER BY bookingCount DESC, totalSeats DESC, m.movieId ASC
                LIMIT " + limit;

            var paramsDict = new Dictionary<string, object> { { "username", currentUsername ?? "" } };
            var response = ExecuteCypher(query, paramsDict);

            return ParseMovieListResponse(response);
        }

        /// <summary>
        /// Truy vấn Top Phim Yêu Thích Nhất (Nhiều lượt thả tim)
        /// </summary>
        public List<Neo4jMovieViewModel> GetTopFavoriteMovies(int limit = 6, string currentUsername = "")
        {
            string query = @"
                MATCH (u:User)-[r:FAVORITE]->(m:Movie)
                OPTIONAL MATCH (m)-[:BELONGS_TO]->(g:Genre)
                WITH m, g, COUNT(r) AS favCount
                OPTIONAL MATCH (uFav:User {userId: $username})-[f:FAVORITE]->(m)
                RETURN m.movieId AS movieId, m.title AS title, m.poster AS poster, m.duration AS duration, 
                       g.genreName AS genreName, favCount AS favoriteCount, (f IS NOT NULL) AS isFav
                ORDER BY favCount DESC
                LIMIT " + limit;

            var paramsDict = new Dictionary<string, object> { { "username", currentUsername ?? "" } };
            var response = ExecuteCypher(query, paramsDict);

            return ParseMovieListResponse(response, isFavList: true);
        }

        /// <summary>
        /// Truy vấn Thống Kê Top Thể Loại Phim Thịnh Hành
        /// </summary>
        public List<Neo4jGenreViewModel> GetTrendingGenres(int limit = 5)
        {
            string query = @"
                MATCH (m:Movie)-[:BELONGS_TO]->(g:Genre)
                OPTIONAL MATCH (u1:User)-[b:BOOKED]->(m)
                OPTIONAL MATCH (u2:User)-[f:FAVORITE]->(m)
                RETURN g.genreId AS genreId, g.genreName AS genreName, 
                       COUNT(DISTINCT b) AS totalBookings, COUNT(DISTINCT f) AS totalFavorites, 
                       (COUNT(DISTINCT b) + COUNT(DISTINCT f)) AS popularityScore
                ORDER BY popularityScore DESC
                LIMIT " + limit;

            var response = ExecuteCypher(query);
            var genres = new List<Neo4jGenreViewModel>();

            if (response == null || response["results"] == null) return genres;

            try
            {
                var dataRows = response["results"]?[0]?["data"];
                if (dataRows != null)
                {
                    foreach (var row in dataRows)
                    {
                        var rowVal = row["row"];
                        genres.Add(new Neo4jGenreViewModel
                        {
                            GenreId = rowVal[0].Value<int>(),
                            GenreName = rowVal[1]?.ToString() ?? "Khác",
                            TotalBookings = rowVal[2].Value<int>(),
                            TotalFavorites = rowVal[3].Value<int>(),
                            PopularityScore = rowVal[4].Value<int>()
                        });
                    }
                }
            }
            catch { }

            return genres;
        }

        /// <summary>
        /// Bật/Tắt Yêu Thích Phim (Toggle Favorite Relationship)
        /// </summary>
        public bool ToggleFavorite(string username, int movieId, string movieTitle = "", string poster = "")
        {
            if (string.IsNullOrEmpty(username)) return false;

            // Kiểm tra quan hệ đã tồn tại chưa
            string checkQuery = @"
                MATCH (u:User {userId: $username})-[r:FAVORITE]->(m:Movie {movieId: $movieId})
                RETURN COUNT(r) AS favCount";

            var checkParams = new Dictionary<string, object>
            {
                { "username", username },
                { "movieId", movieId }
            };

            var checkRes = ExecuteCypher(checkQuery, checkParams);
            int count = 0;
            try
            {
                count = checkRes["results"]?[0]?["data"]?[0]?["row"]?[0]?.Value<int>() ?? 0;
            }
            catch { }

            if (count > 0)
            {
                // XÓA quan hệ Favorite
                string deleteQuery = @"
                    MATCH (u:User {userId: $username})-[r:FAVORITE]->(m:Movie {movieId: $movieId})
                    DELETE r";
                ExecuteCypher(deleteQuery, checkParams);
                return false; // Trạng thái mới: Unfavorited
            }
            else
            {
                // TẠO mới quan hệ Favorite
                string createQuery = @"
                    MERGE (u:User {userId: $username})
                    MERGE (m:Movie {movieId: $movieId})
                    ON CREATE SET m.title = $title, m.poster = $poster
                    MERGE (u)-[:FAVORITE { createdAt: $now }]->(m)";

                var createParams = new Dictionary<string, object>
                {
                    { "username", username },
                    { "movieId", movieId },
                    { "title", movieTitle ?? ("Phim #" + movieId) },
                    { "poster", poster ?? "" },
                    { "now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                };
                ExecuteCypher(createQuery, createParams);
                return true; // Trạng thái mới: Favorited
            }
        }

        /// <summary>
        /// Thêm quan hệ Đặt vé (BOOKED) khi khách hàng thanh toán thành công
        /// </summary>
        public bool AddBooking(string username, int movieId, string bookingId, int seatCount, decimal totalAmount, string movieTitle = "")
        {
            if (string.IsNullOrEmpty(username)) return false;

            string query = @"
                MERGE (u:User {userId: $username})
                MERGE (m:Movie {movieId: $movieId})
                ON CREATE SET m.title = $title
                CREATE (u)-[:BOOKED { bookingId: $bookingId, seatCount: $seatCount, totalAmount: $amount, date: $now }]->(m)";

            var paramsDict = new Dictionary<string, object>
            {
                { "username", username },
                { "movieId", movieId },
                { "title", movieTitle ?? ("Phim #" + movieId) },
                { "bookingId", bookingId ?? ("BK" + Guid.NewGuid().ToString().Substring(0, 6)) },
                { "seatCount", seatCount },
                { "amount", Convert.ToDouble(totalAmount) },
                { "now", DateTime.Now.ToString("yyyy-MM-dd") }
            };

            var res = ExecuteCypher(query, paramsDict);
            return res != null;
        }

        // Helper chuyển đổi dữ liệu JSON từ Neo4j sang ViewModel C#
        private List<Neo4jMovieViewModel> ParseMovieListResponse(JObject response, bool isFavList = false)
        {
            var list = new List<Neo4jMovieViewModel>();
            if (response == null || response["results"] == null) return list;

            try
            {
                var dataRows = response["results"]?[0]?["data"];
                if (dataRows != null)
                {
                    foreach (var row in dataRows)
                    {
                        var rowVal = row["row"];
                        var item = new Neo4jMovieViewModel
                        {
                            MovieId = rowVal[0].Value<int>(),
                            Title = rowVal[1]?.ToString() ?? "Chưa rõ",
                            Poster = rowVal[2]?.ToString() ?? "/Images/default.jpg",
                            Duration = rowVal[3]?.Value<int>() ?? 120,
                            GenreName = rowVal[4]?.ToString() ?? "Tổng hợp",
                            IsFavorite = rowVal[6]?.Value<bool>() ?? false
                        };

                        if (isFavList)
                        {
                            item.FavoriteCount = rowVal[5]?.Value<int>() ?? 0;
                        }
                        else
                        {
                            item.BookingCount = rowVal[5]?.Value<int>() ?? 0;
                        }

                        list.Add(item);
                    }
                }
            }
            catch { }

            return list;
        }
    }
}
