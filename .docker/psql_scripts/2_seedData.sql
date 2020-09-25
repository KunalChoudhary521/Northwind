INSERT INTO "Locations" ("Address", "Country") VALUES
('1 seed location', 'USA'),
('2 seed location', 'Canada');

INSERT INTO "Suppliers" ("CompanyName", "ContactName", "LocationId") VALUES
('seed supplier 1', 'Dev 1', 1),
('seed supplier 2', 'Dev 2', 2);

INSERT INTO "Categories" ("CategoryName", "Description") VALUES
('seed category 1', 'category 1'),
('seed category 2', 'category 2');

INSERT INTO "Products" ("ProductName", "UnitPrice", "UnitsInStock",
                        "UnitsOnOrder", "ReorderLevel", "Discontinued",
                        "SupplierId", "CategoryId") VALUES
('seed product 1', 1.49, 10, 0, 0, 0, 1, 1),
('seed product 2', 2.79, 15, 0, 0, 0, 1, 2),
('seed product 3', 3.49, 18, 0, 0, 0, 2, 1),
('seed product 4', 4.79, 27, 0, 0, 0, 2, 2);

INSERT INTO "Customers" ("CompanyName", "ContactName", "LocationId") VALUES
('seed customer 1', 'Cust 1', 1),
('seed customer 2', 'Cust 2', 2);

INSERT INTO "Shippers" ("CompanyName", "Phone") VALUES
('shipper 1', '416-908-4933'),
('shipper 2', '981-561-0176');

INSERT INTO "Orders" ("CustomerId", "OrderDate", "RequiredDate", "LocationId", "ShipVia") VALUES
(1, '2020-8-16 15:41:52', '2020-9-16 17:00:00', 1, 1),
(2, '2020-8-20 18:14:13', '2020-9-1 9:00:00', 2, 1),
(1, '2020-8-23 11:03:13', '2020-10-14 19:00:00', 2, null);

INSERT INTO "Order Details" ("OrderId", "ProductId", "UnitPrice", "Quantity", "Discount") VALUES
(1, 1, 1.49, 4, 0),
(1, 2, 2.79, 5, 0),
(2, 3, 3.49, 3, 0),
(2, 4, 4.79, 2, 0);
