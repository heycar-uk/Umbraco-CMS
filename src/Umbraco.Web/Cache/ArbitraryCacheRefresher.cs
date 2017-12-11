using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    public class ArbitraryCacheRefresher : JsonCacheRefresherBase<ArbitraryCacheRefresher>
    {
        public static readonly Guid RefresherTypeId = Guid.Parse("a65e1f93-dd89-4844-b729-bf524f101277");

        protected override ArbitraryCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return RefresherTypeId; }
        }

        public override string Name
        {
            get { return "Cache refresher for arbitrary keys"; }
        }

        public override void Refresh(string jsonPayload)
        {
            if (jsonPayload.EndsWith("*"))
            {
                string prefix = jsonPayload.TrimEnd('*');
                foreach (var key in HttpRuntime.Cache.OfType<DictionaryEntry>()
                                                    .Select(de => de.Key as string)
                                                    .Where(k => k.StartsWith(prefix)))
                {
                    Umbraco.Core.Logging.LogHelper.Debug<ArbitraryCacheRefresher>("Removed cache key {0}", () => key);
                    HttpRuntime.Cache.Remove(key);
                }
            }
            else
            {
                Umbraco.Core.Logging.LogHelper.Debug<ArbitraryCacheRefresher>("Removed cache key {0}", () => jsonPayload);

                HttpRuntime.Cache.Remove(jsonPayload);
            }

            base.Refresh(jsonPayload);
        }
    }
}