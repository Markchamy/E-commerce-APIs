-- ============================================================================
-- Migration: 2026-05-12_03_AddRefreshTokensTable
-- Purpose:   Persistent refresh tokens for JWT rotation.
--            Hash-only storage so DB leak doesn't expose valid tokens.
-- ============================================================================

START TRANSACTION;

CREATE TABLE IF NOT EXISTS `refresh_tokens` (
    `Id`          BIGINT       NOT NULL AUTO_INCREMENT,
    `user_id`     BIGINT       NOT NULL,
    `token_hash`  VARCHAR(64)  NOT NULL,
    `expires_at`  DATETIME(6)  NOT NULL,
    `revoked_at`  DATETIME(6)  NULL,
    `created_at`  DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_refresh_tokens_token_hash` (`token_hash`),
    KEY `IX_refresh_tokens_user_id` (`user_id`),
    KEY `IX_refresh_tokens_expires_at` (`expires_at`),
    CONSTRAINT `FK_refresh_tokens_users` FOREIGN KEY (`user_id`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

COMMIT;
