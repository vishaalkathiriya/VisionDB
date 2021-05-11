BEGIN TRANSACTION
GO
ALTER TABLE dbo.Practices ADD
	PatientNumberPrefix nvarchar(20) NULL
GO
COMMIT
