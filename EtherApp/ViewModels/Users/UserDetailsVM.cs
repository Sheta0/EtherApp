using EtherApp.Data.Models;

namespace EtherApp.ViewModels.Users
{
    internal class UserDetailsVM
    {
        public User? User { get; set; }
        public List<Post>? Posts { get; set; }
    }
}