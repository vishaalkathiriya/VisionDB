--This script set the password of a user to password

--select * from AspNetUsers where UserName like 'b'

declare @userId nvarchar(128) = '33ef518c-fc7c-4644-8101-1549d191d128'
update AspNetUsers 
set PasswordHash = 'AN9yfM9UrvsLrws+HQENf+nLiBvOncN8Wy6X63AcjddJZgIi0/C9lRQVQRBEDSuB6A==', -- password will be set to 'password'
SecurityStamp = '7bf19a4c-475b-4203-b99f-b5897a48104f' -- password will be set to 'password'
where Id = @userId