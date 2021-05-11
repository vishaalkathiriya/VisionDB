ALTER PROCEDURE [dbo].[sp_VDB_InvoiceDetails]
	@invoice_Id uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

    select p.Name, 
	id.UnitPrice, 
	id.Quantity, id.VATRate, 
	id.UnitPrice * id.Quantity LineTotal,
	id.UnitPrice * id.Quantity * ((100 - id.VATRate) / 100) LineTotalExcVAT,
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
	id.UnitPrice * id.Quantity * ((100 - id.VATRate) / 100) LineTotalExcVAT,
	isnull(id.SpectacleNumber, 99) SpectacleNumber, 
	id.Description, p.Colour,
	pt.NegativeValue, id.Added
	from InvoiceDetails id
	inner join Products p on p.Id = id.product_Id
	inner join ProductTypes pt on pt.Id = p.ProductTypeEnum
	where id.invoice_Id = @invoice_Id and id.Deleted is null and id.DiscountPercentage > 0
	order by pt.NegativeValue, isnull(id.SpectacleNumber, 99), id.Added
END