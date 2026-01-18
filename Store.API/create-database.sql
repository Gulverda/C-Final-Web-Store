-- SQL Script to create storeFainalDB database
-- Run this script if you need to manually create the database

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'storeFainalDB')
BEGIN
    CREATE DATABASE storeFainalDB;
    PRINT 'Database storeFainalDB created successfully!';
END
ELSE
BEGIN
    PRINT 'Database storeFainalDB already exists.';
END
GO

-- Verify the database was created
USE storeFainalDB;
GO
