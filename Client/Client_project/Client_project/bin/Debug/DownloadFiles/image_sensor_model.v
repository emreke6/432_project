module image_sensor_model(in_clk, in_rst, in_frame_capture, out_data_read, out_done); //Image sensor for a 64x8 image frame

// 1 cc high in_frame_capture signal starts the model
// 8 bit data out for each cc
// out_done is high when done


input in_clk, in_rst;
input in_frame_capture; 				//Input from the ISI telling the sensor to start the capture operation

output [8:0] out_data_read;	 		//The 8-bit data output to the ISI
output reg out_done;

reg [8:0] image[63:0]; 			// 8*8 image --> 64*8 register array
reg [5:0] counter;				// Counter for register address
reg reading;

initial		// Reset the data_read and read from text file
begin
	//Read from input file
	$readmemb("input.txt",image);
end


always @ (posedge in_clk or posedge in_rst) // 8 bit data for each clock cycle after in_frame_capture signal
begin
	if(in_rst)
	begin
		reading <= 0;
		out_done <= 0;
	end
	else if (in_frame_capture)
	begin
		reading <= 1;
	end
	else if(counter == 63) 
	begin
		reading <= 0;
		out_done <= 1;
	end
	else
	begin
		out_done <= 0;
	end
end

always @ (posedge in_clk or posedge in_rst)
begin
	if(in_rst)
	begin
		counter <= 0;
	end
	else if(reading)
	begin
		counter <= counter + 1;
	end
	else
	begin
		counter <= 0;
	end
	
end

assign out_data_read = reading ? image[counter] : 0;

endmodule
