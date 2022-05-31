 module forward_transform_soc(in_HCLK, in_HRESET, out_HREADY, in_HADDR, in_SRAM_r_DATA,
							  in_ISM_data_read, in_ISM_done, in_HWRITE, in_HWDATA,
							  out_HRDATA, out_SRAM_we, out_SRAM_re, out_SRAM_w_ADDR,
							  out_SRAM_r_ADDR, out_SRAM_w_DATA, out_ISM_frame_capture, out_interrupt
							  );			  
 // BUS INTERFACE //
 input in_HCLK, in_HRESET;
 input [31:0] in_HADDR, in_HWDATA;
 input in_HWRITE;
 output [31:0] out_HRDATA;
 output out_HREADY, out_interrupt;
 // BUS INTERFACE //
 
 // ISM INTERFACE //
 input [8:0] in_ISM_data_read;
 input in_ISM_done;
 output out_ISM_frame_capture;
 // ISM INTERFACE //
 
 // SRAM MODEL INTERFACE //
 input [31:0] in_SRAM_r_DATA;
 output [14:0] out_SRAM_r_ADDR, out_SRAM_w_ADDR;
 output [31:0] out_SRAM_w_DATA;
 output out_SRAM_re, out_SRAM_we;
 // SRAM MODEL INTERFACE //
 
 // ACCELERATOR CONNECTIONS //
 wire [31:0] accelerator_mux_HRDATA;
 wire accelerator_mux_HREADY;
 wire [14:0] accelerator_SRAM_addr;
 wire [31:0] accelerator_SRAM_data;
 wire accelerator_decoder_HSEL;
 wire accelerator_SRAM_we;
 // ACCELERATOR CONNECTIONS //
 
 // SRAM CONNECTIONS //
 wire [31:0] SRAM_mux_HRDATA;
 wire SRAM_mux_HREADY;
 wire SRAM_decoder_HSEL;
 // SRAM CONNECTIONS //
 
 // DEFAULT SLAVE CONNECTIONS //
 wire default_slave_decoder_HSEL;
 wire default_slave_mux_HREADY;
 // DEFAULT SLAVE CONNECTIONS //
 
 
 accelerator_shell accel_shell(
	.in_HCLK(in_HCLK),
	.in_HRESET(in_HRESET),
	.in_HWRITE(in_HWRITE),
	.in_HADDR(in_HADDR),
	.in_HWDATA(in_HWDATA),
	.in_HSEL(accelerator_decoder_HSEL),
	.out_HRDATA(accelerator_mux_HRDATA),
	.out_HREADY(accelerator_mux_HREADY),
	.out_DATA_SRAM(accelerator_SRAM_data),
	.out_ADDR_SRAM(accelerator_SRAM_addr),
	.out_WE_SRAM(accelerator_SRAM_we),
	.out_frame_capture(out_ISM_frame_capture),
	.in_img_sensor_done(in_ISM_done),
	.in_ISI_data_read(in_ISM_data_read),
	.out_interrupt(out_interrupt)
 );
 
 ahb_decoder decoder(
 .in_HADDR(in_HADDR),
 .out_HSEL_DefaultSlave(default_slave_decoder_HSEL),
 .out_HSEL_SRAMController(SRAM_decoder_HSEL),
 .out_HSEL_Accelerator(accelerator_decoder_HSEL)
 );
 
 ahb_mux_s2m mux(
 .in_HCLK(in_HCLK),
 .in_HRESET(in_HRESET),
 .in_HSEL_DefaultSlave(default_slave_decoder_HSEL),
 .in_HSEL_Accelerator(accelerator_decoder_HSEL),
 .in_HSEL_SRAMController(SRAM_decoder_HSEL),
 .in_HREADY_DefaultSlave(default_slave_mux_HREADY),
 .in_HREADY_Accelerator(accelerator_mux_HREADY),
 .in_HREADY_SRAMController(SRAM_mux_HREADY),
 .in_HRDATA_DefaultSlave(32'd0),
 .in_HRDATA_Accelerator(accelerator_mux_HRDATA),
 .in_HRDATA_SRAMController(SRAM_mux_HRDATA),
 .in_HREADY(out_HREADY),
 .out_HREADY(out_HREADY),
 .out_HRDATA(out_HRDATA)
 );
 
 
 ahb_default_slave default_slave(
  .HCLK(in_HCLK),
  .HRESET(in_HRESET),
  .HREADYIn(out_HREADY),
  .HSELDefault(default_slave_decoder_HSEL),                   
  .HREADYOut(default_slave_mux_HREADY)
 );
 
 sram_controller sram_controller(
	.in_HCLK(in_HCLK),
	.in_HRESET(in_HRESET),
	.in_HWRITE(in_HWRITE),
	.in_HSEL(SRAM_decoder_HSEL),
	.in_HADDR(in_HADDR),
	.in_accel_write_en(accelerator_SRAM_we),
	.in_accel_write_addr(accelerator_SRAM_addr),
	.in_accel_write_data(accelerator_SRAM_data),
	.in_sram_read_data(in_SRAM_r_DATA),
	.out_HREADY(SRAM_mux_HREADY),
	.out_HRDATA(SRAM_mux_HRDATA),
	.out_write_en(out_SRAM_we),
	.out_read_en(out_SRAM_re),
	.out_sram_write_addr(out_SRAM_w_ADDR),
	.out_sram_write_data(out_SRAM_w_DATA),
	.out_sram_read_addr(out_SRAM_r_ADDR)
 );

 endmodule