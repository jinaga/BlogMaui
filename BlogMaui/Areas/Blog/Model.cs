using Jinaga;

namespace BlogMaui.Areas.Blog;

[FactType("Blog.User.Name")]
public partial record UserName(User user, string value, UserName[] prior) { }

[FactType("Blog.Site")]
public partial record Site(User creator, DateTime createdAt) { }

[FactType("Blog.Site.Name")]
public partial record SiteName(Site site, string value, SiteName[] prior) { }

[FactType("Blog.Site.Domain")]
public partial record SiteDomain(Site site, string value, SiteDomain[] prior) { }

[FactType("Blog.Post")]
public partial record Post(Site site, User author, DateTime createdAt) { }

[FactType("Blog.Post.Title")]
public partial record PostTitle(Post post, string value, PostTitle[] prior) { }

[FactType("Blog.Post.Deleted")]
public partial record PostDeleted(Post post, DateTime deletedAt) { }

[FactType("Blog.Post.Publish")]
public partial record Publish(Post post, DateTime date) { }