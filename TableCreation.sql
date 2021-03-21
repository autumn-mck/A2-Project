DROP TABLE [Shift Exception];
--DROP TABLE [Shift];
DROP TABLE Appointment;
DROP TABLE Booking;
DROP TABLE [Grooming Room];
DROP TABLE [Appointment Type];
--DROP TABLE [Staff];
DROP TABLE [Dog];
DROP TABLE [Contact];
DROP TABLE [Client];

CREATE TABLE [Client] (
	[Client ID] INT NOT NULL PRIMARY KEY,
	[Client Notes] VARCHAR(127),
	[Client Join Date] DATE NOT NULL,
	[Client Payment Method] VARCHAR(15) NOT NULL
);
	
CREATE TABLE [Contact] (
	[Contact ID] INT NOT NULL PRIMARY KEY,
	[Client ID] INT NOT NULL FOREIGN KEY REFERENCES [Client] ON DELETE CASCADE,
	[Contact Name] VARCHAR(70) NOT NULL,
	[Contact Email] VARCHAR(257),
	[Contact Address] VARCHAR(127),
	[Contact Town] VARCHAR(63),
	[Contact County] VARCHAR(31),
	[Contact Postcode] VARCHAR(8),
	[Contact Phone No] VARCHAR(15)
);

CREATE TABLE [Dog] (
	[Dog ID] INT NOT NULL PRIMARY KEY,
	[Client ID] INT NOT NULL REFERENCES [Client] ON DELETE CASCADE,
	[Dog Name] VARCHAR(30) NOT NULL,
	[Dog DOB] DATE,
	[Dog Gender] VARCHAR(1),
	[Dog Type] VARCHAR(40)
);

--CREATE TABLE [Staff] (
--	[Staff ID] INT NOT NULL PRIMARY KEY,
--	[Staff Name] VARCHAR(70) NOT NULL,
--	[Staff Password] VARCHAR(64) NOT NULL,
--	[Password Salt] VARCHAR(32) NOT NULL,
--	[Staff Email] VARCHAR(257),
--	[Staff Phone No] VARCHAR(15), 
--	[Staff Uses 2FA] BIT NOT NULL
--);

--INSERT INTO [Staff] VALUES (
--	0,
--	'Dianne',
--	'4b8027d7604b6ca0fb4e6b0ba32a489d8f83404744fce2aaa5bd76557503e1e1',
--	'WHXGuuNRWvOFda1iZFxuS43nqdsU3gTc',
--	'atempmailfortestingcsharp@gmail.com',
--	'07700 900730',
--	1
--);

--INSERT INTO [Staff] VALUES (
--	1,
--	'Jon',
--	'2241f96d20db3dd70220ff1c83c4b5fc9e31ec7ae147fedd5fd744dae7f8d285',
--	'EbT0yIBi1tIZvdQXFPmEYKt8tylZhwY5',
--	'not.a@real.email',
--	'07700 900747',
--	0
--);

--INSERT INTO [Staff] VALUES (
--	2,
--	'Elaine',
--	'5e0524de4400e6875b14c07da5731796166037f54bc87043ab9abd98c3d33036',
--	'T5GBVgh0ZeVeZP4wl8tjnPT96ag1drTs',
--	'',
--	'07700 900835',
--	0
--);

--INSERT INTO [Staff] VALUES (
--	3,
--	'Richard',
--	'5e0524de4400e6875b14c07da5731796166037f54bc87043ab9abd98c3d33036',
--	'RMo5PySIYiNSIRa0YfKEeJT2YMj3NF0M',
--	'',
--	'07700 900042',
--	0
--);

--INSERT INTO [Staff] VALUES (
--	4,
--	'Jane',
--	'5e0524de4400e6875b14c07da5731796166037f54bc87043ab9abd98c3d33036',
--	'97Lh0VLIaaFfm278gFPoLQNTYj0bvcX7',
--	'',
--	'07700 900276',
--	0
--);

CREATE TABLE [Appointment Type] (
	[Appointment Type ID] INT NOT NULL PRIMARY KEY,
	[Base Time Taken] DECIMAL (7, 5) NOT NULL,
	[Base Price] DECIMAL (10, 2) NOT NULL,
	[Description] VARCHAR(127) NOT NULL
);

INSERT INTO [Appointment Type] VALUES (
	0,
	1,
	35,
	'Wash & Brush'
);

INSERT INTO [Appointment Type] VALUES (
	1,
	1.5,
	40,
	'Wash, Brush & Trim'
);

INSERT INTO [Appointment Type] VALUES (
	2,
	2,
	50,
	'Wash, Brush & Full Cut'
);

CREATE TABLE [Grooming Room] (
	[Grooming Room ID] INT NOT NULL PRIMARY KEY,
	[Grooming Room Name] VARCHAR(31) NOT NULL,
	[Grooming Room Notes] VARCHAR(63)
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

CREATE TABLE [Booking] (
	[Booking ID] INT NOT NULL PRIMARY KEY,
	[Date Made] DATE NOT NULL
);

CREATE TABLE [Appointment] (
	[Appointment ID] INT NOT NULL PRIMARY KEY,
	[Dog ID] INT NOT NULL FOREIGN KEY REFERENCES [Dog] ON DELETE CASCADE,
	[Appointment Type ID] INT NOT NULL FOREIGN KEY REFERENCES [Appointment Type] ON DELETE CASCADE,
	[Staff ID] INT NOT NULL FOREIGN KEY REFERENCES [Staff] ON DELETE CASCADE,
	[Booking ID] INT NOT NULL FOREIGN KEY REFERENCES [Booking] ON DELETE CASCADE,
	[Grooming Room ID] INT NOT NULL FOREIGN KEY REFERENCES [Grooming Room] ON DELETE CASCADE,
	[Nails And Teeth] BIT NOT NULL,
	[Is Cancelled] BIT NOT NULL,
	[Is Paid] BIT NOT NULL,
	[Appointment Date] DATE NOT NULL,
	[Appointment Time] TIME NOT NULL
);

--CREATE TABLE [Shift] (
--	[Shift ID] INT NOT NULL PRIMARY KEY,
--	[Staff ID] INT NOT NULL FOREIGN KEY REFERENCES [Staff] ON DELETE CASCADE,
--	[Shift Day] INT NOT NULL,
--	[Shift Start Time] TIME NOT NULL,
--	[Shift End Time] TIME NOT NULL
--);

CREATE TABLE [Shift Exception] (
	[Shift Exception ID] INT NOT NULL PRIMARY KEY,
	[Staff ID] INT NOT NULL FOREIGN KEY REFERENCES [Staff] ON DELETE CASCADE,
	[Start Date] DATE NOT NULL,
	[End Date] DATE NOT NULL
);
