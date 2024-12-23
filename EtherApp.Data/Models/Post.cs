using System.ComponentModel.DataAnnotations;

namespace EtherApp.Data.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public int NrOfReports { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        //foreign key
        public int UserId { get; set; }

        //Navigation properties
        public User User { get; set; }
        public ICollection<Like> Like { get; set; } = new List<Like>();
        public ICollection<Comment> Comment { get; set; } = new List<Comment>();
        
    }
}
