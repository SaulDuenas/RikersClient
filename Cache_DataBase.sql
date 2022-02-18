-- Script Date: 17/02/2022 11:16 p. m.  - ErikEJ.SqlCeScripting version 3.5.2.90
-- Database information:
-- Locale Identifier: 2058
-- Encryption Mode: 
-- Case Sensitive: False
-- Database: C:\Citi\RikersInterfaceService\cache\DataService.sdf
-- ServerVersion: 4.0.8876.1
-- DatabaseSize: 128 KB
-- SpaceAvailable: 3.999 GB
-- Created: 21/12/2021 05:55 p. m.

-- User Table information:
-- Number of tables: 4
-- CommentFile: 0 row(s)
-- StatusFile: 6 row(s)
-- TicketAttempt: 9 row(s)
-- TicketFile: 2 row(s)

CREATE TABLE [TicketAttempt] (
  [Id] int IDENTITY (1,1) NOT NULL
, [NoTicket] nvarchar(100) NOT NULL
, [NAttempt] int NOT NULL
, [Response] int NOT NULL
, [Type] nvarchar(50) NULL
, [Code] nvarchar(50) NULL
, [Message] nvarchar(1000) NULL
, [TransactionId] nvarchar(100) NOT NULL
, [TransactionDate] datetime NOT NULL
, [DateResponse] datetime NOT NULL
);
GO
CREATE TABLE [StatusFile] (
  [Id] int NOT NULL
, [Name] nvarchar(100) NULL
, [Description] nvarchar(500) NULL
);
GO
CREATE TABLE [TicketFile] (
  [FileName] nvarchar(100) NOT NULL
, [FullPath] nvarchar(300) NULL
, [DateCreate] datetime NULL
, [DateModified] datetime NULL
, [Length] bigint NULL
, [Status] int NULL
, [NoTicket] nvarchar(25) NULL
, [Attempts] int DEFAULT (((0))) NOT NULL
, [Response] int NULL
, [CaseNumber] nvarchar(100) NULL
, [TransactionId] nvarchar(100) NULL
, [TransactionDate] datetime NULL
, [DateResponse] datetime NULL
, [FileResponseCreated] tinyint DEFAULT (((0))) NULL
, [FullPathResponse] nvarchar(200) NULL
, [DateNextAttempt] datetime NULL
, [FileMove] tinyint DEFAULT ((0)) NULL
, [Message] nvarchar(1000) NULL
, [Processed] tinyint DEFAULT ((0)) NULL
);
GO
CREATE TABLE [CommentFile] (
  [FileName] nvarchar(100) NOT NULL
, [FullPath] nvarchar(300) NULL
, [DateCreate] datetime NULL
, [DateModified] datetime NULL
, [Length] bigint NULL
, [Status] int NULL
, [NoTicket] nvarchar(25) NULL
, [CaseNumber] nvarchar(100) NULL
, [Attempts] int DEFAULT (((0))) NOT NULL
, [Response] int NULL
, [TransactionId] nvarchar(100) NULL
, [TransactionDate] datetime NULL
, [DateResponse] datetime NULL
, [FileResponseCreated] tinyint DEFAULT (((0))) NULL
, [FullPathResponse] nvarchar(200) NULL
, [DateNextAttempt] datetime NULL
, [FileMove] tinyint NULL
, [Message] nvarchar(1000) NULL
, [Processed] tinyint DEFAULT ((0)) NULL
);
GO
ALTER TABLE [TicketAttempt] ADD CONSTRAINT [PK_TicketAttempt] PRIMARY KEY ([Id],[NoTicket],[NAttempt]);
GO
ALTER TABLE [StatusFile] ADD CONSTRAINT [PK_STATUSFILE] PRIMARY KEY ([Id]);
GO
ALTER TABLE [TicketFile] ADD CONSTRAINT [PK_TICKETFILE] PRIMARY KEY ([FileName]);
GO
ALTER TABLE [CommentFile] ADD CONSTRAINT [PK_COMMENTFILE] PRIMARY KEY ([FileName]);
GO
ALTER TABLE [TicketFile] ADD CONSTRAINT [FK_TICKETFI_REFERENCE_STATUSFI] FOREIGN KEY ([Status]) REFERENCES [StatusFile]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [CommentFile] ADD CONSTRAINT [FK_COMMENTF_REFERENCE_STATUSFI] FOREIGN KEY ([Status]) REFERENCES [StatusFile]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO