-- =====================================================
-- AUTOMATED INDEX CREATION WITH ERROR HANDLING
-- =====================================================
-- This script creates a stored procedure that automatically
-- handles errors and creates indexes that work
-- =====================================================

DELIMITER //

DROP PROCEDURE IF EXISTS CreateSearchIndexes//

CREATE PROCEDURE CreateSearchIndexes()
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        -- Ignore errors and continue
        GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE, @errno = MYSQL_ERRNO, @text = MESSAGE_TEXT;
        SELECT CONCAT('Warning: ', @errno, ' - ', @text) AS WarningMessage;
    END;

    -- Products Table Indexes
    CREATE INDEX idx_products_title ON Products(Title);
    CREATE INDEX idx_products_vendor ON Products(Vendor);
    CREATE INDEX idx_products_product_type ON Products(product_type);
    CREATE INDEX idx_products_handle ON Products(handle);
    CREATE INDEX idx_products_status ON Products(Status);
    CREATE INDEX idx_products_created_at ON Products(created_at);
    CREATE INDEX idx_products_collection ON Products(product_collection);

    -- Variants Table Indexes
    CREATE INDEX idx_variants_sku ON Variants(sku);
    CREATE INDEX idx_variants_barcode ON Variants(barcode);
    CREATE INDEX idx_variants_title ON Variants(title);
    CREATE INDEX idx_variants_product_id ON Variants(product_id);

    -- Orders Table Indexes
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

    -- Collections Table Indexes
    CREATE INDEX idx_collections_title ON Collection(title);
    CREATE INDEX idx_collections_handle ON Collection(handle);
    CREATE INDEX idx_collections_updated_at ON Collection(updated_at);

    -- Users Table Indexes
    CREATE INDEX idx_users_first_name ON Users(first_name);
    CREATE INDEX idx_users_last_name ON Users(last_name);
    CREATE INDEX idx_users_email ON Users(email);
    CREATE INDEX idx_users_phone_number ON Users(phone_number);
    CREATE INDEX idx_users_role ON Users(role);

    -- Customers Table Indexes
    CREATE INDEX idx_customers_first_name ON Customers(first_name);
    CREATE INDEX idx_customers_last_name ON Customers(last_name);
    CREATE INDEX idx_customers_email ON Customers(email);

    -- Billing Address Table Indexes
    CREATE INDEX idx_billing_address_first_name ON billing_address(first_name);
    CREATE INDEX idx_billing_address_last_name ON billing_address(last_name);
    CREATE INDEX idx_billing_address_city ON billing_address(city);
    CREATE INDEX idx_billing_address_order_id ON billing_address(OrdersId);

    -- Composite Indexes
    CREATE INDEX idx_products_status_created ON Products(Status, created_at);
    CREATE INDEX idx_orders_status_created ON Orders(financial_status, created_at);

    SELECT 'Index creation completed - check warnings above for any skipped indexes' AS Result;
END//

DELIMITER ;

-- =====================================================
-- RUN THE PROCEDURE
-- =====================================================
CALL CreateSearchIndexes();

-- =====================================================
-- VERIFY CREATED INDEXES
-- =====================================================
SELECT
    TABLE_NAME,
    INDEX_NAME,
    COLUMN_NAME,
    SEQ_IN_INDEX
FROM
    INFORMATION_SCHEMA.STATISTICS
WHERE
    TABLE_SCHEMA = DATABASE()
    AND INDEX_NAME LIKE 'idx_%'
ORDER BY
    TABLE_NAME, INDEX_NAME, SEQ_IN_INDEX;

-- =====================================================
-- CLEANUP (Optional - removes the procedure after use)
-- =====================================================
-- DROP PROCEDURE IF EXISTS CreateSearchIndexes;
