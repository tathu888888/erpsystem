-- MySQL dump 10.13  Distrib 5.7.24, for Win64 (x86_64)
--
-- Host: localhost    Database: sunstar
-- ------------------------------------------------------
-- Server version	8.0.44

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
-- Table structure for table `account_master`
--

DROP TABLE IF EXISTS `account_master`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `account_master` (
  `account_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `account_code` varchar(20) NOT NULL,
  `account_name` varchar(100) NOT NULL,
  `account_type` enum('ASSET','LIABILITY','EQUITY','REVENUE','EXPENSE') NOT NULL,
  `parent_id` bigint DEFAULT NULL,
  `is_active` tinyint DEFAULT '1',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `account_category` varchar(50) DEFAULT NULL COMMENT 'CURRENT_ASSET / CURRENT_LIABILITY „Å™„Å©',
  `is_postable` tinyint(1) NOT NULL DEFAULT '1' COMMENT '‰ªïË®≥ÂèØËÉΩ„Éï„É©„Ç∞',
  `sort_order` int DEFAULT '0' COMMENT 'Ë°®Á§∫È†Ü',
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` int NOT NULL DEFAULT '1',
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`account_id`),
  UNIQUE KEY `uq_account_code` (`account_code`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `accounting_period`
--

DROP TABLE IF EXISTS `accounting_period`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `accounting_period` (
  `period_id` int NOT NULL AUTO_INCREMENT,
  `fiscal_year` int NOT NULL,
  `period_no` int NOT NULL,
  `period_name` varchar(20) NOT NULL,
  `date_from` date NOT NULL,
  `date_to` date NOT NULL,
  `is_closed` tinyint(1) NOT NULL DEFAULT '0',
  `closed_at` datetime DEFAULT NULL,
  `closed_by` int DEFAULT NULL,
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`period_id`),
  UNIQUE KEY `uq_period` (`fiscal_year`,`period_no`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ap_payment_apply`
--

DROP TABLE IF EXISTS `ap_payment_apply`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ap_payment_apply` (
  `payment_id` bigint unsigned NOT NULL,
  `po_id` bigint unsigned NOT NULL,
  `applied_amount` decimal(12,2) NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`payment_id`,`po_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ap_payment_header`
--

DROP TABLE IF EXISTS `ap_payment_header`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ap_payment_header` (
  `payment_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `supplier_code` varchar(20) NOT NULL,
  `payment_date` date NOT NULL,
  `amount` decimal(12,2) NOT NULL,
  `method` varchar(30) DEFAULT NULL,
  `memo` varchar(255) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`payment_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ar_invoice_apply_shipment`
--

DROP TABLE IF EXISTS `ar_invoice_apply_shipment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ar_invoice_apply_shipment` (
  `invoice_id` bigint unsigned NOT NULL,
  `shipment_batch_id` bigint NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`invoice_id`,`shipment_batch_id`),
  KEY `fk_inv_ship_ship` (`shipment_batch_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_inv_ship_inv` FOREIGN KEY (`invoice_id`) REFERENCES `ar_invoice_header` (`invoice_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_inv_ship_ship` FOREIGN KEY (`shipment_batch_id`) REFERENCES `shipments_header` (`shipment_batch_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ar_invoice_header`
--

DROP TABLE IF EXISTS `ar_invoice_header`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ar_invoice_header` (
  `invoice_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `customer_code` varchar(20) NOT NULL,
  `invoice_date` date NOT NULL,
  `due_date` date DEFAULT NULL,
  `total_net` decimal(12,2) NOT NULL DEFAULT '0.00',
  `tax_amount` decimal(12,2) NOT NULL DEFAULT '0.00',
  `total_gross` decimal(12,2) NOT NULL DEFAULT '0.00',
  `status` enum('DRAFT','ISSUED','VOID') NOT NULL DEFAULT 'DRAFT',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`invoice_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ar_invoice_line`
--

DROP TABLE IF EXISTS `ar_invoice_line`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ar_invoice_line` (
  `invoice_line_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `invoice_id` bigint unsigned NOT NULL,
  `line_no` int NOT NULL,
  `revenue_account_code` varchar(20) NOT NULL,
  `amount` decimal(12,2) NOT NULL,
  `item_id` bigint unsigned DEFAULT NULL,
  `memo` varchar(255) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`invoice_line_id`),
  KEY `idx_inv_line` (`invoice_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_inv_line_hdr` FOREIGN KEY (`invoice_id`) REFERENCES `ar_invoice_header` (`invoice_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ar_invoice_shipment`
--

DROP TABLE IF EXISTS `ar_invoice_shipment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ar_invoice_shipment` (
  `invoice_id` bigint unsigned NOT NULL,
  `shipment_batch_id` bigint unsigned NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`invoice_id`,`shipment_batch_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `ar_invoice_shipment_ibfk_1` FOREIGN KEY (`invoice_id`) REFERENCES `ar_invoice_header` (`invoice_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ar_receipt_apply`
--

DROP TABLE IF EXISTS `ar_receipt_apply`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ar_receipt_apply` (
  `receipt_id` bigint unsigned NOT NULL,
  `invoice_id` bigint unsigned NOT NULL,
  `applied_amount` decimal(12,2) NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`receipt_id`,`invoice_id`),
  KEY `invoice_id` (`invoice_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `ar_receipt_apply_ibfk_1` FOREIGN KEY (`receipt_id`) REFERENCES `ar_receipt_header` (`receipt_id`),
  CONSTRAINT `ar_receipt_apply_ibfk_2` FOREIGN KEY (`invoice_id`) REFERENCES `ar_invoice_header` (`invoice_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ar_receipt_apply_shipment`
--

DROP TABLE IF EXISTS `ar_receipt_apply_shipment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ar_receipt_apply_shipment` (
  `receipt_id` bigint unsigned NOT NULL,
  `shipment_batch_id` bigint NOT NULL,
  `applied_amount` decimal(12,2) NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`receipt_id`,`shipment_batch_id`),
  KEY `fk_rcpt_apply_ship` (`shipment_batch_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_rcpt_apply_hdr` FOREIGN KEY (`receipt_id`) REFERENCES `ar_receipt_header` (`receipt_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_rcpt_apply_ship` FOREIGN KEY (`shipment_batch_id`) REFERENCES `shipments_header` (`shipment_batch_id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ar_receipt_header`
--

DROP TABLE IF EXISTS `ar_receipt_header`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ar_receipt_header` (
  `receipt_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `customer_code` varchar(20) NOT NULL,
  `receipt_date` date NOT NULL,
  `amount` decimal(12,2) NOT NULL,
  `method` varchar(30) DEFAULT NULL,
  `memo` varchar(255) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`receipt_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `audit_log`
--

DROP TABLE IF EXISTS `audit_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `audit_log` (
  `audit_id` bigint NOT NULL AUTO_INCREMENT,
  `table_name` varchar(64) NOT NULL,
  `action` enum('INSERT','UPDATE','DELETE') NOT NULL,
  `pk_json` json DEFAULT NULL,
  `changed_by` bigint DEFAULT NULL,
  `changed_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `before_json` json DEFAULT NULL,
  `after_json` json DEFAULT NULL,
  `save_seq` int DEFAULT NULL,
  `ref_type` varchar(32) DEFAULT NULL,
  `ref_id` bigint DEFAULT NULL,
  `ref_line_no` int DEFAULT NULL,
  PRIMARY KEY (`audit_id`),
  KEY `idx_tbl_time` (`table_name`,`changed_at`),
  KEY `idx_user_time` (`changed_by`,`changed_at`)
) ENGINE=InnoDB AUTO_INCREMENT=75 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `customer_master`
--

DROP TABLE IF EXISTS `customer_master`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `customer_master` (
  `customer_code` varchar(10) NOT NULL,
  `customer_name` varchar(50) NOT NULL,
  `phone_number` varchar(15) NOT NULL,
  `address` varchar(100) NOT NULL,
  `prefecture` varchar(10) DEFAULT NULL,
  `apartment_name` varchar(50) DEFAULT NULL,
  `birth_date` date DEFAULT NULL,
  `payment_method` varchar(20) DEFAULT NULL,
  `purchase_start` date DEFAULT NULL,
  `purchase_reason` varchar(100) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`customer_code`),
  UNIQUE KEY `uq_customer_phone` (`phone_number`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `gl_journal_header`
--

DROP TABLE IF EXISTS `gl_journal_header`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `gl_journal_header` (
  `journal_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `tran_type` varchar(30) NOT NULL,
  `tran_id` bigint unsigned NOT NULL,
  `save_seq` int unsigned NOT NULL DEFAULT '1',
  `journal_date` date NOT NULL,
  `ref_type` varchar(30) NOT NULL,
  `ref_id` bigint unsigned NOT NULL,
  `memo` varchar(255) DEFAULT NULL,
  `posted` tinyint(1) NOT NULL DEFAULT '1',
  `is_reversal` tinyint(1) NOT NULL DEFAULT '0',
  `reversed_journal_id` bigint unsigned DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint DEFAULT NULL,
  `updated_by` bigint DEFAULT NULL,
  PRIMARY KEY (`journal_id`),
  UNIQUE KEY `uq_tran` (`tran_type`,`tran_id`,`is_reversal`),
  UNIQUE KEY `uq_tran_save` (`tran_type`,`tran_id`,`save_seq`),
  KEY `idx_tran` (`tran_type`,`tran_id`),
  KEY `idx_date` (`journal_date`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`)
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = cp932 */ ;
/*!50003 SET character_set_results = cp932 */ ;
/*!50003 SET collation_connection  = cp932_japanese_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_gl_journal_lock_bi` BEFORE INSERT ON `gl_journal_header` FOR EACH ROW BEGIN
  DECLARE v_closed INT;

  SELECT is_closed INTO v_closed
  FROM accounting_period
  WHERE NEW.journal_date BETWEEN date_from AND date_to;

  IF v_closed = 1 THEN
     SIGNAL SQLSTATE '45000'
     SET MESSAGE_TEXT = 'Ç±ÇÃâÔåvä˙ä‘ÇÕí˜ÇﬂçœÇ›Ç≈Ç∑ÅBédñÛìoò^ïsâ¬ÅB';
  END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = cp932 */ ;
/*!50003 SET character_set_results = cp932 */ ;
/*!50003 SET collation_connection  = cp932_japanese_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_gl_journal_lock_bu` BEFORE UPDATE ON `gl_journal_header` FOR EACH ROW BEGIN
  DECLARE v_closed INT DEFAULT 0;

  SELECT ap.is_closed INTO v_closed
  FROM accounting_period ap
  WHERE OLD.journal_date BETWEEN ap.date_from AND ap.date_to
  LIMIT 1;

  IF IFNULL(v_closed, 0) = 1 THEN
    SIGNAL SQLSTATE '45000'
      SET MESSAGE_TEXT = 'í˜ÇﬂçœÇ›ä˙ä‘ÇÃédñÛÇÕïœçXïsâ¬ÅB';
  END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `gl_journal_line`
--

DROP TABLE IF EXISTS `gl_journal_line`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `gl_journal_line` (
  `journal_line_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `journal_id` bigint unsigned NOT NULL,
  `line_no` int unsigned NOT NULL,
  `account_code` varchar(30) NOT NULL,
  `debit` decimal(18,2) NOT NULL DEFAULT '0.00',
  `credit` decimal(18,2) NOT NULL DEFAULT '0.00',
  `item_id` int unsigned DEFAULT NULL,
  `customer_code` varchar(30) DEFAULT NULL,
  `supplier_code` varchar(40) DEFAULT NULL,
  `dept_code` varchar(30) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint DEFAULT NULL,
  `updated_by` bigint DEFAULT NULL,
  PRIMARY KEY (`journal_line_id`),
  UNIQUE KEY `uq_journal_line` (`journal_id`,`line_no`),
  KEY `idx_account` (`account_code`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_gl_line_header` FOREIGN KEY (`journal_id`) REFERENCES `gl_journal_header` (`journal_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `inventory_ledger`
--

DROP TABLE IF EXISTS `inventory_ledger`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `inventory_ledger` (
  `ledger_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `item_id` int unsigned NOT NULL,
  `lot_id` bigint unsigned DEFAULT NULL,
  `qty_delta` int NOT NULL,
  `ref_type` varchar(30) NOT NULL,
  `ref_id` bigint unsigned NOT NULL,
  `ref_line_no` int unsigned NOT NULL,
  `entry_type` varchar(20) NOT NULL,
  `save_seq` int unsigned NOT NULL,
  `is_void` tinyint(1) NOT NULL DEFAULT '0',
  `voided_at` datetime DEFAULT NULL,
  `void_reason` varchar(255) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `active_key` varchar(100) GENERATED ALWAYS AS ((case when (`is_void` = 0) then concat(`ref_type`,_utf8mb4':',`ref_id`,_utf8mb4':',`ref_line_no`) else NULL end)) STORED,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint DEFAULT NULL,
  `updated_by` bigint DEFAULT NULL,
  PRIMARY KEY (`ledger_id`),
  UNIQUE KEY `uq_deaup` (`ref_type`,`ref_id`,`save_seq`,`ref_line_no`,`entry_type`,`lot_id`),
  UNIQUE KEY `uq_inventory_ledger_active` (`active_key`),
  KEY `idx_ref` (`ref_type`,`ref_id`,`save_seq`),
  KEY `idx_item` (`item_id`,`created_at`),
  KEY `ix_ledger_lot` (`lot_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_ledger_lot` FOREIGN KEY (`lot_id`) REFERENCES `lot` (`lot_id`)
) ENGINE=InnoDB AUTO_INCREMENT=243 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = cp932 */ ;
/*!50003 SET character_set_results = cp932 */ ;
/*!50003 SET collation_connection  = cp932_japanese_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_inventory_ledger_ai` AFTER INSERT ON `inventory_ledger` FOR EACH ROW BEGIN
  INSERT INTO audit_log(table_name, action, pk_json, changed_by, after_json, save_seq, ref_type, ref_id, ref_line_no)
  VALUES(
    'inventory_ledger','INSERT',
    JSON_OBJECT('ledger_id', NEW.ledger_id),
    IFNULL(@app_user_id, NEW.created_by),
    JSON_OBJECT(
      'ledger_id', NEW.ledger_id,
      'item_id', NEW.item_id,
      'lot_id', NEW.lot_id,
      'qty_delta', NEW.qty_delta,
      'ref_type', NEW.ref_type,
      'ref_id', NEW.ref_id,
      'ref_line_no', NEW.ref_line_no,
      'entry_type', NEW.entry_type,
      'save_seq', NEW.save_seq
    ),
    NEW.save_seq, NEW.ref_type, NEW.ref_id, NEW.ref_line_no
  );
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `item_master`
--

DROP TABLE IF EXISTS `item_master`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `item_master` (
  `item_id` int unsigned NOT NULL AUTO_INCREMENT,
  `item_code` varchar(32) NOT NULL,
  `item_name` varchar(120) NOT NULL,
  `cogs_account_id` bigint unsigned DEFAULT NULL,
  `inventory_account_id` bigint unsigned DEFAULT NULL,
  `revenue_account_id` bigint unsigned DEFAULT NULL,
  `unit1` varchar(20) NOT NULL DEFAULT '',
  `quantity1` bigint unsigned NOT NULL,
  `unit2` varchar(20) NOT NULL DEFAULT '',
  `conversion_qty` int unsigned NOT NULL DEFAULT '1',
  `is_lot_item` char(1) NOT NULL DEFAULT 'F',
  `quantity2` int unsigned NOT NULL DEFAULT '1',
  `unit1_price` int unsigned NOT NULL DEFAULT '0',
  `unit2_price` int unsigned NOT NULL DEFAULT '0',
  `default_price` int unsigned NOT NULL DEFAULT '0',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `cost_per_piece` int unsigned NOT NULL DEFAULT '0',
  `created_by` bigint DEFAULT NULL,
  `updated_by` bigint DEFAULT NULL,
  PRIMARY KEY (`item_id`),
  UNIQUE KEY `uq_item_code` (`item_code`),
  KEY `idx_item_active` (`is_active`,`item_name`),
  KEY `fk_item_cogs_account_v2` (`cogs_account_id`),
  KEY `fk_item_inventory_account_v2` (`inventory_account_id`),
  KEY `fk_item_revenue_account_v2` (`revenue_account_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_item_cogs_account` FOREIGN KEY (`cogs_account_id`) REFERENCES `account_master` (`account_id`),
  CONSTRAINT `fk_item_cogs_account_v2` FOREIGN KEY (`cogs_account_id`) REFERENCES `account_master` (`account_id`),
  CONSTRAINT `fk_item_inventory_account` FOREIGN KEY (`inventory_account_id`) REFERENCES `account_master` (`account_id`),
  CONSTRAINT `fk_item_inventory_account_v2` FOREIGN KEY (`inventory_account_id`) REFERENCES `account_master` (`account_id`),
  CONSTRAINT `fk_item_revenue_account` FOREIGN KEY (`revenue_account_id`) REFERENCES `account_master` (`account_id`),
  CONSTRAINT `fk_item_revenue_account_v2` FOREIGN KEY (`revenue_account_id`) REFERENCES `account_master` (`account_id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = cp932 */ ;
/*!50003 SET character_set_results = cp932 */ ;
/*!50003 SET collation_connection  = cp932_japanese_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_item_master_bu` BEFORE UPDATE ON `item_master` FOR EACH ROW BEGIN
  DECLARE conv INT;
  SET conv = IFNULL(NEW.conversion_qty, 1);
  IF conv <= 0 THEN SET conv = 1; END IF;

  SET NEW.quantity2 = FLOOR(NEW.quantity1 / conv);
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = cp932 */ ;
/*!50003 SET character_set_results = cp932 */ ;
/*!50003 SET collation_connection  = cp932_japanese_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_item_master_au` AFTER UPDATE ON `item_master` FOR EACH ROW BEGIN
  IF NOT (OLD.quantity1 <=> NEW.quantity1) OR NOT (OLD.quantity2 <=> NEW.quantity2) THEN
    INSERT INTO audit_log(table_name, action, pk_json, changed_by, before_json, after_json)
    VALUES(
      'item_master','UPDATE',
      JSON_OBJECT('item_id', NEW.item_id),
      IFNULL(@app_user_id, NEW.updated_by),
      JSON_OBJECT('quantity1', OLD.quantity1, 'quantity2', OLD.quantity2),
      JSON_OBJECT('quantity1', NEW.quantity1, 'quantity2', NEW.quantity2)
    );
  END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `lot`
--

DROP TABLE IF EXISTS `lot`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `lot` (
  `lot_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `item_id` int unsigned NOT NULL,
  `lot_no` varchar(64) NOT NULL,
  `received_date` date DEFAULT NULL,
  `qty_on_hand_pieces` int NOT NULL DEFAULT '0',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint DEFAULT NULL,
  `updated_by` bigint DEFAULT NULL,
  PRIMARY KEY (`lot_id`),
  UNIQUE KEY `uk_item_lot` (`item_id`,`lot_no`),
  KEY `ix_item_active` (`item_id`,`is_active`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_lot_item` FOREIGN KEY (`item_id`) REFERENCES `item_master` (`item_id`)
) ENGINE=InnoDB AUTO_INCREMENT=36 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = cp932 */ ;
/*!50003 SET character_set_results = cp932 */ ;
/*!50003 SET collation_connection  = cp932_japanese_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_lot_au` AFTER UPDATE ON `lot` FOR EACH ROW BEGIN
  IF NOT (OLD.qty_on_hand_pieces <=> NEW.qty_on_hand_pieces) THEN
    INSERT INTO audit_log(table_name, action, pk_json, changed_by, before_json, after_json)
    VALUES(
      'lot','UPDATE',
      JSON_OBJECT('lot_id', NEW.lot_id),
      IFNULL(@app_user_id, NEW.updated_by),
      JSON_OBJECT('qty_on_hand_pieces', OLD.qty_on_hand_pieces),
      JSON_OBJECT('qty_on_hand_pieces', NEW.qty_on_hand_pieces)
    );
  END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `lot_unit`
--

DROP TABLE IF EXISTS `lot_unit`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `lot_unit` (
  `unit_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `lot_id` bigint unsigned NOT NULL,
  `serial_no` varchar(64) DEFAULT NULL,
  `status` enum('ON_HAND','ALLOCATED','ISSUED','VOID') NOT NULL DEFAULT 'ON_HAND',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint DEFAULT NULL,
  `updated_by` bigint DEFAULT NULL,
  PRIMARY KEY (`unit_id`),
  UNIQUE KEY `uk_lot_serial` (`lot_id`,`serial_no`),
  KEY `ix_lot_status` (`lot_id`,`status`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_unit_lot` FOREIGN KEY (`lot_id`) REFERENCES `lot` (`lot_id`)
) ENGINE=InnoDB AUTO_INCREMENT=419 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `purchase_order_detail`
--

DROP TABLE IF EXISTS `purchase_order_detail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `purchase_order_detail` (
  `po_detail_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `po_id` bigint unsigned NOT NULL,
  `line_no` int NOT NULL,
  `item_id` bigint unsigned DEFAULT NULL,
  `item_name` varchar(200) NOT NULL,
  `lot_no` varchar(64) DEFAULT NULL,
  `qty` int NOT NULL DEFAULT '0',
  `received_qty` decimal(18,3) NOT NULL DEFAULT '0.000',
  `unit` varchar(20) DEFAULT NULL,
  `unit_price` int NOT NULL DEFAULT '0',
  `amount` bigint NOT NULL DEFAULT '0',
  `remark` varchar(200) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`po_detail_id`),
  KEY `idx_po_id` (`po_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_po_detail_header` FOREIGN KEY (`po_id`) REFERENCES `purchase_order_header` (`po_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `purchase_order_header`
--

DROP TABLE IF EXISTS `purchase_order_header`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `purchase_order_header` (
  `po_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `order_date` date DEFAULT NULL,
  `po_no` varchar(40) NOT NULL,
  `po_date` date NOT NULL DEFAULT (curdate()),
  `supplier_id` bigint unsigned NOT NULL,
  `supplier_name` varchar(120) NOT NULL,
  `status` varchar(20) NOT NULL DEFAULT 'OPEN',
  `subtotal` bigint NOT NULL DEFAULT '0',
  `tax` bigint NOT NULL DEFAULT '0',
  `total` bigint NOT NULL DEFAULT '0',
  `remark` varchar(500) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`po_id`),
  UNIQUE KEY `uk_po_no` (`po_no`),
  KEY `idx_po_date` (`po_date`),
  KEY `fk_po_supplier` (`supplier_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_po_supplier` FOREIGN KEY (`supplier_id`) REFERENCES `supplier_master` (`supplier_id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `shipment_lot_alloc`
--

DROP TABLE IF EXISTS `shipment_lot_alloc`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `shipment_lot_alloc` (
  `alloc_id` bigint NOT NULL AUTO_INCREMENT,
  `shipment_detail_id` bigint NOT NULL,
  `lot_id` bigint unsigned NOT NULL,
  `qty_pieces` int NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint DEFAULT NULL,
  `updated_by` bigint DEFAULT NULL,
  PRIMARY KEY (`alloc_id`),
  KEY `idx_alloc_detail` (`shipment_detail_id`),
  KEY `idx_alloc_lot` (`lot_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_alloc_detail` FOREIGN KEY (`shipment_detail_id`) REFERENCES `shipments_detail` (`shipment_detail_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_alloc_lot` FOREIGN KEY (`lot_id`) REFERENCES `lot` (`lot_id`),
  CONSTRAINT `fk_ship_lot_alloc_detail` FOREIGN KEY (`shipment_detail_id`) REFERENCES `shipments_detail` (`shipment_detail_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=80 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = cp932 */ ;
/*!50003 SET character_set_results = cp932 */ ;
/*!50003 SET collation_connection  = cp932_japanese_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_shipment_lot_alloc_ai` AFTER INSERT ON `shipment_lot_alloc` FOR EACH ROW BEGIN
  INSERT INTO audit_log(
    table_name, action, pk_json, changed_by, after_json, ref_type, ref_id
  )
  VALUES(
    'shipment_lot_alloc',
    'INSERT',
    JSON_OBJECT(
      'shipment_detail_id', NEW.shipment_detail_id,
      'lot_id', NEW.lot_id
    ),
    IFNULL(@app_user_id, NEW.created_by),
    JSON_OBJECT(
      'shipment_detail_id', NEW.shipment_detail_id,
      'lot_id', NEW.lot_id,
      'qty_pieces', NEW.qty_pieces
    ),
    'SHIPMENT_DETAIL',
    NEW.shipment_detail_id
  );
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `shipment_unit_alloc`
--

DROP TABLE IF EXISTS `shipment_unit_alloc`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `shipment_unit_alloc` (
  `alloc_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `shipment_detail_id` bigint NOT NULL,
  `unit_id` bigint unsigned NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint DEFAULT NULL,
  `updated_by` bigint DEFAULT NULL,
  PRIMARY KEY (`alloc_id`),
  UNIQUE KEY `uq_detail_unit` (`shipment_detail_id`,`unit_id`),
  KEY `ix_unit` (`unit_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_unit_alloc_detail` FOREIGN KEY (`shipment_detail_id`) REFERENCES `shipments_detail` (`shipment_detail_id`) ON DELETE CASCADE,
  CONSTRAINT `fk_unit_alloc_unit` FOREIGN KEY (`unit_id`) REFERENCES `lot_unit` (`unit_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1010 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `shipments`
--

DROP TABLE IF EXISTS `shipments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `shipments` (
  `shipment_id` int NOT NULL AUTO_INCREMENT COMMENT 'Áô∫ÈÄÅID',
  `shipment_date` date NOT NULL COMMENT 'Áô∫ÈÄÅÂπ¥ÊúàÊó•',
  `customer_code` char(6) NOT NULL COMMENT 'È°ßÂÆ¢„Ç≥„Éº„Éâ',
  `quantity` varchar(255) DEFAULT NULL,
  `unit_price` varchar(255) DEFAULT NULL,
  `amount` varchar(255) DEFAULT NULL,
  `remark` text,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`shipment_id`),
  KEY `fk_shipments_customer` (`customer_code`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_shipments_customer` FOREIGN KEY (`customer_code`) REFERENCES `customer_master` (`customer_code`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `shipments_detail`
--

DROP TABLE IF EXISTS `shipments_detail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `shipments_detail` (
  `shipment_detail_id` bigint NOT NULL AUTO_INCREMENT,
  `shipment_batch_id` bigint NOT NULL,
  `line_no` int NOT NULL,
  `item_id` int unsigned DEFAULT NULL,
  `lot_id` bigint unsigned DEFAULT NULL,
  `quantity` int NOT NULL,
  `quantity2` int unsigned NOT NULL DEFAULT '0',
  `unit_price` int NOT NULL DEFAULT '0',
  `unit` varchar(32) NOT NULL DEFAULT '',
  `remark` varchar(255) NOT NULL DEFAULT '',
  `amount` decimal(20,0) GENERATED ALWAYS AS ((`quantity2` * `unit_price`)) STORED,
  `unit_seq` int NOT NULL DEFAULT '0',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint DEFAULT NULL,
  `updated_by` bigint DEFAULT NULL,
  PRIMARY KEY (`shipment_detail_id`),
  UNIQUE KEY `uq_ship_dtl_batch_line` (`shipment_batch_id`,`line_no`),
  KEY `idx_ship_dtl_batch` (`shipment_batch_id`),
  KEY `idx_shipments_detail_item_id` (`item_id`),
  KEY `ix_ship_detail_lot` (`lot_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_ship_detail_lot` FOREIGN KEY (`lot_id`) REFERENCES `lot` (`lot_id`),
  CONSTRAINT `fk_shipments_detail__shipment_batch_id` FOREIGN KEY (`shipment_batch_id`) REFERENCES `shipments_header` (`shipment_batch_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_shipments_detail_item` FOREIGN KEY (`item_id`) REFERENCES `item_master` (`item_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=375 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = cp932 */ ;
/*!50003 SET character_set_results = cp932 */ ;
/*!50003 SET collation_connection  = cp932_japanese_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_shipments_detail_ai` AFTER INSERT ON `shipments_detail` FOR EACH ROW BEGIN
  INSERT INTO audit_log(table_name, action, pk_json, changed_by, after_json, ref_type, ref_id, ref_line_no)
  VALUES(
    'shipments_detail','INSERT',
    JSON_OBJECT('shipment_detail_id', NEW.shipment_detail_id),
    IFNULL(@app_user_id, NEW.created_by),
    JSON_OBJECT(
      'shipment_detail_id', NEW.shipment_detail_id,
      'shipment_batch_id', NEW.shipment_batch_id,
      'line_no', NEW.line_no,
      'item_id', NEW.item_id,
      'lot_id', NEW.lot_id,
      'quantity', NEW.quantity,
      'quantity2', NEW.quantity2,
      'unit_price', NEW.unit_price,
      'unit', NEW.unit,
      'remark', NEW.remark
    ),
    'SHIPMENT', NEW.shipment_batch_id, NEW.line_no
  );
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `shipments_header`
--

DROP TABLE IF EXISTS `shipments_header`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `shipments_header` (
  `shipment_batch_id` bigint NOT NULL AUTO_INCREMENT,
  `shipment_date` date NOT NULL,
  `customer_code` varchar(20) NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `save_seq` int unsigned NOT NULL DEFAULT '0',
  `created_by` bigint DEFAULT NULL,
  `updated_by` bigint DEFAULT NULL,
  PRIMARY KEY (`shipment_batch_id`),
  KEY `idx_ship_hdr_date` (`shipment_date`),
  KEY `idx_ship_hdr_customer` (`customer_code`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_ship_hdr_customer` FOREIGN KEY (`customer_code`) REFERENCES `customer_master` (`customer_code`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=206 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = cp932 */ ;
/*!50003 SET character_set_results = cp932 */ ;
/*!50003 SET collation_connection  = cp932_japanese_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_shipments_header_bi` BEFORE INSERT ON `shipments_header` FOR EACH ROW BEGIN
  SET NEW.created_by = IFNULL(NEW.created_by, @app_user_id);
  SET NEW.updated_by = IFNULL(NEW.updated_by, @app_user_id);
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = cp932 */ ;
/*!50003 SET character_set_results = cp932 */ ;
/*!50003 SET collation_connection  = cp932_japanese_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_shipments_header_ai` AFTER INSERT ON `shipments_header` FOR EACH ROW BEGIN
  INSERT INTO audit_log(table_name, action, pk_json, changed_by, after_json, ref_type, ref_id)
  VALUES(
    'shipments_header','INSERT',
    JSON_OBJECT('shipment_batch_id', NEW.shipment_batch_id),
    IFNULL(@app_user_id, NEW.created_by),
    JSON_OBJECT(
      'shipment_batch_id', NEW.shipment_batch_id,
      'shipment_date', NEW.shipment_date,
      'customer_code', NEW.customer_code,
      'save_seq', NEW.save_seq
    ),
    'SHIPMENT', NEW.shipment_batch_id
  );
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = cp932 */ ;
/*!50003 SET character_set_results = cp932 */ ;
/*!50003 SET collation_connection  = cp932_japanese_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_shipments_header_bu` BEFORE UPDATE ON `shipments_header` FOR EACH ROW BEGIN
  SET NEW.updated_by = IFNULL(NEW.updated_by, @app_user_id);
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `subscription`
--

DROP TABLE IF EXISTS `subscription`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `subscription` (
  `subscription_id` bigint NOT NULL AUTO_INCREMENT,
  `customer_code` varchar(6) NOT NULL,
  `status` enum('active','canceled') NOT NULL DEFAULT 'active',
  `start_date` date NOT NULL,
  `cancel_date` date DEFAULT NULL,
  `ship_day` tinyint NOT NULL DEFAULT '15',
  `box_qty` int NOT NULL DEFAULT '1',
  `unit_price` int NOT NULL DEFAULT '5300',
  `note` varchar(255) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`subscription_id`),
  UNIQUE KEY `uq_subscription_customer` (`customer_code`),
  KEY `idx_subscription_status_day` (`status`,`ship_day`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_subscription_customer` FOREIGN KEY (`customer_code`) REFERENCES `customer_master` (`customer_code`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `subscriptions`
--

DROP TABLE IF EXISTS `subscriptions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `subscriptions` (
  `subscription_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `customer_code` char(6) NOT NULL,
  `product_code` varchar(32) NOT NULL DEFAULT 'GREEN_SARANA_CAN',
  `unit_price` int unsigned NOT NULL DEFAULT '5300',
  `quantity` int unsigned NOT NULL DEFAULT '1',
  `ship_day_of_month` tinyint unsigned NOT NULL DEFAULT '15',
  `status` enum('ACTIVE','PAUSED','CANCELED') NOT NULL DEFAULT 'ACTIVE',
  `start_date` date NOT NULL,
  `end_date` date DEFAULT NULL,
  `cancel_reason` varchar(255) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `active_guard` tinyint GENERATED ALWAYS AS ((case when (`status` = _utf8mb4'ACTIVE') then 1 else NULL end)) STORED,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`subscription_id`),
  UNIQUE KEY `uq_subscriptions_one_active_per_customer` (`customer_code`,`active_guard`),
  KEY `idx_subscriptions_customer_code` (`customer_code`),
  KEY `idx_subscriptions_status` (`status`),
  KEY `idx_subscriptions_start_date` (`start_date`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`),
  CONSTRAINT `fk_subscriptions_customer_code` FOREIGN KEY (`customer_code`) REFERENCES `customer_master` (`customer_code`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_subscriptions_customers_code` FOREIGN KEY (`customer_code`) REFERENCES `customer_master` (`customer_code`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=26 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `supplier_master`
--

DROP TABLE IF EXISTS `supplier_master`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `supplier_master` (
  `supplier_id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `supplier_code` varchar(32) NOT NULL,
  `ap_account_code` varchar(20) NOT NULL DEFAULT '2100',
  `supplier_name` varchar(120) NOT NULL,
  `tel` varchar(40) DEFAULT NULL,
  `email` varchar(120) DEFAULT NULL,
  `address1` varchar(200) DEFAULT NULL,
  `address2` varchar(200) DEFAULT NULL,
  `remark` varchar(500) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`supplier_id`),
  UNIQUE KEY `uk_supplier_code` (`supplier_code`),
  KEY `idx_supplier_name` (`supplier_name`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `users` (
  `user_id` int NOT NULL AUTO_INCREMENT,
  `username` varchar(50) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `role` enum('admin','user') NOT NULL DEFAULT 'user',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  `updated_by` bigint NOT NULL,
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `username` (`username`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_updated_at` (`updated_at`),
  KEY `idx_created_by` (`created_by`),
  KEY `idx_updated_by` (`updated_by`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping events for database 'sunstar'
--

--
-- Dumping routines for database 'sunstar'
--
/*!50003 DROP PROCEDURE IF EXISTS `add_audit_all_tables` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = cp932 */ ;
/*!50003 SET character_set_results = cp932 */ ;
/*!50003 SET collation_connection  = cp932_japanese_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `add_audit_all_tables`()
BEGIN
  
  CALL add_audit_columns('account_master');
  CALL add_audit_columns('ap_payment_apply');
  CALL add_audit_columns('ap_payment_header');
  CALL add_audit_columns('ar_invoice_apply_shipment');
  CALL add_audit_columns('ar_invoice_header');
  CALL add_audit_columns('ar_invoice_line');
  CALL add_audit_columns('ar_invoice_shipment');
  CALL add_audit_columns('ar_receipt_apply');
  CALL add_audit_columns('ar_receipt_apply_shipment');
  CALL add_audit_columns('ar_receipt_header');
  CALL add_audit_columns('customer_master');
  CALL add_audit_columns('gl_journal_header');
  CALL add_audit_columns('gl_journal_line');
  CALL add_audit_columns('inventory_ledger');
  CALL add_audit_columns('item_master');
  CALL add_audit_columns('lot');
  CALL add_audit_columns('lot_unit');
  CALL add_audit_columns('purchase_order_detail');
  CALL add_audit_columns('purchase_order_header');
  CALL add_audit_columns('shipment_lot_alloc');
  CALL add_audit_columns('shipment_unit_alloc');
  CALL add_audit_columns('shipments');
  CALL add_audit_columns('shipments_detail');
  CALL add_audit_columns('shipments_header');
  CALL add_audit_columns('subscription');
  CALL add_audit_columns('subscriptions');
  CALL add_audit_columns('supplier_master');
  CALL add_audit_columns('users');
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `add_audit_columns` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = cp932 */ ;
/*!50003 SET character_set_results = cp932 */ ;
/*!50003 SET collation_connection  = cp932_japanese_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `add_audit_columns`(IN p_table VARCHAR(64))
BEGIN
  DECLARE v_exists INT DEFAULT 0;

  
  SELECT COUNT(*) INTO v_exists
  FROM information_schema.columns
  WHERE table_schema = DATABASE()
    AND table_name = p_table
    AND column_name = 'created_at';
  IF v_exists = 0 THEN
    SET @sql = CONCAT('ALTER TABLE `', p_table, '` ',
                      'ADD COLUMN `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP');
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
  END IF;

  
  SELECT COUNT(*) INTO v_exists
  FROM information_schema.columns
  WHERE table_schema = DATABASE()
    AND table_name = p_table
    AND column_name = 'updated_at';
  IF v_exists = 0 THEN
    SET @sql = CONCAT('ALTER TABLE `', p_table, '` ',
                      'ADD COLUMN `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ',
                      'ON UPDATE CURRENT_TIMESTAMP');
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
  END IF;

  
  SELECT COUNT(*) INTO v_exists
  FROM information_schema.columns
  WHERE table_schema = DATABASE()
    AND table_name = p_table
    AND column_name = 'created_by';
  IF v_exists = 0 THEN
    SET @sql = CONCAT('ALTER TABLE `', p_table, '` ',
                      'ADD COLUMN `created_by` BIGINT NULL');
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
  END IF;

  
  SELECT COUNT(*) INTO v_exists
  FROM information_schema.columns
  WHERE table_schema = DATABASE()
    AND table_name = p_table
    AND column_name = 'updated_by';
  IF v_exists = 0 THEN
    SET @sql = CONCAT('ALTER TABLE `', p_table, '` ',
                      'ADD COLUMN `updated_by` BIGINT NULL');
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
  END IF;

  
  
  SELECT COUNT(*) INTO v_exists
  FROM information_schema.statistics
  WHERE table_schema = DATABASE()
    AND table_name = p_table
    AND index_name = 'idx_created_at';
  IF v_exists = 0 THEN
    SET @sql = CONCAT('ALTER TABLE `', p_table, '` ',
                      'ADD INDEX `idx_created_at` (`created_at`)');
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
  END IF;

  
  SELECT COUNT(*) INTO v_exists
  FROM information_schema.statistics
  WHERE table_schema = DATABASE()
    AND table_name = p_table
    AND index_name = 'idx_updated_at';
  IF v_exists = 0 THEN
    SET @sql = CONCAT('ALTER TABLE `', p_table, '` ',
                      'ADD INDEX `idx_updated_at` (`updated_at`)');
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
  END IF;

  
  SELECT COUNT(*) INTO v_exists
  FROM information_schema.statistics
  WHERE table_schema = DATABASE()
    AND table_name = p_table
    AND index_name = 'idx_created_by';
  IF v_exists = 0 THEN
    SET @sql = CONCAT('ALTER TABLE `', p_table, '` ',
                      'ADD INDEX `idx_created_by` (`created_by`)');
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
  END IF;

  
  SELECT COUNT(*) INTO v_exists
  FROM information_schema.statistics
  WHERE table_schema = DATABASE()
    AND table_name = p_table
    AND index_name = 'idx_updated_by';
  IF v_exists = 0 THEN
    SET @sql = CONCAT('ALTER TABLE `', p_table, '` ',
                      'ADD INDEX `idx_updated_by` (`updated_by`)');
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
  END IF;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-02-24 15:51:10
