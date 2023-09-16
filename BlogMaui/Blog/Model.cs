using Jinaga;

namespace BlogMaui.Blog;

[FactType("Blog.Site")]
public record Site(User creator, string domain) { }

[FactType("Blog.Post")]
public record Post(Site site, DateTime createdAt) { }

[FactType("Blog.Publish")]
public record Publish(Post post, DateTime date) { }

[FactType("Blog.Post.Title")]
public record PostTitle(Post post, string value, PostTitle[] prior) { }

[FactType("Blog.Post.Deleted")]
public record PostDeleted(Post post, DateTime deletedAt) { }