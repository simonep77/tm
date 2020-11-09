/*
SQLyog Community v13.1.7 (64 bit)
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
  `DatiDir` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `MostraConsole` tinyint(1) NOT NULL DEFAULT '1',
  `TipoNotificaId` smallint NOT NULL,
  `MailFROM` text,
  `MailTO` text,
  `MailCC` text,
  `MailBCC` text,
  `Riferimento` varchar(100) DEFAULT NULL,
  `Note` text,
  `MantieniNumLogDB` int NOT NULL DEFAULT '60',
  `MantieniNumLogFS` int NOT NULL DEFAULT '60',
  `Eliminato` tinyint(1) NOT NULL DEFAULT '0',
  `SchedCronString` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `SchedNodoId` int DEFAULT NULL COMMENT 'Id del nodo slave che deve eseguire il task schedulato',
  `DataInizio` date NOT NULL DEFAULT '2001-01-01',
  `DataFine` date NOT NULL DEFAULT '9999-12-31',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Nome` (`Nome`),
  KEY `SistemaId` (`SistemaId`),
  KEY `TipoNotificaId` (`TipoNotificaId`),
  KEY `TipoTaskId` (`TipoTaskId`),
  KEY `SchedNodoId` (`SchedNodoId`),
  CONSTRAINT `taskdefinizioni_ibfk_1` FOREIGN KEY (`SistemaId`) REFERENCES `tasksistemi` (`Id`),
  CONSTRAINT `taskdefinizioni_ibfk_2` FOREIGN KEY (`TipoNotificaId`) REFERENCES `tasktipinotifiche` (`Id`),
  CONSTRAINT `taskdefinizioni_ibfk_3` FOREIGN KEY (`TipoTaskId`) REFERENCES `tasktipitask` (`Id`),
  CONSTRAINT `taskdefinizioni_ibfk_4` FOREIGN KEY (`SchedNodoId`) REFERENCES `tasknodi` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `taskdefinizioni` */

insert  into `taskdefinizioni`(`Id`,`Nome`,`Attivo`,`SistemaId`,`TipoTaskId`,`AssemblyPath`,`TaskClass`,`LogDir`,`DatiDir`,`MostraConsole`,`TipoNotificaId`,`MailFROM`,`MailTO`,`MailCC`,`MailBCC`,`Riferimento`,`Note`,`MantieniNumLogDB`,`MantieniNumLogFS`,`Eliminato`,`SchedCronString`,`SchedNodoId`,`DataInizio`,`DataFine`) values 
(1,'TaskProva',1,1,1,'E:\\applicazioni_data\\task_management\\assembly\\TaskProva\\TaskEsempio.dll','TaskEsempio.TaskProva','E:\\applicazioni_data\\task_management\\log\\TaskProva','E:\\applicazioni_data\\task_management\\dati\\TaskProva',1,1,NULL,NULL,NULL,NULL,'Simone Pelaia',NULL,1,1,0,NULL,NULL,'2001-01-01','9999-12-31'),
(2,'TaskNetstat',1,1,2,'netstat.exe','-ano','E:\\applicazioni_data\\task_management\\log\\TaskEsterno','E:\\applicazioni_data\\task_management\\dati\\TaskEsterno',1,1,NULL,NULL,NULL,NULL,'Simone pelaia',NULL,5,5,0,NULL,NULL,'2001-01-01','9999-12-31'),
(3,'TaskJobProva',1,1,3,'','','E:\\applicazioni_data\\task_management\\log\\TaskJob','E:\\applicazioni_data\\task_management\\dati\\Taskjob',1,1,NULL,NULL,NULL,NULL,'Simone pelaia',NULL,5,5,0,NULL,NULL,'2001-01-01','9999-12-31');
/*Table structure for table `taskdettaglijob` */

DROP TABLE IF EXISTS `taskdettaglijob`;

CREATE TABLE `taskdettaglijob` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `JobTaskDefId` int NOT NULL,
  `SubTaskDefId` int NOT NULL,
  `Progressivo` int NOT NULL,
  `AbilitaNotifiche` tinyint NOT NULL DEFAULT '0',
  `Attivo` tinyint NOT NULL DEFAULT '1',
  `MinPredReturnCode` smallint NOT NULL DEFAULT '999' COMMENT 'Indica il return code minimo del predecessore per potersi avviare',
  `Asincrono` tinyint NOT NULL DEFAULT '0',
  `DataInizio` date NOT NULL DEFAULT '2001-01-01',
  `DataFine` date NOT NULL DEFAULT '9999-12-31',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `JobTaskDefId` (`JobTaskDefId`,`Progressivo`),
  KEY `SubTaskDefId` (`SubTaskDefId`),
  CONSTRAINT `taskdettaglijob_ibfk_1` FOREIGN KEY (`JobTaskDefId`) REFERENCES `taskdefinizioni` (`Id`),
  CONSTRAINT `taskdettaglijob_ibfk_2` FOREIGN KEY (`SubTaskDefId`) REFERENCES `taskdefinizioni` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `taskdettaglijob` */

insert  into `taskdettaglijob`(`Id`,`JobTaskDefId`,`SubTaskDefId`,`Progressivo`,`AbilitaNotifiche`,`Attivo`,`MinPredReturnCode`,`Asincrono`,`DataInizio`,`DataFine`) values 
(1,3,1,1,0,1,999,0,'2001-01-01','9999-12-31'),
(2,3,2,2,0,1,999,0,'2001-01-01','9999-12-31');

/*Table structure for table `taskesecuzioni` */

DROP TABLE IF EXISTS `taskesecuzioni`;

CREATE TABLE `taskesecuzioni` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `TaskDefId` int NOT NULL,
  `SchedPianoId` bigint DEFAULT NULL,
  `JobEsecuzioneId` bigint DEFAULT NULL,
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

/*Table structure for table `taskfiles` */

DROP TABLE IF EXISTS `taskfiles`;

CREATE TABLE `taskfiles` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `TaskEsecuzioneId` bigint NOT NULL,
  `TipoFileId` smallint NOT NULL,
  `FileName` varchar(150) NOT NULL,
  `FileData` longblob NOT NULL,
  `DataInserimento` datetime NOT NULL,
  `DataAggiornamento` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `TaskEsecuzioneId` (`TaskEsecuzioneId`),
  KEY `TipoFileId` (`TipoFileId`),
  CONSTRAINT `taskfiles_ibfk_1` FOREIGN KEY (`TaskEsecuzioneId`) REFERENCES `taskesecuzioni` (`Id`),
  CONSTRAINT `taskfiles_ibfk_2` FOREIGN KEY (`TipoFileId`) REFERENCES `tasktipifile` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `taskfiles` */

/*Table structure for table `tasknodi` */

DROP TABLE IF EXISTS `tasknodi`;

CREATE TABLE `tasknodi` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Hostname` varchar(50) NOT NULL,
  `FQDN` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Note` text,
  `RunStart` datetime DEFAULT NULL,
  `RunEnd` datetime DEFAULT NULL,
  `RunPID` varchar(10) DEFAULT NULL,
  `DataAggiornamento` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Hostname` (`Hostname`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `tasknodi` */

/*Table structure for table `taskparametri` */

DROP TABLE IF EXISTS `taskparametri`;

CREATE TABLE `taskparametri` (
  `TaskDefId` int NOT NULL,
  `IsCondiviso` tinyint NOT NULL DEFAULT '0' COMMENT 'Se 1 indica che trattasi di un riferimento ad un parametro condiviso. Il campo valore verrà pertanto ignorato',
  `Chiave` varchar(50) NOT NULL,
  `Valore` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ValoreOpzionale` text,
  `Visibile` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`TaskDefId`,`Chiave`),
  CONSTRAINT `taskparametri_ibfk_1` FOREIGN KEY (`TaskDefId`) REFERENCES `taskdefinizioni` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `taskparametri` */


/*Table structure for table `taskparametricondivisi` */

DROP TABLE IF EXISTS `taskparametricondivisi`;

CREATE TABLE `taskparametricondivisi` (
  `Id` int NOT NULL,
  `Chiave` varchar(100) NOT NULL COMMENT 'Identificativo univoco',
  `Valore` text NOT NULL COMMENT 'A seconda del tipo assume il valore di riferimento',
  `ValoreOpzionale` text COMMENT 'Ulteriore valore a supporto',
  `Note` text,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Chiave` (`Chiave`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `taskparametricondivisi` */


/*Table structure for table `taskschedulazioni_piano` */

DROP TABLE IF EXISTS `taskschedulazioni_piano`;

CREATE TABLE `taskschedulazioni_piano` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `TaskDefId` int NOT NULL,
  `DataEsecuzione` datetime NOT NULL,
  `StatoEsecuzioneId` smallint NOT NULL,
  `IsManuale` tinyint NOT NULL DEFAULT '0',
  `JsonParametriOverride` text COMMENT 'Json che va a forzare i parametri definiti per il task per questa esecuzione. Es. {"Parametri":[{"Chiave": "param1", "Valore": "valore1"}]}',
  `DataInserimento` datetime NOT NULL,
  `DataAggiornamento` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `SchedulazioneId` (`TaskDefId`,`DataEsecuzione`),
  KEY `StatoEsecuzioneId` (`StatoEsecuzioneId`),
  CONSTRAINT `taskschedulazioni_piano_ibfk_1` FOREIGN KEY (`TaskDefId`) REFERENCES `taskdefinizioni` (`Id`),
  CONSTRAINT `taskschedulazioni_piano_ibfk_2` FOREIGN KEY (`StatoEsecuzioneId`) REFERENCES `taskstatiesecuzione` (`Id`)
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

insert  into `tasksistemi`(`Id`,`Nome`) values 
(1,'TEST');

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
(1001,'Pianificata'),
(1002,'In esecuzione'),
(1003,'Terminata con successo'),
(1004,'Terminata con errori'),
(1005,'Saltato (Non verrà eseguita)');

/*Table structure for table `tasktipifile` */

DROP TABLE IF EXISTS `tasktipifile`;

CREATE TABLE `tasktipifile` (
  `Id` smallint NOT NULL,
  `Nome` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/*Data for the table `tasktipifile` */

insert  into `tasktipifile`(`Id`,`Nome`) values 
(1,'File di log');

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
(2,'Eseguibile esterno'),
(3,'Job (esegue una catena di task)');

/* Procedure structure for procedure `clean_db` */

/*!50003 DROP PROCEDURE IF EXISTS  `clean_db` */;

DELIMITER $$

/*!50003 CREATE DEFINER=`root`@`localhost` PROCEDURE `clean_db`()
BEGIN

 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 ;
 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 ;
 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' ;
 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 ;

		 truncate table taskfiles;
		 truncate table taskesecuzioni;
		 TRUNCATE TABLE taskschedulazioni_piano;
		
 SET SQL_MODE=@OLD_SQL_MODE ;
 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS ;
 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS ;
 SET SQL_NOTES=@OLD_SQL_NOTES ;
	END */$$
DELIMITER ;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
