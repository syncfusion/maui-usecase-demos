namespace SocialMediaFeed
{
    public class Post
    {
        public string Name { get; set; }
        public string Caption { get; set; }
        public string ProfileImage { get; set; }
        public string PostImage { get; set; }

        public bool IsTextOnly { get; set; }
        public bool IsLinkPreview { get; set; }

        public string LinkTitle { get; set; }
        public string LinkUrl { get; set; }
        public string LinkImage { get; set; }
    }
}
