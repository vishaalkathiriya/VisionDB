/* Creates the VisionDB database */

CREATE DATABASE [VisionDBDev]
GO
USE [VisionDBDev]
GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_Backup_Database]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_Backup_Database] 
	@path VARCHAR(256) = 'C:\click\database backups\VisionDB\' -- path on server
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @name VARCHAR(50) = DB_NAME()
	DECLARE @fileName VARCHAR(256) = lower(replace(@@SERVERNAME, '\', ' ')) + ' ' + DB_NAME()
	DECLARE @fileDate VARCHAR(20) = REPLACE(CONVERT(VARCHAR(20),GETDATE(),120), ':', '') -- format YYYY-MM-DD HHMMSS

	SET @fileName = @path + @fileName + ' ' + @fileDate + '.bak'  
	BACKUP DATABASE @name TO DISK = @fileName  
END

GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_CustomReport_6978_Lab]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_CustomReport_6978_Lab] 
	@practice_Id uniqueidentifier,
	@start datetime,
	@end datetime
AS
BEGIN
	SET NOCOUNT ON;

	declare @InvoiceId uniqueidentifier

	DECLARE @output TABLE(
	Id uniqueidentifier,
	invoice_date datetime,
	patient_name  NVARCHAR(200),
	frame NVARCHAR(500),
	lenses NVARCHAR(500),
	tint nvarchar(200),
	rpd float,
	lpd float,
	rheight float,
	lheight float,
	rsph float,
	lsph float,
	rcyl float,
	lcyl float,
	raxis float,
	laxis float,
	radd float,
	ladd float
	)

	DECLARE @ReportData CURSOR
	SET @ReportData = CURSOR FOR
	select 
	i.Id from Invoices i 
	inner join Customers c on c.Id = i.customer_Id
	where c.practice_Id = @practice_Id and i.Deleted is null and i.InvoiceDate >= @start and i.InvoiceDate <= @end
	OPEN @ReportData
	FETCH NEXT
	FROM @ReportData INTO @InvoiceId
	WHILE @@FETCH_STATUS = 0
	BEGIN

	declare @customerId uniqueidentifier = null
	declare @invoice_date datetime = null
	select @customerId = i.customer_Id,  @invoice_date = i.InvoiceDate from Invoices i where i.Id = @InvoiceId 

	declare @patient_name nvarchar(500) = null
	select @patient_name = isnull(c.Title, '') + ' ' + c.Firstnames + ' ' + c.Surname from Customers c where c.Id = @customerId

	declare @frame nvarchar(500) = null
	SELECT @frame = COALESCE(@frame +', ' ,'') + Name + isnull(CHAR(13) + 'colour: ' + Colour, '')
	FROM InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 1

	declare @lenses nvarchar(500) = null
	SELECT @lenses = COALESCE(@lenses+', ' ,'') + Name
	FROM InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 2

	declare @tint nvarchar(500) = null
	SELECT @tint = COALESCE(@tint + ', ' ,'') + Name + ' colour: ' + Colour
	FROM InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 9

	declare @rpd float = null
	declare @lpd float = null
	declare @rheight float = null
	declare @lheight float = null
	declare @rsph float = null
	declare @lsph float = null
	declare @rcyl float = null
	declare @lcyl float = null
	declare @raxis float = null
	declare @laxis float = null
	declare @radd float = null
	declare @ladd float = null

	select top 1 
	@rpd = ee.PDRight, 
	@lpd = ee.PDLeft,
	@rheight = ee.RHeight,
	@lheight = ee.LHeight,
	@rsph = ee.RSphericalDist,
	@lsph = ee.LSphericalDist,
	@rcyl = ee.RCylDist,
	@lcyl = ee.LCylDist,
	@raxis = ee.RAxisDist,
	@laxis = ee.LAxisDist,
	@radd = ee.NAddRight,
	@ladd = ee.NAddRight
	from EyeExams ee
	where customer_Id = @customerId and Deleted is null 
	order by ee.CreatedTimestamp desc

	insert into @output (Id, invoice_date, patient_name, frame, lenses, tint, rpd, lpd, rheight, lheight, rsph, lsph, rcyl, lcyl, raxis, laxis, radd, ladd) 
	values (@InvoiceId, @invoice_date, @patient_name, @frame, @lenses, @tint, @rpd, @lpd, @rheight, @lheight, @rsph, @lsph, @rcyl, @lcyl, @raxis, @laxis, @radd, @ladd)

	FETCH NEXT
	FROM @ReportData INTO @InvoiceId
	END
	CLOSE @ReportData
	DEALLOCATE @ReportData

	select * from @output order by invoice_date 

END

GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_CustomReport_F55D_DailyFinancialSummary]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_CustomReport_F55D_DailyFinancialSummary]
	@practiceId uniqueidentifier,
	@inputdate datetime
AS
BEGIN
	SET NOCOUNT ON;

	set @inputdate = DATEADD(dd, 0, DATEDIFF(dd, 0, @inputdate))

	select pt.ProductName, 
	round(sum(id.UnitPrice * id.Quantity * (1 / (1 + (id.VATRate / 100))) * (1 - (id.DiscountPercentage / 100))), 2) total_exc_vat, 
	sum(round((id.UnitPrice * id.Quantity * (1 - (id.DiscountPercentage / 100))), 2)) total_inc_vat, 
	pt.NegativeValue, pt.Id 
	from InvoiceDetails id
	inner join Invoices i on i.Id = id.invoice_Id
	left join Customers c on c.Id = i.customer_Id
	inner join Products p on p.Id = id.product_Id
	inner join ProductTypes pt on pt.Id = p.ProductTypeEnum
	where i.practice_Id = @practiceId and id.Added > @inputdate and id.Added < DATEADD(day, 1, @inputdate)
	and id.Deleted is null and i.Deleted is null
	and p.ProductTypeEnum in (0, 1, 2, 3, 4, 6, 7, 8, 9, 10)
	group by pt.Id, pt.ProductName, pt.NegativeValue
	union
	select p.Name, 
	round(sum(id.UnitPrice * id.Quantity * (1 / (1 + (id.VATRate / 100))) * (1 - (id.DiscountPercentage / 100))), 2) total_exc_vat, 
	sum(round((id.UnitPrice * id.Quantity * (1 - (id.DiscountPercentage / 100))), 2)) total_inc_vat, 
	pt.NegativeValue, pt.Id 
	from InvoiceDetails id
	inner join Invoices i on i.Id = id.invoice_Id
	left join Customers c on c.Id = i.customer_Id
	inner join Products p on p.Id = id.product_Id
	inner join ProductTypes pt on pt.Id = p.ProductTypeEnum
	where i.practice_Id = @practiceId and id.Added > @inputdate and id.Added < DATEADD(day, 1, @inputdate)
	and id.Deleted is null and i.Deleted is null
	and p.ProductTypeEnum = 5
	group by pt.Id, p.Name, pt.NegativeValue
	order by pt.Id

END
GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_DailyFinancialSummary]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_DailyFinancialSummary]
	@practiceId uniqueidentifier,
	@inputdate datetime
AS
BEGIN
	SET NOCOUNT ON;

	set @inputdate = DATEADD(dd, 0, DATEDIFF(dd, 0, @inputdate))

	select pt.ProductName, 
	round(sum(id.UnitPrice * id.Quantity * (1 / (1 + (id.VATRate / 100))) * (1 - (id.DiscountPercentage / 100))), 2) total_exc_vat, 
	sum(round((id.UnitPrice * id.Quantity * (1 - (id.DiscountPercentage / 100))), 2)) total_inc_vat, 
	pt.NegativeValue, pt.Id 
	from InvoiceDetails id
	inner join Invoices i on i.Id = id.invoice_Id
	left join Customers c on c.Id = i.customer_Id
	inner join Products p on p.Id = id.product_Id
	inner join ProductTypes pt on pt.Id = p.ProductTypeEnum
	where i.practice_Id = @practiceId and id.Added > @inputdate and id.Added < DATEADD(day, 1, @inputdate)
	and id.Deleted is null and p.ProductTypeEnum != 5 and i.Deleted is null
	group by pt.Id, pt.ProductName, pt.NegativeValue
	union
	select p.Name, 
	round(sum(id.UnitPrice * id.Quantity * (1 / (1 + (id.VATRate / 100))) * (1 - (id.DiscountPercentage / 100))), 2) total_exc_vat, 
	sum(round((id.UnitPrice * id.Quantity * (1 - (id.DiscountPercentage / 100))), 2)) total_inc_vat, 
	pt.NegativeValue, pt.Id 
	from InvoiceDetails id
	inner join Invoices i on i.Id = id.invoice_Id
	left join Customers c on c.Id = i.customer_Id
	inner join Products p on p.Id = id.product_Id
	inner join ProductTypes pt on pt.Id = p.ProductTypeEnum
	where i.practice_Id = @practiceId and id.Added > @inputdate and id.Added < DATEADD(day, 1, @inputdate)
	and id.Deleted is null and p.ProductTypeEnum = 5 and i.Deleted is null
	group by pt.Id, p.Name, pt.NegativeValue
	order by pt.Id

END
GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_DispensingRecord]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_DispensingRecord]
	@invoiceId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	declare @eyeExamsId uniqueidentifier
	select top 1 @eyeExamsId = ee.Id from EyeExams ee
	inner join Invoices i on i.customer_Id = ee.customer_Id
	where i.Id = @invoiceId and ee.Deleted is null
	order by ee.TestDateAndTime desc

	exec sp_VDB_EyeExamData @eyeExamsId
END

GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_EyeExamData]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_EyeExamData]
	@EyeExamId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

    select
	c.Title,
	c.PreviousSurname,
	c.Surname,
	c.Firstnames,
	c.Title + ' ' + c.Firstnames + ' ' + c.Surname FullName,
	c.DOB,
	replace(c.Address, char(13) + char(10), ' ') Address,
	c.Postcode,
	ee.TestDateAndTime,
	c.NHSNo,
	c.NINo,
	c.PreviousEyeExamDate, 
	c.NextDueDateEyeExam,
	dbo.fn_IsOver60(c.DOB) Over60,
	dbo.fn_IsUnder16(c.DOB) Under16,
	c.Doctor,
	c.GPpracticename,
	replace(c.GPpracticeaddress, char(13) + char(10), ' ') GPpracticeaddress,
	c.GPpracticepostcode,
	op.Title + ' ' + op.Firstnames + ' ' + op.Surname OpticianName,
	op.ListNumber OpticianListNumber,
	p.Name PracticeName,
	replace(p.Address, char(13) + char(10), ' ') PracticeAddress,
	p.Postcode PracticePostcode,
	p.Tel PracticeTel,
	p.Email PracticeEmail,
	dbo.fn_VDB_SignedNumeric2DP(ee.RSphericalDist) RSphericalDist,
	dbo.fn_VDB_SignedNumeric2DP(ee.LSphericalDist) LSphericalDist,
	dbo.fn_VDB_SignedNumeric2DP(ee.RCylDist) RCylDist,
	dbo.fn_VDB_SignedNumeric2DP(ee.LCylDist) LCylDist,
	dbo.fn_VDB_Numeric0DP(ee.RAxisDist) RAxisDist,
	dbo.fn_VDB_Numeric0DP(ee.LAxisDist) LAxisDist,
	isnull(dbo.fn_VDB_SignedNumeric2DP(ee.RPrismDistH), '') + ' / ' + isnull(dbo.fn_VDB_SignedNumeric2DP(ee.RPrismDistV), '') RPrismDist,
	isnull(dbo.fn_VDB_SignedNumeric2DP(ee.LPrismDistH), '') + ' / ' + isnull(dbo.fn_VDB_SignedNumeric2DP(ee.LPrismDistV), '') LPrismDist,
	bRBaseDistH.Name + ' / ' + bRBaseDistV.Name RBaseDist,
	bLBaseDistH.Name + ' / ' + bLBaseDistV.Name LBaseDist,
	dbo.fn_VDB_SignedNumeric2DP(ee.RSphericalNear) RSphericalNear,
	dbo.fn_VDB_SignedNumeric2DP(ee.LSphericalNear) LSphericalNear,
	dbo.fn_VDB_SignedNumeric2DP(ee.RCylNear) RCylNear,
	dbo.fn_VDB_SignedNumeric2DP(ee.LCylNear) LCylNear,
	dbo.fn_VDB_Numeric0DP(ee.RAxisNear) RAxisNear,
	dbo.fn_VDB_Numeric0DP(ee.LAxisNear) LAxisNear,
	isnull(dbo.fn_VDB_SignedNumeric2DP(ee.RPrismNearH), '') + ' / ' + isnull(dbo.fn_VDB_SignedNumeric2DP(ee.RPrismNearV), '') RPrismNear,
	isnull(dbo.fn_VDB_SignedNumeric2DP(ee.LPrismNearH), '') + ' / ' + isnull(dbo.fn_VDB_SignedNumeric2DP(ee.LPrismNearV), '') LPrismNear,
	bRBaseNearH.Name + ' / ' + bRBaseNearV.Name RBaseNear,
	bLBaseNearH.Name + ' / ' + bLBaseNearV.Name LBaseNear,
	dbo.fn_VDB_Numeric2DP(ee.PDRight) PDRight,
	dbo.fn_VDB_Numeric2DP(ee.PDLeft) PDLeft,
	dbo.fn_VDB_Numeric2DP(ee.RHeight) RHeight,
	dbo.fn_VDB_Numeric2DP(ee.LHeight) LHeight,
	ee.Notes,
	p.PrimaryCareTrustGOS,
	c.SchoolCollegeUniversity,
	replace(c.SchoolCollegeUniversityAddress, char(13) + char(10), ' ') SchoolCollegeUniversityAddress,
	c.SchoolCollegeUniversityPostcode,
	op.GOCNumber,
	p.ContractorName,
	p.ContractorNumber,
	ee.SymptomsAndHistory,
	ee.RFV,
	ee.POH,
	ee.Dipl,
	ee.HA,
	ee.FF,
	ee.GH,
	ee.MEDS,
	ee.Allergies,
	ee.FH

	from EyeExams ee
	inner join Customers c on c.Id = ee.Customer_Id
	inner join AspNetUsers op on op.Id = ee.Optician_Id
	inner join Practices p on p.Id = c.practice_Id
	left join Bases bRBaseDistH on bRBaseDistH.Id = ee.RBaseDistH
	left join Bases bLBaseDistH on bLBaseDistH.Id = ee.LBaseDistH
	left join Bases bRBaseDistV on bRBaseDistV.Id = ee.RBaseDistV
	left join Bases bLBaseDistV on bLBaseDistV.Id = ee.LBaseDistV
	left join Bases bRBaseNearH on bRBaseNearH.Id = ee.RBaseNearH
	left join Bases bLBaseNearH on bLBaseNearH.Id = ee.LBaseNearH
	left join Bases bRBaseNearV on bRBaseNearV.Id = ee.RBaseNearV
	left join Bases bLBaseNearV on bLBaseNearV.Id = ee.LBaseNearV
	where ee.Id = @EyeExamId

	
END



GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_InvoiceDetails]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_InvoiceDetails]
	@invoice_Id uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

    select p.Name, 
	id.UnitPrice, 
	id.Quantity, id.VATRate, 
	id.UnitPrice * id.Quantity LineTotal,
	isnull(id.SpectacleNumber, 99) SpectacleNumber, 
	id.Description, p.Colour,
	pt.NegativeValue, id.Added
	from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id
	inner join ProductTypes pt on pt.Id = p.ProductTypeEnum
	where id.invoice_Id = @invoice_Id and id.Deleted is null and id.DiscountPercentage = 0
	union
	select p.Name + ' (' + cast(id.DiscountPercentage as nvarchar(50)) + '% discount applied)', 
	id.UnitPrice * (1 - (id.DiscountPercentage / 100)) UnitPrice, 
	id.Quantity, id.VATRate, 
	id.UnitPrice * id.Quantity * (1 - (id.DiscountPercentage / 100)) LineTotal,
	isnull(id.SpectacleNumber, 99) SpectacleNumber, 
	id.Description, p.Colour,
	pt.NegativeValue, id.Added
	from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id
	inner join ProductTypes pt on pt.Id = p.ProductTypeEnum
	where id.invoice_Id = @invoice_Id and id.Deleted is null and id.DiscountPercentage > 0
	order by pt.NegativeValue, isnull(id.SpectacleNumber, 99), id.Added
END
GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_InvoiceHeader]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_InvoiceHeader] 
	@invoice_Id uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

    select i.InvoiceDate, c.Number, c.Title, c.Firstnames, c.Surname, c.Address CustomerAddress, c.Postcode CustomerPostcode, 
	p.Name, p.Address PracticeAddress, p.Postcode PracticePostcode, p.Tel, p.Email,
	i.InvoiceNumber, i.DiscountPercentage, i.BalanceIncVAT, i.BalanceExcVAT, i.TotalIncVAT, i.TotalExcVAT
	from Invoices i
	inner join Customers c on c.Id = i.customer_Id
	inner join Practices p on p.Id = c.practice_Id
	where i.Id = @invoice_Id
END

GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_InvoiceHeadersForPractice]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_InvoiceHeadersForPractice]
	@practice_Id uniqueidentifier,
	@start datetime,
	@end datetime
AS
BEGIN
	SET NOCOUNT ON;

    select i.Id, i.InvoiceDate, c.Number, c.Title, c.Firstnames, c.Surname, c.Address CustomerAddress, c.Postcode CustomerPostcode, 
	p.Name, p.Address PracticeAddress, p.Postcode PracticePostcode, p.Tel, p.Email,
	i.InvoiceNumber, i.DiscountPercentage, i.DispenseEyeExam_Id
	from Invoices i
	inner join Customers c on c.Id = i.customer_Id
	inner join Practices p on p.Id = c.practice_Id
	where p.Id = @practice_Id and i.Deleted is null and i.InvoiceDate > @start and i.InvoiceDate < @end
	order by i.InvoiceDate
END

GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_NHS_Vouchers]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_NHS_Vouchers]
	@practiceId uniqueidentifier,
	@Start datetime,
	@End datetime
AS
BEGIN
	SET NOCOUNT ON;

	select 
	i.InvoiceDate, c.Title, c.Firstnames, c.Surname, c.Number, c.Address, c.Postcode, p.Name, ABS(id.UnitPrice) Amount, i.InvoiceNumber, rs.Name ReconciliationStatuses
	from InvoiceDetails id
	inner join Invoices i on i.Id = id.invoice_Id
	inner join Products p on p.Id = id.product_Id
	inner join Customers c on c.Id = i.customer_Id
	inner join ReconciliationStatuses rs on rs.Id = id.ReconciliationStatusEnum
	where i.practice_Id = @practiceId 
	and i.InvoiceDate > @Start and i.InvoiceDate < DATEADD(day, 1, @End)
	and p.ProductTypeEnum = 6
	and id.ReconciliationStatusEnum in (1, 3)
	and i.Deleted is null and id.Deleted is null
END

GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_OutstandingMessageCount]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_OutstandingMessageCount] 
AS
BEGIN
	SET NOCOUNT ON;

	select
	p.Name,
	count(*) MessageCount
	from [Messages] m 
	inner join Practices p on p.Id = m.practice_Id
	where m.Sent is null and m.Cancelled is null
	group by p.Name
END

GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_PatientDataDownload]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_PatientDataDownload]
	@practiceId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	select 
	c.Id PatientId, c.Title, c.Firstnames, c.Surname, c.Number, c.DOB, c.Telephone, c.WorkTelephone, c.SMSNumber, c.Email, c.Address, c.Postcode, 
	c.PreviousEyeExamDate, c.NextDueDateEyeExam, c.PreviousContactLensExamDate, c.NextDueDateContactLensExam, NINo, NHSNo, Comments PatientNotes, Occupation,
	ee.Id EyeExamId, ee.TestDateAndTime, ee.customer_Id, ee.Notes, ee.RSphericalDist, ee.LSphericalDist, ee.RCylDist, ee.LCylDist, ee.RAxisDist, ee.LAxisDist, ee.RPrismDistH, ee.LPrismDistH, ee.RBaseDistH, ee.LBaseDistH, ee.RSphericalNear, ee.LSphericalNear, ee.RCylNear, ee.LCylNear, ee.RAxisNear, ee.LAxisNear, ee.RPrismNearH, ee.LPrismNearH, ee.RBaseNearH, ee.LBaseNearH, ee.RVision, ee.LVision, ee.RDistVA, ee.LDistVA, ee.RNearVA, ee.LNearVA, ee.Title, ee.Start, ee.[End], ee.RFV, ee.GH, ee.MEDS, ee.POH, ee.FH, ee.CoverTestDistance, ee.CoverTestNear, ee.ConvergenceDistance, ee.ConvergenceNear, ee.MotilityDistance, ee.MotilityNear, ee.AccommodationDistance, ee.AccommodationNear, ee.MuscleBalanceFDDistance, ee.MuscleBalanceFDNear, ee.KeratometryDistance, ee.KeratometryNear, ee.RetinoscopyRight, ee.RetinoscopyLeft, ee.BVDRight, ee.BVDLeft, ee.PDRight, ee.PDLeft, ee.VisualFields, ee.Amsler, ee.ColourVision, ee.StereoVision, ee.Other, ee.DVisionRight, ee.DVisionLeft, ee.NVision, ee.SphRight, ee.SphLeft, ee.CylRight, ee.CylLeft, ee.PrismRight, ee.PrismLeft, ee.VARight, ee.VALeft, ee.BinVA, ee.IAddRight, ee.IAddLeft, ee.NAddRight, ee.NAddLeft, ee.Prism2Right, ee.Prism2Left, ee.NVA, ee.AdviceAndRecall, ee.AxisRight, ee.AxisLeft, ee.DispenseSRight, ee.DispenseCRight, ee.DispenseARight, ee.DispensePrismRight, ee.DispenseAddRight, ee.DispenseLensRight, ee.DispenseOCRight, ee.DispenseHeightAboveRight, ee.DispenseSLeft, ee.DispenseCLeft, ee.DispenseALeft, ee.DispensePrismLeft, ee.DispenseAddLeft, ee.DispenseLensLeft, ee.DispenseOCLeft, ee.DispenseHeightAboveLeft, ee.DispenseFrameDetails, ee.DispenseFrame, ee.DispenseFramePrice, ee.DispenseLens, ee.DispenseLensPrice, ee.DispenseEyeExamination, ee.DispenseEyeExaminationPrice, ee.DispenseLessVoucher, ee.DispenseTotal, ee.DispenseDeposit, ee.DispenseDepositDate, ee.DispenseOutstandingBalance, ee.DispenseOutstandingBalanceDate, ee.appointmentCategory, ee.AppointmentRoom, ee.NHSPrivate, ee.NHSReason, ee.NHSVoucher, ee.CLFitExistingWearer, ee.CLFitPreviousWearingDetails, ee.CLFitType, ee.CLFitWearingTime, ee.CLFitSolutionsUsed, ee.CLFitCurrentPreviousProblems, ee.CLFitTrialComments, ee.CLFitTrialOptometrist, ee.CLFitWearingSchedule, ee.CLFitCleaningRegime, ee.CLFitDOHFormCompleted, ee.CLFitCollectionLensesIn, ee.CLFitCollectionWearingTime, ee.CLFitCollectionAdvice, ee.CLFitCollectionNextAppointment, ee.CLFitCollectionOptometrist, ee.AdviceAndRecallType, ee.NextTestDueDate, ee.lidslashesleft, ee.cornealeft, ee.acleft, ee.vitreousleft, ee.lensleft, ee.disccdratioleft, ee.discnrrleft, ee.disccolourleft, ee.marginsleft, ee.vesselsleft, ee.avratioleft, ee.macularleft, ee.peripheryleft, ee.pupilsleft, ee.lidslashesright, ee.cornearight, ee.acright, ee.vitreousright, ee.lensright, ee.disccdratioright, ee.discnrrright, ee.disccolourright, ee.marginsright, ee.vesselsright, ee.avratioright, ee.macularright, ee.peripheryright, ee.pupilsright, ee.CTDist, ee.CTNear, ee.PhoriaD, ee.PhoriaN, ee.NPC, ee.IOPR1, ee.IOPR2, ee.IOPR3, ee.IOPL1, ee.IOPL2, ee.IOPL3, ee.VfieldsRight, ee.VfieldsLeft, ee.PupilRAPD, ee.ConfrontationRight, ee.ConfrontationLeft, ee.Dialated, ee.DrugUsed, ee.TimeDrugUsed, ee.IOPRAvg, ee.IOPLAvg, ee.IOPTime, ee.VisualFieldsR, ee.AmslerR, ee.ColourVisionR, ee.StereoVisionR, ee.OtherR, ee.PupilDiameterL, ee.PupilDiameterR, ee.UpperLidR, ee.UpperLidL, ee.CorneaL, ee.CorneaR, ee.LowerLidR, ee.LowerLidL, ee.MeibomianGlandsL, ee.MeibomianGlandsR, ee.LimbalAppearanceR, ee.LimbalAppearanceL, ee.CounjunctivalAppaeranceL, ee.CounjunctivalAppaeranceR, ee.TearQualityR, ee.TearQualityL, ee.LensTypeL, ee.LensTypeR, ee.SpecificationR, ee.SpecificationL, ee.FittingCentrationL, ee.FittingCentrationR, ee.MovementR, ee.MovementL, ee.CLSphL, ee.CLSphR, ee.CLCyl, ee.CLAxisR, ee.CLAxisL, ee.CLVaR, ee.CLVaL, ee.CLOverReactionL, ee.CLOverReactionR, ee.CLBestVaR, ee.CLBestVaL, ee.LensTypeR2, ee.LensTypeL2, ee.SpecificationR2, ee.SpecificationL2, ee.FittingCentrationL2, ee.FittingCentrationR2, ee.MovementR2, ee.MovementL2, ee.AppointmentComplete, ee.CleaningRegime, ee.LastTestDate, ee.Smoker, ee.HasArrived, ee.ContactLensDueDate, ee.NVisionLeft, ee.NVALeft, ee.BinVALeft, ee.BinNVALeft, ee.BinNVARight, ee.PDLeftNear, ee.PDRightNear, ee.ConjunctivaRight, ee.ConjunctivaLeft, ee.IrisRight, ee.IrisLeft, ee.anisocoria, ee.CLCylLeft, ee.LensTypeLeft, ee.LensTypeRight, ee.ManufacturerLeft, ee.ManufacturerRight, ee.LensNameLeft, ee.LensNameRight, ee.CLBozrRight, ee.CLTdRight, ee.CLBinVARight, ee.CLBozrLeft, ee.CLTdLeft, ee.CLBinVALeft, ee.CLCondofLensLeft, ee.CLCondofLensRight, ee.CLLagRight, ee.CLLagLeft, ee.CLToricRotationLeft, ee.CLToricRotationRight, ee.CLCondofLensLeft2, ee.CLCondofLensRight2, ee.CLLagRight2, ee.CLLagLeft2, ee.CLToricRotationLeft2, ee.CLToricRotationRight2, ee.DrugExpiry, ee.DrugBatch, ee.RetDvisionR, ee.RetDvisionL, ee.RetNvisionR, ee.RetNvisionL, ee.RetSphereR, ee.RetSphereL, ee.RetCylR, ee.RetCylL, ee.RetAxisR, ee.RetAxisL, ee.TbutL, ee.TbutR, ee.SplitACR, ee.SplitACL, ee.CLNotes, ee.InsertionRemoval, ee.ReasonforCLApp, ee.FirstLensType, ee.SecondLensType, ee.FirstOccupational, ee.FirstUV, ee.FirstPhotochromic, ee.FirstMAR, ee.FirstTint, ee.FirstScratch, ee.FirstThin, ee.SecondOccupational, ee.SecondUV, ee.SecondPhotochromic, ee.SecondMAR, ee.SecondTint, ee.SecondScratch, ee.SecondThin, ee.CreatedByUser_Id, ee.CreatedTimestamp, ee.UpdatedTimestamp, ee.Deleted, ee.DeletedByUser_Id, ee.Optician_Id, ee.RBaseDistV, ee.LBaseDistV, ee.RBaseNearV, ee.LBaseNearV, ee.RPrismDistV, ee.LPrismDistV, ee.RPrismNearV, ee.LPrismNearV, ee.SubRVisionD, ee.SubRVisionN, ee.SubLVisionD, ee.SubLVisionN, ee.SubRSph, ee.SubLSph, ee.SubRCyl, ee.SubLCyl, ee.SubRAxis, ee.SubLAxis, ee.SubRPrismD, ee.SubLPrismD, ee.SubRVA, ee.SubLVA, ee.SubRBinVA, ee.SubLBinVA, ee.SubRIAdd, ee.SubLIAdd, ee.SubRNAdd, ee.SubLNAdd, ee.SubRNVA, ee.SubLNVA, ee.SubRBinNVA, ee.SubLBinNVA, ee.SubRPrismN, ee.SubLPrismN, ee.OphthalmoscopyMethodUsed, ee.OphthalmoscopyRDisc, ee.OphthalmoscopyLDisc, ee.OphthalmoscopyRNRR, ee.OphthalmoscopyLNRR, ee.OphthalmoscopyRCDRatio, ee.OphthalmoscopyLCDRatio, ee.OphthalmoscopyRVessels, ee.OphthalmoscopyLVessels, ee.OphthalmoscopyRPeriphery, ee.OphthalmoscopyLPeriphery, ee.OphthalmoscopyRMacula, ee.OphthalmoscopyLMacula, ee.OphthalmoscopyDescription, ee.PatientAdvice, ee.ProductRecommendations, ee.ReferralSent, ee.SymptomsAndHistory, ee.Allergies, ee.IOPR4, ee.IOPR5, ee.IOPL4, ee.IOPL5, ee.ObjectiveMethodEnum, ee.RHeight, ee.LHeight, ee.Dipl, ee.HA, ee.FF, ee.ExternalOpticianName
	from Customers c 
	left join EyeExams ee on ee.customer_Id = c.Id
	where c.practice_Id = @practiceId
	and c.Deleted is null
	order by c.Firstnames, c.Surname, c.Number, ee.TestDateAndTime
END

GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_PatientDetails_For_GOS]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_PatientDetails_For_GOS]
	@CustomerId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

    select
	c.Title,
	c.PreviousSurname,
	c.Surname,
	c.Firstnames,
	c.Title + ' ' + c.Firstnames + ' ' + c.Surname FullName,
	c.DOB,
	replace(c.Address, char(13) + char(10), ' ') Address,
	c.Postcode,
	c.NHSNo,
	c.NINo,
	c.PreviousEyeExamDate, 
	dbo.fn_IsOver60(c.DOB) Over60,
	dbo.fn_IsUnder16(c.DOB) Under16,
	c.Doctor,
	c.GPpracticename,
	replace(c.GPpracticeaddress, char(13) + char(10), ' ') GPpracticeaddress,
	c.GPpracticepostcode,
	p.Name PracticeName,
	replace(p.Address, char(13) + char(10), ' ') PracticeAddress,
	p.Postcode PracticePostcode,
	p.Tel PracticeTel,
	p.Email PracticeEmail,
	p.PrimaryCareTrustGOS,
	c.SchoolCollegeUniversity,
	replace(c.SchoolCollegeUniversityAddress, char(13) + char(10), ' ') SchoolCollegeUniversityAddress,
	c.SchoolCollegeUniversityPostcode,
	op.Title + ' ' + op.Firstnames + ' ' + op.Surname OpticianName,
	op.ListNumber OpticianListNumber,
	op.GOCNumber,
	p.ContractorName,
	p.ContractorNumber
	from Customers c 
	inner join Practices p on p.Id = c.practice_Id
	left join AspNetUsers op on p.DefaultOptician_Id = op.Id
	where c.Id = @CustomerId
END
GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_PatientDetails_For_GOS_For_Date_Range]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE  [dbo].[sp_VDB_PatientDetails_For_GOS_For_Date_Range]
	@practiceId uniqueidentifier, 
	@start datetime,
	@end datetime
AS
BEGIN
	SET NOCOUNT ON;

    select
	c.Title,
	c.PreviousSurname,
	c.Surname,
	c.Firstnames,
	c.Title + ' ' + c.Firstnames + ' ' + c.Surname FullName,
	c.DOB,
	replace(c.Address, char(13) + char(10), ' ') Address,
	c.Postcode,
	c.NHSNo,
	c.NINo,
	c.PreviousEyeExamDate, 
	dbo.fn_IsOver60(c.DOB) Over60,
	dbo.fn_IsUnder16(c.DOB) Under16,
	c.Doctor,
	c.GPpracticename,
	replace(c.GPpracticeaddress, char(13) + char(10), ' ') GPpracticeaddress,
	c.GPpracticepostcode,
	p.Name PracticeName,
	replace(p.Address, char(13) + char(10), ' ') PracticeAddress,
	p.Postcode PracticePostcode,
	p.Tel PracticeTel,
	p.Email PracticeEmail,
	p.PrimaryCareTrustGOS,
	c.SchoolCollegeUniversity,
	replace(c.SchoolCollegeUniversityAddress, char(13) + char(10), ' ') SchoolCollegeUniversityAddress,
	c.SchoolCollegeUniversityPostcode,
	op.Title + ' ' + op.Firstnames + ' ' + op.Surname OpticianName,
	op.ListNumber OpticianListNumber,
	op.GOCNumber,
	p.ContractorName,
	p.ContractorNumber
	from Customers c 
	inner join Practices p on p.Id = c.practice_Id
	left join AspNetUsers op on p.DefaultOptician_Id = op.Id
	where c.practice_Id = @practiceId and c.Deleted is null
	and c.NextDueDateEyeExam > @start and c.NextDueDateEyeExam < @end
END
GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_PaymentSummary]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_PaymentSummary]
	@practiceId uniqueidentifier,
	@start datetime,
	@end datetime
AS
BEGIN
	SET NOCOUNT ON;

	set @start = DATEADD(dd, 0, DATEDIFF(dd, 0, @start))
	set @end = DATEADD(dd, 0, DATEDIFF(dd, 0, @end))

	select p.Name, 
	round(sum(id.UnitPrice * id.Quantity * (1 / (1 + (id.VATRate / 100))) * (1 - (id.DiscountPercentage / 100))), 2) total_exc_vat, 
	sum(round((id.UnitPrice * id.Quantity * (1 - (id.DiscountPercentage / 100))), 2)) total_inc_vat, 
	pt.NegativeValue, pt.Id 
	from InvoiceDetails id
	inner join Invoices i on i.Id = id.invoice_Id
	left join Customers c on c.Id = i.customer_Id
	inner join Products p on p.Id = id.product_Id
	inner join ProductTypes pt on pt.Id = p.ProductTypeEnum
	where i.practice_Id = @practiceId and id.Added > @start and id.Added < DATEADD(day, 1, @end)
	and id.Deleted is null and p.ProductTypeEnum in (5, 6) and i.Deleted is null
	group by pt.Id, p.Name, pt.NegativeValue
	order by pt.Id

END
GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_PracticeDetails]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_PracticeDetails] 
	@practice_Id uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	select p.Name, p.Address, p.Postcode, p.Tel, p.Email from Practices p
	where p.Id = @practice_Id 
END

GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_TransactionExport]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_TransactionExport] 
	@practice_Id uniqueidentifier,
	@start datetime,
	@end datetime
AS
BEGIN
	SET NOCOUNT ON;

    select 
	c.Number, c.Title, isnull(c.Firstnames, 'Counter sale') Firstnames, c.Surname, c.Address, c.Postcode, i.InvoiceDate, p.Name ProductName, pt.ProductName ProductType, 
	abs(id.UnitPrice) as UnitPriceIncVAT, 
	id.Quantity, 
	abs(id.UnitPrice * id.Quantity) as LineTotalIncVAT,
	id.VATRate, 
	cast(abs(round(id.UnitPrice * (1 / (1 + (id.VATRate / 100))), 3)) as real) UnitPriceExcVAT, 
	cast(abs(round(id.UnitPrice * id.Quantity * (1 / (1 + (id.VATRate / 100))), 3)) as real) LineTotalExcVAT,
	p.CostPrice ProductUnitCostPrice,
	p.CostPrice ProductTotalCostPrice,
	i.InvoiceNumber,
	pt.NegativeValue,
	id.Added
	from Invoices i
	left join Customers c on c.Id = i.customer_Id
	inner join InvoiceDetails id on id.invoice_Id = i.Id
	inner join Products p on p.Id = id.product_Id
	inner join ProductTypes pt on pt.Id = p.ProductTypeEnum
	where i.practice_Id = @practice_Id and id.Deleted is null
	and i.InvoiceDate > cast(@start as date) and i.InvoiceDate < cast(dateadd(day, 1, @end) as date)
	and id.DiscountPercentage = 0
	and id.Deleted is null and i.Deleted is null
	union
	select 
	c.Number, c.Title, isnull(c.Firstnames, 'Counter sale') Firstnames, c.Surname, c.Address, c.Postcode, i.InvoiceDate, 
	p.Name  + ' (' + cast(id.DiscountPercentage as nvarchar(50)) + '% discount applied)' ProductName, 
	pt.ProductName ProductType, 
	abs(id.UnitPrice * (1 - (id.DiscountPercentage / 100))) as UnitPriceIncVAT, 
	id.Quantity, 
	abs(id.UnitPrice * id.Quantity * (1 - (id.DiscountPercentage / 100))) as LineTotalIncVAT,
	id.VATRate, 
	cast(abs(round(id.UnitPrice * (1 / (1 + (id.VATRate / 100))) * (1 - (id.DiscountPercentage / 100)), 3)) as real) UnitPriceExcVAT, 
	cast(abs(round(id.UnitPrice * id.Quantity * (1 / (1 + (id.VATRate / 100))) * (1 - (id.DiscountPercentage / 100)), 3)) as real) LineTotalExcVAT,
	p.CostPrice ProductUnitCostPrice,
	p.CostPrice ProductTotalCostPrice,
	i.InvoiceNumber,
	pt.NegativeValue,
	id.Added
	from Invoices i
	left join Customers c on c.Id = i.customer_Id
	inner join InvoiceDetails id on id.invoice_Id = i.Id
	inner join Products p on p.Id = id.product_Id
	inner join ProductTypes pt on pt.Id = p.ProductTypeEnum
	where i.practice_Id = @practice_Id and id.Deleted is null
	and i.InvoiceDate > cast(@start as date) and i.InvoiceDate < cast(dateadd(day, 1, @end) as date)
	and id.DiscountPercentage > 0
	and id.Deleted is null and i.Deleted is null
	order by i.InvoiceDate, Firstnames, c.Surname, pt.NegativeValue, id.Added
END

GO
/****** Object:  StoredProcedure [dbo].[sp_VDB_VATReport]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_VDB_VATReport] 
	@practice_Id uniqueidentifier,
	@start datetime,
	@end datetime
AS
BEGIN
	SET NOCOUNT ON;

declare @InvoiceId uniqueidentifier

DECLARE @report TABLE(
Id uniqueidentifier,
invoice_date datetime,
invoice_number nvarchar(50),
invoice_type nvarchar(50),
patient_name nvarchar(100),
patient_number nvarchar(50),
st_value float,	
frame nvarchar(500),
lenses nvarchar(500),
voucher nvarchar(500),
voucher_value float,
frame_px float,
lenses_px float,
frame_we_paid float,
lenses_we_paid float,
total_dispense float,
total_dispense_exc_voucher float,
professional_fee float,
tint float,
[service] float,
accessory float,
sunglasses float,
discount float,
contact_lens float,
payments float,
repairs_replacements float,
balance float
)

DECLARE @ReportData CURSOR
SET @ReportData = CURSOR FOR
select 
i.Id from Invoices i 
where i.practice_Id = @practice_Id and i.InvoiceDate > @start and i.InvoiceDate < @end
and i.Deleted is null
order by i.InvoiceDate
OPEN @ReportData
FETCH NEXT
FROM @ReportData INTO @InvoiceId
WHILE @@FETCH_STATUS = 0
BEGIN

	declare @InvoiceDate datetime 
	declare @InvoiceNumber nvarchar(50)
	declare @CustomerId uniqueidentifier 
	select @InvoiceDate = InvoiceDate, @InvoiceNumber = InvoiceNumber, @CustomerId = customer_Id from Invoices where Id = @InvoiceId

	declare @ProfessionalFee float
	select @ProfessionalFee = isnull(sum(id.UnitPrice), 0) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 10 and id.Deleted is null

	declare @nhsVoucherValue float
	select @nhsVoucherValue = abs(isnull(sum(id.UnitPrice), 0)) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 6 and id.Deleted is null 
	
	declare @StValue float
	select @StValue = abs(isnull(sum(id.UnitPrice), 0)) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 12 and id.Deleted is null 

	declare @PatientName nvarchar(100)
	declare @PatientNumber nvarchar(50)
	IF @CustomerId is not null
	BEGIN
		select @PatientName = isnull(Title + ' ', '') + Firstnames + ' ' + Surname, @PatientNumber = Number from Customers where Id = @CustomerId 
	END
	ELSE
	BEGIN
		set @PatientName = 'Counter sale'
	END

	declare @Frame nvarchar(500) = null
	SELECT @Frame = COALESCE(@Frame+', ' ,'') + p.Name
	FROM InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 1
	and id.Deleted is null 
	IF LEN(@Frame) = 0
	BEGIN
		select @Frame = 'Patient''s own frame'
	END

	declare @Lenses nvarchar(500) = null
	SELECT @Lenses = COALESCE(@Lenses+', ' ,'') + p.Name
	FROM InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 2
	and id.Deleted is null 

	declare @Voucher nvarchar(500) = null
	SELECT @Voucher = COALESCE(@Voucher+', ' ,'') + p.Code
	FROM InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 6
	and id.Deleted is null

	declare @InvoiceType nvarchar(50)
	IF @Voucher is not null
	BEGIN
		set @InvoiceType = 'NHS'
	END
	ELSE
	BEGIN
		set @InvoiceType = 'PVT'
	END

	declare @FrameValue float = null
	select @FrameValue = isnull(sum(id.UnitPrice * id.Quantity), 0) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 1
	and id.Deleted is null 

	declare @LensesValue float = null
	select @LensesValue = isnull(sum(id.UnitPrice * id.Quantity), 0) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 2
	and id.Deleted is null 

	declare @FrameCost float = null
	select @FrameCost = isnull(sum(p.CostPrice * id.Quantity), 0) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 1
	and id.Deleted is null 

	declare @LensesCost float = null
	select @LensesCost = isnull(sum(p.CostPrice * id.Quantity), 0) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 2
	and id.Deleted is null 

	declare @Tint float = null
	select @Tint = isnull(sum(id.UnitPrice * id.Quantity), 0) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 9
	and id.Deleted is null 

	declare @Service float = null
	select @Service = isnull(sum(id.UnitPrice * id.Quantity), 0) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 3
	and id.Deleted is null 

	declare @Accessory float = null
	select @Accessory = isnull(sum(id.UnitPrice * id.Quantity), 0) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 0
	and id.Deleted is null 

	declare @Sunglasses float = null
	select @Sunglasses = isnull(sum(id.UnitPrice * id.Quantity), 0) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 7
	and id.Deleted is null 

	declare @Discount float = null
	select @Discount = isnull(sum(id.UnitPrice * id.Quantity), 0) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 4
	and id.Deleted is null 

	declare @ContactLenses float = null
	select @ContactLenses = isnull(sum(id.UnitPrice * id.Quantity), 0) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 8
	and id.Deleted is null 

	declare @Payments float = null
	select @Payments = abs(isnull(sum(id.UnitPrice * id.Quantity), 0)) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 5
	and id.Deleted is null 

	declare @RepairReplacements float = null
	select @RepairReplacements = abs(isnull(sum(id.UnitPrice * id.Quantity), 0)) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId and p.ProductTypeEnum = 11
	and id.Deleted is null 

	declare @Balance float = null
	select @Balance = isnull(sum(id.UnitPrice * id.Quantity), 0) from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id 
	where id.invoice_Id = @InvoiceId 
	and id.Deleted is null 

	insert into @report (Id, invoice_date, invoice_number, invoice_type, patient_name, patient_number, st_value, frame, lenses, voucher, voucher_value, frame_px, lenses_px, frame_we_paid, lenses_we_paid, total_dispense, professional_fee, tint, [service], accessory, sunglasses, discount, contact_lens, payments, total_dispense_exc_voucher, balance, repairs_replacements) 
	values (@InvoiceId, @InvoiceDate, @InvoiceNumber, @InvoiceType, @PatientName, @PatientNumber, @StValue, @Frame, @Lenses, @Voucher, @nhsVoucherValue, @FrameValue, @LensesValue, @FrameCost, @LensesCost, @FrameValue + @LensesValue + @nhsVoucherValue + @ProfessionalFee + @Tint + @Service + @Accessory + @Sunglasses + @Discount + @ContactLenses, @ProfessionalFee, @Tint, @Service, @Accessory, @Sunglasses, @Discount, @ContactLenses, @Payments, @FrameValue + @LensesValue + @ProfessionalFee + @Tint + @Service + @Accessory + @Sunglasses + @Discount + @ContactLenses, @Balance, @RepairReplacements)

	FETCH NEXT
	FROM @ReportData INTO @InvoiceId
	END
	CLOSE @ReportData
	DEALLOCATE @ReportData

	select * from @report 
	order by invoice_date 
END
GO
/****** Object:  UserDefinedFunction [dbo].[fn_IsOver60]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_IsOver60]
(
	@dob datetime
)
RETURNS bit
AS
BEGIN
	declare @Over60 bit

	if (DATEADD(year, 60, @dob) > convert(date, GETDATE()))
	begin
		set @Over60 = 0;
	end
	else
	begin
		set @Over60 = 1;
	end

	return @Over60
END

GO
/****** Object:  UserDefinedFunction [dbo].[fn_IsUnder16]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_IsUnder16]
(
	@dob datetime
)
RETURNS bit
AS
BEGIN
	declare @Under16 bit

	if (DATEADD(year, 16, @dob) < convert(date, GETDATE()))
	begin
		set @Under16 = 0;
	end
	else
	begin
		set @Under16 = 1;
	end

	return @Under16
END

GO
/****** Object:  UserDefinedFunction [dbo].[fn_VDB_DiscountTotal]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_VDB_DiscountTotal] 
(
	@invoice_Id uniqueidentifier
)
RETURNS decimal(18, 2)
AS
BEGIN
	DECLARE @result decimal(18, 2)

	SELECT @result = abs(isnull(sum(Quantity * id.UnitPrice), 0.00)) from InvoiceDetails id 
	inner join Products p on p.Id = id.product_Id
	where id.invoice_Id = @invoice_Id and id.Deleted is null
	and p.ProductTypeEnum in (4)

	RETURN @result
END
GO
/****** Object:  UserDefinedFunction [dbo].[fn_VDB_GoodsCostTotal]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_VDB_GoodsCostTotal] 
(
	@invoice_Id uniqueidentifier
)
RETURNS decimal(18, 2)
AS
BEGIN
	DECLARE @result decimal(18, 2)

	SELECT @result = sum(Quantity * id.CostPrice) from InvoiceDetails id 
	inner join Products p on p.Id = id.product_Id
	where id.invoice_Id = @invoice_Id and id.Deleted is null
	and p.ProductTypeEnum in (0, 1, 2, 7, 8, 9)

	RETURN @result
END
GO
/****** Object:  UserDefinedFunction [dbo].[fn_VDB_GoodsTotalIncVAT]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_VDB_GoodsTotalIncVAT] 
(
	@invoice_Id uniqueidentifier
)
RETURNS decimal(18, 2)
AS
BEGIN
	DECLARE @result decimal(18, 2)

	SELECT @result = sum(Quantity * UnitPrice * (1 - (DiscountPercentage / 100))) from InvoiceDetails id 
	inner join Products p on p.Id = id.product_Id
	where id.invoice_Id = @invoice_Id and id.Deleted is null
	and p.ProductTypeEnum in (0, 1, 2, 7, 8, 9)

	RETURN @result
END
GO
/****** Object:  UserDefinedFunction [dbo].[fn_VDB_InvoiceBalanceIncVAT]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_VDB_InvoiceBalanceIncVAT] 
(
	@invoice_Id uniqueidentifier
)
RETURNS decimal(18, 2)
AS
BEGIN
	DECLARE @result decimal(18, 2)

	SELECT @result = sum(Quantity * UnitPrice * (1 - (DiscountPercentage / 100))) from InvoiceDetails id 
	where id.invoice_Id = @invoice_Id and id.Deleted is null

	RETURN @result
END

GO
/****** Object:  UserDefinedFunction [dbo].[fn_VDB_InvoiceTotalIncVAT]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_VDB_InvoiceTotalIncVAT] 
(
	@invoice_Id uniqueidentifier
)
RETURNS decimal(18, 2)
AS
BEGIN
	DECLARE @result decimal(18, 2)

	SELECT @result = sum(Quantity * UnitPrice * (1 - (DiscountPercentage / 100))) from InvoiceDetails id 
	inner join Products p on p.Id = id.product_Id
	inner join ProductTypes pt on pt.Id = p.ProductTypeEnum
	where id.invoice_Id = @invoice_Id and id.Deleted is null
	and pt.NegativeValue = 0

	RETURN @result
END

GO
/****** Object:  UserDefinedFunction [dbo].[fn_VDB_Numeric0DP]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_VDB_Numeric0DP]
(
	@input real
)
RETURNS nvarchar(10)
AS
BEGIN
	DECLARE @output nvarchar(10)

	SELECT @output = convert(decimal(5,0),@input)

	RETURN @output
END

GO
/****** Object:  UserDefinedFunction [dbo].[fn_VDB_Numeric2DP]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_VDB_Numeric2DP]
(
	@input real
)
RETURNS nvarchar(10)
AS
BEGIN
	DECLARE @output nvarchar(10)

	SELECT @output = convert(decimal(5,2),@input)

	RETURN @output
END

GO
/****** Object:  UserDefinedFunction [dbo].[fn_VDB_PaymentTotal]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_VDB_PaymentTotal] 
(
	@invoice_Id uniqueidentifier
)
RETURNS decimal(18, 2)
AS
BEGIN
	DECLARE @result decimal(18, 2)

	SELECT @result = abs(isnull(sum(Quantity * id.UnitPrice), 0.00)) from InvoiceDetails id 
	inner join Products p on p.Id = id.product_Id
	where id.invoice_Id = @invoice_Id and id.Deleted is null
	and p.ProductTypeEnum in (5)

	RETURN @result
END
GO
/****** Object:  UserDefinedFunction [dbo].[fn_VDB_ProfessionalFeeTotal]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_VDB_ProfessionalFeeTotal] 
(
	@invoice_Id uniqueidentifier
)
RETURNS decimal(18, 2)
AS
BEGIN
	DECLARE @result decimal(18, 2)

	SELECT @result = sum(Quantity * UnitPrice * (1 - (DiscountPercentage / 100))) from InvoiceDetails id 
	inner join Products p on p.Id = id.product_Id
	where id.invoice_Id = @invoice_Id and id.Deleted is null
	and p.ProductTypeEnum in (10)

	RETURN @result
END
GO
/****** Object:  UserDefinedFunction [dbo].[fn_VDB_ServiceTotalIncVAT]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_VDB_ServiceTotalIncVAT] 
(
	@invoice_Id uniqueidentifier
)
RETURNS decimal(18, 2)
AS
BEGIN
	DECLARE @result decimal(18, 2)

	SELECT @result = sum(Quantity * UnitPrice * (1 - (DiscountPercentage / 100))) from InvoiceDetails id 
	inner join Products p on p.Id = id.product_Id
	where id.invoice_Id = @invoice_Id and id.Deleted is null
	and p.ProductTypeEnum in (3, 10)

	RETURN @result
END
GO
/****** Object:  UserDefinedFunction [dbo].[fn_VDB_SignedNumeric2DP]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fn_VDB_SignedNumeric2DP]
(
	@input real
)
RETURNS nvarchar(10)
AS
BEGIN
	DECLARE @output nvarchar(10)

	SELECT @output = case 
     when @input > 0 
     then concat('+', convert(decimal(5,2),@input)) 
     else concat('', convert(decimal(5,2),@input)) 
    end

	RETURN @output
END

GO
/****** Object:  Table [dbo].[Appointments]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Appointments](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Appointments_Id]  DEFAULT (newid()),
	[CreatedTimestamp] [datetime] NOT NULL CONSTRAINT [DF_Appointment_CreatedTimestamp]  DEFAULT (getdate()),
	[practice_Id] [uniqueidentifier] NULL,
	[CreatedByUser_Id] [uniqueidentifier] NOT NULL,
	[Title] [nvarchar](50) NOT NULL,
	[Notes] [nvarchar](2000) NULL,
	[customer_Id] [uniqueidentifier] NULL,
	[Start] [datetime] NOT NULL,
	[End] [datetime] NOT NULL,
	[Deleted] [datetime] NULL,
	[ReminderSent] [datetime] NULL,
	[ReminderSentByUserName] [nvarchar](50) NULL,
	[Arrived] [datetime] NULL,
	[ArrivedUserSet_Id] [nvarchar](128) NULL,
	[StatusEnum] [int] NOT NULL CONSTRAINT [DF_Appointments_StatusEnum]  DEFAULT ((0)),
	[appointmentType_Id] [uniqueidentifier] NULL,
	[StaffMember_Id] [nvarchar](128) NULL,
 CONSTRAINT [PK_Appointment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AppointmentTypes]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppointmentTypes](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_AppointmentTypes_Id]  DEFAULT (newid()),
	[Name] [nvarchar](50) NOT NULL,
	[AppointmentCategoryEnum] [int] NOT NULL,
	[practice_Id] [uniqueidentifier] NOT NULL,
	[DefaultAppointmentLength] [int] NOT NULL CONSTRAINT [DF_AppointmentTypes_DefaultAppointmentLength]  DEFAULT ((30)),
	[Added] [datetime] NOT NULL CONSTRAINT [DF_AppointmentTypes_Added]  DEFAULT (getdate()),
	[Deleted] [datetime] NULL,
 CONSTRAINT [PK_AppointmentTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
	[practiceId] [uniqueidentifier] NOT NULL,
	[IsOptician] [bit] NOT NULL CONSTRAINT [DF_AspNetUsers_IsOptician]  DEFAULT ((0)),
	[IsAdmin] [bit] NOT NULL CONSTRAINT [DF_AspNetUsers_IsAdmin]  DEFAULT ((0)),
	[Title] [nvarchar](50) NOT NULL,
	[Firstnames] [nvarchar](50) NOT NULL,
	[Surname] [nvarchar](50) NOT NULL,
	[ListNumber] [nvarchar](50) NULL,
	[SupportCode] [nvarchar](20) NOT NULL,
	[DefaultHomePageEnum] [int] NOT NULL CONSTRAINT [DF_AspNetUsers_DefaultHomePageEnum]  DEFAULT ((0)),
	[GOCNumber] [nvarchar](50) NULL,
	[Hidden] [bit] NOT NULL CONSTRAINT [DF_AspNetUsers_Hidden]  DEFAULT ((0)),
	[Added] [datetime] NOT NULL CONSTRAINT [DF_AspNetUsers_Added]  DEFAULT (getdate()),
	[PreventDraggingAppointments] [bit] NOT NULL CONSTRAINT [DF_AspNetUsers_PreventDraggingAppointments]  DEFAULT ((0)),
	[AutomaticallyResizeCalendar] [bit] NOT NULL CONSTRAINT [DF_AspNetUsers_AutomaticallyResizeCalendar]  DEFAULT ((0)),
 CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Attachments]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Attachments](
	[Id] [uniqueidentifier] NOT NULL,
	[Filename] [nvarchar](max) NOT NULL,
	[FileComments] [nvarchar](max) NULL,
	[customer_Id] [uniqueidentifier] NULL,
	[CreatedTimestamp] [datetime] NOT NULL CONSTRAINT [DF_Attachments_CreatedTimestamp]  DEFAULT (getdate()),
	[CreatedByUser_Id] [uniqueidentifier] NOT NULL,
	[Deleted] [datetime] NULL,
	[DeletedByUser_Id] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Attachments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[AuditLog]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditLog](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_AuditLog_Id]  DEFAULT (newid()),
	[Added] [datetime] NOT NULL CONSTRAINT [DF_AuditLog_Added]  DEFAULT (getdate()),
	[UserName] [nvarchar](50) NOT NULL,
	[EntryType] [nvarchar](50) NOT NULL,
	[EntryText] [nvarchar](max) NULL,
	[EntryId] [uniqueidentifier] NULL,
	[Visible] [bit] NOT NULL CONSTRAINT [DF_AuditLog_Visible]  DEFAULT ((0)),
	[practice_Id] [uniqueidentifier] NULL,
 CONSTRAINT [PK_AuditLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Bases]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bases](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Bases] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Companies]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Companies](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Companies_Id]  DEFAULT (newid()),
	[Name] [nvarchar](max) NULL,
	[Address] [nvarchar](max) NULL,
	[Postcode] [nvarchar](max) NULL,
	[Tel] [nvarchar](max) NULL,
	[Fax] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[ExternalId] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Companies_ExternalId]  DEFAULT (newid()),
	[CreatedByUser_Id] [uniqueidentifier] NOT NULL,
	[CompanyRef] [nvarchar](4) NULL,
	[Added] [datetime] NOT NULL CONSTRAINT [DF_Companies_Added]  DEFAULT (getdate()),
	[LastProductNumber] [int] NOT NULL CONSTRAINT [DF_Companies_LastProductCode]  DEFAULT ((0)),
 CONSTRAINT [PK__Companie__3214EC077F60ED59] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ__Companie__5D7C80D3B7EF839D] UNIQUE NONCLUSTERED 
(
	[CompanyRef] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Customers]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Customers](
	[Id] [uniqueidentifier] NOT NULL,
	[Number] [nvarchar](20) NULL,
	[Title] [nvarchar](max) NULL,
	[Firstnames] [nvarchar](max) NOT NULL,
	[Surname] [nvarchar](max) NOT NULL,
	[PreviousSurname] [nvarchar](max) NULL,
	[DOB] [datetime] NULL,
	[Address] [nvarchar](1000) NOT NULL,
	[Postcode] [nvarchar](20) NOT NULL,
	[Telephone] [nvarchar](max) NULL,
	[SMSNumber] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[TestFrequencyId] [int] NOT NULL,
	[LastTestDate] [datetime] NULL,
	[Comments] [nvarchar](max) NULL,
	[LastOptician] [nvarchar](max) NULL,
	[LastOpticianContact] [nvarchar](max) NULL,
	[Occupation] [nvarchar](max) NULL,
	[CLFitDrName] [nvarchar](max) NULL,
	[CLFitDrAddress] [nvarchar](max) NULL,
	[CLFitDriver] [int] NULL,
	[CLFitVDU] [int] NULL,
	[CLFitOccupation] [nvarchar](max) NULL,
	[CLFitHobbiesSports] [nvarchar](max) NULL,
	[CLFitExistingWearer] [nvarchar](max) NULL,
	[CLFitPreviousWearingDetails] [nvarchar](max) NULL,
	[CLFitType] [nvarchar](max) NULL,
	[CLFitWearingTime] [nvarchar](max) NULL,
	[CLFitSolutionsUsed] [nvarchar](max) NULL,
	[CLFitCurrentPreviousProblems] [nvarchar](max) NULL,
	[CLFitTrialComments] [nvarchar](max) NULL,
	[CLFitTrialOptometrist] [nvarchar](max) NULL,
	[CLFitWearingSchedule] [nvarchar](max) NULL,
	[CLFitCleaningSchedule] [nvarchar](max) NULL,
	[CLFitDOHFormCompleted] [nvarchar](max) NULL,
	[CLFitCollectionLensesIn] [nvarchar](max) NULL,
	[CLFitCollectionWearingTime] [nvarchar](max) NULL,
	[CLFitCollectionAdvice] [nvarchar](max) NULL,
	[CLFitCollectionNextAppointment] [nvarchar](max) NULL,
	[CLFitCollectionOptometrist] [nvarchar](max) NULL,
	[CLFitCleaningRegime] [nvarchar](max) NULL,
	[NINo] [nvarchar](max) NULL,
	[NHSNo] [nvarchar](max) NULL,
	[ParentTitle] [nvarchar](max) NULL,
	[ParentName] [nvarchar](max) NULL,
	[ParentAddress] [nvarchar](max) NULL,
	[ParentPostCode] [nvarchar](max) NULL,
	[Parentsurname] [nvarchar](max) NULL,
	[RFV] [nvarchar](max) NULL,
	[GH] [nvarchar](max) NULL,
	[MEDS] [nvarchar](max) NULL,
	[POH] [nvarchar](max) NULL,
	[FH] [nvarchar](max) NULL,
	[NHSPrivate] [int] NULL,
	[NHSReason] [nvarchar](max) NULL,
	[NHSVoucher] [nvarchar](max) NULL,
	[GPpracticename] [nvarchar](max) NULL,
	[GPpracticepostcode] [nvarchar](max) NULL,
	[GPpracticephone] [nvarchar](max) NULL,
	[GPpracticefax] [nvarchar](max) NULL,
	[GPpracticeemail] [nvarchar](max) NULL,
	[glaucoma] [int] NULL,
	[contactlenswearer] [int] NULL,
	[lidslashesleft] [varchar](max) NULL,
	[cornealeft] [varchar](max) NULL,
	[acleft] [varchar](max) NULL,
	[vitreousleft] [varchar](max) NULL,
	[lensleft] [varchar](max) NULL,
	[disccdratioleft] [varchar](max) NULL,
	[discnrrleft] [varchar](max) NULL,
	[disccolourleft] [varchar](max) NULL,
	[marginsleft] [varchar](max) NULL,
	[vesselsleft] [varchar](max) NULL,
	[avratioleft] [varchar](max) NULL,
	[macularleft] [varchar](max) NULL,
	[peripheryleft] [varchar](max) NULL,
	[pupilsleft] [varchar](max) NULL,
	[lidslashesright] [varchar](max) NULL,
	[cornearight] [varchar](max) NULL,
	[acright] [varchar](max) NULL,
	[vitreousright] [varchar](max) NULL,
	[lensright] [varchar](max) NULL,
	[disccdratioright] [varchar](max) NULL,
	[discnrrright] [varchar](max) NULL,
	[disccolourright] [varchar](max) NULL,
	[marginsright] [varchar](max) NULL,
	[vesselsright] [varchar](max) NULL,
	[avratioright] [varchar](max) NULL,
	[macularright] [varchar](max) NULL,
	[peripheryright] [varchar](max) NULL,
	[pupilsright] [varchar](max) NULL,
	[Smoker] [int] NULL,
	[diabetic] [int] NULL,
	[practice_Id] [uniqueidentifier] NOT NULL,
	[ExternalId] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Customers_ExternalId]  DEFAULT (newid()),
	[LastUpdated] [datetime] NOT NULL CONSTRAINT [DF_Customers_LastUpdated]  DEFAULT (getdate()),
	[CreatedByUser_Id] [uniqueidentifier] NOT NULL,
	[EyeExamFrequencyValue] [int] NULL,
	[EyeExamFrequencyUnit] [int] NOT NULL,
	[ContactLensExamFrequencyValue] [int] NULL,
	[ContactLensExamFrequencyUnit] [int] NOT NULL,
	[NextDueDateEyeExam] [datetime] NULL,
	[NextDueDateContactLensExam] [datetime] NULL,
	[EyeExamRecallSent] [datetime] NULL,
	[ContactLensExamRecallSent] [datetime] NULL,
	[PreviousEyeExamDate] [datetime] NULL,
	[GPpracticeaddress] [nvarchar](200) NULL,
	[Doctor] [nvarchar](50) NULL,
	[DoctorContact] [nvarchar](50) NULL,
	[DefaultMessageMethod] [int] NOT NULL CONSTRAINT [DF_Customers_DefaultMessageMethod]  DEFAULT ((0)),
	[Deleted] [datetime] NULL,
	[DeletedByUser_Id] [uniqueidentifier] NULL,
	[SchoolCollegeUniversity] [nvarchar](100) NULL,
	[SchoolCollegeUniversityAddress] [nvarchar](500) NULL,
	[SchoolCollegeUniversityPostcode] [nvarchar](50) NULL,
	[RecallCount] [int] NOT NULL,
	[SymptomsAndHistory] [nvarchar](2000) NULL,
	[Allergies] [nvarchar](2000) NULL,
	[PreviousContactLensExamDate] [datetime] NULL,
	[Benefit] [nvarchar](50) NULL,
	[Created] [datetime] NOT NULL CONSTRAINT [DF_Customers_Created]  DEFAULT (getdate()),
	[CareHome] [nvarchar](50) NULL,
	[ShelteredAccommodation] [nvarchar](50) NULL,
	[EmailReminders] [bit] NOT NULL CONSTRAINT [DF_Customers_EmailReminders]  DEFAULT ((0)),
	[SMSReminders] [bit] NOT NULL CONSTRAINT [DF_Customers_AcceptSMS]  DEFAULT ((0)),
	[LetterReminders] [bit] NOT NULL CONSTRAINT [DF_Customers_LetterReminders]  DEFAULT ((0)),
	[eyeExamRecallTemplate_Id] [uniqueidentifier] NULL,
	[contactLensExamRecallTemplate_Id] [uniqueidentifier] NULL,
	[TelephoneReminders] [bit] NOT NULL CONSTRAINT [DF_Customers_CallReminders]  DEFAULT ((0)),
	[ExcludeFromRecalls] [bit] NOT NULL CONSTRAINT [DF_Customers_ExcludeFromRecalls]  DEFAULT ((0)),
	[WorkTelephone] [nvarchar](50) NULL,
	[LastRecallMessage_Id] [uniqueidentifier] NULL,
 CONSTRAINT [PK__Customer__3214EC0703317E3D] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CustomerTags]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerTags](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_CustomerTags_Id]  DEFAULT (newid()),
	[customer_Id] [uniqueidentifier] NOT NULL,
	[tag_Id] [uniqueidentifier] NOT NULL,
	[Added] [datetime] NOT NULL CONSTRAINT [DF_CustomerTags_Added]  DEFAULT (getdate()),
	[Deleted] [datetime] NULL,
 CONSTRAINT [PK_CustomerTags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DocumentTemplates]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocumentTemplates](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[company_Id] [uniqueidentifier] NOT NULL,
	[TemplateTypeEnum] [int] NOT NULL,
	[TemplateHtml] [nvarchar](max) NOT NULL,
	[Deleted] [datetime] NULL,
	[TemplateMethodEnum] [int] NOT NULL CONSTRAINT [DF_DocumentTemplates_TemplateMethodEnum]  DEFAULT ((0)),
 CONSTRAINT [PK_Templates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ErrorCodes]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ErrorCodes](
	[Code] [int] IDENTITY(107,1) NOT NULL,
	[ErrorMessage] [nvarchar](2000) NULL,
	[Description] [nvarchar](2000) NULL,
	[Recommendation] [nvarchar](2000) NULL,
 CONSTRAINT [PK_ErrorCodes] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Expenses]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Expenses](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Expenses_Id]  DEFAULT (newid()),
	[practice_Id] [uniqueidentifier] NOT NULL,
	[Added] [datetime] NOT NULL CONSTRAINT [DF_Expenses_Added]  DEFAULT (getdate()),
	[CreatedByUser_Id] [nvarchar](128) NOT NULL,
	[Cost] [money] NOT NULL,
	[Payee] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](2000) NULL,
	[ExpenseDate] [datetime] NOT NULL CONSTRAINT [DF_Expenses_ExpenseDate]  DEFAULT (getdate()),
	[Deleted] [datetime] NULL,
	[Category] [nvarchar](50) NULL,
	[StatusEnum] [int] NOT NULL CONSTRAINT [DF_Expenses_StatusEnum]  DEFAULT ((0)),
	[VATRate] [decimal](18, 2) NOT NULL CONSTRAINT [DF_Expenses_VATRate]  DEFAULT ((0)),
	[ReferenceNumber] [nvarchar](50) NULL,
 CONSTRAINT [PK_Expenses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[EyeExams]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[EyeExams](
	[Id] [uniqueidentifier] NOT NULL,
	[TestDateAndTime] [datetime] NOT NULL,
	[customer_Id] [uniqueidentifier] NOT NULL,
	[Notes] [nvarchar](max) NULL,
	[RSphericalDist] [real] NULL,
	[LSphericalDist] [real] NULL,
	[RCylDist] [real] NULL,
	[LCylDist] [real] NULL,
	[RAxisDist] [real] NULL,
	[LAxisDist] [real] NULL,
	[RPrismDistH] [real] NULL,
	[LPrismDistH] [real] NULL,
	[RBaseDistH] [int] NULL,
	[LBaseDistH] [int] NULL,
	[RSphericalNear] [real] NULL,
	[LSphericalNear] [real] NULL,
	[RCylNear] [real] NULL,
	[LCylNear] [real] NULL,
	[RAxisNear] [real] NULL,
	[LAxisNear] [real] NULL,
	[RPrismNearH] [real] NULL,
	[LPrismNearH] [real] NULL,
	[RBaseNearH] [int] NULL,
	[LBaseNearH] [int] NULL,
	[RVision] [real] NULL,
	[LVision] [real] NULL,
	[RDistVA] [int] NULL,
	[LDistVA] [int] NULL,
	[RNearVA] [int] NULL,
	[LNearVA] [int] NULL,
	[Title] [nvarchar](50) NULL,
	[Start] [datetime] NULL,
	[End] [datetime] NULL,
	[RFV] [nvarchar](max) NULL,
	[GH] [nvarchar](max) NULL,
	[MEDS] [nvarchar](max) NULL,
	[POH] [nvarchar](max) NULL,
	[FH] [nvarchar](max) NULL,
	[CoverTestDistance] [nvarchar](max) NULL,
	[CoverTestNear] [nvarchar](max) NULL,
	[ConvergenceDistance] [nvarchar](max) NULL,
	[ConvergenceNear] [nvarchar](max) NULL,
	[MotilityDistance] [nvarchar](max) NULL,
	[MotilityNear] [nvarchar](max) NULL,
	[AccommodationDistance] [nvarchar](max) NULL,
	[AccommodationNear] [nvarchar](max) NULL,
	[MuscleBalanceFDDistance] [nvarchar](max) NULL,
	[MuscleBalanceFDNear] [nvarchar](max) NULL,
	[KeratometryDistance] [nvarchar](max) NULL,
	[KeratometryNear] [nvarchar](max) NULL,
	[RetinoscopyRight] [nvarchar](max) NULL,
	[RetinoscopyLeft] [nvarchar](max) NULL,
	[BVDRight] [real] NULL,
	[BVDLeft] [real] NULL,
	[PDRight] [real] NULL,
	[PDLeft] [real] NULL,
	[VisualFields] [nvarchar](max) NULL,
	[Amsler] [nvarchar](max) NULL,
	[ColourVision] [nvarchar](max) NULL,
	[StereoVision] [nvarchar](max) NULL,
	[Other] [nvarchar](max) NULL,
	[DVisionRight] [nvarchar](max) NULL,
	[DVisionLeft] [nvarchar](max) NULL,
	[NVision] [nvarchar](max) NULL,
	[SphRight] [nvarchar](max) NULL,
	[SphLeft] [nvarchar](max) NULL,
	[CylRight] [nvarchar](max) NULL,
	[CylLeft] [nvarchar](max) NULL,
	[PrismRight] [nvarchar](max) NULL,
	[PrismLeft] [nvarchar](max) NULL,
	[VARight] [nvarchar](max) NULL,
	[VALeft] [nvarchar](max) NULL,
	[BinVA] [nvarchar](max) NULL,
	[IAddRight] [real] NULL,
	[IAddLeft] [real] NULL,
	[NAddRight] [real] NULL,
	[NAddLeft] [real] NULL,
	[Prism2Right] [nvarchar](max) NULL,
	[Prism2Left] [nvarchar](max) NULL,
	[NVA] [nvarchar](max) NULL,
	[AdviceAndRecall] [nvarchar](max) NULL,
	[AxisRight] [nvarchar](max) NULL,
	[AxisLeft] [nvarchar](max) NULL,
	[DispenseSRight] [nvarchar](max) NULL,
	[DispenseCRight] [nvarchar](max) NULL,
	[DispenseARight] [nvarchar](max) NULL,
	[DispensePrismRight] [nvarchar](max) NULL,
	[DispenseAddRight] [nvarchar](max) NULL,
	[DispenseLensRight] [nvarchar](max) NULL,
	[DispenseOCRight] [nvarchar](max) NULL,
	[DispenseHeightAboveRight] [nvarchar](max) NULL,
	[DispenseSLeft] [nvarchar](max) NULL,
	[DispenseCLeft] [nvarchar](max) NULL,
	[DispenseALeft] [nvarchar](max) NULL,
	[DispensePrismLeft] [nvarchar](max) NULL,
	[DispenseAddLeft] [nvarchar](max) NULL,
	[DispenseLensLeft] [nvarchar](max) NULL,
	[DispenseOCLeft] [nvarchar](max) NULL,
	[DispenseHeightAboveLeft] [nvarchar](max) NULL,
	[DispenseFrameDetails] [nvarchar](max) NULL,
	[DispenseFrame] [nvarchar](max) NULL,
	[DispenseFramePrice] [nvarchar](max) NULL,
	[DispenseLens] [nvarchar](max) NULL,
	[DispenseLensPrice] [nvarchar](max) NULL,
	[DispenseEyeExamination] [nvarchar](max) NULL,
	[DispenseEyeExaminationPrice] [nvarchar](max) NULL,
	[DispenseLessVoucher] [nvarchar](max) NULL,
	[DispenseTotal] [nvarchar](max) NULL,
	[DispenseDeposit] [nvarchar](max) NULL,
	[DispenseDepositDate] [datetime] NULL,
	[DispenseOutstandingBalance] [nvarchar](max) NULL,
	[DispenseOutstandingBalanceDate] [nvarchar](max) NULL,
	[appointmentCategory] [int] NOT NULL,
	[AppointmentRoom] [nvarchar](max) NULL,
	[NHSPrivate] [nvarchar](max) NULL,
	[NHSReason] [nvarchar](max) NULL,
	[NHSVoucher] [nvarchar](max) NULL,
	[CLFitExistingWearer] [nvarchar](max) NULL,
	[CLFitPreviousWearingDetails] [nvarchar](max) NULL,
	[CLFitType] [nvarchar](max) NULL,
	[CLFitWearingTime] [nvarchar](max) NULL,
	[CLFitSolutionsUsed] [nvarchar](max) NULL,
	[CLFitCurrentPreviousProblems] [nvarchar](max) NULL,
	[CLFitTrialComments] [nvarchar](max) NULL,
	[CLFitTrialOptometrist] [nvarchar](max) NULL,
	[CLFitWearingSchedule] [nvarchar](max) NULL,
	[CLFitCleaningRegime] [nvarchar](max) NULL,
	[CLFitDOHFormCompleted] [nvarchar](max) NULL,
	[CLFitCollectionLensesIn] [nvarchar](max) NULL,
	[CLFitCollectionWearingTime] [nvarchar](max) NULL,
	[CLFitCollectionAdvice] [nvarchar](max) NULL,
	[CLFitCollectionNextAppointment] [nvarchar](max) NULL,
	[CLFitCollectionOptometrist] [nvarchar](max) NULL,
	[AdviceAndRecallType] [int] NULL,
	[NextTestDueDate] [datetime] NULL,
	[lidslashesleft] [varchar](max) NULL,
	[cornealeft] [varchar](max) NULL,
	[acleft] [varchar](max) NULL,
	[vitreousleft] [varchar](max) NULL,
	[lensleft] [varchar](max) NULL,
	[disccdratioleft] [varchar](max) NULL,
	[discnrrleft] [varchar](max) NULL,
	[disccolourleft] [varchar](max) NULL,
	[marginsleft] [varchar](max) NULL,
	[vesselsleft] [varchar](max) NULL,
	[avratioleft] [varchar](max) NULL,
	[macularleft] [varchar](max) NULL,
	[peripheryleft] [varchar](max) NULL,
	[pupilsleft] [varchar](max) NULL,
	[lidslashesright] [varchar](max) NULL,
	[cornearight] [varchar](max) NULL,
	[acright] [varchar](max) NULL,
	[vitreousright] [varchar](max) NULL,
	[lensright] [varchar](max) NULL,
	[disccdratioright] [varchar](max) NULL,
	[discnrrright] [varchar](max) NULL,
	[disccolourright] [varchar](max) NULL,
	[marginsright] [varchar](max) NULL,
	[vesselsright] [varchar](max) NULL,
	[avratioright] [varchar](max) NULL,
	[macularright] [varchar](max) NULL,
	[peripheryright] [varchar](max) NULL,
	[pupilsright] [varchar](max) NULL,
	[CTDist] [varchar](max) NULL,
	[CTNear] [varchar](max) NULL,
	[PhoriaD] [varchar](max) NULL,
	[PhoriaN] [varchar](max) NULL,
	[NPC] [varchar](max) NULL,
	[IOPR1] [varchar](max) NULL,
	[IOPR2] [varchar](max) NULL,
	[IOPR3] [varchar](max) NULL,
	[IOPL1] [varchar](max) NULL,
	[IOPL2] [varchar](max) NULL,
	[IOPL3] [varchar](max) NULL,
	[VfieldsRight] [varchar](max) NULL,
	[VfieldsLeft] [varchar](max) NULL,
	[PupilRAPD] [int] NULL,
	[ConfrontationRight] [varchar](max) NULL,
	[ConfrontationLeft] [varchar](max) NULL,
	[Dialated] [int] NULL,
	[DrugUsed] [varchar](max) NULL,
	[TimeDrugUsed] [varchar](max) NULL,
	[IOPRAvg] [nvarchar](max) NULL,
	[IOPLAvg] [nvarchar](max) NULL,
	[IOPTime] [nvarchar](max) NULL,
	[VisualFieldsR] [nvarchar](max) NULL,
	[AmslerR] [nvarchar](max) NULL,
	[ColourVisionR] [nvarchar](max) NULL,
	[StereoVisionR] [nvarchar](max) NULL,
	[OtherR] [nvarchar](max) NULL,
	[PupilDiameterL] [nvarchar](max) NULL,
	[PupilDiameterR] [nvarchar](max) NULL,
	[UpperLidR] [nvarchar](max) NULL,
	[UpperLidL] [nvarchar](max) NULL,
	[CorneaL] [nvarchar](max) NULL,
	[CorneaR] [nvarchar](max) NULL,
	[LowerLidR] [nvarchar](max) NULL,
	[LowerLidL] [nvarchar](max) NULL,
	[MeibomianGlandsL] [nvarchar](max) NULL,
	[MeibomianGlandsR] [nvarchar](max) NULL,
	[LimbalAppearanceR] [nvarchar](max) NULL,
	[LimbalAppearanceL] [nvarchar](max) NULL,
	[CounjunctivalAppaeranceL] [nvarchar](max) NULL,
	[CounjunctivalAppaeranceR] [nvarchar](max) NULL,
	[TearQualityR] [nvarchar](max) NULL,
	[TearQualityL] [nvarchar](max) NULL,
	[LensTypeL] [int] NULL,
	[LensTypeR] [int] NULL,
	[SpecificationR] [nvarchar](max) NULL,
	[SpecificationL] [nvarchar](max) NULL,
	[FittingCentrationL] [nvarchar](max) NULL,
	[FittingCentrationR] [nvarchar](max) NULL,
	[MovementR] [nvarchar](max) NULL,
	[MovementL] [nvarchar](max) NULL,
	[CLSphL] [nvarchar](max) NULL,
	[CLSphR] [nvarchar](max) NULL,
	[CLCyl] [nvarchar](max) NULL,
	[CLAxisR] [nvarchar](max) NULL,
	[CLAxisL] [nvarchar](max) NULL,
	[CLVaR] [nvarchar](max) NULL,
	[CLVaL] [nvarchar](max) NULL,
	[CLOverReactionL] [nvarchar](max) NULL,
	[CLOverReactionR] [nvarchar](max) NULL,
	[CLBestVaR] [nvarchar](max) NULL,
	[CLBestVaL] [nvarchar](max) NULL,
	[LensTypeR2] [nvarchar](max) NULL,
	[LensTypeL2] [nvarchar](max) NULL,
	[SpecificationR2] [nvarchar](max) NULL,
	[SpecificationL2] [nvarchar](max) NULL,
	[FittingCentrationL2] [nvarchar](max) NULL,
	[FittingCentrationR2] [nvarchar](max) NULL,
	[MovementR2] [nvarchar](max) NULL,
	[MovementL2] [nvarchar](max) NULL,
	[AppointmentComplete] [int] NULL,
	[CleaningRegime] [nvarchar](max) NULL,
	[LastTestDate] [datetime] NULL,
	[Smoker] [int] NULL,
	[HasArrived] [bit] NULL,
	[ContactLensDueDate] [datetime] NULL,
	[NVisionLeft] [nvarchar](max) NULL,
	[NVALeft] [nvarchar](max) NULL,
	[BinVALeft] [nvarchar](max) NULL,
	[BinNVALeft] [nvarchar](max) NULL,
	[BinNVARight] [nvarchar](max) NULL,
	[PDLeftNear] [real] NULL,
	[PDRightNear] [real] NULL,
	[ConjunctivaRight] [nvarchar](max) NULL,
	[ConjunctivaLeft] [nvarchar](max) NULL,
	[IrisRight] [nvarchar](max) NULL,
	[IrisLeft] [nvarchar](max) NULL,
	[anisocoria] [int] NULL,
	[CLCylLeft] [nvarchar](max) NULL,
	[LensTypeLeft] [int] NULL,
	[LensTypeRight] [int] NULL,
	[ManufacturerLeft] [nvarchar](max) NULL,
	[ManufacturerRight] [nvarchar](max) NULL,
	[LensNameLeft] [nvarchar](200) NULL,
	[LensNameRight] [nvarchar](200) NULL,
	[CLBozrRight] [real] NULL,
	[CLTdRight] [real] NULL,
	[CLBinVARight] [nvarchar](max) NULL,
	[CLBozrLeft] [real] NULL,
	[CLTdLeft] [real] NULL,
	[CLBinVALeft] [nvarchar](max) NULL,
	[CLCondofLensLeft] [nvarchar](max) NULL,
	[CLCondofLensRight] [nvarchar](max) NULL,
	[CLLagRight] [nvarchar](max) NULL,
	[CLLagLeft] [nvarchar](max) NULL,
	[CLToricRotationLeft] [nvarchar](max) NULL,
	[CLToricRotationRight] [nvarchar](max) NULL,
	[CLCondofLensLeft2] [nvarchar](max) NULL,
	[CLCondofLensRight2] [nvarchar](max) NULL,
	[CLLagRight2] [nvarchar](max) NULL,
	[CLLagLeft2] [nvarchar](max) NULL,
	[CLToricRotationLeft2] [nvarchar](max) NULL,
	[CLToricRotationRight2] [nvarchar](max) NULL,
	[DrugExpiry] [nvarchar](max) NULL,
	[DrugBatch] [nvarchar](max) NULL,
	[RetDvisionR] [nvarchar](max) NULL,
	[RetDvisionL] [nvarchar](max) NULL,
	[RetNvisionR] [nvarchar](max) NULL,
	[RetNvisionL] [nvarchar](max) NULL,
	[RetSphereR] [real] NULL,
	[RetSphereL] [real] NULL,
	[RetCylR] [real] NULL,
	[RetCylL] [real] NULL,
	[RetAxisR] [real] NULL,
	[RetAxisL] [real] NULL,
	[TbutL] [nvarchar](max) NULL,
	[TbutR] [nvarchar](max) NULL,
	[SplitACR] [nvarchar](max) NULL,
	[SplitACL] [nvarchar](max) NULL,
	[CLNotes] [nvarchar](max) NULL,
	[InsertionRemoval] [int] NULL,
	[ReasonforCLApp] [nvarchar](max) NULL,
	[FirstLensType] [nvarchar](max) NULL,
	[SecondLensType] [nvarchar](max) NULL,
	[FirstOccupational] [bit] NULL,
	[FirstUV] [bit] NULL,
	[FirstPhotochromic] [bit] NULL,
	[FirstMAR] [bit] NULL,
	[FirstTint] [bit] NULL,
	[FirstScratch] [bit] NULL,
	[FirstThin] [bit] NULL,
	[SecondOccupational] [bit] NULL,
	[SecondUV] [bit] NULL,
	[SecondPhotochromic] [bit] NULL,
	[SecondMAR] [bit] NULL,
	[SecondTint] [bit] NULL,
	[SecondScratch] [bit] NULL,
	[SecondThin] [bit] NULL,
	[CreatedByUser_Id] [nvarchar](128) NULL,
	[CreatedTimestamp] [datetime] NOT NULL CONSTRAINT [DF_EyeExams_CreatedTimestamp]  DEFAULT (getdate()),
	[UpdatedTimestamp] [datetime] NULL CONSTRAINT [DF_EyeExams_UpdatedTimestamp]  DEFAULT (getdate()),
	[Deleted] [datetime] NULL,
	[DeletedByUser_Id] [nvarchar](128) NULL,
	[Optician_Id] [nvarchar](128) NOT NULL,
	[RBaseDistV] [int] NULL,
	[LBaseDistV] [int] NULL,
	[RBaseNearV] [int] NULL,
	[LBaseNearV] [int] NULL,
	[RPrismDistV] [real] NULL,
	[LPrismDistV] [real] NULL,
	[RPrismNearV] [real] NULL,
	[LPrismNearV] [real] NULL,
	[SubRVisionD] [nvarchar](50) NULL,
	[SubRVisionN] [nvarchar](50) NULL,
	[SubLVisionD] [nvarchar](50) NULL,
	[SubLVisionN] [nvarchar](50) NULL,
	[SubRSph] [real] NULL,
	[SubLSph] [real] NULL,
	[SubRCyl] [real] NULL,
	[SubLCyl] [real] NULL,
	[SubRAxis] [real] NULL,
	[SubLAxis] [real] NULL,
	[SubRPrismD] [real] NULL,
	[SubLPrismD] [real] NULL,
	[SubRVA] [nvarchar](50) NULL,
	[SubLVA] [nvarchar](50) NULL,
	[SubRBinVA] [nvarchar](50) NULL,
	[SubLBinVA] [nvarchar](50) NULL,
	[SubRIAdd] [real] NULL,
	[SubLIAdd] [real] NULL,
	[SubRNAdd] [real] NULL,
	[SubLNAdd] [real] NULL,
	[SubRNVA] [nvarchar](50) NULL,
	[SubLNVA] [nvarchar](50) NULL,
	[SubRBinNVA] [nvarchar](50) NULL,
	[SubLBinNVA] [nvarchar](50) NULL,
	[SubRPrismN] [real] NULL,
	[SubLPrismN] [real] NULL,
	[OphthalmoscopyMethodUsed] [nvarchar](max) NULL,
	[OphthalmoscopyRDisc] [nvarchar](max) NULL,
	[OphthalmoscopyLDisc] [nvarchar](max) NULL,
	[OphthalmoscopyRNRR] [nvarchar](max) NULL,
	[OphthalmoscopyLNRR] [nvarchar](max) NULL,
	[OphthalmoscopyRCDRatio] [nvarchar](max) NULL,
	[OphthalmoscopyLCDRatio] [nvarchar](max) NULL,
	[OphthalmoscopyRVessels] [nvarchar](max) NULL,
	[OphthalmoscopyLVessels] [nvarchar](max) NULL,
	[OphthalmoscopyRPeriphery] [nvarchar](max) NULL,
	[OphthalmoscopyLPeriphery] [nvarchar](max) NULL,
	[OphthalmoscopyRMacula] [nvarchar](max) NULL,
	[OphthalmoscopyLMacula] [nvarchar](max) NULL,
	[OphthalmoscopyDescription] [nvarchar](max) NULL,
	[PatientAdvice] [nvarchar](max) NULL,
	[ProductRecommendations] [nvarchar](max) NULL,
	[ReferralSent] [bit] NOT NULL CONSTRAINT [DF_EyeExams_ReferralSent]  DEFAULT ((0)),
	[SymptomsAndHistory] [nvarchar](2000) NULL,
	[Allergies] [nvarchar](2000) NULL,
	[IOPR4] [varchar](max) NULL,
	[IOPR5] [varchar](max) NULL,
	[IOPL4] [varchar](max) NULL,
	[IOPL5] [varchar](max) NULL,
	[ObjectiveMethodEnum] [int] NOT NULL CONSTRAINT [DF_EyeExams_ObjectMethodEnum]  DEFAULT ((0)),
	[RHeight] [real] NULL,
	[LHeight] [real] NULL,
	[Dipl] [nvarchar](2000) NULL,
	[HA] [nvarchar](2000) NULL,
	[FF] [nvarchar](2000) NULL,
	[ExternalOpticianName] [nvarchar](200) NULL,
	[ExternalPracticeName] [nvarchar](2000) NULL,
 CONSTRAINT [PK__EyeTests__3214EC0707020F21] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[FieldTemplates]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FieldTemplates](
	[Id] [uniqueidentifier] NOT NULL,
	[company_Id] [uniqueidentifier] NOT NULL,
	[TemplateTextFieldEnum] [int] NOT NULL,
	[TemplateText] [nvarchar](2000) NOT NULL,
 CONSTRAINT [PK_FieldTemplates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FrequencyUnit]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FrequencyUnit](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_FrequencyUnit] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Inventory]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Inventory](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Inventory_Id]  DEFAULT (newid()),
	[Price] [money] NOT NULL,
	[Quantity] [int] NOT NULL,
	[VATRateValue] [decimal](18, 0) NOT NULL,
	[practice_Id] [uniqueidentifier] NULL,
	[customer_Id] [uniqueidentifier] NULL,
	[supplier_Id] [uniqueidentifier] NULL,
	[product_Id] [uniqueidentifier] NOT NULL,
	[CreatedByUser_Id] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Inventory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[InvoiceDetails]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InvoiceDetails](
	[Id] [uniqueidentifier] NOT NULL,
	[invoice_Id] [uniqueidentifier] NOT NULL,
	[UnitPrice] [decimal](18, 2) NOT NULL,
	[Quantity] [int] NOT NULL,
	[product_Id] [uniqueidentifier] NOT NULL,
	[CreatedByUser_Id] [uniqueidentifier] NOT NULL,
	[Deleted] [datetime] NULL,
	[DeletedByUser_Id] [uniqueidentifier] NULL,
	[VATRate] [decimal](18, 2) NOT NULL,
	[Added] [datetime] NOT NULL CONSTRAINT [DF_InvoiceDetails_Added]  DEFAULT (getdate()),
	[SpectacleNumber] [int] NULL,
	[Description] [nvarchar](2000) NULL,
	[ReconciliationStatusEnum] [int] NOT NULL CONSTRAINT [DF_InvoiceDetails_ReconciliationStatusEnum]  DEFAULT ((0)),
	[ReconciledStatusUpdated] [datetime] NULL,
	[ReconciledStatusUpdatedByUser_Id] [uniqueidentifier] NULL,
	[ReconciliationNotes] [nvarchar](2000) NULL,
	[DiscountPercentage] [decimal](18, 2) NOT NULL CONSTRAINT [DF_InvoiceDetails_DiscountPercentage]  DEFAULT ((0)),
	[StatusEnum] [int] NOT NULL CONSTRAINT [DF_InvoiceDetails_InvoiceStatusEnum]  DEFAULT ((0)),
	[CostPrice] [money] NULL,
	[Dispensed] [bit] NOT NULL CONSTRAINT [DF_InvoiceDetails_Dispensed]  DEFAULT ((0)),
	[Code] [nvarchar](50) NULL,
 CONSTRAINT [PK__InvoiceD__3214EC070EA330E9] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Invoices]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Invoices](
	[Id] [uniqueidentifier] NOT NULL,
	[CreatedTimestamp] [datetime] NOT NULL,
	[InvoiceDate] [datetime] NOT NULL,
	[customer_Id] [uniqueidentifier] NULL,
	[Notes] [nvarchar](max) NULL,
	[CreatedByUser_Id] [uniqueidentifier] NOT NULL,
	[practice_Id] [uniqueidentifier] NOT NULL,
	[Deleted] [datetime] NULL,
	[DeletedByUser_Id] [uniqueidentifier] NULL,
	[InvoiceNumber] [nvarchar](50) NOT NULL,
	[DiscountPercentage] [decimal](18, 2) NOT NULL CONSTRAINT [DF_Invoices_DiscountPercentage]  DEFAULT ((0)),
	[MethodSentByEnum] [int] NOT NULL CONSTRAINT [DF_Invoices_MethodSentByEnum]  DEFAULT ((1)),
	[DispenseEyeExam_Id] [uniqueidentifier] NULL,
	[TotalIncVAT] [decimal](18, 2) NOT NULL CONSTRAINT [DF_Invoices_TotalIncVAT]  DEFAULT ((0)),
	[TotalExcVAT] [decimal](18, 2) NOT NULL CONSTRAINT [DF_Invoices_TotalExcVAT]  DEFAULT ((0)),
	[BalanceIncVAT] [decimal](18, 2) NOT NULL CONSTRAINT [DF_Invoices_BalanceIncVAT]  DEFAULT ((0)),
	[BalanceExcVAT] [decimal](18, 2) NOT NULL CONSTRAINT [DF_Invoices_BalanceExcVAT]  DEFAULT ((0)),
 CONSTRAINT [PK__Invoices__3214EC070AD2A005] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[IOPs]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IOPs](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_IOPs_Id]  DEFAULT (newid()),
	[eyeExam_Id] [uniqueidentifier] NOT NULL,
	[eye] [int] NOT NULL,
	[Value1] [real] NULL,
	[Value2] [real] NULL,
	[Value3] [real] NULL,
	[Value4] [real] NULL,
	[Value5] [real] NULL,
	[Average] [real] NOT NULL,
	[Added] [datetime] NOT NULL CONSTRAINT [DF_IOPs_Added]  DEFAULT (getdate()),
	[Deleted] [datetime] NULL,
	[DeletedByUser_Id] [nvarchar](128) NULL,
 CONSTRAINT [PK_IOPs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Messages]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Messages](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Messages_Id]  DEFAULT (newid()),
	[RecipientId] [uniqueidentifier] NULL,
	[ToAddressNumber] [nvarchar](1000) NOT NULL,
	[messageMethod] [int] NOT NULL,
	[Subject] [nvarchar](50) NULL,
	[MessageText] [nvarchar](max) NOT NULL,
	[AddedToQueue] [datetime] NOT NULL CONSTRAINT [DF_Messages_AddedToQueue]  DEFAULT (getdate()),
	[Sent] [datetime] NULL,
	[Cancelled] [datetime] NULL,
	[SenderUser_Id] [uniqueidentifier] NOT NULL,
	[CancelledByUser_Id] [uniqueidentifier] NULL,
	[IsRecall] [bit] NOT NULL CONSTRAINT [DF_Messages_IsRecall]  DEFAULT ((0)),
	[SMSInventory_Id] [uniqueidentifier] NULL,
	[practice_Id] [uniqueidentifier] NOT NULL,
	[Sender] [nvarchar](50) NOT NULL,
	[Ref] [nvarchar](500) NULL,
	[ScheduledToBeSent] [datetime] NULL,
	[Postcode] [nvarchar](20) NULL,
	[StatusMessage] [nvarchar](2000) NULL,
 CONSTRAINT [PK_Messages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MessageTemplates]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MessageTemplates](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_MessageTemplates_Id]  DEFAULT (newid()),
	[messageMethod] [int] NOT NULL,
	[Template] [nvarchar](max) NOT NULL,
	[Subject] [nvarchar](max) NULL,
	[company_Id] [uniqueidentifier] NOT NULL,
	[MessageTypeEnum] [int] NOT NULL CONSTRAINT [DF_MessageTemplates_MessageTypeEnum]  DEFAULT ((0)),
 CONSTRAINT [PK_MessageTemplates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Notes]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notes](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Notes_Id]  DEFAULT (newid()),
	[customer_Id] [uniqueidentifier] NULL,
	[Description] [nvarchar](2000) NOT NULL,
	[CreatedTimestamp] [datetime] NOT NULL CONSTRAINT [DF_Notes_CreatedTimestamp]  DEFAULT (getdate()),
	[CreatedByUser_Id] [uniqueidentifier] NOT NULL,
	[Deleted] [datetime] NULL,
	[DeletedByUser_Id] [uniqueidentifier] NULL,
	[practice_Id] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Notes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PostcodeLookups]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PostcodeLookups](
	[Id] [uniqueidentifier] NOT NULL,
	[InsertTimestamp] [datetime] NOT NULL,
	[company_Id] [uniqueidentifier] NOT NULL,
	[Quantity] [int] NOT NULL,
	[user_Id] [uniqueidentifier] NULL,
	[PostcodeSearchedFor] [nvarchar](50) NULL,
	[NoOfResults] [int] NULL,
 CONSTRAINT [PK_PostcodeLookup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Practices]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Practices](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Practices_Id]  DEFAULT (newid()),
	[Name] [nvarchar](100) NOT NULL,
	[Address] [nvarchar](200) NOT NULL,
	[Postcode] [nvarchar](20) NOT NULL,
	[Tel] [nvarchar](100) NULL,
	[Fax] [nvarchar](100) NULL,
	[Email] [nvarchar](100) NULL,
	[company_Id] [uniqueidentifier] NOT NULL,
	[ExternalId] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Practices_ExternalId]  DEFAULT (newid()),
	[CreatedByUser_Id] [uniqueidentifier] NOT NULL,
	[CanSendMessages] [bit] NOT NULL CONSTRAINT [DF_Practices_CanSendMessages]  DEFAULT ((1)),
	[DefaultAppointmentLength] [int] NOT NULL CONSTRAINT [DF_Practices_DefaultAppointmentLength]  DEFAULT ((30)),
	[WorkDayStart] [datetime] NOT NULL,
	[WorkDayEnd] [datetime] NOT NULL,
	[PrimaryCareTrustGOS] [nvarchar](50) NULL,
	[PracticeNumber] [int] NOT NULL,
	[LastInvoiceNumber] [int] NOT NULL,
	[DefaultOptician_Id] [nvarchar](128) NULL,
	[PracticeRef] [nvarchar](4) NOT NULL,
	[Added] [datetime] NULL CONSTRAINT [DF_Practices_Added]  DEFAULT (getdate()),
	[ShowGOSForms] [bit] NOT NULL CONSTRAINT [DF_Practices_ShowGOSForms]  DEFAULT ((1)),
	[SMSSenderName] [nvarchar](50) NULL,
	[SchedulerMinorTickCount] [int] NOT NULL CONSTRAINT [DF_Practices_MinorTickCount]  DEFAULT ((2)),
	[SchedulerMajorTick] [int] NOT NULL CONSTRAINT [DF_Practices_SchedulerMajorTick]  DEFAULT ((60)),
	[LastPatientNumber] [int] NOT NULL CONSTRAINT [DF_Practices_LastPatientNumber]  DEFAULT ((0)),
	[VATAppliedToSalePercentage] [int] NOT NULL CONSTRAINT [DF_Practices_VATAppliedToSalePercentage]  DEFAULT ((100)),
	[ContractorNumber] [nvarchar](50) NULL,
	[ContractorName] [nvarchar](200) NULL,
	[ShowDOBOnPatientSearch] [bit] NOT NULL CONSTRAINT [DF_Practices_ShowDOBOnPatientSearch]  DEFAULT ((0)),
	[TelAreaPrefix] [nvarchar](10) NULL,
	[EyeExamScreenDesign] [int] NOT NULL CONSTRAINT [DF_Practices_UseEyeExamAlternative]  DEFAULT ((1)),
	[DefaultEyeExamTimeToPatientsAppointment] [bit] NOT NULL CONSTRAINT [DF_Practices_DefaultEyeExamStartToPatientsAppointment]  DEFAULT ((0)),
	[ShowDomiciliaryFields] [bit] NOT NULL CONSTRAINT [DF_Practices_ShowDomiciliaryFields]  DEFAULT ((0)),
	[ShowCallButtons] [bit] NOT NULL CONSTRAINT [DF_Practices_ShowCallButtons]  DEFAULT ((1)),
	[ShowPracticeNotesOnDashboard] [bit] NOT NULL CONSTRAINT [DF_Practices_ShowPracticeNotesOnDashboard]  DEFAULT ((0)),
	[EditPatientFromCalendar] [bit] NOT NULL CONSTRAINT [DF_Practices_EditPatientFromCalendar]  DEFAULT ((0)),
	[RecallDateCutOff] [datetime] NOT NULL CONSTRAINT [DF_Practices_RecallDateCutOff_1]  DEFAULT ('2014-01-01 00:00:00'),
	[MonthlyRate] [money] NOT NULL CONSTRAINT [DF_Practices_MonthlyRate]  DEFAULT ((0)),
 CONSTRAINT [PK__Practice__3214EC071273C1CD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ__Practice__31A76CC7578AA2A7] UNIQUE NONCLUSTERED 
(
	[PracticeRef] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ__Practice__59E86AC2310273C7] UNIQUE NONCLUSTERED 
(
	[PracticeNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Products]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Products_Id]  DEFAULT (newid()),
	[Name] [nvarchar](2000) NOT NULL,
	[Price] [money] NOT NULL,
	[VATRate] [decimal](18, 0) NOT NULL,
	[ProductTypeEnum] [int] NOT NULL CONSTRAINT [DF_Products_ProductTypeEnum]  DEFAULT ((0)),
	[Code] [nvarchar](50) NULL,
	[company_Id] [uniqueidentifier] NULL,
	[CreatedByUser_Id] [uniqueidentifier] NULL,
	[LastUpdated] [datetime] NOT NULL CONSTRAINT [DF_Products_LastUpdated]  DEFAULT (getdate()),
	[Deleted] [datetime] NULL,
	[DeletedByUser_Id] [uniqueidentifier] NULL,
	[Colour] [nvarchar](50) NULL,
	[ReferenceNumber] [nvarchar](50) NULL,
	[Description] [nvarchar](2000) NULL,
	[FrequentlyUsed] [bit] NOT NULL CONSTRAINT [DF_Products_FrequentlyUsed]  DEFAULT ((0)),
	[CostPrice] [money] NULL,
	[Manufacturer] [nvarchar](50) NULL,
	[LensType] [nvarchar](50) NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ProductTypes]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductTypes](
	[Id] [int] NOT NULL,
	[ProductName] [nvarchar](2000) NOT NULL,
	[NegativeValue] [bit] NOT NULL,
 CONSTRAINT [PK_ProductTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[RecallDocuments]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecallDocuments](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_RecallDocuments_Id]  DEFAULT (newid()),
	[recallTemplate_Id] [uniqueidentifier] NOT NULL,
	[documentTemplate_Id] [uniqueidentifier] NOT NULL,
	[Deleted] [datetime] NULL,
	[BeforeOrAfterEnum] [int] NOT NULL,
	[DateSpanValue] [int] NOT NULL,
	[DateSpanUnit] [int] NOT NULL CONSTRAINT [DF_RecallDocuments_DateSpanUnit]  DEFAULT ((2)),
 CONSTRAINT [PK_RecallDocuments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[RecallTemplates]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecallTemplates](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_RecallTemplates_Id]  DEFAULT (newid()),
	[Name] [nvarchar](50) NOT NULL,
	[Deleted] [datetime] NULL,
	[company_Id] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_RecallTemplates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ReconciliationStatuses]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReconciliationStatuses](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ReconciliationStatuses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Reports]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reports](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Reports_Id]  DEFAULT (newid()),
	[practice_Id] [uniqueidentifier] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[IsCustomReport] [bit] NOT NULL CONSTRAINT [DF_Reports_IsCustomReport]  DEFAULT ((0)),
	[DisplayName] [nvarchar](50) NULL,
 CONSTRAINT [PK_Reports] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Rooms]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rooms](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[practice_Id] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Rooms] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Settings]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Settings](
	[Id] [int] NOT NULL,
	[DailyDatabaseBackupTime] [datetime] NOT NULL,
	[DailyBackupLastTaken] [datetime] NULL,
	[SMSStartTime] [datetime] NOT NULL,
	[SMSEndTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SMSInventory]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SMSInventory](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_SMSStock_Id]  DEFAULT (newid()),
	[company_Id] [uniqueidentifier] NOT NULL,
	[InsertTimestamp] [datetime] NOT NULL CONSTRAINT [DF_SMSStock_WhenBought]  DEFAULT (getdate()),
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_SMSStock] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Suppliers]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Suppliers](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Suppliers_Id]  DEFAULT (newid()),
	[Name] [nvarchar](max) NOT NULL,
	[Address] [nvarchar](max) NULL,
	[Postcode] [nvarchar](max) NULL,
	[Telephone] [nvarchar](max) NULL,
	[Fax] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[CompanyId] [uniqueidentifier] NOT NULL,
	[CreatedByUser_Id] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Suppliers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Tags]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tags](
	[Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Tags_Id]  DEFAULT (newid()),
	[Name] [nvarchar](50) NOT NULL,
	[company_Id] [uniqueidentifier] NULL,
	[Deleted] [datetime] NULL,
	[TagTypeEnum] [int] NOT NULL CONSTRAINT [DF_Tags_TagTypeEnum]  DEFAULT ((0)),
 CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TagType]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TagType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_TagType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TestFrequencies]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TestFrequencies](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Tickets]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tickets](
	[Id] [uniqueidentifier] NOT NULL,
	[CreatedTimestamp] [datetime] NOT NULL,
	[CreatedByUser_Id] [uniqueidentifier] NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Due] [datetime] NULL,
	[Priority_Id] [int] NOT NULL,
	[Type_Id] [int] NOT NULL,
	[Status_Id] [int] NOT NULL,
 CONSTRAINT [PK_Tickets] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[VATRate]    Script Date: 10/07/2015 20:18:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VATRate](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[VATRateValue] [decimal](18, 0) NOT NULL,
	[Enabled] [bit] NOT NULL,
 CONSTRAINT [PK_VATRate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [RoleNameIndex]    Script Date: 10/07/2015 20:18:47 ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_UserId]    Script Date: 10/07/2015 20:18:47 ******/
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_UserId]    Script Date: 10/07/2015 20:18:47 ******/
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_RoleId]    Script Date: 10/07/2015 20:18:47 ******/
CREATE NONCLUSTERED INDEX [IX_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_UserId]    Script Date: 10/07/2015 20:18:47 ******/
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[AspNetUserRoles]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [UserNameIndex]    Script Date: 10/07/2015 20:18:47 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_Customer_PatientNumber]    Script Date: 10/07/2015 20:18:47 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Customer_PatientNumber] ON [dbo].[Customers]
(
	[practice_Id] ASC,
	[Number] ASC
)
WHERE ([Number] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PostcodeLookups] ADD  CONSTRAINT [DF_PostcodeLookup_Id]  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[PostcodeLookups] ADD  CONSTRAINT [DF_PostcodeLookup_InsertTimestamp]  DEFAULT (getdate()) FOR [InsertTimestamp]
GO
ALTER TABLE [dbo].[Rooms] ADD  CONSTRAINT [DF_Rooms_Id]  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[Tickets] ADD  CONSTRAINT [DF_Tickets_Id]  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[Tickets] ADD  CONSTRAINT [DF_Table_1_InsertTimestamp]  DEFAULT (getdate()) FOR [CreatedTimestamp]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[Customers]  WITH CHECK ADD  CONSTRAINT [FK_Customers_Practices] FOREIGN KEY([practice_Id])
REFERENCES [dbo].[Practices] ([Id])
GO
ALTER TABLE [dbo].[Customers] CHECK CONSTRAINT [FK_Customers_Practices]
GO
ALTER TABLE [dbo].[InvoiceDetails]  WITH CHECK ADD  CONSTRAINT [Invoice_InvoiceDetails] FOREIGN KEY([invoice_Id])
REFERENCES [dbo].[Invoices] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[InvoiceDetails] CHECK CONSTRAINT [Invoice_InvoiceDetails]
GO
ALTER TABLE [dbo].[Invoices]  WITH CHECK ADD  CONSTRAINT [Customer_Invoices] FOREIGN KEY([customer_Id])
REFERENCES [dbo].[Customers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Invoices] CHECK CONSTRAINT [Customer_Invoices]
GO
ALTER TABLE [dbo].[Practices]  WITH CHECK ADD  CONSTRAINT [FK_Practices_Companies] FOREIGN KEY([company_Id])
REFERENCES [dbo].[Companies] ([Id])
GO
ALTER TABLE [dbo].[Practices] CHECK CONSTRAINT [FK_Practices_Companies]
GO