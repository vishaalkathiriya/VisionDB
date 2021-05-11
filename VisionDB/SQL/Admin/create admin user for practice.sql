--select * from Practices p

--VisionDB admin user
declare @PracticeId uniqueidentifier = '32B4791A-43CE-4AE5-AE23-44B27AF15C9D'
declare @PasswordHash nvarchar(max) = N'AKH04lz+DMxPXA1hvUmVZ8eMjFZvlzSTFZtaCYmLtKszT8T4wwDE3ETysMb1CEA+pw==' --for admin
declare @SecurityStamp nvarchar(max) = N'c1600a03-9a55-41fb-82ba-44eeedf714f4' --for admin
declare @UserId nvarchar(128) = lower(newid())
declare @Title nvarchar(50) = ''
declare @UserName nvarchar(256) = UPPER(SUBSTRING(cast(@PracticeId as nvarchar(128)), 1, 4))
declare @Firstnames nvarchar(50) = ''
declare @Surname nvarchar(50) = ''
INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [practiceId], [IsOptician], [IsAdmin], [Title], [Firstnames], [Surname], [ListNumber], [SupportCode], [DefaultHomePageEnum], [GOCNumber], [Hidden], [PreventDraggingAppointments], [AutomaticallyResizeCalendar]) 
VALUES (@UserId, null, N'0', @PasswordHash, @SecurityStamp, null, N'0', N'0', null, N'0', N'0', @UserName, @PracticeId, N'0', N'0', @Title, @Firstnames, @Surname, null, UPPER(SUBSTRING(@UserId, 1, 4)), N'0', null, N'1', N'1', N'0')
--grant admin
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, 'e7a28b03-2443-4fb0-a741-e6aa9d4bdba6')