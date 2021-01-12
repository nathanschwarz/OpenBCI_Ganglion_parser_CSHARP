# OpenBCI_Ganglion_parser_CSHARP

C# parser for the open source openBCI Ganglion Board
This project will be used to parse the data from the Ganglion into Unity.

# Basic usage

public void Main(){

	// declaration

	Ganglion ganglion = new Ganglion();

	// start your BLE receiver (not included in this project), which will handle the communication with the Ganglion board.

	// When receiving data from the Ganglion, call ganglion.parse(data);

	// in your main loop

	while (true){

		ganglion.process_sample(0);

		process_data(ganglion.data);

		//put a delay < 0.005s (100Hz) here

		ganglion.process_sample(1);

		process_data(ganglion.data);

		//put a delay < 0.005s (100Hz) here

	}

}

public void process_data(Int32[] data){

	//process the data here

}

# Constants
Ganglion.V_SCALE : voltage scale factor to use when processing the data from the ganglion to convert the values to Volts.

Ganglion.A_SCALE : accelerometer scale factor to use when processing the accelerometer data from the ganglion.

# attributes
ganglion.id - (byte) : use this attribute to know what was the last Ganglion messsage ID.

ganglion.accelerometer - (Int32[3]) : use this attribute to get the x, y and z axis of the Ganglion accelerometer.

ganglion.data - (Int32[4]) : use this attribute to get the current values of the Ganglion EEG.

ganglion.last_ascii_msg - (String) : use this attribute to access the last ascii value sent by the Ganglion.

ganglion.impedance_channels - (int[5]) :  use this attribute to access the impedance values sent by the Ganglion, respectively : 1, 2, 3, 4, ref.

# methods
Int ganglion.parse(byte[] buffer): parse the data received in you're BLE listener : return values: {0 : OK, -1 : buffer length is incorrect, -2 : byteID not recognized};

Int ganglion.process_sample(int sample): add the delta to the data (sample must be either 0 or 1) : return values : {0: OK, -1 : passed value is incorrect};
