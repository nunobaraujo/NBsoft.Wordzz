CREATE DATABASE IF NOT EXISTS nbsoftwordzz;
USE nbsoftwordzz;

CREATE TABLE `Session` ( 
	SessionToken		char(255)     	NOT NULL UNIQUE PRIMARY KEY,
	UserId				char(128)     	NOT NULL,
	UserInfo			text            NOT NULL, 
	Registered			datetime        NOT NULL,
	LastAction			datetime        NOT NULL,
    Expired			    datetime        NULL
);
CREATE TABLE `SessionHistory` ( 
	SessionToken		char(255)     	NOT NULL UNIQUE PRIMARY KEY,
	UserId				char(128)     	NOT NULL,
	UserInfo			text            NOT NULL, 
	Registered			datetime        NOT NULL,
	LastAction			datetime        NOT NULL,
    Expired			    datetime        NULL
);

CREATE TABLE `User` (
    UserName			char(128)     	NOT NULL UNIQUE PRIMARY KEY,
    CreationDate		datetime        NOT NULL,
    PasswordHash		text            NOT NULL,
    Salt				text            NOT NULL,
    Deleted			    bit             NOT NULL        default 0,
    INDEX user_index (username)
);

CREATE TABLE `UserSettings` (
    UserName			char(128)     	NOT NULL UNIQUE PRIMARY KEY,
	MainSettings 	    text 			NULL,
    WindowsSettings 	text 			NULL,
    AndroidSettings 	text 			NULL,
    IOSSettings 	    text 			NULL,
	INDEX user_index (username),
	FOREIGN KEY (username) 		REFERENCES User(UserName)	ON DELETE RESTRICT
);

CREATE TABLE `UserDetails` (
    UserName			char(128)     	NOT NULL UNIQUE PRIMARY KEY,
    FirstName			text     	        NULL,
    LastName			text     	        NULL,
    Address				text              	NULL,
    PostalCode			text              	NULL,
    City				text              	NULL,
    Country				text              	NULL,
	Email				text              	NULL,	
    INDEX user_index (username),
	FOREIGN KEY (username) 		REFERENCES User(UserName)	ON DELETE RESTRICT
);

CREATE TABLE `Lexicon` (
    Language			char(64)     	    NOT NULL UNIQUE PRIMARY KEY,
    CreationDate		datetime            NOT NULL,
    Description			text     	        NOT NULL,    
    INDEX lexicon_index (language)
);

CREATE TABLE `Word` (
    Id                  int UNSIGNED        AUTO_INCREMENT UNIQUE PRIMARY KEY,
    Language			char(8)     	    NOT NULL,
    Name			    char(255)     	    NOT NULL,        
    Description			text     	        NULL,        
    INDEX word_index (Language),
    INDEX wordl_index (Name(16)),
    FOREIGN KEY (Language) 		REFERENCES Lexicon(Language)	ON DELETE RESTRICT
);

CREATE TABLE `UserContacts` (
    Id                  int UNSIGNED        AUTO_INCREMENT UNIQUE PRIMARY KEY,
    UserName			char(128)     	    NOT NULL,
    Contact     		char(128)           NOT NULL,
    INDEX usercontacts_index (username),
    FOREIGN KEY (username) 		REFERENCES User(UserName)	ON DELETE CASCADE 
);

CREATE TABLE `Board` (
    Id                  int                 AUTO_INCREMENT UNIQUE PRIMARY KEY,
    Name			    char(128)     	    NOT NULL,
    Rows     		    int                 NOT NULL,
    Columns     		int                 NOT NULL
);
CREATE TABLE `Tile` (
    Id                  int                 AUTO_INCREMENT UNIQUE PRIMARY KEY,
    BoardId             int     	        NOT NULL,
    X			        int     	        NOT NULL,
    Y     		        int                 NOT NULL,
    Bonus     		    int                 NOT NULL,
    FOREIGN KEY (BoardId) 		REFERENCES Board(Id)	ON DELETE RESTRICT 

);

CREATE TABLE `Game` (
    Id		            char(36)     	    NOT NULL UNIQUE PRIMARY KEY,
    BoardId             int     	        NOT NULL,
    Language			char(8)     	    NOT NULL,
    CreationDate		datetime            NOT NULL,
    Status		        int                 NOT NULL,
    CurrentPlayer       char(128)           NOT NULL,
    CurrentStart	    datetime            NOT NULL,
    CurrentPauseStart   datetime            NULL,
    WEQJKWHEQJWEGQJEGQJ
    FOREIGN KEY (BoardId) 		REFERENCES Board(Id)	ON DELETE RESTRICT 
);