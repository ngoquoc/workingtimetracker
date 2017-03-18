namespace WorkingTimeTracker.Implementations.Migrations.WorkingTimeTracker
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TimeEntries",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Date = c.DateTimeOffset(nullable: false, precision: 7),
                        Note = c.String(),
                        Duration = c.Double(nullable: false),
                        OwnerId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.OwnerId, cascadeDelete: true)
                .Index(t => t.OwnerId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(),
                        Name = c.String(),
                        PreferredWorkingHourPerDay = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TimeEntries", "OwnerId", "dbo.Users");
            DropIndex("dbo.TimeEntries", new[] { "OwnerId" });
            DropTable("dbo.Users");
            DropTable("dbo.TimeEntries");
        }
    }
}
