# RealEstate Visit Microservice

Este microservicio gestiona las solicitudes de visita a propiedades en una plataforma inmobiliaria. Forma parte de una arquitectura de microservicios junto a servicios de usuarios y propiedades.

## 🚀 Funcionalidades

- Crear solicitud de visita a una propiedad
- Consultar visitas del usuario interesado
- Consultar visitas por propiedad (solo accesible al dueño)
- Cambiar estado de visita (aceptada, rechazada, etc.)
- Control de acceso con JWT
- Almacenamiento distribuido con Cassandra

## 🛠️ Tecnologías

- .NET 7
- ASP.NET Web API
- Cassandra (DataStax Astra compatible)
- JWT Authentication
- FluentValidation
- Swagger (OpenAPI)

## 🔐 Seguridad

Este servicio requiere autenticación mediante JWT. El token debe incluir el `sub` como el ID del usuario autenticado.

## 📦 Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/visits` | Crear una solicitud de visita |
| GET | `/visits/my-visits` | Obtener visitas del usuario interesado |
| GET | `/visits/owner` | Obtener visitas recibidas (solo para propietarios) |
| GET | `/visits/property/{propertyId}` | Obtener visitas por propiedad |
| PUT | `/visits/{id}/status` | Cambiar el estado de una visita (solo propietarios) |

## ⚙️ Variables de configuración

Crea un archivo `appsettings.Development.json` con el siguiente formato:

```json
{
  "Jwt": {
    "Issuer": "realestate-auth",
    "Audience": "realestate-api",
    "Secret": "TU_SECRETO_AQUI"
  },
  "Cassandra": {
    "ContactPoints": [ "localhost" ],
    "Port": 9042,
    "Username": "usuario",
    "Password": "contraseña",
    "Keyspace": "realestate"
  }
}
