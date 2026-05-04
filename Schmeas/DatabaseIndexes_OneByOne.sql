-- =====================================================
-- Database Indexes for Global Search Performance
-- ONE-BY-ONE VERSION - Run each statement individually
-- =====================================================
-- INSTRUCTIONS:
-- 1. Run each CREATE INDEX statement ONE AT A TIME
-- 2. If you get an error on a specific index, SKIP IT and move to the next one
-- 3. Common errors to ignore:
--    - "Duplicate key name" = Index already exists (SKIP IT)
--    - "Incorrect prefix key" = Column type doesn't need index (SKIP IT)
-- 4. The goal is to create as many indexes as possible
-- =====================================================

-- =============== PRODUCTS TABLE ===============
-- Run these ONE BY ONE, skip any that error

CREATE INDEX idx_products_title ON Products(Title);

CREATE INDEX idx_products_vendor ON Products(Vendor);

CREATE INDEX idx_products_product_type ON Products(product_type);

CREATE INDEX idx_products_handle ON Products(handle);

CREATE INDEX idx_products_status ON Products(Status);

CREATE INDEX idx_products_created_at ON Products(created_at);

-- For Tags and body_html - if these error, try without them (skip to next section)
CREATE INDEX idx_products_tags ON Products(Tags(255));

CREATE INDEX idx_products_body_html ON Products(body_html(255));

CREATE INDEX idx_products_collection ON Products(product_collection);

-- Composite index
CREATE INDEX idx_products_status_created ON Products(Status, created_at);

-- =============== VARIANTS TABLE ===============

CREATE INDEX idx_variants_sku ON Variants(sku);

CREATE INDEX idx_variants_barcode ON Variants(barcode);

CREATE INDEX idx_variants_title ON Variants(title);

CREATE INDEX idx_variants_product_id ON Variants(product_id);

-- =============== ORDERS TABLE ===============

CREATE INDEX idx_orders_name ON Orders(name);

CREATE INDEX idx_orders_email ON Orders(email);

CREATE INDEX idx_orders_contact_email ON Orders(contact_email);

CREATE INDEX idx_orders_phone ON Orders(phone);

CREATE INDEX idx_orders_order_number ON Orders(order_number);

CREATE INDEX idx_orders_confirmation_number ON Orders(confirmation_number);

CREATE INDEX idx_orders_financial_status ON Orders(financial_status);

CREATE INDEX idx_orders_fulfillment_status ON Orders(fulfillment_status);

CREATE INDEX idx_orders_created_at ON Orders(created_at);

CREATE INDEX idx_orders_customer_id ON Orders(CustomerId);

-- For tags - if this errors, skip it
CREATE INDEX idx_orders_tags ON Orders(tags(255));

-- Composite index
CREATE INDEX idx_orders_status_created ON Orders(financial_status, created_at);

-- =============== COLLECTIONS TABLE ===============

CREATE INDEX idx_collections_title ON Collection(title);

CREATE INDEX idx_collections_handle ON Collection(handle);

CREATE INDEX idx_collections_updated_at ON Collection(updated_at);

-- For body_html - if this errors, skip it
CREATE INDEX idx_collections_body_html ON Collection(body_html(255));

-- =============== USERS TABLE ===============

CREATE INDEX idx_users_first_name ON Users(first_name);

CREATE INDEX idx_users_last_name ON Users(last_name);

CREATE INDEX idx_users_email ON Users(email);

CREATE INDEX idx_users_phone_number ON Users(phone_number);

CREATE INDEX idx_users_role ON Users(role);

-- =============== CUSTOMERS TABLE ===============

CREATE INDEX idx_customers_first_name ON Customers(first_name);

CREATE INDEX idx_customers_last_name ON Customers(last_name);

CREATE INDEX idx_customers_email ON Customers(email);

-- =============== BILLING ADDRESS TABLE ===============

CREATE INDEX idx_billing_address_first_name ON billing_address(first_name);

CREATE INDEX idx_billing_address_last_name ON billing_address(last_name);

CREATE INDEX idx_billing_address_city ON billing_address(city);

CREATE INDEX idx_billing_address_order_id ON billing_address(OrdersId);

-- =====================================================
-- VERIFICATION - Run this to see what was created
-- =====================================================

SELECT
    TABLE_NAME,
    INDEX_NAME,
    COLUMN_NAME
FROM
    INFORMATION_SCHEMA.STATISTICS
WHERE
    TABLE_SCHEMA = DATABASE()
    AND INDEX_NAME LIKE 'idx_%'
ORDER BY
    TABLE_NAME, INDEX_NAME;

-- =====================================================
-- You should see a list of all successfully created indexes above
-- Don't worry if some failed - even partial indexing will help!
-- =====================================================
