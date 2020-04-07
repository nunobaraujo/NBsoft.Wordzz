CREATE DATABASE IF NOT EXISTS nbsoftwordzz;
USE nbsoftwordzz;

CREATE TABLE `UserSession` ( 
	SessionToken		nvarchar(255)     	NOT NULL UNIQUE PRIMARY KEY,
	UserId				nvarchar(128)     	NOT NULL,
	UserInfo			text              	NOT NULL, 
	Registered			datetime          	NOT NULL,
	LastAction			datetime          	NOT NULL,
	ActiveCompany 		nvarchar(255)  		NULL
);