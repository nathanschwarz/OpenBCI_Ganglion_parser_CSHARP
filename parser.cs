using System;

public class Ganglion
{
	private readonly int[] accelerometer_axis = { 7, 8, 9 };
	private readonly int[] byteId_format_18 = { 0, 101 };
	private readonly int[] byteId_format_19 = { 100, 201 };
	private readonly int[] byteId_impedance = { 200, 206 };
	private const int byteId_ascii = 206;
	private const int byteId_end_ascii = 207;
	private const int BUFFER_LENGTH_ERR = -1;
	private const int BYTEID_ERR = -2;
	private Int32 samples_count = 0;
	private Int32[,] samples = new Int32[2, 4];
	
	public const double V_SCALE = 1.2 * 8388607.0 * 1.5 * 51.0;
	public const double A_SCALE = 0.032;

	public byte id;
	public int[] accelerometer = new int[3];
	public Int32[] data = new Int32[4];
	public String last_ascii_msg;

	// impedance_channels : respectively : 1, 2, 3, 4, ref;
	public int[] impedance_channels = new int[5];

	public int parse(byte[] buffer)
	{
		this.id = buffer[0];
		if (this.id == 0)
			return (this.set_channels_raw(buffer));
		else if (this.id > byteId_format_18[0] && this.id < byteId_format_18[1])
			return (this.set_channels_18(buffer));
		else if (this.id > byteId_format_19[0] && this.id < byteId_format_19[1])
			return (this.set_channels_19(buffer));
		else if (this.id > byteId_impedance[0] && this.id < byteId_impedance[1])
			return (this.set_impedance_channel(this.id - 201, buffer));
		else if (this.id == byteId_ascii)
			return (this.set_ascii(buffer));
		else if (this.id == byteId_end_ascii)
			return (0);
		return (BYTEID_ERR);
	}

	public int process_sample(int sample)
	{
		if (sample != 0 && sample != 1)
			return (-1);
		if (this.id > byteId_format_18[0] && this.id < byteId_format_19[1]){
			this.data[0] += this.samples[sample, 0];
			this.data[1] += this.samples[sample, 1];
			this.data[2] += this.samples[sample, 2];
			this.data[3] += this.samples[sample, 3];
		}
		return (0);
	}

	int set_ascii(byte[] buffer){
		char[] msg = new char[buffer.Length];
		for (int x = 0; x < buffer.Length; x++){
			msg[x] = (char) buffer[x];
		}
		this.last_ascii_msg = new String(msg);
		return (0);
	}

	int set_impedance_channel(int channel, byte[] buffer)
	{
		int len = buffer.Length - 1;
		while (len != 0 && Double.IsNaN(buffer[len]))
			len--;
		if (len != 0)
			this.impedance_channels[channel] = buffer[len];
		return (0);
	}

	int set_channels_raw(byte[] buffer)
	{
		if (buffer.Length != 20)
			return (BUFFER_LENGTH_ERR);
		this.data[0] = this.bit_format_16(buffer, 1);
		this.data[1] = this.bit_format_16(buffer, 4);
		this.data[2] = this.bit_format_16(buffer, 7);
		this.data[3] = this.bit_format_16(buffer, 10);
		return (0);
	}

	int set_channels_18(byte[] buffer)
	{
		if (buffer.Length != 20)
			return (BUFFER_LENGTH_ERR);
		this.samples_count += 2;
		byte[] s0_c0 = new byte[]{  (byte) (buffer[1] >> 6),
								(byte) (((buffer[1] & 0x3F) << 2) | (buffer[2] >> 6)),
								(byte) (((buffer[2] & 0x3F) << 2) | (buffer[3] >> 6))};
		byte[] s0_c1 = new byte[] { (byte) ((buffer[3] & 0x3F) >> 4),
								(byte) ((buffer[3] << 4) | (buffer[4] >> 4)),
								(byte) ((buffer[4] << 4) | (buffer[5] >> 4))};
		byte[] s0_c2 = new byte[]{  (byte) ((buffer[5] & 0x0F) >> 2),
								(byte) ((buffer[5] << 6) | (buffer[6] >> 2)),
								(byte) ((buffer[6] << 6) | (buffer[7] >> 2))};
		byte[] s0_c3 = new byte[] { (byte)(buffer[7] & 0x03), buffer[8], buffer[9] };
		this.set_sample(0, s0_c0, s0_c1, s0_c2, s0_c3, 18);
		byte[] s1_c0 = new byte[]{  (byte) (buffer[10] >> 6),
								(byte) (((buffer[10] & 0x3F) << 2) | (buffer[11] >> 6)),
								(byte) (((buffer[11] & 0x3F) << 2) | (buffer[12] >> 6))};
		byte[] s1_c1 = new byte[]{  (byte) ((buffer[12] & 0x3F) >> 4),
								(byte) ((buffer[12] << 4) | (buffer[13] >> 4)),
								(byte) ((buffer[13] << 4) | (buffer[14] >> 4))};
		byte[] s1_c2 = new byte[]{  (byte) ((buffer[14] & 0x0F) >> 2),
								(byte) ((buffer[14] << 6) | (buffer[15] >> 2)),
								(byte) ((buffer[15] << 6) | (buffer[16] >> 2))};
		byte[] s1_c3 = new byte[] { (byte)(buffer[16] & 0x03), buffer[17], buffer[18] };
		this.set_sample(1, s1_c0, s1_c1, s1_c2, s1_c3, 18);
		Int32 has_accelerometer = this.samples_count % 10;
		if (has_accelerometer == accelerometer_axis[0])
			this.accelerometer[0] = buffer[19];
		else if (has_accelerometer == accelerometer_axis[1])
			this.accelerometer[1] = buffer[19];
		else if (has_accelerometer == accelerometer_axis[2])
			this.accelerometer[2] = buffer[19];
		return (0);
	}

	int set_channels_19(byte[] buffer)
	{
		if (buffer.Length != 20)
			return (BUFFER_LENGTH_ERR);
		this.samples_count += 2;
		byte[] s0_c0 = new byte[]{  (byte) (buffer[1] >> 5),
								(byte) (((buffer[1] & 0x1F) << 3) | (buffer[2] >> 5)),
								(byte) (((buffer[2] & 0x1F) << 3) | (buffer[3] >> 5))};
		byte[] s0_c1 = new byte[]{  (byte) ((buffer[3] & 0x1F) >> 2),
								(byte) ((buffer[3] << 6) | (buffer[4] >> 2)),
								(byte) ((buffer[4] << 6) | (buffer[5] >> 2))};
		byte[] s0_c2 = new byte[]{  (byte) (((buffer[5] & 0x03) << 1) | (buffer[6] >> 7)),
								(byte) (((buffer[6] & 0x7F) << 1) | (buffer[7] >> 7)),
								(byte) (((buffer[7] & 0x7F) << 1) | (buffer[8] >> 7))};
		byte[] s0_c3 = new byte[]{  (byte) (((buffer[8] & 0x7F) >> 4)),
								(byte) (((buffer[8] & 0x0F) << 4) | (buffer[9] >> 4)),
								(byte) (((buffer[9] & 0x0F) << 4) | (buffer[10] >> 4))};
		this.set_sample(0, s0_c0, s0_c1, s0_c2, s0_c3, 19);
		byte[] s1_c0 = new byte[]{  (byte) ((buffer[10] & 0x0F) >> 1),
								(byte) ((buffer[10] << 7) | (buffer[11] >> 1)),
								(byte) ((buffer[11] << 7) | (buffer[12] >> 1))};
		byte[] s1_c1 = new byte[]{  (byte) (((buffer[12] & 0x01) << 2) | (buffer[13] >> 6)),
								(byte) ((buffer[13] << 2) | (buffer[14] >> 6)),
								(byte) ((buffer[14] << 2) | (buffer[15] >> 6))};
		byte[] s1_c2 = new byte[]{  (byte) (((buffer[15] & 0x38) >> 3)),
								(byte) (((buffer[15] & 0x07) << 5) | ((buffer[16] & 0xF8) >> 3)),
								(byte) (((buffer[16] & 0x07) << 5) | ((buffer[17] & 0xF8) >> 3))};
		byte[] s1_c3 = new byte[] { (byte)(buffer[17] & 0x07), buffer[18], buffer[19] };
		this.set_sample(1, s1_c0, s1_c1, s1_c2, s1_c3, 19);
		return (0);
	}

	void set_sample(int sample, byte[] channel_0, byte[] channel_1, byte[] channel_2, byte[] channel_3, int size)
	{
		if (size == 18)
		{
			this.samples[sample, 0] = this.bit_format_18(channel_0);
			this.samples[sample, 1] = this.bit_format_18(channel_1);
			this.samples[sample, 2] = this.bit_format_18(channel_2);
			this.samples[sample, 3] = this.bit_format_18(channel_3);
		}
		else
		{
			this.samples[sample, 0] = this.bit_format_19(channel_0);
			this.samples[sample, 1] = this.bit_format_19(channel_1);
			this.samples[sample, 2] = this.bit_format_19(channel_2);
			this.samples[sample, 3] = this.bit_format_19(channel_3);
		}
	}

	Int32 bit_format_16(byte[] b, int index)
	{
		Int32 result = ((0xFF & b[index]) << 16) | ((0xFF & b[index + 1]) << 8) | (0xFF & b[index + 2]);
		if ((result & 0x00800000) > 0)
			result = (byte)(result | 0xFF000000);
		else
			result = result & 0x00FFFFFF;
		return (result);
	}

	Int32 bit_format_18(byte[] to_concat)
	{
		Int32 default_byte = 0;
		if ((to_concat[2] & 0x01) > 0)
			default_byte = 0x3FFF;
		return (default_byte << 18) | (to_concat[0] << 16) | (to_concat[1] << 8) | to_concat[2];
	}

	Int32 bit_format_19(byte[] to_concat)
	{
		Int32 default_byte = 0;
		if ((to_concat[2] & 0x01) > 0)
			default_byte = 0x3FFF;
		return (default_byte << 19) | (to_concat[0] << 16) | (to_concat[1] << 8) | to_concat[2];
	}
}