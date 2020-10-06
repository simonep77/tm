/*
SQLyog Community v13.1.6 (64 bit)
MySQL - 8.0.21 : Database - task_management
*********************************************************************
*/

/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
CREATE DATABASE /*!32312 IF NOT EXISTS*/`task_management` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `task_management`;

/*Table structure for table `taskdefinizioni` */

DROP TABLE IF EXISTS `taskdefinizioni`;

CREATE TABLE `taskdefinizioni` (
  `Id` int NOT NULL,
  `Nome` varchar(100) NOT NULL,
  `Attivo` tinyint NOT NULL DEFAULT '1',
  `SistemaId` smallint NOT NULL,
  `TipoTaskId` smallint NOT NULL,
  `AssemblyPath` varchar(300) NOT NULL,
  `TaskClass` varchar(100) NOT NULL,
  `LogDir` varchar(250) NOT NULL,
  `DataDir` varchar(250) NOT NULL,
  `MostraConsole` tinyint NOT NULL DEFAULT '1',
  `TipoNotificaId` smallint NOT NULL,
  `MailFROM` text,
  `MailTO` text,
  `MailCC` text,
  `MailBCC` text,
  `Riferimento` varchar(100) DEFAULT NULL,
  `Note` text,
  `MantieniNumLogDB` int NOT NULL DEFAULT '60',
  `MantieniNumLogFS` int NOT NULL DEFAULT '60',
  `DataInizio` date NOT NULL DEFAULT '2001-01-01',
  `DataFine` date NOT NULL DEFAULT '9999-12-31',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Nome` (`Nome`),
  KEY `SistemaId` (`SistemaId`),
  KEY `TipoNotificaId` (`TipoNotificaId`),
  KEY `TipoTaskId` (`TipoTaskId`),
  CONSTRAINT `taskdefinizioni_ibfk_1` FOREIGN KEY (`SistemaId`) REFERENCES `tasksistemi` (`Id`),
  CONSTRAINT `taskdefinizioni_ibfk_2` FOREIGN KEY (`TipoNotificaId`) REFERENCES `tasktipinotifiche` (`Id`),
  CONSTRAINT `taskdefinizioni_ibfk_3` FOREIGN KEY (`TipoTaskId`) REFERENCES `tasktipitask` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `taskdefinizioni` */

/*Table structure for table `taskesecuzioni` */

DROP TABLE IF EXISTS `taskesecuzioni`;

CREATE TABLE `taskesecuzioni` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `TaskDefId` int NOT NULL,
  `SchedPianoId` bigint DEFAULT NULL,
  `StatoEsecuzioneId` smallint NOT NULL,
  `Host` varchar(100) NOT NULL,
  `Pid` varchar(20) NOT NULL,
  `ReturnCode` int NOT NULL DEFAULT '0',
  `ReturnMessage` text,
  `NotificaCode` int NOT NULL DEFAULT '0',
  `NotificaMessage` text,
  `DataTermine` datetime DEFAULT NULL,
  `DataInserimento` datetime NOT NULL,
  `DataAggiornamento` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `TaskDefId` (`TaskDefId`),
  KEY `StatoEsecuzioneId` (`StatoEsecuzioneId`),
  KEY `SchedPianoId` (`SchedPianoId`),
  CONSTRAINT `taskesecuzioni_ibfk_1` FOREIGN KEY (`TaskDefId`) REFERENCES `taskdefinizioni` (`Id`),
  CONSTRAINT `taskesecuzioni_ibfk_2` FOREIGN KEY (`StatoEsecuzioneId`) REFERENCES `taskstatiesecuzione` (`Id`),
  CONSTRAINT `taskesecuzioni_ibfk_3` FOREIGN KEY (`SchedPianoId`) REFERENCES `taskschedulazioni_piano` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `taskesecuzioni` */

/*Table structure for table `taskparametri` */

DROP TABLE IF EXISTS `taskparametri`;

CREATE TABLE `taskparametri` (
  `TaskDefId` int NOT NULL,
  `Chiave` varchar(50) NOT NULL,
  `Valore` varchar(500) NOT NULL,
  `Visibile` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`TaskDefId`,`Chiave`),
  CONSTRAINT `taskparametri_ibfk_1` FOREIGN KEY (`TaskDefId`) REFERENCES `taskdefinizioni` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `taskparametri` */

/*Table structure for table `taskschedulazioni` */

DROP TABLE IF EXISTS `taskschedulazioni`;

CREATE TABLE `taskschedulazioni` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TaskDefId` int NOT NULL COMMENT 'Definizione del task da eseguire',
  `Attivo` tinyint NOT NULL DEFAULT '1',
  `CronString` varchar(100) NOT NULL COMMENT 'Stringa cron con il dettaglio della schedulazione',
  `Host` varchar(150) DEFAULT NULL COMMENT 'Eventuale nome host su cui è vincolata l''esecuzione',
  `SubTaskDefList` varchar(150) DEFAULT NULL COMMENT 'Eventuali task da eseguire in successione a quello corrente separati da , o ;',
  PRIMARY KEY (`Id`),
  KEY `TaskDefId` (`TaskDefId`),
  CONSTRAINT `taskschedulazioni_ibfk_1` FOREIGN KEY (`TaskDefId`) REFERENCES `taskdefinizioni` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `taskschedulazioni` */

/*Table structure for table `taskschedulazioni_piano` */

DROP TABLE IF EXISTS `taskschedulazioni_piano`;

CREATE TABLE `taskschedulazioni_piano` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `SchedulazioneId` int NOT NULL,
  `DataEsecuzione` datetime NOT NULL,
  `StatoEsecuzioneId` smallint NOT NULL,
  `DataInserimento` datetime NOT NULL,
  `DataAggiornamento` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `SchedulazioneId` (`SchedulazioneId`,`DataEsecuzione`),
  CONSTRAINT `taskschedulazioni_piano_ibfk_1` FOREIGN KEY (`SchedulazioneId`) REFERENCES `taskschedulazioni` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `taskschedulazioni_piano` */

/*Table structure for table `tasksistemi` */

DROP TABLE IF EXISTS `tasksistemi`;

CREATE TABLE `tasksistemi` (
  `Id` smallint NOT NULL,
  `Nome` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `tasksistemi` */

/*Table structure for table `taskstatiesecuzione` */

DROP TABLE IF EXISTS `taskstatiesecuzione`;

CREATE TABLE `taskstatiesecuzione` (
  `Id` smallint NOT NULL,
  `Nome` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `taskstatiesecuzione` */

insert  into `taskstatiesecuzione`(`Id`,`Nome`) values 
(1,'In esecuzione'),
(2,'Terminata con successo'),
(3,'Terminata con errori'),
(1000,'Non eseguita'),
(1001,'In attesa di esecuzione'),
(1002,'In esecuzione'),
(1003,'Terminata con successo'),
(1004,'Terminata con errori');

/*Table structure for table `tasktipifile` */

DROP TABLE IF EXISTS `tasktipifile`;

CREATE TABLE `tasktipifile` (
  `Id` smallint NOT NULL,
  `Nome` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `tasktipifile` */

insert  into `tasktipifile`(`Id`,`Nome`) values 
(1,'File di log'),
(2,'File utente');

/*Table structure for table `tasktipinotifiche` */

DROP TABLE IF EXISTS `tasktipinotifiche`;

CREATE TABLE `tasktipinotifiche` (
  `Id` smallint NOT NULL,
  `Nome` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `tasktipinotifiche` */

insert  into `tasktipinotifiche`(`Id`,`Nome`) values 
(1,'Nessuna'),
(2,'Email');

/*Table structure for table `tasktipitask` */

DROP TABLE IF EXISTS `tasktipitask`;

CREATE TABLE `tasktipitask` (
  `Id` smallint NOT NULL,
  `Nome` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `tasktipitask` */

insert  into `tasktipitask`(`Id`,`Nome`) values 
(1,'Classe Interfaccia .NET'),
(2,'Eseguibile esterno');

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
