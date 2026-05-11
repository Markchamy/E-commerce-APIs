-- ============================================================================
-- Migration: 2026-05-11_01_AddStoresTable
-- Purpose:   Phase 1a of multi-store conversion.
--            Creates the `stores` table and seeds the default "Akiki" store
--            with id=1 so existing rows can later be backfilled to it.
-- Safe to run on existing production data: ADDITIVE ONLY (no schema changes
-- to other tables). No code path uses `store_id` yet — that's Phase 1b.
-- ============================================================================

START TRANSACTION;

CREATE TABLE IF NOT EXISTS `stores` (
    `Id`            INT          NOT NULL AUTO_INCREMENT,
    `Name`          VARCHAR(100) NOT NULL,
    `slug`          VARCHAR(63)  NOT NULL,
    `domain`        VARCHAR(255) NULL,
    `logo_url`      VARCHAR(255) NULL,
    `primary_color` VARCHAR(7)   NULL,
    `Currency`      VARCHAR(3)   NULL,
    `Locale`        VARCHAR(5)   NULL,
    `is_active`     TINYINT(1)   NOT NULL DEFAULT 1,
    `created_at`    DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `updated_at`    DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_Stores_Slug` (`slug`),
    KEY `IX_Stores_Domain` (`domain`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Seed the default store. id=1 is reserved for the legacy Akiki tenant so the
-- Phase 1b backfill (`UPDATE <table> SET store_id = 1`) is a no-op for them.
INSERT INTO `stores` (`Id`, `Name`, `slug`, `domain`, `Currency`, `Locale`, `is_active`)
VALUES (1, 'Akiki''s Cigars', 'akiki', NULL, 'USD', 'en-US', 1)
ON DUPLICATE KEY UPDATE `Name` = VALUES(`Name`);

COMMIT;
