# Idea del Proyecto

## ¿Qué quiero construir?

"ProjectLite" es una API REST para la gestión de obras de construcción.
Permite a una empresa organizar sus proyectos, las obras asociadas a cada
proyecto, y las partes de trabajo diarias dentro de cada obra. La empresa
también gestiona su plantilla de trabajadores, que pueden ser asignados
a obras concretas.

## Usuarios objetivo

Empresas constructoras que necesitan llevar un registro estructurado de
sus proyectos y obras, y los responsables internos que gestionan la
plantilla y el avance diario del trabajo.

## Requisitos funcionales

Gestión de la empresa: cada usuario pertenece a una empresa. Todos los
datos (proyectos, obras, partes, trabajadores) son privados a esa empresa
— ningún usuario puede ver datos de otra empresa.

Gestión de trabajadores: la empresa puede registrar trabajadores. Un
trabajador es un usuario del sistema vinculado a una empresa.

Gestión de proyectos: la empresa puede crear proyectos. Un proyecto
pertenece a una única empresa.

Gestión de obras: cada proyecto puede tener múltiples obras. Una obra
pertenece a un único proyecto.

Gestión de partes: cada obra puede tener múltiples partes de trabajo.
Una parte registra la fecha, una descripción del trabajo realizado, y
el trabajador responsable.

Autenticación y autorización: autenticación por token. Un usuario solo
puede acceder a los datos de su propia empresa (filtrado multi-tenant
en todos los endpoints).

## Restricciones técnicas

- Lenguaje/Framework: Python 3.11+ con Django 5.x y Django REST Framework.
- Base de datos: PostgreSQL (producción) / SQLite (desarrollo local).
- Sin tareas asíncronas en esta versión: no se requiere Celery ni Redis.
- Backend puro: API REST + panel de administración de Django. Sin frontend.
- Sin facturación, pagos ni gestión de planes.

## Lo que NO debe hacer

- No incluye notificaciones por email ni tareas en segundo plano.
- No incluye chat, comentarios ni actividad en tiempo real.
- No incluye gestión de roles complejos: hay un único tipo de usuario.
- No incluye multi-idioma ni internacionalización.
