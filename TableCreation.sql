DROP TABLE Appointment;
DROP TABLE [Grooming Room];
DROP TABLE [Appointment Type];
DROP TABLE Staff;
DROP TABLE Dog;
DROP TABLE Contact;
DROP TABLE Client;

CREATE TABLE [Client] (
	[Client ID] INT NOT NULL PRIMARY KEY,
	[Client Notes] VARCHAR(100),
	[Client Join Date] DATE
);

CREATE TABLE [Contact] (
	[Contact ID] INT NOT NULL PRIMARY KEY,
	[Client ID] INT NOT NULL FOREIGN KEY REFERENCES [Client] ON DELETE CASCADE,
	[Client Name] VARCHAR(30) NOT NULL,
	[Client Email] VARCHAR(40),
	[Client Address] VARCHAR(100),
	[Client Postcode] VARCHAR(10),
	[Client Phone No] VARCHAR(20)
);

CREATE TABLE [Dog] (
	[Dog ID] INT NOT NULL PRIMARY KEY,
	[Client ID] INT NOT NULL REFERENCES [Client] ON DELETE CASCADE,
	[Dog Name] VARCHAR(20) NOT NULL,
	[Dog DOB] DATE,
	[Dog Gender] VARCHAR(1),
	[Dog Type] VARCHAR(40)
);

CREATE TABLE [Staff] (
	[Staff ID] INT NOT NULL PRIMARY KEY,
	[Staff Name] VARCHAR(20) NOT NULL,
	[Staff Password] VARCHAR(64) NOT NULL,
	[Password Salt] VARCHAR(32),
	[Staff Email] VARCHAR(128),
	[Staff Phone No] VARCHAR(15), 
	[Staff Uses 2FA] BIT NOT NULL
);

INSERT INTO [Staff] VALUES (
	0,
	'Dianne',
	'9693a6491db493c87133af8dc801dd04b6c73ca2fdf92a6443d5e5066992d1f3',
	'',
	'atempmailfortestingcsharp@gmail.com',
	'07700 900730',
	1
);

INSERT INTO [Staff] VALUES (
	1,
	'Jon',
	'5e0524de4400e6875b14c07da5731796166037f54bc87043ab9abd98c3d33036',
	'',
	'not.a@real.email',
	'07700 900747',
	0
);

INSERT INTO [Staff] VALUES (
	2,
	'Elaine',
	'5e0524de4400e6875b14c07da5731796166037f54bc87043ab9abd98c3d33036',
	'',
	'',
	'07700 900835',
	0
);

INSERT INTO [Staff] VALUES (
	3,
	'Richard',
	'5e0524de4400e6875b14c07da5731796166037f54bc87043ab9abd98c3d33036',
	'',
	'',
	'07700 900042',
	0
);

INSERT INTO [Staff] VALUES (
	4,
	'Jane',
	'5e0524de4400e6875b14c07da5731796166037f54bc87043ab9abd98c3d33036',
	'',
	'',
	'07700 900276',
	0
);

CREATE TABLE [Appointment Type] (
	[Appointment Type ID] INT NOT NULL PRIMARY KEY,
	[Base Time Taken] DECIMAL (10, 5) NOT NULL,
	[Base Price] DECIMAL (10, 2) NOT NULL,
	[Description] VARCHAR(100) NOT NULL
);

INSERT INTO [Appointment Type] VALUES (
	0,
	1,
	35,
	'Wash, Shampoo & Brush'
);

INSERT INTO [Appointment Type] VALUES (
	1,
	1.5,
	40,
	'Wash, Shampoo, Brush & Trim'
);

INSERT INTO [Appointment Type] VALUES (
	2,
	2,
	50,
	'Wash, Shampoo, Brush & Full Cut'
);

CREATE TABLE [Grooming Room] (
	[Grooming Room ID] INT NOT NULL PRIMARY KEY,
	[Grooming Room Name] VARCHAR(20) NOT NULL,
	[Grooming Room Notes] VARCHAR(50)
);

INSERT INTO [Grooming Room] VALUES (
	0,
	'Shed',
	'Ideal for smaller dogs.'
);

INSERT INTO [Grooming Room] VALUES (
	1,
	'Barn 1',
	'Fit for all dogs.'
);

INSERT INTO [Grooming Room] VALUES (
	2,
	'Barn 2',
	'Fit for all dogs, ideal for big dogs.'
);

CREATE TABLE [Appointment] (
	[Appointment ID] INT NOT NULL PRIMARY KEY,
	[Dog ID] INT NOT NULL FOREIGN KEY REFERENCES [Dog] ON DELETE CASCADE,
	[Appointment Type ID] INT NOT NULL FOREIGN KEY REFERENCES [Appointment Type] ON DELETE CASCADE,
	[Staff ID] INT NOT NULL FOREIGN KEY REFERENCES [Staff] ON DELETE CASCADE,
	[Is Initial Appointment] BIT NOT NULL,
	[Includes Nail And Teeth] BIT NOT NULL,
	[Is Cancelled] BIT NOT NULL,
	[Is Paid] BIT NOT NULL,
	[Booked In Advance Discount] DECIMAL(5,2),
	[Appointment Date] DATE NOT NULL,
	[Appointment Time] TIME NOT NULL,
	[Last Invoice Date] DATE,
	[Last Invoice Time] TIME,
	[Invoices Sent] INT NOT NULL,
	[Grooming Room ID] INT NOT NULL FOREIGN KEY REFERENCES [Grooming Room] ON DELETE CASCADE
);