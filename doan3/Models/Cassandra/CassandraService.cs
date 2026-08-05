using Cassandra;
using doan3.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace doan3.Models.Cassandra
{
    public static class CassandraService
    {
        private const string Keyspace = "movie_booking_cassandra";

        private static readonly ICluster _cluster;
        private static readonly ISession _session;

        static CassandraService()
        {
            _cluster = Cluster.Builder()
                .AddContactPoint("127.0.0.1")
                .WithPort(9042)
                .Build();

            _session = _cluster.Connect(Keyspace);
        }

        public static ISession Session
        {
            get { return _session; }
        }

        public static void LogUserActivity(
            int userId,
            string action,
            int? movieId,
            int? showtimeId,
            long? bookingId,
            string description,
            string ip,
            string device)
        {
            ExecuteSafe("LogUserActivity", delegate
            {
                const string cql = @"
                    INSERT INTO user_activity_by_user_month
                    (
                        user_id,
                        activity_month,
                        event_time,
                        event_id,
                        action_type,
                        movie_id,
                        showtime_id,
                        booking_id,
                        description,
                        ip_address,
                        device
                    )
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                PreparedStatement prepared = _session.Prepare(cql);

                BoundStatement bound = prepared.Bind(
                    userId,
                    ToLocalDate(DateTime.Now),
                    DateTime.Now,
                    Guid.NewGuid(),
                    action ?? "UNKNOWN",
                    movieId,
                    showtimeId,
                    bookingId,
                    description ?? string.Empty,
                    ParseIpAddress(ip),
                    device ?? string.Empty
                );

                _session.Execute(bound);
            });
        }

        public static void LogUserActivity(
            int userId,
            int movieId,
            long lichChieuId,
            long bookingId,
            string action,
            string description)
        {
            LogUserActivity(
                userId,
                action,
                movieId,
                ToInt32Id(lichChieuId, "LichChieuID"),
                bookingId,
                description,
                null,
                null
            );
        }

        public static void LogActivityByDay(
            int userId,
            int movieId,
            long lichChieuId,
            long bookingId,
            string action,
            string description)
        {
            ExecuteSafe("LogActivityByDay", delegate
            {
                const string cql = @"
                    INSERT INTO activity_by_day_type
                    (
                        activity_date,
                        action_type,
                        event_time,
                        event_id,
                        user_id,
                        movie_id,
                        showtime_id,
                        booking_id,
                        description
                    )
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";

                DateTime now = DateTime.UtcNow;
                PreparedStatement prepared = _session.Prepare(cql);

                BoundStatement bound = prepared.Bind(
                    ToLocalDate(now),
                    action ?? "UNKNOWN",
                    now,
                    Guid.NewGuid(),
                    userId,
                    movieId,
                    ToInt32Id(lichChieuId, "LichChieuID"),
                    bookingId,
                    description ?? string.Empty
                );

                _session.Execute(bound);
            });
        }

        public static void LogBookingEvent(
            long bookingId,
            int userId,
            long lichChieuId,
            string eventType,
            decimal amount)
        {
            LogBookingEvent(
                bookingId,
                userId,
                lichChieuId,
                null,
                eventType,
                null,
                eventType == "PAYMENT_SUCCESS" ? "PAID" : "PENDING",
                amount
            );
        }

        public static void LogBookingEvent(
            long bookingId,
            int userId,
            long lichChieuId,
            string seatCode,
            string eventType,
            string oldStatus,
            string newStatus,
            decimal? amount)
        {
            ExecuteSafe("LogBookingEvent", delegate
            {
                const string cql = @"
                    INSERT INTO booking_events_by_booking
                    (
                        booking_id,
                        event_time,
                        event_id,
                        event_type,
                        user_id,
                        showtime_id,
                        seat_code,
                        old_status,
                        new_status,
                        amount,
                        metadata
                    )
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                var metadata = new Dictionary<string, string>
                {
                    { "source", "ASP.NET MVC" }
                };

                PreparedStatement prepared = _session.Prepare(cql);

                BoundStatement bound = prepared.Bind(
                    bookingId,
                    DateTime.UtcNow,
                    Guid.NewGuid(),
                    eventType ?? "BOOKING_CREATED",
                    userId,
                    ToInt32Id(lichChieuId, "LichChieuID"),
                    seatCode,
                    oldStatus,
                    newStatus,
                    amount,
                    metadata
                );

                _session.Execute(bound);
            });
        }

        public static void LogSeatLock(
    long lichChieuId,
    long seatId,
    int userId)
        {
            const string cql = @"
    INSERT INTO seat_timeline_by_showtime
    (
        showtime_id,
        seat_code,
        event_time,
        event_id,
        booking_id,
        user_id,
        event_type,
        old_status,
        new_status,
        expires_at
    )
    VALUES (?,?,?,?,?,?,?,?,?,?)";

            var stmt = _session.Prepare(cql);

            _session.Execute(stmt.Bind(
                ToInt32Id(lichChieuId, "LichChieu"),
                seatId.ToString(),
                DateTime.UtcNow,
                Guid.NewGuid(),
                0L,
                userId,
                "LOCK",
                "AVAILABLE",
                "HELD",
                DateTime.UtcNow.AddMinutes(1)
            ));
        }

        public static void LogSeatTimeline(
            long lichChieuId,
            GheTinhDiem ghe,
            long maDonHang,
            int userId)
        {
            string oldStatus = "AVAILABLE";
            string newStatus = "HELD";

            if (ghe == null)
            {
                return;
            }

            ExecuteSafe("LogSeatTimeline", delegate
            {
                const string cql = @"
                    INSERT INTO seat_timeline_by_showtime
                    (
                        showtime_id,
                        seat_code,
                        event_time,
                        event_id,
                        booking_id,
                        user_id,
                        event_type,
                        old_status,
                        new_status,
                        expires_at
                    )
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                PreparedStatement prepared = _session.Prepare(cql);

                BoundStatement bound = prepared.Bind(
                    ToInt32Id(lichChieuId, "LichChieuID"),
                    ghe.MaGhe ?? ghe.GheID.ToString(),
                    DateTime.UtcNow,
                    Guid.NewGuid(),
                    maDonHang,
                    userId,
                    "SOLD",
                    "HELD",
                    "SOLD",
                    null
                );

                _session.Execute(bound);
            });
        }

        public static void LogSeatTimeline(
            long lichChieuId,
            GheTinhDiem ghe,
            long maDonHang,
            object userId)
        {
            LogSeatTimeline(
                lichChieuId,
                ghe,
                maDonHang,
                Convert.ToInt32(userId)
            );
        }

        public static void LogPaymentHistory(
            int userId,
            long bookingId,
            decimal amount,
            string method,
            string status)
        {
            ExecuteSafe("LogPaymentHistory", delegate
            {
                const string cql = @"
                    INSERT INTO payment_history_by_user
                    (
                        user_id,
                        payment_month,
                        paid_at,
                        payment_id,
                        booking_id,
                        amount,
                        method,
                        status,
                        transaction_code
                    )
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";

                DateTime now = DateTime.UtcNow;
                PreparedStatement prepared = _session.Prepare(cql);

                BoundStatement bound = prepared.Bind(
                    userId,
                    ToLocalDate(now),
                    now,
                    Guid.NewGuid(),
                    bookingId,
                    amount,
                    method ?? "UNKNOWN",
                    status ?? "SUCCESS",
                    "PAY-" + bookingId + "-" + now.ToString("yyyyMMddHHmmss")
                );

                _session.Execute(bound);
            });
        }

        public static void LogBookingHistory(
            int userId,
            long bookingId,
            int movieId,
            string movieTitle,
            long lichChieuId,
            DateTime? showtimeAt,
            IEnumerable<string> seatCodes,
            decimal amount,
            string status,
            string paymentMethod)
        {
            ExecuteSafe("LogBookingHistory", delegate
            {
                const string cql = @"
                    INSERT INTO booking_history_by_user
                    (
                        user_id,
                        booking_month,
                        created_at,
                        booking_id,
                        movie_id,
                        movie_title,
                        showtime_id,
                        showtime_at,
                        seat_codes,
                        total_amount,
                        status,
                        payment_method
                    )
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                DateTime now = DateTime.UtcNow;
                var seats = new HashSet<string>(
                    (seatCodes ?? Enumerable.Empty<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                );

                PreparedStatement prepared = _session.Prepare(cql);

                BoundStatement bound = prepared.Bind(
                    userId,
                    ToLocalDate(now),
                    now,
                    bookingId,
                    movieId,
                    movieTitle ?? string.Empty,
                    ToInt32Id(lichChieuId, "LichChieuID"),
                    showtimeAt,
                    seats,
                    amount,
                    status ?? "PAID",
                    paymentMethod ?? "UNKNOWN"
                );

                _session.Execute(bound);
            });
        }

        public static void UpdateDashboard(DateTime date, decimal revenue)
        {
            ExecuteSafe("UpdateDashboard", delegate
            {
                DateTime now = DateTime.UtcNow;
                LocalDate dashboardDate = ToLocalDate(date);
                sbyte bucketHour = checked((sbyte)now.Hour);

                const string selectCql = @"
                    SELECT bookings, paid_bookings, revenue, tickets_sold,
                           active_users, searches, page_views, payment_failures
                    FROM dashboard_by_day
                    WHERE dashboard_date = ? AND bucket_hour = ?";

                PreparedStatement selectPrepared = _session.Prepare(selectCql);
                Row row = _session.Execute(
                    selectPrepared.Bind(dashboardDate, bucketHour)
                ).FirstOrDefault();

                int bookings = GetInt(row, "bookings") + 1;
                int paidBookings = GetInt(row, "paid_bookings") + 1;
                decimal currentRevenue = GetDecimal(row, "revenue") + revenue;
                int ticketsSold = GetInt(row, "tickets_sold") + 1;
                int activeUsers = Math.Max(1, GetInt(row, "active_users"));
                int searches = GetInt(row, "searches");
                int pageViews = GetInt(row, "page_views");
                int failures = GetInt(row, "payment_failures");

                const string insertCql = @"
                    INSERT INTO dashboard_by_day
                    (
                        dashboard_date,
                        bucket_hour,
                        bookings,
                        paid_bookings,
                        revenue,
                        tickets_sold,
                        active_users,
                        searches,
                        page_views,
                        payment_failures,
                        updated_at
                    )
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                PreparedStatement insertPrepared = _session.Prepare(insertCql);
                _session.Execute(insertPrepared.Bind(
                    dashboardDate,
                    bucketHour,
                    bookings,
                    paidBookings,
                    currentRevenue,
                    ticketsSold,
                    activeUsers,
                    searches,
                    pageViews,
                    failures,
                    now
                ));
            });
        }

        public static void UpdateAnalytics(DateTime date, decimal revenue)
        {
            ExecuteSafe("UpdateAnalytics", delegate
            {
                LocalDate month = new LocalDate(date.Year, date.Month, 1);

                const string selectCql = @"
                    SELECT metric_value, event_count
                    FROM analytics_by_month_metric
                    WHERE metric_month = ?
                      AND metric_type = ?
                      AND dimension = ?";

                PreparedStatement selectPrepared = _session.Prepare(selectCql);
                Row row = _session.Execute(
                    selectPrepared.Bind(month, "REVENUE", "ALL")
                ).FirstOrDefault();

                decimal metricValue = GetDecimal(row, "metric_value") + revenue;
                long eventCount = GetLong(row, "event_count") + 1L;

                const string insertCql = @"
                    INSERT INTO analytics_by_month_metric
                    (
                        metric_month,
                        metric_type,
                        dimension,
                        metric_value,
                        event_count,
                        updated_at
                    )
                    VALUES (?, ?, ?, ?, ?, ?)";

                PreparedStatement insertPrepared = _session.Prepare(insertCql);
                _session.Execute(insertPrepared.Bind(
                    month,
                    "REVENUE",
                    "ALL",
                    metricValue,
                    eventCount,
                    DateTime.UtcNow
                ));
            });
        }

        private static void ExecuteSafe(string operation, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "[CassandraService][" + operation + "] " +
                    ex.GetType().Name + ": " + ex.Message
                );
            }
        }

        private static LocalDate ToLocalDate(DateTime date)
        {
            return new LocalDate(date.Year, date.Month, date.Day);
        }

        private static int ToInt32Id(long value, string fieldName)
        {
            if (value < int.MinValue || value > int.MaxValue)
            {
                throw new OverflowException(
                    fieldName + " vượt phạm vi kiểu int của Cassandra: " + value
                );
            }

            return (int)value;
        }

        private static IPAddress ParseIpAddress(string ip)
        {
            IPAddress parsed;
            if (!string.IsNullOrWhiteSpace(ip) &&
                IPAddress.TryParse(ip, out parsed))
            {
                return parsed;
            }

            return IPAddress.Loopback;
        }

        private static int GetInt(Row row, string column)
        {
            if (row == null || row.IsNull(column))
            {
                return 0;
            }

            return row.GetValue<int>(column);
        }

        private static long GetLong(Row row, string column)
        {
            if (row == null || row.IsNull(column))
            {
                return 0L;
            }

            return row.GetValue<long>(column);
        }

        private static decimal GetDecimal(Row row, string column)
        {
            if (row == null || row.IsNull(column))
            {
                return 0m;
            }

            return row.GetValue<decimal>(column);
        }
    }
}