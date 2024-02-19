using Jinaga;

namespace BlogMaui.Areas.Blog;

[FactType("Blog.User.Name")]
public record UserName(User user, string value, UserName[] prior) { }

[FactType("Blog.Site")]
public record Site(User creator, DateTime createdAt) { }

[FactType("Blog.Site.Deleted")]
public record SiteDeleted(Site site, DateTime deletedAt) { }

[FactType("Blog.Site.Restored")]
public record SiteRestored(SiteDeleted deleted) { }

[FactType("Blog.Site.Name")]
public record SiteName(Site site, string value, SiteName[] prior) { }

[FactType("Blog.Site.Domain")]
public record SiteDomain(Site site, string value, SiteDomain[] prior) { }

[FactType("Blog.Post")]
public record Post(Site site, User author, DateTime createdAt) { }

[FactType("Blog.Post.Title")]
public record PostTitle(Post post, string value, PostTitle[] prior) { }

[FactType("Blog.Post.Deleted")]
public record PostDeleted(Post post, DateTime deletedAt) { }

[FactType("Blog.Post.Restored")]
public record PostRestored(PostDeleted deleted) { }

[FactType("Blog.Post.Publish")]
public record Publish(Post post, DateTime date) { }