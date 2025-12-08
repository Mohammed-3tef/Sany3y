USE Sany3yDB;
GO

INSERT INTO Addresses (Street, City, Governorate) VALUES
(N'شارع 9', N'المعادي', N'القاهرة'),
(N'شارع التحرير', N'الدقي', N'الجيزة'),
(N'شارع الجيش', N'طنطا', N'الغربية'),
(N'شارع قناة السويس', N'الإسماعيلية', N'الإسماعيلية'),
(N'شارع الثورة', N'مصر الجديدة', N'القاهرة'),
(N'شارع فيصل', N'فيصل', N'الجيزة'),
(N'شارع النصر', N'مدينة نصر', N'القاهرة'),
(N'شارع البحر', N'دمنهور', N'البحيرة'),
(N'شارع صلاح سالم', N'المنصورة', N'الدقهلية'),
(N'شارع بور سعيد', N'حلوان', N'القاهرة'),

(N'شارع النور', N'6 أكتوبر', N'الجيزة'),
(N'شارع الجمهورية', N'المنيا', N'المنيا'),
(N'شارع الاستاد', N'المحلة', N'الغربية'),
(N'شارع جمال عبد الناصر', N'كفر الدوار', N'البحيرة'),
(N'شارع سعد زغلول', N'كفر الشيخ', N'كفر الشيخ'),
(N'شارع البحر الأعظم', N'الجيزة', N'الجيزة'),
(N'شارع النصر', N'السويس', N'السويس'),
(N'شارع الجيش', N'سوهاج', N'سوهاج'),
(N'شارع الحرية', N'الإسكندرية', N'الإسكندرية'),
(N'شارع مسجد السلام', N'سيدي بشر', N'الإسكندرية'),

(N'شارع المستشفى', N'أجا', N'الدقهلية'),
(N'شارع النادي', N'أبو حمص', N'البحيرة'),
(N'شارع المحكمة', N'قليوب', N'القليوبية'),
(N'شارع فريد ندا', N'بنها', N'القليوبية'),
(N'شارع الجيش', N'بدر', N'القاهرة'),
(N'شارع الورش', N'أوسيم', N'الجيزة'),
(N'شارع السوق', N'الرحمانية', N'البحيرة'),
(N'شارع السلام', N'سموحة', N'الإسكندرية'),
(N'شارع المدينة', N'فايد', N'الإسماعيلية'),
(N'شارع 30', N'الزاوية الحمراء', N'القاهرة');
GO

SET IDENTITY_INSERT AspNetUsers ON;
INSERT INTO AspNetUsers (
Id, NationalId, FirstName, LastName, Bio, BirthDate, Gender, IsOnline,
CreatedAt, ProfilePictureId, AddressId, UserName, NormalizedUserName,
Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp,
ConcurrencyStamp, PhoneNumber, TwoFactorEnabled, LockoutEnd, LockoutEnabled,
AccessFailedCount, CategoryID, ExperienceYears, Price, Rating
)
VALUES
(102, 29801012345678, N'يوسف', N'حاتم', N'نجار مسلح', '1995-08-01', 'M', 0, GETDATE(), NULL, 3, 'youssef102', 'YOUSSEF102', '[yo102@mail.com](mailto:yo102@mail.com)', '[YO102@MAIL.COM](mailto:YO102@MAIL.COM)', 1, 'HASH', 'ST', 'CC', '010001102', 0, NULL, 0, 0, 3, 4, 120.00, 4.2),
(103, 29902098765432, N'محمود', N'نبيل', N'فني تبريد', '1990-11-10', 'M', 1, GETDATE(), NULL, 4, 'mahmoud103', 'MAHMOUD103', '[ma103@mail.com](mailto:ma103@mail.com)', '[MA103@MAIL.COM](mailto:MA103@MAIL.COM)', 1, 'HASH', 'ST', 'CC', '010001103', 0, NULL, 0, 0, 4, 6, 250.00, 4.9),
(104, 30005012349876, N'عمر', N'مجدي', N'حداد', '1994-01-15', 'M', 0, GETDATE(), NULL, 5, 'omar104', 'OMAR104', '[om104@mail.com](mailto:om104@mail.com)', '[OM104@MAIL.COM](mailto:OM104@MAIL.COM)', 1, 'HASH', 'ST', 'CC', '010001104', 0, NULL, 0, 0, 5, 3, 130.00, 4.3),
(105, 30206012349111, N'مصطفى', N'عبدالله', N'ميكانيكي سيارات', '1992-07-20', 'M', 1, GETDATE(), NULL, 6, 'most105', 'MOST105', '[most105@mail.com](mailto:most105@mail.com)', '[MOST105@MAIL.COM](mailto:MOST105@MAIL.COM)', 1, 'HASH', 'ST', 'CC', '010001105', 0, NULL, 0, 0, 6, 8, 300.00, 4.7),
(106, 30112345670000, N'سعيد', N'عبدالعظيم', N'عامل بناء', '1989-12-03', 'M', 0, GETDATE(), NULL, 7, 'saeed106', 'SAEED106', '[sd106@mail.com](mailto:sd106@mail.com)', '[SD106@MAIL.COM](mailto:SD106@MAIL.COM)', 1, 'HASH', 'ST', 'CC', '010001106', 0, NULL, 0, 0, 7, 2, 90.00, 4.0),
(107, 30199887766554, N'حسن', N'فتحي', N'فني دش', '1996-09-09', 'M', 1, GETDATE(), NULL, 8, 'has107', 'HAS107', '[hs107@mail.com](mailto:hs107@mail.com)', '[HS107@MAIL.COM](mailto:HS107@MAIL.COM)', 1, 'HASH', 'ST', 'CC', '010001107', 0, NULL, 0, 0, 8, 3, 100.00, 4.1),
(108, 29812345670011, N'مينا', N'فؤاد', N'سباك ممتاز', '1993-03-10', 'M', 1, GETDATE(), NULL, 9, 'mina108', 'MINA108', '[mn108@mail.com](mailto:mn108@mail.com)', '[MN108@MAIL.COM](mailto:MN108@MAIL.COM)', 1, 'HASH', 'ST', 'CC', '010001108', 0, NULL, 0, 0, 9, 5, 180.00, 4.6),
(109, 30155667788990, N'كريم', N'عاطف', N'كهربائي منازل', '1991-04-29', 'M', 1, GETDATE(), NULL, 10, 'kar109', 'KAR109', '[kr109@mail.com](mailto:kr109@mail.com)', '[KR109@MAIL.COM](mailto:KR109@MAIL.COM)', 1, 'HASH', 'ST', 'CC', '010001109', 0, NULL, 0, 0, 10, 6, 160.00, 4.4),
-- تابع بنفس النمط حتى 130
(110, 30211223344551, N'علاء', N'عبدالمنعم', N'نقاش محترف', '1990-11-02', 'M', 0, GETDATE(), NULL, 11, 'alaa110','ALAA110','[al110@mail.com](mailto:al110@mail.com)','[AL110@MAIL.COM](mailto:AL110@MAIL.COM)',1,'HASH','ST','CC','010001110',0,NULL,0,0,1,4,200.00,4.8),
(111, 30322334455662, N'رامي', N'يوسف', N'نجار', '1988-02-12', 'M', 1, GETDATE(), NULL, 12, 'ram111','RAM111','[rm111@mail.com](mailto:rm111@mail.com)','[RM111@MAIL.COM](mailto:RM111@MAIL.COM)',1,'HASH','ST','CC','010001111',0,NULL,0,0,2,5,220.00,4.5),
(112, 30433445566773, N'عمرو', N'جمال', N'سباك ممتاز', '1996-06-13', 'M', 0, GETDATE(), NULL, 13, 'amr112','AMR112','[am112@mail.com](mailto:am112@mail.com)','[AM112@MAIL.COM](mailto:AM112@MAIL.COM)',1,'HASH','ST','CC','010001112',0,NULL,0,0,3,4,140.00,4.2),
(113, 30544556677884, N'باسم', N'مدحت', N'كهربائي', '1987-10-21', 'M', 1, GETDATE(), NULL, 14, 'bas113','BAS113','[bs113@mail.com](mailto:bs113@mail.com)','[BS113@MAIL.COM](mailto:BS113@MAIL.COM)',1,'HASH','ST','CC','010001113',0,NULL,0,0,4,7,300.00,4.9),
(114, 30655667788995, N'وليد', N'عماد', N'فني تكييف', '1992-09-15', 'M', 0, GETDATE(), NULL, 15, 'wal114','WAL114','[wl114@mail.com](mailto:wl114@mail.com)','[WL114@MAIL.COM](mailto:WL114@MAIL.COM)',1,'HASH','ST','CC','010001114',0,NULL,0,0,5,5,210.00,4.3),
(115, 30766778899001, N'شريف', N'مجاهد', N'نجار مسلح', '1995-12-10', 'M', 1, GETDATE(), NULL, 16, 'shr115','SHR115','[sh115@mail.com](mailto:sh115@mail.com)','[SH115@MAIL.COM](mailto:SH115@MAIL.COM)',1,'HASH','ST','CC','010001115',0,NULL,0,0,6,6,170.00,4.4),
(116, 30877889900112, N'إسلام', N'حامد', N'ميكانيكي', '1989-01-27', 'M', 0, GETDATE(), NULL, 17, 'esl116','ESL116','[es116@mail.com](mailto:es116@mail.com)','[ES116@MAIL.COM](mailto:ES116@MAIL.COM)',1,'HASH','ST','CC','010001116',0,NULL,0,0,7,8,260.00,4.7),
(117, 30988990011223, N'طارق', N'فتحي', N'فني دش', '1993-11-08', 'M', 1, GETDATE(), NULL, 18, 'tar117','TAR117','[tr117@mail.com](mailto:tr117@mail.com)','[TR117@MAIL.COM](mailto:TR117@MAIL.COM)',1,'HASH','ST','CC','010001117',0,NULL,0,0,8,3,120.00,4.1),
(118, 31099001122334, N'أيمن', N'رمزي', N'سباك', '1990-05-25', 'M', 1, GETDATE(), NULL, 19, 'aym118','AYM118','[ay118@mail.com](mailto:ay118@mail.com)','[AY118@MAIL.COM](mailto:AY118@MAIL.COM)',1,'HASH','ST','CC','010001118',0,NULL,0,0,9,5,190.00,4.6),
(119, 31110012233445, N'ماركو', N'جرجس', N'كهربائي', '1994-12-12', 'M', 0, GETDATE(), NULL, 20, 'mark119','MARK119','[mk119@mail.com](mailto:mk119@mail.com)','[MK119@MAIL.COM](mailto:MK119@MAIL.COM)',1,'HASH','ST','CC','010001119',0,NULL,0,0,10,4,180.00,4.5),
(120, 31211123344556, N'فادي', N'سمير', N'نقاش', '1996-07-01','M',1,GETDATE(),NULL,21,'fady120','FADY120','[fd120@mail.com](mailto:fd120@mail.com)','[FD120@MAIL.COM](mailto:FD120@MAIL.COM)',1,'HASH','ST','CC','010001120',0,NULL,0,0,1,3,110.00,4.1),
(121, 31312234455667, N'حازم', N'مكرم', N'نجار', '1987-04-22','M',0,GETDATE(),NULL,22,'haz121','HAZ121','[hz121@mail.com](mailto:hz121@mail.com)','[HZ121@MAIL.COM](mailto:HZ121@MAIL.COM)',1,'HASH','ST','CC','010001121',0,NULL,0,0,2,7,230.00,4.6),
(122, 31413345566778, N'نبيل', N'حنفي', N'كهربائي', '1991-08-31','M',1,GETDATE(),NULL,23,'nbl122','NBL122','[nb122@mail.com](mailto:nb122@mail.com)','[NB122@MAIL.COM](mailto:NB122@MAIL.COM)',1,'HASH','ST','CC','010001122',0,NULL,0,0,3,6,250.00,4.8),
(123, 31514456677889, N'محمود', N'يسري', N'ميكانيكي', '1989-06-19','M',0,GETDATE(),NULL,24,'mh123','MH123','[mh123@mail.com](mailto:mh123@mail.com)','[MH123@MAIL.COM](mailto:MH123@MAIL.COM)',1,'HASH','ST','CC','010001123',0,NULL,0,0,4,8,300.00,4.9),
(124, 31615567788990, N'حمزة', N'جمال', N'فني تكييف', '1995-10-03','M',1,GETDATE(),NULL,25,'ham124','HAM124','[hm124@mail.com](mailto:hm124@mail.com)','[HM124@MAIL.COM](mailto:HM124@MAIL.COM)',1,'HASH','ST','CC','010001124',0,NULL,0,0,5,5,200.00,4.2),
(125, 31716678899001, N'رأفت', N'حسين', N'سباك', '1993-01-14','M',1,GETDATE(),NULL,26,'raf125','RAF125','[rf125@mail.com](mailto:rf125@mail.com)','[RF125@MAIL.COM](mailto:RF125@MAIL.COM)',1,'HASH','ST','CC','010001125',0,NULL,0,0,6,4,180.00,4.5),
(126, 31817789900112, N'سامي', N'فاروق', N'نجار', '1992-04-10','M',0,GETDATE(),NULL,27,'sam126','SAM126','[sm126@mail.com](mailto:sm126@mail.com)','[SM126@MAIL.COM](mailto:SM126@MAIL.COM)',1,'HASH','ST','CC','010001126',0,NULL,0,0,7,6,190.00,4.4),
(127, 31918890011223, N'مينا', N'حليم', N'كهربائي', '1994-03-21','M',0,GETDATE(),NULL,28,'min127','MIN127','[mn127@mail.com](mailto:mn127@mail.com)','[MN127@MAIL.COM](mailto:MN127@MAIL.COM)',1,'HASH','ST','CC','010001127',0,NULL,0,0,8,3,130.00,4.0),
(128, 32019901122334, N'شادي', N'فوزي', N'نقاش', '1990-02-11','M',1,GETDATE(),NULL,29,'sh128','SH128','[sh128@mail.com](mailto:sh128@mail.com)','[SH128@MAIL.COM](mailto:SH128@MAIL.COM)',1,'HASH','ST','CC','010001128',0,NULL,0,0,9,7,240.00,4.6),
(129, 32120012233445, N'جورج', N'متياس', N'فني دش', '1996-06-08','M',0,GETDATE(),NULL,30,'ge129','GE129','[ge129@mail.com](mailto:ge129@mail.com)','[GE129@MAIL.COM](mailto:GE129@MAIL.COM)',1,'HASH','ST','CC','010001129',0,NULL,0,0,10,2,100.00,3.9),
(130, 32221123344556, N'علي', N'خالد', N'سباك', '1995-09-18','M',1,GETDATE(),NULL,31,'ali130','ALI130','[al130@mail.com](mailto:al130@mail.com)','[AL130@MAIL.COM](mailto:AL130@MAIL.COM)',1,'HASH','ST','CC','010001130',0,NULL,0,0,1,5,170.00,4.2);
GO
SET IDENTITY_INSERT AspNetUsers OFF;

INSERT INTO AspNetUserRoles(UserId, RoleId)
VALUES
(102, 2),
(103, 2),
(104, 2),
(105, 2),
(106, 2),
(107, 2),
(108, 2),
(109, 2),
(110, 2),
(111, 2),
(112, 2),
(113, 2),
(114, 2),
(115, 2),
(116, 2),
(117, 2),
(118, 2),
(119, 2),
(120, 2),
(121, 2),
(122, 2),
(123, 2),
(124, 2),
(125, 2),
(126, 2),
(127, 2),
(128, 2),
(129, 2),
(130, 2);
GO