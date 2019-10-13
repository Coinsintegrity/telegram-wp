﻿// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;

namespace TelegramClient.Controls.StartView
{
    internal abstract class InputCompletedArgs : InputBaseArgs
    {
        protected InputCompletedArgs(UIElement source, Point origin)
            : base(source, origin)
        {
        }

        public abstract Point TotalTranslation { get; }

        public abstract Point FinalLinearVelocity { get; }

        public abstract bool IsInertial { get; }
    }
}
