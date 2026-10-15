SET NOCOUNT ON;

INSERT INTO Movies (Title, Duration, IsActive, PosterUrl, Genre, ReleaseDate, Description, Director)
VALUES
(N'Dune: Hành tinh cát - Phần hai', 166, 1, N'https://images.unsplash.com/photo-1536440136628-849c177e76a1?auto=format&fit=crop&w=800&q=80', N'Khoa học viễn tưởng', '2026-10-02', N'Paul Atreides hợp lực với người Fremen để giành lại Arrakis.', N'Denis Villeneuve'),
(N'Oppenheimer', 180, 1, N'https://images.unsplash.com/photo-1440404653325-ab127d49abc1?auto=format&fit=crop&w=800&q=80', N'Tiểu sử', '2026-09-18', N'Câu chuyện về nhà khoa học đứng sau dự án bom nguyên tử.', N'Christopher Nolan'),
(N'Spider-Man: Không còn nhà', 148, 1, N'https://images.unsplash.com/photo-1635805737707-575885ab0820?auto=format&fit=crop&w=800&q=80', N'Hành động', '2026-10-09', N'Peter Parker nhờ Doctor Strange giúp, nhưng phép thuật mở ra nhiều vũ trụ.', N'Jon Watts'),
(N'Ký sinh trùng', 132, 1, N'https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?auto=format&fit=crop&w=800&q=80', N'Tâm lý', '2026-09-25', N'Một gia đình nghèo tìm cách sống nhờ vào nhà của một gia đình giàu.', N'Bong Joon-ho'),
(N'Hố đen liên sao', 169, 1, N'https://images.unsplash.com/photo-1419242902214-272b3f66ee7a?auto=format&fit=crop&w=800&q=80', N'Khoa học viễn tưởng', '2026-10-04', N'Nhóm phi hành gia đi qua hố đen để tìm hành tinh mới cho nhân loại.', N'Christopher Nolan'),
(N'Người Dơi', 176, 1, N'https://images.unsplash.com/photo-1509347528160-9a9e33742cdb?auto=format&fit=crop&w=800&q=80', N'Hành động', '2026-10-11', N'Batman điều tra một kẻ sát nhân đang nhắm vào Gotham.', N'Matt Reeves'),
(N'Mọi thứ mọi nơi', 139, 1, N'https://images.unsplash.com/photo-1478720568477-152d9b164e26?auto=format&fit=crop&w=800&q=80', N'Hài', '2026-09-20', N'Một người phụ nữ bình thường phải cứu đa vũ trụ trong lúc khai thuế.', N'Daniel Kwan, Daniel Scheinert'),
(N'Phi công siêu đẳng', 130, 1, N'https://images.unsplash.com/photo-1436491865332-7a61a109cc05?auto=format&fit=crop&w=800&q=80', N'Hành động', '2026-10-06', N'Maverick trở lại để huấn luyện đội phi công cho một nhiệm vụ khó.', N'Joseph Kosinski'),
(N'Thành phố tình yêu', 128, 1, N'https://images.unsplash.com/photo-1485846234645-a62644f84728?auto=format&fit=crop&w=800&q=80', N'Nhạc kịch', '2026-09-28', N'Một nhạc sĩ jazz và một diễn viên trẻ gặp nhau ở Los Angeles.', N'Damien Chazelle'),
(N'Tên cậu là gì', 106, 1, N'https://images.unsplash.com/photo-1578662996442-48f60103fc96?auto=format&fit=crop&w=800&q=80', N'Hoạt hình', '2026-10-08', N'Hai bạn trẻ đổi thân xác qua những giấc mơ và cố tìm lại nhau.', N'Makoto Shinkai');

DECLARE @m TABLE (Title nvarchar(200), MovieId int);
INSERT INTO @m (Title, MovieId)
SELECT Title, Id FROM Movies WHERE Title IN (
 N'Dune: Hành tinh cát - Phần hai', N'Oppenheimer', N'Spider-Man: Không còn nhà', N'Ký sinh trùng',
 N'Hố đen liên sao', N'Người Dơi', N'Mọi thứ mọi nơi', N'Phi công siêu đẳng', N'Thành phố tình yêu', N'Tên cậu là gì');

INSERT INTO ShowTimes (StartTime, MovieId, RoomId)
SELECT s.StartTime, m.MovieId, s.RoomId
FROM (VALUES
 (N'Dune: Hành tinh cát - Phần hai', CAST('2026-10-16T09:00:00' AS datetime2), 1),
 (N'Dune: Hành tinh cát - Phần hai', CAST('2026-10-18T15:00:00' AS datetime2), 2),
 (N'Dune: Hành tinh cát - Phần hai', CAST('2026-10-20T21:00:00' AS datetime2), 3),
 (N'Oppenheimer', CAST('2026-10-16T15:00:00' AS datetime2), 3),
 (N'Oppenheimer', CAST('2026-10-18T09:00:00' AS datetime2), 1),
 (N'Oppenheimer', CAST('2026-10-21T18:00:00' AS datetime2), 2),
 (N'Spider-Man: Không còn nhà', CAST('2026-10-16T18:00:00' AS datetime2), 2),
 (N'Spider-Man: Không còn nhà', CAST('2026-10-19T12:00:00' AS datetime2), 1),
 (N'Spider-Man: Không còn nhà', CAST('2026-10-21T21:00:00' AS datetime2), 3),
 (N'Ký sinh trùng', CAST('2026-10-17T09:00:00' AS datetime2), 2),
 (N'Ký sinh trùng', CAST('2026-10-19T18:00:00' AS datetime2), 3),
 (N'Ký sinh trùng', CAST('2026-10-22T14:00:00' AS datetime2), 1),
 (N'Hố đen liên sao', CAST('2026-10-17T14:00:00' AS datetime2), 3),
 (N'Hố đen liên sao', CAST('2026-10-19T21:00:00' AS datetime2), 1),
 (N'Hố đen liên sao', CAST('2026-10-22T09:00:00' AS datetime2), 2),
 (N'Người Dơi', CAST('2026-10-17T21:00:00' AS datetime2), 1),
 (N'Người Dơi', CAST('2026-10-20T12:00:00' AS datetime2), 2),
 (N'Người Dơi', CAST('2026-10-22T18:00:00' AS datetime2), 3),
 (N'Mọi thứ mọi nơi', CAST('2026-10-18T12:00:00' AS datetime2), 3),
 (N'Mọi thứ mọi nơi', CAST('2026-10-20T18:00:00' AS datetime2), 1),
 (N'Mọi thứ mọi nơi', CAST('2026-10-21T09:00:00' AS datetime2), 2),
 (N'Phi công siêu đẳng', CAST('2026-10-18T21:00:00' AS datetime2), 2),
 (N'Phi công siêu đẳng', CAST('2026-10-20T09:00:00' AS datetime2), 3),
 (N'Phi công siêu đẳng', CAST('2026-10-22T15:00:00' AS datetime2), 1),
 (N'Thành phố tình yêu', CAST('2026-10-19T09:00:00' AS datetime2), 3),
 (N'Thành phố tình yêu', CAST('2026-10-21T14:00:00' AS datetime2), 1),
 (N'Thành phố tình yêu', CAST('2026-10-22T21:00:00' AS datetime2), 2),
 (N'Tên cậu là gì', CAST('2026-10-17T16:30:00' AS datetime2), 1),
 (N'Tên cậu là gì', CAST('2026-10-20T16:30:00' AS datetime2), 3),
 (N'Tên cậu là gì', CAST('2026-10-21T11:00:00' AS datetime2), 1)
) AS s(Title, StartTime, RoomId)
JOIN @m m ON m.Title = s.Title;

SELECT Title, Id FROM Movies WHERE Id > 2 ORDER BY Id;
SELECT COUNT(*) AS NewShowtimes FROM ShowTimes WHERE MovieId > 2;
