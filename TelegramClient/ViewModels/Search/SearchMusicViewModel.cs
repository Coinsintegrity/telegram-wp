﻿// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Media;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Search
{
    public class SearchMusicViewModel : FilesViewModelBase<IInputPeer>
    {
        public string Text { get; set; }

        public override TLInputMessagesFilterBase InputMessageFilter
        {
            get { return new TLInputMessagesFilterMusic(); }
        }

        public SearchMusicViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);

            Status = AppResources.SearchSharedMusic;

            if (StateService.CurrentInputPeer != null)
            {
                CurrentItem = StateService.CurrentInputPeer;
                StateService.CurrentInputPeer = null;
            }

            if (StateService.Source != null)
            {
                _source = StateService.Source;
                StateService.Source = null;
            }
        }

        protected override bool SkipMessage(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage;
            if (message == null)
            {
                return true;
            }

            var mediaDocument = message.Media as TLMessageMediaDocument;
            if (mediaDocument == null)
            {
                return true;
            }

            var document = mediaDocument.Document as TLDocument22;
            if (document == null)
            {
                return true;
            }

            var audioAttribute = document.Attributes.FirstOrDefault(x => x is TLDocumentAttributeAudio46) as TLDocumentAttributeAudio46;
            if (audioAttribute == null || audioAttribute.Voice)
            {
                return true;
            }

            return false;
        }

        #region Searching

        private SearchDocumentsRequest _lastDocumentsRequest;

        private readonly List<TLMessageBase> _source;

        private readonly LRUCache<string, SearchDocumentsRequest> _searchResultsCache = new LRUCache<string, SearchDocumentsRequest>(Constants.MaxCacheCapacity);

        public void Search()
        {
            if (_lastDocumentsRequest != null)
            {
                _lastDocumentsRequest.Cancel();
            }

            var text = Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                LazyItems.Clear();
                Items.Clear();
                Status = string.IsNullOrEmpty(Text) ? AppResources.SearchAmongYourFiles : AppResources.NoResults;
                return;
            }

            SearchDocumentsRequest nextDocumentsRequest;
            if (!_searchResultsCache.TryGetValue(text, out nextDocumentsRequest))
            {
                IList<TLMessageBase> source;

                if (_lastDocumentsRequest != null
                    && text.IndexOf(_lastDocumentsRequest.Text, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    source = _lastDocumentsRequest.Source;
                }
                else
                {
                    source = _source;
                }

                nextDocumentsRequest = new SearchDocumentsRequest(CurrentItem.ToInputPeer(), text, source);
            }

            IsWorking = true;
            nextDocumentsRequest.ProcessAsync(results =>
                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    if (nextDocumentsRequest.IsCanceled) return;

                    Status = results.Count > 0 ? string.Empty : Status;
                    Items.Clear();
                    LazyItems.Clear();
                    for (var i = 0; i < results.Count; i++)
                    {
                        if (i < 6)
                        {
                            Items.Add((TLMessage)results[i]);
                        }
                        else
                        {
                            LazyItems.Add((TLMessage)results[i]);
                        }
                    }

                    IsWorking = false;
                    NotifyOfPropertyChange(() => IsEmptyList);

                    if (LazyItems.Count > 0)
                    {
                        PopulateItems(() => ProcessGlobalSearch(nextDocumentsRequest));
                    }
                    else
                    {
                        ProcessGlobalSearch(nextDocumentsRequest);
                    }
                }));

            _searchResultsCache[nextDocumentsRequest.Text] = nextDocumentsRequest;
            _lastDocumentsRequest = nextDocumentsRequest;
        }

        private void ProcessGlobalSearch(SearchDocumentsRequest nextDocumentsRequest)
        {
            if (nextDocumentsRequest.GlobalResults != null)
            {
                if (nextDocumentsRequest.GlobalResults.Count > 0)
                {
                    BeginOnUIThread(() =>
                    {
                        if (nextDocumentsRequest.IsCanceled) return;

                        foreach (var result in nextDocumentsRequest.GlobalResults)
                        {
                            Items.Add((TLMessage)result);
                        }
                        NotifyOfPropertyChange(() => IsEmptyList);
                        Status = Items.Count > 0 ? string.Empty : AppResources.NoResults;
                    });
                }
            }
            else
            {
                IsWorking = true;
                MTProtoService.SearchAsync(
                    nextDocumentsRequest.InputPeer,
                    new TLString(nextDocumentsRequest.Text),
                    null,
                    InputMessageFilter,
                    new TLInt(0), 
                    new TLInt(0), 
                    new TLInt(0), 
                    new TLInt(0), 
                    new TLInt(100),
                    new TLInt(0),
                    result =>
                    {
                        IsWorking = false;
                        nextDocumentsRequest.GlobalResults = new List<TLMessageBase>(result.Messages.Count);

                        foreach (var message in result.Messages)
                        {
                            if (nextDocumentsRequest.ResultsIndex == null
                                || !nextDocumentsRequest.ResultsIndex.ContainsKey(message.Index))
                            {
                                nextDocumentsRequest.GlobalResults.Add(message);
                            }
                        }


                        BeginOnUIThread(() =>
                        {
                            if (nextDocumentsRequest.IsCanceled) return;

                            if (nextDocumentsRequest.GlobalResults.Count > 0)
                            {
                                foreach (var message in nextDocumentsRequest.GlobalResults)
                                {
                                    Items.Add((TLMessage)message);
                                }
                                NotifyOfPropertyChange(() => IsEmptyList);
                            }

                            Status = Items.Count > 0 ? string.Empty : AppResources.NoResults;
                        });

                    },
                    error =>
                    {
                        IsWorking = false;

                        if (TLRPCError.CodeEquals(error, ErrorCode.BAD_REQUEST)
                            && TLRPCError.TypeEquals(error, ErrorType.QUERY_TOO_SHORT))
                        {
                            nextDocumentsRequest.GlobalResults = new List<TLMessageBase>();
                        }
                        else if (TLRPCError.CodeEquals(error, ErrorCode.FLOOD))
                        {
                            nextDocumentsRequest.GlobalResults = new List<TLMessageBase>();
                            BeginOnUIThread(() => MessageBox.Show(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, MessageBoxButton.OK));
                        }

                        Execute.ShowDebugMessage("messages.search error " + error);
                    });
            }
        }

        #endregion
    }
}
