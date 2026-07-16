USE MenStyleDb;
GO

SELECT Id, Email, UserName, FullName, PhoneNumber, Address, Gender
FROM AspNetUsers;

SELECT u.Email, r.Name AS RoleName
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id;

SELECT Id, Name, CategoryName, CategorySlug, Price, OldPrice, IsActive
FROM Products;

SELECT Id, OrderCode, CustomerName, PhoneNumber, ShippingAddress, TotalAmount, Status, CreatedAt
FROM CustomerOrders
ORDER BY CreatedAt DESC;

SELECT CustomerOrderId, ProductName, Price, Quantity, LineTotal
FROM CustomerOrderItems
ORDER BY CustomerOrderId DESC;