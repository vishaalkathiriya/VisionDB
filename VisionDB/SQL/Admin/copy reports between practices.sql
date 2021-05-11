declare @ExistingPracticeId uniqueidentifier = 'F55D3669-D239-2C22-23D0-D49BA8312CE4'
declare @NewPracticeId uniqueidentifier = '32B4791A-43CE-4AE5-AE23-44B27AF15C9D'

--insert into Reports (practice_Id, Name, IsCustomReport, DisplayName)
select @NewPracticeId, Name, IsCustomReport, DisplayName from Reports where practice_Id = @ExistingPracticeId