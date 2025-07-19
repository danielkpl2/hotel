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

> **Possible improvements:**
> - When searching for rooms for 4 people, if there are 2 doubles and 1 single available, the single should not be returned since it cannot accommodate the group of 4. However, if there are 2 singles and 2 doubles available, the 2 singles could be included in the results, as the user could select 2 singles and 1 double to accommodate all 4 people.
> - For room searches, in addition to specifying the number of people, it would be beneficial to allow users to specify the number of rooms required. This would enable more precise querying and better match

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
  http://localhost:5189/openapi/v1.json
  ```
- The interactive Swagger UI is available at:
  ```
  http://localhost:5189/swagger
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

> **Note:**  
> There is a small naming clash between the namespace `Hotel` and the model name `Hotel`.  
> I'm aware of this but it's a bit of a hassle to change it now so I left it as is.

