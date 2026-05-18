CREATE TABLE clientes (
    id UNIQUEIDENTIFIER PRIMARY KEY,
    codigo_cliente VARCHAR(50),
    nombre VARCHAR(100),
    direccion VARCHAR(255),
    ciudad VARCHAR(100),
    codigo_postal VARCHAR(20),
    pais VARCHAR(100),
    telefono VARCHAR(30),
    nif_iva VARCHAR(50)
);

CREATE TABLE ordenes (
    id UNIQUEIDENTIFIER PRIMARY KEY,
    numero VARCHAR(50),
    fecha DATETIME,
    fecha_recepcion DATETIME,
    modo_pago VARCHAR(50),
    gestionado_por VARCHAR(100),
    direccion_entrega VARCHAR(255),
    total_ht DECIMAL(18,2),
    total_iva DECIMAL(18,2),
    total_ttc DECIMAL(18,2),
    moneda VARCHAR(10),
    id_proveedor UNIQUEIDENTIFIER,
    id_cliente UNIQUEIDENTIFIER,
    estado NUMERIC(2) DEFAULT 0,
    FOREIGN KEY (id_cliente) REFERENCES clientes(id)
);

CREATE TABLE lineas_orden (
    id UNIQUEIDENTIFIER PRIMARY KEY,
    id_orden UNIQUEIDENTIFIER,
    descripcion VARCHAR(255),
    cantidad INT,
    precio_unitario DECIMAL(18,2),
    descuento DECIMAL(18,2),
    precio_neto DECIMAL(18,2),
    importe_ht DECIMAL(18,2),
    tva DECIMAL(18,2),
    codigo_cliente VARCHAR(50),
    codigo_proveedor VARCHAR(50),
    FOREIGN KEY (id_orden) REFERENCES ordenes(id)
);