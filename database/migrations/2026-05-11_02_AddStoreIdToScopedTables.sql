-- ============================================================================
-- Migration: 2026-05-11_02_AddStoreIdToScopedTables
-- Purpose:   Phase 1b of multi-store conversion.
--            Adds `store_id INT NOT NULL DEFAULT 1` to every store-scoped
--            table, plus an FK to stores(Id) and an index on store_id.
--
-- Prerequisite: migration 2026-05-11_01_AddStoresTable must run first
--               (creates `stores` and seeds id=1 = Akiki).
--
-- Backfill strategy: existing rows take StoreId=1 via the DEFAULT, so all
-- legacy data is auto-assigned to the Akiki tenant. No data is lost.
--
-- Safety: no controller queries reference store_id yet, so this migration is
-- additive and non-breaking even on a running production system. The FK is
-- ON DELETE RESTRICT — stores cannot be deleted while they own data.
--
-- Performance note: each ALTER TABLE rewrites the table on MySQL 5.7+. For
-- tables with millions of rows (orders, line items siblings, variant audit)
-- run during a low-traffic window or use pt-online-schema-change.
-- ============================================================================

START TRANSACTION;

-- -----------------------------
-- 19 store-scoped tables
-- -----------------------------

ALTER TABLE `customers`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `Id`,
    ADD KEY `IX_customers_store_id` (`store_id`),
    ADD CONSTRAINT `FK_customers_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `employees`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `Id`,
    ADD KEY `IX_employees_store_id` (`store_id`),
    ADD CONSTRAINT `FK_employees_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `products`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `Id`,
    ADD KEY `IX_products_store_id` (`store_id`),
    ADD CONSTRAINT `FK_products_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `smart_collections`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `Id`,
    ADD KEY `IX_smart_collections_store_id` (`store_id`),
    ADD CONSTRAINT `FK_smart_collections_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `orders`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `orderid`,
    ADD KEY `IX_orders_store_id` (`store_id`),
    ADD CONSTRAINT `FK_orders_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `refund`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `id`,
    ADD KEY `IX_refund_store_id` (`store_id`),
    ADD CONSTRAINT `FK_refund_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `gift_card`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `id`,
    ADD KEY `IX_gift_card_store_id` (`store_id`),
    ADD CONSTRAINT `FK_gift_card_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `discount_code`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `id`,
    ADD KEY `IX_discount_code_store_id` (`store_id`),
    ADD CONSTRAINT `FK_discount_code_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `price_rules`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `id`,
    ADD KEY `IX_price_rules_store_id` (`store_id`),
    ADD CONSTRAINT `FK_price_rules_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `cart`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `Id`,
    ADD KEY `IX_cart_store_id` (`store_id`),
    ADD CONSTRAINT `FK_cart_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `badges`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `Id`,
    ADD KEY `IX_badges_store_id` (`store_id`),
    ADD CONSTRAINT `FK_badges_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `comments`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `Id`,
    ADD KEY `IX_comments_store_id` (`store_id`),
    ADD CONSTRAINT `FK_comments_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `timeline_events`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `Id`,
    ADD KEY `IX_timeline_events_store_id` (`store_id`),
    ADD CONSTRAINT `FK_timeline_events_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `variant_adjustment_history`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `id`,
    ADD KEY `IX_variant_adjustment_history_store_id` (`store_id`),
    ADD CONSTRAINT `FK_variant_adjustment_history_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `inventory_transaction_log`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `id`,
    ADD KEY `IX_inventory_transaction_log_store_id` (`store_id`),
    ADD CONSTRAINT `FK_inventory_transaction_log_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `supplier`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `id`,
    ADD KEY `IX_supplier_store_id` (`store_id`),
    ADD CONSTRAINT `FK_supplier_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `pmicustomers`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `id`,
    ADD KEY `IX_pmicustomers_store_id` (`store_id`),
    ADD CONSTRAINT `FK_pmicustomers_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `pmiorders`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `orderReference`,
    ADD KEY `IX_pmiorders_store_id` (`store_id`),
    ADD CONSTRAINT `FK_pmiorders_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

ALTER TABLE `pmiproducts`
    ADD COLUMN `store_id` INT NOT NULL DEFAULT 1 AFTER `id`,
    ADD KEY `IX_pmiproducts_store_id` (`store_id`),
    ADD CONSTRAINT `FK_pmiproducts_stores` FOREIGN KEY (`store_id`) REFERENCES `stores` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

COMMIT;

-- -----------------------------
-- Verification queries (run after COMMIT, optional)
-- -----------------------------
-- SELECT COUNT(*) AS akiki_rows_orders FROM orders WHERE store_id = 1;
-- SELECT COUNT(*) AS non_default_rows FROM orders WHERE store_id <> 1;  -- should be 0
-- SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
--   WHERE TABLE_SCHEMA = DATABASE() AND COLUMN_NAME = 'store_id'
--   ORDER BY TABLE_NAME;
