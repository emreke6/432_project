
module ahb_default_slave (
// Inputs
                   HCLK,
                   HRESET,
                   HREADYIn,
                   HSELDefault,                   
// Outputs
                   HREADYOut
                  );

input         HCLK;            // system bus clock
input         HRESET;          // reset input (active low)
input         HREADYIn;        // AHB ready input
input         HSELDefault;     // peripheral select - default slave

output        HREADYOut;       // AHB ready output to S->M mux

wire       NextHREADY;
reg        iHREADYOut;

// -----------------------------------------------------------------------------
// When an undefined area of the memory map is accessed, or an invalid address
// is driven onto the address bus, the default slave outputs are selected and
// passed to the current bus master.
// An OKAY response is generated for IDLE or BUSY transfers to undefined
// locations, but a two cycle ERROR response is generated if a non-sequential
// or sequential transfer is attempted.
// -----------------------------------------------------------------------------s

assign NextHREADY  = (iHREADYOut == 1'b0) ? 1'b1 : ((HSELDefault == 1'b1) & (HREADYIn == 1'b1)) ? 1'b0 : 1'b1;

always @(posedge HRESET or posedge HCLK)
begin : p_HREADYSeq
  if (HRESET)
    iHREADYOut <= 1'b1;
  else
    iHREADYOut <= NextHREADY;
end 

assign HREADYOut = iHREADYOut;

endmodule
