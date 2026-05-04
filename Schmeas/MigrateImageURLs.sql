-- =============================================
-- Migrate Image URLs from Old EC2 to New EC2
-- =============================================
-- This script updates all product image URLs from the old EC2 IP (16.171.91.212)
-- to the new EC2 IP (52.73.44.227)
--
-- Run this script on your MySQL database after deploying to the new EC2 instance
-- =============================================

USE akikiscigar_website;

-- Start transaction for safety
START TRANSACTION;

-- Display current state before migration
SELECT
    'Before Migration' AS Status,
    COUNT(*) AS TotalImages,
    SUM(CASE WHEN src LIKE '%16.171.91.212%' THEN 1 ELSE 0 END) AS OldIPImages,
    SUM(CASE WHEN src LIKE '%52.73.44.227%' THEN 1 ELSE 0 END) AS NewIPImages
FROM product_images;

-- Update all image URLs from old IP to new IP
-- This handles both product images and variant images
UPDATE product_images
SET
    src = REPLACE(src, 'http://16.171.91.212', 'http://52.73.44.227'),
    updated_at = NOW()
WHERE
    src LIKE '%16.171.91.212%';

-- Display results after migration
SELECT
    'After Migration' AS Status,
    COUNT(*) AS TotalImages,
    SUM(CASE WHEN src LIKE '%16.171.91.212%' THEN 1 ELSE 0 END) AS OldIPImages,
    SUM(CASE WHEN src LIKE '%52.73.44.227%' THEN 1 ELSE 0 END) AS NewIPImages,
    ROW_COUNT() AS RowsUpdated
FROM product_images;

-- Show sample of updated URLs (first 10)
SELECT
    id,
    product_id,
    variant_id,
    src,
    updated_at
FROM product_images
WHERE src LIKE '%52.73.44.227%'
LIMIT 10;

-- Commit the changes if everything looks good
-- If you see any issues, run: ROLLBACK;
COMMIT;

-- =============================================
-- Verification Query
-- =============================================
-- Run this after committing to verify all URLs are updated
-- SELECT COUNT(*) AS RemainingOldURLs
-- FROM product_images
-- WHERE src LIKE '%16.171.91.212%';
--
-- Result should be 0

-- =============================================
-- Rollback Instructions (if needed)
-- =============================================
-- If you need to rollback before committing, run:
-- ROLLBACK;
--
-- If you already committed and need to revert:
-- UPDATE product_images
-- SET src = REPLACE(src, 'http://52.73.44.227', 'http://16.171.91.212')
-- WHERE src LIKE '%52.73.44.227%';
