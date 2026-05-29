namespace Uccs;

public class CountStream : Stream
{
	public override bool CanRead { get; }
	public override bool CanSeek { get; }
	public override bool CanWrite => true;
	public override long Position { get; set; }
	public override long Length => Position;

	public override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if(offset != 0)
			throw new NotImplementedException();	

		Position += count;
	}
}
