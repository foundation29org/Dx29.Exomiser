using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dx29.Exomiser
{
    public enum AnalysisMode
    {
        PASS_ONLY,
        FULL
    }

    public class ExomiserRequest
    {
        const string REQUIRED = "Value is required.";

        public string VcfFilename { get; set; }
        public string PedFilename { get; set; }

        public string UserId { get; set; }
        public string CaseId { get; set; }
        public string ResourceId { get; set; }
        public string NotificationUrl { get; set; }

        public bool IsGenome { get; set; }
        public string GenomeAssembly { get; set; }

        public string Proband { get; set; }

        [Range(0, Int32.MaxValue, ErrorMessage = "Value must be positive.")]
        public int NumGenes { get; set; }

        public IList<string> Hpos { get; set; }

        [NonEmpty]
        public IList<string> HiPhivePrioritisers { get; set; }
        [NonEmpty]
        public IList<string> PathogenicitySources { get; set; }
        [NonEmpty]
        public IList<string> FrequencySources { get; set; }

        [EnumDataType(typeof(AnalysisMode))]
        public string AnalysisMode { get; set; }
        public double Frequency { get; set; }
        public bool KeepNonPathogenic { get; set; }
        public bool RegulatoryFeatureFilter { get; set; }
        public double MinQuality { get; set; }

        public bool OutputPassVariantsOnly { get; set; }

        public IDictionary<string, double> InheritanceModes { get; set; }
        public IDictionary<string, IList<string>> VariantEffectFilters { get; set; }
        public IDictionary<string, object> PriorityScoreFilter { get; set; }

        [NonEmpty]
        public IList<string> OutputFormats { get; set; }

        public ExomiserRequest()
        {
            IsGenome = false;
            NumGenes = 100;
            PathogenicitySources = new string[] { "POLYPHEN", "MUTATION_TASTER", "SIFT" };
            AnalysisMode = "PASS_ONLY";
            Frequency = 1.0;
            FrequencySources = new string[] {
                "THOUSAND_GENOMES","TOPMED","UK10K","ESP_AFRICAN_AMERICAN",
                "ESP_EUROPEAN_AMERICAN","ESP_ALL","EXAC_AFRICAN_INC_AFRICAN_AMERICAN",
                "EXAC_AMERICAN","EXAC_SOUTH_ASIAN","EXAC_EAST_ASIAN","EXAC_FINNISH",
                "EXAC_NON_FINNISH_EUROPEAN","EXAC_OTHER","GNOMAD_E_AFR","GNOMAD_E_AMR",
                "GNOMAD_E_EAS","GNOMAD_E_FIN","GNOMAD_E_NFE","GNOMAD_E_OTH",
                "GNOMAD_E_SAS", "GNOMAD_G_AFR","GNOMAD_G_AMR","GNOMAD_G_EAS",
                "GNOMAD_G_FIN","GNOMAD_G_NFE","GNOMAD_G_OTH","GNOMAD_G_SAS"
            };
            KeepNonPathogenic = true;
            RegulatoryFeatureFilter = false;
            MinQuality = 50.0;
            OutputPassVariantsOnly = false;
            InheritanceModes = new Dictionary<string, double>{
                {"AUTOSOMAL_DOMINANT", 0.1},
                {"AUTOSOMAL_RECESSIVE_HOM_ALT", 0.1},
                {"AUTOSOMAL_RECESSIVE_COMP_HET", 2.0},
                {"X_DOMINANT", 0.1},
                {"X_RECESSIVE_HOM_ALT", 0.1},
                {"X_RECESSIVE_COMP_HET", 2.0},
                {"MITOCHONDRIAL", 0.2}
            };
            VariantEffectFilters = new Dictionary<string, IList<string>> { };
            PriorityScoreFilter = new Dictionary<string, object> { { "priorityType", "HIPHIVE_PRIORITY" }, { "minPriorityScore", 0.501 } };
            OutputFormats = new string[] { "JSON", "HTML" }; // "HTML, JSON, TSV_GENE, TSV_VARIANT, VCF"
        }
    }
}
