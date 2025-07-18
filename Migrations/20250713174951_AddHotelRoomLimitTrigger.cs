using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelRoomLimitTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the trigger function
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION check_hotel_room_limit()
                RETURNS TRIGGER AS $$
                BEGIN
                    IF (SELECT COUNT(*) FROM ""Rooms"" WHERE ""HotelId"" = NEW.""HotelId"") >= 6 THEN
                        RAISE EXCEPTION 'Hotel cannot have more than 6 rooms. Current hotel has % rooms.', 
                            (SELECT COUNT(*) FROM ""Rooms"" WHERE ""HotelId"" = NEW.""HotelId"");
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Create the trigger
            migrationBuilder.Sql(@"
                CREATE TRIGGER hotel_room_limit_trigger
                    BEFORE INSERT OR UPDATE ON ""Rooms""
                    FOR EACH ROW
                    EXECUTE FUNCTION check_hotel_room_limit();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS hotel_room_limit_trigger ON \"Rooms\";");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS check_hotel_room_limit();");
        }
    }
}
