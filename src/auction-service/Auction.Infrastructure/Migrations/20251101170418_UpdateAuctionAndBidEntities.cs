using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuctionAndBidEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bid_time",
                table: "bids");

            migrationBuilder.AddColumn<string>(
                name: "bidder_email",
                table: "bids",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bidder_phone",
                table: "bids",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "bids",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "bids",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "bids",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "seller_email",
                table: "auctions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "seller_phone",
                table: "auctions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bidder_email",
                table: "bids");

            migrationBuilder.DropColumn(
                name: "bidder_phone",
                table: "bids");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "bids");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "bids");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "bids");

            migrationBuilder.DropColumn(
                name: "seller_email",
                table: "auctions");

            migrationBuilder.DropColumn(
                name: "seller_phone",
                table: "auctions");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "bid_time",
                table: "bids",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
