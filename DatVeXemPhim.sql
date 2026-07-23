CREATE DATABASE LTW_DatVeXemPhim;
GO
USE LTW_DatVeXemPhim;


----------------------------------------------------------
-- 1. KHUNG PHÂN QUYỀN VÀ NGƯỜI DÙNG
----------------------------------------------------------
CREATE TABLE NhomNguoiDung (
	ID INT IDENTITY(1,1) PRIMARY KEY,
	Name NVARCHAR(50)
);

CREATE TABLE NguoiDung (
	UserID INT IDENTITY(1,1) PRIMARY KEY,
	UserName NVARCHAR(50) UNIQUE,
	Password NVARCHAR(100),
	GroupID INT,
	Name NVARCHAR(100),
	FOREIGN KEY (GroupID) REFERENCES NhomNguoiDung(ID)
);
GO

CREATE TABLE Khach_Hang (
	KhachHangID BIGINT IDENTITY(1,1) PRIMARY KEY,
	MaKhachHang AS ('KH' + RIGHT('00000000' + CAST(KhachHangID AS VARCHAR(8)), 8)) PERSISTED,
	Ngaysinh DATE NULL, 
	TenDayDu NVARCHAR(150),
	Email VARCHAR(100) UNIQUE,
	SoDienThoai VARCHAR(20),
	DiemThanhVien INT DEFAULT 0,
	UserID INT UNIQUE NULL,
	NgayTaoTaiKhoan DATETIME DEFAULT GETDATE(),
	FOREIGN KEY (UserID) REFERENCES NguoiDung(UserID)
);

----------------------------------------------------------
-- 2. KHUNG NỘI DUNG VÀ RẠP CHIẾU
----------------------------------------------------------
CREATE TABLE TheLoai (
	MaTheLoai INT IDENTITY(1,1) PRIMARY KEY,
	TenTheLoai NVARCHAR(100) UNIQUE
);

CREATE TABLE Phim (
	PhimID BIGINT IDENTITY(1,1) PRIMARY KEY,
	MaPhim AS ('P' + RIGHT('00000000' + CAST(PhimID AS VARCHAR(8)), 8)) PERSISTED,
	TenPhim NVARCHAR(255),
	MoTa NVARCHAR(MAX),
	NgayKhoiChieu DATE,
	ThoiLuong INT,
	DaoDien NVARCHAR(255),
	PhanLoaiDoTuoi NVARCHAR(50),
	Poster VARCHAR(255),
	TrangThai NVARCHAR(50) DEFAULT N'Dang Chieu',
	MaTheLoai INT,
	FOREIGN KEY (MaTheLoai) REFERENCES TheLoai(MaTheLoai)
);

CREATE TABLE Rap_Chieu (
	RapID BIGINT IDENTITY(1,1) PRIMARY KEY,
	MaRap AS ('R' + RIGHT('00000000' + CAST(RapID AS VARCHAR(8)), 8)) PERSISTED,
	TenRap NVARCHAR(255),
	DiaChi NVARCHAR(255),
	ThanhPho NVARCHAR(100),
	SoDienThoai VARCHAR(20)
);

CREATE TABLE Phong_Chieu (
	PhongID BIGINT IDENTITY(1,1) PRIMARY KEY,
	MaPhong AS ('PH' + RIGHT('000000' + CAST(PhongID AS VARCHAR(6)), 6)) PERSISTED,
	RapID BIGINT,
	TenPhong NVARCHAR(50),
	FOREIGN KEY (RapID) REFERENCES Rap_Chieu(RapID)
);

CREATE TABLE Ghe_Ngoi (
	GheID BIGINT IDENTITY(1,1) PRIMARY KEY,
	PhongID BIGINT,
	HangGhe VARCHAR(10),
	SoGhe INT,
	MaGhe AS (HangGhe + CAST(SoGhe AS VARCHAR(10))) PERSISTED,
	LoaiGhe NVARCHAR(50) DEFAULT N'Thuong',
	FOREIGN KEY (PhongID) REFERENCES Phong_Chieu(PhongID),
	UNIQUE (PhongID, HangGhe, SoGhe)
);

CREATE TABLE Lich_Chieu (
    LichChieuID BIGINT IDENTITY(1,1) PRIMARY KEY,
    MaLichChieu AS ('LC' + RIGHT('00000000' + CAST(LichChieuID AS VARCHAR(8)), 8)) PERSISTED,
    PhimID BIGINT,
    PhongID BIGINT,
    ThoiGianBatDau DATETIME,
    DinhDang NVARCHAR(20) DEFAULT N'2D',
    TrangThai NVARCHAR(50) NOT NULL 
        CONSTRAINT DF_LichChieu_TrangThai DEFAULT N'Hoat Dong',
    FOREIGN KEY (PhimID) REFERENCES Phim(PhimID),
    FOREIGN KEY (PhongID) REFERENCES Phong_Chieu(PhongID)
);


CREATE TABLE TienVe (
	LoaiGhe NVARCHAR(50) PRIMARY KEY,
	GiaTien DECIMAL(10,2)
);

----------------------------------------------------------
-- 3. KHUNG GIAO DỊCH VÀ ĐẶT VÉ
----------------------------------------------------------
CREATE TABLE Don_Dat_Ve (
	DonDatVeID BIGINT IDENTITY(1,1) PRIMARY KEY,
	MaDatVe AS ('DV' + RIGHT('00000000' + CAST(DonDatVeID AS VARCHAR(8)), 8)) PERSISTED,
	KhachHangID BIGINT NULL,
	TongTienDonHang DECIMAL(10, 2) DEFAULT 0,
	TrangThaiDonHang NVARCHAR(50) DEFAULT N'Da Tao',
	ThoiGianDat DATETIME DEFAULT GETDATE(),
	FOREIGN KEY (KhachHangID) REFERENCES Khach_Hang(KhachHangID)
);

CREATE TABLE Chi_Tiet_Ve (
	ChiTietVeID BIGINT IDENTITY(1,1) PRIMARY KEY,
	DonDatVeID BIGINT,
	LichChieuID BIGINT,
	GheID BIGINT,
    LoaiGhe NVARCHAR(50) NOT NULL,
	LoaiVe NVARCHAR(50) DEFAULT N'Nguoi Lon',
	TrangThaiSuDung BIT DEFAULT 0,
	FOREIGN KEY (DonDatVeID) REFERENCES Don_Dat_Ve(DonDatVeID),
	FOREIGN KEY (LichChieuID) REFERENCES Lich_Chieu(LichChieuID),
	FOREIGN KEY (GheID) REFERENCES Ghe_Ngoi(GheID),
    FOREIGN KEY (LoaiGhe) REFERENCES TienVe(LoaiGhe),
	UNIQUE (LichChieuID, GheID)
);

CREATE TABLE Khoa_Ghe_Tam_Thoi (
	KhoaGheID BIGINT IDENTITY(1,1) PRIMARY KEY,
	MaKhoa AS ('KGT' + RIGHT('00000000' + CAST(KhoaGheID AS VARCHAR(8)), 8)) PERSISTED,
	LichChieuID BIGINT,
	GheID BIGINT,
	KhachHangID BIGINT NULL,
	ThoiGianKhoa DATETIME DEFAULT GETDATE(),
	ThoiGianHetHan DATETIME NOT NULL,
	FOREIGN KEY (LichChieuID) REFERENCES Lich_Chieu(LichChieuID),
	FOREIGN KEY (GheID) REFERENCES Ghe_Ngoi(GheID),
	FOREIGN KEY (KhachHangID) REFERENCES Khach_Hang(KhachHangID),
	UNIQUE (LichChieuID, GheID)
);
GO


----------------------------------------------------------
-- 1. NhomNguoiDung & NguoiDung & Khach_Hang
----------------------------------------------------------
SET IDENTITY_INSERT NhomNguoiDung ON;
INSERT INTO NhomNguoiDung (ID, Name) VALUES (1, N'Admin');
INSERT INTO NhomNguoiDung (ID, Name) VALUES (2, N'Customer');
INSERT INTO NhomNguoiDung (ID, Name) VALUES (3, N'Staff');

SET IDENTITY_INSERT NhomNguoiDung OFF;

INSERT INTO NguoiDung (UserName, Password, GroupID, Name) VALUES
(N'admin', N'admin123', 1, N'Quản Trị Viên'), 
(N'kh1_long', N'kh123', 2, N'Lê Hoàng Long'), 
(N'kh2_huong', N'kh123', 2, N'Trần Thị Hương'),
(N'kh3_tai', N'kh123', 2, N'Nguyễn Văn Tài'),
(N'kh4_nhi', N'kh123', 2, N'Phạm Thanh Nhi'),
(N'kh5_cuong', N'kh123', 2, N'Đỗ Mạnh Cường');

INSERT INTO Khach_Hang (TenDayDu, Email, SoDienThoai, Ngaysinh, DiemThanhVien, UserID) VALUES

-- KhachHangID = 1
(N'Lê Hoàng Long', 'long.le@email.com', '0901000001', '1995-03-15', 10, 2), 

-- KhachHangID = 2
(N'Trần Thị Hương', 'huong.tran@email.com', '0901000002', '2001-07-28', 25, 3), 

-- KhachHangID = 3
(N'Nguyễn Văn Tài', 'tai.nguyen@email.com', '0901000003', '1988-11-01', 0, 4), 

-- KhachHangID = 4
(N'Phạm Thanh Nhi', 'nhi.pham@email.com', '0901000004', '1999-05-10', 50, 5), 

-- KhachHangID = 5
(N'Đỗ Mạnh Cường', 'cuong.do@email.com', '0901000005', '1990-01-20', 5, 6);
----------------------------------------------------------
-- 2. TheLoai & Phim
----------------------------------------------------------
INSERT INTO TheLoai (TenTheLoai) VALUES
(N'Hành động'), (N'Viễn tưởng'), (N'Tâm lý'), (N'Hoạt hình'),
(N'Kinh dị'), (N'Bí ẩn'), (N'Tình cảm'), (N'Trinh thám'),
(N'Gia đình'), (N'Hài hước'), (N'Lịch sử'), (N'Phiêu lưu');

INSERT INTO Phim (TenPhim, MoTa, NgayKhoiChieu, ThoiLuong, DaoDien, PhanLoaiDoTuoi, Poster, TrangThai, MaTheLoai) VALUES
(N'Vùng đất chết chóc', N'Tác phẩm hành động sinh tồn khốc liệt...', '2025-11-15', 125, N'Chad Stahelski', N'18+', '001.png', N'Dang Chieu', 1),
(N'Cuộc Chiến Sinh Tử', N'Một bộ phim gay cấn về một trận đấu sinh tồn...', '2025-11-23', 125, N'Christopher Nolan', N'18+', '013.png', N'Dang Chieu', 1),
(N'The PARTISAN', N'Phim chiến tranh căng thẳng kể về một nhóm kháng chiến...', '2025-12-02', 118, N'Denis Villeneuve', N'18+', '015.png', N'Dang Chieu', 1),
(N'Venom: Kèo Cuối', N'Phần cuối cùng trong loạt phim về người hùng phản diện Venom...', '2025-12-05', 120, N'Ruben Fleischer', N'18+', '029.png', N'Dang Chieu', 1),
(N'Godzilla Minus One', N'Lấy bối cảnh Nhật Bản hậu Thế chiến thứ hai...', '2025-11-20', 130, N'Takashi Yamazaki', N'18+', '002.png', N'Dang Chieu', 2),
(N'LAZARUS', N'Bộ phim khoa học viễn tưởng theo chân một kẻ chết đã sống lại...', '2025-11-24', 125, N'Ridley Scott', N'18+', '019.png', N'Dang Chieu', 2),
(N'Ma Trận', N'Một lập trình viên phát hiện ra rằng thực tại của loài người...', '2025-12-05', 120, N'The Wachowskis', N'18+', '047.png', N'Dang Chieu', 2),
(N'Mickey 17', N'Một người lính được cử đi khám phá một thế giới băng giá...', '2025-12-05', 120, N'Bong Joon Ho', N'18+', '048.png', N'Dang Chieu', 2),
(N'Mộ Đom Đóm', N'Tuyệt tác hoạt hình cảm động về hai anh em Seita và Setsuko...', '2025-11-25', 100, N'Isao Takahata', N'13+', '003.png', N'Dang Chieu', 3),
(N'2 Nhân Cách', N'Câu chuyện tâm lý phức tạp về một người đàn ông mắc chứng rối loạn đa nhân cách...', '2025-11-21', 115, N'David Fincher', N'18+', '010.png', N'Dang Chieu', 3),
(N'Ngã Ba Đồng Lộc', N'Bộ phim tái hiện lại sự hy sinh anh dũng của Mười cô gái...', '2025-11-30', 110, N'Nguyễn Quang Dũng', N'18+', '014.png', N'Dang Chieu', 3),
(N'SÁI BÁCH TUẤN', N'Phim tiểu sử sâu sắc kể về cuộc đời và sự nghiệp thăng trầm...', '2025-12-05', 120, N'Trần Anh Hùng', N'18+', '020.png', N'Dang Chieu', 3),
(N'Tớ và ROBOCO', N'Phim hoạt hình vui nhộn và ấm áp về cậu bé Bô-chan...', '2025-11-22', 95, N'Akira Shigino', N'13+', '004.png', N'Dang Chieu', 4),
(N'Chú Thuật Hồi Chiến', N'Các pháp sư trẻ tuổi tham gia vào một cuộc chiến chống lại...', '2025-11-19', 130, N'Park Sung-hoo', N'13+', '007.png', N'Dang Chieu', 4),
(N'The Legend Of VOX MACHINA', N'Một nhóm anh hùng không hoàn hảo được tập hợp để cứu vương quốc...', '2025-12-01', 130, N'Sam Riegel', N'13+', '011.png', N'Dang Chieu', 4),
(N'Cô Bé Người Cá Ponyo', N'Câu chuyện thần tiên về một cô bé người cá tên Ponyo...', '2025-12-05', 120, N'Hayao Miyazaki', N'13+', '030.png', N'Dang Chieu', 4),
(N'Mục sư và Con quỷ', N'Một vị mục sư tài năng và đức hạnh phải đối mặt với một con quỷ...', '2025-11-10', 115, N'James Wan', N'18+', '005.png', N'Dang Chieu', 5),
(N'Kẻ Ăn Hồn', N'Phim kinh dị dân gian Việt Nam, xoay quanh một nghi lễ bí ẩn...', '2025-12-05', 120, N'Trần Hữu Tấn', N'18+', '031.png', N'Dang Chieu', 5),
(N'Bóng Đè', N'Một người phụ nữ trẻ bị ám ảnh bởi những cơn bóng đè khủng khiếp...', '2025-12-05', 120, N'Lê Văn Kiệt', N'18+', '032.png', N'Dang Chieu', 5),
(N'Ác Qủy Ma Sơ', N'Một nữ tu và một linh mục được cử đến một tu viện hẻo lánh...', '2025-12-05', 120, N'Corin Hardy', N'18+', '033.png', N'Dang Chieu', 5),
(N'Phỏng Vấn Sát Nhân', N'Một nhà báo liều lĩnh tìm cách phỏng vấn một kẻ sát nhân...', '2025-11-18', 120, N'Park Chan-wook', N'18+', '006.png', N'Dang Chieu', 6),
(N'Frankenstein', N'Chuyển thể kinh điển về một nhà khoa học trẻ tuổi đầy tham vọng...', '2025-11-27', 125, N'Kenneth Branagh', N'18+', '008.png', N'Dang Chieu', 6),
(N'Cái Giá Của Lời Thú Tội', N'Một linh mục bị buộc phải giữ kín bí mật tội ác...', '2025-11-17', 115, N'Anh Tú', N'18+', '017.png', N'Dang Chieu', 6),
(N'Lần theo dấu vết', N'Một thám tử bị ám ảnh theo đuổi một vụ án lạnh lẽo kéo dài...', '2025-12-05', 120, N'Guy Ritchie', N'18+', '021.png', N'Dang Chieu', 6),
(N'SIHIIR PELAKOR', N'Câu chuyện tình cảm phức tạp xoay quanh một mối quan hệ tay ba...', '2025-11-28', 110, N'Choi Seung-ho', N'18+', '009.png', N'Dang Chieu', 7),
(N'Mắt Biếc', N'Dựa trên tiểu thuyết cùng tên, phim kể về mối tình đơn phương...', '2025-12-05', 120, N'Victor Vũ', N'18+', '041.png', N'Dang Chieu', 7),
(N'365 Ngày: Hôm Nay', N'Phần tiếp theo của câu chuyện tình yêu đầy đam mê...', '2025-12-05', 120, N'Barbara Białowąs', N'18+', '042.png', N'Dang Chieu', 7),
(N'Man In Love', N'Một gã côn đồ thô lỗ phải lòng một cô gái có hoàn cảnh khó khăn...', '2025-12-05', 120, N'Yin Chen-Hao', N'18+', '043.png', N'Dang Chieu', 7),
(N'Náo Loạn Đường Phố', N'Bộ phim trinh thám hành động về một sĩ quan cảnh sát chìm...', '2025-11-16', 120, N'Martin Scorsese', N'18+', '012.png', N'Dang Chieu', 8),
(N'Seven', N'Hai thám tử cùng nhau truy lùng một kẻ sát nhân hàng loạt...', '2025-12-05', 120, N'David Fincher', N'18+', '044.png', N'Dang Chieu', 8),
(N'Inferno', N'Giáo sư Robert Langdon phải giải mã một loạt các manh mối...', '2025-12-05', 120, N'Ron Howard', N'18+', '045.png', N'Dang Chieu', 8),
(N'Forgotten', N'Một người đàn ông tìm kiếm em trai bị bắt cóc của mình...', '2025-12-05', 120, N'Jang Hang-jun', N'18+', '046.png', N'Dang Chieu', 8),
(N'Những Ông Bố Của Mari', N'Phim hài hước và ấm áp về tình cha con...', '2025-11-26', 108, N'Lê Hoàng', N'13+', '018.png', N'Dang Chieu', 9),
(N'Thưa Mẹ Con Đi', N'Câu chuyện chân thực và cảm động về chàng trai Văn...', '2025-12-05', 120, N'Trịnh Đình Lê Minh', N'13+', '022.png', N'Dang Chieu', 9),
(N'Hồn PaPa Da Con Gái', N'Sau một tai nạn bất ngờ, linh hồn của một ông bố và cô con gái...', '2025-12-05', 120, N'Ken Ochiai', N'13+', '023.png', N'Dang Chieu', 9),
(N'Đường Về Nhà Của Cún Con', N'Một chuyến phiêu lưu kỳ thú và cảm động của một chú cún bị lạc...', '2025-12-05', 120, N'Lê Thanh Sơn', N'13+', '024.png', N'Dang Chieu', 9),
(N'Siêu Lừa Gặp Siêu Lầy', N'Ngay trước thềm lễ hội lớn, bảo vật linh thiêng Long Diện Hương...', '2025-12-05', 120, N'Võ Thanh Hòa', N'18+', '025.png', N'Dang Chieu', 10),
(N'Bỗng Dưng Trúng Số', N'Một nhóm binh sĩ Hàn Quốc phát hiện ra một tờ vé số trúng giải...', '2025-12-05', 120, N'Park Gyu-tae', N'13+', '026.png', N'Dang Chieu', 10),
(N'Nghề Siêu Dễ', N'Một nhóm cảnh sát quyết định mở quán phở để làm nơi mật phục...', '2025-12-05', 120, N'Võ Thanh Hòa', N'18+', '027.png', 'Dang Chieu', 10),
(N'Yêu Nhầm Bạn Thân', N'Câu chuyện hài lãng mạn về một chàng trai và cô gái...', '2025-12-05', 120, N'Victor Vũ', N'18+', '028.png', N'Dang Chieu', 10),
(N'Đào, Phở và Piano', N'Tái hiện lại cuộc chiến đấu bảo vệ Hà Nội 1946-1947...', '2025-12-05', 120, N'Phi Tiến Sơn', N'13+', '034.png', N'Dang Chieu', 11),
(N'Mùi Cỏ Cháy', N'Phim kể về những năm tháng chiến tranh khốc liệt qua góc nhìn...', '2025-12-05', 120, N'Nguyễn Hữu Mười', N'13+', '035.png', N'Dang Chieu', 11),
(N'Mưa Đỏ', N'Bộ phim lịch sử về sự kiện trận chiến lịch sử tại một cao điểm...', '2025-12-05', 120, N'Trần Văn Thủy', N'13+', '036.png', N'Dang Chieu', 11),
(N'Hà Nội 12 Ngày Đêm', N'Bộ phim hoành tráng tái hiện lại chiến dịch "Điện Biên Phủ trên không"...', '2025-12-05', 120, N'Bùi Đình Hạc', N'13+', '037.png', N'Dang Chieu', 11),
(N'CRUTCH', N'Một vận động viên bị tàn tật sau tai nạn đã vượt qua mọi giới hạn...', '2025-11-29', 105, N'Steven Spielberg', N'13+', '016.png', N'Dang Chieu', 12),
(N'Interstellar', N'Trước bờ vực tuyệt chủng của Trái Đất, một nhóm nhà du hành vũ trụ...', '2025-12-05', 120, N'Christopher Nolan', N'13+', '038.png', N'Dang Chieu', 12),
(N'Kẻ Đánh Cắp Giấc Mơ', N'Một tên trộm chuyên nghiệp có khả năng xâm nhập vào giấc mơ...', '2025-12-05', 120, N'Quentin Tarantino', N'18+', '039.png', N'Dang Chieu', 12),
(N'Người Nhện: Trở Về Nhà', N'Peter Parker cố gắng cân bằng giữa cuộc sống trung học và trách nhiệm...', '2025-12-05', 120, N'Jon Watts', N'13+', '040.png', N'Dang Chieu', 12);

----------------------------------------------------------
-- 3. Rap_Chieu & Phong_Chieu & Ghe_Ngoi
----------------------------------------------------------
INSERT INTO Rap_Chieu (TenRap, DiaChi, ThanhPho, SoDienThoai) VALUES
(N'CGV Aeon Tân Phú', N'30 Bờ Bao Tân Thắng, Quận Tân Phú', N'TP.HCM', '02838111111'),
(N'Galaxy Nguyễn Du', N'116 Nguyễn Du, Quận 1', N'TP.HCM', '02838222222'),
(N'Lotte Cộng Hòa', N'20 Cộng Hòa, Quận Tân Bình', N'TP.HCM', '02838333333'),
(N'Beta Cineplex Mỹ Đình', N'Tòa nhà Golden Field, Nam Từ Liêm', N'Hà Nội', '02438444444'),
(N'CGV Vincom Đà Nẵng', N'910A Ngô Quyền, Sơn Trà', N'Đà Nẵng', '02363555555'),
(N'BHD Star Bitexco', N'Lầu 4, Tháp Bitexco, Quận 1', N'TP.HCM', '02838666666'),
(N'Cinestar Quốc Thanh', N'271 Nguyễn Trãi, Quận 1', N'TP.HCM', '02838777777'),
(N'Lotte Cinema Gò Vấp', N'242 Nguyễn Văn Lượng, Gò Vấp', N'TP.HCM', '02838888888');


INSERT INTO Phong_Chieu (RapID, TenPhong) VALUES
(1, N'Phòng Chiếu 1'),
(1, N'Phòng Chiếu 2'), 
(1, N'Phòng Chiếu 3'),
(2, N'Phòng Chiếu 4'), 
(2, N'Phòng Chiếu 5'),
(2, N'Phòng Chiếu 6'), 
(3, N'Phòng Chiếu 7'),
(3, N'Phòng Chiếu 8'), 
(3, N'Phòng Chiếu 9'),
(4, N'Phòng Chiếu 10'), 
(4, N'Phòng Chiếu 11'), 
(4, N'Phòng Chiếu 12'), 
(5, N'Phòng Chiếu 13'),
(5, N'Phòng Chiếu 14'),
(5, N'Phòng Chiếu 15'),
(6, N'Phòng Chiếu 16'),
(6, N'Phòng Chiếu 17'),
(6, N'Phòng Chiếu 18');
DECLARE @Phong INT = 1

WHILE @Phong <= 12
BEGIN
    -- HÀNG A – Ghế Thường
    INSERT INTO Ghe_Ngoi (PhongID, HangGhe, SoGhe, LoaiGhe)
    VALUES
    (@Phong, 'A', 1, N'Thuong'), (@Phong, 'A', 2, N'Thuong'),
    (@Phong, 'A', 3, N'Thuong'), (@Phong, 'A', 4, N'Thuong'),
    (@Phong, 'A', 5, N'Thuong'), (@Phong, 'A', 6, N'Thuong'),
    (@Phong, 'A', 7, N'Thuong'), (@Phong, 'A', 8, N'Thuong');

    -- HÀNG B – Ghế Thường
    INSERT INTO Ghe_Ngoi (PhongID, HangGhe, SoGhe, LoaiGhe)
    VALUES
    (@Phong, 'B', 1, N'Thuong'), (@Phong, 'B', 2, N'Thuong'),
    (@Phong, 'B', 3, N'Thuong'), (@Phong, 'B', 4, N'Thuong'),
    (@Phong, 'B', 5, N'Thuong'), (@Phong, 'B', 6, N'Thuong'),
    (@Phong, 'B', 7, N'Thuong'), (@Phong, 'B', 8, N'Thuong');

    -- HÀNG C – Ghế VIP
    INSERT INTO Ghe_Ngoi (PhongID, HangGhe, SoGhe, LoaiGhe)
    VALUES
    (@Phong, 'C', 1, N'VIP'), (@Phong, 'C', 2, N'VIP'),
    (@Phong, 'C', 3, N'VIP'), (@Phong, 'C', 4, N'VIP'),
    (@Phong, 'C', 5, N'VIP'), (@Phong, 'C', 6, N'VIP'),
    (@Phong, 'C', 7, N'VIP'), (@Phong, 'C', 8, N'VIP');

    -- HÀNG D – Ghế Đôi
    INSERT INTO Ghe_Ngoi (PhongID, HangGhe, SoGhe, LoaiGhe)
    VALUES
    (@Phong, 'D', 1, N'Doi'), (@Phong, 'D', 2, N'Doi'),
    (@Phong, 'D', 3, N'Doi'), (@Phong, 'D', 4, N'Doi'),
    (@Phong, 'D', 5, N'Doi'), (@Phong, 'D', 6, N'Doi'),
    (@Phong, 'D', 7, N'Doi'), (@Phong, 'D', 8, N'Doi');

    SET @Phong = @Phong + 1;
END
----------------------------------------------------------
-- 4. TienVe & Lich_Chieu
----------------------------------------------------------
INSERT INTO TienVe (LoaiGhe, GiaTien) VALUES
(N'Thuong', 95000),
(N'VIP', 120000),
(N'Doi', 180000);

INSERT INTO Lich_Chieu (PhimID, PhongID, ThoiGianBatDau, DinhDang) VALUES
(1, 1, '2025-11-20 18:30:00', N'2D'),   
(2, 2, '2025-11-20 20:45:00', N'3D'),  
(3, 3, '2025-11-20 16:00:00', N'2D'),    
(4, 1, '2025-11-20 21:15:00', N'2D'),    
(5, 4, '2025-11-21 10:00:00', N'2D'),  
(6, 5, '2025-11-21 14:30:00', N'3D'),  
(7, 6, '2025-11-21 18:00:00', N'2D'),   
(8, 4, '2025-11-21 21:00:00', N'2D');  
----------------------------------------------------------
-- 5. Don_Dat_Ve & Chi_Tiet_Ve & Khoa_Ghe_Tam_Thoi
----------------------------------------------------------
INSERT INTO Don_Dat_Ve (KhachHangID, TongTienDonHang, TrangThaiDonHang, ThoiGianDat) VALUES
(1, 190000.00, N'Đã thanh toán', '2025-11-16 10:00:00'), 
(2, 120000.00, N'Đã thanh toán', '2025-11-16 11:30:00'),  
(3, 360000.00, N'Chưa thanh toán', '2025-11-16 13:45:00'),
(4, 190000.00, N'Đã thanh toán', '2025-11-16 15:20:00'), 
(5, 120000.00, N'Đã thanh toán', '2025-11-16 16:55:00');  


INSERT INTO Chi_Tiet_Ve (DonDatVeID, LichChieuID, GheID, LoaiGhe, LoaiVe, TrangThaiSuDung) VALUES
(1, 1, 1, N'Thuong', N'Nguoi Lon', 0),  
(1, 1, 2, N'Thuong', N'Nguoi Lon', 0), 
(2, 2, 25, N'Thuong', N'Nguoi Lon', 0),   
(3, 3, 49, N'Thuong', N'Nguoi Lon', 0),  
(3, 3, 50, N'Thuong', N'Nguoi Lon', 0),   
(4, 7, 71, N'Thuong', N'Nguoi Lon', 0),  
(4, 7, 72, N'Thuong', N'Sinh Vien', 0),  
(5, 6, 30, N'Thuong', N'Nguoi Lon', 0);   

INSERT INTO Khoa_Ghe_Tam_Thoi (LichChieuID, GheID, KhachHangID, ThoiGianKhoa, ThoiGianHetHan) VALUES
(5, 73, 1, GETDATE(), DATEADD(MINUTE, 10, GETDATE())),  
(6, 75, 3, GETDATE(), DATEADD(MINUTE, 5, GETDATE()));   
GO


-- 1. Khung Phân Quyền và Người Dùng
SELECT * FROM NhomNguoiDung;
SELECT * FROM NguoiDung;
SELECT * FROM Khach_Hang;
SELECT * FROM TheLoai;

-- 2. Khung Nội Dung và Rạp Chiếu
SELECT * FROM Phim;
SELECT * FROM Rap_Chieu;
SELECT * FROM Phong_Chieu;
SELECT * FROM Ghe_Ngoi;
SELECT * FROM TienVe;
SELECT * FROM Lich_Chieu;

-- 3. Khung Giao Dịch và Đặt Vé
SELECT * FROM Don_Dat_Ve;
SELECT * FROM Chi_Tiet_Ve;
SELECT * FROM Khoa_Ghe_Tam_Thoi;


go

DECLARE @PhimID BIGINT;
DECLARE @PhongID INT = 1;
DECLARE phim_cursor CURSOR FOR
SELECT PhimID FROM Phim ORDER BY PhimID;

OPEN phim_cursor;
FETCH NEXT FROM phim_cursor INTO @PhimID;

WHILE @@FETCH_STATUS = 0
BEGIN
   
    INSERT INTO Lich_Chieu (PhimID, PhongID, ThoiGianBatDau, DinhDang)
    VALUES (
        @PhimID,
        @PhongID,
        DATEADD(DAY, @PhimID % 5, '2025-11-20 14:30:00'),
        N'2D'
    );

    INSERT INTO Lich_Chieu (PhimID, PhongID, ThoiGianBatDau, DinhDang)
    VALUES (
        @PhimID,
        CASE WHEN @PhongID = 12 THEN 1 ELSE @PhongID + 1 END,
        DATEADD(DAY, @PhimID % 5, '2025-11-20 18:00:00'),
        N'2D'
    );

   
    IF (@PhimID % 2 = 0)
    BEGIN
        INSERT INTO Lich_Chieu (PhimID, PhongID, ThoiGianBatDau, DinhDang)
        VALUES (
            @PhimID,
            CASE 
                WHEN @PhongID >= 11 THEN 1 
                ELSE @PhongID + 2 
            END,
            DATEADD(DAY, @PhimID % 5, '2025-11-20 20:30:00'),
            N'3D'
        );
    END

   
    SET @PhongID = CASE WHEN @PhongID = 12 THEN 1 ELSE @PhongID + 1 END;

    FETCH NEXT FROM phim_cursor INTO @PhimID;
END

CLOSE phim_cursor;
DEALLOCATE phim_cursor;

go

UPDATE TOP (50) PERCENT Lich_Chieu
SET ThoiGianBatDau = CAST(CAST(GETDATE() AS DATE) AS DATETIME) + CAST(CAST(ThoiGianBatDau AS TIME) AS DATETIME);
UPDATE Lich_Chieu
SET ThoiGianBatDau = CAST(CAST(DATEADD(day, 1, GETDATE()) AS DATE) AS DATETIME) + CAST(CAST(ThoiGianBatDau AS TIME) AS DATETIME)
WHERE ThoiGianBatDau < CAST(GETDATE() AS DATE);

go


DECLARE @CurrentPhongID INT;


DECLARE phong_cursor CURSOR FOR 
SELECT PhongID FROM Phong_Chieu 
WHERE PhongID NOT IN (SELECT DISTINCT PhongID FROM Ghe_Ngoi);

OPEN phong_cursor;
FETCH NEXT FROM phong_cursor INTO @CurrentPhongID;

WHILE @@FETCH_STATUS = 0
BEGIN
   
    INSERT INTO Ghe_Ngoi (PhongID, HangGhe, SoGhe, LoaiGhe) VALUES
    (@CurrentPhongID, 'A', 1, N'Thuong'), (@CurrentPhongID, 'A', 2, N'Thuong'),
    (@CurrentPhongID, 'A', 3, N'Thuong'), (@CurrentPhongID, 'A', 4, N'Thuong'),
    (@CurrentPhongID, 'A', 5, N'Thuong'), (@CurrentPhongID, 'A', 6, N'Thuong'),
    (@CurrentPhongID, 'A', 7, N'Thuong'), (@CurrentPhongID, 'A', 8, N'Thuong');

    
    INSERT INTO Ghe_Ngoi (PhongID, HangGhe, SoGhe, LoaiGhe) VALUES
    (@CurrentPhongID, 'B', 1, N'Thuong'), (@CurrentPhongID, 'B', 2, N'Thuong'),
    (@CurrentPhongID, 'B', 3, N'Thuong'), (@CurrentPhongID, 'B', 4, N'Thuong'),
    (@CurrentPhongID, 'B', 5, N'Thuong'), (@CurrentPhongID, 'B', 6, N'Thuong'),
    (@CurrentPhongID, 'B', 7, N'Thuong'), (@CurrentPhongID, 'B', 8, N'Thuong');

    
    INSERT INTO Ghe_Ngoi (PhongID, HangGhe, SoGhe, LoaiGhe) VALUES
    (@CurrentPhongID, 'C', 1, N'VIP'), (@CurrentPhongID, 'C', 2, N'VIP'),
    (@CurrentPhongID, 'C', 3, N'VIP'), (@CurrentPhongID, 'C', 4, N'VIP'),
    (@CurrentPhongID, 'C', 5, N'VIP'), (@CurrentPhongID, 'C', 6, N'VIP'),
    (@CurrentPhongID, 'C', 7, N'VIP'), (@CurrentPhongID, 'C', 8, N'VIP');

    INSERT INTO Ghe_Ngoi (PhongID, HangGhe, SoGhe, LoaiGhe) VALUES
    (@CurrentPhongID, 'D', 1, N'Doi'), (@CurrentPhongID, 'D', 2, N'Doi'),
    (@CurrentPhongID, 'D', 3, N'Doi'), (@CurrentPhongID, 'D', 4, N'Doi'),
    (@CurrentPhongID, 'D', 5, N'Doi'), (@CurrentPhongID, 'D', 6, N'Doi'),
    (@CurrentPhongID, 'D', 7, N'Doi'), (@CurrentPhongID, 'D', 8, N'Doi');

    FETCH NEXT FROM phong_cursor INTO @CurrentPhongID;
END

CLOSE phong_cursor;
DEALLOCATE phong_cursor;
GO


