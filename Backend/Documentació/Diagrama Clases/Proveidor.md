´´´
        +----------------------+
        |  ProveedorBusiness   |
        +----------------------+
        | +CrearProveedor(req) |
        +----------+-----------+
                   |
                   v
        +----------------------+
        |   ProveedorDomain    |
        +----------------------+
        | nombre               |
        | direccion            |
        | ciudad               |
        | codigo_postal        |
        | pais                 |
        | telefono             |
        +----------+-----------+
                   |
                   v
        +----------------------+
        |   ProveedorEntity    |
        +----------------------+
        | ID                   |
        | nombre               |
        | direccion            |
        | ciudad               |
        | codigo_postal        |
        | pais                 |
        | telefono             |
        | fax                  |
        +----------------------+

        +----------------------+
        |   ProveedorRequest   |
        +----------------------+
        | nombre               |
        | direccion            |
        | ciudad               |
        | codigo_postal        |
        | pais                 |
        | telefono             |
        | fax                  |
        +----------+-----------+
                   |
                   v
        (mapping DTO ⇄ Domain)

        (ús estàtic)
               ^
               |
   +---------------------------+
   | ProveedorValidator        |
   | (static)                  |
   +---------------------------+
   | +EsValido(domain)         |
   | +ValidarTelefono()        |
   +---------------------------+

´´´