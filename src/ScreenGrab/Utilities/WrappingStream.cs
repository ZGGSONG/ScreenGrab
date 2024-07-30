﻿using System.IO;
using ScreenGrab.Models;

namespace ScreenGrab.Utilities;

/// <summary>
///     A <see cref="Stream" /> that wraps another stream. The major feature of <see cref="WrappingStream" /> is that it
///     does not dispose the
///     underlying stream when it is disposed; this is useful when using classes such as <see cref="BinaryReader" /> and
///     <see cref="System.Security.Cryptography.CryptoStream" /> that take ownership of the stream passed to their
///     constructors.
/// </summary>
public class WrappingStream : Stream
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WrappingStream" /> class.
    /// </summary>
    /// <param name="streamBase">The wrapped stream.</param>
    public WrappingStream(Stream streamBase)
    {
        // check parameters

        if (streamBase == null)
            throw new ArgumentNullException("streamBase");
        WrappedStream = streamBase;
    }

    /// <summary>
    ///     Gets a value indicating whether the current stream supports reading.
    /// </summary>
    /// <returns><c>true</c> if the stream supports reading; otherwise, <c>false</c>.</returns>
    public override bool CanRead => WrappedStream == null ? false : WrappedStream.CanRead;

    /// <summary>
    ///     Gets a value indicating whether the current stream supports seeking.
    /// </summary>
    /// <returns><c>true</c> if the stream supports seeking; otherwise, <c>false</c>.</returns>
    public override bool CanSeek => WrappedStream == null ? false : WrappedStream.CanSeek;

    /// <summary>
    ///     Gets a value indicating whether the current stream supports writing.
    /// </summary>
    /// <returns><c>true</c> if the stream supports writing; otherwise, <c>false</c>.</returns>
    public override bool CanWrite => WrappedStream == null ? false : WrappedStream.CanWrite;

    /// <summary>
    ///     Gets the length in bytes of the stream.
    /// </summary>
    public override long Length
    {
        get
        {
            ThrowIfDisposed();
            if (WrappedStream is not null)
                return WrappedStream.Length;

            return 0;
        }
    }

    /// <summary>
    ///     Gets or sets the position within the current stream.
    /// </summary>
    public override long Position
    {
        get
        {
            ThrowIfDisposed();
            if (WrappedStream is not null)
                return WrappedStream.Position;
            return 0;
        }
        set
        {
            ThrowIfDisposed();
            if (WrappedStream is not null)
                WrappedStream.Position = value;
        }
    }

    /// <summary>
    ///     Gets the wrapped stream.
    /// </summary>
    /// <value>The wrapped stream.</value>
    protected Stream? WrappedStream { get; private set; }

    /// <summary>
    ///     Begins an asynchronous read operation.
    /// </summary>
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        ThrowIfDisposed();

        if (WrappedStream is not null && callback is not null && state is not null)
            return WrappedStream.BeginRead(buffer, offset, count, callback, state);

        return new NullAsyncResult();
    }

    /// <summary>
    ///     Begins an asynchronous write operation.
    /// </summary>
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback,
        object? state)
    {
        ThrowIfDisposed();

        if (WrappedStream is not null && callback is not null && state is not null)
            return WrappedStream.BeginWrite(buffer, offset, count, callback, state);

        return new NullAsyncResult();
    }

    /// <summary>
    ///     Waits for the pending asynchronous read to complete.
    /// </summary>
    public override int EndRead(IAsyncResult asyncResult)
    {
        ThrowIfDisposed();

        if (WrappedStream is not null)
            return WrappedStream.EndRead(asyncResult);

        return 0;
    }

    /// <summary>
    ///     Ends an asynchronous write operation.
    /// </summary>
    public override void EndWrite(IAsyncResult asyncResult)
    {
        ThrowIfDisposed();

        if (WrappedStream is not null)
            WrappedStream.EndWrite(asyncResult);
    }

    /// <summary>
    ///     Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
    /// </summary>
    public override void Flush()
    {
        ThrowIfDisposed();

        if (WrappedStream is not null)
            WrappedStream.Flush();
    }

    /// <summary>
    ///     Reads a sequence of bytes from the current stream and advances the position
    ///     within the stream by the number of bytes read.
    /// </summary>
    public override int Read(byte[] buffer, int offset, int count)
    {
        ThrowIfDisposed();

        if (WrappedStream is not null)
            return WrappedStream.Read(buffer, offset, count);
        return 0;
    }

    /// <summary>
    ///     Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end
    ///     of the stream.
    /// </summary>
    public override int ReadByte()
    {
        ThrowIfDisposed();

        if (WrappedStream is not null)
            return WrappedStream.ReadByte();
        return 0;
    }

    /// <summary>
    ///     Sets the position within the current stream.
    /// </summary>
    /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
    /// <param name="origin">
    ///     A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to
    ///     obtain the new position.
    /// </param>
    /// <returns>The new position within the current stream.</returns>
    public override long Seek(long offset, SeekOrigin origin)
    {
        ThrowIfDisposed();

        if (WrappedStream is not null)
            return WrappedStream.Seek(offset, origin);
        return 0;
    }

    /// <summary>
    ///     Sets the length of the current stream.
    /// </summary>
    /// <param name="value">The desired length of the current stream in bytes.</param>
    public override void SetLength(long value)
    {
        ThrowIfDisposed();

        if (WrappedStream is not null)
            WrappedStream.SetLength(value);
    }

    /// <summary>
    ///     Writes a sequence of bytes to the current stream and advances the current position
    ///     within this stream by the number of bytes written.
    /// </summary>
    public override void Write(byte[] buffer, int offset, int count)
    {
        ThrowIfDisposed();

        if (WrappedStream is not null)
            WrappedStream.Write(buffer, offset, count);
    }

    /// <summary>
    ///     Writes a byte to the current position in the stream and advances the position within the stream by one byte.
    /// </summary>
    public override void WriteByte(byte value)
    {
        ThrowIfDisposed();

        if (WrappedStream is not null)
            WrappedStream.WriteByte(value);
    }

    /// <summary>
    ///     Releases the unmanaged resources used by the <see cref="WrappingStream" /> and optionally releases the managed
    ///     resources.
    /// </summary>
    /// <param name="disposing">
    ///     true to release both managed and unmanaged resources; false to release only unmanaged
    ///     resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        // doesn't close the base stream, but just prevents access to it through this WrappingStream

        if (disposing)
            WrappedStream = null;
        base.Dispose(disposing);
    }

    private void ThrowIfDisposed()
    {
        // throws an ObjectDisposedException if this object has been disposed

        if (WrappedStream == null)
            throw new ObjectDisposedException(GetType().Name);
    }
}