﻿// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLInputChatPhotoBase : TLObject { }

    public class TLInputChatPhotoEmpty : TLInputChatPhotoBase
    {
        public const uint Signature = TLConstructors.TLInputChatPhotoEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature));
        }
    }

    public class TLInputChatUploadedPhoto56 : TLInputChatPhotoBase
    {
        public const uint Signature = TLConstructors.TLInputChatUploadedPhoto56;

        public TLInputFile File { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            File = GetObject<TLInputFile>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes());
        }
    }

    public class TLInputChatUploadedPhoto : TLInputChatPhotoBase
    {
        public const uint Signature = TLConstructors.TLInputChatUploadedPhoto;

        public TLInputFile File { get; set; }

        public TLInputPhotoCropBase Crop { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            File = GetObject<TLInputFile>(bytes, ref position);
            Crop = GetObject<TLInputPhotoCropBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                Crop.ToBytes());
        }
    }

    public class TLInputChatPhoto56 : TLInputChatPhotoBase
    {
        public const uint Signature = TLConstructors.TLInputChatPhoto;

        public TLInputPhotoBase Id { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInputPhotoBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes());
        }
    }

    public class TLInputChatPhoto : TLInputChatPhotoBase
    {
        public const uint Signature = TLConstructors.TLInputChatPhoto;

        public TLInputPhotoBase Id { get; set; }

        public TLInputPhotoCropBase Crop { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInputPhotoBase>(bytes, ref position);
            Crop = GetObject<TLInputPhotoCropBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                Crop.ToBytes());
        }
    }
}
