declare @PracticeId uniqueidentifier = '9D2409B9-85A6-4F18-B062-D17E0A2C6F68'
declare @PasswordHash nvarchar(max) 
declare @SecurityStamp nvarchar(max) 
declare @UserId nvarchar(128) = lower(newid())
declare @Title nvarchar(50) = ''
declare @UserName nvarchar(256) = ''
declare @Firstnames nvarchar(50) = ''
declare @Surname nvarchar(50) = ''
set @PasswordHash = N'ADndbQ7tH1/8wqeIaoL9Ty/4+CHbrq2oTfy21DfW9BaeJKkqRrvIGW68w+B2GKdGcA==' --for default password
set @SecurityStamp = N'4e7d3500-0344-4a20-9bb6-8baeb70e961f' --for default password
set @UserId = lower(newid())
set @Title = 'Mr'
set @UserName = 'Mark4'
set @Firstnames = 'Mark'
set @Surname = 'Young'
INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [practiceId], [IsOptician], [IsAdmin], [Title], [Firstnames], [Surname], [ListNumber], [SupportCode], [DefaultHomePageEnum], [GOCNumber], [Hidden], [PreventDraggingAppointments], [AutomaticallyResizeCalendar]) 
VALUES (@UserId, null, N'0', @PasswordHash, @SecurityStamp, null, N'0', N'0', null, N'0', N'0', @UserName, @PracticeId, N'0', N'0', @Title, @Firstnames, @Surname, null, UPPER(SUBSTRING(@UserId, 1, 4)), N'0', null, N'0', N'1', N'0')
