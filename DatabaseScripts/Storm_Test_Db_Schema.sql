--------------------------
-- Storm_Test_Db Schema
-------------------------- 
CREATE DATABASE Storm_Test_Db;

GO;

USE Storm_Test_Db;

GO;

-- employee table schema
CREATE TABLE employee
(
    employee_id bigint IDENTITY(1,1) PRIMARY KEY,
    first_name nvarchar(100) null,
    last_name nvarchar(100) null,
    birth_date datetime null,
    salary decimal(18,2) null,
    is_active bit not null DEFAULT(1),
    is_deleted bit not null DEFAULT(0)    
);


