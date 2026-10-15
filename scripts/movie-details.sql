IF COL_LENGTH('Movies', 'Country') IS NULL ALTER TABLE Movies ADD Country nvarchar(80) NULL;
IF COL_LENGTH('Movies', 'Language') IS NULL ALTER TABLE Movies ADD Language nvarchar(120) NULL;
IF COL_LENGTH('Movies', 'Cast') IS NULL ALTER TABLE Movies ADD [Cast] nvarchar(500) NULL;
IF COL_LENGTH('Movies', 'TrailerUrl') IS NULL ALTER TABLE Movies ADD TrailerUrl nvarchar(300) NULL;
IF COL_LENGTH('Movies', 'Rating') IS NULL ALTER TABLE Movies ADD Rating decimal(3,1) NULL;
GO

UPDATE Movies SET
  PosterUrl = N'https://image.tmdb.org/t/p/w500/kyeqWdyUXW608qlYkRqosgbbJyK.jpg',
  Genre = N'Khoa học viễn tưởng, Phiêu lưu',
  Country = N'Mỹ',
  Language = N'Tiếng Anh - Phụ đề Việt',
  Director = N'James Cameron',
  [Cast] = N'Sam Worthington, Zoe Saldana, Sigourney Weaver',
  TrailerUrl = N'https://www.youtube.com/watch?v=5PSNL1qE6VY',
  Rating = 7.6,
  Description = N'Trên hành tinh Pandora, Jake Sully bước vào thế giới của người Na''vi và phải chọn phe khi con người muốn khai thác tài nguyên của họ. Phim theo chân hành trình từ một người lạ trở thành một phần của bộ tộc, giữa rừng già, những sinh vật bay và cuộc chiến để giữ quê hương.'
WHERE Title = N'Avatar';

UPDATE Movies SET
  PosterUrl = N'https://image.tmdb.org/t/p/w500/oYuLEt3zVCKq57qu2F8dT7NIa6f.jpg',
  Genre = N'Hành động, Khoa học viễn tưởng',
  Country = N'Mỹ',
  Language = N'Tiếng Anh - Phụ đề Việt',
  Director = N'Christopher Nolan',
  [Cast] = N'Leonardo DiCaprio, Joseph Gordon-Levitt, Elliot Page',
  TrailerUrl = N'https://www.youtube.com/watch?v=YoHD9XEInc0',
  Rating = 8.8,
  Description = N'Một tên trộm chuyên cấy giấc mơ được thuê để trồng một ý tưởng vào tâm trí người thừa kế tập đoàn. Mỗi tầng mơ sâu hơn, thời gian trôi khác đi, và ranh giới giữa ký ức với thực tại dần biến mất.'
WHERE Title = N'Inception';

UPDATE Movies SET
  PosterUrl = N'https://image.tmdb.org/t/p/w500/1pdfLvkbY9ohJlCjQH2CZjjYVvJ.jpg',
  Genre = N'Khoa học viễn tưởng, Phiêu lưu',
  Country = N'Mỹ',
  Language = N'Tiếng Anh - Phụ đề Việt',
  [Cast] = N'Timothée Chalamet, Zendaya, Rebecca Ferguson, Austin Butler',
  TrailerUrl = N'https://www.youtube.com/watch?v=Way9Dexny3w',
  Rating = 8.5,
  Description = N'Paul Atreides hợp lực với người Fremen trên sa mạc Arrakis. Giữa những cơn bão cát và lời tiên tri, anh phải đối đầu đế chế đang chiếm gia vị, đồng thời giữ cho bộ tộc không bị nuốt chửng bởi chiến tranh.'
WHERE Title = N'Dune: Hành tinh cát - Phần hai';

UPDATE Movies SET
  PosterUrl = N'https://image.tmdb.org/t/p/w500/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg',
  Genre = N'Tiểu sử, Chính kịch',
  Country = N'Mỹ',
  Language = N'Tiếng Anh - Phụ đề Việt',
  [Cast] = N'Cillian Murphy, Emily Blunt, Robert Downey Jr.',
  TrailerUrl = N'https://www.youtube.com/watch?v=uYPbbksJxIg',
  Rating = 8.3,
  Description = N'J. Robert Oppenheimer dẫn dắt dự án chế tạo vũ khí hạt nhân trong Thế chiến thứ hai. Sau ánh chớp đầu tiên, ông phải sống cùng câu hỏi về trách nhiệm của khoa học và cái giá của quyền lực.'
WHERE Title = N'Oppenheimer';

UPDATE Movies SET
  PosterUrl = N'https://image.tmdb.org/t/p/w500/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg',
  Genre = N'Hành động, Phiêu lưu',
  Country = N'Mỹ',
  Language = N'Tiếng Anh - Phụ đề Việt',
  [Cast] = N'Tom Holland, Zendaya, Benedict Cumberbatch',
  TrailerUrl = N'https://www.youtube.com/watch?v=JfVOs4VSpmA',
  Rating = 8.2,
  Description = N'Peter Parker nhờ Doctor Strange xóa ký ức của mọi người về danh tính của mình. Phép thuật vỡ vụn, kéo những kẻ thù từ vũ trụ khác đến New York, và Peter phải chọn giữa bạn bè với cả thành phố.'
WHERE Title = N'Spider-Man: Không còn nhà';

UPDATE Movies SET
  PosterUrl = N'https://image.tmdb.org/t/p/w500/7IiTTgloJzvGI1TAYymCfbfl3vT.jpg',
  Genre = N'Tâm lý, Hài, Kinh dị',
  Country = N'Hàn Quốc',
  Language = N'Tiếng Hàn - Phụ đề Việt',
  [Cast] = N'Song Kang-ho, Lee Sun-kyun, Cho Yeo-jeong, Choi Woo-shik',
  TrailerUrl = N'https://www.youtube.com/watch?v=5xH0HfJHsaY',
  Rating = 8.5,
  Description = N'Gia đình Kim thất nghiệp lần lượt xin vào làm cho nhà Park giàu có. Khi hai thế giới sống chung một mái nhà, những bí mật dưới tầng hầm và khoảng cách giàu nghèo đẩy câu chuyện sang một hướng không thể quay lại.'
WHERE Title = N'Ký sinh trùng';

UPDATE Movies SET
  PosterUrl = N'https://image.tmdb.org/t/p/w500/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg',
  Genre = N'Khoa học viễn tưởng, Phiêu lưu',
  Country = N'Mỹ',
  Language = N'Tiếng Anh - Phụ đề Việt',
  [Cast] = N'Matthew McConaughey, Anne Hathaway, Jessica Chastain',
  TrailerUrl = N'https://www.youtube.com/watch?v=zSWdZVtXT7E',
  Rating = 8.7,
  Description = N'Trái Đất sắp cạn nguồn sống. Cooper dẫn một nhóm phi hành gia xuyên qua hố đen để tìm hành tinh mới, trong khi thời gian trên tàu và thời gian của con gái ông không còn chạy cùng một nhịp.'
WHERE Title = N'Hố đen liên sao';

UPDATE Movies SET
  PosterUrl = N'https://image.tmdb.org/t/p/w500/74xTEgt7R36Fpooo50r9T25onhq.jpg',
  Genre = N'Hành động, Tội phạm',
  Country = N'Mỹ',
  Language = N'Tiếng Anh - Phụ đề Việt',
  [Cast] = N'Robert Pattinson, Zoë Kravitz, Paul Dano',
  TrailerUrl = N'https://www.youtube.com/watch?v=mqqft2x_Aa4',
  Rating = 7.8,
  Description = N'Năm thứ hai làm Người Dơi, Bruce Wayne lần theo một chuỗi án mạng nhắm vào những người quyền lực ở Gotham. Mỗi manh mối kéo anh gần hơn với quá khứ của gia đình mình.'
WHERE Title = N'Người Dơi';

UPDATE Movies SET
  PosterUrl = N'https://image.tmdb.org/t/p/w500/w3LxiVYdWWRvEVdn5RYq6jIqkb1.jpg',
  Genre = N'Hài, Hành động, Khoa học viễn tưởng',
  Country = N'Mỹ',
  Language = N'Tiếng Anh - Phụ đề Việt',
  [Cast] = N'Michelle Yeoh, Ke Huy Quan, Stephanie Hsu',
  TrailerUrl = N'https://www.youtube.com/watch?v=wxN1T1uxQ2g',
  Rating = 7.8,
  Description = N'Evelyn, chủ tiệm giặt ủi, phát hiện mình có thể nhảy giữa các phiên bản của chính mình. Giữa đống hóa đơn và gia đình sắp tan, bà phải ngăn một mối nguy đang xé đa vũ trụ.'
WHERE Title = N'Mọi thứ mọi nơi';

UPDATE Movies SET
  PosterUrl = N'https://image.tmdb.org/t/p/w500/62HCnUTziyWcpDaBO2i1DX17ljH.jpg',
  Genre = N'Hành động, Phiêu lưu',
  Country = N'Mỹ',
  Language = N'Tiếng Anh - Phụ đề Việt',
  [Cast] = N'Tom Cruise, Miles Teller, Jennifer Connelly',
  TrailerUrl = N'https://www.youtube.com/watch?v=giXco2jaZ_4',
  Rating = 8.3,
  Description = N'Maverick được gọi lại để huấn luyện một đội phi công trẻ cho nhiệm vụ gần như không thể. Trên không, tốc độ và những quyết định trong vài giây quyết định ai trở về.'
WHERE Title = N'Phi công siêu đẳng';

UPDATE Movies SET
  PosterUrl = N'https://image.tmdb.org/t/p/w500/uDO8zWDhfWwoFdKS4fzkUJt0Rf0.jpg',
  Genre = N'Nhạc kịch, Tình cảm',
  Country = N'Mỹ',
  Language = N'Tiếng Anh - Phụ đề Việt',
  [Cast] = N'Ryan Gosling, Emma Stone',
  TrailerUrl = N'https://www.youtube.com/watch?v=0pdqf4P9MB8',
  Rating = 8.0,
  Description = N'Một nhạc sĩ jazz và một diễn viên trẻ gặp nhau ở Los Angeles. Họ theo đuổi giấc mơ riêng, và mỗi lựa chọn nghề nghiệp kéo họ gần hơn rồi cũng đẩy họ ra xa.'
WHERE Title = N'Thành phố tình yêu';

UPDATE Movies SET
  PosterUrl = N'https://image.tmdb.org/t/p/w500/q719jXXEzOoYaps6babgKnONONX.jpg',
  Genre = N'Hoạt hình, Tình cảm',
  Country = N'Nhật Bản',
  Language = N'Tiếng Nhật - Phụ đề Việt',
  [Cast] = N'Ryunosuke Kamiki, Mone Kamishiraishi',
  TrailerUrl = N'https://www.youtube.com/watch?v=xU47nhruN-Q',
  Rating = 8.4,
  Description = N'Mitsuha ở thị trấn miền núi và Taki ở Tokyo bất ngờ đổi thân xác qua những giấc mơ. Họ để lại ghi chú cho nhau, rồi phải tìm ra người kia trước khi sao băng vụt qua.'
WHERE Title = N'Tên cậu là gì';
