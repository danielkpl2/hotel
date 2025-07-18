# Hotel Booking API

## Prerequisites

- **.NET 8.0:**  
  [Download .NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

- **PostgreSQL:**  
  Make sure PostgreSQL is installed and running.

---

## Installation

```sh
dotnet restore
```

---

## Running the App

```sh
dotnet run
```

---

## Running Tests

```sh
dotnet test
```

---

## Database Setup

### 1. Create PostgreSQL User and Databases

```sql
CREATE USER hoteluser WITH PASSWORD 'hotelpassword';
ALTER USER hoteluser CREATEDB;

CREATE DATABASE "HotelDb" WITH OWNER = hoteluser ENCODING = 'UTF8' CONNECTION LIMIT = -1;
GRANT ALL PRIVILEGES ON DATABASE "HotelDb" TO hoteluser;

CREATE DATABASE "HotelDb_Test" WITH OWNER = hoteluser ENCODING = 'UTF8' CONNECTION LIMIT = -1;
GRANT ALL PRIVILEGES ON DATABASE "HotelDb_Test" TO hoteluser;
```

- The DB credentials are in the `appsettings` files.  
  (They are committed for convenience in this project.)

**Note:**  
The database tables use **PascalCase** by default (e.g., `"Bookings"`, `"BookingRooms"`, `"Hotels"`), as generated

---

## Presumptions Made

- Hotels have **exactly 6 rooms** (not "up to 6").
- Room types and capacities:
  - **Single:** capacity 1
  - **Double:** capacity 2
  - **Deluxe:** capacity 3

---

## Room Availability Logic

If the number of available rooms at the hotel between requested dates is **greater than or equal to the requested amount**,  
**all of the available rooms get returned**. The user can then select the combination of rooms they want.

> **Possible improvement:**  
> If requesting 4 people and there are 2 doubles and 1 single, don't return the single because it can't accommodate the required 4.  
> But if there are 2 singles and 2 doubles, the 2 singles could be returned because the user can take 2 singles and 1 double.

---

## Seeding the Database

I decided to seed the DB by executing a SQL file.  
This minimizes the work required and makes it easier to modify.

- **2 seed files are provided:**  
  - `small.sql` (used for integration tests)
  - `large.sql`

### To seed data:

```http
POST /api/admin/seed/small
```

### To clear the DB:

```http
DELETE /api/admin/seed/clear
```

---

## API Documentation

- The OpenAPI/Swagger file can be accessed at:  
  ```
  {url}/openapi/v1.json
  ```

---

## Booking References

Booking references use this format:  
`BK{timestamp}{random}`  
Where timestamp is `yyyyMMddHHmmss` and random is `100-999`.

**Example:**  
```
BK20250715120000001
```
> (The large seed dataset does not follow this format.)

---

## Project Structure

### Repositories

I used a repository pattern to organize data fetching.

### Tests

I used integration tests to test the required functionality.

### DTOs

I used DTOs (Data Transfer Objects) to contain the data being transported to the controllers and from the DB.

### Exceptions

I throw exceptions where it made sense, e.g. when an invalid booking is attempted.

---

## 6 Room Limit per Hotel

This is enforced with a **trigger function at the DB level**.  
(It could also be enforced at the API level, but the trigger method is more reliable as it will also prevent SQL inserts and updates.)

This will prevent any inserts into the Rooms table that exceed the room limit for the hotel (6).

**Example error:**
```json
{
  "error": "P0001: Hotel cannot have more than 6 rooms. Current hotel has 6 rooms."
}
```

