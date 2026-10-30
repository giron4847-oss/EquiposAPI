-- Ejecutar en SSMS conectada a .\SQLEXPRESS (cambia la contrasena por una tuya)
USE master;
CREATE LOGIN reportes WITH PASSWORD = 'CAMBIA_ESTA_CLAVE_123!';
GO
USE EquiposDb;
CREATE USER reportes FOR LOGIN reportes;
ALTER ROLE db_datareader ADD MEMBER reportes;   -- SOLO lectura
GO
