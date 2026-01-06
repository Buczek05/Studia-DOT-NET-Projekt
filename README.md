# Hotel Reservation API

REST API do zarządzania rezerwacjami hotelowymi zbudowane w .NET 9.

## Technologie

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core 9
- PostgreSQL
- xUnit + Moq + FluentAssertions (testy)
- Swagger/OpenAPI

## Struktura projektu

```
HotelReservation.sln
├── Hotel.Api/            # Warstwa prezentacji (kontrolery, middleware)
├── Hotel.Application/    # Logika biznesowa (serwisy, DTOs, interfejsy)
├── Hotel.Infrastructure/ # Dostęp do danych (EF Core, repozytoria)
└── Hotel.Tests/          # Testy jednostkowe
```

## Wymagania

- .NET 9 SDK
- PostgreSQL 16+ (lub Docker)

## Uruchomienie

### 1. Uruchom bazę danych PostgreSQL

**Opcja A: Docker Compose**
```bash
docker-compose up -d
```

**Opcja B: Lokalna instalacja PostgreSQL**
- Utwórz bazę danych `hotel_db`
- Użytkownik: `postgres`, hasło: `postgres`

### 2. Uruchom API

```bash
dotnet run --project Hotel.Api
```

API automatycznie:
- Zastosuje migracje bazy danych
- Załaduje przykładowe dane (seed data)

### 3. Otwórz Swagger UI

```
https://localhost:5001/swagger
```
lub
```
http://localhost:5000/swagger
```

## Konfiguracja

### Zmienne środowiskowe

| Zmienna | Opis | Domyślna wartość |
|---------|------|------------------|
| `CONNECTION_STRING` | Pełny connection string | - |
| `DATABASE_HOST` | Host bazy danych | localhost |
| `DATABASE_PORT` | Port bazy danych | 5432 |
| `DATABASE_NAME` | Nazwa bazy danych | hotel_db |
| `DATABASE_USER` | Użytkownik | postgres |
| `DATABASE_PASSWORD` | Hasło | postgres |

### appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=hotel_db;Username=postgres;Password=postgres"
  }
}
```

## Endpointy API

### Rooms (Pokoje)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| GET | `/api/rooms` | Lista pokoi (z filtrowaniem i paginacją) |
| GET | `/api/rooms/{id}` | Szczegóły pokoju |
| GET | `/api/rooms/available` | Dostępne pokoje w danym terminie |
| POST | `/api/rooms` | Dodaj nowy pokój |
| PUT | `/api/rooms/{id}` | Edytuj pokój |
| DELETE | `/api/rooms/{id}` | Dezaktywuj pokój (soft delete) |

**Filtrowanie pokoi:**
```
GET /api/rooms?type=Double&minCapacity=2&maxPrice=300&isActive=true&page=1&pageSize=10
```

### Guests (Goście)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| GET | `/api/guests` | Lista gości |
| GET | `/api/guests/{id}` | Szczegóły gościa |
| GET | `/api/guests/by-email/{email}` | Szukaj gościa po emailu |
| POST | `/api/guests` | Dodaj nowego gościa |
| PUT | `/api/guests/{id}` | Edytuj gościa |

### Reservations (Rezerwacje)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| GET | `/api/reservations` | Lista rezerwacji |
| GET | `/api/reservations/{id}` | Szczegóły rezerwacji |
| GET | `/api/reservations/by-guest/{guestId}` | Rezerwacje gościa |
| GET | `/api/reservations/by-room/{roomId}` | Rezerwacje pokoju |
| POST | `/api/reservations` | Utwórz rezerwację |
| DELETE | `/api/reservations/{id}` | Anuluj rezerwację |

### Availability (Dostępność)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| GET | `/api/availability` | Sprawdź dostępne pokoje |

**Przykład:**
```
GET /api/availability?checkIn=2025-06-01&checkOut=2025-06-05&minCapacity=2
```

## Przykłady użycia

### Tworzenie rezerwacji

```bash
curl -X POST https://localhost:5001/api/reservations \
  -H "Content-Type: application/json" \
  -d '{
    "roomId": 1,
    "guestId": 1,
    "checkInDate": "2025-07-01",
    "checkOutDate": "2025-07-05",
    "guestsCount": 2
  }'
```

### Sprawdzenie dostępności

```bash
curl "https://localhost:5001/api/availability?checkIn=2025-07-01&checkOut=2025-07-05&minCapacity=2"
```

## Reguły biznesowe

### Rezerwacje

- **Minimalna długość pobytu:** 1 noc
- **Maksymalna długość pobytu:** 30 nocy
- **Liczba gości:** nie może przekraczać pojemności pokoju
- **Pokój:** musi być aktywny
- **Kolizje:** system wykrywa nakładające się rezerwacje
- **Cena:** `liczba_nocy * cena_za_noc`

### Anulowanie

- Możliwe tylko **przed datą zameldowania**
- Operacja jest **idempotentna** (wielokrotne anulowanie = OK)
- Rezerwacja zmienia status na `Canceled` (soft delete)

## Typy pokoi

| Typ | Opis |
|-----|------|
| `Single` | Pokój jednoosobowy |
| `Double` | Pokój dwuosobowy |
| `Suite` | Apartament |

## Obsługa błędów

API zwraca błędy w formacie RFC 7807 (ProblemDetails):

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Room with ID 999 was not found.",
  "instance": "/api/rooms/999"
}
```

| Kod | Opis |
|-----|------|
| 400 | Błąd walidacji |
| 404 | Zasób nie znaleziony |
| 409 | Konflikt (np. pokój zajęty, duplikat) |
| 500 | Błąd serwera |

## Testy

```bash
dotnet test
```

Projekt zawiera 35 testów jednostkowych pokrywających:
- Algorytm wykrywania kolizji dat
- Kalkulację ceny rezerwacji
- Walidację rezerwacji
- Logikę anulowania
- Filtrowanie i dostępność pokoi

## Decyzje projektowe

### Architektura

Projekt wykorzystuje **Clean Architecture** z podziałem na warstwy:
- **Api** - kontrolery REST, middleware
- **Application** - logika biznesowa, DTOs, interfejsy
- **Infrastructure** - implementacja dostępu do danych

### Baza danych

**PostgreSQL** - wybrane ze względu na:
- Wsparcie dla typów danych (decimal, datetime)
- Wydajność przy złożonych zapytaniach
- Darmowa licencja

### Concurrency

Wykorzystanie `xmin` (PostgreSQL) jako concurrency token zamiast `RowVersion` (SQL Server).

### Soft Delete

Pokoje są dezaktywowane (`IsActive = false`) zamiast usuwane fizycznie, aby zachować historię rezerwacji.

## Autorzy

- Kacper Bukowski
- Bartłomiej Fryc
