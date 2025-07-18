-- Clear existing data first
DELETE FROM "BookingRooms";
DELETE FROM "Bookings";
DELETE FROM "Rooms";
DELETE FROM "Hotels";
DELETE FROM "RoomTypes";

-- Insert Room Types
INSERT INTO "RoomTypes" ("Id", "Name", "MaxOccupancy") VALUES
(1, 'Single', 1),
(2, 'Double', 2),
(3, 'Deluxe', 3);

-- Insert Hotels
INSERT INTO "Hotels" ("Id", "Name", "Address", "PhoneNumber", "Email") VALUES
(1, 'The Westminster Palace Hotel', '10 Downing Street, Westminster, London SW1A 2AA', '+44-20-7930-4832', 'reservations@westminsterpalace.co.uk'),
(2, 'Big Ben Tower Suites', '123 Sesame Street, Camden, London NW1 8QG', '+44-20-7387-9876', 'bookings@bigbensuites.co.uk'),
(3, 'Buckingham Gardens Lodge', '221B Baker Street, Marylebone, London NW1 6XE', '+44-20-7224-3688', 'stay@buckinghamgardens.co.uk');

-- Insert Rooms (6 rooms per hotel: 2 Single, 2 Double, 2 Deluxe)
INSERT INTO "Rooms" ("Id", "RoomNumber", "Price", "HotelId", "RoomTypeId") VALUES
-- The Westminster Palace Hotel (Hotel ID: 1)
(1, '101', 120.00, 1, 1),  -- Single
(2, '102', 125.00, 1, 1),  -- Single
(3, '201', 180.00, 1, 2),  -- Double
(4, '202', 185.00, 1, 2),  -- Double
(5, '301', 280.00, 1, 3),  -- Deluxe
(6, '302', 290.00, 1, 3),  -- Deluxe

-- Big Ben Tower Suites (Hotel ID: 2)
(7, '101', 150.00, 2, 1),  -- Single
(8, '102', 155.00, 2, 1),  -- Single
(9, '201', 220.00, 2, 2),  -- Double
(10, '202', 225.00, 2, 2), -- Double
(11, '301', 350.00, 2, 3), -- Deluxe
(12, '302', 360.00, 2, 3), -- Deluxe

-- Buckingham Gardens Lodge (Hotel ID: 3)
(13, '101', 100.00, 3, 1), -- Single
(14, '102', 105.00, 3, 1), -- Single
(15, '201', 160.00, 3, 2), -- Double
(16, '202', 165.00, 3, 2), -- Double
(17, '301', 250.00, 3, 3), -- Deluxe
(18, '302', 260.00, 3, 3); -- Deluxe

-- Insert Sample Bookings
INSERT INTO "Bookings" ("Id", "GuestName", "PeopleCount", "CheckInDate", "CheckOutDate", "TotalPrice", "BookingReference", "HotelId") VALUES
(1, 'Sherlock Holmes', 2, '2025-07-15', '2025-07-18', 540.00, 'BK20250715120000001', 1),        -- 3 nights at Westminster Palace
(2, 'Elmo Monster', 1, '2025-07-20', '2025-07-22', 240.00, 'BK20250720120000002', 2),           -- 2 nights at Big Ben Tower
(3, 'Cookie Monster', 3, '2025-07-25', '2025-07-28', 750.00, 'BK20250725120000003', 3),         -- 3 nights at Buckingham Gardens
(4, 'Wallace Gromit', 2, '2025-08-01', '2025-08-04', 555.00, 'BK20250801120000004', 1),         -- 3 nights at Westminster Palace
(5, 'Big Bird', 1, '2025-08-05', '2025-08-07', 210.00, 'BK20250805120000005', 3);               -- 2 nights at Buckingham Gardens

-- Insert Booking-Room relationships (junction table)
INSERT INTO "BookingRooms" ("BookingId", "RoomId") VALUES
-- Sherlock Holmes booking (Double room at Westminster Palace)
(1, 3),  -- Room 201

-- Elmo Monster booking (Single room at Big Ben Tower)
(2, 7),  -- Room 101

-- Cookie Monster booking (Deluxe room at Buckingham Gardens)
(3, 17), -- Room 301

-- Wallace Gromit booking (Double room at Westminster Palace)
(4, 4),  -- Room 202

-- Big Bird booking (Single room at Buckingham Gardens)
(5, 13); -- Room 101

-- Reset sequences to continue from the highest ID
SELECT setval('"Hotels_Id_seq"', (SELECT MAX("Id") FROM "Hotels"));
SELECT setval('"RoomTypes_Id_seq"', (SELECT MAX("Id") FROM "RoomTypes"));
SELECT setval('"Rooms_Id_seq"', (SELECT MAX("Id") FROM "Rooms"));
SELECT setval('"Bookings_Id_seq"', (SELECT MAX("Id") FROM "Bookings"));