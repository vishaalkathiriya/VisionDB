CREATE PROCEDURE [dbo].[sp_VDB_IOPData]
	@EyeExamId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	select 
	e.Name Eye,
	i.Value1,
	i.Value2,
	i.Value3,
	i.Value4,
	i.Value5,
	i.Average,
	i.Added
	from IOPs i
	inner join Enums e on e.EnumName = 'Eye' and e.EnumId = i.eye
	where i.eyeExam_Id = @EyeExamId and Deleted is null
	order by i.Added
END
GO