using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Umbraco.Core;
using Umbraco.Web.Routing;

using umbraco;
using umbraco.IO;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web
{
	/// <summary>
	/// Resolves NiceUrls for a given node id
	/// </summary>
    internal class NiceUrlResolver
    {
		public NiceUrlResolver(ContentStore contentStore, UmbracoContext umbracoContext)
        {
            _umbracoContext = umbracoContext;
            _contentStore = contentStore;
        }

        private readonly UmbracoContext _umbracoContext;
        private readonly ContentStore _contentStore;

        // note: this could be a parameter...
        const string UrlNameProperty = "@urlName";

		#region GetNiceUrl

		public string GetNiceUrl(int nodeId)
		{
			return GetNiceUrl(nodeId, _umbracoContext.UmbracoUrl, false);
		}

		public string GetNiceUrl(int nodeId, Uri current, bool absolute)
        {
        	string path;
			Uri domainUri;

			// will not read cache if previewing!
        	var route = _umbracoContext.InPreviewMode
				? null
				: _umbracoContext.RoutesCache.GetRoute(nodeId);

            if (route != null)
            {
				// route is <id>/<path> eg "-1/", "-1/foo", "123/", "123/foo/bar"...
                int pos = route.IndexOf('/');
                path = route.Substring(pos);
				int id = int.Parse(route.Substring(0, pos)); // will be -1 or 1234
				domainUri = id > 0 ? DomainUriAtNode(id, current) : null;
			}
			else
			{
				var node = _contentStore.GetNodeById(nodeId);
				if (node == null)
					return "#"; // legacy wrote to the log here...

				var pathParts = new List<string>();
				int id = nodeId;
				domainUri = DomainUriAtNode(id, current);
				while (domainUri == null && id > 0)
				{
					pathParts.Add(_contentStore.GetNodeProperty(node, UrlNameProperty));
					node = _contentStore.GetNodeParent(node);
					id = int.Parse(_contentStore.GetNodeProperty(node, "@id")); // will be -1 or 1234
					domainUri = id > 0 ? DomainUriAtNode(id, current) : null;
	            }

				// no domain, respect HideTopLevelNodeFromPath for legacy purposes
				if (domainUri == null && global::umbraco.GlobalSettings.HideTopLevelNodeFromPath)
					pathParts.RemoveAt(pathParts.Count - 1);

				pathParts.Reverse();
				path = "/" + string.Join("/", pathParts); // will be "/" or "/foo" or "/foo/bar" etc
				route = id.ToString() + path;

				if (!_umbracoContext.InPreviewMode)
					_umbracoContext.RoutesCache.Store(nodeId, route);
			}

			return AssembleUrl(domainUri, path, current, absolute).ToString();
		}

		Uri AssembleUrl(Uri domain, string path, Uri current, bool absolute)
		{
			Uri uri;

			if (domain == null)
			{
				// no domain was found : return a relative url,  add vdir if any
				uri = new Uri(global::umbraco.IO.SystemDirectories.Root + path, UriKind.Relative);
			}
			else
			{
				// a domain was found : return an absolute or relative url
				// cannot handle vdir, has to be in domain uri
				if (!absolute && current != null && domain.GetLeftPart(UriPartial.Authority) == current.GetLeftPart(UriPartial.Authority))
					uri = new Uri(domain.AbsolutePath.TrimEnd('/') + path, UriKind.Relative); // relative
				else
					uri = new Uri(domain.GetLeftPart(UriPartial.Path).TrimEnd('/') + path); // absolute
			}

			return UriFromUmbraco(uri);
		}

		Uri DomainUriAtNode(int nodeId, Uri current)
		{
			// be safe
			if (nodeId <= 0)
				return null;

			// apply filter on domains defined on that node
			var domainAndUri = Domains.ApplicableDomains(Domain.GetDomainsById(nodeId), current, true);
			return domainAndUri == null ? null : domainAndUri.Uri;
		}

		#endregion

		#region Map public urls to/from umbraco urls

		// fixme - what about vdir?
		// path = path.Substring(UriUtility.AppVirtualPathPrefix.Length); // remove virtual directory

		public static Uri UriFromUmbraco(Uri uri)
		{
			var path = uri.GetSafeAbsolutePath();
			if (path == "/")
				return uri;

			if (!global::umbraco.GlobalSettings.UseDirectoryUrls)
				path += ".aspx";
			else if (global::umbraco.UmbracoSettings.AddTrailingSlash)
				path += "/";

			return uri.Rewrite(path);
		}

		public static Uri UriToUmbraco(Uri uri)
		{
			var path = uri.GetSafeAbsolutePath();

			path = path.ToLower();
			if (path != "/")
				path = path.TrimEnd('/');
			if (path.EndsWith(".aspx"))
				path = path.Substring(0, path.Length - ".aspx".Length);

			return uri.Rewrite(path);
		}

		#endregion
	}
}