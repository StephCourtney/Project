using System.Data.Entity;

namespace AzureMediaPortal.Models
{
    public class AzureMediaPortalContext : DbContext
    {

        public AzureMediaPortalContext() : base("name=AzureMediaPortalContext")
        {
        }

        public DbSet<MediaElement> MediaElements { get; set; }

        public DbSet<Post> Posts { get; set; }

        
    }
}
