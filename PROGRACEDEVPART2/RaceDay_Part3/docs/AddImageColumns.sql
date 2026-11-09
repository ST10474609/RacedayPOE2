-- Execute against the existing RaceDay database before starting the updated API.
IF COL_LENGTH('dbo.Events','BannerImageUrl') IS NULL ALTER TABLE dbo.Events ADD BannerImageUrl NVARCHAR(2048) NULL;
IF COL_LENGTH('dbo.Users','ProfilePictureUrl') IS NULL ALTER TABLE dbo.Users ADD ProfilePictureUrl NVARCHAR(2048) NULL;
