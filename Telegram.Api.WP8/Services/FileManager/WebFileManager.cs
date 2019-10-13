﻿// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System;
using System.Linq;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.TL;

namespace Telegram.Api.Services.FileManager
{
    public class WebFileManager : FileManagerBase, IWebFileManager
    {
        public WebFileManager(ITelegramEventAggregator eventAggregator, IMTProtoService mtProtoService)
            : base(eventAggregator, mtProtoService)
        {
            for (var i = 0; i < Constants.WorkersNumber; i++)
            {
                var worker = new Worker(OnDownloading, "webDownloader"+i);
                _workers.Add(worker);
            }
        }

        private void OnDownloading(object state)
        {
            DownloadablePart part = null;
            lock (_itemsSyncRoot)
            {
                for (var i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    if (item.Canceled)
                    {

                        _items.RemoveAt(i--);

                        try
                        {
                            _eventAggregator.Publish(new DownloadingCanceledEventArgs(item));
                        }
                        catch (Exception e)
                        {
                            TLUtils.WriteException(e);
                        }
                    }
                }

                foreach (var item in _items)
                {
                    part = item.Parts.FirstOrDefault(x => x.Status == PartStatus.Ready);
                    if (part != null)
                    {
                        part.Status = PartStatus.Processing;
                        break;
                    }
                }
            }

            if (part == null)
            {
                var currentWorker = (Worker)state;
                currentWorker.Stop();
                return;
            }

            bool canceled;
            ProcessFilePart(part, part.ParentItem.DCId, part.ParentItem.InputLocation, out canceled);
            if (canceled)
            {
                lock (_itemsSyncRoot)
                {
                    part.ParentItem.Canceled = true;
                    part.Status = PartStatus.Processed;
                    _items.Remove(part.ParentItem);
                }

                return;
            }

            // indicate progress
            // indicate complete
            bool isComplete;
            bool isCanceled;
            var progress = 0.0;
            lock (_itemsSyncRoot)
            {
                part.Status = PartStatus.Processed;

                var data = part.File.Bytes.Data;
                if (data.Length < part.Limit.Value && (part.Number + 1) != part.ParentItem.Parts.Count)
                {
                    var complete = part.ParentItem.Parts.All(x => x.Status == PartStatus.Processed);
                    if (!complete)
                    {
                        var emptyBufferSize = part.Limit.Value - data.Length;
                        var position = data.Length;

                        var missingPart = new DownloadablePart(part.ParentItem, new TLInt(position), new TLInt(emptyBufferSize), -part.Number);

                        var currentItemIndex = part.ParentItem.Parts.IndexOf(part);
                        part.ParentItem.Parts.Insert(currentItemIndex + 1, missingPart);
                    }
                }

                isCanceled = part.ParentItem.Canceled;

                isComplete = part.ParentItem.Parts.All(x => x.Status == PartStatus.Processed);
                if (!isComplete)
                {
                    var downloadedCount = part.ParentItem.Parts.Count(x => x.Status == PartStatus.Processed);
                    var count = part.ParentItem.Parts.Count;
                    progress = (double)downloadedCount / count;
                }
                else
                {
                    _items.Remove(part.ParentItem);
                }
            }

            if (!isCanceled)
            {
                if (isComplete)
                {
                    byte[] bytes = { };
                    foreach (var p in part.ParentItem.Parts)
                    {
                        bytes = TLUtils.Combine(bytes, p.File.Bytes.Data);
                    }
                    //part.ParentItem.Location.Buffer = bytes;

                    var fileName = part.ParentItem.FileName.ToString();

                    StringLocker.Lock(fileName, () => FileUtils.WriteBytes(fileName, bytes));

                    if (part.ParentItem.Callback != null)
                    {
                        Execute.BeginOnThreadPool(() =>
                        {
                            part.ParentItem.Callback.SafeInvoke(part.ParentItem);
                            if (part.ParentItem.Callbacks != null)
                            {
                                foreach (var callback in part.ParentItem.Callbacks)
                                {
                                    callback.SafeInvoke(part.ParentItem);
                                }
                            }
                        });
                    }
                    else
                    {
                        Execute.BeginOnThreadPool(() => _eventAggregator.Publish(part.ParentItem));
                    }
                }
                else
                {
                    //Execute.BeginOnThreadPool(() => _eventAggregator.Publish(new ProgressChangedEventArgs(part.ParentItem, progress)));
                }
            }
        }

        protected DownloadableItem GetDownloadableItem(TLInt dcId, TLInputWebFileGeoPointLocation location, string fileName, TLObject owner, TLInt fileSize, Action<DownloadableItem> callback)
        {
            var item = new DownloadableItem
            {
                Owner = owner,
                DCId = dcId,
                Callback = callback,
                InputLocation = location
            };
            item.Parts = GetItemParts(fileSize, item);

            return item;
        }

        public void DownloadFile(TLInt dcId, TLInputWebFileGeoPointLocation file, string fileName, TLObject owner)
        {
            DownloadFile(dcId, file, fileName, owner, null);
        }

        public void DownloadFile(TLInt dcId, TLInputWebFileGeoPointLocation file, string fileName, TLObject owner, Action<DownloadableItem> callback)
        {
            var downloadableItem = GetDownloadableItem(dcId, file, fileName, owner, new TLInt(0), callback);
            downloadableItem.FileName = new TLString(fileName);
            lock (_itemsSyncRoot)
            {
                bool addFile = true;
                foreach (var item in _items)
                {
                    if (item.InputLocation.LocationEquals(file)
                        && item.Owner == owner)
                    {
                        addFile = false;
                        break;
                    }
                }

                if (addFile)
                {
                    _items.Add(downloadableItem);
                }
            }

            StartAwaitingWorkers();
        }
    }
}
