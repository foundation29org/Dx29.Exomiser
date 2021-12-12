using System;
using System.IO;
using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace Dx29.Exomiser
{
    public class ExomiserAnalysis
    {
        public Analysis analysis { get; set; }
        public OutputOptions outputOptions { get; set; }

        static public ExomiserAnalysis CreateDefault()
        {
            var deserializer = new Deserializer();
            string yaml = File.ReadAllText("ExomiserAnalysis.yaml");
            var instance = deserializer.Deserialize<ExomiserAnalysis>(yaml);
            instance.analysis.steps = new List<object>();
            return instance;
        }
    }

    public class Analysis
    {
        public string genomeAssembly { get; set; }

        public string vcf { get; set; }
        public string ped { get; set; }
        public string proband { get; set; }

        public IList<string> hpoIds { get; set; }

        public InheritanceModes inheritanceModes { get; set; }
        public string analysisMode { get; set; } // FULL or PASS_ONLY

        public IList<string> frequencySources { get; set; }
        public IList<string> pathogenicitySources { get; set; } // POLYPHEN, MUTATION_TASTER, SIFT, CADD, REMM

        public IList<object> steps { get; set; }
    }

    public class InheritanceModes
    {
        public decimal AUTOSOMAL_DOMINANT { get; set; }
        public decimal AUTOSOMAL_RECESSIVE_HOM_ALT { get; set; }
        public decimal AUTOSOMAL_RECESSIVE_COMP_HET { get; set; }
        public decimal X_DOMINANT { get; set; }
        public decimal X_RECESSIVE_HOM_ALT { get; set; }
        public decimal X_RECESSIVE_COMP_HET { get; set; }
        public decimal MITOCHONDRIAL { get; set; }
    }

    public class IntervalFilter
    {
        public string interval { get; set; }
        public IList<string> intervals { get; set; }
        public string Bed { get; set; }
    }

    public class GenePanelFilter
    {
        public IList<string> geneSymbols { get; set; }
    }

    public class FailedVariantFilter
    {
    }

    public class QualityFilter
    {
        public decimal minQuality { get; set; }
    }

    public class KnownVariantFilter
    {
    }

    public class VariantEffectFilter
    {
        public IList<string> remove { get; set; }
    }

    public class FrequencyFilter
    {
        public decimal maxFrequency { get; set; }
    }

    public class PathogenicityFilter
    {
        public bool keepNonPathogenic { get; set; }
    }

    public class InheritanceFilter
    {
    }

    public class OmimPrioritiser
    {
    }

    public class PriorityScoreFilter
    {
        public decimal minPriorityScore { get; set; }
    }

    public class HiPhivePrioritiser
    {
        public string runParams { get; set; }
    }

    public class PhivePrioritiser
    {
    }

    public class PhenixPrioritiser
    {
    }

    public class ExomeWalkerPrioritiser
    {
        public IList<int> seedGeneIds { get; set; }
    }

    public class RegulatoryFeatureFilter
    {
    }

    public class OutputOptions
    {
        public bool outputContributingVariantsOnly { get; set; }
        public int numGenes { get; set; }
        public string outputPrefix { get; set; }
        public IList<string> outputFormats { get; set; }
    }
}
