using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string? ProfilePictureUrl { get; set; }

        // Navigation Properties
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Like> Like { get; set; } = new List<Like>();
        public ICollection<Comment> Comment { get; set; } = new List<Comment>();
    }
}
