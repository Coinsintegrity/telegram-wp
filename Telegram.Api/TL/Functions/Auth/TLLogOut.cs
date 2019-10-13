﻿// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Auth
{
    public class TLLogOut : TLObject
    {
        public const string Signature = "#5717da40";

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature));
        }
    }
}
