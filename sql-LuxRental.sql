USE LuxCarRental;

DROP TABLE IF EXISTS [Transaction];
DROP TABLE IF EXISTS Booking;
DROP TABLE IF EXISTS Car;
DROP TABLE IF EXISTS model;
DROP TABLE IF EXISTS Make;
DROP TABLE IF EXISTS FuelType;
DROP TABLE IF EXISTS CarStatus;
DROP TABLE IF EXISTS VehicleClass;
DROP TABLE IF EXISTS BookingStatus;
DROP TABLE IF EXISTS Customer;

-- =====================
-- Customer
-- =====================
CREATE TABLE Customer (
    pkCustomerId INT IDENTITY(1,1) PRIMARY KEY,
    firstName VARCHAR(40) NOT NULL,
    lastName VARCHAR(40) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    phoneNumber VARCHAR(20),
    driverLicenceNo VARCHAR(20) UNIQUE,
    licenceVerified BIT
);

-- =====================
-- BookingStatus
-- =====================
CREATE TABLE BookingStatus (
    pkBookingStatusId INT IDENTITY(1,1) PRIMARY KEY,
    bookingStatus VARCHAR(255) NOT NULL
);

-- =====================
-- VehicleClass
-- =====================
CREATE TABLE VehicleClass (
    pkVehicleClassId INT IDENTITY(1,1) PRIMARY KEY,
    vehicleClass VARCHAR(255) NOT NULL
);

-- =====================
-- CarStatus
-- =====================
CREATE TABLE CarStatus (
    pkCarStatusId INT IDENTITY(1,1) PRIMARY KEY,
    statusFlag VARCHAR(255) NOT NULL
);

-- =====================
-- FuelType
-- =====================
CREATE TABLE FuelType (
    pkFuelTypeId INT IDENTITY(1,1) PRIMARY KEY,
    fuelType VARCHAR(255) NOT NULL
);

-- =====================
-- Make
-- =====================
CREATE TABLE Make (
    pkMakeId INT IDENTITY(1,1) PRIMARY KEY,
    makeName VARCHAR(255) NOT NULL
);

-- =====================
-- Model
-- =====================
CREATE TABLE Model (
    pkModelId INT IDENTITY(1,1) PRIMARY KEY,
    modelName VARCHAR(255) NOT NULL,
    fkMakeId INT NOT NULL,
    CONSTRAINT FK_Model_Make
        FOREIGN KEY (fkMakeId) REFERENCES Make(pkMakeId)
);

-- =====================
-- Car
-- =====================
CREATE TABLE Car (
    pkCarId INT IDENTITY(1,1) PRIMARY KEY,
    colour VARCHAR(40),
    transmissionType TINYINT CHECK (transmissionType IN (0,1)) NOT NULL, -- 0=Manual, 1=Auto
    year INT NOT NULL,
    carThumbnail VARCHAR(255),
    vinNumber VARCHAR(17) UNIQUE NOT NULL,
    licencePlate VARCHAR(10) UNIQUE NOT NULL,
    personCap INT NOT NULL,
    luggageCap INT NOT NULL,
    dailyRate DECIMAL(19,2) NOT NULL,

    fkVehicleClassId INT NOT NULL,
    fkCarStatusId INT NOT NULL,
    fkModelId INT NOT NULL,
    fkFuelTypeId INT NOT NULL,

    CONSTRAINT FK_Car_VehicleClass FOREIGN KEY (fkVehicleClassId)
        REFERENCES VehicleClass(pkVehicleClassId),

    CONSTRAINT FK_Car_CarStatus FOREIGN KEY (fkCarStatusId)
        REFERENCES CarStatus(pkCarStatusId),

    CONSTRAINT FK_Car_Model FOREIGN KEY (fkModelId)
        REFERENCES Model(pkModelId),

    CONSTRAINT FK_Car_FuelType FOREIGN KEY (fkFuelTypeId)
        REFERENCES FuelType(pkFuelTypeId)
);

-- =====================
-- Booking
-- =====================
CREATE TABLE Booking (
    pkBookingId INT IDENTITY(1,1) PRIMARY KEY,
    startDateTime DATETIME2 NOT NULL,
    endDateTime DATETIME2 NOT NULL,
    createdAt DATETIME2 NOT NULL,
    cancelledAt DATETIME2 NULL,

    fkBookingStatusId INT NOT NULL,
    fkCarId INT NOT NULL,
    fkCustomerId INT NOT NULL,

    CONSTRAINT FK_Booking_Status FOREIGN KEY (fkBookingStatusId)
        REFERENCES BookingStatus(pkBookingStatusId),

    CONSTRAINT FK_Booking_Car FOREIGN KEY (fkCarId)
        REFERENCES Car(pkCarId),

    CONSTRAINT FK_Booking_Customer FOREIGN KEY (fkCustomerId)
        REFERENCES Customer(pkCustomerId)
);

-- =====================
-- Transaction
-- =====================
CREATE TABLE [Transaction] (
    pkTransactionId INT IDENTITY(1,1) PRIMARY KEY,
    amountPaid DECIMAL(19,2) NOT NULL,
    paymentDate DATETIME2 NOT NULL,
    fkBookingId INT NOT NULL,

    CONSTRAINT FK_Transaction_Booking FOREIGN KEY (fkBookingId)
        REFERENCES Booking(pkBookingId)
);