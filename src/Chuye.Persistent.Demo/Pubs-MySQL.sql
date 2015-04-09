DROP DATABASE IF EXISTS Pubs;
CREATE DATABASE Pubs;
USE Pubs;

drop table if exists `Employee`;

create table `Employee` (
    Id INTEGER NOT NULL AUTO_INCREMENT,
    Name VARCHAR(255) not null,
    Birth DATETIME not null,
    Address VARCHAR(255),
    JobId INTEGER,
    primary key (Id)
);

drop table if exists `Job`;
create table `Job` (
    Id INTEGER NOT NULL AUTO_INCREMENT,
    Title VARCHAR(255) not null,
    Salary NUMERIC(19,5) not null,
    primary key (Id)
);

drop table if exists EmployeeSalary;
create table `EmployeeSalary` (
	EmployeeId INTEGER PRIMARY KEY,
	Salary NUMERIC(19,5) not null
);