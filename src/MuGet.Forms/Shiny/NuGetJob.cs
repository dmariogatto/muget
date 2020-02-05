using MuGet.Forms.Localisation;
using MuGet.Forms.Services;
using Shiny.Jobs;
using Shiny.Notifications;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MuGet.Forms
{
    public class NuGetJob : IJob
    {
        private readonly INuGetService _nuGetService;
        private readonly INotificationManager _notifications;

        public NuGetJob(INuGetService nuGetService, INotificationManager notifications)
        {
            _nuGetService = nuGetService;
            _notifications = notifications;
        }

        public async Task<bool> Run(JobInfo jobInfo, CancellationToken cancelToken)
        {
#if DEBUG
            try
            {
                await _notifications.Send(new Notification()
                {
                    Id = GetHashCode(),
                    Title = "Running background fetch",
                    Message = DateTime.Now.ToString("g"),
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
#endif

            var favouritePackages = _nuGetService.GetFavouritePackages();

            if (favouritePackages.Any())
            {
                var includePrerelease = _nuGetService.IncludePrerelease;
                foreach (var fp in favouritePackages)
                {
                    var catalogEntries = await _nuGetService.GetCatalogEntries(fp.PackageId, cancelToken);
                    var latestEntries = catalogEntries
                        .Where(e => (includePrerelease || !e.PackVersion.IsPrerelease) &&
                                    e.Published > fp.Published)
                        .OrderByDescending(e => e.Published).ToList();

                    if (latestEntries.Any())
                    {
                        // Only show latest package if this a triggered from development settings
                        if (fp.Published == DateTime.MinValue)
                            latestEntries.RemoveRange(1, latestEntries.Count - 1);

                        fp.Version = latestEntries.First().Version;
                        fp.Published = latestEntries.First().Published;                        

                        _nuGetService.UpsertFavouritePackage(fp);

                        try
                        {
                            foreach (var le in latestEntries)
                            {
                                await _notifications.Send(new Notification()
                                {
                                    Id = le.Id.GetHashCode(),
                                    Title = string.Format(Resources.ItemParenthesesItem, le.Id, le.Version),
                                    Message = string.Format(Resources.NotificationContentFormat, le.Authors, le.Published.ToShortDateString()),
                                    Payload = $"{le.Id},{le.Version}",
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex);
                        }
                    }                    
                }

                return true;
            }
                        
            return false;
        }
    }
}
