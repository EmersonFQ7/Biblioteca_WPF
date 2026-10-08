# 📚 BibliotecaDB — Lab Semana 07

**Curso:** Desarrollo de Aplicaciones Empresariales Avanzadas
**Tema:** Arquitectura de Capas en el Proceso de Datos
**Tecnologías:** C# · WPF · .NET 10 · SQL Server · Visual Studio 2022

---

## 🧱 Arquitectura

El proyecto está organizado en **4 capas**:

```
Biblioteca.Entidades  →  (no referencia a nadie)
Biblioteca.Datos      →  Entidades
Biblioteca.Negocio    →  Datos + Entidades
Biblioteca.WPF        →  Negocio + Entidades  (nunca Datos)
```

| Capa | Responsabilidad |
|---|---|
| **Entidades** | Clases POCO con solo propiedades |
| **Datos** | Acceso a SQL Server con `SqlParameter`, operaciones CRUD |
| **Negocio** | Validaciones y reglas de negocio, lanza `ReglaNegocioException` |
| **WPF** | Interfaz de usuario con `async/await` de punta a punta |

---

## 🗄️ Base de Datos

**Motor:** SQL Server 2022 Express
**Herramienta:** SQL Server Management Studio 22
**Cadena de conexión:** definida en `App.config` del proyecto WPF

### Script completo de creación

```sql
/* =====================================================================
   LAB SEMANA 07 - Arquitectura de Capas
   Base de Datos: BibliotecaDB
   SQL Server Management Studio 22
   ===================================================================== */

-- 1) CREAR BASE DE DATOS
USE master;
GO

IF DB_ID('BibliotecaDB') IS NOT NULL
BEGIN
    ALTER DATABASE BibliotecaDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BibliotecaDB;
END
GO

CREATE DATABASE BibliotecaDB;
GO

USE BibliotecaDB;
GO

/* =====================================================================
   2) CREACION DE TABLAS
   ===================================================================== */

-- Tabla Autores
CREATE TABLE Autores (
    AutorId      INT IDENTITY(1,1) NOT NULL,
    Nombre       VARCHAR(100)      NOT NULL,
    Nacionalidad VARCHAR(50)       NOT NULL,
    Activo       BIT               NOT NULL CONSTRAINT DF_Autores_Activo DEFAULT (1),
    CONSTRAINT PK_Autores PRIMARY KEY (AutorId)
);
GO

-- Tabla Libros
CREATE TABLE Libros (
    LibroId    INT IDENTITY(1,1) NOT NULL,
    Titulo     VARCHAR(150)      NOT NULL,
    ISBN       VARCHAR(20)       NOT NULL,
    AutorId    INT               NOT NULL,
    Ejemplares INT               NOT NULL CONSTRAINT DF_Libros_Ejemplares DEFAULT (0),
    Activo     BIT               NOT NULL CONSTRAINT DF_Libros_Activo DEFAULT (1),
    CONSTRAINT PK_Libros PRIMARY KEY (LibroId),
    CONSTRAINT UQ_Libros_ISBN UNIQUE (ISBN),
    CONSTRAINT FK_Libros_Autores FOREIGN KEY (AutorId) REFERENCES Autores(AutorId),
    CONSTRAINT CK_Libros_Ejemplares CHECK (Ejemplares >= 0)
);
GO

-- Tabla Socios
CREATE TABLE Socios (
    SocioId INT IDENTITY(1,1) NOT NULL,
    DNI     VARCHAR(8)        NOT NULL,
    Nombre  VARCHAR(100)      NOT NULL,
    Email   VARCHAR(100)      NOT NULL,
    Activo  BIT               NOT NULL CONSTRAINT DF_Socios_Activo DEFAULT (1),
    CONSTRAINT PK_Socios PRIMARY KEY (SocioId),
    CONSTRAINT UQ_Socios_DNI UNIQUE (DNI)
);
GO

-- Tabla Prestamos (cabecera)
CREATE TABLE Prestamos (
    PrestamoId    INT IDENTITY(1,1) NOT NULL,
    SocioId       INT               NOT NULL,
    FechaPrestamo DATETIME          NOT NULL CONSTRAINT DF_Prestamos_FechaPrestamo DEFAULT (GETDATE()),
    FechaLimite   DATETIME          NOT NULL,
    Estado        VARCHAR(20)       NOT NULL CONSTRAINT DF_Prestamos_Estado DEFAULT ('Pendiente'),
    CONSTRAINT PK_Prestamos PRIMARY KEY (PrestamoId),
    CONSTRAINT FK_Prestamos_Socios FOREIGN KEY (SocioId) REFERENCES Socios(SocioId),
    CONSTRAINT CK_Prestamos_Estado CHECK (Estado IN ('Pendiente','Devuelto'))
);
GO

-- Tabla DetallePrestamo (clave primaria compuesta)
CREATE TABLE DetallePrestamo (
    PrestamoId      INT      NOT NULL,
    LibroId         INT      NOT NULL,
    FechaDevolucion DATETIME NULL,
    CONSTRAINT PK_DetallePrestamo PRIMARY KEY (PrestamoId, LibroId),
    CONSTRAINT FK_Detalle_Prestamos FOREIGN KEY (PrestamoId) REFERENCES Prestamos(PrestamoId),
    CONSTRAINT FK_Detalle_Libros    FOREIGN KEY (LibroId)    REFERENCES Libros(LibroId)
);
GO

/* =====================================================================
   3) INSERCION DE DATOS DE PRUEBA
   ===================================================================== */

-- 8 AUTORES
INSERT INTO Autores (Nombre, Nacionalidad) VALUES
('Mario Vargas Llosa',       'Peruana'),
('Gabriel Garcia Marquez',   'Colombiana'),
('Jorge Luis Borges',        'Argentina'),
('Isabel Allende',           'Chilena'),
('Julio Cortazar',           'Argentina'),
('Octavio Paz',              'Mexicana'),
('Pablo Neruda',             'Chilena'),
('Cesar Vallejo',            'Peruana');
GO

-- 20 LIBROS
INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares) VALUES
('La ciudad y los perros',        '978-84-376-0001', 1, 5),
('Conversacion en La Catedral',   '978-84-376-0002', 1, 3),
('La fiesta del Chivo',           '978-84-376-0003', 1, 4),
('Cien anios de soledad',         '978-84-376-0004', 2, 6),
('El amor en los tiempos',        '978-84-376-0005', 2, 2),
('Cronica de una muerte',         '978-84-376-0006', 2, 3),
('Ficciones',                     '978-84-376-0007', 3, 4),
('El Aleph',                      '978-84-376-0008', 3, 5),
('La casa de los espiritus',      '978-84-376-0009', 4, 3),
('Paula',                         '978-84-376-0010', 4, 2),
('Rayuela',                       '978-84-376-0011', 5, 4),
('Bestiario',                     '978-84-376-0012', 5, 3),
('El laberinto de la soledad',    '978-84-376-0013', 6, 2),
('Libertad bajo palabra',         '978-84-376-0014', 6, 3),
('Veinte poemas de amor',         '978-84-376-0015', 7, 5),
('Canto general',                 '978-84-376-0016', 7, 2),
('Los heraldos negros',           '978-84-376-0017', 8, 4),
('Trilce',                        '978-84-376-0018', 8, 3),
('Poemas humanos',                '978-84-376-0019', 8, 2),
('De sobremesa',                  '978-84-376-0020', 3, 1);
GO

-- 10 SOCIOS
INSERT INTO Socios (DNI, Nombre, Email) VALUES
('45678901', 'Juan Perez Garcia',       'juan.perez@mail.com'),
('45678902', 'Maria Lopez Rojas',       'maria.lopez@mail.com'),
('45678903', 'Carlos Ramirez Soto',     'carlos.ramirez@mail.com'),
('45678904', 'Ana Torres Vega',         'ana.torres@mail.com'),
('45678905', 'Luis Mendoza Diaz',       'luis.mendoza@mail.com'),
('45678906', 'Sofia Castro Luna',       'sofia.castro@mail.com'),
('45678907', 'Pedro Gutierrez Paz',     'pedro.gutierrez@mail.com'),
('45678908', 'Lucia Flores Rios',       'lucia.flores@mail.com'),
('45678909', 'Jorge Salazar Nunez',     'jorge.salazar@mail.com'),
('45678910', 'Rosa Herrera Cruz',       'rosa.herrera@mail.com');
GO

/* ---------------------------------------------------------------------
   PRESTAMOS (5 cabeceras con sus detalles)
   - El SocioId = 1 tendra 3 libros pendientes (Prestamo 1 con 3 libros)
     para poder probar la regla de "no mas de 3 libros pendientes".
   --------------------------------------------------------------------- */

-- Prestamo 1 : Socio 1 -> 3 libros pendientes
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (1, DATEADD(DAY, -5, GETDATE()), DATEADD(DAY, 2, GETDATE()), 'Pendiente');
DECLARE @P1 INT = SCOPE_IDENTITY();

INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(@P1, 1, NULL),
(@P1, 4, NULL),
(@P1, 7, NULL);

UPDATE Libros SET Ejemplares = Ejemplares - 1 WHERE LibroId IN (1, 4, 7);
GO

-- Prestamo 2 : Socio 2 -> 1 libro pendiente
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (2, DATEADD(DAY, -3, GETDATE()), DATEADD(DAY, 4, GETDATE()), 'Pendiente');
DECLARE @P2 INT = SCOPE_IDENTITY();

INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(@P2, 9, NULL);

UPDATE Libros SET Ejemplares = Ejemplares - 1 WHERE LibroId = 9;
GO

-- Prestamo 3 : Socio 3 -> 2 libros, uno ya devuelto (sigue Pendiente)
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (3, DATEADD(DAY, -10, GETDATE()), DATEADD(DAY, -3, GETDATE()), 'Pendiente');
DECLARE @P3 INT = SCOPE_IDENTITY();

INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(@P3, 11, DATEADD(DAY, -1, GETDATE())),
(@P3, 15, NULL);

UPDATE Libros SET Ejemplares = Ejemplares - 1 WHERE LibroId = 15;
-- El libro 11 ya volvio al stock, no se descuenta
GO

-- Prestamo 4 : Socio 4 -> 1 libro, ya DEVUELTO completo
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (4, DATEADD(DAY, -15, GETDATE()), DATEADD(DAY, -8, GETDATE()), 'Devuelto');
DECLARE @P4 INT = SCOPE_IDENTITY();

INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(@P4, 17, DATEADD(DAY, -10, GETDATE()));
-- El libro ya se devolvio, no se descuenta del stock
GO

-- Prestamo 5 : Socio 5 -> 2 libros pendientes
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (5, DATEADD(DAY, -2, GETDATE()), DATEADD(DAY, 5, GETDATE()), 'Pendiente');
DECLARE @P5 INT = SCOPE_IDENTITY();

INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(@P5, 2, NULL),
(@P5, 5, NULL);

UPDATE Libros SET Ejemplares = Ejemplares - 1 WHERE LibroId IN (2, 5);
GO

/* =====================================================================
   4) VERIFICACION RAPIDA
   ===================================================================== */
SELECT 'Autores'          AS Tabla, COUNT(*) AS Total FROM Autores
UNION ALL SELECT 'Libros',           COUNT(*) FROM Libros
UNION ALL SELECT 'Socios',           COUNT(*) FROM Socios
UNION ALL SELECT 'Prestamos',        COUNT(*) FROM Prestamos
UNION ALL SELECT 'DetallePrestamo',  COUNT(*) FROM DetallePrestamo;
GO

-- Prueba de la regla: socio con 3 libros pendientes
SELECT  s.SocioId, s.Nombre, COUNT(*) AS LibrosPendientes
FROM    Socios s
INNER JOIN Prestamos p        ON p.SocioId = s.SocioId
INNER JOIN DetallePrestamo d  ON d.PrestamoId = p.PrestamoId
WHERE   d.FechaDevolucion IS NULL
GROUP BY s.SocioId, s.Nombre
ORDER BY LibrosPendientes DESC;
GO
```

---

## 🔧 Configuración

### Cadena de conexión (`App.config`)

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <connectionStrings>
    <add name="BibliotecaDB"
         connectionString="Server=EMERSON\SQLEXPRESS;Database=BibliotecaDB;Integrated Security=True;TrustServerCertificate=True;"
         providerName="Microsoft.Data.SqlClient" />
  </connectionStrings>
</configuration>
```

### Paquetes NuGet utilizados

| Proyecto | Paquete |
|---|---|
| `Biblioteca.Datos` | `Microsoft.Data.SqlClient` |
| `Biblioteca.Datos` | `System.Configuration.ConfigurationManager` |
| `Biblioteca.WPF` | `System.Configuration.ConfigurationManager` |

---

## ▶️ Cómo ejecutar

1. Abrir **SQL Server Management Studio 22** y ejecutar el script completo (crea BD, tablas y datos de prueba).
2. Abrir la solución **`Biblioteca.sln`** en Visual Studio 2022.
3. Ajustar el `Server=` del `App.config` al nombre de la instancia SQL local.
4. Establecer `Biblioteca.WPF` como proyecto de inicio y presionar **F5**.

---

## 📏 Reglas de negocio implementadas

| Entidad | Reglas |
|---|---|
| **Libros** | ISBN único · no eliminar con préstamos pendientes · baja lógica |
| **Socios** | DNI único · DNI de 8 dígitos · email válido · no eliminar con libros pendientes · baja lógica |
| **Préstamos** | Máximo 3 libros pendientes por socio · solo libros/socios activos · solo con ejemplares disponibles · transacción atómica |
| **Devoluciones** | Reposición de stock · cálculo de multa (**S/ 1.50 por día de retraso**) · estado automático a `Devuelto` cuando se devuelve el último libro |

---

## 📁 Estructura de la solución

```
Biblioteca/
├── Biblioteca.Entidades/
│   ├── Autor.cs
│   ├── Libro.cs
│   ├── Socio.cs
│   ├── Prestamo.cs
│   └── DetallePrestamo.cs
├── Biblioteca.Datos/
│   ├── Conexion.cs
│   ├── AutorDatos.cs
│   ├── LibroDatos.cs
│   ├── SocioDatos.cs
│   └── PrestamoDatos.cs
├── Biblioteca.Negocio/
│   ├── ReglaNegocioException.cs
│   ├── AutorNegocio.cs
│   ├── LibroNegocio.cs
│   ├── SocioNegocio.cs
│   └── PrestamoNegocio.cs
└── Biblioteca.WPF/
    ├── Estilos/Tema.xaml
    ├── App.xaml / App.xaml.cs / App.config
    ├── MainWindow.xaml / .xaml.cs
    ├── VentanaLibros.xaml / .xaml.cs
    ├── VentanaSocios.xaml / .xaml.cs
    ├── VentanaPrestamo.xaml / .xaml.cs
    ├── VentanaDevolucion.xaml / .xaml.cs
    └── VentanaReporte.xaml / .xaml.cs
```
