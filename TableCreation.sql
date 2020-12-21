DROP TABLE Appointment;
DROP TABLE GroomingRoom;
DROP TABLE AppointmentType;
DROP TABLE Staff;
DROP TABLE Dog;
DROP TABLE Contact;
DROP TABLE Client;

CREATE TABLE [Client] (
	ClientID INT NOT NULL PRIMARY KEY,
	ClientNotes VARCHAR(100),
	ClientJoinDate DATE
);

CREATE TABLE [Contact] (
	ContactID INT NOT NULL PRIMARY KEY,
	ClientID INT NOT NULL FOREIGN KEY REFERENCES [Client],
	ClientName VARCHAR(30) NOT NULL,
	ClientEmail VARCHAR(40),
	ClientAddress VARCHAR(100),
	ClientPostcode VARCHAR(10),
	ClientPhoneNo VARCHAR(20)
);

CREATE TABLE [Dog] (
	DogID INT NOT NULL PRIMARY KEY,
	ClientID INT NOT NULL REFERENCES [Client],
	DogName VARCHAR(20) NOT NULL,
	DogDOB DATE,
	DogGender VARCHAR(1),
	DogType VARCHAR(40)
);

CREATE TABLE [Staff] (
	StaffID INT NOT NULL PRIMARY KEY,
	StaffName VARCHAR(20) NOT NULL,
	StaffPassword VARCHAR(64) NOT NULL,
	PasswordSalt VARCHAR(32),
	StaffEmail VARCHAR(128),
	StaffPhoneNo VARCHAR(15), 
	StaffUses2FA BIT NOT NULL
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

CREATE TABLE [AppointmentType] (
	AppointmentTypeID INT NOT NULL PRIMARY KEY,
	BaseTimeTaken DECIMAL NOT NULL,
	BasePrice DECIMAL NOT NULL,
	[Description] VARCHAR(100) NOT NULL
);

INSERT INTO [AppointmentType] VALUES (
	0,
	1,
	35,
	'Wash, Shampoo & Brush'
);

INSERT INTO [AppointmentType] VALUES (
	1,
	1.5,
	40,
	'Wash, Shampoo, Brush & Trim'
);

INSERT INTO [AppointmentType] VALUES (
	2,
	2,
	50,
	'Wash, Shampoo, Brush & Full Cut'
);

CREATE TABLE [GroomingRoom] (
	GroomingRoomID INT NOT NULL PRIMARY KEY,
	GroomingRoomName VARCHAR(20) NOT NULL,
	GroomingRoomNotes VARCHAR(50)
);

INSERT INTO [GroomingRoom] VALUES (
	0,
	'Shed',
	'Ideal for smaller dogs.'
);

INSERT INTO [GroomingRoom] VALUES (
	1,
	'Barn 1',
	'Fit for all dogs.'
);

INSERT INTO [GroomingRoom] VALUES (
	2,
	'Barn 2',
	'Fit for all dogs, ideal for big dogs.'
);

CREATE TABLE [Appointment] (
	AppointmentID INT NOT NULL PRIMARY KEY,
	DogID INT NOT NULL FOREIGN KEY REFERENCES [Dog],
	AppointmentTypeID INT NOT NULL FOREIGN KEY REFERENCES [AppointmentType],
	StaffID INT NOT NULL FOREIGN KEY REFERENCES [Staff],
	IsInitialAppointment BIT NOT NULL,
	IncludesNailAndTeeth BIT NOT NULL,
	IsCancelled BIT NOT NULL,
	IsPaid BIT NOT NULL,
	BookedInAdvanceDiscount DECIMAL(5,2),
	AppointmentDateTime DATETIME NOT NULL,
	InvoiceLastSent DATETIME,
	InvoicesSent INT NOT NULL,
	GroomingRoomID INT NOT NULL FOREIGN KEY REFERENCES [GroomingRoom]
);