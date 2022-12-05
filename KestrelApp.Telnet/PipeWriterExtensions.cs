﻿using System;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KestrelApp.Telnet
{
    static class PipeWriterExtensions
    {
        public unsafe static ValueTask<FlushResult> WriteAsync(this PipeWriter writer, ReadOnlySpan<char> text, Encoding? encoding = null)
        {
            writer.Write(text, encoding);
            return writer.FlushAsync();
        }

        public unsafe static int Write(this PipeWriter writer, ReadOnlySpan<char> text, Encoding? encoding = null)
        {
            if (text.IsEmpty)
            {
                return 0;
            }

            encoding ??= Encoding.UTF8;

            var chars = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(text));
            var byteCount = encoding.GetByteCount(chars, text.Length);

            var span = writer.GetSpan(byteCount);
            var bytes = (byte*)Unsafe.AsPointer(ref span[0]);
            var len = encoding.GetEncoder().GetBytes(chars, text.Length, bytes, byteCount, flush: true);
            writer.Advance(len);
            return len;
        }
    }
}