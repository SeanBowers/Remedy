using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Remedy.Models;

namespace Remedy.Data
{
    public class ApplicationDbContext : IdentityDbContext<BTUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public virtual DbSet<Company>? Companies { get; set; } = default!;
        public virtual DbSet<Invite>? Invites { get; set; } = default!;
        public virtual DbSet<Notification>? Notifications { get; set; } = default!;
        public virtual DbSet<NotificationType>? NotificationTypes { get; set; } = default!;
        public virtual DbSet<Project>? Projects { get; set; } = default!;
        public virtual DbSet<ProjectPriority>? ProjectPriorities { get; set; } = default!;
        public virtual DbSet<Ticket>? Tickets { get; set; } = default!;
        public virtual DbSet<TicketAttachment>? TicketAttachments { get; set; } = default!;
        public virtual DbSet<TicketComment>? TicketComments { get; set; } = default!;
        public virtual DbSet<TicketHistory>? TicketHistories { get; set; } = default!;
        public virtual DbSet<TicketPriority>? TicketPriorities { get; set; } = default!;
        public virtual DbSet<TicketStatus>? TicketStatuses { get; set; } = default!;
        public virtual DbSet<TicketType>? TicketTypes { get; set; } = default!;
    }
}