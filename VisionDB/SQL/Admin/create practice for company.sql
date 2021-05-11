BEGIN TRANSACTION

declare @CompanyId uniqueidentifier = 'E7BB2F17-B176-4780-8AD2-00A6F93186A8'
declare @CreatedByUserId uniqueidentifier = '667cbe27-170b-42a0-92cb-18876ad2c8a2' --admin account Id
-- create practice
declare @PracticeId uniqueidentifier = 'D58EC004-63DE-4D4A-A272-11C961D7A755'
declare @PracticeName nvarchar(100) = 'practice name'
declare @PracticeAddress nvarchar(1000) = 'address'
declare @PracticePostcode nvarchar(100) = 'postcode'
declare @PracticeTel nvarchar(100) = 'tel'
declare @PracticeRef nvarchar(4) = upper(substring(cast(@PracticeId as nvarchar(50)), 0, 5))
declare @WorkDayStart datetime = '2014-01-01 09:00:00' --only time component is used
declare @WorkDayEnd datetime = '2014-01-01 17:00:00' --only time component is used
declare @PracticeNumber int 
select @PracticeNumber = max(PracticeNumber) + 1 from Practices
declare @LastInvoiceNumber int = 112

insert into Practices (Id, Name, Address, Postcode, Tel, PracticeRef, WorkDayStart, WorkDayEnd, PracticeNumber, LastInvoiceNumber, company_Id, CreatedByUser_Id)
values (@PracticeId, @PracticeName, @PracticeAddress, @PracticePostcode, @PracticeTel, @PracticeRef, @WorkDayStart, @WorkDayEnd, @PracticeNumber, @LastInvoiceNumber, @CompanyId, @CreatedByUserId)

--add messages to company inventory
INSERT INTO SMSInventory (company_Id, Quantity) VALUES (@CompanyId, 0)

--VisionDB admin user
declare @PasswordHash nvarchar(max) = N'AKH04lz+DMxPXA1hvUmVZ8eMjFZvlzSTFZtaCYmLtKszT8T4wwDE3ETysMb1CEA+pw==' --for admin
declare @SecurityStamp nvarchar(max) = N'c1600a03-9a55-41fb-82ba-44eeedf714f4' --for admin
declare @UserId nvarchar(128) = lower(newid())
declare @Title nvarchar(50) = ''
declare @UserName nvarchar(256) = ''
declare @Firstnames nvarchar(50) = ''
declare @Surname nvarchar(50) = ''
INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [practiceId], [IsOptician], [IsAdmin], [Title], [Firstnames], [Surname], [ListNumber], [SupportCode], [DefaultHomePageEnum], [GOCNumber], [Hidden], [PreventDraggingAppointments], [AutomaticallyResizeCalendar]) 
VALUES (@UserId, null, N'0', @PasswordHash, @SecurityStamp, null, N'0', N'0', null, N'0', N'0', @UserName, @PracticeId, N'0', N'0', @Title, @Firstnames, @Surname, null, UPPER(SUBSTRING(@UserId, 1, 4)), N'0', null, N'0', N'1', N'0')
--grant admin
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, 'e7a28b03-2443-4fb0-a741-e6aa9d4bdba6')

-- create user accounts
-- password: hawkeyes
set @PasswordHash = N'ADndbQ7tH1/8wqeIaoL9Ty/4+CHbrq2oTfy21DfW9BaeJKkqRrvIGW68w+B2GKdGcA==' --for default password
set @SecurityStamp = N'4e7d3500-0344-4a20-9bb6-8baeb70e961f' --for default password
--user 1
set @UserId = lower(newid())
set @Title = 'Mr'
set @UserName = 'Usman'
set @Firstnames = 'Usman'
set @Surname = 'Syed'
INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [practiceId], [IsOptician], [IsAdmin], [Title], [Firstnames], [Surname], [ListNumber], [SupportCode], [DefaultHomePageEnum], [GOCNumber], [Hidden], [PreventDraggingAppointments], [AutomaticallyResizeCalendar]) 
VALUES (@UserId, null, N'0', @PasswordHash, @SecurityStamp, null, N'0', N'0', null, N'0', N'0', @UserName, @PracticeId, N'0', N'0', @Title, @Firstnames, @Surname, null, UPPER(SUBSTRING(@UserId, 1, 4)), N'0', null, N'1', N'1', N'0')
--grant admin
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, 'e7a28b03-2443-4fb0-a741-e6aa9d4bdba6')

--user 2
set @UserId = lower(newid())
set @Title = 'Ms'
set @UserName = 'Diane'
set @Firstnames = 'Diane'
set @Surname = 'Holmes'
INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [practiceId], [IsOptician], [IsAdmin], [Title], [Firstnames], [Surname], [ListNumber], [SupportCode], [DefaultHomePageEnum], [GOCNumber], [Hidden], [PreventDraggingAppointments], [AutomaticallyResizeCalendar]) 
VALUES (@UserId, null, N'0', @PasswordHash, @SecurityStamp, null, N'0', N'0', null, N'0', N'0', @UserName, @PracticeId, N'0', N'0', @Title, @Firstnames, @Surname, null, UPPER(SUBSTRING(@UserId, 1, 4)), N'0', null, N'0', N'1', N'0')

--user 3
set @UserId = lower(newid())
set @Title = 'Ms'
set @UserName = 'Michelle'
set @Firstnames = 'Michelle'
set @Surname = 'Water'
INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [practiceId], [IsOptician], [IsAdmin], [Title], [Firstnames], [Surname], [ListNumber], [SupportCode], [DefaultHomePageEnum], [GOCNumber], [Hidden], [PreventDraggingAppointments], [AutomaticallyResizeCalendar]) 
VALUES (@UserId, null, N'0', @PasswordHash, @SecurityStamp, null, N'0', N'0', null, N'0', N'0', @UserName, @PracticeId, N'0', N'0', @Title, @Firstnames, @Surname, null, UPPER(SUBSTRING(@UserId, 1, 4)), N'0', null, N'0', N'1', N'0')

--user 4
set @UserId = lower(newid())
set @Title = 'Mr'
set @UserName = 'Mark2'
set @Firstnames = 'Mark'
set @Surname = 'Young'
INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName], [practiceId], [IsOptician], [IsAdmin], [Title], [Firstnames], [Surname], [ListNumber], [SupportCode], [DefaultHomePageEnum], [GOCNumber], [Hidden], [PreventDraggingAppointments], [AutomaticallyResizeCalendar]) 
VALUES (@UserId, null, N'0', @PasswordHash, @SecurityStamp, null, N'0', N'0', null, N'0', N'0', @UserName, @PracticeId, N'0', N'0', @Title, @Firstnames, @Surname, null, UPPER(SUBSTRING(@UserId, 1, 4)), N'0', null, N'0', N'1', N'0')

select 'Practice Ref', upper(substring(cast(@PracticeId as nvarchar(50)), 0, 5))
union
select 'Practice Id', lower(@PracticeId)
union 
select 'Company Id', lower(@CompanyId)

COMMIT TRANSACTION
GO