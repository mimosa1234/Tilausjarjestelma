USE Tilausjarjestelma;
GO

-- Customers
INSERT INTO Customers (FirstName, LastName, Email, Phone, Address) VALUES
('Matti', 'Meikäläinen', 'matti@example.com', '0401231234', 'Esimerkkikatu 1'),
('Maija', 'Mallikas', 'maija@example.com', '0509879876', 'Testikuja 9');

-- Categories
INSERT INTO Categories (Name) VALUES
('Elektroniikka'),
('Kodinkoneet'),
('Kirjat'),
('Lelut');

-- Products
INSERT INTO Products (Name, Price, Description, CategoryId, Stock) VALUES
('Kannettava tietokone', 999.99, '14” kevyt kannettava', 1, 10),
('Älypuhelin', 699.00, 'Uusin malli', 1, 25),
('Kahvinkeitin', 49.90, 'Perusmalli 12 kuppia', 2, 15),
('Pölynimuri', 129.00, 'Tehokas HEPA-suodattimella', 2, 5),
('Fantasiakirja', 19.99, 'Seikkailu alkaa…', 3, 30),
('Nallepehmolelu', 12.50, 'Söpö ja pehmeä', 4, 50);

-- Test order (optional)
INSERT INTO Orders (CustomerId, CustomerName, CustomerEmail, CustomerPhone, CustomerAddress)
SELECT CustomerId, FirstName + ' ' + LastName, Email, Phone, Address
FROM Customers WHERE FirstName = 'Matti';

DECLARE @orderId INT = SCOPE_IDENTITY();

INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice)
VALUES
(@orderId, 1, 1, 999.99),
(@orderId, 5, 2, 19.99);

-- päivitellään stock manuaalisesti testitilaukselle
UPDATE Products SET Stock = Stock - 1 WHERE ProductId = 1;
UPDATE Products SET Stock = Stock - 2 WHERE ProductId = 5;
