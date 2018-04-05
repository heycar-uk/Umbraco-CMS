using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using Umbraco.Web.SyncFileSystem;

namespace Umbraco.Web.Cache
{
    public class FileSystemCacheRefresher : CacheRefresherBase<FileSystemCacheRefresher>
    {
        public static readonly Guid RefresherTypeId = Guid.Parse("3dbae2fa-6094-4d41-8a9a-8cb0f40c78ec");

        protected override FileSystemCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return RefresherTypeId; }
        }

        public override string Name
        {
            get { return "Cache refresher for SyncFileSystemWrapper"; }
        }

        public override void Refresh(int id)
        {
            Umbraco.Core.Logging.LogHelper.Info<FileSystemCacheRefresher>("Received file update ID {0}", () => id);

            var dto = ApplicationContext.Current.DatabaseContext.Database.SingleOrDefault<CacheFileDto>(id);
            if ( dto != null)
            {
                Umbraco.Core.Logging.LogHelper.Info<FileSystemCacheRefresher>("Update for path {0}, action {1}, file is {2} bytes", () => dto.Path, () => dto.Action, () => dto.Data.Length);

                string targetPath = System.Web.Hosting.HostingEnvironment.MapPath(dto.Path);
                var action = (SyncFileSystemWrapper.Action)dto.Action;

                if (action == SyncFileSystemWrapper.Action.DeleteFile && File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
                else if (action == SyncFileSystemWrapper.Action.DeleteDirectory && Directory.Exists(targetPath))
                {
                    Directory.Delete(targetPath);
                }
                else if (action == SyncFileSystemWrapper.Action.Add)
                {
                    string directory = Path.GetDirectoryName(targetPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        
                        FileSystemProviderManager.Current.MediaFileSystem.ResetFolderCounter();
                    }
                    File.WriteAllBytes(targetPath, dto.Data);
                }
            }
        }
    }
}
