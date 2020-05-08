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
    BoardRows     		int                 NOT NULL,
    BoardColumns        int                 NOT NULL
);
CREATE TABLE `BoardTile` (
    Id                  int                 AUTO_INCREMENT UNIQUE PRIMARY KEY,
    BoardId             int     	        NOT NULL,
    X			        int     	        NOT NULL,
    Y     		        int                 NOT NULL,
    Bonus     		    int                 NOT NULL,
    FOREIGN KEY (BoardId) 		REFERENCES Board(Id)	ON DELETE RESTRICT 
);

CREATE TABLE `Game` (
    Id		            char(32)     	    NOT NULL UNIQUE PRIMARY KEY,
    BoardId             int     	        NOT NULL,
    Language			char(8)     	    NOT NULL,
    CreationDate		datetime            NOT NULL,
    Player01            char(128)     	    NOT NULL,
    Player01Rack        char(32)     	    NOT NULL,
    Player02            char(128)     	    NOT NULL,
    Player02Rack        char(32)     	    NOT NULL,
    Status		        int                 NOT NULL,
    CurrentPlayer       char(128)           NOT NULL,
    CurrentStart	    datetime            NOT NULL,
    CurrentPauseStart   datetime            NULL,
    LetterBag           text                NOT NULL,
    Winner              char(128)     	    NULL,
    FinishReason        int                 NULL,
    ConsecutivePasses   int                 NOT NULL,
    FinishDate          datetime            NULL,
    P1FinalScore        int                 NOT NULL,
    P2FinalScore        int                 NOT NULL,
    FOREIGN KEY (BoardId) 		REFERENCES Board(Id)	ON DELETE RESTRICT,
    FOREIGN KEY (Player01) 		REFERENCES User(UserName)	ON DELETE RESTRICT,
    FOREIGN KEY (Player02) 		REFERENCES User(UserName)	ON DELETE RESTRICT 
);
CREATE TABLE `GameMove` (
    Id                  int UNSIGNED        AUTO_INCREMENT UNIQUE PRIMARY KEY,
    GameId              char(32)  	        NOT NULL,
    PlayerId    		char(128)     	    NOT NULL,
    PlayStart           datetime            NOT NULL,
    PlayFinish          datetime            NULL,
    Score		        int                 NOT NULL,
    Letters             text              	NULL,
    Words               text              	NULL,
    INDEX move_player_index (PlayerId),
    FOREIGN KEY (GameId) 		REFERENCES Game(Id)	ON DELETE RESTRICT,
    FOREIGN KEY (PlayerId) 		REFERENCES User(UserName)	ON DELETE RESTRICT 
);

CREATE TABLE `UserStats` (
    UserName			    char(128)     	    NOT NULL UNIQUE PRIMARY KEY,
    GamesPlayed    		    int UNSIGNED        NOT NULL,
    Victories    		    int UNSIGNED        NOT NULL,
    Defeats    		        int UNSIGNED        NOT NULL,
    Draws    		        int UNSIGNED        NOT NULL,
    TotalScore    		    int UNSIGNED        NOT NULL,
    HighScoreGame  		    int UNSIGNED        NOT NULL,
    HighScoreGameOpponent   char(128)     	    NULL,
    HighScorePlay  		    int UNSIGNED        NOT NULL,
    HighScorePlayOpponent   char(128)     	    NULL,
    HighScoreWord  		    int UNSIGNED        NOT NULL,
    HighScoreWordName       char(255)     	    NULL,
    HighScoreWordOpponent   char(128)     	    NULL,
    MostUsedWord            char(128)           NULL,
    MostFrequentOpponent    char(128)           NULL,
    Forfeits                int UNSIGNED        NOT NULL
);
