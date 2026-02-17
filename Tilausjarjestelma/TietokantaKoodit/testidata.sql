USE Tilausjarjestelma;
GO

-- Customers
INSERT INTO Customers (first_name, last_name, email, phone, address) VALUES
('Matti', 'Meikalainen', 'matti@example.com', '0401231234', 'Esimerkkikatu 1'),
('Maija', 'Mallikas', 'maija@example.com', '0509879876', 'Testikuja 9');

-- Categories
INSERT INTO Categories (name) VALUES
('Elektroniikka'),
('Kodinkoneet'),
('Kirjat'),
('Lelut');

-- Products
INSERT INTO Products (name, price, description, category_id, stock) VALUES
('Kannettava tietokone', 999.99, '14" kevyt kannettava', 1, 10),
('Alypuhelin', 699.00, 'Uusin malli', 1, 25),
('Kahvinkeitin', 49.90, 'Perusmalli 12 kuppia', 2, 15),
('Polynimuri', 129.00, 'Tehokas HEPA-suodattimella', 2, 5),
('Fantasiakirja', 19.99, 'Seikkailu alkaa...', 3, 30),
('Nallepehmolelu', 12.50, 'Sopo ja pehmea', 4, 50);

-- Test order (optional)
INSERT INTO Orders (customer_id, customer_name, customer_email, customer_phone, customer_address)
SELECT id, first_name + ' ' + last_name, email, phone, address
FROM Customers WHERE first_name = 'Matti';

DECLARE @orderId INT = SCOPE_IDENTITY();

INSERT INTO OrderItems (order_id, product_id, quantity, unit_price, total_price)
VALUES
(@orderId, 1, 1, 999.99, 999.99),
(@orderId, 5, 2, 19.99, 39.98);

UPDATE Orders
SET total_price = (
    SELECT SUM(total_price) FROM OrderItems WHERE order_id = @orderId
)
WHERE id = @orderId;

-- paivitellaan stock manuaalisesti testitilaukselle
UPDATE Products SET stock = stock - 1 WHERE id = 1;
UPDATE Products SET stock = stock - 2 WHERE id = 5;
