-- =====================================================================
-- CASSANDRA SCRIPT NỘP BÀI - ASP.NET MVC 
-- ID mapping:
--   UserID      -> int
--   MovieID     -> int
--   LichChieuID -> int
--   DonDatVeID  -> bigint
--   event_id, audit_id, payment_id, request_id -> uuid
-- =====================================================================

DROP KEYSPACE IF EXISTS movie_booking_cassandra;

CREATE KEYSPACE movie_booking_cassandra
WITH replication = {'class':'SimpleStrategy','replication_factor':1}
AND durable_writes = true;

USE movie_booking_cassandra;


-- ---------------------------------------------------------------------
-- TABLE: user_activity_by_user_month
CREATE TABLE IF NOT EXISTS user_activity_by_user_month (
    user_id int,
    activity_month date,
    event_time timestamp,
    event_id uuid,
    action_type text,
    movie_id int,
    showtime_id int,
    booking_id bigint,
    description text,
    ip_address inet,
    device text,
    PRIMARY KEY ((user_id, activity_month), event_time, event_id)
) WITH CLUSTERING ORDER BY (event_time DESC, event_id ASC)
  AND comment = 'User Timeline theo user và tháng; PK=(user_id,activity_month), CK=(event_time,event_id)';


-- ---------------------------------------------------------------------
-- TABLE: activity_by_day_type
CREATE TABLE IF NOT EXISTS activity_by_day_type (
    activity_date date,
    action_type text,
    event_time timestamp,
    event_id uuid,
    user_id int,
    movie_id int,
    showtime_id int,
    booking_id bigint,
    description text,
    PRIMARY KEY ((activity_date, action_type), event_time, event_id)
) WITH CLUSTERING ORDER BY (event_time DESC, event_id ASC)
  AND comment = 'Hoạt động theo ngày và loại hành động; PK=(activity_date,action_type), CK=(event_time,event_id)';


-- ---------------------------------------------------------------------
-- TABLE: booking_history_by_user
CREATE TABLE IF NOT EXISTS booking_history_by_user (
    user_id int,
    booking_month date,
    created_at timestamp,
    booking_id bigint,
    movie_id int,
    movie_title text,
    showtime_id int,
    showtime_at timestamp,
    seat_codes set<text>,
    total_amount decimal,
    status text,
    payment_method text,
    PRIMARY KEY ((user_id, booking_month), created_at, booking_id)
) WITH CLUSTERING ORDER BY (created_at DESC, booking_id ASC)
  AND comment = 'Lịch sử đặt vé theo user/tháng; PK=(user_id,booking_month), CK=(created_at,booking_id)';


-- ---------------------------------------------------------------------
-- TABLE: booking_events_by_booking
CREATE TABLE IF NOT EXISTS booking_events_by_booking (
    booking_id bigint,
    event_time timestamp,
    event_id uuid,
    event_type text,
    user_id int,
    showtime_id int,
    seat_code text,
    old_status text,
    new_status text,
    amount decimal,
    metadata map<text,text>,
    PRIMARY KEY ((booking_id), event_time, event_id)
) WITH CLUSTERING ORDER BY (event_time ASC, event_id ASC)
  AND comment = 'Booking Event Store; PK=booking_id, CK=(event_time,event_id)';


-- ---------------------------------------------------------------------
-- TABLE: seat_timeline_by_showtime
CREATE TABLE IF NOT EXISTS seat_timeline_by_showtime (
    showtime_id int,
    seat_code text,
    event_time timestamp,
    event_id uuid,
    booking_id bigint,
    user_id int,
    event_type text,
    old_status text,
    new_status text,
    expires_at timestamp,
    PRIMARY KEY ((showtime_id), seat_code, event_time, event_id)
) WITH CLUSTERING ORDER BY (seat_code ASC, event_time DESC, event_id ASC)
  AND comment = 'Seat Timeline theo suất chiếu; PK=showtime_id, CK=(seat_code,event_time,event_id)';


-- ---------------------------------------------------------------------
-- TABLE: payment_history_by_user
CREATE TABLE IF NOT EXISTS payment_history_by_user (
    user_id int,
    payment_month date,
    paid_at timestamp,
    payment_id uuid,
    booking_id bigint,
    amount decimal,
    method text,
    status text,
    transaction_code text,
    PRIMARY KEY ((user_id, payment_month), paid_at, payment_id)
) WITH CLUSTERING ORDER BY (paid_at DESC, payment_id ASC)
  AND comment = 'Payment History theo user/tháng; PK=(user_id,payment_month), CK=(paid_at,payment_id)';


-- ---------------------------------------------------------------------
-- TABLE: search_history_by_user
CREATE TABLE IF NOT EXISTS search_history_by_user (
    user_id int,
    search_month date,
    searched_at timestamp,
    search_id bigint,
    keyword text,
    filters map<text,text>,
    result_count int,
    selected_movie_id int,
    PRIMARY KEY ((user_id, search_month), searched_at, search_id)
) WITH CLUSTERING ORDER BY (searched_at DESC, search_id ASC)
  AND comment = 'Search History theo user/tháng; PK=(user_id,search_month), CK=(searched_at,search_id)';


-- ---------------------------------------------------------------------
-- TABLE: analytics_by_month_metric
CREATE TABLE IF NOT EXISTS analytics_by_month_metric (
    metric_month date,
    metric_type text,
    dimension text,
    metric_value decimal,
    event_count bigint,
    updated_at timestamp,
    PRIMARY KEY ((metric_month, metric_type), dimension)
) WITH comment = 'Analytics theo tháng/loại metric; PK=(metric_month,metric_type), CK=dimension';


-- ---------------------------------------------------------------------
-- TABLE: dashboard_by_day
CREATE TABLE IF NOT EXISTS dashboard_by_day (
    dashboard_date date,
    bucket_hour tinyint,
    bookings int,
    paid_bookings int,
    revenue decimal,
    tickets_sold int,
    active_users int,
    searches int,
    page_views int,
    payment_failures int,
    updated_at timestamp,
    PRIMARY KEY ((dashboard_date), bucket_hour)
) WITH CLUSTERING ORDER BY (bucket_hour DESC)
  AND comment = 'Dashboard theo ngày và giờ; PK=dashboard_date, CK=bucket_hour';


-- ---------------------------------------------------------------------
-- TABLE: traffic_by_day_path
CREATE TABLE IF NOT EXISTS traffic_by_day_path (
    traffic_date date,
    path text,
    request_time timestamp,
    request_id uuid,
    user_id int,
    method text,
    status_code int,
    duration_ms int,
    ip_address inet,
    user_agent text,
    PRIMARY KEY ((traffic_date, path), request_time, request_id)
) WITH CLUSTERING ORDER BY (request_time DESC, request_id ASC)
  AND comment = 'Traffic theo ngày/đường dẫn; PK=(traffic_date,path), CK=(request_time,request_id)';


-- ---------------------------------------------------------------------
-- TABLE: request_logs_by_day
CREATE TABLE IF NOT EXISTS request_logs_by_day (
    log_date date,
    logged_at timestamp,
    request_id uuid,
    level text,
    service text,
    endpoint text,
    user_id int,
    status_code int,
    duration_ms int,
    message text,
    PRIMARY KEY ((log_date), logged_at, request_id)
) WITH CLUSTERING ORDER BY (logged_at DESC, request_id ASC)
  AND default_time_to_live = 7776000
  AND comment = 'Request Logs giữ 90 ngày; PK=log_date, CK=(logged_at,request_id)';


-- ---------------------------------------------------------------------
-- TABLE: audit_logs_by_month_action
CREATE TABLE IF NOT EXISTS audit_logs_by_month_action (
    audit_month date,
    action_type text,
    action_time timestamp,
    audit_id uuid,
    actor_id int,
    actor_role text,
    entity_type text,
    entity_id bigint,
    before_data text,
    after_data text,
    ip_address inet,
    PRIMARY KEY ((audit_month, action_type), action_time, audit_id)
) WITH CLUSTERING ORDER BY (action_time DESC, audit_id ASC)
  AND comment = 'Audit Logs theo tháng/loại hành động; PK=(audit_month,action_type), CK=(action_time,audit_id)';


-- =====================================================================
-- SEED DATA
-- =====================================================================


-- SEED: user_activity_by_user_month

INSERT INTO user_activity_by_user_month (user_id, activity_month, event_time, event_id, action_type, movie_id, showtime_id, booking_id, description, ip_address, device)
VALUES (1, '2026-06-01', '2026-06-12T09:01:10Z', 51000000-0000-4000-8000-000000000001, 'LOGIN', null, null, null, 'Đăng nhập thành công', '10.0.0.11', 'Chrome/Windows');

INSERT INTO user_activity_by_user_month (user_id, activity_month, event_time, event_id, action_type, movie_id, showtime_id, booking_id, description, ip_address, device)
VALUES (1, '2026-06-01', '2026-06-12T09:03:05Z', 51000000-0000-4000-8000-000000000002, 'SEARCH', 1, null, null, 'Tìm kiếm phim Dune: Part Two', '10.0.0.11', 'Chrome/Windows');

INSERT INTO user_activity_by_user_month (user_id, activity_month, event_time, event_id, action_type, movie_id, showtime_id, booking_id, description, ip_address, device)
VALUES (1, '2026-06-01', '2026-06-12T09:05:00Z', 51000000-0000-4000-8000-000000000003, 'VIEW_MOVIE', 1, 1, null, 'Xem chi tiết phim và suất chiếu', '10.0.0.11', 'Chrome/Windows');

INSERT INTO user_activity_by_user_month (user_id, activity_month, event_time, event_id, action_type, movie_id, showtime_id, booking_id, description, ip_address, device)
VALUES (1, '2026-06-01', '2026-06-12T09:10:00Z', 51000000-0000-4000-8000-000000000004, 'BOOKING', 1, 1, 1, 'Tạo đơn đặt vé B1', '10.0.0.11', 'Chrome/Windows');

INSERT INTO user_activity_by_user_month (user_id, activity_month, event_time, event_id, action_type, movie_id, showtime_id, booking_id, description, ip_address, device)
VALUES (1, '2026-06-01', '2026-06-12T09:12:00Z', 51000000-0000-4000-8000-000000000005, 'PAYMENT', 1, 1, 1, 'Thanh toán VNPay thành công', '10.0.0.11', 'Chrome/Windows');

INSERT INTO user_activity_by_user_month (user_id, activity_month, event_time, event_id, action_type, movie_id, showtime_id, booking_id, description, ip_address, device)
VALUES (2, '2026-07-01', '2026-07-03T14:00:00Z', 52000000-0000-4000-8000-000000000001, 'LOGIN', null, null, null, 'Đăng nhập thành công', '10.0.0.12', 'Safari/iPhone');

INSERT INTO user_activity_by_user_month (user_id, activity_month, event_time, event_id, action_type, movie_id, showtime_id, booking_id, description, ip_address, device)
VALUES (2, '2026-07-01', '2026-07-03T14:04:00Z', 52000000-0000-4000-8000-000000000002, 'SEARCH', 2, null, null, 'Tìm kiếm phim Inside Out 2', '10.0.0.12', 'Safari/iPhone');

INSERT INTO user_activity_by_user_month (user_id, activity_month, event_time, event_id, action_type, movie_id, showtime_id, booking_id, description, ip_address, device)
VALUES (2, '2026-07-01', '2026-07-03T14:09:00Z', 52000000-0000-4000-8000-000000000003, 'BOOKING', 2, 2, 2, 'Tạo đơn đặt vé B2', '10.0.0.12', 'Safari/iPhone');

INSERT INTO user_activity_by_user_month (user_id, activity_month, event_time, event_id, action_type, movie_id, showtime_id, booking_id, description, ip_address, device)
VALUES (3, '2026-08-01', '2026-08-01T18:30:00Z', 53000000-0000-4000-8000-000000000001, 'LOGIN', null, null, null, 'Đăng nhập thành công', '10.0.0.13', 'Edge/Windows');

INSERT INTO user_activity_by_user_month (user_id, activity_month, event_time, event_id, action_type, movie_id, showtime_id, booking_id, description, ip_address, device)
VALUES (3, '2026-08-01', '2026-08-01T18:35:00Z', 53000000-0000-4000-8000-000000000002, 'VIEW_MOVIE', 3, 3, null, 'Xem chi tiết phim Deadpool & Wolverine', '10.0.0.13', 'Edge/Windows');


-- SEED: activity_by_day_type

INSERT INTO activity_by_day_type (activity_date, action_type, event_time, event_id, user_id, movie_id, showtime_id, booking_id, description)
VALUES ('2026-06-12', 'LOGIN', '2026-06-12T09:01:10Z', 51000000-0000-4000-8000-000000000001, 1, null, null, null, 'Đăng nhập thành công');

INSERT INTO activity_by_day_type (activity_date, action_type, event_time, event_id, user_id, movie_id, showtime_id, booking_id, description)
VALUES ('2026-06-12', 'SEARCH', '2026-06-12T09:03:05Z', 51000000-0000-4000-8000-000000000002, 1, 1, null, null, 'Tìm kiếm phim Dune: Part Two');

INSERT INTO activity_by_day_type (activity_date, action_type, event_time, event_id, user_id, movie_id, showtime_id, booking_id, description)
VALUES ('2026-06-12', 'VIEW_MOVIE', '2026-06-12T09:05:00Z', 51000000-0000-4000-8000-000000000003, 1, 1, 1, null, 'Xem chi tiết phim và suất chiếu');

INSERT INTO activity_by_day_type (activity_date, action_type, event_time, event_id, user_id, movie_id, showtime_id, booking_id, description)
VALUES ('2026-06-12', 'BOOKING', '2026-06-12T09:10:00Z', 51000000-0000-4000-8000-000000000004, 1, 1, 1, 1, 'Tạo đơn đặt vé B1');

INSERT INTO activity_by_day_type (activity_date, action_type, event_time, event_id, user_id, movie_id, showtime_id, booking_id, description)
VALUES ('2026-06-12', 'PAYMENT', '2026-06-12T09:12:00Z', 51000000-0000-4000-8000-000000000005, 1, 1, 1, 1, 'Thanh toán VNPay thành công');

INSERT INTO activity_by_day_type (activity_date, action_type, event_time, event_id, user_id, movie_id, showtime_id, booking_id, description)
VALUES ('2026-07-03', 'LOGIN', '2026-07-03T14:00:00Z', 52000000-0000-4000-8000-000000000001, 2, null, null, null, 'Đăng nhập thành công');

INSERT INTO activity_by_day_type (activity_date, action_type, event_time, event_id, user_id, movie_id, showtime_id, booking_id, description)
VALUES ('2026-07-03', 'SEARCH', '2026-07-03T14:04:00Z', 52000000-0000-4000-8000-000000000002, 2, 2, null, null, 'Tìm kiếm phim Inside Out 2');

INSERT INTO activity_by_day_type (activity_date, action_type, event_time, event_id, user_id, movie_id, showtime_id, booking_id, description)
VALUES ('2026-07-03', 'BOOKING', '2026-07-03T14:09:00Z', 52000000-0000-4000-8000-000000000003, 2, 2, 2, 2, 'Tạo đơn đặt vé B2');

INSERT INTO activity_by_day_type (activity_date, action_type, event_time, event_id, user_id, movie_id, showtime_id, booking_id, description)
VALUES ('2026-08-01', 'LOGIN', '2026-08-01T18:30:00Z', 53000000-0000-4000-8000-000000000001, 3, null, null, null, 'Đăng nhập thành công');

INSERT INTO activity_by_day_type (activity_date, action_type, event_time, event_id, user_id, movie_id, showtime_id, booking_id, description)
VALUES ('2026-08-01', 'VIEW_MOVIE', '2026-08-01T18:35:00Z', 53000000-0000-4000-8000-000000000002, 3, 3, 3, null, 'Xem chi tiết phim Deadpool & Wolverine');


-- SEED: booking_history_by_user

INSERT INTO booking_history_by_user (user_id, booking_month, created_at, booking_id, movie_id, movie_title, showtime_id, showtime_at, seat_codes, total_amount, status, payment_method)
VALUES (1, '2026-06-01', '2026-06-12T09:10:00Z', 1, 1, 'Dune: Part Two', 1, '2026-06-12T12:30:00Z', {'A5','A6'}, 190000.00, 'PAID', 'VNPAY');

INSERT INTO booking_history_by_user (user_id, booking_month, created_at, booking_id, movie_id, movie_title, showtime_id, showtime_at, seat_codes, total_amount, status, payment_method)
VALUES (1, '2026-07-01', '2026-07-20T10:20:00Z', 4, 2, 'Inside Out 2', 2, '2026-07-20T15:00:00Z', {'C3'}, 85000.00, 'CANCELLED', 'MOMO');

INSERT INTO booking_history_by_user (user_id, booking_month, created_at, booking_id, movie_id, movie_title, showtime_id, showtime_at, seat_codes, total_amount, status, payment_method)
VALUES (2, '2026-07-01', '2026-07-03T14:09:00Z', 2, 2, 'Inside Out 2', 2, '2026-07-03T16:00:00Z', {'B7','B8'}, 170000.00, 'PENDING', 'MOMO');

INSERT INTO booking_history_by_user (user_id, booking_month, created_at, booking_id, movie_id, movie_title, showtime_id, showtime_at, seat_codes, total_amount, status, payment_method)
VALUES (3, '2026-08-01', '2026-08-01T18:40:00Z', 3, 3, 'Deadpool & Wolverine', 3, '2026-08-01T21:00:00Z', {'D1'}, 110000.00, 'PAID', 'CASH');


-- SEED: booking_events_by_booking

INSERT INTO booking_events_by_booking (booking_id, event_time, event_id, event_type, user_id, showtime_id, seat_code, old_status, new_status, amount, metadata)
VALUES (1, '2026-06-12T09:10:00Z', 61000000-0000-4000-8000-000000000001, 'BOOKING_CREATED', 1, 1, null, null, 'PENDING', 190000.00, {'channel':'WEB'});

INSERT INTO booking_events_by_booking (booking_id, event_time, event_id, event_type, user_id, showtime_id, seat_code, old_status, new_status, amount, metadata)
VALUES (1, '2026-06-12T09:11:00Z', 61000000-0000-4000-8000-000000000002, 'SEAT_SELECTED', 1, 1, 'A5,A6', 'AVAILABLE', 'HELD', null, {'hold_seconds':'300'});

INSERT INTO booking_events_by_booking (booking_id, event_time, event_id, event_type, user_id, showtime_id, seat_code, old_status, new_status, amount, metadata)
VALUES (1, '2026-06-12T09:12:00Z', 61000000-0000-4000-8000-000000000003, 'PAYMENT_SUCCEEDED', 1, 1, null, 'PENDING', 'PAID', 190000.00, {'provider':'VNPAY','transaction':'VNP-001'});

INSERT INTO booking_events_by_booking (booking_id, event_time, event_id, event_type, user_id, showtime_id, seat_code, old_status, new_status, amount, metadata)
VALUES (2, '2026-07-03T14:09:00Z', 62000000-0000-4000-8000-000000000001, 'BOOKING_CREATED', 2, 2, null, null, 'PENDING', 170000.00, {'channel':'MOBILE'});

INSERT INTO booking_events_by_booking (booking_id, event_time, event_id, event_type, user_id, showtime_id, seat_code, old_status, new_status, amount, metadata)
VALUES (2, '2026-07-03T14:10:00Z', 62000000-0000-4000-8000-000000000002, 'SEAT_SELECTED', 2, 2, 'B7,B8', 'AVAILABLE', 'HELD', null, {'hold_seconds':'300'});

INSERT INTO booking_events_by_booking (booking_id, event_time, event_id, event_type, user_id, showtime_id, seat_code, old_status, new_status, amount, metadata)
VALUES (3, '2026-08-01T18:40:00Z', 63000000-0000-4000-8000-000000000001, 'BOOKING_CREATED', 3, 3, null, null, 'PENDING', 110000.00, {'channel':'COUNTER'});

INSERT INTO booking_events_by_booking (booking_id, event_time, event_id, event_type, user_id, showtime_id, seat_code, old_status, new_status, amount, metadata)
VALUES (3, '2026-08-01T18:42:00Z', 63000000-0000-4000-8000-000000000002, 'PAYMENT_SUCCEEDED', 3, 3, null, 'PENDING', 'PAID', 110000.00, {'provider':'CASH'});


-- SEED: seat_timeline_by_showtime

INSERT INTO seat_timeline_by_showtime (showtime_id, seat_code, event_time, event_id, booking_id, user_id, event_type, old_status, new_status, expires_at)
VALUES (1, 'A5', '2026-06-12T09:11:00Z', 71000000-0000-4000-8000-000000000001, 1, 1, 'HELD', 'AVAILABLE', 'HELD', '2026-06-12T09:16:00Z');

INSERT INTO seat_timeline_by_showtime (showtime_id, seat_code, event_time, event_id, booking_id, user_id, event_type, old_status, new_status, expires_at)
VALUES (1, 'A5', '2026-06-12T09:12:00Z', 71000000-0000-4000-8000-000000000002, 1, 1, 'SOLD', 'HELD', 'SOLD', null);

INSERT INTO seat_timeline_by_showtime (showtime_id, seat_code, event_time, event_id, booking_id, user_id, event_type, old_status, new_status, expires_at)
VALUES (1, 'A6', '2026-06-12T09:11:00Z', 71000000-0000-4000-8000-000000000003, 1, 1, 'HELD', 'AVAILABLE', 'HELD', '2026-06-12T09:16:00Z');

INSERT INTO seat_timeline_by_showtime (showtime_id, seat_code, event_time, event_id, booking_id, user_id, event_type, old_status, new_status, expires_at)
VALUES (2, 'B7', '2026-07-03T14:10:00Z', 72000000-0000-4000-8000-000000000001, 2, 2, 'HELD', 'AVAILABLE', 'HELD', '2026-07-03T14:15:00Z');

INSERT INTO seat_timeline_by_showtime (showtime_id, seat_code, event_time, event_id, booking_id, user_id, event_type, old_status, new_status, expires_at)
VALUES (2, 'B8', '2026-07-03T14:10:00Z', 72000000-0000-4000-8000-000000000002, 2, 2, 'HELD', 'AVAILABLE', 'HELD', '2026-07-03T14:15:00Z');

INSERT INTO seat_timeline_by_showtime (showtime_id, seat_code, event_time, event_id, booking_id, user_id, event_type, old_status, new_status, expires_at)
VALUES (3, 'D1', '2026-08-01T18:41:00Z', 73000000-0000-4000-8000-000000000001, 3, 3, 'SOLD', 'AVAILABLE', 'SOLD', null);


-- SEED: payment_history_by_user

INSERT INTO payment_history_by_user (user_id, payment_month, paid_at, payment_id, booking_id, amount, method, status, transaction_code)
VALUES (1, '2026-06-01', '2026-06-12T09:12:00Z', 40000000-0000-4000-8000-000000000001, 1, 190000.00, 'VNPAY', 'SUCCESS', 'VNP-001');

INSERT INTO payment_history_by_user (user_id, payment_month, paid_at, payment_id, booking_id, amount, method, status, transaction_code)
VALUES (2, '2026-07-01', '2026-07-03T14:12:00Z', 40000000-0000-4000-8000-000000000002, 2, 170000.00, 'MOMO', 'FAILED', 'MOMO-002');

INSERT INTO payment_history_by_user (user_id, payment_month, paid_at, payment_id, booking_id, amount, method, status, transaction_code)
VALUES (3, '2026-08-01', '2026-08-01T18:42:00Z', 40000000-0000-4000-8000-000000000003, 3, 110000.00, 'CASH', 'SUCCESS', 'CASH-003');


-- SEED: search_history_by_user

INSERT INTO search_history_by_user (user_id, search_month, searched_at, search_id, keyword, filters, result_count, selected_movie_id)
VALUES (1, '2026-06-01', '2026-06-12T09:03:05Z', 1, 'Dune', {'genre':'Sci-Fi','city':'HCM'}, 6, 1);

INSERT INTO search_history_by_user (user_id, search_month, searched_at, search_id, keyword, filters, result_count, selected_movie_id)
VALUES (2, '2026-07-01', '2026-07-03T14:04:00Z', 2, 'Inside Out', {'genre':'Animation'}, 3, 2);

INSERT INTO search_history_by_user (user_id, search_month, searched_at, search_id, keyword, filters, result_count, selected_movie_id)
VALUES (3, '2026-08-01', '2026-08-01T18:32:00Z', 3, 'Deadpool', {'format':'2D'}, 2, 3);


-- SEED: analytics_by_month_metric

INSERT INTO analytics_by_month_metric (metric_month, metric_type, dimension, metric_value, event_count, updated_at)
VALUES ('2026-06-01', 'BOOKING', 'ALL', 1.00, 1, '2026-06-30T23:59:00Z');

INSERT INTO analytics_by_month_metric (metric_month, metric_type, dimension, metric_value, event_count, updated_at)
VALUES ('2026-06-01', 'REVENUE', 'ALL', 190000.00, 1, '2026-06-30T23:59:00Z');

INSERT INTO analytics_by_month_metric (metric_month, metric_type, dimension, metric_value, event_count, updated_at)
VALUES ('2026-07-01', 'BOOKING', 'ALL', 2.00, 2, '2026-07-31T23:59:00Z');

INSERT INTO analytics_by_month_metric (metric_month, metric_type, dimension, metric_value, event_count, updated_at)
VALUES ('2026-07-01', 'PAYMENT_FAILED', 'MOMO', 1.00, 1, '2026-07-31T23:59:00Z');

INSERT INTO analytics_by_month_metric (metric_month, metric_type, dimension, metric_value, event_count, updated_at)
VALUES ('2026-08-01', 'REVENUE', 'ALL', 110000.00, 1, '2026-08-05T15:00:00Z');

INSERT INTO analytics_by_month_metric (metric_month, metric_type, dimension, metric_value, event_count, updated_at)
VALUES ('2026-08-01', 'LOGIN', 'ALL', 1.00, 1, '2026-08-05T15:00:00Z');


-- SEED: dashboard_by_day

INSERT INTO dashboard_by_day (dashboard_date, bucket_hour, bookings, paid_bookings, revenue, tickets_sold, active_users, searches, page_views, payment_failures, updated_at)
VALUES ('2026-06-12', 9, 1, 1, 190000.00, 2, 1, 1, 4, 0, '2026-06-12T09:59:00Z');

INSERT INTO dashboard_by_day (dashboard_date, bucket_hour, bookings, paid_bookings, revenue, tickets_sold, active_users, searches, page_views, payment_failures, updated_at)
VALUES ('2026-07-03', 14, 1, 0, 0.00, 0, 1, 1, 5, 1, '2026-07-03T14:59:00Z');

INSERT INTO dashboard_by_day (dashboard_date, bucket_hour, bookings, paid_bookings, revenue, tickets_sold, active_users, searches, page_views, payment_failures, updated_at)
VALUES ('2026-08-01', 18, 1, 1, 110000.00, 1, 1, 1, 3, 0, '2026-08-01T18:59:00Z');


-- SEED: traffic_by_day_path

INSERT INTO traffic_by_day_path (traffic_date, path, request_time, request_id, user_id, method, status_code, duration_ms, ip_address, user_agent)
VALUES ('2026-06-12', '/login', '2026-06-12T09:01:10Z', 91000000-0000-4000-8000-000000000001, 1, 'POST', 200, 120, '10.0.0.11', 'Chrome/Windows');

INSERT INTO traffic_by_day_path (traffic_date, path, request_time, request_id, user_id, method, status_code, duration_ms, ip_address, user_agent)
VALUES ('2026-06-12', '/movies/search', '2026-06-12T09:03:05Z', 91000000-0000-4000-8000-000000000002, 1, 'GET', 200, 85, '10.0.0.11', 'Chrome/Windows');

INSERT INTO traffic_by_day_path (traffic_date, path, request_time, request_id, user_id, method, status_code, duration_ms, ip_address, user_agent)
VALUES ('2026-07-03', '/payments/momo', '2026-07-03T14:12:00Z', 92000000-0000-4000-8000-000000000001, 2, 'POST', 502, 1450, '10.0.0.12', 'Safari/iPhone');

INSERT INTO traffic_by_day_path (traffic_date, path, request_time, request_id, user_id, method, status_code, duration_ms, ip_address, user_agent)
VALUES ('2026-08-01', '/bookings', '2026-08-01T18:40:00Z', 93000000-0000-4000-8000-000000000001, 3, 'POST', 201, 210, '10.0.0.13', 'Edge/Windows');


-- SEED: request_logs_by_day

INSERT INTO request_logs_by_day (log_date, logged_at, request_id, level, service, endpoint, user_id, status_code, duration_ms, message)
VALUES ('2026-06-12', '2026-06-12T09:01:10Z', 91000000-0000-4000-8000-000000000001, 'INFO', 'WEB', '/login', 1, 200, 120, 'Login request completed');

INSERT INTO request_logs_by_day (log_date, logged_at, request_id, level, service, endpoint, user_id, status_code, duration_ms, message)
VALUES ('2026-07-03', '2026-07-03T14:12:00Z', 92000000-0000-4000-8000-000000000001, 'ERROR', 'PAYMENT', '/payments/momo', 2, 502, 1450, 'Payment gateway timeout');

INSERT INTO request_logs_by_day (log_date, logged_at, request_id, level, service, endpoint, user_id, status_code, duration_ms, message)
VALUES ('2026-08-01', '2026-08-01T18:40:00Z', 93000000-0000-4000-8000-000000000001, 'INFO', 'BOOKING', '/bookings', 3, 201, 210, 'Booking created');


-- SEED: audit_logs_by_month_action

INSERT INTO audit_logs_by_month_action (audit_month, action_type, action_time, audit_id, actor_id, actor_role, entity_type, entity_id, before_data, after_data, ip_address)
VALUES ('2026-06-01', 'LOGIN', '2026-06-12T09:01:10Z', a1000000-0000-4000-8000-000000000001, 1, 'USER', 'USER_SESSION', 1, null, '{"status":"ACTIVE"}', '10.0.0.11');

INSERT INTO audit_logs_by_month_action (audit_month, action_type, action_time, audit_id, actor_id, actor_role, entity_type, entity_id, before_data, after_data, ip_address)
VALUES ('2026-06-01', 'PAYMENT', '2026-06-12T09:12:00Z', a1000000-0000-4000-8000-000000000002, 1, 'USER', 'BOOKING', 1, '{"status":"PENDING"}', '{"status":"PAID"}', '10.0.0.11');

INSERT INTO audit_logs_by_month_action (audit_month, action_type, action_time, audit_id, actor_id, actor_role, entity_type, entity_id, before_data, after_data, ip_address)
VALUES ('2026-07-01', 'BOOKING', '2026-07-03T14:09:00Z', a2000000-0000-4000-8000-000000000001, 2, 'USER', 'BOOKING', 2, null, '{"status":"PENDING"}', '10.0.0.12');

INSERT INTO audit_logs_by_month_action (audit_month, action_type, action_time, audit_id, actor_id, actor_role, entity_type, entity_id, before_data, after_data, ip_address)
VALUES ('2026-08-01', 'ADMIN_UPDATE', '2026-08-02T08:00:00Z', a3000000-0000-4000-8000-000000000001, 1, 'ADMIN', 'MOVIE', 3, '{"status":"COMING_SOON"}', '{"status":"NOW_SHOWING"}', '10.0.0.99');


-- =====================================================================
-- MINH HỌA BATCH / UPDATE / TTL / DELETE
-- =====================================================================

BEGIN BATCH
INSERT INTO booking_events_by_booking
(booking_id,event_time,event_id,event_type,user_id,showtime_id,seat_code,old_status,new_status,amount,metadata)
VALUES (4,'2026-07-20T10:22:00Z',74000000-0000-4000-8000-000000000003,
'BOOKING_CANCELLED',1,2,'C3','PENDING','CANCELLED',85000,
{'reason':'USER_REQUEST'});

INSERT INTO activity_by_day_type
(activity_date,action_type,event_time,event_id,user_id,movie_id,showtime_id,booking_id,description)
VALUES ('2026-07-20','BOOKING','2026-07-20T10:22:00Z',
54000000-0000-4000-8000-000000000004,1,2,2,4,'User cancelled booking B4');
APPLY BATCH;

UPDATE booking_history_by_user
SET status='PAID', payment_method='MOMO'
WHERE user_id=2
  AND booking_month='2026-07-01'
  AND created_at='2026-07-03T14:09:00Z'
  AND booking_id=2;

INSERT INTO request_logs_by_day
(log_date,logged_at,request_id,level,service,endpoint,user_id,status_code,duration_ms,message)
VALUES ('2026-08-05','2026-08-05T15:00:00Z',
99000000-0000-4000-8000-000000000001,'DEBUG','WEB','/health',null,200,5,
'Temporary health-check log')
USING TTL 3600;

DELETE FROM request_logs_by_day
WHERE log_date='2026-08-05'
  AND logged_at='2026-08-05T15:00:00Z'
  AND request_id=99000000-0000-4000-8000-000000000001;

-- =====================================================================
-- TRUY VẤN KIỂM TRA
-- =====================================================================

SELECT * FROM user_activity_by_user_month
WHERE user_id=1 AND activity_month='2026-06-01';

SELECT * FROM activity_by_day_type
WHERE activity_date='2026-06-12' AND action_type='LOGIN';

SELECT * FROM booking_history_by_user
WHERE user_id=1 AND booking_month='2026-06-01';

SELECT * FROM booking_events_by_booking
WHERE booking_id=1;

SELECT * FROM seat_timeline_by_showtime
WHERE showtime_id=1;

SELECT * FROM payment_history_by_user
WHERE user_id=1 AND payment_month='2026-06-01';

SELECT * FROM search_history_by_user
WHERE user_id=1 AND search_month='2026-06-01';

SELECT * FROM analytics_by_month_metric
WHERE metric_month='2026-06-01' AND metric_type='REVENUE';

SELECT * FROM dashboard_by_day
WHERE dashboard_date='2026-06-12';

SELECT * FROM traffic_by_day_path
WHERE traffic_date='2026-06-12' AND path='/login';

SELECT * FROM request_logs_by_day
WHERE log_date='2026-06-12';

SELECT * FROM audit_logs_by_month_action
WHERE audit_month='2026-06-01' AND action_type='LOGIN';
