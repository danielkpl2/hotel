- Used dotnet 8.0:
https://dotnet.microsoft.com/en-us/download/dotnet/8.0

- To install:
dotnet restore

- To run the app:
dotnet run

- To run tests run:
dotnet test

- To create Postges DB and user:

CREATE USER hoteluser WITH PASSWORD 'hotelpassword';
ALTER USER hoteluser CREATEDB;

CREATE DATABASE "HotelDb" WITH OWNER = hoteluser ENCODING = 'UTF8' CONNECTION LIMIT = -1;
GRANT ALL PRIVILEGES ON DATABASE "HotelDb" TO hoteluser;

CREATE DATABASE "HotelDb_Test" WITH OWNER = hoteluser ENCODING = 'UTF8' CONNECTION LIMIT = -1;
GRANT ALL PRIVILEGES ON DATABASE "HotelDb_Test" TO hoteluser;

- The DB credentials are in appsettings files. I know they're not supposed to be committed but it doesn't matter here.

Presumptions made:

- Hotels have exactly 6 rooms. (rather than up to 6)

- a single room has a capacity of 1
- a double room has a capacity of 2
- a deluxe room has a capacity of 3

- room availability:

If the number of available rooms at the hotel between requested dates is greater than or equal to the requested amount,
all of the available rooms get returned, then the user can select the combination of the rooms they want. This could be further improved to account for more use cases such as:
if requesting 4 people and have 2 doubles and 1 single, don't return the single because it can't accomodate the required 4.
But if have 2 singles and 2 doubles, the 2 singles could be returned because the user can take 2 singles and 1 double.


- I decided to seed the DB by executing a SQL file.
This minimises the work required and makes it easier to modify.
2 seed files are provided, small.sql, large.sql. 
The small.sql seed file is used for integration tests.

To seed data:
`POST /api/admin/seed/small` 

To clear the DB:
`DELETE /api/admin/seed/clear`


- the openapi/swagger file can be accessed at:
{url}/openapi/v1.json

- Booking references use this format: BK{timestamp}{random} where timestamp is yyyyMMddHHmmss and random is 100-999
e.g.: BK20250715120000001
(the large seed dataset does not follow this format)

- repositories
I used a repository pattern to organise data fetching.

- tests
I used integration tests to test the required functionality.


- DTOs
I used DTOs (Data Transfer Objects) to contain the data being transported to the controllers and from the DB.

- Exceptions
I throw exceptions where it made sense, e.g. when an invalid booking is attempted.

- 6 room limit per hotel:
 This is enforced with a trigger function at the DB level. 
 (It could also be enforced at the API level but the trigger method is more reliable as it will also prevent SQL inserts and updates.)

 This will prevent any inserts into the Rooms table that exceed the room limit for the hotel (6).
 {
    "error": "P0001: Hotel cannot have more than 6 rooms. Current hotel has 6 rooms."
}

