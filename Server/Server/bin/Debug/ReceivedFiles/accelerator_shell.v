 module accelerator_shell(in_HCLK,in_HRESET,in_HWRITE,in_HADDR, in_HWDATA, in_HSEL,
						  out_HREADY,
						  out_HRDATA, out_DATA_SRAM, out_ADDR_SRAM, out_WE_SRAM,
						  in_ISI_data_read, in_img_sensor_done, out_frame_capture, out_interrupt);

input in_HCLK;
input in_HRESET;
input in_HSEL;

input in_HWRITE;
input[31:0] in_HADDR;
input [31:0] in_HWDATA;

output reg [31:0] out_HRDATA;
output reg out_HREADY;

// PROCESSOR MODEL //
wire [8:0] Dataout_ARM;
wire [5:0] proc_addr;
// PROCESSOR MODEL //

// SRAM //
output [31:0] out_DATA_SRAM;
output [14:0] out_ADDR_SRAM;
output reg out_WE_SRAM;
wire sram_data_available;
reg [14:0] address_generator;
reg [14:0] sram_data;
reg writing, done_writing;
reg [4:0] state;
// SRAM //

reg Control_Reg[0:2];
wire SEL_Control_Reg, SEL_Data_Reg;

// ACCELERATOR //
wire ISI_finished_w;
wire accelarator_done;
wire [14:0] T00, T01, T02, T03, T10, T11, T12, T13, T20, T21, T22, T23, T30, T31, T32, T33;
output out_frame_capture;
input in_img_sensor_done;
input [8:0] in_ISI_data_read;
// ACCELERATOR //

output out_interrupt;


parameter IDLE= 5'd0, write_T00= 5'd1,write_T01= 5'd2,write_T02= 5'd3,write_T03= 5'd4,
write_T10= 5'd5,write_T11= 5'd6,write_T12= 5'd7,write_T13= 5'd8,
write_T20= 5'd9,write_T21= 5'd10,write_T22= 5'd11,write_T23= 5'd12,
write_T30= 5'd13,write_T31= 5'd14,write_T32= 5'd15,write_T33= 5'd16;

//Decoding:
assign SEL_Control_Reg = in_HSEL ? (in_HADDR[31:29] == 3'b011 ? 1'b1 : 1'b0): 0;
assign SEL_Data_Reg = in_HSEL ? (in_HADDR[31:29] == 3'b010 ? 1'b1 : 1'b0) : 0;
assign proc_addr =  in_HADDR[5:0];

assign out_ADDR_SRAM = address_generator;
assign out_DATA_SRAM = {17'd0 , sram_data};

assign out_interrupt = ISI_finished_w;
//Control register//////////////////////////////////////////////////////////////////////
always @ (posedge in_HCLK)
begin
	if (in_HRESET)
	begin	
	Control_Reg[0] <= 1'b0; 	//ISI_start
	Control_Reg[1] <= 1'b0;		//ISI_finished
	Control_Reg[2] <= 1'b0;		//Accelerator_done
	end
	else
	begin
		if (in_HWRITE && SEL_Control_Reg)
		begin
			Control_Reg[0] <= in_HWDATA[0];
		end
		else
		begin
			Control_Reg[0] <= 0;
		end
		Control_Reg[1] <= ISI_finished_w;
		Control_Reg[2] <= done_writing;
	end
end

always @(posedge in_HCLK)
begin
	if (SEL_Control_Reg)
	begin
		out_HRDATA <= {30'd0, Control_Reg[2], Control_Reg[1]};
		out_HREADY <= 1;
	end
	else if(SEL_Data_Reg)
	begin
		out_HRDATA <= {23'd0, Dataout_ARM};
		out_HREADY <= 1;
	end
	else
	begin
		out_HRDATA <= 32'd0;
		out_HREADY <= 0;
	end
end


// SRAM LOGIC //
always @ (posedge in_HCLK or posedge in_HRESET)
begin
	if(in_HRESET)
	begin
		address_generator <= 0;
		done_writing <= 0;
		state <= IDLE;
	end
	else
	begin
		case(state)
			IDLE:
			begin
				if(sram_data_available || accelarator_done)
				begin
					done_writing <= 0;
					out_WE_SRAM <= 1;
					sram_data <= T00;
					state <= write_T00;
				end
			end
			write_T00:
			begin
				sram_data <= T01;
				address_generator <= address_generator + 1;
				state <= write_T01;
			end
			write_T01:
			begin
				sram_data <= T02;
				address_generator <= address_generator + 1;
				state <= write_T02;
			end
			write_T02:
			begin
				sram_data <= T03;
				address_generator <= address_generator + 1;
				state <= write_T03;
			end
			write_T03:
			begin
				sram_data <= T10;
				address_generator <= address_generator + 1;
				state <= write_T10;
			end
			write_T10:
			begin
				sram_data <= T11;
				address_generator <= address_generator + 1;
				state <= write_T11;
			end
			write_T11:
			begin
				sram_data <= T12;
				address_generator <= address_generator + 1;
				state <= write_T12;
			end
			write_T12:
			begin
				sram_data <= T13;
				address_generator <= address_generator + 1;
				state <= write_T13;
			end
			write_T13:
			begin
				sram_data <= T20;
				address_generator <= address_generator + 1;
				state <= write_T20;
			end
			write_T20:
			begin
				sram_data <= T21;
				address_generator <= address_generator + 1;
				state <= write_T21;
			end
			write_T21:
			begin
				sram_data <= T22;
				address_generator <= address_generator + 1;
				state <= write_T22;
			end
			write_T22:
			begin
				sram_data <= T23;
				address_generator <= address_generator + 1;
				state <= write_T23;
			end
			write_T23:
			begin
				sram_data <= T30;
				address_generator <= address_generator + 1;
				state <= write_T30;
			end
			write_T30:
			begin
				sram_data <= T31;
				address_generator <= address_generator + 1;
				state <= write_T31;
			end
			write_T31:
			begin
				sram_data <= T32;
				address_generator <= address_generator + 1;
				state <= write_T32;
			end
			write_T32:
			begin
				sram_data <= T33;
				address_generator <= address_generator + 1;
				state <= write_T33;
			end
			write_T33:
			begin
				state <= IDLE;
				address_generator <= address_generator + 1;
				out_WE_SRAM <= 0;
				if(address_generator == 63)
				begin
					done_writing <= 1;
				end
			end			
		endcase
	end
end
// SRAM LOGIC //

accelerator_top accel_top(
	.in_clk(in_HCLK),
	.in_rst(in_HRESET),
	.in_accelerator_start(ISI_finished_w),
	.in_ISI_start(Control_Reg[0]),
	.in_proc_addr(proc_addr),
	.out_proc_data(Dataout_ARM),
	.out_ISI_finished(ISI_finished_w),
	.out_accelerator_done(accelarator_done),
	.out_data_available(sram_data_available),
	.out_T00(T00),
	.out_T01(T01),
	.out_T02(T02),
	.out_T03(T03),
	.out_T10(T10),
	.out_T11(T11),
	.out_T12(T12),
	.out_T13(T13),
	.out_T20(T20),
	.out_T21(T21),
	.out_T22(T22),
	.out_T23(T23),
	.out_T30(T30),
	.out_T31(T31),
	.out_T32(T32),
	.out_T33(T33),
	.in_img_sensor_done(in_img_sensor_done),
	.in_ISI_data_read(in_ISI_data_read),
	.out_frame_capture(out_frame_capture)
	);
endmodule	