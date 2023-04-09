namespace Walto
{
    public class Card
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string[] Permissions { get; set; }
        public DateTime CreationStamp { get; set; }
        public DateTime ExpiryStamp { get; set; }

        public Card(string id, string userName, int userId, string[] permissions, DateTime creationStamp, DateTime expiryStamp)
        {
            Id = id;
            UserName = userName;
            UserId = userId;
            Permissions = permissions;
            CreationStamp = creationStamp;
            ExpiryStamp = expiryStamp;
        }
    }
}
