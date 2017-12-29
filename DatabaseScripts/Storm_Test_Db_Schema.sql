--===========================
-- Storm_Test_Db Schema
-- Created By: Niraj Rai
-- Created On: 28-12-2017
--=========================== 
-----------------------------
-- Storm_Test_Db Database
-----------------------------
DROP DATABASE IF EXISTS [Storm_Test_Db]
GO;
CREATE DATABASE [Storm_Test_Db];
GO;

USE [Storm_Test_Db];
GO;

-----------------------------
-- employee table schema
-----------------------------
DROP TABLE IF EXISTS [employee]
GO;
CREATE TABLE [employee]
(
    employee_id bigint IDENTITY(1,1) PRIMARY KEY,
    first_name nvarchar(100) null,
    last_name nvarchar(100) null,
    birth_date datetime null,
    salary decimal(18,2) null,
    is_active bit not null DEFAULT(1),
    is_deleted bit not null DEFAULT(0)    
);


