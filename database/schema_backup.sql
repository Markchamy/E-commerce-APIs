/*M!999999\- enable the sandbox mode */ 
-- MariaDB dump 10.19  Distrib 10.5.29-MariaDB, for Linux (x86_64)
--
-- Host: localhost    Database: akikiscigar_website
-- ------------------------------------------------------
-- Server version	10.5.29-MariaDB

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `PmiCustomers`
--

DROP TABLE IF EXISTS `PmiCustomers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `PmiCustomers` (
  `id` varchar(45) NOT NULL,
  `name` varchar(45) DEFAULT NULL,
  `lastName` varchar(45) DEFAULT NULL,
  `address` varchar(200) DEFAULT NULL,
  `phone` varchar(45) DEFAULT NULL,
  `email` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PmiErrors`
--

DROP TABLE IF EXISTS `PmiErrors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `PmiErrors` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `error` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PmiOrderedMachines`
--

DROP TABLE IF EXISTS `PmiOrderedMachines`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `PmiOrderedMachines` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `orderId` varchar(45) DEFAULT NULL,
  `productId` varchar(45) DEFAULT NULL,
  `quantity` int(11) DEFAULT NULL,
  `price` int(11) DEFAULT NULL,
  `serialNum` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PmiOrderedProduct`
--

DROP TABLE IF EXISTS `PmiOrderedProduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `PmiOrderedProduct` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `orderId` varchar(45) DEFAULT NULL,
  `productId` varchar(45) DEFAULT NULL,
  `quantity` int(11) DEFAULT NULL,
  `price` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PmiOrders`
--

DROP TABLE IF EXISTS `PmiOrders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `PmiOrders` (
  `orderReference` varchar(50) NOT NULL,
  `orderNumber` varchar(45) DEFAULT NULL,
  `dateDelivered` varchar(45) DEFAULT NULL,
  `customerId` varchar(45) DEFAULT NULL,
  `dateCreated` varchar(45) DEFAULT NULL,
  `errorId` int(11) DEFAULT NULL,
  `anonymous` tinyint(4) DEFAULT NULL,
  PRIMARY KEY (`orderReference`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PmiProducts`
--

DROP TABLE IF EXISTS `PmiProducts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `PmiProducts` (
  `id` varchar(55) NOT NULL,
  `name` varchar(255) DEFAULT NULL,
  `price` double DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `addresses`
--

DROP TABLE IF EXISTS `addresses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `addresses` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `customer_id` bigint(20) NOT NULL,
  `first_name` varchar(55) DEFAULT NULL,
  `last_name` varchar(55) DEFAULT NULL,
  `company` varchar(55) DEFAULT NULL,
  `address1` varchar(255) DEFAULT NULL,
  `address2` varchar(255) DEFAULT NULL,
  `city` varchar(55) DEFAULT NULL,
  `province` varchar(55) DEFAULT NULL,
  `country` varchar(55) DEFAULT NULL,
  `zip` varchar(55) DEFAULT NULL,
  `phone` varchar(55) DEFAULT NULL,
  `name` varchar(55) DEFAULT NULL,
  `province_code` varchar(45) DEFAULT NULL,
  `country_code` varchar(55) DEFAULT NULL,
  `country_name` varchar(55) DEFAULT NULL,
  `default_address` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`id`),
  FULLTEXT KEY `first_name` (`first_name`,`last_name`,`city`,`country`)
) ENGINE=InnoDB AUTO_INCREMENT=9686540058875 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `badges`
--

DROP TABLE IF EXISTS `badges`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `badges` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `body_html` longtext DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `base_fee`
--

DROP TABLE IF EXISTS `base_fee`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `base_fee` (
  `shipping_method_uuid` varchar(255) DEFAULT NULL,
  `fee_from` decimal(10,0) DEFAULT NULL,
  `fee_to` decimal(10,0) DEFAULT NULL,
  `amount` decimal(10,0) DEFAULT NULL,
  `amount_formatted` varchar(255) DEFAULT NULL,
  KEY `shipping_method_uuid` (`shipping_method_uuid`),
  CONSTRAINT `base_fee_ibfk_1` FOREIGN KEY (`shipping_method_uuid`) REFERENCES `shipping_methods` (`uuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `billing_address`
--

DROP TABLE IF EXISTS `billing_address`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `billing_address` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `first_name` varchar(255) DEFAULT NULL,
  `address1` varchar(255) DEFAULT NULL,
  `phone` varchar(255) DEFAULT NULL,
  `city` varchar(255) DEFAULT NULL,
  `zip` varchar(55) DEFAULT NULL,
  `province` varchar(255) DEFAULT NULL,
  `country` varchar(255) DEFAULT NULL,
  `last_name` varchar(255) DEFAULT NULL,
  `address2` varchar(255) DEFAULT NULL,
  `company` varchar(55) DEFAULT NULL,
  `latitude` float DEFAULT NULL,
  `longitude` float DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  `country_code` varchar(45) DEFAULT NULL,
  `province_code` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_billing_address_first_name` (`first_name`),
  KEY `idx_billing_address_last_name` (`last_name`),
  KEY `idx_billing_address_city` (`city`),
  KEY `idx_billing_address_order_id` (`orderid`)
) ENGINE=InnoDB AUTO_INCREMENT=92580 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `carriers`
--

DROP TABLE IF EXISTS `carriers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `carriers` (
  `uuid` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `avatar` text DEFAULT NULL,
  PRIMARY KEY (`uuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cart`
--

DROP TABLE IF EXISTS `cart`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `cart` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `product_id` bigint(20) NOT NULL,
  `quantity` bigint(20) DEFAULT NULL,
  `price` varchar(255) DEFAULT NULL,
  `user_id` bigint(20) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=80 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cities`
--

DROP TABLE IF EXISTS `cities`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `cities` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `district_id` bigint(20) DEFAULT NULL,
  `cities` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=707 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `client_details`
--

DROP TABLE IF EXISTS `client_details`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `client_details` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `accept_language` varchar(10) DEFAULT NULL,
  `browser_height` int(11) DEFAULT NULL,
  `browser_width` int(11) DEFAULT NULL,
  `browser_ip` varchar(45) DEFAULT NULL,
  `session_hash` varchar(64) DEFAULT NULL,
  `user_agent` text DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=34692 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `collection_filter`
--

DROP TABLE IF EXISTS `collection_filter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `collection_filter` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `collection_images`
--

DROP TABLE IF EXISTS `collection_images`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `collection_images` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `smart_collection_id` bigint(20) NOT NULL,
  `created_at` datetime NOT NULL,
  `alt` varchar(255) DEFAULT NULL,
  `width` int(11) NOT NULL,
  `height` int(11) NOT NULL,
  `src` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `smart_collection_id` (`smart_collection_id`),
  CONSTRAINT `collection_images_ibfk_1` FOREIGN KEY (`smart_collection_id`) REFERENCES `smart_collections` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `collection_products`
--

DROP TABLE IF EXISTS `collection_products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `collection_products` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `smart_collection_id` bigint(20) NOT NULL,
  `product_id` bigint(20) NOT NULL,
  `position` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_collection_product` (`smart_collection_id`,`product_id`),
  KEY `product_id` (`product_id`),
  CONSTRAINT `collection_products_ibfk_1` FOREIGN KEY (`smart_collection_id`) REFERENCES `smart_collections` (`id`) ON DELETE CASCADE,
  CONSTRAINT `collection_products_ibfk_2` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `collection_rules`
--

DROP TABLE IF EXISTS `collection_rules`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `collection_rules` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `smart_collection_id` bigint(20) NOT NULL,
  `column_name` varchar(255) NOT NULL,
  `relation` varchar(50) NOT NULL,
  `condition_text` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `smart_collection_id` (`smart_collection_id`),
  CONSTRAINT `collection_rules_ibfk_1` FOREIGN KEY (`smart_collection_id`) REFERENCES `smart_collections` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1533 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `collection_sort_by`
--

DROP TABLE IF EXISTS `collection_sort_by`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `collection_sort_by` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `comments`
--

DROP TABLE IF EXISTS `comments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `comments` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `OrderId` bigint(20) NOT NULL,
  `UserId` bigint(20) NOT NULL,
  `Content` varchar(2000) NOT NULL,
  `Mentions` varchar(500) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Comments_OrderId` (`OrderId`),
  KEY `IX_Comments_UserId` (`UserId`),
  CONSTRAINT `FK_Comments_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`orderid`) ON DELETE CASCADE,
  CONSTRAINT `FK_Comments_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `conversations`
--

DROP TABLE IF EXISTS `conversations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `conversations` (
  `ConversationId` char(36) NOT NULL,
  `CustomerId` varchar(255) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `ClosedAt` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`ConversationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `current_subtotal_price`
--

DROP TABLE IF EXISTS `current_subtotal_price`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `current_subtotal_price` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `shop_amount` varchar(55) DEFAULT NULL,
  `shop_currency_code` varchar(55) DEFAULT NULL,
  `presentment_amount` varchar(55) DEFAULT NULL,
  `presentment_currency` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=34639 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `current_total_price_set`
--

DROP TABLE IF EXISTS `current_total_price_set`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `current_total_price_set` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `shop_amount` varchar(55) DEFAULT NULL,
  `shop_currency_code` varchar(55) DEFAULT NULL,
  `presentment_amount` varchar(55) DEFAULT NULL,
  `presentment_currency` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `orderid` (`orderid`),
  CONSTRAINT `current_total_price_set_ibfk_1` FOREIGN KEY (`orderid`) REFERENCES `orders` (`orderid`)
) ENGINE=InnoDB AUTO_INCREMENT=34644 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `customer_sort_by`
--

DROP TABLE IF EXISTS `customer_sort_by`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `customer_sort_by` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `customers`
--

DROP TABLE IF EXISTS `customers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `customers` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `company` varchar(45) DEFAULT NULL,
  `address` varchar(255) DEFAULT NULL,
  `apartment` varchar(55) DEFAULT NULL,
  `city` varchar(55) DEFAULT NULL,
  `country` varchar(55) DEFAULT NULL,
  `email_sms_opt_in` tinyint(1) DEFAULT NULL,
  `news_letter` tinyint(1) DEFAULT NULL,
  `userId` bigint(20) DEFAULT NULL,
  `created_at` varchar(55) DEFAULT NULL,
  `updated_at` varchar(55) DEFAULT NULL,
  `orders_count` int(11) DEFAULT NULL,
  `state` tinyint(1) DEFAULT NULL,
  `total_spent` double DEFAULT NULL,
  `last_order_id` bigint(20) DEFAULT NULL,
  `note` varchar(255) DEFAULT NULL,
  `tags` varchar(55) DEFAULT NULL,
  `last_order_name` varchar(55) DEFAULT NULL,
  `currency` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`),
  FULLTEXT KEY `company` (`company`,`city`,`note`,`tags`,`currency`)
) ENGINE=InnoDB AUTO_INCREMENT=8374953738491 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `discount_applications`
--

DROP TABLE IF EXISTS `discount_applications`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `discount_applications` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `target_type` varchar(255) DEFAULT NULL,
  `type` varchar(55) DEFAULT NULL,
  `value` varchar(55) DEFAULT NULL,
  `value_type` varchar(255) DEFAULT NULL,
  `allocation_method` varchar(55) DEFAULT NULL,
  `target_selection` varchar(55) DEFAULT NULL,
  `code` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1119 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `discount_code`
--

DROP TABLE IF EXISTS `discount_code`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `discount_code` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `price_rule_id` bigint(20) DEFAULT NULL,
  `code` varchar(255) DEFAULT NULL,
  `usage_count` bigint(20) DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  `updated_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `discount_codes`
--

DROP TABLE IF EXISTS `discount_codes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `discount_codes` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `code` varchar(55) DEFAULT NULL,
  `amount` varchar(55) DEFAULT NULL,
  `type` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=911 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `district`
--

DROP TABLE IF EXISTS `district`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `district` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `districts` varchar(255) DEFAULT NULL,
  `delivery_price` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `employees`
--

DROP TABLE IF EXISTS `employees`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `employees` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `access_control` varchar(55) DEFAULT NULL,
  `userId` bigint(20) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `entitlement_purchase`
--

DROP TABLE IF EXISTS `entitlement_purchase`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `entitlement_purchase` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `price_rule_id` bigint(20) DEFAULT NULL,
  `prerequisite_amount` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `entitlement_quantity_ratio`
--

DROP TABLE IF EXISTS `entitlement_quantity_ratio`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `entitlement_quantity_ratio` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `price_rule_id` bigint(20) DEFAULT NULL,
  `prerequisite_quantity` int(11) DEFAULT NULL,
  `entitled_quantity` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=140 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fulfillments`
--

DROP TABLE IF EXISTS `fulfillments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `fulfillments` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  `location_id` bigint(20) NOT NULL,
  `name` varchar(55) DEFAULT NULL,
  `service` varchar(45) DEFAULT NULL,
  `shipment_status` varchar(55) DEFAULT NULL,
  `status` varchar(55) DEFAULT NULL,
  `tracking_company` varchar(255) DEFAULT NULL,
  `tracking_number` varchar(255) DEFAULT NULL,
  `tracking_url` varchar(255) DEFAULT NULL,
  `updated_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=35204 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `gift_card`
--

DROP TABLE IF EXISTS `gift_card`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `gift_card` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `balance` varchar(55) DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  `updated_at` datetime DEFAULT NULL,
  `currency` varchar(45) DEFAULT NULL,
  `initial_value` varchar(45) DEFAULT NULL,
  `disabled_at` datetime DEFAULT NULL,
  `line_item_id` bigint(20) DEFAULT NULL,
  `user_id` bigint(20) DEFAULT NULL,
  `customer_id` bigint(20) DEFAULT NULL,
  `note` varchar(55) DEFAULT NULL,
  `expires_on` datetime DEFAULT NULL,
  `template_suffix` varchar(255) DEFAULT NULL,
  `last_characters` varchar(55) DEFAULT NULL,
  `order_id` bigint(20) DEFAULT NULL,
  `code` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `giftcard_filter`
--

DROP TABLE IF EXISTS `giftcard_filter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `giftcard_filter` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `giftcard_sort_by`
--

DROP TABLE IF EXISTS `giftcard_sort_by`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `giftcard_sort_by` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `inventory_sort_by`
--

DROP TABLE IF EXISTS `inventory_sort_by`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `inventory_sort_by` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `inventory_transaction_log`
--

DROP TABLE IF EXISTS `inventory_transaction_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `inventory_transaction_log` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `variant_id` bigint(20) NOT NULL,
  `transaction_type` varchar(50) NOT NULL COMMENT 'BRAINS_SYNC, ORDER_RESERVE, FULFILLMENT, CANCELLATION, ADJUSTMENT',
  `quantity_change` int(11) NOT NULL COMMENT 'Positive or negative change',
  `inventory_before` int(11) NOT NULL,
  `inventory_after` int(11) NOT NULL,
  `reserved_before` int(11) NOT NULL,
  `reserved_after` int(11) NOT NULL,
  `reason` varchar(500) DEFAULT NULL COMMENT 'Human-readable reason',
  `order_id` bigint(20) DEFAULT NULL COMMENT 'Link to orders table',
  `line_item_id` bigint(20) DEFAULT NULL COMMENT 'Link to line_items table',
  `performed_by` varchar(100) NOT NULL COMMENT 'BrainsSyncService, System, Admin, User ID',
  `created_at` datetime NOT NULL DEFAULT current_timestamp(),
  `additional_data` text DEFAULT NULL COMMENT 'JSON for extra context',
  PRIMARY KEY (`id`),
  KEY `idx_variant_id` (`variant_id`),
  KEY `idx_transaction_type` (`transaction_type`),
  KEY `idx_order_id` (`order_id`),
  KEY `idx_created_at` (`created_at`),
  CONSTRAINT `fk_inventory_log_order` FOREIGN KEY (`order_id`) REFERENCES `orders` (`orderid`) ON DELETE SET NULL,
  CONSTRAINT `fk_inventory_log_variant` FOREIGN KEY (`variant_id`) REFERENCES `variants` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=8916882 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Tracks all inventory changes for audit and reconciliation';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `lineitems`
--

DROP TABLE IF EXISTS `lineitems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `lineitems` (
  `lineItemId` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) NOT NULL,
  `current_quantity` int(11) DEFAULT NULL,
  `fulfillable_quantity` int(11) DEFAULT NULL,
  `fulfillment_service` varchar(55) DEFAULT NULL,
  `fulfillment_status` varchar(55) DEFAULT NULL,
  `gift_card` tinyint(1) DEFAULT NULL,
  `grams` int(11) DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  `price` varchar(255) DEFAULT NULL,
  `product_exists` tinyint(1) DEFAULT NULL,
  `product_id` bigint(20) NOT NULL,
  `quantity` int(11) DEFAULT NULL,
  `requires_shipping` tinyint(1) DEFAULT NULL,
  `sku` varchar(55) DEFAULT NULL,
  `taxable` tinyint(1) DEFAULT NULL,
  `title` varchar(255) DEFAULT NULL,
  `total_discount` double DEFAULT NULL,
  `variant_id` bigint(20) NOT NULL,
  `variant_inventory_management` varchar(55) DEFAULT NULL,
  `variant_title` varchar(55) DEFAULT NULL,
  `vendor` varchar(55) DEFAULT NULL,
  `refund_line_id` bigint(20) DEFAULT NULL,
  `product_fulfilled` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`lineItemId`),
  KEY `orderid` (`orderid`),
  CONSTRAINT `lineitems_ibfk_1` FOREIGN KEY (`orderid`) REFERENCES `orders` (`orderid`)
) ENGINE=InnoDB AUTO_INCREMENT=15336128479581 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `messages`
--

DROP TABLE IF EXISTS `messages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `messages` (
  `MessageId` char(36) NOT NULL,
  `ConversationId` char(36) NOT NULL,
  `SenderType` varchar(50) NOT NULL,
  `Content` text NOT NULL,
  `Timestamp` datetime(6) NOT NULL,
  `IsRead` tinyint(1) NOT NULL,
  PRIMARY KEY (`MessageId`),
  KEY `ConversationId` (`ConversationId`),
  CONSTRAINT `messages_ibfk_1` FOREIGN KEY (`ConversationId`) REFERENCES `conversations` (`ConversationId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `note_attributes`
--

DROP TABLE IF EXISTS `note_attributes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `note_attributes` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `name` varchar(55) DEFAULT NULL,
  `value` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=52060 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `options`
--

DROP TABLE IF EXISTS `options`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `options` (
  `id` bigint(20) NOT NULL,
  `product_id` bigint(20) NOT NULL,
  `name` varchar(255) DEFAULT NULL,
  `position` int(11) DEFAULT NULL,
  `product_values` text DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `options_ibfk_1` (`product_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `order_filter`
--

DROP TABLE IF EXISTS `order_filter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `order_filter` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `order_sort_by`
--

DROP TABLE IF EXISTS `order_sort_by`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `order_sort_by` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `orders`
--

DROP TABLE IF EXISTS `orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `orders` (
  `orderid` bigint(20) NOT NULL,
  `app_id` bigint(20) NOT NULL,
  `browser_ip` varchar(55) DEFAULT NULL,
  `buyer_accepts_marketing` tinyint(1) DEFAULT NULL,
  `cancel_reason` varchar(255) DEFAULT NULL,
  `cancelled_at` datetime DEFAULT NULL,
  `cart_token` varchar(55) DEFAULT NULL,
  `checkout_id` bigint(20) NOT NULL,
  `checkout_token` varchar(55) DEFAULT NULL,
  `closed_at` datetime DEFAULT NULL,
  `confirmation_number` varchar(55) DEFAULT NULL,
  `confirmed` tinyint(1) DEFAULT NULL,
  `contact_email` varchar(255) DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  `currency` varchar(55) DEFAULT NULL,
  `current_subtotal_price` double DEFAULT NULL,
  `current_total_additional_fees_set` double DEFAULT NULL,
  `current_total_discounts` double DEFAULT NULL,
  `current_total_duties_set` double DEFAULT NULL,
  `current_total_price` double DEFAULT NULL,
  `customer_local` varchar(55) DEFAULT NULL,
  `device_id` bigint(20) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  `estimated_taxes` tinyint(1) DEFAULT NULL,
  `financial_status` varchar(55) DEFAULT NULL,
  `fulfillment_status` varchar(55) DEFAULT NULL,
  `landing_site` varchar(55) DEFAULT NULL,
  `landing_site_ref` varchar(55) DEFAULT NULL,
  `location_id` bigint(20) DEFAULT NULL,
  `merchant_of_record_app_id` varchar(55) DEFAULT NULL,
  `name` varchar(55) DEFAULT NULL,
  `note` varchar(255) DEFAULT NULL,
  `number` int(11) DEFAULT NULL,
  `order_number` bigint(20) DEFAULT NULL,
  `order_status` varchar(255) DEFAULT NULL,
  `original_total_additional_fees_set` varchar(55) DEFAULT NULL,
  `original_total_duties_set` varchar(55) DEFAULT NULL,
  `payment_gatewaynames` varchar(255) DEFAULT NULL,
  `phone` varchar(55) DEFAULT NULL,
  `po_number` varchar(55) DEFAULT NULL,
  `presentment_currency` varchar(55) DEFAULT NULL,
  `processed_at` datetime DEFAULT NULL,
  `reference` varchar(55) DEFAULT NULL,
  `referring_site` varchar(55) DEFAULT NULL,
  `source_identifier` varchar(55) DEFAULT NULL,
  `source_name` varchar(55) DEFAULT NULL,
  `source_url` varchar(55) DEFAULT NULL,
  `subtotal_price` double DEFAULT NULL,
  `tags` varchar(255) DEFAULT NULL,
  `tax_exempt` tinyint(1) DEFAULT NULL,
  `test` tinyint(1) DEFAULT NULL,
  `token` varchar(255) DEFAULT NULL,
  `total_discounts` double DEFAULT NULL,
  `total_outstanding` double DEFAULT NULL,
  `total_price` double DEFAULT NULL,
  `total_taxe` double DEFAULT NULL,
  `total_tip_received` double DEFAULT NULL,
  `total_weight` int(11) DEFAULT NULL,
  `updated_at` datetime DEFAULT NULL,
  `user_id` bigint(20) DEFAULT NULL,
  `payment_terms` varchar(55) DEFAULT NULL,
  `current_total_tax` double DEFAULT NULL,
  `taxes_included` tinyint(1) DEFAULT NULL,
  `total_line_items_price` double DEFAULT NULL,
  PRIMARY KEY (`orderid`),
  KEY `idx_orders_name` (`name`),
  KEY `idx_orders_email` (`email`),
  KEY `idx_orders_contact_email` (`contact_email`),
  KEY `idx_orders_phone` (`phone`),
  KEY `idx_orders_order_number` (`order_number`),
  KEY `idx_orders_confirmation_number` (`confirmation_number`),
  KEY `idx_orders_financial_status` (`financial_status`),
  KEY `idx_orders_fulfillment_status` (`fulfillment_status`),
  KEY `idx_orders_created_at` (`created_at`),
  KEY `idx_orders_customer_id` (`user_id`),
  KEY `idx_orders_tags` (`tags`),
  KEY `idx_orders_status_created` (`financial_status`,`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `permissions`
--

DROP TABLE IF EXISTS `permissions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `permissions` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `category` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`,`category`)
) ENGINE=InnoDB AUTO_INCREMENT=75 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pmicustomers`
--

DROP TABLE IF EXISTS `pmicustomers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `pmicustomers` (
  `id` varchar(45) NOT NULL,
  `name` varchar(45) DEFAULT NULL,
  `lastName` varchar(45) DEFAULT NULL,
  `address` varchar(200) DEFAULT NULL,
  `phone` varchar(45) DEFAULT NULL,
  `email` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pmierrors`
--

DROP TABLE IF EXISTS `pmierrors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `pmierrors` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `error` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=34 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pmiorderedmachines`
--

DROP TABLE IF EXISTS `pmiorderedmachines`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `pmiorderedmachines` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `orderId` varchar(45) DEFAULT NULL,
  `productId` varchar(45) DEFAULT NULL,
  `quantity` int(11) DEFAULT NULL,
  `price` int(11) DEFAULT NULL,
  `serialNum` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pmiorderedproduct`
--

DROP TABLE IF EXISTS `pmiorderedproduct`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `pmiorderedproduct` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `orderId` varchar(45) DEFAULT NULL,
  `productId` varchar(45) DEFAULT NULL,
  `quantity` int(11) DEFAULT NULL,
  `price` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=17117 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pmiorders`
--

DROP TABLE IF EXISTS `pmiorders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `pmiorders` (
  `orderReference` varchar(50) NOT NULL,
  `orderNumber` varchar(45) DEFAULT NULL,
  `dateDelivered` varchar(45) DEFAULT NULL,
  `customerId` varchar(45) DEFAULT NULL,
  `dateCreated` varchar(45) DEFAULT NULL,
  `errorId` int(11) DEFAULT NULL,
  `anonymous` tinyint(4) DEFAULT NULL,
  PRIMARY KEY (`orderReference`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pmiproducts`
--

DROP TABLE IF EXISTS `pmiproducts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `pmiproducts` (
  `id` varchar(55) NOT NULL,
  `name` varchar(255) DEFAULT NULL,
  `price` double DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `price_rules`
--

DROP TABLE IF EXISTS `price_rules`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `price_rules` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `value_type` varchar(255) DEFAULT NULL,
  `value` varchar(255) DEFAULT NULL,
  `customer_selection` varchar(255) DEFAULT NULL,
  `target_type` varchar(255) DEFAULT NULL,
  `target_selection` varchar(255) DEFAULT NULL,
  `allocation_method` varchar(255) DEFAULT NULL,
  `allocation_limit` int(11) DEFAULT NULL,
  `once_per_customer` tinyint(1) DEFAULT NULL,
  `usage_limit` int(11) DEFAULT NULL,
  `starts_at` datetime DEFAULT NULL,
  `ends_at` datetime DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  `updated_at` datetime DEFAULT NULL,
  `entitled_product_ids` text DEFAULT NULL,
  `entitled_variant_ids` text DEFAULT NULL,
  `entitled_collection_ids` text DEFAULT NULL,
  `entitled_country_ids` text DEFAULT NULL,
  `prerequisite_product_ids` text DEFAULT NULL,
  `prerequisite_variant_ids` text DEFAULT NULL,
  `prerequisite_collection_ids` text DEFAULT NULL,
  `customer_segment_prerequisite_ids` text DEFAULT NULL,
  `prerequisite_customer_ids` text DEFAULT NULL,
  `prerequisite_subtotal_range` text DEFAULT NULL,
  `prerequisite_quantity_range` text DEFAULT NULL,
  `prerequisite_shipping_price_range` text DEFAULT NULL,
  `title` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `price_set`
--

DROP TABLE IF EXISTS `price_set`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `price_set` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `shop_amount` varchar(55) DEFAULT NULL,
  `shop_currency_code` varchar(55) DEFAULT NULL,
  `presentment_amount` varchar(55) DEFAULT NULL,
  `presentment_currency` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `orderid` (`orderid`),
  CONSTRAINT `price_set_ibfk_1` FOREIGN KEY (`orderid`) REFERENCES `orders` (`orderid`)
) ENGINE=InnoDB AUTO_INCREMENT=34639 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `product_filter`
--

DROP TABLE IF EXISTS `product_filter`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `product_filter` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `product_sort_by`
--

DROP TABLE IF EXISTS `product_sort_by`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `product_sort_by` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `productimages`
--

DROP TABLE IF EXISTS `productimages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `productimages` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `product_id` bigint(20) NOT NULL,
  `width` int(11) DEFAULT NULL,
  `height` int(11) DEFAULT NULL,
  `position` int(11) DEFAULT NULL,
  `alt` text DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  `updated_at` datetime DEFAULT NULL,
  `src` longtext DEFAULT NULL,
  `variantId` bigint(20) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `product_id` (`product_id`)
) ENGINE=InnoDB AUTO_INCREMENT=10657 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `products`
--

DROP TABLE IF EXISTS `products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `products` (
  `id` bigint(20) NOT NULL,
  `title` varchar(255) DEFAULT NULL,
  `body_html` text DEFAULT NULL,
  `vendor` varchar(255) DEFAULT NULL,
  `product_type` varchar(255) DEFAULT NULL,
  `created_at` date DEFAULT NULL,
  `updated_at` date DEFAULT NULL,
  `published_at` date DEFAULT NULL,
  `handle` varchar(255) DEFAULT NULL,
  `template_suffix` varchar(255) DEFAULT NULL,
  `published_scope` varchar(255) DEFAULT NULL,
  `tags` text DEFAULT NULL,
  `status` varchar(50) DEFAULT NULL,
  `product_collection` varchar(255) DEFAULT NULL,
  `smart_collection_id` int(11) DEFAULT NULL,
  `product_status` varchar(50) DEFAULT NULL,
  `currency` varchar(55) DEFAULT NULL,
  `product_url` varchar(255) DEFAULT NULL,
  `max_purchase` bigint(20) DEFAULT NULL,
  `track_quantity` tinyint(1) DEFAULT NULL,
  `physical_product` tinyint(1) DEFAULT NULL,
  `continue_selling` tinyint(1) DEFAULT NULL,
  `New` tinyint(1) DEFAULT NULL,
  `badge_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_products_title` (`title`),
  KEY `idx_products_vendor` (`vendor`),
  KEY `idx_products_product_type` (`product_type`),
  KEY `idx_products_handle` (`handle`),
  KEY `idx_products_status` (`status`),
  KEY `idx_products_created_at` (`created_at`),
  KEY `idx_products_tags` (`tags`(255)),
  KEY `idx_products_body_html` (`body_html`(255)),
  KEY `idx_products_collection` (`product_collection`),
  KEY `idx_products_status_created` (`status`,`created_at`),
  KEY `IX_Products_ProductStatus` (`product_status`),
  KEY `IX_Products_UpdatedAt` (`updated_at`),
  KEY `IX_Products_Tags` (`tags`(100)),
  KEY `IX_Products_Status_Created` (`product_status`,`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `recommended_products`
--

DROP TABLE IF EXISTS `recommended_products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `recommended_products` (
  `id` bigint(20) DEFAULT NULL,
  `title` text DEFAULT NULL,
  `collection` text DEFAULT NULL,
  `vendor` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `refund`
--

DROP TABLE IF EXISTS `refund`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `refund` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) NOT NULL,
  `created_at` date DEFAULT NULL,
  `note` varchar(255) DEFAULT NULL,
  `user_id` bigint(20) DEFAULT NULL,
  `processed_at` date DEFAULT NULL,
  `restock` tinyint(1) DEFAULT NULL,
  `refunds_return` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `refund_line_items`
--

DROP TABLE IF EXISTS `refund_line_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `refund_line_items` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `location_id` bigint(20) DEFAULT NULL,
  `restock_type` varchar(55) DEFAULT NULL,
  `quantity` int(11) DEFAULT NULL,
  `line_item_id` bigint(20) NOT NULL,
  `subtotal` double DEFAULT NULL,
  `total_tax` double DEFAULT NULL,
  `refund_id` bigint(20) NOT NULL,
  `orderid` bigint(20) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `shipping_address`
--

DROP TABLE IF EXISTS `shipping_address`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `shipping_address` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `first_name` varchar(55) DEFAULT NULL,
  `address1` varchar(255) DEFAULT NULL,
  `phone` varchar(255) DEFAULT NULL,
  `city` varchar(255) DEFAULT NULL,
  `zip` varchar(55) DEFAULT NULL,
  `province` varchar(55) DEFAULT NULL,
  `country` varchar(55) DEFAULT NULL,
  `last_name` varchar(55) DEFAULT NULL,
  `address2` varchar(255) DEFAULT NULL,
  `company` varchar(55) DEFAULT NULL,
  `latitude` float DEFAULT NULL,
  `longitude` float DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  `country_code` varchar(55) DEFAULT NULL,
  `province_code` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=33245 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `shipping_lines`
--

DROP TABLE IF EXISTS `shipping_lines`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `shipping_lines` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `carrier_identifier` varchar(45) DEFAULT NULL,
  `code` varchar(55) DEFAULT NULL,
  `discounted_price` double DEFAULT NULL,
  `phone` varchar(55) DEFAULT NULL,
  `price` varchar(255) DEFAULT NULL,
  `requested_fulfillment_service_id` bigint(20) DEFAULT NULL,
  `source` varchar(55) DEFAULT NULL,
  `title` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=31718 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `shipping_methods`
--

DROP TABLE IF EXISTS `shipping_methods`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `shipping_methods` (
  `uuid` varchar(255) NOT NULL,
  `carrier_uuid` varchar(255) DEFAULT NULL,
  `name` varchar(255) NOT NULL,
  `base_fee_from` decimal(10,0) DEFAULT NULL,
  `base_fee_to` decimal(10,0) DEFAULT NULL,
  `base_fee_amount` decimal(10,0) DEFAULT NULL,
  `base_fee_amount_formatted` varchar(255) DEFAULT NULL,
  `currency` varchar(10) DEFAULT NULL,
  `weight_threshold` decimal(10,0) DEFAULT NULL,
  `weight_surcharge` decimal(10,0) DEFAULT NULL,
  `weight_surcharge_flat` tinyint(1) DEFAULT NULL,
  `weight_unit` varchar(10) DEFAULT NULL,
  `distance_threshold` decimal(10,0) DEFAULT NULL,
  `distance_surcharge` decimal(10,0) DEFAULT NULL,
  `distance_surcharge_flat` tinyint(1) DEFAULT NULL,
  `distance_unit` varchar(10) DEFAULT NULL,
  `volume_threshold` decimal(10,0) DEFAULT NULL,
  `volume_surcharge` decimal(10,0) DEFAULT NULL,
  `volume_surcharge_flat` tinyint(1) DEFAULT NULL,
  `volume_divisor` int(11) DEFAULT NULL,
  `volume_unit` varchar(10) DEFAULT NULL,
  PRIMARY KEY (`uuid`),
  KEY `carrier_uuid` (`carrier_uuid`),
  CONSTRAINT `shipping_methods_ibfk_1` FOREIGN KEY (`carrier_uuid`) REFERENCES `carriers` (`uuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `smart_collections`
--

DROP TABLE IF EXISTS `smart_collections`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `smart_collections` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `handle` varchar(255) NOT NULL,
  `title` varchar(255) NOT NULL,
  `updated_at` datetime NOT NULL,
  `body_html` text DEFAULT NULL,
  `published_at` datetime NOT NULL,
  `sort_order` varchar(50) NOT NULL,
  `template_suffix` varchar(255) DEFAULT NULL,
  `disjunctive` tinyint(1) NOT NULL,
  `published_scope` varchar(50) NOT NULL,
  `menu_category` tinyint(1) DEFAULT NULL,
  `Layer` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_collections_title` (`title`),
  KEY `idx_collections_handle` (`handle`),
  KEY `idx_collections_updated_at` (`updated_at`),
  KEY `idx_collections_body_html` (`body_html`(255))
) ENGINE=InnoDB AUTO_INCREMENT=508 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `supplier`
--

DROP TABLE IF EXISTS `supplier`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `supplier` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `company` varchar(255) DEFAULT NULL,
  `country` varchar(255) DEFAULT NULL,
  `address` varchar(255) DEFAULT NULL,
  `apartment` varchar(255) DEFAULT NULL,
  `city` varchar(255) DEFAULT NULL,
  `postal_code` varchar(255) DEFAULT NULL,
  `contact_name` varchar(255) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  `phone` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `taxlines`
--

DROP TABLE IF EXISTS `taxlines`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `taxlines` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `channel_liable` tinyint(1) DEFAULT NULL,
  `price` varchar(55) DEFAULT NULL,
  `rate` varchar(55) DEFAULT NULL,
  `title` varchar(55) DEFAULT NULL,
  `lineItemId` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `orderid` (`orderid`),
  CONSTRAINT `taxlines_ibfk_1` FOREIGN KEY (`orderid`) REFERENCES `orders` (`orderid`)
) ENGINE=InnoDB AUTO_INCREMENT=5537 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `timeline_events`
--

DROP TABLE IF EXISTS `timeline_events`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `timeline_events` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `OrderId` bigint(20) NOT NULL,
  `UserId` bigint(20) NOT NULL,
  `UserName` varchar(255) NOT NULL,
  `EventType` varchar(50) NOT NULL,
  `Description` text NOT NULL,
  `Metadata` text DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT current_timestamp(6),
  PRIMARY KEY (`Id`),
  KEY `FK_TimelineEvents_Users_UserId` (`UserId`),
  KEY `IX_TimelineEvents_OrderId` (`OrderId`),
  KEY `IX_TimelineEvents_CreatedAt` (`CreatedAt`),
  CONSTRAINT `FK_TimelineEvents_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`orderid`) ON DELETE CASCADE,
  CONSTRAINT `FK_TimelineEvents_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=40 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `total_discount_set`
--

DROP TABLE IF EXISTS `total_discount_set`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `total_discount_set` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `shop_amount` varchar(55) DEFAULT NULL,
  `shop_currency_code` varchar(55) DEFAULT NULL,
  `presentment_amount` varchar(55) DEFAULT NULL,
  `presentment_currency` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `orderid` (`orderid`),
  CONSTRAINT `total_discount_set_ibfk_1` FOREIGN KEY (`orderid`) REFERENCES `orders` (`orderid`)
) ENGINE=InnoDB AUTO_INCREMENT=34633 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `total_line_items_price_set`
--

DROP TABLE IF EXISTS `total_line_items_price_set`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `total_line_items_price_set` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `shop_amount` varchar(55) DEFAULT NULL,
  `shop_currency_code` varchar(55) DEFAULT NULL,
  `presentment_amount` varchar(55) DEFAULT NULL,
  `presentment_currency` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `orderid` (`orderid`),
  CONSTRAINT `total_line_items_price_set_ibfk_1` FOREIGN KEY (`orderid`) REFERENCES `orders` (`orderid`)
) ENGINE=InnoDB AUTO_INCREMENT=34633 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `total_shipping_price_set`
--

DROP TABLE IF EXISTS `total_shipping_price_set`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `total_shipping_price_set` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `shop_amount` varchar(55) DEFAULT NULL,
  `shop_currency_code` varchar(55) DEFAULT NULL,
  `presentment_amount` varchar(55) DEFAULT NULL,
  `presentment_currency` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `orderid` (`orderid`),
  CONSTRAINT `total_shipping_price_set_ibfk_1` FOREIGN KEY (`orderid`) REFERENCES `orders` (`orderid`)
) ENGINE=InnoDB AUTO_INCREMENT=34695 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `total_tax_set`
--

DROP TABLE IF EXISTS `total_tax_set`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `total_tax_set` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) DEFAULT NULL,
  `shop_amount` varchar(55) DEFAULT NULL,
  `shop_currency_code` varchar(55) DEFAULT NULL,
  `presentment_amount` varchar(55) DEFAULT NULL,
  `presentment_currency` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `orderid` (`orderid`),
  CONSTRAINT `total_tax_set_ibfk_1` FOREIGN KEY (`orderid`) REFERENCES `orders` (`orderid`)
) ENGINE=InnoDB AUTO_INCREMENT=34633 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `transactions`
--

DROP TABLE IF EXISTS `transactions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `transactions` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `orderid` bigint(20) NOT NULL,
  `kind` varchar(255) DEFAULT NULL,
  `gateway` varchar(255) DEFAULT NULL,
  `status` varchar(255) DEFAULT NULL,
  `message` varchar(255) DEFAULT NULL,
  `created_at` date DEFAULT NULL,
  `test` tinyint(1) DEFAULT NULL,
  `user_id` bigint(20) DEFAULT NULL,
  `parent_id` bigint(20) DEFAULT NULL,
  `processed_at` date DEFAULT NULL,
  `source_name` varchar(255) DEFAULT NULL,
  `amount` double DEFAULT NULL,
  `currency` varchar(55) DEFAULT NULL,
  `payment_id` varchar(255) DEFAULT NULL,
  `manual_payment_gateway` tinyint(1) DEFAULT NULL,
  `refund_id` bigint(20) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id` bigint(20) NOT NULL,
  `role` varchar(45) DEFAULT NULL,
  `first_name` varchar(55) DEFAULT NULL,
  `last_name` varchar(55) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  `password` varchar(255) DEFAULT NULL,
  `phone_number` varchar(45) DEFAULT NULL,
  `birthday` date DEFAULT NULL,
  `PasswordResetToken` varchar(255) DEFAULT NULL,
  `ResetTokenExpiry` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `email` (`email`),
  KEY `idx_users_first_name` (`first_name`),
  KEY `idx_users_last_name` (`last_name`),
  KEY `idx_users_email` (`email`),
  KEY `idx_users_phone_number` (`phone_number`),
  KEY `idx_users_role` (`role`),
  FULLTEXT KEY `idx_user_search` (`first_name`,`last_name`,`email`,`phone_number`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `variant_adjustment_history`
--

DROP TABLE IF EXISTS `variant_adjustment_history`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `variant_adjustment_history` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `variant_id` bigint(20) NOT NULL,
  `product_id` bigint(20) NOT NULL,
  `activity_type` enum('order_created','order_fulfilled','order_edited','order_cancelled','manual_adjustment','transfer','return') NOT NULL,
  `activity_reference` varchar(255) DEFAULT NULL,
  `activity_description` text DEFAULT NULL,
  `created_by` varchar(255) NOT NULL,
  `created_by_id` bigint(20) DEFAULT NULL,
  `unavailable` int(11) DEFAULT 0,
  `committed` int(11) DEFAULT 0,
  `available` int(11) DEFAULT 0,
  `on_hand` int(11) DEFAULT 0,
  `incoming` int(11) DEFAULT 0,
  `unavailable_change` int(11) DEFAULT 0,
  `committed_change` int(11) DEFAULT 0,
  `available_change` int(11) DEFAULT 0,
  `on_hand_change` int(11) DEFAULT 0,
  `incoming_change` int(11) DEFAULT 0,
  `notes` text DEFAULT NULL,
  `created_at` timestamp NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`id`),
  KEY `idx_variant_id` (`variant_id`),
  KEY `idx_product_id` (`product_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_activity_type` (`activity_type`),
  CONSTRAINT `variant_adjustment_history_ibfk_1` FOREIGN KEY (`variant_id`) REFERENCES `variants` (`id`) ON DELETE CASCADE,
  CONSTRAINT `variant_adjustment_history_ibfk_2` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `variants`
--

DROP TABLE IF EXISTS `variants`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `variants` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `product_id` bigint(20) NOT NULL,
  `title` varchar(255) DEFAULT NULL,
  `price` varchar(255) DEFAULT NULL,
  `sku` varchar(255) DEFAULT NULL,
  `position` int(11) DEFAULT NULL,
  `inventory_policy` varchar(50) DEFAULT NULL,
  `compare_at_price` varchar(255) DEFAULT NULL,
  `fulfillment_service` varchar(255) DEFAULT NULL,
  `inventory_management` varchar(255) DEFAULT NULL,
  `option1` varchar(255) DEFAULT NULL,
  `option2` varchar(255) DEFAULT NULL,
  `option3` varchar(255) DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  `updated_at` datetime DEFAULT NULL,
  `taxable` tinyint(1) DEFAULT NULL,
  `barcode` varchar(255) DEFAULT NULL,
  `grams` int(11) DEFAULT NULL,
  `image_id` bigint(20) DEFAULT NULL,
  `weight` decimal(10,2) DEFAULT NULL,
  `weight_unit` varchar(50) DEFAULT NULL,
  `requires_shipping` tinyint(1) DEFAULT NULL,
  `inventory_id` int(11) DEFAULT NULL,
  `inventory_quantity` int(11) DEFAULT NULL,
  `reserved_quantity` int(11) DEFAULT 0 COMMENT 'Quantity reserved for pending orders',
  `old_inventory_quantity` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `product_id` (`product_id`),
  KEY `idx_variants_sku` (`sku`),
  KEY `idx_variants_barcode` (`barcode`),
  KEY `idx_variants_title` (`title`),
  KEY `idx_variants_product_id` (`product_id`),
  KEY `idx_variants_reserved_qty` (`reserved_quantity`),
  KEY `IX_Variants_ProductId_Inventory` (`product_id`,`inventory_quantity`)
) ENGINE=InnoDB AUTO_INCREMENT=47707668545781 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-04-20 18:17:47
