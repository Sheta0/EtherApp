namespace EtherApp.ViewModels.Friends
{
    public class UserWithFriendsCountVM
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int FriendsCount { get; set; }
        public string FriendsCountText => FriendsCount == 0 ? "No friends" : FriendsCount == 1 ? "1 friend" : $"{FriendsCount} friends";
        
    }
}
