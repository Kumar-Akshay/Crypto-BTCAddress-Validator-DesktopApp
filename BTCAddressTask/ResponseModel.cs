using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCAddressTask
{
    public class Txref
    {
        public string tx_hash { get; set; }
        public long block_height { get; set; }
        public long tx_input_n { get; set; }
        public long tx_output_n { get; set; }
        public long value { get; set; }
        public long ref_balance { get; set; }
        public long confirmations { get; set; }
        public DateTime confirmed { get; set; }
        public bool double_spend { get; set; }
        public bool? spent { get; set; }
        public string spent_by { get; set; }
    }

    public class ResponseModel
    {
        public string address { get; set; }
        public long total_received { get; set; }
        public long total_sent { get; set; }
        public long balance { get; set; }
        public long unconfirmed_balance { get; set; }
        public long final_balance { get; set; }
        public long n_tx { get; set; }
        public long unconfirmed_n_tx { get; set; }
        public long final_n_tx { get; set; }
        public List<Txref> txrefs { get; set; }
        public string tx_url { get; set; }
    }


}
