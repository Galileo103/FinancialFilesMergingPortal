﻿CREATE TABLE Users
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [Username] VARCHAR(50) NOT NULL, 
    [Password] VARCHAR(50) NOT NULL, 
    [Email] VARCHAR(50) NULL
);