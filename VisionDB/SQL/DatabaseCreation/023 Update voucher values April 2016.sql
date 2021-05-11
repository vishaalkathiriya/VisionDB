update Products set Price = '-39.10' where Id = '8D50CE47-03E7-487D-AA98-F1E38135D833'
update Products set Price = '-59.30' where Id = '620A7392-8686-4E55-8EC8-FCC84955F95E'
update Products set Price = '-86.90' where Id = '75AD544E-F9D7-4464-90E7-4710E9AD74F9'
update Products set Price = '-196.00' where Id = 'DAFB14D1-461E-4662-8F04-9F85BBC3A2D0'
update Products set Price = '-67.50' where Id = 'E101CD90-90A7-4D9C-A952-B401C00E175F'
update Products set Price = '-85.60' where Id = '28DB1CCC-2006-4135-A578-35780EEF28C4'
update Products set Price = '-111.20' where Id = '45B5205A-91C3-43D8-AD01-CAF5F0EDD506'
update Products set Price = '-215.50' where Id = 'C31812C0-864E-445E-806C-5406D035E7ED'

select * from Products p where p.company_Id is null and p.ProductTypeEnum = 6 order by p.Name