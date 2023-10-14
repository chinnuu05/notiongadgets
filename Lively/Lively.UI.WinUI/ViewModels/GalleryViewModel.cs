﻿using Lively.Gallery.Client;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using System.Diagnostics;
using CommunityToolkit.Common.Collections;
using System.IO;
using Lively.Models;
using Lively.Models.Gallery.API;
using Lively.UI.WinUI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lively.UI.WinUI.ViewModels
{
    public partial class GalleryViewModel : ObservableObject, IIncrementalSource<GalleryModel>
    {
        [ObservableProperty]
        private IncrementalLoadingCollection<GalleryViewModel, GalleryModel> wallpapers;
        private readonly DispatcherQueue dispatcherQueue;
        private int currentPage = 0;

        private readonly GalleryClient galleryClient;
        private readonly IDialogService dialogService;
        private readonly LibraryViewModel libraryVm;
        private readonly ICacheService cacheService;

        public GalleryViewModel(GalleryClient galleryClient,
            LibraryViewModel libraryVm,
            IDialogService dialogService,
            ICacheService cacheService)
        {
            this.libraryVm = libraryVm;
            this.dialogService = dialogService;
            this.galleryClient = galleryClient;
            this.cacheService = cacheService;

            if (!galleryClient.IsLoggedIn)
                return;

            dispatcherQueue = DispatcherQueue.GetForCurrentThread() ?? DispatcherQueueController.CreateOnCurrentThread().DispatcherQueue;
            Wallpapers = new IncrementalLoadingCollection<GalleryViewModel, GalleryModel>(this);

            galleryClient.LoggedIn += (s, id) =>
            {
                dispatcherQueue.TryEnqueue(async () =>
                {
                    currentPage = 0;
                    await Wallpapers?.RefreshAsync();
                });
            };

            libraryVm.WallpaperDeleted += (s, id) =>
            {
                var item = Wallpapers.FirstOrDefault(x => x.LivelyInfo.Id == id);
                if (item != null)
                {
                    item.IsInstalled = false;
                }
            };

            libraryVm.WallpaperDownloadCompleted += (s, id) =>
            {
                var item = Wallpapers.FirstOrDefault(x => x.LivelyInfo.Id == id);
                if (item != null)
                {
                    item.DownloadingProgressText = "-/- MB";
                    item.DownloadingProgress = 0;
                    item.IsDownloading = false;
                    item.IsInstalled = true;
                }
            };

            libraryVm.WallpaperDownloadFailed += (s, id) =>
            {
                var item = Wallpapers.FirstOrDefault(x => x.LivelyInfo.Id == id);
                if (item != null)
                {
                    item.DownloadingProgressText = "-/- MB";
                    item.DownloadingProgress = 0;
                    item.IsDownloading = false;
                }
            };

            libraryVm.WallpaperDownloadProgress += (s, e) =>
            {
                var item = Wallpapers.FirstOrDefault(x => x.LivelyInfo.Id == e.Item1);
                if (item != null)
                {
                    item.DownloadingProgressText = $"{e.Item3:0.##}/{e.Item4:0.##} MB";
                    item.DownloadingProgress = e.Item2;
                    item.IsDownloading = true;
                }
            };
        }

        public async Task<IEnumerable<GalleryModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            var page = await galleryClient.SearchWallpapers(new SearchQueryBuilder().SortBy(SortingType.Newest)
                .SetPage(currentPage++)
                .SetLimit(10)
                .Build());
            Debug.WriteLine($"Loading -> page: {currentPage} wallpapers: {page?.Data?.Count}");
            var items = new List<GalleryModel>();
            foreach (var item in page?.Data)
            {
                var obj = new GalleryModel(item, libraryVm.LibraryItems.Any(x => item.Id == x.LivelyInfo.Id));
                _ = SetCacheImage(obj);
                items.Add(obj);
            }
            return items;
        }

        private async Task SetCacheImage(GalleryModel obj)
        {
            var uri = new Uri(obj.LivelyInfo.Preview ?? obj.LivelyInfo.Thumbnail);
            obj.Image = await cacheService.GetFileFromCacheAsync(uri);      
        }

        private RelayCommand<GalleryModel> _downloadCommand;
        public RelayCommand<GalleryModel> DownloadCommand =>
            _downloadCommand ??= new RelayCommand<GalleryModel>(async (obj) => {
                obj.IsDownloading = true;
                await libraryVm.AddWallpaperGallery(obj);
            });

        private RelayCommand<GalleryModel> _cancelCommand;
        public RelayCommand<GalleryModel> CancelCommand =>
            _cancelCommand ??= new RelayCommand<GalleryModel>((obj) => libraryVm.CancelDownload(obj.LivelyInfo.Id));
    }
}