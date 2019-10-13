﻿// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLDHGenBase : TLObject
    {
        public TLInt128 Nonce { get; set; }

        public TLInt128 ServerNonce { get; set; }

        public TLInt128 NewNonce { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            Nonce = GetObject<TLInt128>(bytes, ref position);
            ServerNonce = GetObject<TLInt128>(bytes, ref position);
            NewNonce = GetObject<TLInt128>(bytes, ref position);

            return this;
        }
    }

    public class TLDHGenOk : TLDHGenBase
    {
        public const uint Signature = TLConstructors.TLDHGenOk;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            TLUtils.WriteLine("--Parse TLDHGenOk--");
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return base.FromBytes(bytes, ref position);
        }
    }

    public class TLDHGenRetry : TLDHGenBase
    {
        public const uint Signature = TLConstructors.TLDHGenRetry;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            TLUtils.WriteLine("--Parse TLDHGenRetry--");
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return base.FromBytes(bytes, ref position);
        }
    }

    public class TLDHGenFail : TLDHGenBase
    {
        public const uint Signature = TLConstructors.TLDHGenFail;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            TLUtils.WriteLine("--Parse TLDHGenFail--");
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return base.FromBytes(bytes, ref position);
        }
    }


}
