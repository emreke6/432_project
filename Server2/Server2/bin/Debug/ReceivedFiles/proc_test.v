`timescale 1ns / 1ps

module proc_test;

	// Inputs
	reg in_HCLK;
	reg in_HRESET;
	reg [31:0] in_HADDR;
	reg in_HWRITE;
	reg [31:0] in_HWDATA;

	// Outputs
	wire out_HREADY;
	wire [31:0] out_HRDATA;
	wire out_interrupt;

	// Instantiate the Unit Under Test (UUT)
	project_model uut (
		.in_HCLK(in_HCLK), 
		.in_HRESET(in_HRESET), 
		.in_HADDR(in_HADDR), 
		.in_HWRITE(in_HWRITE), 
		.in_HWDATA(in_HWDATA), 
		.out_HREADY(out_HREADY), 
		.out_HRDATA(out_HRDATA), 
		.out_interrupt(out_interrupt)
	);

	parameter CLK_Period = 20;
	reg [31:0] K;
	reg [31:0] J;
	reg [14:0] tempRegs [3:0];
	reg [31:0] accelerator_register_startaddr;
	reg [8:0] RegFileInputs [63:0]; 
	reg [14:0] RegFileOutputs [63:0];
	
	always #(CLK_Period/2) in_HCLK = ~in_HCLK;

	initial begin
	
		in_HCLK = 0;
		in_HRESET = 1;
		in_HADDR = 0;
		in_HWRITE = 0;
		in_HWDATA = 0;
		accelerator_register_startaddr = 32'h40000000;
		
		#(CLK_Period * 5);
		in_HRESET = 0;
		#CLK_Period;
		in_HADDR = 32'h60000000;
		in_HWRITE = 1;
		in_HWDATA = 32'd1;
		#CLK_Period;
		in_HWRITE = 0;
		while(out_interrupt == 0)
		begin
			#CLK_Period;
		end
		for(K = 0; K < 64; K = K + 1)
		begin
			in_HADDR = accelerator_register_startaddr + K;
			#CLK_Period;
			RegFileInputs[K] = out_HRDATA[8:0];
		end
		for(K = 0; K < 4; K = K + 1)
		begin
			for(J = 0; J < 4; J = J + 1)
			begin
				tempRegs[J] = RegFileInputs[K*16 + J] + RegFileInputs[K*16 + 4 + J] + RegFileInputs[K*16 + 8 + J] + RegFileInputs[K*16 + 12 + J];	
			end
			RegFileOutputs[K*16] = tempRegs[0] + tempRegs[1] + tempRegs[2] + tempRegs[3];
			RegFileOutputs[K*16+1] = 2*tempRegs[0] + tempRegs[1] - tempRegs[2] - 2*tempRegs[3];
			RegFileOutputs[K*16+2] = tempRegs[0] - tempRegs[1] - tempRegs[2] + tempRegs[3];
			RegFileOutputs[K*16+3] = tempRegs[0] - 2*tempRegs[1] + 2*tempRegs[2] - tempRegs[3];
			for(J = 0; J < 4; J = J + 1)
			begin
				tempRegs[J] = (2*RegFileInputs[K*16 + J]) + RegFileInputs[K*16 + 4 + J] - RegFileInputs[K*16 + 8 + J] - (2*RegFileInputs[K*16 + 12 + J]);
			end
			RegFileOutputs[K*16+4] = tempRegs[0] + tempRegs[1] + tempRegs[2] + tempRegs[3];
			RegFileOutputs[K*16+5] = 2*tempRegs[0] + tempRegs[1] - tempRegs[2] - 2*tempRegs[3];
			RegFileOutputs[K*16+6] = tempRegs[0] - tempRegs[1] - tempRegs[2] + tempRegs[3];
			RegFileOutputs[K*16+7] = tempRegs[0] - 2*tempRegs[1] + 2*tempRegs[2] - tempRegs[3];			
			for(J = 0; J < 4; J = J + 1)
			begin
				tempRegs[J] = RegFileInputs[K*16 + J] - RegFileInputs[K*16 + 4 + J] - RegFileInputs[K*16 + 8 + J] + RegFileInputs[K*16 + 12 + J];			
			end
			RegFileOutputs[K*16+8] = tempRegs[0] + tempRegs[1] + tempRegs[2] + tempRegs[3];
			RegFileOutputs[K*16+9] = 2*tempRegs[0] + tempRegs[1] - tempRegs[2] - 2*tempRegs[3];
			RegFileOutputs[K*16+10] = tempRegs[0] - tempRegs[1] - tempRegs[2] + tempRegs[3];
			RegFileOutputs[K*16+11] = tempRegs[0] - 2*tempRegs[1] + 2*tempRegs[2] - tempRegs[3];	
			for(J = 0; J < 4; J = J + 1)
			begin
				tempRegs[J] = RegFileInputs[K*16 + J] - (2*RegFileInputs[K*16 + 4 + J]) + (2*RegFileInputs[K*16 + 8 + J]) - RegFileInputs[K*16 + 12 + J];			
			end
			RegFileOutputs[K*16+12] = tempRegs[0] + tempRegs[1] + tempRegs[2] + tempRegs[3]; 
			RegFileOutputs[K*16+13] = 2*tempRegs[0] + tempRegs[1] - tempRegs[2] - 2*tempRegs[3];
			RegFileOutputs[K*16+14] = tempRegs[0] - tempRegs[1] - tempRegs[2] + tempRegs[3];
			RegFileOutputs[K*16+15] = tempRegs[0] - 2*tempRegs[1] + 2*tempRegs[2] - tempRegs[3];		
			#CLK_Period;
		end
		in_HADDR = 32'h60000000;
		#CLK_Period;
		while(out_HRDATA[1] == 0)
		begin
			#CLK_Period;
		end
		for(K = 0; K < 64; K = K + 1)
		begin
			in_HADDR = K;
			#CLK_Period;
			if(out_HRDATA[14:0] == RegFileOutputs[K])
			begin
				 $display("%d OK", K);
			end
			else
			begin
				$display("%b binary file regfile", RegFileOutputs[K]);
				$display("%b binary file hrdata", out_HRDATA[14:0]);
				$display("%d NOPE", K);
			end
		end
		$stop;
	end
endmodule

