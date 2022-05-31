module ahb_mux_s2m (in_HCLK, in_HRESET, 
					in_HSEL_DefaultSlave, in_HSEL_Accelerator, in_HSEL_SRAMController, 
					in_HREADY_DefaultSlave, in_HREADY_Accelerator, in_HREADY_SRAMController,
					in_HRDATA_DefaultSlave, in_HRDATA_Accelerator, in_HRDATA_SRAMController,
					in_HREADY,

					out_HREADY,
					out_HRDATA);

// Inputs					
input in_HCLK;
input in_HRESET;

input in_HSEL_DefaultSlave;
input in_HSEL_Accelerator;
input in_HSEL_SRAMController;

input in_HREADY_DefaultSlave;
input in_HREADY_Accelerator;
input in_HREADY_SRAMController;


input [31:0] in_HRDATA_DefaultSlave;
input [31:0] in_HRDATA_Accelerator;
input [31:0] in_HRDATA_SRAMController;

input in_HREADY;

// Outputs
output out_HREADY; // muxed hready out
output [31:0] out_HRDATA; // muxed read data bus out to master(s)

// Intermediates
wire masked_HREADY_DefaultSlave; 
wire masked_HREADY_Accelerator;
wire masked_HREADY_SRAMController;

reg registered_SEL_SRAM;
reg registered_SEL_Accelerator;
reg registered_SEL_DefaultSlave;

// delaying HSEL inputs by registerinng them due to AHB standard
always @(posedge in_HRESET or posedge in_HCLK)
begin
  if (in_HRESET)
    begin
      registered_SEL_SRAM  <= 1'b0;
      registered_SEL_Accelerator <= 1'b0;
      registered_SEL_DefaultSlave  <= 1'b1;
    end
  else
    if (in_HREADY)
      begin
        registered_SEL_SRAM  <= in_HSEL_SRAMController;
        registered_SEL_Accelerator <= in_HSEL_Accelerator;
        registered_SEL_DefaultSlave  <= in_HSEL_DefaultSlave;
      end
end

assign out_HRDATA = registered_SEL_SRAM ? in_HRDATA_SRAMController : registered_SEL_Accelerator ? in_HRDATA_Accelerator : 32'h00000000;

assign masked_HREADY_DefaultSlave     = registered_SEL_DefaultSlave ? in_HREADY_DefaultSlave : 1'b0;
assign masked_HREADY_Accelerator      = registered_SEL_Accelerator ? in_HREADY_Accelerator :1'b0;
assign masked_HREADY_SRAMController   = registered_SEL_SRAM ? in_HREADY_SRAMController  : 1'b0;
assign out_HREADY                  = masked_HREADY_Accelerator | masked_HREADY_SRAMController | masked_HREADY_DefaultSlave;



endmodule
