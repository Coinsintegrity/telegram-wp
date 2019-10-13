﻿// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Windows.Data;
using Telegram.Api.TL;
using TelegramClient.Resources;

namespace TelegramClient.Converters
{
    public class MessageStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageStatus)
            {
                var status = (MessageStatus) value;

                if (status == MessageStatus.Failed)
                    return string.Format("{0}", AppResources.SendingFailed);

                if (status == MessageStatus.Confirmed)
                    return string.Empty;

                if (status == MessageStatus.Sending)
                    //return string.Format("{0}...", AppResources.Sending);
                    return string.Empty;

                if (status == MessageStatus.Read)
                    return string.Empty;

                if (status == MessageStatus.Compressing)
                    return string.Format("{0}...", AppResources.Compressing);

                if (status == MessageStatus.Broadcast)
                    return string.Empty;

            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
