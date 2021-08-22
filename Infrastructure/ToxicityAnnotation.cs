using System;
using System.Collections.Generic;

#nullable disable

namespace net_6_ef.Infrastructure
{
    public partial class ToxicityAnnotation
    {
        public decimal RevId { get; set; }
        public decimal WorkerId { get; set; }
        public decimal? Toxicity { get; set; }
        public decimal? ToxicityScore { get; set; }
    }
}
