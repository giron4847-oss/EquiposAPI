-- Ejecutar en SSMS conectada a .\SQLEXPRESS (cambia la contrasena por una tuya)
USE master;
CREATE LOGIN reportes WITH PASSWORD = 'CAMBIA_ESTA_CLAVE_123!';
GO
USE EquiposDb;
CREATE USER reportes FOR LOGIN reportes;
ALTER ROLE db_datareader ADD MEMBER reportes;   -- SOLO lectura
GO

--categorias
USE EquiposDb;
GO
SELECT c.Nombre AS Categoria, e.Estado, COUNT(*) AS Cantidad
FROM Equipos e
JOIN Categorias c ON c.Id = e.CategoriaId
GROUP BY c.Nombre, e.Estado;

--usuarios
USE EquiposDb;
GO
SELECT Id, Username, Rol FROM Usuarios;

--los 5 equipos mas prestados
  USE EquiposDb;
  GO
  SELECT TOP 5 e.CodigoInventario, e.Nombre, COUNT(*) AS Prestamos
  FROM Prestamos p JOIN Equipos e ON e.Id = p.EquipoId
  GROUP BY e.CodigoInventario, e.Nombre
  ORDER BY COUNT(*) DESC;

    USE EquiposDb;
  GO
  SELECT * FROM Prestamos WHERE FechaDevolucion IS NULL AND FechaVencimiento < CAST(GETDATE() AS date);