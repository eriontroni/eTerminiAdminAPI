# eTerminiAdminAPI — I ARKIVUAR

Ky repository nuk mirëmbahet më. I gjithë funksionaliteti është zhvendosur brenda [eTerminiAPI] dhe shërbehet nga i njëjti proces në rrugët `/api/admin/*`.

## Pse u hoq?

`eTerminiAdminAPI` ishte praktikisht një wrapper i hollë mbi `eTerminiAPI`:

- Nuk kishte `DbContext`, `Migrations`, apo `Repositories` të vetat — referonte direkt `eTerminiAPI.Application` + `eTerminiAPI.Infrastructure` dhe përdorte të njëjtin `IUnitOfWork`.
- Përdorte **të njëjtën bazë të dhënash**, të njëjtin `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` dhe `Auth:PasswordSalt` si API-ja publike — pra ndarja ekzistonte vetëm si dy procese, jo si dy sisteme.
- Mbajtja e dy shërbimeve krijonte kosto pa përfitim:
  - dy container Docker (`etermini-api` + `etermini-adminapi`) për të njëjtën ngarkesë
  - dy host Cloudflare (`etermini-api.troni.dev` + `etermini-adminapi.troni.dev`) me DNS/tunel të veçantë
  - dy konfigurime CORS / JWT / Serilog që duhej mbajtur në sinkron manual (rrezik: një ndryshim jashtë sinkroni thyen tokenat ose hash-in e fjalëkalimeve)
  - build kontekst i çuditshëm (`context: ..`) sepse `Dockerfile`-i i AdminAPI-t duhej të hynte në direktoriumin e eTerminiAPI për të kopjuar projektet e referuara

## Ku ndodhet kodi tani?

I gjithë kodi u zhvendos brenda `eTerminiAPI` në namespace `.Admin`:

| Ish-vendodhja | Vendodhja e re |
|---|---|
| `eTerminiAdminAPI.Application.DTOs.*` | `eTerminiAPI.Application.Admin.DTOs.*` |
| `eTerminiAdminAPI.Application.Interfaces.Services` | `eTerminiAPI.Application.Admin.Interfaces.Services` |
| `eTerminiAdminAPI.Infrastructure.Services.*` | `eTerminiAPI.Infrastructure.Admin.Services.*` |
| `eTerminiAdminAPI.Infrastructure.AdminSeeder` | `eTerminiAPI.Infrastructure.Persistence.AdminSeeder` |
| `eTerminiAdminAPI.API.Controllers.*` (11 controllers) | `eTerminiAPI.API.Controllers.Admin` |
| `eTerminiAdminAPI.API.Authorization.*` (RBAC) | `eTerminiAPI.API.Authorization` |
| `eTerminiAdminAPI.API.Middleware.GlobalExceptionMiddleware` | `eTerminiAPI.API.Middleware` |

Endpoint-et publike qëndrojnë të pandryshuara në `/api/admin/*`. `AuthService` tani emit-on claim-in `permission` në JWT (SuperAdmin merr `Permissions.All`; të tjerët nga `AdminRole.Permissions`).

