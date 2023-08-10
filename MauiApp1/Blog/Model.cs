using Jinaga;

namespace MauiApp1.Blog;

[FactType("Blog.Site")]
public record Site(string domain) { }

[FactType("Blog.Post")]
public record Post(Site site, DateTime createdAt) { }

[FactType("Blog.Post.Title")]
public record PostTitle(Post post, string value, PostTitle[] prior) { }

[FactType("Blog.Post.Deleted")]
public record PostDeleted(Post post, DateTime deletedAt) { }