USE Sany3yDB;
GO

CREATE TABLE Province (
    id INT PRIMARY KEY,
    province_name_ar NVARCHAR(100) NOT NULL,
    province_name_en NVARCHAR(100) NOT NULL
);

CREATE TABLE Governorate (
    id INT PRIMARY KEY,
    province_id INT FOREIGN KEY REFERENCES Province(id) NOT NULL,
    governorate_name_ar NVARCHAR(50) NOT NULL,
    governorate_name_en NVARCHAR(50) NOT NULL
);
GO

CREATE TABLE City (
    id INT PRIMARY KEY IDENTITY(1, 1),
    governorate_id INT FOREIGN KEY REFERENCES Governorate(id) NOT NULL,
    city_name_ar NVARCHAR(200) NOT NULL,
    city_name_en NVARCHAR(200) NOT NULL
);
GO