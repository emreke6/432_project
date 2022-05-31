 module image_sensor_interface(in_clk, in_rst, in_interface_start, in_data_read, 
							   in_imgsensor_done,out_write_en, out_data_addr, out_data, out_done,
							   out_frame_capture);
 
 
 input in_clk, in_rst; // Global clock and reset signals
 
 // CONTROL SIGNALS RELATED WITH IMAGE SENSOR MODEL //
 input [8:0] in_data_read; // Data received from image sensor model
 input in_imgsensor_done; // Done signal received from image sensor model
 output reg out_frame_capture; // start signal to image sensor model
 // CONTROL SIGNALS RELATED WITH IMAGE SENSOR MODEL //
 
 // CONTROL SIGNALS RELATED WITH REGISTER FILE //
 output reg out_write_en; // write enable signal to register file
 output reg[5:0] out_data_addr; // address signal to register file
 output reg [8:0] out_data; // data output to register file
 // CONTROL SIGNALS RELATED WITH REGISTER FILE //
 
 // CONTROL SIGNALS RELATED WITH ACCELERATOR //
 input in_interface_start; // Interface should be started from accelerator
 output reg out_done; // done signal
 // CONTROL SIGNALS RELATED WITH ACCELERATOR //
 
 reg writing_to_reg;
 reg start_writing;
 reg [5:0] counter;
 
 always @ (posedge in_clk or posedge in_rst)
 begin
	if(in_rst)
	begin
		out_frame_capture <= 0;
		start_writing <= 0;
	end
	else if(in_interface_start)
	begin
		out_frame_capture <= 1;
		start_writing <= 1;
	end
	else
	begin
		start_writing <= 0;
		out_frame_capture <= 0;
	end
 end
 
 always @ (posedge in_clk or posedge in_rst)
 begin
	if(in_rst)
	begin
		writing_to_reg <= 0;
		out_done <= 0;
		out_write_en <= 0;
	end	
	else if(start_writing)
	begin
		writing_to_reg <= 1;
		out_write_en <= 1;
		out_done <= 0;
	end	
	else if(counter == 63)
	begin
		writing_to_reg <= 0;
		out_write_en <= 0;
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
	else if(writing_to_reg)
	begin
		counter <= counter + 1;
	end	
	else
	begin
		counter <= 0;
	end
 end
 
 always @ (*)
 begin
	if(writing_to_reg)
	begin
		out_data_addr = counter;
		out_data = in_data_read;
	end
	else
	begin
		out_data = 0;
		out_data_addr = 0;
	end
 end
 endmodule