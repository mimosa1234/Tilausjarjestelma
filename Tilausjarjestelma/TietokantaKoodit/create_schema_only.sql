
USE Tilausjarjestelma;

-- ============================
-- TABLE: Customers
-- ============================
CREATE TABLE Customers (
    id INT PRIMARY KEY IDENTITY(1,1),
    first_name NVARCHAR(50) NOT NULL,
    last_name NVARCHAR(50) NOT NULL,
    email NVARCHAR(100),
    phone NVARCHAR(20),
    address NVARCHAR(200)
);

-- ============================
-- TABLE: Categories
-- ============================
CREATE TABLE Categories (
    id INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(100) NOT NULL
);

-- ============================
-- TABLE: Products
-- ============================
CREATE TABLE Products (
    id INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(100) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    description NVARCHAR(300),
    category_id INT NOT NULL,
    stock INT NOT NULL DEFAULT 0,
    is_active BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_Product_Category
        FOREIGN KEY (category_id)
        REFERENCES Categories(id)
);

-- ============================
-- TABLE: Orders
-- ============================
CREATE TABLE Orders (
    id INT PRIMARY KEY IDENTITY(1,1),
    customer_id INT NOT NULL,
    order_date DATETIME NOT NULL DEFAULT GETDATE(),
    total_price DECIMAL(10,2) NOT NULL DEFAULT 0,

    -- Lisatyt kentat, joita WPF-koodi kayttaa
    customer_name NVARCHAR(120),
    customer_email NVARCHAR(120),
    customer_phone NVARCHAR(50),
    customer_address NVARCHAR(200),

    CONSTRAINT FK_Orders_Customers
        FOREIGN KEY (customer_id)
        REFERENCES Customers(id)
);

-- ============================
-- TABLE: OrderItems
-- ============================
CREATE TABLE OrderItems (
    id INT PRIMARY KEY IDENTITY(1,1),
    order_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(10,2) NOT NULL,
    total_price DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_OrderItems_Orders
        FOREIGN KEY (order_id)
        REFERENCES Orders(id)
        ON DELETE CASCADE,

    CONSTRAINT FK_OrderItems_Products
        FOREIGN KEY (product_id)
        REFERENCES Products(id)
);
